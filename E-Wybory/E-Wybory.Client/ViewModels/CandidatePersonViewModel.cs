using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class CandidatePersonViewModel: IValidatableObject
    {
        [Required]
        public CandidateViewModel candidateViewModel { get; set; } = new CandidateViewModel();

        [Required]
        public PersonViewModel personViewModel { get; set; } = new PersonViewModel();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(candidateViewModel, new ValidationContext(candidateViewModel), results, true);
            Validator.TryValidateObject(personViewModel, new ValidationContext(personViewModel), results, true);

            return results;
        }
    }
}
