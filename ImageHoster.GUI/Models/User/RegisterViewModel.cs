using ImageHoster.CQRS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.User
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,20}$", ErrorMessage="Password should contain atleast one lowercase letter, one uppercase letter, and one digit, and should be between 8 and 20 characters")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "First name")]
        [StringLength(50, MinimumLength=2)]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [EnumDataType(typeof(Sex))]
        public string Sex { get; set; }

        [Display(Name="About yourself")]
        public string About { get; set; }

    }
}