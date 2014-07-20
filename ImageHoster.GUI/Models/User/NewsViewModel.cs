using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.User
{
    public class NewsViewModel
    {
        public List<AnnouncementDto> Announcements { get; set; }
    }
}