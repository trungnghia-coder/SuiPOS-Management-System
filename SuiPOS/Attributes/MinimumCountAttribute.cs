using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Attributes
{
    /// <summary>
    /// Validation attribute ?? ki?m tra s? l??ng t?i thi?u trong collection
    /// </summary>
    public class MinimumCountAttribute : ValidationAttribute
    {
        private readonly int _minCount;

        public MinimumCountAttribute(int minCount)
        {
            _minCount = minCount;
            ErrorMessage = $"Ph?i có ít nh?t {_minCount} ph?n t?";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"Ph?i có ít nh?t {_minCount} ph?n t?");
            }

            if (value is ICollection collection)
            {
                if (collection.Count < _minCount)
                {
                    return new ValidationResult(ErrorMessage ?? $"Ph?i có ít nh?t {_minCount} ph?n t?");
                }
            }

            return ValidationResult.Success;
        }
    }
}
