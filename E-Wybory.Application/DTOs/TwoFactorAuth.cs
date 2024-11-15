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
        [Required]
        [StringLength(7, ErrorMessage = "Kod {0} musi mieć długość pomiędzy {2} a {1} znaki.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Kod weryfikacyjny")]
        public string Code { get; set; } = string.Empty;
    }

    public class TwoFactorAuthResponse
    {
        [Required]
        public string QRCodeUri { get; } = string.Empty;
        [Required]
        public string Secret { get; } = string.Empty;

    }
}
