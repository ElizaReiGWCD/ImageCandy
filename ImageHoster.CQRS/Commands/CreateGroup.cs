using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class CreateGroup : Command
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> AlreadyExistingGroups { get; set; }
        public GroupPrivacy Privacy { get; set; }
    }
}
