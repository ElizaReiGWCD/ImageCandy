using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Photo
{
    public class PhotoViewModel
    {
        public string Username { get; set; }
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
}