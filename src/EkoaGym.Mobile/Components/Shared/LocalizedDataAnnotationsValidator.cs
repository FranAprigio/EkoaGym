using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using EkoaGym.Mobile.Resources;

namespace EkoaGym.Mobile.Components.Shared;

// Drop-in replacement for DataAnnotationsValidator whose messages come from SharedResources, not the attribute's English text.
// Message key: the attribute's ErrorMessage when it is a resx key, otherwise "Validation.<AttributeName>".
public sealed partial class LocalizedDataAnnotationsValidator : ComponentBase, IDisposable
{
    [CascadingParameter] private EditContext? CurrentEditContext { get; set; }

    [Inject] private IStringLocalizer<SharedResources> L { get; set; } = default!;

    private ValidationMessageStore? messageStore;

    [GeneratedRegex(@"^[A-Za-z]+(\.[A-Za-z]+)+$")]
    private static partial Regex ResourceKeyPattern();

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(LocalizedDataAnnotationsValidator)} requires a cascading EditContext.");
        }

        messageStore = new ValidationMessageStore(CurrentEditContext);
        CurrentEditContext.OnValidationRequested += HandleValidationRequested;
        CurrentEditContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        var model = CurrentEditContext!.Model;
        messageStore!.Clear();
        foreach (var property in model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ValidateProperty(model, property);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        var field = e.FieldIdentifier;
        var property = field.Model.GetType().GetProperty(field.FieldName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            return;
        }

        messageStore!.Clear(field);
        ValidateProperty(field.Model, property);
        CurrentEditContext!.NotifyValidationStateChanged();
    }

    private void ValidateProperty(object model, PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes<ValidationAttribute>().ToList();
        if (attributes.Count == 0)
        {
            return;
        }

        var displayName = GetDisplayName(property);
        var context = new ValidationContext(model) { MemberName = property.Name, DisplayName = displayName };
        var value = property.GetValue(model);
        var field = new FieldIdentifier(model, property.Name);

        foreach (var attribute in attributes)
        {
            if (attribute.GetValidationResult(value, context) != ValidationResult.Success)
            {
                messageStore!.Add(field, FormatMessage(attribute, displayName, model));
            }
        }
    }

    private string GetDisplayName(PropertyInfo property)
    {
        var name = property.GetCustomAttribute<DisplayAttribute>()?.Name;
        return name is null ? property.Name : L[name].Value;
    }

    private string FormatMessage(ValidationAttribute attribute, string displayName, object model)
    {
        var key = attribute.ErrorMessage is { } message && ResourceKeyPattern().IsMatch(message)
            ? message
            : $"Validation.{attribute.GetType().Name.Replace("Attribute", string.Empty)}";

        object[] args = attribute switch
        {
            RangeAttribute range => [displayName, range.Minimum, range.Maximum],
            StringLengthAttribute length => [displayName, length.MinimumLength, length.MaximumLength],
            MinLengthAttribute minLength => [displayName, minLength.Length],
            MaxLengthAttribute maxLength => [displayName, maxLength.Length],
            CompareAttribute compare => [displayName, OtherDisplayName(model, compare.OtherProperty)],
            _ => [displayName],
        };

        return L[key, args].Value;
    }

    private string OtherDisplayName(object model, string otherProperty)
    {
        var property = model.GetType().GetProperty(otherProperty);
        return property is null ? otherProperty : GetDisplayName(property);
    }

    public void Dispose()
    {
        if (CurrentEditContext is not null)
        {
            CurrentEditContext.OnValidationRequested -= HandleValidationRequested;
            CurrentEditContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
