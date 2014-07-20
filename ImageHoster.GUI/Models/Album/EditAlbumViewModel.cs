using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Album
{
    public class EditAlbumViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength=3)]
        public string Title { get; set; }
        public string Description { get; set; }

        public PrivacyViewModel Privacy { get; set; }
    }
}