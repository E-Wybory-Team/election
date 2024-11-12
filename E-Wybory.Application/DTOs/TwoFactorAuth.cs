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
