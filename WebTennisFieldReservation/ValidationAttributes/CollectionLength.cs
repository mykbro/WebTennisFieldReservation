using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace WebTennisFieldReservation.ValidationAttributes
{
    public class CollectionLength : ValidationAttribute
    {
        public override bool RequiresValidationContext => false;
        private int _minSize;
		private int _maxSize;

		public CollectionLength(int minSize, int maxSize = int.MaxValue):base("Number of entries must be between {0} and {1}")
        {
            _minSize = minSize;
            _maxSize = maxSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection c && _minSize <= c.Count && c.Count <= _maxSize)
            {               
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(string.Format(ErrorMessageString, _minSize, _maxSize));
            }
        }
    }
}
