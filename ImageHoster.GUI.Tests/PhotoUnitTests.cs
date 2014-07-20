using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel.Eventhandlers;
using System.IO;
using System.Linq;
using ImageHoster.CQRS.Events;

namespace ImageHoster.GUI.Tests
{
    [TestClass]
    public class PhotoUnitTests
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
        public void Test_UploadPhoto_NoGroup()
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

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            foreach (var g in groupguids)
            {
                Assert.IsFalse(eventStore.EventsStored[g].Any(e => e.EventData is AddedPhotoToGroup));
            }
        }

        [TestMethod]
        public void Test_UploadPhoto_InGroup()
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
        }

        [TestMethod]
        public void Test_DeletePhoto_NoGroup()
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

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new DeletePhoto() { Bus = bus, Id = Guid.NewGuid(), PhotoId = photoguid, UserId = userguid, Albums = new List<Guid>() });

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is DeletedPhoto));

            foreach (var g in groupguids)
            {
                Assert.IsFalse(eventStore.EventsStored[g].Any(e => e.EventData is DeletedPhotoFromGroup));
            }
        }

        [TestMethod]
        public void Test_DeletePhoto_InGroup()
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

            bus.Send(new DeletePhoto() { Bus = bus, Id = Guid.NewGuid(), PhotoId = photoguid, UserId = userguid, Albums = new List<Guid>() });

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is DeletedPhoto));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is DeletedPhotoFromGroup));
            }
        }

        [TestMethod]
        public void Test_ChangePhoto_FromPublicToGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Privacy oldPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            Privacy newPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", oldPrivacy, bus));

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new EditPhotoInfo() { Bus = bus, OldPrivacy = oldPrivacy, Description = "asda", Id = Guid.NewGuid(), PhotoId = photoguid, Privacy = newPrivacy, Title = "asdas", User = userguid, Albums = new List<Guid>() });

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is AddedPhotoToGroup));
            }
        }

        [TestMethod]
        public void Test_ChangePhoto_FromGroupToPublic()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Privacy oldPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            Privacy newPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", oldPrivacy, bus));

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new EditPhotoInfo() { Bus = bus, OldPrivacy = oldPrivacy, Description = "asda", Id = Guid.NewGuid(), PhotoId = photoguid, Privacy = newPrivacy, Title = "asdas", User = userguid, Albums = new List<Guid>() });

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is DeletedPhotoFromGroup));
            }
        }

        [TestMethod]
        public void Test_ChangePhoto_FromGroupToGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Privacy oldPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids.Take(1)
            };

            Privacy newPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids.Skip(1)
            };

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", oldPrivacy, bus));

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new EditPhotoInfo() { Bus = bus, OldPrivacy = oldPrivacy, Description = "asda", Id = Guid.NewGuid(), PhotoId = photoguid, Privacy = newPrivacy, Title = "asdas", User = userguid, Albums = new List<Guid>() });

           Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is DeletedPhotoFromGroup));
           Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is AddedPhotoToGroup));
           Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is AddedPhotoToGroup));
        }

        [TestMethod]
        public void Test_DeletePhotoInAlbum()
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

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));
            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new AddPhotoToAlbum() { UserId = userguid, PhotoId = photoguid, Id = Guid.NewGuid(), AlbumId = albumguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is AddedPhotoToAlbum));

            bus.Send(new DeletePhoto() { Albums = new List<Guid>() { albumguid }, UserId = userguid, PhotoId = photoguid, Id = Guid.NewGuid(), Bus = bus });

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is DeletedPhoto));
            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is RemovedPhotoFromAlbum));
        }

        [TestMethod]
        public void Test_ChangePhotoInAlbum()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Privacy oldPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            Privacy newPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Private,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            };

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "sads", "image/jpeg", oldPrivacy, bus));

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));
            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new AddPhotoToAlbum() { UserId = userguid, PhotoId = photoguid, Id = Guid.NewGuid(), AlbumId = albumguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is AddedPhotoToAlbum));

            bus.Send(new EditPhotoInfo() { Bus = bus, OldPrivacy = oldPrivacy, Description = "asda", Id = Guid.NewGuid(), PhotoId = photoguid, Privacy = newPrivacy, Title = "asdas", User = userguid, Albums = new List<Guid>() { albumguid } });

            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is ChangedPhotoInfo));
            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is RemovedPhotoFromAlbum));
        }
    }
}
