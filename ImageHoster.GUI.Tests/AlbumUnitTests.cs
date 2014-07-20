using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using ImageHoster.CQRS.Domain;
using System.Collections.Generic;
using System.Linq;
using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel.Eventhandlers;
using System.IO;

namespace ImageHoster.GUI.Tests
{
    [TestClass]
    public class AlbumUnitTests
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
        public void Test_AddPhotoToAlbum_Public_Public()
        {
            Photo photo = new Photo(photoguid, userguid, "", new Privacy() { Level = CQRS.PrivacyLevel.Public, OnlyForLoggedInUsers = true, OwnerId = userguid, Published = true, VisibleToGroups = new List<Guid>() });
            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = new List<Guid>()
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Public_Hidden()
        {
            Photo photo = new Photo(photoguid, userguid, "", new Privacy() { Level = CQRS.PrivacyLevel.Public, OnlyForLoggedInUsers = true, OwnerId = userguid, Published = true, VisibleToGroups = new List<Guid>() });
            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Hidden,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = new List<Guid>()
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Private_Private()
        {
            Photo photo = new Photo(photoguid, userguid, "", new Privacy() { Level = CQRS.PrivacyLevel.Private, OnlyForLoggedInUsers = true, OwnerId = userguid, Published = true, VisibleToGroups = new List<Guid>() });
            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Private,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = new List<Guid>()
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Public_Private()
        {
            Photo photo = new Photo(photoguid, userguid, "", new Privacy() { Level = CQRS.PrivacyLevel.Public, OnlyForLoggedInUsers = true, OwnerId = userguid, Published = true, VisibleToGroups = new List<Guid>() });
            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Private,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = new List<Guid>()
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Private_Public()
        {
            Photo photo = new Photo(photoguid, userguid, "", new Privacy() { Level = CQRS.PrivacyLevel.Private, OnlyForLoggedInUsers = true, OwnerId = userguid, Published = true, VisibleToGroups = new List<Guid>() });
            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = new List<Guid>()
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsFalse(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Group_Group_Photo_Superset()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Photo photo = new Photo(photoguid, userguid, "",
                new Privacy()
                {
                    Level = CQRS.PrivacyLevel.Group,
                    OnlyForLoggedInUsers = true,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                });

            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Group,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids.Take(1)
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_Group_Group_Photo_Subset()
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

            Assert.IsFalse(eventStore.EventsStored[albumguid].Any(e => e.EventData is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_LoggedIn_NotLoggedIn()
        {
            Photo photo = new Photo(photoguid, userguid, "",
                new Privacy()
                {
                    Level = CQRS.PrivacyLevel.Public,
                    OnlyForLoggedInUsers = false,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids.Take(1)
                });

            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = true,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsTrue(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_AddPhotoToAlbum_NotLoggedIn_LoggedIn()
        {
            Photo photo = new Photo(photoguid, userguid, "",
                new Privacy()
                {
                    Level = CQRS.PrivacyLevel.Public,
                    OnlyForLoggedInUsers = true,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids.Take(1)
                });

            Album album = new Album(albumguid, userguid, "", "", new Privacy()
            {
                Level = CQRS.PrivacyLevel.Public,
                OnlyForLoggedInUsers = false,
                OwnerId = userguid,
                Published = true,
                VisibleToGroups = groupguids
            });

            album.AddPhoto(Guid.NewGuid(), userguid, photo, albumguid);

            Assert.IsFalse(album.GetUncommittedChanges().Any(e => e is AddedPhotoToAlbum));
        }

        [TestMethod]
        public void Test_CreateAlbum_NotGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

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
                        OnlyForLoggedInUsers = false,
                        OwnerId = userguid,
                        Published = true,
                        VisibleToGroups = groupguids
                    }
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            foreach (var g in groupguids)
            {
                Assert.IsFalse(eventStore.EventsStored[g].Any(e => e.EventData is AddedAlbumToGroup));
            }
        }

        [TestMethod]
        public void Test_CreateAlbum_InGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

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
                    OnlyForLoggedInUsers = false,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                }
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is AddedAlbumToGroup));
            }
        }

        [TestMethod]
        public void Test_DeleteAlbum_NotGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

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
                    OnlyForLoggedInUsers = false,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                }
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new DeleteAlbum() { AlbumId = albumguid, Bus = bus, Id = Guid.NewGuid(), User = userguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is DeletedAlbum));

            foreach (var g in groupguids)
            {
                Assert.IsFalse(eventStore.EventsStored[g].Any(e => e.EventData is AddedAlbumToGroup));
                Assert.IsFalse(eventStore.EventsStored[g].Any(e => e.EventData is DeletedAlbumFromGroup));
            }
        }

