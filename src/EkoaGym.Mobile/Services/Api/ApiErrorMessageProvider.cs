using System.Text.Json;
using EkoaGym.Mobile.Resources;
using Microsoft.Extensions.Localization;

namespace EkoaGym.Mobile.Services.Api;

public sealed class ApiErrorMessageProvider(IStringLocalizer<SharedResources> localizer) : IApiErrorMessageProvider
{
    public string GetMessage(ApiErrorInfo error)
    {
        if (error.Code == "validation_error" && error.FieldErrors is { Count: > 0 } fieldErrors)
        {
            var messages = fieldErrors.Select(FormatFieldError).Distinct();
            return string.Join(" ", messages);
        }

        if (error.Code is { } code && TryGet($"Error.{code}", out var text, ArgumentValues(error)))
        {
            return text;
        }

        var fallbackKey = error.StatusCode switch
        {
            401 => "Error.http_401",
            403 => "Error.http_403",
            404 => "Error.http_404",
            409 => "Error.http_409",
            429 => "Error.http_429",
            >= 500 => "Error.internal_error",
            _ => "Error.http_other",
        };

        return localizer[fallbackKey].Value;
    }

    private string FormatFieldError(ApiFieldError error)
    {
        var label = TryGet($"Field.{error.Field}", out var fieldName) ? fieldName : error.Field;
        return TryGet($"Error.field.{error.Code}", out var text, label)
            ? text
            : localizer["Error.field.invalid", label].Value;
    }

    // Numbers are passed as decimals so the active culture formats them (82,5 in pt-BR).
    private static object[] ArgumentValues(ApiErrorInfo error) =>
        error.Args?.Values.Select<JsonElement, object>(value => value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String => value.GetString() ?? string.Empty,
            _ => value.ToString(),
        }).ToArray() ?? [];

    private bool TryGet(string key, out string text, params object[] args)
    {
        var localized = localizer[key, args];
        text = localized.Value;
        return !localized.ResourceNotFound;
    }
}
