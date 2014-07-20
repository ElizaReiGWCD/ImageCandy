using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public abstract class PrivacySettings
    {
        public Guid Id { get; private set; }
        public PrivacyLevel Level { get; private set; }
        public bool OnlyLoggedIn { get; private set; }
        public bool Publicized { get; private set; }

        public PrivacySettings(PrivacyLevel level, bool onlyLoggedIn, bool publicize)
        {
            Level = level;
            OnlyLoggedIn = onlyLoggedIn;
            Publicized = publicize;
            this.Id = Guid.NewGuid();
        }

        public PrivacySettings()
        {

        }

        /// <summary>
        /// Check if the user can see the object
        /// </summary>
        /// <param name="userId">User that views the object, if the user isnt logged in, pass Guid.Empty</param>
        /// <returns></returns>
        public abstract bool CanSee(Guid userId);

        protected bool IsLoggedIn(Guid userId)
        {
            return userId != Guid.Empty;
        }

        public static PrivacySettings Create(PrivacyLevel level, bool onlyLoggedIn, bool publicize, Guid owner, ICollection<GroupDto> groups = null)
        {
            switch (level)
            {
                case PrivacyLevel.Public:
                    return new PublicPrivacy(onlyLoggedIn, publicize);
                case PrivacyLevel.Hidden:
                    return new HiddenPrivacy(onlyLoggedIn, publicize);
                case PrivacyLevel.Group:
                    return new GroupPrivacy(onlyLoggedIn, publicize, groups);
                case PrivacyLevel.Private:
                    return new PrivatePrivacy(onlyLoggedIn, publicize, owner);
                default:
                    return new PublicPrivacy(onlyLoggedIn, publicize);
            }
        }
    }

    public class PublicPrivacy : PrivacySettings
    {
        public PublicPrivacy(bool onlyLoggedIn, bool publicize)
            : base(PrivacyLevel.Public, onlyLoggedIn, publicize)
        { }

        public PublicPrivacy()
        { }

        public override bool CanSee(Guid userId)
        {
            if (!OnlyLoggedIn)
                return true;
            else
                return IsLoggedIn(userId);
        }
    }

    public class HiddenPrivacy : PrivacySettings
    {
        public HiddenPrivacy(bool onlyLoggedIn, bool publicize)
            : base(PrivacyLevel.Hidden, onlyLoggedIn, publicize)
        { }

        public HiddenPrivacy()
        { }

        public override bool CanSee(Guid userId)
        {
            if (!OnlyLoggedIn)
                return true;
            else
                return IsLoggedIn(userId);
        }
    }

    public class GroupPrivacy : PrivacySettings
    {
        public virtual ICollection<GroupDto> GroupsEligible { get; private set; }

        public GroupPrivacy(bool onlyLoggedIn, bool publicize, ICollection<GroupDto> groups)
            : base(PrivacyLevel.Group, onlyLoggedIn, publicize)
        {
            GroupsEligible = groups;
        }

        public GroupPrivacy()
        { }

        public override bool CanSee(Guid userId)
        {
            if (!IsLoggedIn(userId))
                return false;

            return GroupsEligible.Where(g => g != null).SelectMany(g => g.Members).FirstOrDefault(u => u.Id == userId) != null;
        }
    }

    public class PrivatePrivacy : PrivacySettings
    {
        private Guid Owner { get; set; }

        public PrivatePrivacy(bool onlyLoggedIn, bool publicize, Guid owner)
            : base(PrivacyLevel.Private, onlyLoggedIn, publicize)
        {
            this.Owner = owner;
        }

        public PrivatePrivacy()
        { }

        public override bool CanSee(Guid userId)
        {
            return Owner == userId;
        }
    }
}
