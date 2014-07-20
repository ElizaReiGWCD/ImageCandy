using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel
{
    public class EFReadModel : IReadModel
    {
        EntityFrameworkDatabase context;

        public EFReadModel(string connectionstring)
        {
            context = new EntityFrameworkDatabase(connectionstring);
        }

        public IEnumerable<UserDto> GetUsers()
        {
            return context.Users.ToList();
        }

        public IEnumerable<PhotoDto> GetPhotos()
        {
            return context.Photos.ToList();
        }

        public IEnumerable<AlbumDto> GetAlbums()
        {
            return context.Albums.ToList();
        }

        public IEnumerable<GroupDto> GetGroups()
        {
            return context.Groups.ToList();
        }
    }
}
