using ImageHoster.CQRS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.User
{
    public class ManageViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,20}$", ErrorMessage = "Password should contain atleast one lowercase letter, one uppercase letter, and one digit")]
        [Display(Name="Old Password")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,20}$", ErrorMessage = "Password should contain atleast one lowercase letter, one uppercase letter, and one digit")]
        [Display(Name="New password")]
        public string NewPassword { get; set; }

        [Display(Name="First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EnumDataType(typeof(Sex))]
        public string Sex { get; set; }

        [Display(Name = "About yourself")]
        public string About { get; set; }
    }
}