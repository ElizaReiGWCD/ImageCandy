using ImageHoster.CQRS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Group
{
    public class CreateGroupViewModel
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name="Level of privacy")]
        public GroupPrivacyLevel Level { get; set; }

        [Display(Name = "The members of this group can be seen by people from outside the group")]
        public bool MembersVisibleToOutsiders { get; set; }

        [Display(Name = "The photos of this group can be seen by users from outside the group")]
        public bool PhotosVisibleToOutsiders { get; set; }

        [Display(Name = "Display the group on the group browse pages")]
        public bool GroupVisibleToOutsiders { get; set; }

        [Display(Name = "Only add photos after approval from a moderator")]
        public bool AcceptPhotos { get; set; }
    }
}