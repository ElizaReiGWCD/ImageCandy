using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageHoster.CQRS.ReadModel.Eventhandlers;
using ImageHoster.CQRS.Commands;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System.Collections.Generic;
using ImageHoster.CQRS.Domain;
using System.IO;
using ImageHoster.CQRS.Events;
using System.Linq;

namespace ImageHoster.GUI.Tests
{
    [TestClass]
    public class GroupUnitTests
    {
        Guid photoguid;
        Guid userguid;
        Guid albumguid;
        List<Guid> groupguids;

        FakeBus bus;
        EventStore eventStore;
        Repository<User> userRepo;
        Repository<Photo> photoRepo;
        Repository<Album> albumRepo;
        Repository<Group> groupRepo;

        [TestInitialize]
        public void Initialize()
        {
            photoguid = Guid.NewGuid();
            userguid = Guid.NewGuid();
            albumguid = Guid.NewGuid();
            groupguids = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            bus = new FakeBus();
            eventStore = new EventStore(bus);
            userRepo = new Repository<User>(eventStore);
            photoRepo = new Repository<Photo>(eventStore);
            albumRepo = new Repository<Album>(eventStore);
            groupRepo = new Repository<Group>(eventStore);

            var userCommands = new UserCommandHandler(userRepo);
            InitializeClass.RegisterHandlers(bus, userCommands);

            var photoCommands = new PhotoCommandHandler(photoRepo);
            InitializeClass.RegisterHandlers(bus, photoCommands);

            var albumCommands = new AlbumCommandHandler(albumRepo, photoRepo);
            InitializeClass.RegisterHandlers(bus, albumCommands);

            var groupCommands = new GroupCommandHandler(groupRepo);
            InitializeClass.RegisterHandlers(bus, groupCommands);

            var userEvents = new UserEventHandler();
            InitializeClass.RegisterHandlers(bus, userEvents);

            var photoEvents = new PhotoEventHandler();
            InitializeClass.RegisterHandlers(bus, photoEvents);

            var albumEvents = new AlbumEventHandler();
            InitializeClass.RegisterHandlers(bus, albumEvents);

            var groupEvents = new GroupEventHandler();
            InitializeClass.RegisterHandlers(bus, groupEvents);
        }

        [TestCleanup]
        public void Cleanup()
        {
            bus = null;
            eventStore = null;
            userRepo = null;
            photoRepo = null;
            albumRepo = null;
            groupRepo = null;
        }

        [TestMethod]
        public void Test_BanPhoto_InGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            }, bus));

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is AddedPhotoToGroup));
            }

            bus.Send(new BanPhotoFromGroup() { UserId = userguid, PhotoId = photoguid, GroupId = groupguids[0] });

            Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is BannedPhotoFromGroup));
        }

        [TestMethod]
        public void Test_BanPhoto_NotInGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids.Take(1)
            }, bus));

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new BanPhotoFromGroup() { UserId = userguid, PhotoId = photoguid, GroupId = groupguids[1] });

            Assert.IsFalse(eventStore.EventsStored[groupguids[1]].Any(e => e.EventData is BannedPhotoFromGroup));
        }

        [TestMethod]
        public void Test_BanAlbum_InGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            }, bus));

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = new Privacy()
                {
                    Level = CQRS.PrivacyLevel.Group,
                    OnlyForLoggedInUsers = true,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                }
            });

            bus.Send(new AddPhotoToAlbum() { AlbumId = albumguid, Id = Guid.NewGuid(), PhotoId = photoguid, UserId = userguid });

            bus.Send(new BanAlbumFromGroup() { AlbumId = albumguid, Photos = new List<Guid>() { photoguid }, UserId = userguid, GroupId = groupguids[0] });

            Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is BannedPhotoFromGroup));
            Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is BannedAlbumFromGroup));
        }

        [TestMethod]
        public void Test_BanAlbum_NotInGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            }, bus));

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = new Privacy()
                {
                    Level = CQRS.PrivacyLevel.Public,
                    OnlyForLoggedInUsers = true,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                }
            });

            bus.Send(new AddPhotoToAlbum() { AlbumId = albumguid, Id = Guid.NewGuid(), PhotoId = photoguid, UserId = userguid });

            bus.Send(new BanAlbumFromGroup() { AlbumId = albumguid, Photos = new List<Guid>() { photoguid }, UserId = userguid, GroupId = groupguids[0] });

            Assert.IsFalse(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is BannedPhotoFromGroup));
            Assert.IsFalse(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is BannedAlbumFromGroup));
        }
    }
}
