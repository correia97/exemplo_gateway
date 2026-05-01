using Microsoft.AspNetCore.Http.HttpResults;
using System.Runtime.InteropServices;

namespace OpenCode.Music.Api.Endpoints;

public static class Version
{
    public static RouteGroupBuilder MapVersionEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/version", GetVersionAsync).AllowAnonymous();
        return group;
    }

    private static Ok<VersionResponse> GetVersionAsync()
    {
        return TypedResults.Ok(new VersionResponse(
            ApiName: "Music API",
            AssemblyVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0.0",
            RuntimeVersion: RuntimeInformation.FrameworkDescription,
            OsDescription: RuntimeInformation.OSDescription,
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            EnvironmentName: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Status: "Healthy"
        ));
    }
}

internal record VersionResponse(
    string ApiName,
    string AssemblyVersion,
    string RuntimeVersion,
    string OsDescription,
    string ProcessArchitecture,
    string EnvironmentName,
    string Status
);
