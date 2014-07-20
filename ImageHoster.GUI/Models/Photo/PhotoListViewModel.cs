using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Photo
{
    public class PhotoListViewModel
    {
        public List<PhotoDto> Photos { get; set; }
        public string Username { get; set; }
    }
}