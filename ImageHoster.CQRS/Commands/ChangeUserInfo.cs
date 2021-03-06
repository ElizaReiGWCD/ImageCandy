﻿using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class ChangeUserInfo : Command
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Sex { get; set; }
        public string About { get; set; }

        public ChangeUserInfo(Guid id, string password, string firstname, string lastname, string sex, string about)
        {
            Password = password;
            Id = id;
            FirstName = firstname;
            LastName = lastname;
            Sex = sex;
            About = about;
        }
    }
}
