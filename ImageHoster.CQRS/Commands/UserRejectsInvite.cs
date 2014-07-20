﻿using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Commands
{
    public class UserRejectsInvite : Command
    {
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
    }
}
