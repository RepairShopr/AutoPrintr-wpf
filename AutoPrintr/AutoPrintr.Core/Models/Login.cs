using AutoPrintr.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace AutoPrintr.Core.Models
{
    public class Login : ValidatableBaseModel
    {
        [Required(ErrorMessage = ErrorMessagesContainer.RequiredFieldError)]
        [EmailAddress(ErrorMessage = ErrorMessagesContainer.EmailError)]
        public string Username { get; set; }

        [Required(ErrorMessage = ErrorMessagesContainer.RequiredFieldError)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = ErrorMessagesContainer.LengthError)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}