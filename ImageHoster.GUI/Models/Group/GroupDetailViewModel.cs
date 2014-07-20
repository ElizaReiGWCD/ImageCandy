using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Group
{
    public class GroupDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<UserDto> Users { get; set; }
        public IEnumerable<AlbumDto> Albums { get; set; }
        public IEnumerable<PhotoDto> Photos { get; set; }
        public IEnumerable<UserDto> Moderators { get; set; }
        public GroupPrivacySettings Privacy { get; set; }
        public AnnouncementDto LatestAnnouncement { get; set; }

        public GroupDetailViewModel()
        {
        }
    }
}