using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Eventhandlers
{
    public class AlbumEventHandler : Handles<CreatedAlbum>, Handles<AddedPhotoToAlbum>, Handles<ChangedAlbumInfo>, Handles<DeletedAlbum>, Handles<RemovedPhotoFromAlbum>
    {
        public void Handle(CreatedAlbum message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            var album = new AlbumDto() { Id = message.Id, Description = message.Description, Title = message.Title, Owner = user, Privacy = message.Privacy.ToReadModel(InMemoryDatabase.groups) };
            InMemoryDatabase.albums.Add(album);
            user.Albums.Add(album);
        }

        public void Handle(AddedPhotoToAlbum message)
        {
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            album.Photos.Add(photo);

            if (album.FrontPhoto == null)
            {
                album.FrontPhoto = photo;
            }
        }

        public void Handle(RemovedPhotoFromAlbum message)
        {
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            album.Photos.Remove(photo);

            if (album.FrontPhoto == photo)
            {
                var pic = album.Photos.FirstOrDefault();
                album.FrontPhoto = pic;
            }
        }

        public void Handle(ChangedAlbumInfo message)
        {
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);

            album.Title = message.Title;
            album.Description = message.Description;
            album.Privacy = message.Privacy.ToReadModel(InMemoryDatabase.groups);
        }

        public void Handle(DeletedAlbum message)
        {
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            InMemoryDatabase.users.Find(u => u == album.Owner)
                .Albums.Remove(album);
            InMemoryDatabase.albums.Remove(album);
        }
    }
}
