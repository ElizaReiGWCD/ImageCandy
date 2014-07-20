using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class RegisterUser : Command
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
        public IEnumerable<string> AlreadyExistingNames { get; set; }

        public RegisterUser(Guid id, string username, string password, string salt, string email, string firstname, string lastname, string sex, string about, IEnumerable<string> alreadyExistingNames)
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
            AlreadyExistingNames = alreadyExistingNames;
        }
    }
}
