using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Nodsoft.Cutter.Server.Components.Validation;

/// <summary>
/// Provides a way to display custom validation messages in a Blazor form.
/// </summary>
public class CustomValidation : ComponentBase
{
    private ValidationMessageStore? _messageStore;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CustomValidation)} requires a cascading parameter of type {nameof(EditContext)}.");
        }

        _messageStore = new(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += (_, _) => _messageStore?.Clear();
        CurrentEditContext.OnFieldChanged += (_, e) => _messageStore?.Clear(e.FieldIdentifier);
    }

    public void DisplayErrors(Dictionary<string, List<string>> errors)
    {
        if (CurrentEditContext is null)
        {
            return;
        }

        foreach ((string? key, List<string> value) in errors)
        {
            _messageStore?.Add(CurrentEditContext.Field(key), value);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    public void ClearErrors()
    {
        _messageStore?.Clear();
        CurrentEditContext?.NotifyValidationStateChanged();
    }
}