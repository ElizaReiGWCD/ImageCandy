using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class UserDto : IEquatable<UserDto>
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string About { get; set; }

        public virtual ICollection<PhotoDto> Photos { get; set; }
        public virtual ICollection<AlbumDto> Albums { get; set; }
        public virtual ICollection<GroupDto> Groups { get; set; }
        public virtual ICollection<GroupDto> GroupInvites { get; set; }

        public int NewsCount { get; set; }

        public UserDto()
        {
            Photos = new List<PhotoDto>();
            Albums = new List<AlbumDto>();
            Groups = new List<GroupDto>();
            GroupInvites = new List<GroupDto>();
        }

        public UserDto(Guid id, string username, string password, string salt, string email, string firstname, string lastname, string sex, string about)
        {
            Username = username;
            Password = password;
            Salt = salt;
            Id = id;
            Email = email;
            FirstName = firstname;
            LastName = lastname;
            Sex = sex;
            About = about;

            Photos = new List<PhotoDto>();
            Albums = new List<AlbumDto>();
            Groups = new List<GroupDto>();
            GroupInvites = new List<GroupDto>();
        }

        public bool Equals(UserDto other)
        {
            return this.Id == other.Id;
        }
    }
}
