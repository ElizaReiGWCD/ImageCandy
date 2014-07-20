using ImageHoster.CQRS.ReadModel.Dto;
using Raven.Client;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel
{
    public class ReadmodelSnapshot : IReadmodelSnapshot
    {
        private class Snapshot
        {
            public string id;
            public List<AlbumDto> albums { get; set; }
            public List<PhotoDto> photos {get;set;} 
            public List<GroupDto> groups {get;set;}
            public List<UserDto> users { get; set; } 
        }

        public async void RestoreReadModel(IDocumentSession session)
        {
            var snapshots = session.Load<dynamic>("snapshot/1");

            //var snapshot = snapshots.OrderByDescending(s => s.id).First();

            InMemoryDatabase.albums = snapshots.albums;
            InMemoryDatabase.groups = snapshots.groups;
            InMemoryDatabase.photos = snapshots.photos;
            InMemoryDatabase.users = snapshots.users;
        }

        public void SaveReadModel(IDocumentSession session)
        {
            var snap = new Snapshot();

            snap.users = InMemoryDatabase.users;
            snap.albums = InMemoryDatabase.albums;
            snap.groups = InMemoryDatabase.groups;
            snap.photos = InMemoryDatabase.photos;

            session.Store(new { InMemoryDatabase.users }, "snapshot/1");
            session.SaveChanges();
        }
    }
}
