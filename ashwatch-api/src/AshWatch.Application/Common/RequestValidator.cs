using System.ComponentModel.DataAnnotations;

namespace AshWatch.Application.Common;

public static class RequestValidator
{
    public static List<string> Validate(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        return results
            .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid value." : x.ErrorMessage!)
            .Distinct()
            .ToList();
    }
}
