using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class GroupPrivacySettings
    {
        public Guid Id { get; set; }
        public GroupPrivacyLevel Level { get; set; }
        public bool MembersVisibleToOutsiders { get; set; }
        public bool PhotosVisibleToOutsiders { get; set; }
        public bool GroupVisibleToOutsiders { get; set; }
        public bool AddPhotosAfterAccepting { get; set; }

        public GroupPrivacySettings()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
