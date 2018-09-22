// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [System.Runtime.InteropServices.GuidAttribute("97525542-0362-4E6C-A463-ABA904EBBEE0")]
    public abstract class BasePersonSetup : BaseRepositorySetup
    {
        protected List<string> personIds;
        protected Dictionary<string, DataContracts.Person> personRecords;
        protected Dictionary<string, DataContracts.PersonIntg> personIntgRecords;
        protected Dictionary<string, DataContracts.ForeignPerson> foreignPersonRecords;
        protected Dictionary<string, TxGetHierarchyAddressResponse> preferredAddressResponses;
        protected Dictionary<string, GetPersonContactInfoResponse> getContactInfoResponses;
        protected int PersonCount = 0;
        Collection<DataContracts.Person> personResponseData;
        ApiSettings apiSettings;
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        #region Private data array setup

        private string[,] _personData = {
                                       {"0000001", "Washington", "George", "", "Gen.", "", "", "", "wash1@college.edu;wash2@college.edu", "FAC;PRI", "Geo", "111-11-1111", "M", "WH", "HIS", "M","c90fa3b9-c3e7-4055-b112-946adb7bc104", "1983-08-01", "2040-07-02", "Preferred Name Override", "", "ChosenGeorge", "", "ChosenWashington", "D"},
                                       {"0000002", "Adams", "John", "Peter", "Mr.", "", "Adams J.P.", "FAC", "adams@school.edu", "FAC", "Joe", "222-22-2222", "D", "AS", "NHS", "F", "fad5c1b3-f603-4d07-a825-d92847daf2c4", "1930-01-01", "1999-04-16", "II", "", "", "", "", "U"},
                                       {"0000003", "Jefferson", "Thomas", "", "Dr.", "", "", "", "", "", "Tom", "222-22-2222", "S", "HP", "NRA", "", "74bbe9c0-7dfc-41f1-a654-151bf6264721", "1921-09-24", "", "IM", "", "    CThomas", "", "", "A"},
                                       {"0000004", "Madison", "James", "Adam", "Sir", "", "", "", "", "", "", "", "", "", "", "", "196855e8-1653-4d7c-b389-062d229b72c5", "", "","IM", "", "", "", "CMadison", ""},
                                       {"0000005", "Monroe", "James", "William", "Lord", "", "", "", "", "", "", "", "", "", "", "", "c75c09f3-605f-4e38-9c78-b6c4f1836792", "", "","", "", "T", "C", "CMonroe", ""},
                                       {"0000006", "Adams", "John", "Quincy", "Hon.", "Jr.", "John Quincy Adams;Adams J.Q.;Adams Formatted Name", "PF;FAC;XYZ", "", "", "", "", "", "", "", "", "82b2033b-12e4-4b6f-a69e-206406a28823", "", "","", "", "", "", "", ""},
                                       {"9999999", "Test", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "44e46c94-3bcc-4e64-ab3d-c03044073e26", "", "", "", "", "", "", "", ""},
                                       {"9999998", "  Multi Part-Last   ", " Mary Beth  ", "Anne", "", "", "", "", "", "", "", "", "", "", "", "", "eb168d7a-1765-4982-8ce2-3efefc86d945", "", "", "", "", " CMary CBeth", "CAnne", " Chosen Last ", ""},
                                       {"9999997", "Test", "B", "", "", "", "", "", "", "", "", "", "", "", "", "", "7d586e40-ecc9-4040-b53c-1d4f5d48c60c", "", "", "", "", "", "", "", ""},
                                       {"9999996", "Test", "Carla", "M.", "", "", "", "", "", "", "", "", "", "", "", "", "6f45023b-9aa5-435c-9453-9b6514dfc1f2", "", "", "", "", "", "", "", ""},
                                       {"9999995", "Test", "Carla", "M", "", "", "", "", "", "", "", "", "", "", "", "", "0209c172-9a52-445f-8a4b-bedce3d6cdcb", "", "", "", "", "", "", "", ""},
                                       {"9999994", "Test    Smith", "Carla", "Marie", "", "", "", "", "", "", "", "", "", "", "", "", "e38bb327-026b-4ca8-8e23-cc99a47670e4", "", "", "", "", "", "", "", ""},
                                       {"9999993", "Restricted", "Person", "", "", "", "", "", "", "", "", "", "", "", "", "", "d463ae42-0235-4161-9d9f-3d0451f9fc9b", "", "", "", "S", "", "", "", ""}
                                   };

        private string[,] _addressData = {
                                       {"0000001", "123 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "", "", "Father of our Country"},
                                       {"0000002", "456 ", "", "235 Beacon Hill Dr.", "Boston", "MA", "03549", "", "", ""},
                                       {"0000003", "789 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", "", "", "FR", "France", "Ambassador to France"},
                                       {"0000004", "1357", "", "1812 Dolly Madison Dr.", "Arlington", "VA", "22146", "", "", ""},
                                       {"0000005", "2468", "", "1787 Constitution Ave.", "Franklin", "TN", "34567", "", "", ""},
                                       {"0000006", "3579", "", "1600 Pennsylvania Ave.;The White House", "Washington", "DC", "12345", "", "", "POTUS"},
                                       {"9999999", null, null, null, null, null, null, null, null, null},
                                       {"9999998", "", "", "", "", "", "", "", "", ""},
                                       {"9999997", "", "", "", "", "", "", "", "", ""},
                                       {"9999996", "", "", "", "", "", "", "", "", ""},
                                       {"9999995", "", "", "", "", "", "", "", "", ""},
                                       {"9999994", "", "", "", "", "", "", "", "", ""},
                                       {"9999993", "", "", "", "", "", "", "", "", ""}
                                   };

        private string[,] _emailData = {
                                       {"0000001", "PRI", "g.washington@whitehouse.gov"},
                                       {"0000002", "WWW", "j.adams@whitehouse.gov"},
                                       {"0000003", "BUS", "t.jefferson@whitehouse.gov"},
                                       {"0000004", "GOV", "j.madison@whitehouse.gov"},
                                       {"0000005", "WRK", "j.monroe@whitehouse.gov"},
                                       {"0000006", "", "j.q.adams@whitehouse.gov"},
                                       {"9999999", null, null},
                                       {"9999998", "", ""},
                                       {"9999997", "", ""},
                                       {"9999996", "", ""},
                                       {"9999995", "", ""},
                                       {"9999994", "", ""},
                                       {"9999993", "", ""}
                                   };

        private string[,] _socialMediaData = {
                                       {"0000001", "TWI", "g.washington"},
                                       {"0000002", "FB", "j.adams"},
                                       {"0000003", "YAH", "t.jefferson"},
                                       {"0000004", "AOL", "j.madison"},
                                       {"0000005", "MIC", "j.monroe"},
                                       {"0000006", "", "j.q.adams"},
                                       {"9999999", null, null},
                                       {"9999998", "", ""},
                                       {"9999997", "", ""},
                                       {"9999996", "", ""},
                                       {"9999995", "", ""},
                                       {"9999994", "", ""},
                                       {"9999993", "", ""}
                                   };

        #endregion

        protected void PersonSetupInitialize()
        {
            // Initialize the Mock framework
            MockInitialize();

            apiSettings = new ApiSettings("TEST");

            personRecords = SetupPersons(out personIds);
            personIntgRecords = SetupPersonIntg(out personIds);

            preferredAddressResponses = SetupPreferredAddressResponses();
            
            // Mock the calls for reading individual person records
            dataReaderMock.Setup<Task<DataContracts.Person>>(
                accessor => accessor.ReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string>(), true)
                ).Returns<string, string, bool>((file, id, vms) =>
                {
                    DataContracts.Person response = null;
                    return Task.FromResult((personRecords.TryGetValue(id, out response)) ? response : null);
                });

            dataReaderMock.Setup<Task<DataContracts.Person>>(
                accessor => accessor.ReadRecordAsync<DataContracts.Person>(It.IsAny<GuidLookup>(), true)
                ).Returns<GuidLookup, bool>((guidLookup, vms) =>
                {
                    DataContracts.Person response = null;
                    var personDataContract = personRecords.Values.Where(pr => pr.RecordGuid == guidLookup.Guid).FirstOrDefault();
                    if (personDataContract != null)
                    {
                        response = (personRecords.TryGetValue(personDataContract.Recordkey, out response)) ? response : null;
                    }
                    return Task.FromResult(response);
                });

            // var integrationPerson = await DataReader.ReadRecordAsync<DataContracts.PersonIntg>(record.Recordkey);
           dataReaderMock.Setup<Task<DataContracts.PersonIntg>>(
                accessor => accessor.ReadRecordAsync<DataContracts.PersonIntg>(It.IsAny<string>(), true)
                ).Returns<string, bool>((id, vms) =>
                {
                    DataContracts.PersonIntg response = null;
                    return Task.FromResult((personIntgRecords.TryGetValue(id, out response)) ? response : null);
                });

           // var integrationPersonRecords = await DataReader.BulkReadRecordAsync<DataContracts.PersonIntg>(personIds);
           dataReaderMock.Setup<Task<Collection<DataContracts.PersonIntg>>>(
               accessor => accessor.BulkReadRecordAsync<DataContracts.PersonIntg>(It.IsAny<string[]>(), true))
               .Returns<string[], bool>((ids, b) =>
               {
                   return Task.FromResult(new Collection<DataContracts.PersonIntg>(
                            personIntgRecords
                            .Where(p => ids.Contains(p.Key))
                            .Select(p =>
                            new Ellucian.Colleague.Data.Base.DataContracts.PersonIntg()
                            {
                                Recordkey = p.Key,
                                PerIntgBirthCountry = p.Value.PerIntgBirthCountry,
                                PerIntgDlExpireDate = p.Value.PerIntgDlExpireDate,
                                PerIntgEthnic = p.Value.PerIntgEthnic
                            }).ToList()));
               });         

            //Mock the call to read a batch of person records
            dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(
                accessor => accessor.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                {
                    return Task.FromResult(new Collection<DataContracts.Person>(
                             personRecords
                            .Where(p => ids.Contains(p.Key))
                            .Select(p =>
                            new Ellucian.Colleague.Data.Base.DataContracts.Person()
                            {
                                Recordkey = p.Key,
                                LastName = p.Value.LastName,
                                FirstName = p.Value.FirstName
                            }).ToList()));
                });

            // Mock the call for getting the preferred address
            transManagerMock.Setup<Task<TxGetHierarchyAddressResponse>>(
                manager => manager.ExecuteAsync<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                    It.IsAny<TxGetHierarchyAddressRequest>())
                ).Returns<TxGetHierarchyAddressRequest>(request =>
                {
                    TxGetHierarchyAddressResponse response = null;
                    return Task.FromResult((preferredAddressResponses.TryGetValue(request.IoPersonId, out response)) ? response : null);
                });


            // Mock the call for getting multiple person records
            personResponseData = BuildPersonResponseData(personRecords);
            dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true)).ReturnsAsync(personResponseData);

            dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<GuidLookup[]>(), true)).ReturnsAsync(personResponseData);

            // Mock the call for getting multiple address records - return none
            Collection<DataContracts.Address> addressResponseData = new Collection<DataContracts.Address>();
            dataReaderMock.Setup<Task<Collection<DataContracts.Address>>>(acc => acc.BulkReadRecordAsync<DataContracts.Address>(It.IsAny<string[]>(), true)).ReturnsAsync(addressResponseData);

            // Mock the call for creating/updating a personIntegration
            transManagerMock.Setup<Task<UpdatePersonIntegrationResponse>>(
                manager => manager.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(
                    It.IsAny<UpdatePersonIntegrationRequest>())
                ).Returns<UpdatePersonIntegrationRequest>(request => Task.FromResult(new UpdatePersonIntegrationResponse()
                {
                    PersonGuid = request.PersonGuid,
                    PersonIntgErrors = new List<PersonIntgErrors>()
                }));


            // Mock the call for creating/updating a personIntegration
            transManagerMock.Setup<Task<UpdateOrganizationIntegrationResponse>>(
                manager => manager.ExecuteAsync<UpdateOrganizationIntegrationRequest, UpdateOrganizationIntegrationResponse>(
                    It.IsAny<UpdateOrganizationIntegrationRequest>())
                ).Returns<UpdateOrganizationIntegrationRequest>(request => Task.FromResult(new UpdateOrganizationIntegrationResponse()
                {
                    OrgGuid = request.OrgGuid,
                    OrgIntgErrors = new List<OrgIntgErrors>()
                }));

            // Mock the call for determining whether to update a person
            dataReaderMock.Setup<Task<Dictionary<string, GuidLookupResult>>>(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())
                ).Returns<GuidLookup[]>(lookup =>
                {
                    var personGuid = lookup[0].Guid;
                    var personRecord = personRecords.Where(pr => pr.Value.RecordGuid == personGuid).FirstOrDefault();
                   
                    var result = new Dictionary<string, GuidLookupResult>();
                    result.Add(personGuid, new GuidLookupResult() { PrimaryKey = personRecord .Value.Recordkey, Entity = "PERSON"});
                    return Task.FromResult(result);
                });

            

            // mock data accessor PERSON.ETHNICS
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "W", "H" },
                    ValExternalRepresentation = new List<string>() { "White", "Hispanic" },
                    ValActionCode1 = new List<string>() { "W", "H" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "White",
                            ValActionCode1AssocMember = "W"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "H",
                            ValExternalRepresentationAssocMember = "Hispanic",
                            ValActionCode1AssocMember = "H"
                        }
                    }
                });

            // mock data accessor PERSON.RACES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "CA", "PI" },
                    ValExternalRepresentation = new List<string>() { "Caucasian", "Pacific Islander" },
                    ValActionCode1 = new List<string>() { "1", "2" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "CA",
                            ValExternalRepresentationAssocMember = "Caucasian",
                            ValActionCode1AssocMember = "1"
                        },
                         new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "PI",
                            ValExternalRepresentationAssocMember = "Pacific Islander",
                            ValActionCode1AssocMember = "2"
                        }
                    }
                });

            // mock data accessor MARITAL.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "S", "M", "D", "W" },
                    ValExternalRepresentation = new List<string>() { "Single", "Married", "Divorced", "Widowed" },
                    ValActionCode1 = new List<string>() { "1", "2", "3", "4" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "S",
                            ValExternalRepresentationAssocMember = "Single",
                            ValActionCode1AssocMember = "1"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "M",
                            ValExternalRepresentationAssocMember = "Married",
                            ValActionCode1AssocMember = "2"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "D",
                            ValExternalRepresentationAssocMember = "Divorced",
                            ValActionCode1AssocMember = "3"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "W",
                            ValExternalRepresentationAssocMember = "Widowed",
                            ValActionCode1AssocMember = "W"
                        }
                    }
                });

            // mock data accessor PERSONAL.STATUSES
            dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                a.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSONAL.STATUSES", true))
                .ReturnsAsync(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "A", "U", "D" },
                    ValExternalRepresentation = new List<string>() { "Active", "Unverified Deceased", "Deceased" },
                    ValActionCode1 = new List<string>() { "A", "D", "D" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "A",
                            ValExternalRepresentationAssocMember = "Active",
                            ValActionCode1AssocMember = "A"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "U",
                            ValExternalRepresentationAssocMember = "Unverified Deceased",
                            ValActionCode1AssocMember = "D"
                        },
                        new ApplValcodesVals()
                        {
                            ValInternalCodeAssocMember = "D",
                            ValExternalRepresentationAssocMember = "Deceased",
                            ValActionCode1AssocMember = "D"
                        }

                    }
                });

            // mock data reader for getting the Preferred Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "PREFERRED",
                    NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF"}
                });
        }

        private Dictionary<string, DataContracts.Person> SetupPersons(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _personData;

            PersonCount = recordData.Length / 25;
            Dictionary<string, DataContracts.Person> records = new Dictionary<string, DataContracts.Person>();
            for (int i = 0; i < PersonCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string last = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string first = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                string middle = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd();
                string prefix = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                string suffix = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                List<string> fnames = (recordData[i, 6] == null) ? new List<string>() : recordData[i, 6].TrimEnd().Split(';').ToList<string>();
                List<string> ftypes = (recordData[i, 7] == null) ? new List<string>() : recordData[i, 7].TrimEnd().Split(';').ToList<string>();
                List<string> eaddresses = (string.IsNullOrEmpty(recordData[i, 8])) ? new List<string>() : recordData[i, 8].TrimEnd().Split(';').ToList<string>();
                List<string> etypes = (string.IsNullOrEmpty(recordData[i, 9])) ? new List<string>() : recordData[i, 9].TrimEnd().Split(';').ToList<string>();
                string nickname = (recordData[i, 10] == null) ? null : recordData[i, 10].TrimEnd();
                string ssn = (recordData[i, 11] == null) ? null : recordData[i, 11].TrimEnd();
                string maritalStatus = (recordData[i, 12] == null) ? null : recordData[i, 12].TrimEnd();
                List<string> races = (recordData[i, 13] == null) ? new List<string>() : recordData[i, 13].TrimEnd().Split(';').ToList<string>();
                List<string> ethnics = (recordData[i, 14] == null) ? new List<string>() : recordData[i, 14].TrimEnd().Split(';').ToList<string>();
                string gender = (recordData[i, 15] == null) ? null : recordData[i, 15].TrimEnd();
                string guid = (recordData[i, 16] == null) ? null : recordData[i, 16].TrimEnd();
                string prefNameOverride = (recordData[i, 19] == null) ? null : recordData[i, 19].TrimEnd();
                DateTime? birthDate = null;
                if (!string.IsNullOrEmpty(recordData[i, 17])) birthDate = Convert.ToDateTime(recordData[i, 17].TrimEnd());
                DateTime? deceasedDate = null;
                if (!string.IsNullOrEmpty(recordData[i, 18])) deceasedDate = Convert.ToDateTime(recordData[i, 18].TrimEnd());
                string privacyFlag = (recordData[i, 20] == null) ? null : recordData[i, 20].TrimEnd();
                string chosenFirst = (recordData[i, 21] == null) ? null : recordData[i, 21].TrimEnd();
                string chosenMiddle = (recordData[i, 22] == null) ? null : recordData[i, 22].TrimEnd();
                string chosenLast = (recordData[i, 23] == null) ? null : recordData[i, 23].TrimEnd();
                string status = (recordData[i, 24] == null) ? null : recordData[i, 24].TrimEnd();

                DataContracts.Person record = new DataContracts.Person();
                record.Recordkey = id;
                record.RecordGuid = guid;
                record.LastName = last;
                record.FirstName = first;
                record.MiddleName = middle;
                record.PersonFormattedNames = new List<string>();
                record.PersonFormattedNameTypes = new List<string>();
                if (ftypes.Count() > 0)
                {
                    record.PFormatEntityAssociation = new List<DataContracts.PersonPFormat>();
                    for (int m = 0; m < ftypes.Count(); m++)
                    {
                        var personFormattedName = new DataContracts.PersonPFormat();
                        personFormattedName.PersonFormattedNameTypesAssocMember = ftypes[m];
                        personFormattedName.PersonFormattedNamesAssocMember = fnames[m];
                        record.PFormatEntityAssociation.Add(personFormattedName);
                    }
                    record.PersonFormattedNames.AddRange(fnames);
                    record.PersonFormattedNameTypes.AddRange(ftypes);
                }
                record.PersonAddresses = new List<string>();
                record.PersonEmailTypes = new List<string>();
                record.PersonEmailAddresses = new List<string>();
                record.PersonPreferredEmail = new List<string>();
                if (etypes.Count() > 0)
                {
                    record.PeopleEmailEntityAssociation = new List<DataContracts.PersonPeopleEmail>();
                    for (int k = 0; k < etypes.Count(); k++)
                    {
                        var personPeopleEmail = new DataContracts.PersonPeopleEmail();
                        personPeopleEmail.PersonEmailTypesAssocMember = etypes[k];
                        personPeopleEmail.PersonEmailAddressesAssocMember = eaddresses[k];
                        personPeopleEmail.PersonPreferredEmailAssocMember = "";
                        record.PeopleEmailEntityAssociation.Add(personPeopleEmail);
                    }
                    record.PersonEmailTypes.AddRange(etypes);
                    record.PersonEmailAddresses.AddRange(eaddresses);
                    record.PersonPreferredEmail.Add("");
                }
                record.Nickname = nickname;
                record.Ssn = ssn;
                record.MaritalStatus = maritalStatus;
                record.PerRaces = races;
                record.PerEthnics = ethnics;
                record.Gender = gender;
                record.BirthDate = birthDate;
                record.DeceasedDate = deceasedDate;
                record.PreferredName = prefNameOverride;
                record.PrivacyFlag = privacyFlag;
                if (record.Recordkey == "9999994")
                {
                    record.BirthNameLast = " Maiden  Name ";
                    record.BirthNameMiddle = "Smith";
                }
                if (record.Recordkey == "9999995")
                {
                    record.Prefix = " Mrs. ";
                    record.Suffix = "Pharm.D., J.D.,       D.C.      ";
                }
                if (record.Recordkey == "9999996")
                {
                    // Used to test the FMLS option and removal of special characters.
                    record.Suffix = " Suf-fix123,  !@#$%^&*()+='/?><:;_,       D.C.      ";
                }
                record.PersonChosenFirstName = chosenFirst;
                record.PersonChosenMiddleName = chosenMiddle;
                record.PersonChosenLastName = chosenLast;
                record.PersonStatus = status;
                ids.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Dictionary<string, DataContracts.PersonIntg> SetupPersonIntg(out List<string> ids)
        {
            string[,] _personIntgData = {
                                        {"0000001", "USA", "123123", "1980-09-24", "WH"},
                                       {"0000002", "USA", "D23s123", "1981-09-24", "BL"},
                                       {"0000003", "USA", "ad23123", "1982-09-24", "W"},
                                       {"0000004", "USA", "7865", "1983-09-24", "W"},
                                       {"0000005", "USA", "17896", "1984-09-24", "AS"},
                                       {"0000006", "USA", "", "1985-09-24", "W"},
                                       {"9999999", "USA", "78652", "1986-09-24", "W"},
                                       {"9999998", "USA", "7865", "1987-09-24", ""},
                                       {"9999997", "USA", "C112322", "1987-09-24", "W"},
                                       {"9999996", "USA", "111111", "1987-09-24", "W"},
                                       {"9999995", "USA", "222222", "1987-09-24", "W"},
                                       {"9999994", "USA", "3333", "1987-09-24", "W"},

            };

            ids = new List<string>();
            string[,] recordData = _personIntgData;

            PersonCount = recordData.Length / 5;
            Dictionary<string, DataContracts.PersonIntg> records = new Dictionary<string, DataContracts.PersonIntg>();
            for (int i = 0; i < PersonCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string birthCountry = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string birthCertificateNumber = (recordData[i, 2] == null) ? String.Empty : recordData[i, 2].TrimEnd();
                string birthDLExpire = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd();
                string birthEthnic = (recordData[i, 4] == null) ? String.Empty : recordData[i, 4].TrimEnd();


                DataContracts.PersonIntg record = new DataContracts.PersonIntg();
                record.Recordkey = id;
                record.PerIntgBirthCountry = birthCountry;
                try
                {
                    record.PerIntgDlExpireDate = Convert.ToDateTime(birthDLExpire);
                }
                catch (InvalidCastException) { }
                record.PerIntgEthnic = birthEthnic;

                ids.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Dictionary<string, TxGetHierarchyAddressResponse> SetupPreferredAddressResponses()
        {
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 10;
            Dictionary<string, TxGetHierarchyAddressResponse> results = new Dictionary<string, TxGetHierarchyAddressResponse>();
            for (int i = 0; i < recordCount; i++)
            {
                string key = recordData[i, 0].TrimEnd();
                string addressId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                List<string> label = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd().Split(';').ToList<string>();
                List<string> lines = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd().Split(';').ToList<string>();
                string city = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                string state = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                string zip = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                string country = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                string countryDesc = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                string modifier = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();

                TxGetHierarchyAddressResponse response = new TxGetHierarchyAddressResponse();
                response.IoPersonId = key;
                response.OutAddressId = addressId;
                response.OutAddressLabel = new List<string>();
                if (label != null) response.OutAddressLabel.AddRange(label);
                response.OutAddressLines = new List<string>();
                if (lines != null) response.OutAddressLines.AddRange(lines);
                response.OutAddressCity = city;
                response.OutAddressState = state;
                response.OutAddressZip = zip;
                response.OutAddressCountry = country;
                response.OutAddressCountryDesc = countryDesc;
                response.OutAddressModifier = modifier;

                results.Add(key, response);
            }
            return results;
        }

        private Collection<DataContracts.Person> BuildPersonResponseData(Dictionary<string, DataContracts.Person> personRecords)
        {
            Collection<DataContracts.Person> personContracts = new Collection<DataContracts.Person>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        #region Test Helper Methods

        protected static Domain.Base.Entities.Person GetTestPersonDataEntities(out List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses, out List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones)
        {
            // setup address test objects
            addresses = new List<Ellucian.Colleague.Domain.Base.Entities.Address>();
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "H",
                AddressLines = new List<string>()
                    {
                        "123 Street",
                        "Fairfax, VA 22033",
                        "Three"
                    },
                City = "Fairfax",
                County = "FFX",
                Country = "US",
                State = "VA",
                PostalCode = "22033"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "BUS",
                AddressLines = new List<string>() { "456 Street" },
                City = "Baltimore",
                County = "BAL",
                Country = "CA",
                State = "MD",
                PostalCode = "11111"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "LOCAL",
                AddressLines = new List<string>() { "789 Street" },
                City = "New York City",
                County = "NY",
                Country = "MX",
                State = "NY",
                PostalCode = "22222"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "MAIL",
                AddressLines = new List<string>() { "000 Street" },
                City = "Boston",
                County = "BOS",
                Country = "BR",
                State = "MA",
                PostalCode = "33333"
            });

            // setup phones test objects
            phones = new List<Ellucian.Colleague.Domain.Base.Entities.Phone>();
            phones.Add(new Domain.Base.Entities.Phone("111-111-1111", "HO"));
            phones.Add(new Domain.Base.Entities.Phone("222-222-2222", "CP"));
            phones.Add(new Domain.Base.Entities.Phone("333-333-3333", "CA"));
            phones.Add(new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444"));

            return GetTestPersonEntity();
        }

        protected static Domain.Base.Entities.PersonIntegration GetTestPersonIntegrationDataEntities(out List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses, out List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones)
        {
            // setup address test objects
            addresses = new List<Ellucian.Colleague.Domain.Base.Entities.Address>();
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "H",
                AddressLines = new List<string>()
                    {
                        "123 Street",
                        "Fairfax, VA 22033",
                        "Three"
                    },
                City = "Fairfax",
                County = "FFX",
                Country = "US",
                State = "VA",
                PostalCode = "22033"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "BUS",
                AddressLines = new List<string>() { "456 Street" },
                City = "Baltimore",
                County = "BAL",
                Country = "CA",
                State = "MD",
                PostalCode = "11111"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "LOCAL",
                AddressLines = new List<string>() { "789 Street" },
                City = "New York City",
                County = "NY",
                Country = "MX",
                State = "NY",
                PostalCode = "22222"
            });
            addresses.Add(new Domain.Base.Entities.Address()
            {
                Type = "MAIL",
                AddressLines = new List<string>() { "000 Street" },
                City = "Boston",
                County = "BOS",
                Country = "BR",
                State = "MA",
                PostalCode = "33333"
            });

            // setup phones test objects
            phones = new List<Ellucian.Colleague.Domain.Base.Entities.Phone>();
            phones.Add(new Domain.Base.Entities.Phone("111-111-1111", "HO"));
            phones.Add(new Domain.Base.Entities.Phone("222-222-2222", "CP"));
            phones.Add(new Domain.Base.Entities.Phone("333-333-3333", "CA"));
            phones.Add(new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444"));

            return GetTestPersonIntegrationEntity();
        }

        protected static Ellucian.Colleague.Domain.Base.Entities.Person GetTestPersonEntity()
        {
            // setup person test entity
            var person = new Domain.Base.Entities.Person("0000011", "Brown");
            string personId = "0000011";
            person = new Domain.Base.Entities.Person(personId, "Brown");
            person.Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104";
            person.Prefix = "Mr.";
            person.FirstName = "Ricky";
            person.MiddleName = "Lee";
            person.Suffix = "Jr.";
            person.Nickname = "Rick";
            person.BirthDate = new DateTime(1930, 1, 1);
            person.DeceasedDate = new DateTime(2014, 5, 12);
            person.Gender = "M";
            person.GovernmentId = "111-11-1111";
            person.MaritalStatusCode = "M";
            person.EthnicCodes = new List<string> { "H" };
            person.RaceCodes = new List<string> { "AS" };
            person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "PRI"));
            return person;
        }

        protected static Ellucian.Colleague.Domain.Base.Entities.PersonIntegration GetTestPersonIntegrationEntity()
        {
            // setup person test entity
            var person = new Domain.Base.Entities.PersonIntegration("0000011", "Brown");
            string personId = "0000011";
            person = new Domain.Base.Entities.PersonIntegration(personId, "Brown")
            {
                Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104",
                Prefix = "Mr.",
                FirstName = "Ricky",
                MiddleName = "Lee",
                Suffix = "Jr.",
                Nickname = "Rick",
                BirthDate = new DateTime(1930, 1, 1),
                DeceasedDate = new DateTime(2014, 5, 12),
                Gender = "M",
                GovernmentId = "111-11-1111",
                MaritalStatusCode = "M",
                EthnicCodes = new List<string> {"H"},
                RaceCodes = new List<string> {"AS"}
                
            };
            person.MaritalStatus =  MaritalState.Married;
            person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "PRI"));
            return person;
        }

        protected static bool ComparePersonIntgUpdateRequest(UpdatePersonIntegrationRequest request, Domain.Base.Entities.PersonIntegration person, List<Domain.Base.Entities.Address> addresses, List<Phone> phones)
        {
            // person
            if (request.PersonGuid != person.Guid) return false;
            var legal = request.PersonIntgNames.FirstOrDefault(x => x.PersonNameType == "LEGAL");

            //if (request.Prefix != person.Prefix) return false;
             if (legal.FirstName != person.FirstName) return false;
            if (legal.LastName != person.LastName) return false;
             if (legal.MiddleName != person.MiddleName) return false;
            //if (request.Suffix != person.Suffix) return false;
            //if (request.Nickname != person.Nickname) return false;
            if (request.Gender != person.Gender) return false;
            //if (request.MaritalStatus != person.MaritalStatusCode) return false;
            //if (request.EthnicCodes != person.EthnicCodes) return false;
            if (request.RaceCodes != person.RaceCodes) return false;
            if (request.BirthDate != person.BirthDate) return false;
            if (request.DeceasedDate != person.DeceasedDate) return false;
            //if (request.Ssn != person.GovernmentId) return false;
           // if (request.EmailAddresses[0].EmailAddressValue != person.EmailAddresses[0].Value) return false;
           // if (request.EmailAddresses[0].EmailAddressType != person.EmailAddresses[0].TypeCode) return false;
            /* addresses
            for (int i = 0; i < addresses.Count(); i++)
            {
                if (addresses[i].City != request.Addresses[i].AddressCity) return false;
                if (addresses[i].State != request.Addresses[i].AddressRegion) return false;
                if (addresses[i].County != request.Addresses[i].AddressCounty) return false;
                if (addresses[i].Country != request.Addresses[i].AddressCountry) return false;
                if (addresses[i].PostalCode != request.Addresses[i].AddressPostalCode) return false;
                if (addresses[i].AddressLines[0] != request.Addresses[i].AddressStreet1) return false;
                if (addresses[i].AddressLines.Count() > 1)
                {
                    if (addresses[i].AddressLines[1] != request.Addresses[i].AddressStreet2) return false;
                }
                if (addresses[i].AddressLines.Count() > 2)
                {
                    if (addresses[i].AddressLines[2] != request.Addresses[i].AddressStreet3) return false;
                }
            }
             * */
            /*
            // phones
            for (int i = 0; i < phones.Count(); i++)
            {
                if (phones[i].Number != request.Phones[i].PhoneNumber) return false;
                if (phones[i].TypeCode != request.Phones[i].PhoneType) return false;
                if (phones[i].Extension != request.Phones[i].PhoneExtension) return false;
            }
             * */
            return true;
        }

        protected static bool CompareOrgIntgUpdateRequest(UpdateOrganizationIntegrationRequest request, Domain.Base.Entities.PersonIntegration person, List<Domain.Base.Entities.Address> addresses, List<Phone> phones)
        {
            if (request.OrgGuid != person.Guid) return false;
            if (request.OrgId != person.Id) return false;
            if (request.OrgTitle != person.PreferredName) return false;

            //roles
            if (person.Roles.Any())
            {
                foreach (var role in person.Roles)
                {
                    var orgIntgRole = request.OrgIntgRoles.FirstOrDefault(x => x.OrgRoles == role.RoleType.ToString());
                    if (orgIntgRole == null) return false;
                    if (orgIntgRole.OrgRoleEndDate != role.EndDate) return false;
                    if (orgIntgRole.OrgRoleStartDate != role.StartDate) return false;
                }
            }

            //credentials
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                foreach (var doc in person.PersonAltIds)
                {
                    var orgDoc = request.OrgIntgAlternate.FirstOrDefault(x => x.OrgAlternateIds == doc.Id);

                    if (orgDoc == null) return false;

                    if (orgDoc.OrgAlternateIdTypes != doc.Type) return false;
                }
            }

            /* addresses
            if (addresses != null && addresses.Any())
            {
                foreach (var address in addresses)
                {

                    var addressRequest = request.OrgIntgAddresses.FirstOrDefault(x => x.OrgAddrIds == address.Guid);
                    if (addressRequest == null) return false;

                    if (addressRequest.OrgAddrTypes != address.TypeCode) return false;
                    if (addressRequest.OrgAddrCities != (!string.IsNullOrEmpty(address.City) ? address.City : address.IntlLocality)) return false;
                    if (addressRequest.OrgAddrCountries != (!string.IsNullOrEmpty(address.CountryCode) ? address.CountryCode : address.Country)) return false;
                    if (addressRequest.OrgAddrSubregions != (!string.IsNullOrEmpty(address.County) ? address.County : address.IntlSubRegion)) return false;
                    if (addressRequest.OrgAddrCodes != (!string.IsNullOrEmpty(address.PostalCode) ? address.PostalCode : address.IntlPostalCode)) return false;
                    if (addressRequest.OrgAddrRegions != (!string.IsNullOrEmpty(address.State) ? address.State : address.IntlRegion)) return false;
                    if (addressRequest.OrgAddrLongitude != address.Longitude.ToString()) return false;
                    if (addressRequest.OrgAddrLatitude != address.Latitude.ToString()) return false;
                    if (addressRequest.OrgAddrDeliveryPoint != address.DeliveryPoint) return false;
                    if (addressRequest.OrgAddrCorrectionDigit != address.CorrectionDigit) return false;
                    if (addressRequest.OrgAddrPreference != (address.IsPreferredResidence == true ? "Y" : null)) return false;
                    
                }
            }
            */
            // Phone numbers
            if (phones != null && phones.Any())
            {
                foreach (var phone in phones)
                {
                    //var phoneRequest = new OrgIntgPhones();
                    var phoneRequest = request.OrgIntgPhones.FirstOrDefault(x => x.OrgPhoneNumbers == phone.Number && x.OrgPhoneTypes == phone.TypeCode);
                    if (phoneRequest == null) return false;
                    //phoneRequest.OrgPhoneTypes = phone.TypeCode;
                    //phoneRequest.OrgPhoneNumbers = phone.Number;
                    phoneRequest.OrgPhoneExtensions = phone.Extension;
                    phoneRequest.OrgPhoneCallingCode = phone.CountryCallingCode;
                    phoneRequest.OrgPhonePref = phone.IsPreferred ? "Y" : string.Empty;
                    request.OrgIntgPhones.Add(phoneRequest);
                }
            }
            // Social media
            if (person.SocialMedia != null && person.SocialMedia.Any())
            {
                foreach (var socialMedia in person.SocialMedia)
                {
                    var socialMediaRequest = request.OrgIntgSocialMedia.FirstOrDefault
                        (x => x.OrgSocialMediaTypes == socialMedia.TypeCode && x.OrgSocialMediaHandles == socialMedia.Handle);
                    if (socialMediaRequest == null) return false;
                    //var socialMediaRequest = new OrgIntgSocialMedia();
                    //socialMediaRequest.OrgSocialMediaTypes = socialMedia.TypeCode;
                    //socialMediaRequest.OrgSocialMediaHandles = socialMedia.Handle;
                    if (socialMediaRequest.OrgSocialMediaPref != (socialMedia.IsPreferred ? "Y" : null)) return false;

                }
            }
            return true;
        }

        #endregion

    }
}
