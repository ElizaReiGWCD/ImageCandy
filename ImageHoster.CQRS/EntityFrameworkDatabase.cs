using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS
{
    public class EntityFrameworkDatabase : DbContext
    {
        public EntityFrameworkDatabase(string connectionstring)
            : base(connectionstring)
        {

        }

        public DbSet<AlbumDto> Albums { get; set; }
        public DbSet<GroupDto> Groups { get; set; }
        public DbSet<UserDto> Users { get; set; }
        public DbSet<PhotoDto> Photos { get; set; }
    }
}
