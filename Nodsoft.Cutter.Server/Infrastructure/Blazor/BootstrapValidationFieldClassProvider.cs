using Microsoft.AspNetCore.Components.Forms;

namespace Nodsoft.Cutter.Server.Infrastructure.Blazor;

/// <summary>
/// Provides CSS classes for validation fields in Bootstrap.
/// </summary>
public sealed class BootstrapValidationFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        bool isValid = editContext.IsValid(fieldIdentifier);
        bool isModified = editContext.IsModified(fieldIdentifier);

        // Blazor vs. Bootstrap:
        // isvalid = is-valid
        // isinvalid = is-invalid

        return $"{(isModified ? isValid ? "is-valid" : "is-invalid" : "")}";
    }
}