using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Domain
{
    public class Privacy
    {
        public PrivacyLevel Level { get; set; }
        public Guid OwnerId { get; set; }
        public bool OnlyForLoggedInUsers { get; set; }
        public bool Published { get; set; }
        public IEnumerable<Guid> VisibleToGroups { get; set; }

        public PrivacySettings ToReadModel(IEnumerable<GroupDto> groups)
        {
            return PrivacySettings.Create(Level, OnlyForLoggedInUsers, Published, OwnerId, VisibleToGroups == null ? null : VisibleToGroups.Select(g => groups.FirstOrDefault(gr => gr.Id == g)).ToList());
        }
    }

    public class GroupPrivacy
    {
        public GroupPrivacyLevel Level { get; set; }
        public bool MembersVisibleToOutsiders { get; set; }
        public bool PhotosVisibleToOutsiders { get; set; }
        public bool GroupVisibleToOutsiders { get; set; }
        public bool PhotosAddedAfterAccepting { get; set; }
    }
}
