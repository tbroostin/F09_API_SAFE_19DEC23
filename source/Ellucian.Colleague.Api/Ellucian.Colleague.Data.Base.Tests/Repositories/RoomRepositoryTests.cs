// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class RoomRepositoryTests : BaseRepositorySetup
    {
        RoomRepository repository;
        private Mock<IRoomRepository> RoomRepositoryMock;

        static Collection<Rooms> rooms = TestRoomsRepository.Rooms;
        static Collection<Rooms> rooms100 = new Collection<Rooms>(rooms.Where(r => r.RoomCapacity >= 100).ToList());
        static Collection<Rooms> rooms10000 = new Collection<Rooms>(rooms.Where(r => r.RoomCapacity >= 10000).ToList());       

        [TestInitialize]
        public void MainInitialize()
        {
            // Initialize Mock framework
            MockInitialize();
        }

        [TestClass]
        public class RoomRepository_GetRooms_NoCache : RoomRepositoryTests
        {
            IEnumerable<Room> result, entities;
            string cacheKey;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                MockRecordsAsync<Rooms>("ROOMS", rooms);
                // Build the test repository
                repository = new RoomRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                cacheKey = repository.BuildFullCacheKey("AllRooms_GUID");

                result = repository.GetRoomsAsync(true).GetAwaiter().GetResult(); 
                entities = result.ToList();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Records()
            {
                Assert.AreEqual(rooms.Count, result.Count());
                CollectionAssert.AllItemsAreInstancesOfType(result.ToList(), typeof(Room));
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Guid()
            {
                var roomGuids = rooms.Select(x => x.RecordGuid).ToList();
                var guids = result.Select(x => x.Guid).ToList();
                Assert.AreEqual(roomGuids.Count, guids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(guids, typeof(string));
                CollectionAssert.AreEqual(roomGuids, guids);
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Id()
            {
                var roomIds = rooms.Select(x => x.Recordkey).ToList();
                var ids = result.Select(x => x.Id).ToList();
                Assert.AreEqual(roomIds.Count, ids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(ids, typeof(string));
                CollectionAssert.AreEqual(roomIds, ids);
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Description()
            {
                var roomDescs = rooms.Select(x => x.RoomName).ToList();
                var descs = result.Select(x => x.Description).ToList();
                Assert.AreEqual(roomDescs.Count, descs.Count);
                CollectionAssert.AllItemsAreInstancesOfType(descs, typeof(string));
                CollectionAssert.AreEqual(roomDescs, descs);
            }

           
            [TestMethod]
            public async Task RoomRepository_GetRooms_NoCache_AddToCacheWithoutLock()
            {
                MockCacheSetup<IEnumerable<Room>>(cacheKey, entities);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
              x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));


                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(true)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms are now in the cache
                VerifyCache<IEnumerable<Room>>(cacheKey, entities);
            }

          
            [TestMethod]
            public async Task RoomRepository_GetRooms_NoCache_AddToCacheWithLock()
            {
                object lockObject = "RecordLocked";
                //MockCacheSetupAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
              x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(true)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms are now in the cache
                //VerifyCacheAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
       
            }

          
            [TestMethod]
            public async Task RoomRepository_GetRooms_GetAddToCacheNoLock()
            {
                MockCacheSetup<IEnumerable<Room>>(cacheKey, entities);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));
                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(false)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
                VerifyCache<IEnumerable<Room>>(cacheKey, entities); 
            }

            
            [TestMethod]
            public async Task RoomRepository_GetRooms_GetAddToCacheWithLock()
            {
                object lockObject = "RecordLocked";
               // MockCacheSetupAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                      x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                      .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                
                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(false)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
               // VerifyCacheAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
       
            }

           
            [TestMethod]
            public async Task RoomRepository_GetRooms_GetFromCache()
            {
                MockCacheSetup<IEnumerable<Room>>(cacheKey, entities, true);
                // Get the rooms
               // var result = this.repository.GetRoomsAsync(false).GetAwaiter().GetResult().ToList();
                var result = (await this.repository.GetRoomsAsync(false)).ToList(); 
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
                VerifyCache<IEnumerable<Room>>(cacheKey, entities, true);
            }
        }

        [TestClass]
        public class RoomRepository_GetRooms_WithCache : RoomRepositoryTests
        {
            IEnumerable<Room> result;

            [TestInitialize]
            public void Initialize()
            {
                base.MainInitialize();
                MockRecordsAsync<Rooms>("ROOMS", rooms);
                // Build the test repository
                this.repository = new RoomRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

               // result = repository.GetRoomsAsync(false).GetAwaiter().GetResult();
            }

            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_Records()
            {
                result = await repository.GetRoomsAsync(false);
                Assert.AreEqual(rooms.Count, result.Count());
                CollectionAssert.AllItemsAreInstancesOfType(result.ToList(), typeof(Room));
            }

            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_Guid()
            {
                result = await repository.GetRoomsAsync(false); 
                var roomGuids = rooms.Select(x => x.RecordGuid).ToList();
                var guids = result.Select(x => x.Guid).ToList();
                Assert.AreEqual(roomGuids.Count, guids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(guids, typeof(string));
                CollectionAssert.AreEqual(roomGuids, guids);
            }

            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_Id()
            {
                result = await repository.GetRoomsAsync(false);
                var roomIds = rooms.Select(x => x.Recordkey).ToList();
                var ids = result.Select(x => x.Id).ToList();
                Assert.AreEqual(roomIds.Count, ids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(ids, typeof(string));
                CollectionAssert.AreEqual(roomIds, ids);
            }

            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_Description()
            {
                result = await repository.GetRoomsAsync(false); 
                var roomDescs = rooms.Select(x => x.RoomName).ToList();
                var descs = result.Select(x => x.Description).ToList();
                Assert.AreEqual(roomDescs.Count, descs.Count);
                CollectionAssert.AllItemsAreInstancesOfType(descs, typeof(string));
                CollectionAssert.AreEqual(roomDescs, descs);
            }

           
            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_VerifyNoCacheReadWithoutLock()
            {
                var roomList = (await this.repository.GetRoomsAsync(true)).ToList();
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is in cache
                //  -to cache "Get" request, return data so we know it's getting data from "cache"
                
                string cacheKey = this.repository.BuildFullCacheKey("AllRooms_GUID");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                
                //cacheProviderMock.Setup(x => x.GetAndLock(cacheKey, out lockObject, null)).Returns(roomList);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));

                // Make sure we can verify that it's in the cache
                //cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Verifiable();
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                //cacheProviderMock.Setup(x => x.AddAndUnlock(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<object>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(true)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms were in the cache
                //cacheProviderMock.Verify(x => x.Get(cacheKey, null));
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<CacheItemPolicy>(), null));
                //cacheProviderMock.Verify(x => x.AddAndUnlock(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<object>(), It.IsAny<CacheItemPolicy>(), null));
            }

           


            [TestMethod]
            public async Task RoomRepository_GetRooms_WithCache_VerifyNoCacheReadWithLock()
            {
                var roomList = (await this.repository.GetRoomsAsync(true)).ToList();
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is in cache
                //  -to cache "Get" request, return data so we know it's getting data from "cache"
                object lockObject = "RecordLocked";
                string cacheKey = this.repository.BuildFullCacheKey("AllRooms_GUID");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                //cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(roomList);
                cacheProviderMock.Setup(x => x.GetAndLock(cacheKey, out lockObject, null)).Returns(roomList);

                // Make sure we can verify that it's in the cache
                //cacheProviderMock.Setup(x => x.AddAndUnlock(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<object>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the rooms
                var result = (await this.repository.GetRoomsAsync(true)).ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms were in the cache
                // cacheProviderMock.Verify(x => x.AddAndUnlock(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<object>(), It.IsAny<CacheItemPolicy>(), null));
                cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
       
            }
        }

        [TestClass]
        public class RoomRepository_GetRoomsWithPaging_NoCache : RoomRepositoryTests
        {
            //IEnumerable<Room> result, entities;
            Tuple<IEnumerable<Room>, int> result;
            IEnumerable<Room> entities;
            public string[] roomIds;

            string cacheKey;

            [TestInitialize]
            public void Initialize()
            {
                MainInitialize();
                MockRecordsAsync<Rooms>("ROOMS", rooms);

                roomIds = new string[2];
                roomIds[0] = "id*1";
                roomIds[1] = "id*2";

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(roomIds);

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Rooms>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>((x, y, z) =>
                    Task.FromResult(new Collection<Rooms>(rooms.Select(a =>
                        new Rooms()
                        {
                            Recordkey = a.Recordkey,
                            RecordGuid = a.RecordGuid,
                            RoomName = a.RoomName,
                            RoomFloor = a.RoomFloor,
                            RoomCapacity = a.RoomCapacity,
                            RoomType = a.RoomType,
                            RoomWing = a.RoomWing,
                            RoomCharacteristics = a.RoomCharacteristics
                        }).ToList())));

                // Build the test repository
                repository = new RoomRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                cacheKey = repository.BuildFullCacheKey("AllRooms_GUID");

                result = repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), true).GetAwaiter().GetResult();
                entities = result.Item1.ToList();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestMethod]
            public async Task RoomRepository_GetRoomsWithPaging_Empty()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[0]);

                var result = await repository.GetRoomsWithPagingAsync(0, 10, true);

                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Guid()
            {
                var roomGuids = rooms.Select(x => x.RecordGuid).ToList();
                var guids = result.Item1.Select(x => x.Guid).ToList();
                Assert.AreEqual(roomGuids.Count, guids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(guids, typeof(string));
                CollectionAssert.AreEqual(roomGuids, guids);
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Id()
            {
                var roomIds = rooms.Select(x => x.Recordkey).ToList();
                var ids = result.Item1.Select(x => x.Id).ToList();
                Assert.AreEqual(roomIds.Count, ids.Count);
                CollectionAssert.AllItemsAreInstancesOfType(ids, typeof(string));
                CollectionAssert.AreEqual(roomIds, ids);
            }

            [TestMethod]
            public void RoomRepository_GetRooms_NoCache_Description()
            {
                var roomDescs = rooms.Select(x => x.RoomName).ToList();
                var descs = result.Item1.Select(x => x.Description).ToList();
                Assert.AreEqual(roomDescs.Count, descs.Count);
                CollectionAssert.AllItemsAreInstancesOfType(descs, typeof(string));
                CollectionAssert.AreEqual(roomDescs, descs);
            }


            [TestMethod]
            public async Task RoomRepository_GetRooms_NoCache_AddToCacheWithoutLock()
            {
                //MockCacheSetup<IEnumerable<Room>>(cacheKey, entities);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
              x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));


                // Get the rooms
                var result = (await this.repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), true)).Item1.ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms are now in the cache
                //VerifyCache<IEnumerable<Room>>(cacheKey, entities);
            }


            [TestMethod]
            public async Task RoomRepository_GetRooms_NoCache_AddToCacheWithLock()
            {
                //object lockObject = "RecordLocked";
                //MockCacheSetupAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                //cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
              x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Get the rooms
                var result = (await this.repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), true)).Item1.ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                Assert.AreEqual(rooms[rooms.Count - 1].RecordGuid, result[result.Count - 1].Guid);

                // Verify that the rooms are now in the cache
                //VerifyCacheAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                //cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));

            }


            [TestMethod]
            public async Task RoomRepository_GetRooms_GetAddToCacheNoLock()
            {
                //MockCacheSetup<IEnumerable<Room>>(cacheKey, entities);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));
                // Get the rooms
                var result = (await this.repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).Item1.ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
                //VerifyCache<IEnumerable<Room>>(cacheKey, entities);
            }


            [TestMethod]
            public async Task RoomRepository_GetRooms_GetAddToCacheWithLock()
            {
                //object lockObject = "RecordLocked";
                // MockCacheSetupAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                //cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                      x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                      .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Get the rooms
                var result = (await this.repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).Item1.ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
                // VerifyCacheAsync<IEnumerable<Room>>(cacheKey, entities, false, lockObject);
                //cacheProviderMock.Verify(x => x.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<Room>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));

            }


            [TestMethod]
            public async Task RoomRepository_GetRooms_GetFromCache()
            {
                //MockCacheSetup<IEnumerable<Room>>(cacheKey, entities, true);
                // Get the rooms
                // var result = this.repository.GetRoomsAsync(false).GetAwaiter().GetResult().ToList();
                var result = (await this.repository.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).Item1.ToList();
                Assert.AreEqual(rooms.Count, result.Count);
                Assert.AreEqual(rooms[0].Recordkey, result[0].Id);
                int index = rooms.Count > 0 ? rooms.Count - 1 : rooms.Count;
                Assert.AreEqual(rooms[index].RecordGuid, result[index].Guid);

                // Verify that the rooms are now in the cache
                //VerifyCache<IEnumerable<Room>>(cacheKey, entities, true);
            }
        }

        [TestClass]
        public class RoomRepository_GetRooms_Exceptions : BaseRepositorySetup
        {
            RoomRepository repository;
            IEnumerable<Room> roomsList;
            public string[] roomIds;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                roomsList = new List<Room>() 
                {
                    new Room("guid1", "id*1", "desc1")
                    {
                        Floor = "floor1",
                        Name = "name1",
                        Capacity = 1,
                        RoomType = "type1",
                        Wing = "wing1",
                        Characteristics = new List<string>() {"1"}
                    },
                    new Room("guid2", "id*2", "desc2")
                    {
                        Floor = "floor2",
                        Name = "name2",
                        Capacity = 2,
                        RoomType = "type2",
                        Wing = "wing2",
                        Characteristics = new List<string>() {"2"}
                    },
                    new Room("guid3", "id*3", "desc3")
                    {
                        Floor = "floor3",
                        Name = "name3",
                        Capacity = 3,
                        RoomType = "type3",
                        Wing = "wing3",
                        Characteristics = new List<string>() {"3"}
                    }
                };

                roomIds = new string[2];
                roomIds[0] = "id*1";
                roomIds[1] = "id*2";

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Rooms>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>((x, y, z) =>
                    Task.FromResult(new Collection<Rooms>(roomsList.Select(a =>
                        new Rooms()
                        {
                            Recordkey = a.Code,
                            RecordGuid = a.Guid,
                            RoomName = a.Name,
                            RoomFloor = a.Floor,
                            RoomCapacity = a.Capacity,
                            RoomType = a.RoomType,
                            RoomWing = a.Wing,
                            RoomCharacteristics = a.Characteristics
                        }).ToList())));

                // Build the test repository
                this.repository = new RoomRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task RoomRepository_GetRoomsWithPaging_ArgumentException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(roomIds);

                var result = await repository.GetRoomsWithPagingAsync(0, 10, true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RoomRepository_GetRooms_IgnoreCache_NullData()
            {
                MockRecordsAsync<Rooms>("ROOMS", (IEnumerable<Rooms>)null);
                var result = await repository.GetRoomsAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RoomRepository_GetRooms_IgnoreCache_NoData()
            {
                MockRecordsAsync<Rooms>("ROOMS", new Collection<Rooms>());
                var result = await repository.GetRoomsAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RoomRepository_GetRooms_UseCache_NullData()
            {
                MockRecordsAsync<Rooms>("ROOMS", (IEnumerable<Rooms>)null);
                var result = await repository.GetRoomsAsync(false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RoomRepository_GetRooms_UseCache_NoData()
            {
                MockRecordsAsync<Rooms>("ROOMS", new Collection<Rooms>());
                var result = await repository.GetRoomsAsync(false);
            }
        }
    }
}
