var builder = DistributedApplication.CreateBuilder(args);


var username = builder.AddParameter("postgresUser",  "postgres", secret: true);
var password = builder.AddParameter("postgresPass", "postgres", secret: true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithDataVolume()
    .WithPgWeb()
    .WithEnvironment("POSTGRES_DB", "opencode")
    .WithBindMount("../OpenCode.Domain/Data", "/docker-entrypoint-initdb.d/")
    .WithHostPort(5432);

var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one:latest")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithEnvironment("COLLECTOR_OTLP_HTTP_ENABLED", "true")
    .WithEndpoint(port: 4317, targetPort: 4317, scheme: "http", name: "grpc")
    .WithEndpoint(port: 4318, targetPort: 4318, scheme: "http", name: "http")
    .WithEndpoint(port: 16686, targetPort: 16686, scheme: "http", name: "ui");

var dragonballApi = builder.AddProject<Projects.OpenCode_DragonBall_Api>("dragonball-api")
    .WithReference(postgres)
    .WithEnvironment("ConnectionStrings__dragonball",
        "Host=localhost;Port=5432;Database=opencode;Username=dragonball_user;Password=dragonball_pass;SearchPath=dragonball;")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://jaeger:4317")
    .WithEndpoint("http", e => e.Port = 5000)
    .WithEndpoint("https", e => e.Port = 5001)
    .WithReplicas(1)
    .WaitFor(postgres);

var musicApi = builder.AddProject<Projects.OpenCode_Music_Api>("music-api")
    .WithReference(postgres)
    .WithEnvironment("ConnectionStrings__music",
        "Host=localhost;Port=5432;Database=opencode;Username=music_user;Password=music_pass;SearchPath=music;")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://jaeger:4317")
    .WithEndpoint("http", e => e.Port = 5002)
    .WithEndpoint("https", e => e.Port = 5003)
    .WithReplicas(1)
    .WaitFor(postgres);

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:latest")
    .WithArgs("start-dev")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgres:5432/opencode")
    .WithEnvironment("KC_DB_USERNAME", "keycloak_user")
    .WithEnvironment("KC_DB_PASSWORD", "keycloak_pass")
    .WithEnvironment("KC_DB_SCHEMA", "keycloak")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_IMPORT_REALM", "opencode-realm.json")
    .WithEnvironment("KC_HOSTNAME_URL", "http://localhost:8080")
    .WithBindMount("../../.planning/phases/04-keycloak-authentication-authorization", "/opt/keycloak/data/import")
    .WithEndpoint(port: 8080, targetPort: 8080, scheme: "http", name: "http")
    .WithReference(postgres)
    .WaitFor(postgres);

var etcd = builder.AddContainer("etcd", "dhi.io/etcd", "3.6")
    .WithVolume("etcd_data", "/etcd-data")
    .WithArgs(
        "/usr/local/bin/etcd",
        "--name", "node1",
        "--listen-client-urls", "http://0.0.0.0:2379",
        "--advertise-client-urls", "http://localhost:2379",
        "--listen-peer-urls", "http://0.0.0.0:2380",
        "--initial-advertise-peer-urls", "http://localhost:2380")
    .WithHttpEndpoint(port: 2379, targetPort: 2379, name: "client")
    .WithEndpoint(port: 2380, targetPort: 2380, name: "peer", scheme: "tcp")
    .WithEnvironment("ALLOW_NONE_AUTHENTICATION", "yes");

var apisixConfigPath = Path.Combine(
    Directory.GetParent(typeof(Program).Assembly.Location)!.Parent!.Parent!.Parent!.Parent!.FullName,
    "..", ".planning", "phases", "05-apisix-gateway");

var apisix = builder.AddContainer("gateway", "apache/apisix", "3.16.0-ubuntu")
    .WithEnvironment("UPSTREAM_DRAGONBALL_API", "http://host.docker.internal:5000")
    .WithEnvironment("UPSTREAM_MUSIC_API", "http://host.docker.internal:5002")
    .WithEnvironment("KEYCLOAK_URL", "http://host.docker.internal:8080")
    .WithEnvironment("CLIENT_SECRET_DRAGONBALL", "dragonball-secret")
    .WithEnvironment("CLIENT_SECRET_MUSIC", "music-secret")
    .WithEnvironment("ADMIN_KEY", "edd1c9f034335f136f87ad84b625c8f1")
    .WithEnvironment("JAEGER_ENDPOINT", "http://jaeger:4318")
    .WithBindMount("../../.planning/phases/05-apisix-gateway/", "/scripts", isReadOnly:false)
    .WithBindMount("../../.planning/phases/05-apisix-gateway/config.yaml", "/usr/local/apisix/conf/config.yaml", isReadOnly: false)
    .WithEndpoint(port: 9180, targetPort: 9180, name: "admin", scheme: "http")
    .WithEndpoint(port: 9080, targetPort: 9080, name: "http", scheme: "http")
    .WithEndpoint(port: 9091, targetPort: 9091, name: "metrics", scheme: "http")
    .WithEndpoint(port: 9443, targetPort: 9443, name: "https", scheme: "https")
    .WithEndpoint(port: 9092, targetPort: 9092, name: "stream", scheme: "tcp")
    .WithEndpoint(port: 9000, targetPort: 9000, name: "extra", scheme: "http")
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", "/scripts/init-routes.sh && /scripts/init-routes-otel.sh && exec /usr/local/openresty/bin/openresty -p /usr/local/apisix -g 'daemon off;'")
    .WaitFor(etcd);

var frontend = builder.AddExecutable("frontend", "npm", "../OpenCode.Frontend", "run", "dev")
    .WithEnvironment("VITE_DRAGONBALL_API_URL", "http://localhost:5000")
    .WithEnvironment("VITE_MUSIC_API_URL", "http://localhost:5002")
    .WithEnvironment("VITE_KEYCLOAK_URL", "http://localhost:8080")
    .WithEndpoint(port: 5173, targetPort: 5173, scheme: "http", name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

var angularFrontend = builder.AddExecutable("angular-frontend", "npm", "../OpenCode.AngularFrontend", "run", "start")
    .WithEnvironment("DRAGONBALL_API_URL", "http://localhost:5000")
    .WithEnvironment("MUSIC_API_URL", "http://localhost:5002")
    .WithEnvironment("KEYCLOAK_URL", "http://localhost:8080")
    .WithEndpoint(port: 4200, targetPort: 4200, scheme: "http", name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
