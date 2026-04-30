var builder = DistributedApplication.CreateBuilder(args);


var username = builder.AddParameter("postgresUser",  "postgres", secret: true);
var password = builder.AddParameter("postgresPass", "postgres", secret: true);
var keycloakAdminPassword = builder.AddParameter("keycloakAdminPassword", "admin", secret: true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithDataVolume()
    .WithPgWeb()
    .WithEnvironment("POSTGRES_DB", "opencode")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithBindMount("../../deploy/db", "/docker-entrypoint-initdb.d/")
    .WithHostPort(5432);

var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one", "1.76.0")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithEnvironment("COLLECTOR_OTLP_HTTP_ENABLED", "true")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint(port: 4317, targetPort: 4317, scheme: "http", name: "grpc")
    .WithEndpoint(port: 4318, targetPort: 4318, scheme: "http", name: "http")
    .WithEndpoint(port: 16686, targetPort: 16686, scheme: "http", name: "ui");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.6.1")
    .WithArgs("start-dev", 
    //"--import-realm", 
    "--verbose")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgres:5432/opencode")
    .WithEnvironment("KC_DB_USERNAME", "keycloak_user")
    .WithEnvironment("KC_DB_PASSWORD", "keycloak_pass")
    .WithEnvironment("KC_DB_SCHEMA", "keycloak")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_IMPORT_REALM", "opencode-realm.json")
    .WithEnvironment("KC_HOSTNAME_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithBindMount("../../deploy/keycloak/OpenCode-realm.json", "/opt/keycloak/data/import/OpenCode-realm.json", isReadOnly: false)
    .WithEndpoint(port: 8080, targetPort: 8080, scheme: "http", name: "http")
    .WithReference(postgres)
    .WaitFor(postgres);

var busybox = builder.AddContainer("busybox", "rootpublic/curl", "bookworm-slim_rootio")
    .WithBindMount("../../deploy/kong/init-routes.sh", "/init-routes.sh")
    .WithEntrypoint("/bin/sh")
    .WithArgs("/init-routes.sh")
    .WaitFor(keycloak);

var kongInit = builder.AddContainer("gateway-init", "kong/kong", "3.9.1-ubuntu")
    .WithEnvironment("KONG_PG_HOST", "postgres")
    .WithEnvironment("KONG_PG_PORT", "5432")
    .WithEnvironment("KONG_DATABASE", "postgres")
    .WithEnvironment("KONG_PG_DATABASE", "opencode")
    .WithEnvironment("KONG_PG_USER", "kong_user")
    .WithEnvironment("KONG_PG_PASSWORD", "kong_pass") 
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEntrypoint("kong")
    .WithArgs("migrations", "bootstrap", "--v")
    .WithReference(postgres)
    .WaitFor(postgres);

var kong = builder.AddContainer("gateway", "kong/kong", "3.9.1-ubuntu")
    .WithEnvironment("KONG_PG_HOST", "postgres")
    .WithEnvironment("KONG_PG_PORT", "5432")
    .WithEnvironment("KONG_DATABASE", "postgres")
    .WithEnvironment("KONG_PG_DATABASE", "opencode")
    .WithEnvironment("KONG_PG_USER", "kong_user")
    .WithEnvironment("KONG_PG_PASSWORD", "kong_pass")
    .WithEnvironment("KONG_PROXY_ACCESS_LOG", "/dev/stdout")
    .WithEnvironment("KONG_ADMIN_ACCESS_LOG", "/dev/stdout")
    .WithEnvironment("KONG_PROXY_ERROR_LOG", "/dev/stderr")
    .WithEnvironment("KONG_ADMIN_ERROR_LOG", "/dev/stderr")
    .WithEnvironment("KONG_ADMIN_LISTEN", "0.0.0.0:8001")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint(port: 8000, targetPort: 8000, name: "proxy", scheme: "http")
    .WithEndpoint(port: 8001, targetPort: 8001, name: "admin", scheme: "http")
    .WithEndpoint(port: 8002, targetPort: 8002, name: "gui", scheme: "http")
    .WithReference(postgres)
    .WaitFor(kongInit);

var dragonballApi = builder.AddProject<Projects.OpenCode_DragonBall_Api>("dragonball-api")
    .WithReference(postgres)
    .WithEnvironment("ConnectionStrings__dragonball",
        "Host=localhost;Port=5432;Database=opencode;Username=dragonball_user;Password=dragonball_pass;SearchPath=dragonball;")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://jaeger:4317")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint("http", e => e.Port = 5000)
    .WithEndpoint("https", e => e.Port = 5001)
    .WithReplicas(1)
    .WaitFor(postgres);

var musicApi = builder.AddProject<Projects.OpenCode_Music_Api>("music-api")
    .WithEnvironment("ConnectionStrings__music",
        "Host=localhost;Port=5432;Database=opencode;Username=music_user;Password=music_pass;SearchPath=music;")
    .WithReference(postgres)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://jaeger:4317")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint("http", e => e.Port = 5002)
    .WithEndpoint("https", e => e.Port = 5003)
    .WithReplicas(1)
    .WaitFor(postgres);

var frontend = builder.AddExecutable("frontend", "npm", "../OpenCode.Frontend", "install", "&&", "npm", "run", "dev")
    .WithReference(dragonballApi)
    .WithReference(musicApi)
    .WithEnvironment("VITE_DRAGONBALL_API_URL", "http://localhost:5000")
    .WithEnvironment("VITE_MUSIC_API_URL", "http://localhost:5002")
    .WithEnvironment("VITE_KEYCLOAK_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint(port: 5173, targetPort: 5173, scheme: "http", name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

var frontendAngular = builder.AddExecutable("angular-frontend", "npm", "../OpenCode.AngularFrontend", "install", "&&", "npm", "run", "start")
    .WithReference(dragonballApi)
    .WithReference(musicApi)
    .WithEnvironment("DRAGONBALL_API_URL", "http://localhost:5000")
    .WithEnvironment("MUSIC_API_URL", "http://localhost:5002")
    .WithEnvironment("KEYCLOAK_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint(port: 4200, targetPort: 4200, scheme: "http", name: "http", isProxied: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();
