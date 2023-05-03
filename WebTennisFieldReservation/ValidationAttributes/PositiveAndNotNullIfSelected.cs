using System.ComponentModel.DataAnnotations;
using WebTennisFieldReservation.Models.Administration;

namespace WebTennisFieldReservation.ValidationAttributes
{
    public class PositiveAndNotNullIfSelected : ValidationAttribute
    {  
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(validationContext.ObjectInstance is TemplateEntryModel thisTempEntry)
            {
                if(!thisTempEntry.IsSelected || thisTempEntry.IsSelected && value is decimal price && price >= 0)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    
                    return new ValidationResult(ErrorMessage);
                }
            }
            else
            {
                throw new ValidationException(nameof(PositiveAndNotNullIfSelected));
            }           
           
        }
    }
}
