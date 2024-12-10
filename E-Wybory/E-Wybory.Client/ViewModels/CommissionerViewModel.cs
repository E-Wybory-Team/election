using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class CommissionerViewModel: IValidatableObject
    {
        [Required]
        public UserTypeViewModel userType { get; set; } = new UserTypeViewModel();

        [Required]
        public ElectionUserViewModel userViewModel { get; set; } = new ElectionUserViewModel();

        [Required]
        public PersonViewModel personViewModel { get; set; } = new PersonViewModel();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(userType, new ValidationContext(userType), results, true);
            Validator.TryValidateObject(userViewModel, new ValidationContext(userViewModel), results, true);
            Validator.TryValidateObject(personViewModel, new ValidationContext(personViewModel), results, true);

            return results;
        }
    }
}
