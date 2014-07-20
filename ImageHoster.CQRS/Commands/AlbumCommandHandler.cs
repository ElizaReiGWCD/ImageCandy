using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class AlbumCommandHandler : Handles<CreateAlbum>, Handles<AddPhotoToAlbum>, Handles<EditAlbumInfo>, Handles<DeleteAlbum>, Handles<RemovePhotoFromAlbum>, Handles<EditPhotoInAlbum>
    {
        private IRepository<Album> repo;
        private IRepository<Photo> photos;

        public AlbumCommandHandler(IRepository<Album> repository, IRepository<Photo> photos)
        {
            repo = repository;
            this.photos = photos;
        }

        public void Handle(CreateAlbum message)
        {
            var item = new Album(message.Id, message.UserId, message.Title, message.Description, message.Privacy);
            repo.Save(item, -1);
            item.AddAlbumToGroup(message.Bus);
        }

        public void Handle(AddPhotoToAlbum message)
        {
            var item = repo.GetById(message.AlbumId);
            var photo = photos.GetById(message.PhotoId);
            item.AddPhoto(message.Id, message.UserId, photo, message.AlbumId);
            repo.Save(item, item.Version);
        }
        public void Handle(RemovePhotoFromAlbum message)
        {
            var item = repo.GetById(message.AlbumId);
            item.RemovePhoto(message.UserId, message.PhotoId, message.AlbumId);
            repo.Save(item, item.Version);
        }

        public void Handle(EditAlbumInfo message)
        {
            var item = repo.GetById(message.AlbumId);
            item.EditAlbum(message.Id, message.User, message.AlbumId, message.Title, message.Description, message.Privacy, message.OldPrivacy, message.Bus, message.PhotoPrivacies);
            repo.Save(item, item.Version);
        }

        public void Handle(DeleteAlbum message)
        {
            var item = repo.GetById(message.AlbumId);
            item.DeleteAlbum(message.Id, message.User, message.AlbumId, message.Bus);
            repo.Save(item, item.Version);
        }

        public void Handle(EditPhotoInAlbum message)
        {
            var item = repo.GetById(message.AlbumId);
            item.EditPhotoInAlbum(message.PhotoId, message.Privacy);
            repo.Save(item, item.Version);
        }
    }
}
