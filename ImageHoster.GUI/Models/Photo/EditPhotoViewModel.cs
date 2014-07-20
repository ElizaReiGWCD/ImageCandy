using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Photo
{
    public class EditPhotoViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PrivacyViewModel Privacy { get; set; }
    }
}