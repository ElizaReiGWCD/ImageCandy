using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Group
{
    public class MakeAnnouncementViewModel
    {
        public Guid GroupId { get; set; }
        public string Title { get; set; }
        public string Announcement { get; set; }
    }
}