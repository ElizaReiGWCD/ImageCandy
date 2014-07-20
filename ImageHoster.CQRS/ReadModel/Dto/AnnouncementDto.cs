using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public virtual UserDto Announcer { get; set; }
        public virtual GroupDto Group { get; set; }
        public string Title { get; set; }
        public string Announcement { get; set; }
        public DateTime Time { get; set; }

        public AnnouncementDto()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
