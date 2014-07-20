using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class GroupDto : IEquatable<GroupDto>
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual UserDto Admin { get; set; }
        public virtual ICollection<UserDto> Moderators { get; set; }
        public virtual ICollection<UserDto> Members { get; set; }

        public virtual GroupPrivacySettings Privacy { get; set; }

        public virtual ICollection<UserDto> JoinRequests { get; set; }

        public virtual ICollection<PhotoDto> Photos { get; set; }
        public virtual ICollection<AlbumDto> Albums { get; set; }

        public virtual ICollection<AnnouncementDto> Announcements { get; set; }

        public virtual ICollection<PhotoDto> PendingPhotos { get; set; }
        public virtual ICollection<AlbumDto> PendingAlbums { get; set; }

        public GroupDto()
        {
            Moderators = new List<UserDto>();
            Members = new List<UserDto>();
            JoinRequests = new List<UserDto>();
            Announcements = new List<AnnouncementDto>();

            Photos = new List<PhotoDto>();
            PendingPhotos = new List<PhotoDto>();

            Albums = new List<AlbumDto>();
            PendingAlbums = new List<AlbumDto>();
        }

        public bool Equals(GroupDto other)
        {
            return this.Id == other.Id;
        }
    }
}
