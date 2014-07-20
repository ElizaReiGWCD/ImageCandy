using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.EFEventhandlers
{
    public class AlbumEventHandler : Handles<CreatedAlbum>, Handles<AddedPhotoToAlbum>, Handles<ChangedAlbumInfo>, Handles<DeletedAlbum>, Handles<RemovedPhotoFromAlbum>
    {
        private EntityFrameworkDatabase context;

        public AlbumEventHandler(string connectionstring)
        {
            context = new EntityFrameworkDatabase(connectionstring);
        }

        public void Handle(CreatedAlbum message)
        {
            var user = context.Users.Find(message.UserId);
            var album = new AlbumDto() { Id = message.Id, Description = message.Description, Title = message.Title, Owner = user, Privacy = message.Privacy.ToReadModel(context.Groups) };
            user.Albums.Add(album);

            context.SaveChanges();
        }

        public void Handle(AddedPhotoToAlbum message)
        {
            var album = context.Albums.Find(message.AlbumId);
            var photo = context.Photos.Find(message.PhotoId);

            album.Photos.Add(photo);

            if (album.FrontPhoto == null)
            {
                album.FrontPhoto = photo;
            }

            context.SaveChanges();
        }

        public void Handle(RemovedPhotoFromAlbum message)
        {
            var album = context.Albums.Find(message.AlbumId);
            var photo = context.Photos.Find(message.PhotoId);

            album.Photos.Remove(photo);

            if (album.FrontPhoto == photo)
            {
                var pic = album.Photos.FirstOrDefault();
                album.FrontPhoto = pic;
            }

            context.SaveChanges();
        }

        public void Handle(ChangedAlbumInfo message)
        {
            var album = context.Albums.Find(message.AlbumId);

            album.Title = message.Title;
            album.Description = message.Description;
            album.Privacy = message.Privacy.ToReadModel(context.Groups);

            context.SaveChanges();
        }

        public void Handle(DeletedAlbum message)
        {
            var album = context.Albums.Find(message.AlbumId);

            context.Users.Find(album.Owner).Albums.Remove(album);
            context.Albums.Remove(album);

            context.SaveChanges();
        }
    }
}
