using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Events;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Domain
{
    public class Album : AggregateRoot
    {
        private Guid id;
        private Privacy privacy { get; set; }
        private Guid madeBy { get; set; }
        private List<Guid> photos { get; set; }

        public override Guid Id
        {
            get { return id; }
            protected set { id = value; }
        }

        public Album()
        {
            photos = new List<Guid>();
        }

        private void Apply(CreatedAlbum e)
        {
            this.id = e.Id;
            this.privacy = e.Privacy;
            this.madeBy = e.UserId;
        }

        private void Apply(ChangedAlbumInfo e)
        {
            this.privacy = e.Privacy;
        }

        private void Apply(AddedPhotoToAlbum e)
        {
            this.photos.Add(e.PhotoId);
        }

        private void Apply(RemovedPhotoFromAlbum e)
        {
            this.photos.RemoveAll(g => g == e.PhotoId);
        }

        public Album(Guid id, Guid user, string title, string description, Privacy privacy)
        {
            this.id = id;
            this.madeBy = user;
            this.privacy = privacy;
            photos = new List<Guid>();
            ApplyChange(new CreatedAlbum() { Id = id, Description = description, Title = title, UserId = user, Privacy = privacy });
        }

        public void AddAlbumToGroup(ICommandSender bus)
        {
            if (privacy.Level == PrivacyLevel.Group)
                foreach (var g in privacy.VisibleToGroups)
                    bus.Send(new AddAlbumToGroup() { GroupId = g, AlbumId = this.Id });
        }

        public void AddPhoto(Guid id, Guid user, Photo photo, Guid albumid)
        {
            if (user == madeBy && !photos.Contains(photo.Id) && photo.UploadedBy == user)
            {
                bool canAdd = IsCompatible(photo.Privacy);

                if (canAdd)
                {
                    photos.Add(photo.Id);
                    ApplyChange(new AddedPhotoToAlbum() { Id = id, PhotoId = photo.Id, AlbumId = albumid, Version = Version });
                }
            }
        }

        public void EditPhotoInAlbum(Guid photoid, Privacy Privacy)
        {
            if (!IsCompatible(Privacy))
            {
                RemovePhoto(this.madeBy, photoid, this.Id);
            }
        }

        public void RemovePhoto(Guid user, Guid photoid, Guid albumid)
        {
            if (user == madeBy && photos.Contains(photoid))
            {
                photos.RemoveAll(g => g == photoid);
                ApplyChange(new RemovedPhotoFromAlbum() { PhotoId = photoid, AlbumId = albumid, Version = Version });
            }
        }

        public void EditAlbum(Guid id, Guid user, Guid albumId, string title, string description, Privacy privacy, Privacy oldPrivacy, ICommandSender bus, Dictionary<Guid, Privacy> photoPrivacy)
        {
            if (user == madeBy)
            {
                this.privacy = privacy;

                var groups = privacy.VisibleToGroups.Union(oldPrivacy.VisibleToGroups);

                foreach (var g in groups)
                    bus.Send(new EditAlbumInGroup() { AlbumId = this.Id, GroupId = g, Privacy = privacy });

                foreach (var t in photoPrivacy)
                    if (!IsCompatible(t.Value))
                        RemovePhoto(user, t.Key, this.Id);

                ApplyChange(new ChangedAlbumInfo() { Id = id, AlbumId = albumId, Title = title, Description = description, Version = Version, Privacy = privacy });
            }
        }

        public void DeleteAlbum(Guid id, Guid user, Guid albumId, ICommandSender bus)
        {
            if (user == madeBy)
            {
                if (this.privacy.Level == PrivacyLevel.Group)
                {
                    foreach (var group in privacy.VisibleToGroups)
                        bus.Send(new DeleteAlbumFromGroup() { AlbumId = this.Id, GroupId = group });
                }

                ApplyChange(new DeletedAlbum() { Id = id, AlbumId = albumId });
            }
        }

        private bool IsCompatible(Privacy photoPrivacy)
        {
            bool canAdd = false;

            if (this.privacy.Level == PrivacyLevel.Private)
                canAdd = photoPrivacy.OnlyForLoggedInUsers ? this.privacy.OnlyForLoggedInUsers : true;
            else if (this.privacy.Level == PrivacyLevel.Group)
                if (photoPrivacy.Level == PrivacyLevel.Public || photoPrivacy.Level == PrivacyLevel.Hidden || this.privacy.VisibleToGroups.IsSubsetOf(photoPrivacy.VisibleToGroups))
                    canAdd = photoPrivacy.OnlyForLoggedInUsers ? this.privacy.OnlyForLoggedInUsers : true;
                else { }
            else if (this.privacy.Level == PrivacyLevel.Hidden || this.privacy.Level == PrivacyLevel.Public)
                if (photoPrivacy.Level == PrivacyLevel.Public || photoPrivacy.Level == PrivacyLevel.Hidden)
                    canAdd = photoPrivacy.OnlyForLoggedInUsers ? this.privacy.OnlyForLoggedInUsers : true;
            return canAdd;
        }
    }
}