        [TestMethod]
        public void Test_DeleteAlbum_InGroup()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

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
                    OnlyForLoggedInUsers = false,
                    OwnerId = userguid,
                    Published = true,
                    VisibleToGroups = groupguids
                }
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new DeleteAlbum() { AlbumId = albumguid, Bus = bus, Id = Guid.NewGuid(), User = userguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is DeletedAlbum));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is AddedAlbumToGroup));
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is DeletedAlbumFromGroup));
            }
        }

        [TestMethod]
        public void Test_ChangeAlbum_FromPublicToGroup()
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

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new EditAlbumInfo() { AlbumId = albumguid, Bus = bus, Description = "", Id = Guid.NewGuid(), OldPrivacy = oldPrivacy, Privacy = newPrivacy, Title = "", User = userguid, PhotoPrivacies = new Dictionary<Guid,Privacy>() });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is ChangedAlbumInfo));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is AddedAlbumToGroup));
            }
        }

        [TestMethod]
        public void Test_ChangeAlbum_FromGroupToPublic()
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

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new EditAlbumInfo() { AlbumId = albumguid, Bus = bus, Description = "", Id = Guid.NewGuid(), OldPrivacy = oldPrivacy, Privacy = newPrivacy, Title = "", User = userguid, PhotoPrivacies = new Dictionary<Guid,Privacy>() });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is ChangedAlbumInfo));

            foreach (var g in groupguids)
            {
                Assert.IsTrue(eventStore.EventsStored[g].Any(e => e.EventData is DeletedAlbumFromGroup));
            }
        }

        [TestMethod]
        public void Test_ChangeAlbum_FromGroupToGroup()
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

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));

            bus.Send(new EditAlbumInfo() { AlbumId = albumguid, Bus = bus, Description = "", Id = Guid.NewGuid(), OldPrivacy = oldPrivacy, Privacy = newPrivacy, Title = "", User = userguid, PhotoPrivacies = new Dictionary<Guid,Privacy>() });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is ChangedAlbumInfo));

            Assert.IsTrue(eventStore.EventsStored[groupguids[0]].Any(e => e.EventData is DeletedAlbumFromGroup));
            Assert.IsTrue(eventStore.EventsStored[groupguids[1]].Any(e => e.EventData is AddedAlbumToGroup));
            Assert.IsTrue(eventStore.EventsStored[groupguids[2]].Any(e => e.EventData is AddedAlbumToGroup));
        }

        [TestMethod]
        public void Test_ChangeAlbumPhotoIncompatibility_PrivateToPublic()
        {
            bus.Send(new RegisterUser(userguid, "asdas", "", "", "", "", "", "", "", new List<string>()));

            foreach (var g in groupguids)
                bus.Send(new CreateGroup() { Id = g, AlreadyExistingGroups = new List<string>(), Description = "", Name = "", UserId = userguid, Privacy = new GroupPrivacy() });

            Privacy oldPrivacy = new Privacy()
            {
                Level = CQRS.PrivacyLevel.Private,
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

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "", "", oldPrivacy, bus));

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));
            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new AddPhotoToAlbum() { UserId = userguid, PhotoId = photoguid, Id = Guid.NewGuid(), AlbumId = albumguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is AddedPhotoToAlbum));

            bus.Send(new EditAlbumInfo() { AlbumId = albumguid, Bus = bus, Description = "", Id = Guid.NewGuid(), OldPrivacy = oldPrivacy, Privacy = newPrivacy, Title = "", User = userguid, PhotoPrivacies = new Dictionary<Guid, Privacy>() {{photoguid, oldPrivacy} } });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is ChangedAlbumInfo));

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is RemovedPhotoFromAlbum));
        }

        [TestMethod]
        public void Test_ChangeAlbumPhotoIncompatibility_PublicToPrivate()
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

            bus.Send(new CreateAlbum()
            {
                Bus = bus,
                Description = "",
                Id = albumguid,
                Title = "",
                UserId = userguid,
                Privacy = oldPrivacy
            });

            bus.Send(new UploadPhoto(photoguid, userguid, new MemoryStream(), "", "", oldPrivacy, bus));

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is CreatedAlbum));
            Assert.IsTrue(eventStore.EventsStored[photoguid].Any(e => e.EventData is PhotoUploaded));

            bus.Send(new AddPhotoToAlbum() { UserId = userguid, PhotoId = photoguid, Id = Guid.NewGuid(), AlbumId = albumguid });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is AddedPhotoToAlbum));

            bus.Send(new EditAlbumInfo() { AlbumId = albumguid, Bus = bus, Description = "", Id = Guid.NewGuid(), OldPrivacy = oldPrivacy, Privacy = newPrivacy, Title = "", User = userguid, PhotoPrivacies = new Dictionary<Guid, Privacy>() { { photoguid, oldPrivacy } } });

            Assert.IsTrue(eventStore.EventsStored[albumguid].Any(e => e.EventData is ChangedAlbumInfo));

            Assert.IsFalse(eventStore.EventsStored[albumguid].Any(e => e.EventData is RemovedPhotoFromAlbum));
        }
    }
}
