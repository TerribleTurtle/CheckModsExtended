using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CheckModsExtended.Services.Web;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/settings", GetSettingsAsync);
        api.MapPost("/settings", PostSettingsAsync);
    }

    private static async Task<IResult> GetSettingsAsync(ISettingsService settingsService, CancellationToken token)
    {
        try
        {
            var settings = await settingsService.GetSettingsAsync(token);
            return Results.Content(settings, "application/json");
        }
        catch (Exception ex)
        {
            return Results.Json(new ErrorResponse(ex.Message), statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> PostSettingsAsync(HttpRequest request, ISettingsService settingsService, CancellationToken token)
    {
        try
        {
            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync(token);
            
            var result = await settingsService.UpdateSettingsAsync(content, token);

            return result.Match(
                success => Results.Ok(success),
                apiError => Results.BadRequest(new ErrorResponse(apiError.Message))
            );
        }
        catch (Exception ex)
        {
            return Results.Json(new ErrorResponse(ex.Message), statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
