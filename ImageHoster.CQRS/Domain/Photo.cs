using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Events;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Domain
{
    public class Photo : AggregateRoot
    {
        private Guid id;
        public Privacy Privacy { get; private set; }
        public Guid UploadedBy { get; private set; }
        private bool Deleted { get; set; }

        public override Guid Id
        {
            get { return id; }
            protected set { id = value; }
        }

        public Photo()
        {

        }

        private void Apply(PhotoUploaded e)
        {
            this.id = e.Id;
            this.Privacy = e.Privacy;
            this.UploadedBy = e.UserId;
            this.Deleted = false;
        }

        private void Apply(ChangedPhotoInfo e)
        {
            this.Privacy = e.Privacy;
        }

        private void Apply(DeletedPhoto e)
        {
            this.Deleted = true;
        }

        public Photo(Guid id, Guid user, string filename, Privacy privacy)
        {
            this.id = id;
            this.Privacy = privacy;
            this.UploadedBy = user;
            this.Deleted = false;

            ApplyChange(new PhotoUploaded(id, user, filename, privacy));
        }

        public void AddPhotoToGroup(ICommandSender bus)
        {
            if (Privacy.Level == PrivacyLevel.Group)
                foreach (var g in Privacy.VisibleToGroups)
                    bus.Send(new AddPhotoToGroup() { GroupId = g, PhotoId = this.Id });
        }

        public void UpdatePhotoInfo(Guid id, Guid user, Guid photoid, string title, string description, Privacy privacy, Privacy oldPrivacy, ICommandSender bus, IEnumerable<Guid> albums)
        {
            if (this.UploadedBy == user)
            {
                this.Privacy = privacy;

                var groups = privacy.VisibleToGroups.Union(oldPrivacy.VisibleToGroups);

                foreach (var g in groups)
                    bus.Send(new EditPhotoInGroup() { PhotoId = this.Id, GroupId = g, Privacy = privacy });

                foreach(var a in albums)
                    bus.Send(new EditPhotoInAlbum() { PhotoId = this.Id, AlbumId = a, Privacy = privacy });

                ApplyChange(new ChangedPhotoInfo() { Id = id, Title = title, Description = description, PhotoId = photoid, Version = Version, Privacy = privacy });
            }
        }

        public void DeletePhoto(Guid id, Guid user, Guid photoid, List<Guid> albums, ICommandSender bus)
        {
            if (user == this.UploadedBy)
            {
                if (Privacy.Level == PrivacyLevel.Group)
                    foreach (var g in Privacy.VisibleToGroups)
                        bus.Send(new DeletePhotoFromGroup() { GroupId = g, PhotoId = this.Id });

                foreach (var a in albums)
                    bus.Send(new RemovePhotoFromAlbum() { AlbumId = a, PhotoId = this.Id, UserId = user });

                this.Deleted = true;
                ApplyChange(new DeletedPhoto() { Id = id, PhotoId = photoid });
            }
        }
    }
}
