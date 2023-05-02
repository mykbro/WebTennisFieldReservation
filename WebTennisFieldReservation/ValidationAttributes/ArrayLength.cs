using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.ValidationAttributes
{
    public class ArrayLength : ValidationAttribute
    {
        public override bool RequiresValidationContext => false;
        private int _size;

        public ArrayLength(int size):base("Number of entries must be {0}")
        {
            _size = size;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Array a && a.Length == _size)
            {               
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(string.Format(ErrorMessageString, _size));
            }
        }
    }
}
