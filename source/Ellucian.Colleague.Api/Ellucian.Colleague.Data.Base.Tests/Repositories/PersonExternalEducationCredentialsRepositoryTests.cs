//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.  

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonExternalEducationCredentialsRepositoryTests
    {
        /// <summary>
        /// Test class for PersonExternalEducationCredentials codes
        /// </summary>
        [TestClass]
        public class PersonExternalEducationCredentialsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ExternalEducation> _personExternalEducationCredentialsCollection;
            Collection<DataContracts.Person> _personCollection = new Collection<DataContracts.Person>();
            string codeItemName;
            private readonly int offset = 0;
            private readonly int limit = 100;
            Collection<AcadCredentials> entityCollection = null;
            PersonExternalEducationCredentialsRepository personExternalEducationCredentialsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _personExternalEducationCredentialsCollection = new List<ExternalEducation>()
                {
                    new Domain.Base.Entities.ExternalEducation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "1",
                        Id = "1",
                        AcadAcadProgram = "MATH.BA",
                        AcadDegree = "BA",
                        AcadDegreeDate = new DateTime(2019, 11, 15),
                        AcadStartDate = new DateTime(2017, 01, 15),
                        InstAttendGuid = "7a2bf6b5-cdcd-4c8f-b5d8-4664bf5b3fbc"
                    },
                    new Domain.Base.Entities.ExternalEducation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "2",
                        Id = "2",
                        AcadAcadProgram = "ENGL.BA",
                        AcadDegree = "BA",
                        AcadDegreeDate = new DateTime(2016, 11, 15),
                        AcadStartDate = new DateTime(2014, 01, 15),
                        InstAttendGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa8875f0d"
                    },
                    new Domain.Base.Entities.ExternalEducation("d2253ac7-9931-4560-b42f-1fccd43c952e")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "3",
                        Id = "3",
                        AcadAcadProgram = "HIST.BA",
                        AcadDegree = "BA",
                        AcadDegreeDate = new DateTime(2014, 11, 15),
                        AcadStartDate = new DateTime(2012, 01, 15),
                        InstAttendGuid = "d2253ac7-9931-2289-b42f-1fccd43c952e"
                    }
                };

                // Build repository
                personExternalEducationCredentialsRepo = BuildValidPersonExternalEducationCredentials();
                codeItemName = personExternalEducationCredentialsRepo.BuildFullCacheKey("PersonExternalEducationCredentialsKeys");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _personExternalEducationCredentialsCollection = null;
                personExternalEducationCredentialsRepo = null;
            }

            [TestMethod]
            public async Task GetPersonExternalEducationCredentialsCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(codeItemName, null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_personExternalEducationCredentialsCollection.Select(peec => peec.Id).ToArray(), new SemaphoreSlim(1, 1)));

                var resultTuple = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsAsync(offset, limit, null, "", "", "", false);
                var result = resultTuple.Item1;

                for (int i = 0; i < _personExternalEducationCredentialsCollection.Count(); i++)
                {
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).Id, result.ElementAt(i).Id);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadPersonId, result.ElementAt(i).AcadPersonId);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadInstitutionsId, result.ElementAt(i).AcadInstitutionsId);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadAcadProgram, result.ElementAt(i).AcadAcadProgram);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadAwards, result.ElementAt(i).AcadAwards);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadCcd, result.ElementAt(i).AcadCcd);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadCcdDate, result.ElementAt(i).AcadCcdDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadDegree, result.ElementAt(i).AcadDegree);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadDegreeDate, result.ElementAt(i).AcadDegreeDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadEndDate, result.ElementAt(i).AcadEndDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadGpa, result.ElementAt(i).AcadGpa);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadHonors, result.ElementAt(i).AcadHonors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadMajors, result.ElementAt(i).AcadMajors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadMinors, result.ElementAt(i).AcadMinors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankDenominator, result.ElementAt(i).AcadRankDenominator);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankNumerator, result.ElementAt(i).AcadRankNumerator);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankPercent, result.ElementAt(i).AcadRankPercent);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadSpecialization, result.ElementAt(i).AcadSpecialization);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadStartDate, result.ElementAt(i).AcadStartDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadThesis, result.ElementAt(i).AcadThesis);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadComments, result.ElementAt(i).AcadComments);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadNoYears, result.ElementAt(i).AcadNoYears);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstAttendGuid, result.ElementAt(i).InstAttendGuid);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstExtCredits, result.ElementAt(i).InstExtCredits);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstTransciptDate, result.ElementAt(i).InstTransciptDate);
                }
            }

            [TestMethod]
            public async Task GetPersonExternalEducationCredentialsNonCacheAsync()
            {
                var resultTuple = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsAsync(offset, limit, null, "", "", "", true);
                var result = resultTuple.Item1;

                for (int i = 0; i < _personExternalEducationCredentialsCollection.Count(); i++)
                {
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).Id, result.ElementAt(i).Id);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadPersonId, result.ElementAt(i).AcadPersonId);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadInstitutionsId, result.ElementAt(i).AcadInstitutionsId);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadAcadProgram, result.ElementAt(i).AcadAcadProgram);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadAwards, result.ElementAt(i).AcadAwards);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadCcd, result.ElementAt(i).AcadCcd);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadCcdDate, result.ElementAt(i).AcadCcdDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadDegree, result.ElementAt(i).AcadDegree);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadDegreeDate, result.ElementAt(i).AcadDegreeDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadEndDate, result.ElementAt(i).AcadEndDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadGpa, result.ElementAt(i).AcadGpa);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadHonors, result.ElementAt(i).AcadHonors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadMajors, result.ElementAt(i).AcadMajors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadMinors, result.ElementAt(i).AcadMinors);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankDenominator, result.ElementAt(i).AcadRankDenominator);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankNumerator, result.ElementAt(i).AcadRankNumerator);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadRankPercent, result.ElementAt(i).AcadRankPercent);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadSpecialization, result.ElementAt(i).AcadSpecialization);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadStartDate, result.ElementAt(i).AcadStartDate);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadThesis, result.ElementAt(i).AcadThesis);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadComments, result.ElementAt(i).AcadComments);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).AcadNoYears, result.ElementAt(i).AcadNoYears);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstAttendGuid, result.ElementAt(i).InstAttendGuid);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstExtCredits, result.ElementAt(i).InstExtCredits);
                    Assert.AreEqual(_personExternalEducationCredentialsCollection.ElementAt(i).InstTransciptDate, result.ElementAt(i).InstTransciptDate);
                }
            }

            [TestMethod]
            public async Task GetPersonExternalEducationCredentialsByGuidCacheAsync()
            {
                foreach (var expected in _personExternalEducationCredentialsCollection)
                {
                    var dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { expected.Guid, new GuidLookupResult()
                            {
                                Entity = "ACAD.CREDENTIALS", PrimaryKey = expected.Id,
                                SecondaryKey = expected.Id
                            }
                        }
                    };
                    dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);

                    var result = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsByGuidAsync(expected.Guid);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Id, result.Id);
                    Assert.AreEqual(expected.AcadPersonId, result.AcadPersonId);
                    Assert.AreEqual(expected.AcadInstitutionsId, result.AcadInstitutionsId);
                    Assert.AreEqual(expected.AcadAcadProgram, result.AcadAcadProgram);
                    Assert.AreEqual(expected.AcadCcd, result.AcadCcd);
                    Assert.AreEqual(expected.AcadCcdDate, result.AcadCcdDate);
                    Assert.AreEqual(expected.AcadDegree, result.AcadDegree);
                    Assert.AreEqual(expected.AcadDegreeDate, result.AcadDegreeDate);
                    Assert.AreEqual(expected.AcadEndDate, result.AcadEndDate);
                    Assert.AreEqual(expected.AcadGpa, result.AcadGpa);
                    Assert.AreEqual(expected.AcadHonors, result.AcadHonors);
                    Assert.AreEqual(expected.AcadMajors, result.AcadMajors);
                    Assert.AreEqual(expected.AcadMinors, result.AcadMinors);
                    Assert.AreEqual(expected.AcadRankDenominator, result.AcadRankDenominator);
                    Assert.AreEqual(expected.AcadRankNumerator, result.AcadRankNumerator);
                    Assert.AreEqual(expected.AcadRankPercent, result.AcadRankPercent);
                    Assert.AreEqual(expected.AcadSpecialization, result.AcadSpecialization);
                    Assert.AreEqual(expected.AcadStartDate, result.AcadStartDate);
                    Assert.AreEqual(expected.AcadThesis, result.AcadThesis);
                    Assert.AreEqual(expected.AcadComments, result.AcadComments);
                    Assert.AreEqual(expected.AcadNoYears, result.AcadNoYears);
                    Assert.AreEqual(expected.InstAttendGuid, result.InstAttendGuid);
                    Assert.AreEqual(expected.InstExtCredits, result.InstExtCredits);
                    Assert.AreEqual(expected.InstTransciptDate, result.InstTransciptDate);
                }
            }

            [TestMethod]
            public async Task GetPersonExternalEducationCredentialsByGuidNonCacheAsync()
            {
                foreach (var expected in _personExternalEducationCredentialsCollection)
                {
                    var dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { expected.Guid, new GuidLookupResult()
                            {
                                Entity = "ACAD.CREDENTIALS", PrimaryKey = expected.Id,
                                SecondaryKey = expected.Id
                            }
                        }
                    };
                    dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);

                    var result = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsByGuidAsync(expected.Guid);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Id, result.Id);
                    Assert.AreEqual(expected.AcadPersonId, result.AcadPersonId);
                    Assert.AreEqual(expected.AcadInstitutionsId, result.AcadInstitutionsId);
                    Assert.AreEqual(expected.AcadAcadProgram, result.AcadAcadProgram);
                    Assert.AreEqual(expected.AcadCcd, result.AcadCcd);
                    Assert.AreEqual(expected.AcadCcdDate, result.AcadCcdDate);
                    Assert.AreEqual(expected.AcadDegree, result.AcadDegree);
                    Assert.AreEqual(expected.AcadDegreeDate, result.AcadDegreeDate);
                    Assert.AreEqual(expected.AcadEndDate, result.AcadEndDate);
                    Assert.AreEqual(expected.AcadGpa, result.AcadGpa);
                    Assert.AreEqual(expected.AcadHonors, result.AcadHonors);
                    Assert.AreEqual(expected.AcadMajors, result.AcadMajors);
                    Assert.AreEqual(expected.AcadMinors, result.AcadMinors);
                    Assert.AreEqual(expected.AcadRankDenominator, result.AcadRankDenominator);
                    Assert.AreEqual(expected.AcadRankNumerator, result.AcadRankNumerator);
                    Assert.AreEqual(expected.AcadRankPercent, result.AcadRankPercent);
                    Assert.AreEqual(expected.AcadSpecialization, result.AcadSpecialization);
                    Assert.AreEqual(expected.AcadStartDate, result.AcadStartDate);
                    Assert.AreEqual(expected.AcadThesis, result.AcadThesis);
                    Assert.AreEqual(expected.AcadComments, result.AcadComments);
                    Assert.AreEqual(expected.AcadNoYears, result.AcadNoYears);
                    Assert.AreEqual(expected.InstAttendGuid, result.InstAttendGuid);
                    Assert.AreEqual(expected.InstExtCredits, result.InstExtCredits);
                    Assert.AreEqual(expected.InstTransciptDate, result.InstTransciptDate);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonExternalEducationCredentialsByGuid_KeyNotFoundException()
            {
                var expected = _personExternalEducationCredentialsCollection.FirstOrDefault();
                var result = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsByGuidAsync(Guid.NewGuid().ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonExternalCredentialsByGuid_KeyNotFoundException2()
            {
                var expected = _personExternalEducationCredentialsCollection.FirstOrDefault();
                var instAttendKey = string.Concat(expected.AcadPersonId, "*", expected.AcadInstitutionsId);
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.InstitutionsAttend>(instAttendKey, true)).ReturnsAsync(() => null);
                var result = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsByGuidAsync(expected.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonExternalCredentialsByGuid_RepositoryException()
            {
                var expected = _personExternalEducationCredentialsCollection.FirstOrDefault();
                var dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { expected.Guid, new GuidLookupResult()
                            {
                                Entity = "ACAD.CREDENTIALS", PrimaryKey = expected.Id,
                                SecondaryKey = string.Empty
                            }
                        }
                    };
                dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                try
                {
                    var result = await personExternalEducationCredentialsRepo.GetExternalEducationCredentialsByGuidAsync(expected.Guid);
                }
                catch (RepositoryException ex)
                {
                    Assert.AreEqual(ex.Errors.FirstOrDefault().Message, string.Format("The GUID specified: {0} is used by a different entity/secondary key: ACAD.CREDENTIALS", expected.Guid));
                    throw ex;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateExternalEducationCredentialsAsync_ArgumentNullException()
            {
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateExternalEducationCredentialsAsync_RepositoryException()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();

                var response = new UpdateAcadCredentialsResponse()
                {
                    UpdateAcadCredentialsErrors = new List<UpdateAcadCredentialsErrors>()
                    {
                        new UpdateAcadCredentialsErrors() { ErrorMessages = "ERROR.MESSAGE", ErrorCodes = "ERROR.CODE" }
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateExternalEducationCredentialsAsync_ArgumentNullException_When_Id_Is_NullOrEmpty_On_Get()
            {
                var response = new UpdateAcadCredentialsResponse() { Guid = "" };
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();


                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateExternalEducationCredentialsAsync_KeyNotFoundException()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();

                // var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
                dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new KeyNotFoundException());

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateExternalEducationCredentialsAsync_KeyNotFoundException_When_AcadCred_NotFound()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();

               
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.AcadCredentials>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            public async Task CreateExternalEducationCredentialsAsync()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();


                //dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                //  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ACAD.CREDENTIALS", PrimaryKey = "KEY" } } }));

                //dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.AcadCredentials>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);
                foreach (var expected in _personExternalEducationCredentialsCollection)
                {
                    var dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { expected.Guid, new GuidLookupResult()
                            {
                                Entity = "ACAD.CREDENTIALS", PrimaryKey = expected.Id,
                                SecondaryKey = expected.Id
                            }
                        }
                    };
                    dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                }

                foreach (var entity in entityCollection)
                {
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<AcadCredentials>(entity.Recordkey, true)).ReturnsAsync(entity);
                }

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.CreateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);

                Assert.IsNotNull(result);
            }

            #region Update
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateExternalEducationCredentialsAsync_ArgumentNullException()
            {
                var result = await personExternalEducationCredentialsRepo.UpdateExternalEducationCredentialsAsync(null);
            }

       
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateExternalEducationCredentialsAsync_KeyNotFoundException_When_Id_Is_NullOrEmpty_On_Get()
            {
                var response = new UpdateAcadCredentialsResponse() { Guid = "" };
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();


                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.UpdateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateExternalEducationCredentialsAsync_KeyNotFoundException()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();

                // var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
                dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new KeyNotFoundException());

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.UpdateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateExternalEducationCredentialsAsync_KeyNotFoundException_When_AcadCred_NotFound()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();


                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.AcadCredentials>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.UpdateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);
            }

            [TestMethod]
            public async Task UpdateExternalEducationCredentialsAsync()
            {
                var personExternalEducationCredentialEntity = _personExternalEducationCredentialsCollection.FirstOrDefault();

               foreach (var expected in _personExternalEducationCredentialsCollection)
                {
                    var dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { expected.Guid, new GuidLookupResult()
                            {
                                Entity = "ACAD.CREDENTIALS", PrimaryKey = expected.Id,
                                SecondaryKey = expected.Id
                            }
                        }
                    };
                    dataAccessorMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                }

                foreach (var entity in entityCollection)
                {
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<AcadCredentials>(entity.Recordkey, true)).ReturnsAsync(entity);
                }

                var response = new UpdateAcadCredentialsResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(It.IsAny<UpdateAcadCredentialsRequest>())).ReturnsAsync(response);
                var result = await personExternalEducationCredentialsRepo.UpdateExternalEducationCredentialsAsync(personExternalEducationCredentialEntity);

                Assert.IsNotNull(result);
            }
            #endregion

            private PersonExternalEducationCredentialsRepository BuildValidPersonExternalEducationCredentials()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Setup response to PersonExternalEducationCredentials read
                entityCollection = new Collection<AcadCredentials>(_personExternalEducationCredentialsCollection.Select(record =>
                    new AcadCredentials()
                    {
                        Recordkey = record.Id,
                        RecordGuid = record.Guid,
                        AcadPersonId = record.AcadPersonId,
                        AcadInstitutionsId = record.AcadInstitutionsId,
                        AcadAcadProgram = record.AcadAcadProgram,
                        AcadAwards = record.AcadAwards,
                        AcadCcd = record.AcadCcd,
                        AcadCcdDate = record.AcadCcdDate,
                        AcadDegree = record.AcadDegree,
                        AcadDegreeDate = record.AcadDegreeDate,
                        AcadEndDate = record.AcadEndDate,
                        AcadGpa = record.AcadGpa,
                        AcadHonors = record.AcadHonors,
                        AcadMajors = record.AcadMajors,
                        AcadMinors = record.AcadMinors,
                        AcadRankDenominator = record.AcadRankDenominator,
                        AcadRankNumerator = record.AcadRankNumerator,
                        AcadRankPercent = record.AcadRankPercent,
                        AcadSpecialization = record.AcadSpecialization,
                        AcadStartDate = record.AcadStartDate,
                        AcadThesis = record.AcadThesis,
                        AcadComments = record.AcadComments,
                        AcadNoYears = record.AcadNoYears,
                        AcadCommencementDate = record.AcadCommencementDate
                        
                    }).ToList());

                // Setup response to read for InstitutionsAttend records
                var instAttendEntityCollection = new Collection<DataContracts.InstitutionsAttend>(_personExternalEducationCredentialsCollection.Select(record =>
                   new DataContracts.InstitutionsAttend()
                   {
                       RecordGuid = record.InstAttendGuid,
                       Recordkey = string.Concat(record.AcadPersonId, "*", record.AcadInstitutionsId),
                       InstaAcadCredentials = new List<string>() { record.Id },
                       InstaExtCredits = record.InstExtCredits,
                       InstaTranscriptDate = record.InstTransciptDate

                   }).ToList());

                dataAccessorMock.Setup(ac => ac.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>()))
					.ReturnsAsync(new string[] { "1", "2", "3" });

                //var externalEducationData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", subList);
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.AcadCredentials>()
                { 
                    BulkRecordsRead = entityCollection 
                };
                dataAccessorMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", It.IsAny<string[]>(), true))
                    .ReturnsAsync(entityCollection);
                foreach (var entity in entityCollection)
                {
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<AcadCredentials>(entity.Recordkey, true)).ReturnsAsync(entity);
                }

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.InstitutionsAttend>("INSTITUTIONS.ATTEND", It.IsAny<string[]>(), true))
                    .ReturnsAsync(instAttendEntityCollection);
                foreach (var entity in instAttendEntityCollection)
                {
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.InstitutionsAttend>(entity.Recordkey, true)).ReturnsAsync(entity);
                }
               

                var defaults = new Defaults()
                {
                    DefaultHostCorpId = "000043"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS", true)).ReturnsAsync(defaults);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>
                    {
                        {
                            string.Join("+", new string[] { "ACAD.CREDENTIALS", "1" }),
                            new RecordKeyLookupResult() { Guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc" }
                        },
                        {
                            string.Join("+", new string[] { "ACAD.CREDENTIALS", "2" }),
                            new RecordKeyLookupResult() { Guid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d" }
                        },
                        {
                            string.Join("+", new string[] { "ACAD.CREDENTIALS", "3" }),
                            new RecordKeyLookupResult() { Guid = "d2253ac7-9931-4560-b42f-1fccd43c952e" }
                        }
                    };
                    return Task.FromResult(result);
                });

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "PersonExternalEducationCredentialsKeys",
                    Entity = "ACAD.CREDENTIALS",
                    Sublist = new List<string> { "1", "2", "3" },
                    TotalCount = 3,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);


                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                personExternalEducationCredentialsRepo = new PersonExternalEducationCredentialsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return personExternalEducationCredentialsRepo;
            }
        }
    }
}
