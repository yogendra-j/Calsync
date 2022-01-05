using System.ComponentModel.DataAnnotations;

namespace CalendarSyncPOC.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        public string TwoFactorVerificationCode { get; set; }

        public bool RememberClient { get; set; }

        public string TwoFactorRememberClientToken { get; set; }

        public bool? SingleSignIn { get; set; }

        public string ReturnUrl { get; set; }

        public string CaptchaResponse { get; set; }

        public string Tenancyname { get; set; }
    }
}