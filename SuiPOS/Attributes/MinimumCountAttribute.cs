using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Attributes
{
    public class MinimumCountAttribute : ValidationAttribute
    {
        private readonly int _minCount;

        public MinimumCountAttribute(int minCount)
        {
            _minCount = minCount;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Ép kiểu sang ICollection để đếm phần tử
            var collection = value as ICollection;

            if (collection == null || collection.Count < _minCount)
            {
                return new ValidationResult(ErrorMessage ?? $"Phải có ít nhất {_minCount} phần tử.");
            }

            return ValidationResult.Success;
        }
    }
}