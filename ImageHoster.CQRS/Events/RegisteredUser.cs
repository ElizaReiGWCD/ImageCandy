using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Events
{
    public class RegisteredUser : Event
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string About { get; set; }

        public RegisteredUser(Guid id, string username, string password, string salt, string email, string firstname, string lastname, string sex, string about)
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
        }
    }
}
