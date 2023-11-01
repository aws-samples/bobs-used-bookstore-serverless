using FluentValidation.Results;

namespace BookInventory.Api.Extensions
{
    public static class ValidationExtensions
    {
        public static string GetErrorMessage(this ValidationResult validationResult)
        {
            return validationResult != null
                ? string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage))
                : string.Empty;
        }
    }
}