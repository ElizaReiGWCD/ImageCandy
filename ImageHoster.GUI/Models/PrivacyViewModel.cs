using ImageHoster.CQRS;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Models
{
    public class PrivacyViewModel
    {
        [Display(Name = "Level of privacy")]
        public PrivacyLevel Level { get; set; }

        public IEnumerable<SelectListItem> GroupNames { get; set; }

        [Display(Name = "Visible to groups:")]
        public List<Guid> SelectedGroups { get; set; }

        [Display(Name = "Display this only for logged in users")]
        public bool OnlyLoggedInUsers { get; set; }

        [Display(Name = "Display this on the front page")]
        public bool Publish { get; set; }
    }
}