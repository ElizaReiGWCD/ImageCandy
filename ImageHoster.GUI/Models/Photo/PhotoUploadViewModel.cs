using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Models.Photo
{
    public class PhotoUploadViewModel
    {
        public HttpPostedFileBase ImageData { get; set; }

        [Display(Name="Add to album: ")]
        public Guid AlbumId { get; set; }

        public List<SelectListItem> AlbumSelection { get; set; }

        public PrivacyViewModel Privacy { get; set; }
    }
}