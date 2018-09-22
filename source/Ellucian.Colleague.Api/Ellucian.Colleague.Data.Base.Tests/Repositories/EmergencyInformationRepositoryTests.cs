// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Data.Colleague;
using System.Runtime.Caching;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Colleague.Data.Base.Repositories;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
        
    [TestClass]
    public class EmergencyInformationRepositoryTests
    {

        [TestClass]
        public class EmergencyInformationRepository_Get
        {



            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            EmergencyInformationRepository emergencyInfoRepo;
            ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("null");
                emergencyInfoRepo = BuildEmergencyInformationRepository();
            }



            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPersonEmerInfo_ThrowsExceptionForNullPersonId()
            {
                // Arrange (set up the data).
                // No need to mock because we are testing a null record key.

                // Act (call the method we want to test).
                var emerInfo = emergencyInfoRepo.Get(null);

                // Assert (check that the results are what we expect).
                // Don't need an assert because we expect an exception noted above.
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetPersonEmerInfo_ThrowsExceptionForMissingPersonId()
            {
                // Arrange (set up the data).
                // No need to mock because we are testing a missing (empty) record key.

                // Act (call the method we want to test).
                var emerInfo = emergencyInfoRepo.Get("");

                // Assert (check that the results are what we expect).
                // Don't need an assert because we expect an exception noted above.
            }

            [TestMethod]
            public void GetEmptyPersonEmerInfo()
            {
                // Arrange (set up the data).
                // Mocked to test an empty response. 
                dataAccessorMock.Setup<PersonEmer>(acc => acc.ReadRecord<PersonEmer>("0100000", false)).Returns(new PersonEmer());

                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.Get("0100000");

                // Assert (check that the results are what we expect).
                Assert.IsTrue(emerInfoEntity != null);
                Assert.IsTrue(emerInfoEntity is EmergencyInformation);
                Assert.AreEqual("0100000", emerInfoEntity.PersonId);
            }

            [TestMethod]
            public void GetNullPersonEmerInfo()
            {
                // Arrange (set up the data).
                // Mocked to test a null response.
                PersonEmer nullPersonEmer = null;
                dataAccessorMock.Setup<PersonEmer>(acc => acc.ReadRecord<PersonEmer>("0100000", false)).Returns(nullPersonEmer);

                // Act (call the method we want to test).
                var emerInfo = emergencyInfoRepo.Get("0100000");

                // Assert (check that the results are what we expect).
                Assert.IsTrue(emerInfo != null);
                Assert.IsTrue(emerInfo is EmergencyInformation);
                Assert.AreEqual("0100000", emerInfo.PersonId);
            }


            [TestMethod]
            public void GetFullPersonEmerInfo()
            {
                // Arrange (set up the data).
                // Mocked to test a full and complete response.
                PersonEmer fullPersonEmer = new PersonEmer()
                {
                    EmerLastConfirmedDate = new DateTime(2014, 08, 04),
                    EmerContactsEntityAssociation = new List<PersonEmerEmerContacts>()
                {
                    // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                    new PersonEmerEmerContacts ("John Q. Public", "Day: 703-123-1234", "Eve: 540-234-5678", "Other: 703-098-7654", "Relation is Mama", new DateTime (2014,06,26), "N", "Y", "999 Nicewood Dr., Hometown, VA"),
                    new PersonEmerEmerContacts ("Mary G. Smithy", "Day: 703-123-3456", "Eve: 540-234-1234", "Other: 703-098-6789", "Relation is Papa", new DateTime (2014,06,27), "Y", "N", " "),
                    new PersonEmerEmerContacts ("Mrs. Empty", "", "", "", "", null, "", "", ""),
                    new PersonEmerEmerContacts ("Mr. Null", null, null, null, null, null, null, null, null)
                },
                    EmerHealthConditions = new List<string>()
                {
                    // Sample values for the health conditions validation code table. 
                    "DI",
                    "AL"
                },
                    EmerHospitalPref = "Fairfax Hospital",
                    EmerInsuranceInfo = "Cigna " + Convert.ToChar(DynamicArray.VM) + " ID 3456789",
                    EmerAddnlInformation = "Allergic to peanuts and penicillin." + Convert.ToChar(DynamicArray.VM) + "Epipen is always in my purse in a bright yellow container."

                };


                dataAccessorMock.Setup<PersonEmer>(acc => acc.ReadRecord<PersonEmer>("0100000", false)).Returns(fullPersonEmer);

                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.Get("0100000");

                // Assert (check that the results are what we expect). 
                // fullPersonEmer is the response from Colleague, whereas emerInfoEntity is the domain entity we mocked.
                Assert.IsTrue(emerInfoEntity != null);
                Assert.IsTrue(emerInfoEntity is EmergencyInformation);
                Assert.AreEqual("0100000", emerInfoEntity.PersonId);
                Assert.AreEqual(fullPersonEmer.EmerHospitalPref, emerInfoEntity.HospitalPreference);
                Assert.AreEqual(fullPersonEmer.EmerInsuranceInfo.Replace(Convert.ToChar(DynamicArray.VM), '\n'), emerInfoEntity.InsuranceInformation);
                Assert.AreEqual(fullPersonEmer.EmerAddnlInformation.Replace(Convert.ToChar(DynamicArray.VM), '\n'), emerInfoEntity.AdditionalInformation);
                Assert.AreEqual(fullPersonEmer.EmerLastConfirmedDate, emerInfoEntity.ConfirmedDate);

                for (int i = 0; i < fullPersonEmer.EmerContactsEntityAssociation.Count(); i++)
                {
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerNameAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Name);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerDaytimePhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).DaytimePhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEveningPhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).EveningPhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerOtherPhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).OtherPhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerRelationshipAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Relationship);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerContactDateAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).EffectiveDate);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerContactAddressAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Address);

                    if (fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == "Y" ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == "" ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == string.Empty ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == null)
                    {
                        Assert.IsTrue(emerInfoEntity.EmergencyContacts.ElementAt(i).IsEmergencyContact);
                    }
                    else
                    {
                        Assert.IsFalse(emerInfoEntity.EmergencyContacts.ElementAt(i).IsEmergencyContact);
                    }

                    if (fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerMissingContactFlagAssocMember == "Y")
                    {
                        Assert.IsTrue(emerInfoEntity.EmergencyContacts.ElementAt(i).IsMissingPersonContact);
                    }
                    else
                    {
                        Assert.IsFalse(emerInfoEntity.EmergencyContacts.ElementAt(i).IsMissingPersonContact);
                    }

                }

            }





            [TestMethod]
            public void GetPersonEmerInfoWithInvalidHealthCondition()
            {
                // Arrange (set up the data).
                // Mocked to test a full and complete response with an invalid health condition code.
                PersonEmer fullPersonEmer = new PersonEmer()
                {
                    EmerLastConfirmedDate = new DateTime(2014, 08, 04),
                    EmerContactsEntityAssociation = new List<PersonEmerEmerContacts>()
                    {
                        // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                        new PersonEmerEmerContacts ("John Q. Public", "Day: 703-123-1234", "Eve: 540-234-5678", "Other: 703-098-7654", "Relation is Mama", new DateTime (2014,06,26), "N", "Y", "999 Nicewood Dr., Hometown, VA"),
                        new PersonEmerEmerContacts ("Mary G. Smithy", "Day: 703-123-3456", "Eve: 540-234-1234", "Other: 703-098-6789", "Relation is Papa", new DateTime (2014,06,27), "Y", "N", " "),
                        new PersonEmerEmerContacts ("Mrs. Empty", "", "", "", "", null, "", "", ""),
                        new PersonEmerEmerContacts ("Mr. Null", null, null, null, null, null, null, null, null)
                    },
                    EmerHealthConditions = new List<string>()
                    {
                        // Sample values for the health conditions validation code table. XX is an invalid code.
                        "DI",
                        "XX"
                    },
                    EmerHospitalPref = "Fairfax Hospital",
                    EmerInsuranceInfo = "Cigna " + Convert.ToChar(DynamicArray.VM) + " ID 3456789",
                    EmerAddnlInformation = "Allergic to peanuts and penicillin." + Convert.ToChar(DynamicArray.VM) + "Epipen is always in my purse in a bright yellow container."
                };

                dataAccessorMock.Setup<PersonEmer>(acc => acc.ReadRecord<PersonEmer>("0100000", false)).Returns(fullPersonEmer);

                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.Get("0100000");

                // Assert (check that the results are what we expect). 
                // An invalid health condition on a "get" is just being logged (no error is thrown). 
                // No need to check the log here, just assert that the bad health conditioncode didn't 
                // come through. Do this by counting the health conditions to be sure we only got the 
                // one valid code, and also make sure that the one we get is the correct one.
                int numberOfHealthConditions = emerInfoEntity.HealthConditions.Count;
                Assert.AreEqual(1, numberOfHealthConditions);
                Assert.AreEqual("DI", emerInfoEntity.HealthConditions[0]);
            }



            // Set up for testing.
            private EmergencyInformationRepository BuildEmergencyInformationRepository()
            {

                // Transaction factory mock.
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock.
                localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock.
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking (health Conditions mock).
                ApplValcodes healthConditionsResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() 
                   {
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "DI"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "AS"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "EP"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "AL"}
                   }
                };
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "HEALTH.CONDITIONS", true)).Returns(healthConditionsResponse);


                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Feed my pretend repository the necessary tools. This will be called with the get. Does not actually contain the pretend DB record.
                emergencyInfoRepo = new EmergencyInformationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return emergencyInfoRepo;
            }
        }

        [TestClass]
        public class EmergencyInformationRepository_Update
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<IColleagueTransactionInvoker> mockManager;
            Mock<ILogger> loggerMock;
            EmergencyInformationRepository emergencyInfoRepo;
            UpdateEmergencyInformationRequest updateRequest;
            ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("null");
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Mock a response from the Colleague transaction.
                // Set up transaction manager for mocking (needed for update)
                mockManager = new Mock<IColleagueTransactionInvoker>();

                emergencyInfoRepo = BuildEmergencyInformationRepository();
                updateRequest = new UpdateEmergencyInformationRequest();
            }



            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;

            }




            [TestMethod]
            public void UpdateFullPersonEmerInfo()
            {
                // Arrange (set up the data).
                // Mocked to test a full and complete entity. (Uses domain constructors.)
                EmergencyInformation fullEmergencyInformationEntity = new EmergencyInformation("0100000")
                {
                    ConfirmedDate = new DateTime(2014, 08, 04),
                    HospitalPreference = "Fairfax Hospital",
                    InsuranceInformation = "Cigna \n ID 3456789",
                    AdditionalInformation = "Highly allergic to bee stings! \n Use Epipen immediately!"
                };


                // Sample values for the health conditions validation code table.
                fullEmergencyInformationEntity.AddHealthCondition("DI");
                fullEmergencyInformationEntity.AddHealthCondition("AL");

                fullEmergencyInformationEntity.AddEmergencyContact(
                    
                        // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                        new EmergencyContact ("John Q. Public")
                        {
                            DaytimePhone = "Day: 703-123-3456", 
                            EveningPhone = "Eve: 540-234-1234", 
                            OtherPhone = "Other: 703-098-6789", 
                            Relationship = "Relation is Papa", 
                            EffectiveDate = new DateTime (2014,06,27), 
                            IsEmergencyContact = true,
                            IsMissingPersonContact = false,
                            Address = "Papa's address"
                        });

                fullEmergencyInformationEntity.AddEmergencyContact(
                        new EmergencyContact ("Mary Q. Public")
                        {
                            DaytimePhone = "Day: 703-234-5678", 
                            EveningPhone = "EveHome 540-234-1234", 
                            OtherPhone = "Other: 703-345-6789", 
                            Relationship = "Relation is Mama", 
                            EffectiveDate = new DateTime (2014,06,27), 
                            IsEmergencyContact = false,
                            IsMissingPersonContact = true,
                            Address = "Mama's address"
                        });




                // Mock the response and the callback to get the request.
                UpdateEmergencyInformationResponse updateEmerInfoResponse = new UpdateEmergencyInformationResponse();

                
                mockManager.Setup(mgr => mgr.Execute<UpdateEmergencyInformationRequest, UpdateEmergencyInformationResponse>(It.IsAny<UpdateEmergencyInformationRequest>())).Returns(updateEmerInfoResponse).Callback<UpdateEmergencyInformationRequest>(req => updateRequest = req);


                // Mock the get of the data after update. Use different data so that it is clear which data
                // is before and which is after.
                PersonEmer fullPersonEmer = new PersonEmer()
                {
                    EmerLastConfirmedDate = new DateTime(2014, 10, 27),
                    EmerContactsEntityAssociation = new List<PersonEmerEmerContacts>()
                {
                    // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                    new PersonEmerEmerContacts ("John Q. Public", "Day: 703-123-1234", "Eve: 540-234-5678", "Other: 703-098-7654", "Relation is Mama", new DateTime (2014,06,26), "N", "Y", "999 Nicewood Dr., Hometown, VA"),
                },
                    EmerHealthConditions = new List<string>()
                {
                    // Sample value for the health conditions validation code table. 
                    "AL"
                },
                    EmerHospitalPref = "Ridgeway Hospital",
                    EmerInsuranceInfo = "BCBS ID 1234567",
                    EmerAddnlInformation = "Latex allergy"

                };

                dataAccessorMock.Setup<PersonEmer>(acc => acc.ReadRecord<PersonEmer>("0100000", false)).Returns(fullPersonEmer);



                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.UpdateEmergencyInformation(fullEmergencyInformationEntity);



                // Assert (check that the results are what we expect). 
                // Compare the request that would be sent to the Colleague Transaction (updateRequest) with the mocked entity (fullEmergencyInformationEntity).
                Assert.AreEqual(fullEmergencyInformationEntity.PersonId, updateRequest.PersonId);
                Assert.AreEqual(fullEmergencyInformationEntity.ConfirmedDate, updateRequest.LastConfirmedDate);
                Assert.AreEqual(fullEmergencyInformationEntity.HospitalPreference, updateRequest.HospitalPreference);

                // The insurance information might have new line characters (\n). If so, the repository splits those into separate
                // lines. We need to do the same here or the comparison will fail.
                var insuranceLines = fullEmergencyInformationEntity.InsuranceInformation.Split('\n');
                for (int i = 0; i < insuranceLines.Count(); i++)
                {
                    Assert.AreEqual(insuranceLines.ElementAt(i), updateRequest.InsuranceInformation.ElementAt(i));
                }


                // The additional emergency information might have new line characters (\n). If so, the repository splits 
                // those into separate lines. We need to do the same here or the comparison will fail.
                var additionalInformationLines = fullEmergencyInformationEntity.AdditionalInformation.Split('\n');
                for (int i = 0; i < additionalInformationLines.Count(); i++)
                {
                    Assert.AreEqual(additionalInformationLines.ElementAt(i), updateRequest.AdditionalInformation.ElementAt(i));
                }


                for (int i = 0; i < fullEmergencyInformationEntity.HealthConditions.Count(); i++)
                {
                    // For each health condition listed in the mocked entity, verify that it matches the health condition
                    // listed in the same position of the request.
                    Assert.AreEqual(fullEmergencyInformationEntity.HealthConditions.ElementAt(i), updateRequest.HealthConditions.ElementAt(i));
                }

                for (int i = 0; i < fullEmergencyInformationEntity.EmergencyContacts.Count(); i++)
                {
                    // For each emergency contact object in the mocked entity, verify that all the attributes match the
                    // same object in the request.
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).Name, updateRequest.EmergencyContactName.ElementAt(i));
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).DaytimePhone, updateRequest.DaytimePhones.ElementAt(i));
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).EveningPhone, updateRequest.EveningPhone.ElementAt(i));
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).OtherPhone, updateRequest.OtherPhones.ElementAt(i));
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).Relationship, updateRequest.ContactRelationships.ElementAt(i)); 
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).EffectiveDate, updateRequest.ContactEffectiveDate.ElementAt(i));
                    Assert.AreEqual(fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).Address, updateRequest.ContactAddresses.ElementAt(i));


                    // True/false    Y/N
                    // The 2 flags in fullEmergencyInformationEntity.EmergencyContacts are boolean (true/false), but in the request (updateRequest) they
                    // are Y/N because that's how they are stored in the database.
                    if (fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).IsEmergencyContact)
                    {
                        Assert.AreEqual("Y", updateRequest.EmergencyContactFlags.ElementAt(i));
                    }
                    else
                    {
                        Assert.AreEqual("N", updateRequest.EmergencyContactFlags.ElementAt(i));
                    }

                    if (fullEmergencyInformationEntity.EmergencyContacts.ElementAt(i).IsMissingPersonContact)
                    {
                        Assert.AreEqual("Y", updateRequest.MissingContactFlags.ElementAt(i));
                    }
                    else
                    {
                        Assert.AreEqual("N", updateRequest.MissingContactFlags.ElementAt(i));
                    }

                }


                // Assert that the "get" at the end of the update method (emerInfoEntity) returns the expected data (that I had
                // mocked up in fullPersonEmer).
                Assert.IsTrue(emerInfoEntity != null);
                Assert.IsTrue(emerInfoEntity is EmergencyInformation);
                Assert.AreEqual("0100000", emerInfoEntity.PersonId);
                Assert.AreEqual(fullPersonEmer.EmerHospitalPref, emerInfoEntity.HospitalPreference);
                Assert.AreEqual(fullPersonEmer.EmerInsuranceInfo.Replace(Convert.ToChar(DynamicArray.VM), '\n'), emerInfoEntity.InsuranceInformation);
                Assert.AreEqual(fullPersonEmer.EmerAddnlInformation.Replace(Convert.ToChar(DynamicArray.VM), '\n'), emerInfoEntity.AdditionalInformation);
                Assert.AreEqual(fullPersonEmer.EmerLastConfirmedDate, emerInfoEntity.ConfirmedDate);

                int numberOfHealthConditionsA = emerInfoEntity.HealthConditions.Count;
                int numberOfHealthConditionsB = fullPersonEmer.EmerHealthConditions.Count;
                Assert.AreEqual(numberOfHealthConditionsA, numberOfHealthConditionsB);
                for (int i = 0; i < fullPersonEmer.EmerHealthConditions.Count(); i++)
                {
                    // For each health condition listed in the mock, verify that it matches the health condition
                    // listed in the same position of the request.
                    Assert.AreEqual(fullPersonEmer.EmerHealthConditions.ElementAt(i), emerInfoEntity.HealthConditions.ElementAt(i));
                }


                for (int i = 0; i < fullPersonEmer.EmerContactsEntityAssociation.Count(); i++)
                {
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerNameAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Name);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerDaytimePhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).DaytimePhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEveningPhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).EveningPhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerOtherPhoneAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).OtherPhone);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerRelationshipAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Relationship);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerContactDateAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).EffectiveDate);
                    Assert.AreEqual(fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerContactAddressAssocMember, emerInfoEntity.EmergencyContacts.ElementAt(i).Address);

                    if (fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == "Y" ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == "" ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == string.Empty ||
                        fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerEmergencyContactFlagAssocMember == null)
                    {
                        Assert.IsTrue(emerInfoEntity.EmergencyContacts.ElementAt(i).IsEmergencyContact);
                    }
                    else
                    {
                        Assert.IsFalse(emerInfoEntity.EmergencyContacts.ElementAt(i).IsEmergencyContact);
                    }

                    if (fullPersonEmer.EmerContactsEntityAssociation.ElementAt(i).EmerMissingContactFlagAssocMember == "Y")
                    {
                        Assert.IsTrue(emerInfoEntity.EmergencyContacts.ElementAt(i).IsMissingPersonContact);
                    }
                    else
                    {
                        Assert.IsFalse(emerInfoEntity.EmergencyContacts.ElementAt(i).IsMissingPersonContact);
                    }

                }

            }






            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void UpdatePersonEmerInfoWithColleagueTransactionErrors()
            {
                // Arrange (set up the data).
                // Mocked to test a full and complete entity. (Uses domain constructors.)
                EmergencyInformation fullEmergencyInformationEntity = new EmergencyInformation("01000000")
                {
                    ConfirmedDate = new DateTime(2014, 08, 04),
                    HospitalPreference = "Fairfax Hospital",
                    InsuranceInformation = "Cigna \n ID 3456789",
                    AdditionalInformation = "Highly allergic to bee stings! \n Use Epipen immediately!"
                };

                // Sample values for the health conditions validation code table.
                fullEmergencyInformationEntity.AddHealthCondition("DI");
                fullEmergencyInformationEntity.AddHealthCondition("AL");

                fullEmergencyInformationEntity.AddEmergencyContact(
                        // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                        new EmergencyContact ("John Q. Public")
                        {
                            DaytimePhone = "Day: 703-123-3456", 
                            EveningPhone = "Eve: 540-234-1234", 
                            OtherPhone = "Other: 703-098-6789", 
                            Relationship = "Relation is Papa", 
                            EffectiveDate = new DateTime (2014,06,27), 
                            IsEmergencyContact = true,
                            IsMissingPersonContact = false,
                            Address = "Papa's address"
                        });

                fullEmergencyInformationEntity.AddEmergencyContact(
                        new EmergencyContact ("Mary Q. Public")
                        {
                            DaytimePhone = "Day: 703-234-5678", 
                            EveningPhone = "EveHome 540-234-1234", 
                            OtherPhone = "Other: 703-345-6789", 
                            Relationship = "Relation is Mama", 
                            EffectiveDate = new DateTime (2014,06,27), 
                            IsEmergencyContact = false,
                            IsMissingPersonContact = true,
                            Address = "Mama's address"
                        });


                // Mock the response and the callback to get the request.
                UpdateEmergencyInformationResponse updateEmerInfoResponse = new UpdateEmergencyInformationResponse();
                
                // Dummy up some Colleague transaction errors.
                updateEmerInfoResponse.ErrorMessages = new List<string>() { "CTX error message 1", "Error message 2" };
                
                mockManager.Setup(mgr => mgr.Execute<UpdateEmergencyInformationRequest, UpdateEmergencyInformationResponse>(It.IsAny<UpdateEmergencyInformationRequest>())).Returns(updateEmerInfoResponse).Callback<UpdateEmergencyInformationRequest>(req => updateRequest = req);

                

                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.UpdateEmergencyInformation(fullEmergencyInformationEntity);



                // Assert (check that the results are what we expect). 
                // No asserts because this throws an exception.
            }



            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void UpdatePersonEmerInfoWithInvalidHealthCondition()
            {
                // Arrange (set up the data).
                // Mocked to test a full and complete entity. (Uses domain constructors.)
                EmergencyInformation fullEmergencyInformationEntity = new EmergencyInformation("01000000")
                {
                    ConfirmedDate = new DateTime(2014, 08, 04),
                    HospitalPreference = "Fairfax Hospital",
                    InsuranceInformation = "Cigna \n ID 3456789",
                    AdditionalInformation = "Highly allergic to bee stings! \n Use Epipen immediately!"
                };

                // Sample values for the health conditions validation code table. XX is an invalid code.
                fullEmergencyInformationEntity.AddHealthCondition("DI");
                fullEmergencyInformationEntity.AddHealthCondition("XX");

                fullEmergencyInformationEntity.AddEmergencyContact(

                        // Note: The database will contain Y/N/null for the flags, but the repository code changes those to boolean true/false. 
                        new EmergencyContact("John Q. Public")
                        {
                            DaytimePhone = "Day: 703-123-3456",
                            EveningPhone = "Eve: 540-234-1234",
                            OtherPhone = "Other: 703-098-6789",
                            Relationship = "Relation is Papa",
                            EffectiveDate = new DateTime(2014, 06, 27),
                            IsEmergencyContact = true,
                            IsMissingPersonContact = false,
                            Address = "Papa's address"
                        });

                fullEmergencyInformationEntity.AddEmergencyContact(
                        new EmergencyContact("Mary Q. Public")
                        {
                            DaytimePhone = "Day: 703-234-5678",
                            EveningPhone = "EveHome 540-234-1234",
                            OtherPhone = "Other: 703-345-6789",
                            Relationship = "Relation is Mama",
                            EffectiveDate = new DateTime(2014, 06, 27),
                            IsEmergencyContact = false,
                            IsMissingPersonContact = true,
                            Address = "Mama's address"
                        });
                
                // Mock the response and the callback to get the request.
                UpdateEmergencyInformationResponse updateEmerInfoResponse = new UpdateEmergencyInformationResponse();


                mockManager.Setup(mgr => mgr.Execute<UpdateEmergencyInformationRequest, UpdateEmergencyInformationResponse>(It.IsAny<UpdateEmergencyInformationRequest>())).Returns(updateEmerInfoResponse).Callback<UpdateEmergencyInformationRequest>(req => updateRequest = req);

                // Act (call the method we want to test).
                var emerInfoEntity = emergencyInfoRepo.UpdateEmergencyInformation(fullEmergencyInformationEntity);
                // Assert (check that the results are what we expect). 
                // Don't need an assert because we expect an exception noted above.
            }




            // Set up for testing.
            private EmergencyInformationRepository BuildEmergencyInformationRepository()
            {

                // Transaction factory mock.
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock.
                localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock.
                cacheProviderMock = new Mock<ICacheProvider>();

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

                // Set up data accessor for mocking (health Conditions mock).
                ApplValcodes healthConditionsResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() 
                   {
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "DI"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "AS"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "EP"},
                       new ApplValcodesVals() {ValInternalCodeAssocMember = "AL"}
                   }
                };
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "HEALTH.CONDITIONS", true)).Returns(healthConditionsResponse);
              

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Feed my pretend repository the necessary tools. This will be called with the get. Does not actually contain the pretend DB record.
                emergencyInfoRepo = new EmergencyInformationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return emergencyInfoRepo;
            }

        }       
    }
}
