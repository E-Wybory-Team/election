using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Wybory.Application.DTOs
{
    public class TwoFactorAuthVerifyRequest
    {
        [Required(ErrorMessage = "Podaj kod weryfikacyjny")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod weryfikacyjny musi składać się z 6 cyfr.")]
        [DataType(DataType.Text)]
        [Display(Name = "Kod weryfikacyjny")]
        public string Code { get; set; } = string.Empty;

        public int UserId { get; set; } = 0;
    }

    public class TwoFactorAuthResponse
    {
        [Required]
        public string Secret { get; } = string.Empty;

    }

    public class TwoFactorEnabledRequest
    {
        [Required]
        public int UserId { get; set; } = 0;

        [Required] 
        public bool IsEnabled { get; set; } = false;
    }
}
