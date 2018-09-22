// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ProxyRepositoryTests : BaseRepositorySetup
    {
        public Collection<ProxyAccessPerms> proxyWorkflows;
        public ApplValcodes proxyGroups;
        #region Initialize and Cleanup
        ProxyRepository proxyRepository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            proxyRepository = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            proxyRepository = null;
        }

        #endregion

        [TestClass]
        public class ProxyRepository_GetProxyConfigurationAsync : ProxyRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NoProxyDefaults()
            {
                ProxyDefaults proxyDefaults = null;
                dataReaderMock.Setup<Task<ProxyDefaults>>(accessor =>
                    accessor.ReadRecordAsync<ProxyDefaults>
                    (It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyDefaults);
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);


                var config = await proxyRepository.GetProxyConfigurationAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NoProxyGroups()
            {
                ApplValcodes proxyGroups = null;
                dataReaderMock.Setup<Task<ApplValcodes>>(accessor =>
                    accessor.ReadRecordAsync<ApplValcodes>
                    ("UT.VALCODES", "PROXY.ACCESS.GROUPS", It.IsAny<bool>())).ReturnsAsync(proxyGroups);
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NoProxyWorkflows()
            {
                Collection<ProxyAccessPerms> proxyWorkflows = null;
                //dataReaderMock.Setup<Task<Collection<ProxyAccessPerms>>>(accessor =>
                //    accessor.BulkReadRecordAsync<ProxyAccessPerms>
                //    (It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyWorkflows);
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                MockRecordsAsync("PROXY.ACCESS.PERMS", proxyWorkflows);

                var config = await proxyRepository.GetProxyConfigurationAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NoProxyWorkflows2()
            {
                var proxyWorkflows = new Collection<ProxyAccessPerms>();
                //dataReaderMock.Setup<Task<Collection<ProxyAccessPerms>>>(accessor =>
                //    accessor.BulkReadRecordAsync<ProxyAccessPerms>
                //    (It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyWorkflows);
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                MockRecordsAsync<ProxyAccessPerms>("PROXY.ACCESS.PERMS", proxyWorkflows);

                var config = await proxyRepository.GetProxyConfigurationAsync();
            }

            [TestMethod]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NullDemographics()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var proxyDefaults = new ProxyDefaults()
                {
                    Recordkey = "PROXY.DEFAULTS",
                    PrxdProxyEnabled = "Y",
                    PrxdDisclosureReleaseDoc = "RELEASEDOC",
                    PrxdProxyEmailDocument = "EMAILDOC",
                    PrxdDisclosureReleaseText = "This is some text.",
                    PrxdAddNewProxyAllowed = "Y",
                    PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                    PrxdDemographicsEntityAssociation = null
                };
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.AreEqual(0, config.DemographicFields.Count);
            }

            [TestMethod]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NoDemographics()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var proxyDefaults = new ProxyDefaults()
                {
                    Recordkey = "PROXY.DEFAULTS",
                    PrxdProxyEnabled = "Y",
                    PrxdDisclosureReleaseDoc = "RELEASEDOC",
                    PrxdProxyEmailDocument = "EMAILDOC",
                    PrxdDisclosureReleaseText = "This is some text.",
                    PrxdAddNewProxyAllowed = "Y",
                    PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                    PrxdDemographicsEntityAssociation = new List<ProxyDefaultsPrxdDemographics>()
                };
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.AreEqual(0, config.DemographicFields.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_InvalidDemographics()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var proxyDefaults = new ProxyDefaults()
                {
                    Recordkey = "PROXY.DEFAULTS",
                    PrxdProxyEnabled = "Y",
                    PrxdDisclosureReleaseDoc = "RELEASEDOC",
                    PrxdProxyEmailDocument = "EMAILDOC",
                    PrxdDisclosureReleaseText = "This is some text.",
                    PrxdAddNewProxyAllowed = "Y",
                    PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                    PrxdDemographicsEntityAssociation = new List<ProxyDefaultsPrxdDemographics>()
                    {
                        new ProxyDefaultsPrxdDemographics() { PrxdDemoElementsAssocMember = "ABC", PrxdDemoDescsAssocMember = "Field 1", PrxdDemoRqmtAssocMember = "X" },
                    }
                };

                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.AreEqual(0, config.DemographicFields.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyRepository_GetProxyConfigurationAsync_InvalidDemographics2()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var proxyDefaults = new ProxyDefaults()
                {
                    Recordkey = "PROXY.DEFAULTS",
                    PrxdProxyEnabled = "Y",
                    PrxdDisclosureReleaseDoc = "RELEASEDOC",
                    PrxdProxyEmailDocument = "EMAILDOC",
                    PrxdDisclosureReleaseText = "This is some text.",
                    PrxdAddNewProxyAllowed = "Y",
                    PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                    PrxdDemographicsEntityAssociation = new List<ProxyDefaultsPrxdDemographics>()
                    {
                        new ProxyDefaultsPrxdDemographics() { PrxdDemoElementsAssocMember = "ABC", PrxdDemoDescsAssocMember = "Field 1", PrxdDemoRqmtAssocMember = null },
                    }
                };
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.AreEqual(0, config.DemographicFields.Count);
            }

            [TestMethod]
            public async Task ProxyRepository_GetProxyConfigurationAsync_NullDemographicEntry()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var proxyDefaults = new ProxyDefaults()
                {
                    Recordkey = "PROXY.DEFAULTS",
                    PrxdProxyEnabled = "Y",
                    PrxdDisclosureReleaseDoc = "RELEASEDOC",
                    PrxdProxyEmailDocument = "EMAILDOC",
                    PrxdDisclosureReleaseText = "This is some text.",
                    PrxdAddNewProxyAllowed = "Y",
                    PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                    PrxdDemographicsEntityAssociation = new List<ProxyDefaultsPrxdDemographics>()
                    {
                        null,
                    }
                };
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);

                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.AreEqual(0, config.DemographicFields.Count);
            }

            [TestMethod]
            public async Task ProxyRepository_GetProxyConfigurationAsync_Valid()
            {
                BuildProxyWorkflows();
                BuildProxyGroups();
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.IsNotNull(config);
                Assert.AreEqual(3, config.RelationshipTypeCodes.Count);
                Assert.AreEqual(3, config.DemographicFields.Count);
                Assert.AreEqual("This is some header text for the View/Grant Third Party Access view.", config.ProxyFormHeaderText);
                Assert.AreEqual("This is some text displayed above the Add a Proxy section.", config.AddProxyHeaderText);
                Assert.AreEqual("This is some reauthorization text.", config.ReauthorizationText);
            }

            [TestMethod]
            public async Task ProxyRepository_GetProxyConfigurationEmployeeAsync_Valid()
            {
                BuildEmployeeProxyWorkflows();
                BuildEmployeeProxyGroups();
                var defaults = new Dflts()
                {
                    Recordkey = "DEFAULTS",
                    DfltsLdapContextSubr = "S.OVERRIDE.LDAP.CONTEXT"
                };
                MockRecordAsync<Dflts>("CORE.PARMS", defaults);
                var config = await proxyRepository.GetProxyConfigurationAsync();
                Assert.IsNotNull(config);
                Assert.AreEqual("SSHRTA", config.WorkflowGroups.ElementAt(0).Workflows.ElementAt(0).WorklistCategorySpecialProcessCode);
            }
        }

        [TestClass]
        public class ProxyRepository_GetUserProxyPermissionsAsync : ProxyRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_NullId()
            {
                var permissions = await proxyRepository.GetUserProxyPermissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_EmptyId()
            {
                var permissions = await proxyRepository.GetUserProxyPermissionsAsync(string.Empty);
            }

            [TestMethod]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_NoActivePermissions()
            {
                Collection<ProxyAccess> nullCollection = null;
                MockRecordsAsync<ProxyAccess>("PROXY.ACCESS", nullCollection);

                var permissions = await proxyRepository.GetUserProxyPermissionsAsync("0003314");
                Assert.AreEqual(0, permissions.Count());
            }

            [TestMethod]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_NoActivePermissions2()
            {
                MockRecordsAsync("PROXY.ACCESS", new Collection<ProxyAccess>());

                var permissions = await proxyRepository.GetUserProxyPermissionsAsync("0003314");
                Assert.AreEqual(0, permissions.Count());
            }

            [TestMethod]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_CorruptRecord()
            {
                Collection<ProxyAccess> proxyAccesses = new Collection<ProxyAccess>()
                {
                    new ProxyAccess()
                    {
                        Recordkey = "32",
                        PracProxyWebUser = string.Empty,
                        PracPrincipalWebUser = "0003315",
                        PracBeginDate = DateTime.Parse("08/21/2015"),
                        PracEndDate = null,
                        PracReauthReqDate = null,
                        PracProxyAccessPermission = "SFMAP",
                        PracDisclosureReleaseDoc = null,
                        PracProxyApprovalEmail = null,
                        ProxyAccessChgdate = DateTime.Parse("08/21/2015")
                    },
                    new ProxyAccess()
                    {
                        Recordkey = "33",
                        PracProxyWebUser = "0003316",
                        PracPrincipalWebUser = "0003315",
                        PracBeginDate = DateTime.Parse("08/21/2015"),
                        PracEndDate = null,
                        PracReauthReqDate = null,
                        PracProxyAccessPermission = "SFAA",
                        PracDisclosureReleaseDoc = null,
                        PracProxyApprovalEmail = null,
                        ProxyAccessChgdate = DateTime.Parse("08/21/2015")
                    },
                };
                MockRecordsAsync("PROXY.ACCESS", proxyAccesses);

                var users = await proxyRepository.GetUserProxyPermissionsAsync("0003315");
                Assert.AreEqual(1, users.Count());
            }

            [TestMethod]
            public async Task ProxyRepository_GetUserProxyPermissionsAsync_Valid()
            {
                var users = await proxyRepository.GetUserProxyPermissionsAsync("0003315");
                Assert.AreEqual(3, users.Count(), "User count mismatch");
                foreach (var user in users)
                {
                    Assert.AreEqual(3, user.Permissions.Count, "Permission count mismatch for user " + user.Id);
                }
            }
        }

        [TestClass]
        public class ProxyRepository_PostUserProxyPermissionsAsync : ProxyRepositoryTests
        {
            ProxyPermissionAssignment assignment;
            CreateUpdateProxyAccessRequest createRequest;
            CreateUpdateProxyAccessResponse response, validResponse, warningResponse, errorResponse;
            UpdateProxyEmailRequest updateEmailRequest, checkUpdateEmailRequest;
            UpdateProxyEmailResponse updateEmailSuccessResponse, updateEmailErrorResponse;

            [TestInitialize]
            public void ProxyRepository_PostUserProxyPermissionsAsync_Initialize()
            {
                validResponse = new CreateUpdateProxyAccessResponse()
                {
                    ProxyAccessIdentifiers = new List<string>() { "1", "2" },
                    WarningIndicator = false,
                    ErrorIndicator = false,
                    Messages = new List<string>()
                };
                warningResponse = new CreateUpdateProxyAccessResponse()
                {
                    ProxyAccessIdentifiers = new List<string>() { "1" },
                    WarningIndicator = true,
                    ErrorIndicator = false,
                    Messages = new List<string>() { "This is a warning message." }
                };
                errorResponse = new CreateUpdateProxyAccessResponse()
                {
                    ProxyAccessIdentifiers = new List<string>(),
                    WarningIndicator = false,
                    ErrorIndicator = true,
                    Messages = new List<string>() { "This is an error message." }
                };
                updateEmailSuccessResponse = new UpdateProxyEmailResponse()
                {
                    ErrorOccurred = string.Empty,
                    Msg = string.Empty
                };
                updateEmailErrorResponse = new UpdateProxyEmailResponse()
                {
                    ErrorOccurred = "1",
                    Msg = "Error occurred at update"
                };

                proxyRepository = base.BuildValidRepository();
            }

            [TestMethod]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_Valid()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFAA", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(validResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);

                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
                var resultList = result.ToList();

                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, createRequest.PrincipalIdentifier);
                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, resultList[0].ProxySubjectId);
                Assert.AreEqual(proxyPermissionAssignment.Permissions.Count, resultList.Count);
                Assert.IsFalse(proxyPermissionAssignment.IsReauthorizing);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyUserId, resultList[0].ProxyUserId);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyWorkflowCode, resultList[0].ProxyWorkflowCode);
                Assert.AreEqual(proxyAccessPermissions[1].ProxyUserId, resultList[1].ProxyUserId);
                Assert.AreEqual(proxyAccessPermissions[1].ProxyWorkflowCode, resultList[1].ProxyWorkflowCode);
            }

            [TestMethod]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_Warning()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(warningResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);

                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
                var resultList = result.ToList();

                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, createRequest.PrincipalIdentifier);
                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, resultList[0].ProxySubjectId);
                Assert.AreEqual(1, resultList.Count);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyUserId, resultList[0].ProxyUserId);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyWorkflowCode, resultList[0].ProxyWorkflowCode);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_ErrorDuringProcessing()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(errorResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);

                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_NullDataRead()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(validResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);
                Collection<ProxyAccess> nullRead = null;
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<ProxyAccess>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(nullRead));

                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_RecordCountMismatch()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(validResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);
                Collection<ProxyAccess> emptyRead = new Collection<ProxyAccess>();
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<ProxyAccess>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns<string[], bool>((ids, flag) =>
                    Task.FromResult(emptyRead));

                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostUserProxyPermissionsAsync_CtxError()
            {
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                {
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                    new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today)
                };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .ThrowsAsync(new Exception())
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);
                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
            }

            [TestMethod]
            public async Task ProxyRepository_PostUserProxyPermissionsAsyncWithEmailUpdate_Valid()
            {
                  
                var proxyAccessPermissions = new List<ProxyAccessPermission>()
                    {
                        new ProxyAccessPermission("", "0003900", "0004100", "SFMAP", DateTime.Today),
                        new ProxyAccessPermission("", "0003900", "0004100", "SFAA", DateTime.Today)
                    };
                var proxyPermissionAssignment = new ProxyPermissionAssignment("0003900", proxyAccessPermissions);
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateUpdateProxyAccessRequest, CreateUpdateProxyAccessResponse>(It.IsAny<CreateUpdateProxyAccessRequest>()))
                    .Returns(Task.FromResult<CreateUpdateProxyAccessResponse>(validResponse))
                    .Callback<CreateUpdateProxyAccessRequest>(req => createRequest = req);

                // Add email address and type to the incoming entity, mock email update response
                proxyPermissionAssignment.ProxyEmailAddress = "testemail@ellucian.com";
                proxyPermissionAssignment.ProxyEmailType = "Personal";
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<UpdateProxyEmailRequest, UpdateProxyEmailResponse>(It.IsAny<UpdateProxyEmailRequest>()))
                    .Returns(Task.FromResult<UpdateProxyEmailResponse>(updateEmailErrorResponse))
                    .Callback<UpdateProxyEmailRequest>(req => checkUpdateEmailRequest = req);
                proxyRepository = new ProxyRepository(cacheProvider, transFactory, logger);

                var result = await proxyRepository.PostUserProxyPermissionsAsync(proxyPermissionAssignment);
                var resultList = result.ToList();

                // Verify email update request and logging of error returned
                Assert.AreEqual(proxyPermissionAssignment.ProxyEmailAddress, checkUpdateEmailRequest.EmailAddress);
                Assert.AreEqual(proxyPermissionAssignment.ProxyEmailType, checkUpdateEmailRequest.EmailAddressType);
                loggerMock.Verify(e => e.Error(It.IsAny<string>()));

                // Verify proxy update continued in spite of error response
                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, createRequest.PrincipalIdentifier);
                Assert.AreEqual(proxyPermissionAssignment.ProxySubjectId, resultList[0].ProxySubjectId);
                Assert.AreEqual(proxyPermissionAssignment.Permissions.Count, resultList.Count);
                Assert.IsFalse(proxyPermissionAssignment.IsReauthorizing);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyUserId, resultList[0].ProxyUserId);
                Assert.AreEqual(proxyAccessPermissions[0].ProxyWorkflowCode, resultList[0].ProxyWorkflowCode);
                Assert.AreEqual(proxyAccessPermissions[1].ProxyUserId, resultList[1].ProxyUserId);
                Assert.AreEqual(proxyAccessPermissions[1].ProxyWorkflowCode, resultList[1].ProxyWorkflowCode);
            }
        }

        [TestClass]
        public class ProxyRepository_PostProxyCandidateAsync : ProxyRepositoryTests
        {
            ProxyCandidate defCandidate;
            ProxyCandidate potCandidate;
            CreateProxyCandidateRequest createRequest;
            CreateProxyCandidateResponse validResponse, errorResponse;

            [TestInitialize]
            public void ProxyRepository_PostProxyCandidateAsync_Initialize()
            {
                validResponse = new CreateProxyCandidateResponse()
                {
                    ProxyCandidateId = "CandId",
                    ErrorMessages = new List<string>(),
                    ErrorOccurred = false,
                };
                errorResponse = new CreateProxyCandidateResponse()
                {
                    ProxyCandidateId = string.Empty,
                    ErrorMessages = new List<string>() { "This is an error message" },
                    ErrorOccurred = true,
                };
                // this needs to match up with the PROXY.CANDIDATES mock record in the BuildValidRepository method
                defCandidate = new ProxyCandidate("0005141", "P", new List<string>() { "SFAA" }, "Joe", "TestCase", "TestCase@ellucian.edu",
                    new List<PersonMatchResult>() { new PersonMatchResult("CandId", 90, "D") })
                {
                    BirthDate = DateTime.Today,
                    EmailType = "PRI",
                    FormerFirstName = "WasJoe",
                    FormerLastName = "WasTestCase",
                    FormerMiddleName = "WasMiddleName",
                    Gender = "M",
                    MiddleName = "MiddleName",
                    Phone = "My Phone",
                    PhoneExtension = "My Extension",
                    PhoneType = "CP",
                    Prefix = "Mr.",
                    GovernmentId = "987654321",
                    Suffix = "Jr.",
                    Id = "CandId",
                };

                potCandidate = new ProxyCandidate("0005141", "P", new List<string>() { "SFAA" }, "Joe", "TestCase", "TestCase@ellucian.edu",
                    new List<PersonMatchResult>() { new PersonMatchResult("CandId", 90, "P") })
                {
                    BirthDate = DateTime.Today,
                    EmailType = "PRI",
                    FormerFirstName = "WasJoe",
                    FormerLastName = "WasTestCase",
                    FormerMiddleName = "WasMiddleName",
                    Gender = "M",
                    MiddleName = "MiddleName",
                    Phone = "My Phone",
                    PhoneExtension = "My Extension",
                    PhoneType = "CP",
                    Prefix = "Mr.",
                    GovernmentId = "987654321",
                    Suffix = "Jr.",
                    Id = "CandId",
                };
            }

            [TestMethod]
            public async Task ProxyRepository_PostProxyCandidateAsync_Valid_DefiniteCandidate()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(validResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
            {
                PrxcBirthDate = DateTime.Today,
                PrxcCategory = new List<string>() { "D" },
                PrxcPersonId = new List<string>() { "CandId" },
                PrxcScore = new List<int?>() { 90 },
                ProxyMatchResultsEntityAssociation =
                    new List<ProxyCandidatesProxyMatchResults>()
                    {
                        new ProxyCandidatesProxyMatchResults(){
                            PrxcPersonIdAssocMember = "CandId",
                            PrxcCategoryAssocMember = "D",
                            PrxcScoreAssocMember = 90
                        }
                    },
                PrxcEmailAddress = "TestCase@ellucian.edu",
                PrxcEmailType = "PRI",
                PrxcFirstName = "Joe",
                PrxcFormerFirstName = "WasJoe",
                PrxcFormerLastName = "WasTestCase",
                PrxcFormerMiddleName = "WasMiddleName",
                PrxcGender = "M",
                PrxcGrantedPerms = new List<string>() { "SFAA" },
                PrxcLastName = "TestCase",
                PrxcMiddleName = "MiddleName",
                PrxcPhone = "My Phone",
                PrxcPhoneExtension = "My Extension",
                PrxcPhoneType = "CP",
                PrxcPrefix = "Mr.",
                PrxcProxySubject = "0005141",
                PrxcRelationType = "P",
                PrxcSsn = "987654321",
                PrxcSuffix = "Jr.",
                Recordkey = "ProxyCandidatesId",
            };
                dataReaderMock.Setup<Task<ProxyCandidates>>(accessor =>
                    accessor.ReadRecordAsync<ProxyCandidates>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyCandidateData);

                var result = await proxyRepository.PostProxyCandidateAsync(defCandidate);
                Assert.AreEqual("ProxyCandidatesId", result.Id);
                Assert.AreEqual(defCandidate.BirthDate, result.BirthDate);
                Assert.AreEqual(defCandidate.EmailAddress, result.EmailAddress);
                Assert.AreEqual(defCandidate.EmailType, result.EmailType);
                Assert.AreEqual(defCandidate.FirstName, result.FirstName);
                Assert.AreEqual(defCandidate.FormerFirstName, result.FormerFirstName);
                Assert.AreEqual(defCandidate.FormerLastName, result.FormerLastName);
                Assert.AreEqual(defCandidate.FormerMiddleName, result.FormerMiddleName);
                Assert.AreEqual(defCandidate.Gender, result.Gender);
                Assert.AreEqual(defCandidate.LastName, result.LastName);
                Assert.AreEqual(defCandidate.MiddleName, result.MiddleName);
                Assert.AreEqual(defCandidate.Phone, result.Phone);
                Assert.AreEqual(defCandidate.PhoneExtension, result.PhoneExtension);
                Assert.AreEqual(defCandidate.PhoneType, result.PhoneType);
                Assert.AreEqual(defCandidate.Prefix, result.Prefix);
                Assert.AreEqual(defCandidate.RelationType, result.RelationType);
                Assert.AreEqual(defCandidate.GovernmentId, result.GovernmentId);
                Assert.AreEqual(defCandidate.Suffix, result.Suffix);
                Assert.AreEqual(defCandidate.Id, "CandId");
                CollectionAssert.AreEquivalent(defCandidate.GrantedPermissions.ToList(), result.GrantedPermissions.ToList());
            }

            [TestMethod]
            public async Task ProxyRepository_PostProxyCandidateAsync_Valid_PotentialCandidate()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(validResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
                {
                    PrxcBirthDate = DateTime.Today,
                    PrxcCategory = new List<string>() { "P" },
                    PrxcPersonId = new List<string>() { "CandId" },
                    PrxcScore = new List<int?>() { 90 },
                    ProxyMatchResultsEntityAssociation =
                        new List<ProxyCandidatesProxyMatchResults>()
                    {
                        new ProxyCandidatesProxyMatchResults(){
                            PrxcPersonIdAssocMember = "CandId",
                            PrxcCategoryAssocMember = "P",
                            PrxcScoreAssocMember = 90
                        }
                    },
                    PrxcEmailAddress = "TestCase@ellucian.edu",
                    PrxcEmailType = "PRI",
                    PrxcFirstName = "Joe",
                    PrxcFormerFirstName = "WasJoe",
                    PrxcFormerLastName = "WasTestCase",
                    PrxcFormerMiddleName = "WasMiddleName",
                    PrxcGender = "M",
                    PrxcGrantedPerms = new List<string>() { "SFAA" },
                    PrxcLastName = "TestCase",
                    PrxcMiddleName = "MiddleName",
                    PrxcPhone = "My Phone",
                    PrxcPhoneExtension = "My Extension",
                    PrxcPhoneType = "CP",
                    PrxcPrefix = "Mr.",
                    PrxcProxySubject = "0005141",
                    PrxcRelationType = "P",
                    PrxcSsn = "987654321",
                    PrxcSuffix = "Jr.",
                    Recordkey = "ProxyCandidatesId",
                };
                dataReaderMock.Setup<Task<ProxyCandidates>>(accessor =>
                    accessor.ReadRecordAsync<ProxyCandidates>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyCandidateData);

                var result = await proxyRepository.PostProxyCandidateAsync(potCandidate);
                Assert.AreEqual("ProxyCandidatesId", result.Id);
                Assert.AreEqual(potCandidate.BirthDate, result.BirthDate);
                Assert.AreEqual(potCandidate.EmailAddress, result.EmailAddress);
                Assert.AreEqual(potCandidate.EmailType, result.EmailType);
                Assert.AreEqual(potCandidate.FirstName, result.FirstName);
                Assert.AreEqual(potCandidate.FormerFirstName, result.FormerFirstName);
                Assert.AreEqual(potCandidate.FormerLastName, result.FormerLastName);
                Assert.AreEqual(potCandidate.FormerMiddleName, result.FormerMiddleName);
                Assert.AreEqual(potCandidate.Gender, result.Gender);
                Assert.AreEqual(potCandidate.LastName, result.LastName);
                Assert.AreEqual(potCandidate.MiddleName, result.MiddleName);
                Assert.AreEqual(potCandidate.Phone, result.Phone);
                Assert.AreEqual(potCandidate.PhoneExtension, result.PhoneExtension);
                Assert.AreEqual(potCandidate.PhoneType, result.PhoneType);
                Assert.AreEqual(potCandidate.Prefix, result.Prefix);
                Assert.AreEqual(potCandidate.RelationType, result.RelationType);
                Assert.AreEqual(potCandidate.GovernmentId, result.GovernmentId);
                Assert.AreEqual(potCandidate.Suffix, result.Suffix);
                Assert.AreEqual(potCandidate.Id, "CandId");
                CollectionAssert.AreEquivalent(potCandidate.GrantedPermissions.ToList(), result.GrantedPermissions.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostProxyCandidateAsync_ErrorDuringProcessing()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(errorResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
                {
                    PrxcBirthDate = DateTime.Today,
                    PrxcCategory = new List<string>() { "D" },
                    PrxcPersonId = new List<string>() { "CandId" },
                    PrxcScore = new List<int?>() { 90 },
                    ProxyMatchResultsEntityAssociation =
                        new List<ProxyCandidatesProxyMatchResults>()
                    {
                        new ProxyCandidatesProxyMatchResults(){
                            PrxcPersonIdAssocMember = "CandId",
                            PrxcCategoryAssocMember = "D",
                            PrxcScoreAssocMember = 90
                        }
                    },
                    PrxcEmailAddress = "TestCase@ellucian.edu",
                    PrxcEmailType = "PRI",
                    PrxcFirstName = "Joe",
                    PrxcFormerFirstName = "WasJoe",
                    PrxcFormerLastName = "WasTestCase",
                    PrxcFormerMiddleName = "WasMiddleName",
                    PrxcGender = "M",
                    PrxcGrantedPerms = new List<string>() { "SFAA" },
                    PrxcLastName = "TestCase",
                    PrxcMiddleName = "MiddleName",
                    PrxcPhone = "My Phone",
                    PrxcPhoneExtension = "My Extension",
                    PrxcPhoneType = "CP",
                    PrxcPrefix = "Mr.",
                    PrxcProxySubject = "0005141",
                    PrxcRelationType = "P",
                    PrxcSsn = "987654321",
                    PrxcSuffix = "Jr.",
                    Recordkey = "ProxyCandidatesId",
                };

                proxyRepository = new ProxyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                var result = await proxyRepository.PostProxyCandidateAsync(defCandidate);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostProxyCandidateAsync_NullDataRead()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(errorResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
                {
                    PrxcBirthDate = DateTime.Today,
                    PrxcCategory = new List<string>() { "D" },
                    PrxcPersonId = new List<string>() { "CandId" },
                    PrxcScore = new List<int?>() { 90 },
                    ProxyMatchResultsEntityAssociation =
                        new List<ProxyCandidatesProxyMatchResults>()
                    {
                        new ProxyCandidatesProxyMatchResults(){
                            PrxcPersonIdAssocMember = "CandId",
                            PrxcCategoryAssocMember = "D",
                            PrxcScoreAssocMember = 90
                        }
                    },
                    PrxcEmailAddress = "TestCase@ellucian.edu",
                    PrxcEmailType = "PRI",
                    PrxcFirstName = "Joe",
                    PrxcFormerFirstName = "WasJoe",
                    PrxcFormerLastName = "WasTestCase",
                    PrxcFormerMiddleName = "WasMiddleName",
                    PrxcGender = "M",
                    PrxcGrantedPerms = new List<string>() { "SFAA" },
                    PrxcLastName = "TestCase",
                    PrxcMiddleName = "MiddleName",
                    PrxcPhone = "My Phone",
                    PrxcPhoneExtension = "My Extension",
                    PrxcPhoneType = "CP",
                    PrxcPrefix = "Mr.",
                    PrxcProxySubject = "0005141",
                    PrxcRelationType = "P",
                    PrxcSsn = "987654321",
                    PrxcSuffix = "Jr.",
                    Recordkey = "ProxyCandidatesId",
                };

                dataReaderMock.Setup<Task<ProxyCandidates>>(accessor =>
                    accessor.ReadRecordAsync<ProxyCandidates>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);

                proxyRepository = new ProxyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                var result = await proxyRepository.PostProxyCandidateAsync(defCandidate);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostProxyCandidateAsync_NoMatchResults()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(validResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
                {
                    PrxcBirthDate = DateTime.Today,
                    PrxcCategory = new List<string>() { "D" },
                    PrxcPersonId = new List<string>() { "CandId" },
                    PrxcScore = new List<int?>() { 90 },
                    ProxyMatchResultsEntityAssociation =
                        new List<ProxyCandidatesProxyMatchResults>(),
                    PrxcEmailAddress = "TestCase@ellucian.edu",
                    PrxcEmailType = "PRI",
                    PrxcFirstName = "Joe",
                    PrxcFormerFirstName = "WasJoe",
                    PrxcFormerLastName = "WasTestCase",
                    PrxcFormerMiddleName = "WasMiddleName",
                    PrxcGender = "M",
                    PrxcGrantedPerms = new List<string>() { "SFAA" },
                    PrxcLastName = "TestCase",
                    PrxcMiddleName = "MiddleName",
                    PrxcPhone = "My Phone",
                    PrxcPhoneExtension = "My Extension",
                    PrxcPhoneType = "CP",
                    PrxcPrefix = "Mr.",
                    PrxcProxySubject = "0005141",
                    PrxcRelationType = "P",
                    PrxcSsn = "987654321",
                    PrxcSuffix = "Jr.",
                    Recordkey = "ProxyCandidatesId",
                };
                dataReaderMock.Setup<Task<ProxyCandidates>>(accessor =>
                    accessor.ReadRecordAsync<ProxyCandidates>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyCandidateData);

                var result = await proxyRepository.PostProxyCandidateAsync(defCandidate);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ProxyRepository_PostProxyCandidateAsync_NullMatchResults()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreateProxyCandidateRequest, CreateProxyCandidateResponse>(It.IsAny<CreateProxyCandidateRequest>()))
                    .Returns(Task.FromResult<CreateProxyCandidateResponse>(validResponse))
                    .Callback<CreateProxyCandidateRequest>(req => createRequest = req);

                ProxyCandidates proxyCandidateData = new ProxyCandidates()
                {
                    PrxcBirthDate = DateTime.Today,
                    PrxcCategory = new List<string>() { "D" },
                    PrxcPersonId = new List<string>() { "CandId" },
                    PrxcScore = new List<int?>() { 90 },
                    ProxyMatchResultsEntityAssociation =
                        null,
                    PrxcEmailAddress = "TestCase@ellucian.edu",
                    PrxcEmailType = "PRI",
                    PrxcFirstName = "Joe",
                    PrxcFormerFirstName = "WasJoe",
                    PrxcFormerLastName = "WasTestCase",
                    PrxcFormerMiddleName = "WasMiddleName",
                    PrxcGender = "M",
                    PrxcGrantedPerms = new List<string>() { "SFAA" },
                    PrxcLastName = "TestCase",
                    PrxcMiddleName = "MiddleName",
                    PrxcPhone = "My Phone",
                    PrxcPhoneExtension = "My Extension",
                    PrxcPhoneType = "CP",
                    PrxcPrefix = "Mr.",
                    PrxcProxySubject = "0005141",
                    PrxcRelationType = "P",
                    PrxcSsn = "987654321",
                    PrxcSuffix = "Jr.",
                    Recordkey = "ProxyCandidatesId",
                };
                dataReaderMock.Setup<Task<ProxyCandidates>>(accessor =>
                    accessor.ReadRecordAsync<ProxyCandidates>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyCandidateData);

                var result = await proxyRepository.PostProxyCandidateAsync(defCandidate);

            }

        }

        [TestClass]
        public class ProxyRepository_GetUserProxySubjectsAsync : ProxyRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyRepository_GetUserProxySubjectsAsync_NullId()
            {
                var principals = await proxyRepository.GetUserProxySubjectsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProxyRepository_GetUserProxySubjectsAsync_EmptyId()
            {
                var principals = await proxyRepository.GetUserProxySubjectsAsync(string.Empty);
            }

            [TestMethod]
            public async Task ProxyRepository_GetUserProxySubjectsAsync_NoActivePermissions()
            {
                dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess>>>(accessor =>
                    accessor.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess>());
                proxyRepository = new ProxyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                var principals = await proxyRepository.GetUserProxySubjectsAsync("0003316");
                // If no active permissions then no principals returned.
                Assert.IsTrue(principals.Count() == 0);
            }


            [TestMethod]
            public async Task ProxyRepository_GetUserProxySubjectsAsync_Valid()
            {
                Collection<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess> proxyAccesses = new Collection<ProxyAccess>()
                {
                    new Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess()
                    {
                        Recordkey = "32",
                        PracProxyWebUser = "0003316",
                        PracPrincipalWebUser = "0003315",
                        PracBeginDate = DateTime.Parse("08/21/2015"),
                        PracEndDate = null,
                        PracReauthReqDate = null,
                        PracProxyAccessPermission = "SFMAP",
                        PracDisclosureReleaseDoc = null,
                        PracProxyApprovalEmail = null,
                        ProxyAccessChgdate = DateTime.Parse("08/21/2015")
                    },
                    new Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess()
                    {
                        Recordkey = "33",
                        PracProxyWebUser = "0003316",
                        PracPrincipalWebUser = "0003315",
                        PracBeginDate = DateTime.Parse("08/21/2015"),
                        PracEndDate = null,
                        PracReauthReqDate = null,
                        PracProxyAccessPermission = "SFAA",
                        PracDisclosureReleaseDoc = null,
                        PracProxyApprovalEmail = null,
                        ProxyAccessChgdate = DateTime.Parse("08/21/2015")
                    },
                    new Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess()
                    {
                        Recordkey = "33",
                        PracProxyWebUser = "0003316",
                        PracPrincipalWebUser = "0003317",
                        PracBeginDate = DateTime.Parse("08/21/2015"),
                        PracEndDate = null,
                        PracReauthReqDate = null,
                        PracProxyAccessPermission = "SFAA",
                        PracDisclosureReleaseDoc = null,
                        PracProxyApprovalEmail = null,
                        ProxyAccessChgdate = DateTime.Parse("08/21/2015")
                    }
                };

                dataReaderMock.Setup<Task<Collection<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess>>>(accessor =>
                    accessor.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.ProxyAccess>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proxyAccesses);
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                var principals = await proxyRepository.GetUserProxySubjectsAsync("0003316");

                Assert.IsTrue(principals.Count() == 2);
                var principal1 = principals.Where(p => p.Id == "0003315").First();
                var principal2 = principals.Where(p => p.Id == "0003317").First();
                Assert.IsTrue(principal1.Permissions[0].ProxyWorkflowCode == "SFMAP");
                Assert.IsTrue(principal1.Permissions[1].ProxyWorkflowCode == "SFAA");
                Assert.IsTrue(principal2.Permissions[0].ProxyWorkflowCode == "SFAA");

            }
        }

        private void BuildProxyWorkflows()
        {
            proxyWorkflows = new Collection<ProxyAccessPerms>()
                {
                    new ProxyAccessPerms()
                    {
                        Recordkey = "SFAA",
                        PrcpDescription = "Account Activity",
                        PrcpGroup = "SF",
                        PrcpEnabled = "Y",
                        PrcpWorklistCatSpecProc = ""
                    },
                    new ProxyAccessPerms()
                    {
                        Recordkey = "SFMAP",
                        PrcpDescription = "Make a Payment",
                        PrcpGroup = "SF",
                        PrcpEnabled = "N",
                        PrcpWorklistCatSpecProc = ""
                    }

                };

            MockRecordsAsync("PROXY.ACCESS.PERMS", proxyWorkflows);
        }

        private void BuildEmployeeProxyWorkflows()
        {
            proxyWorkflows = new Collection<ProxyAccessPerms>()
                {
                    new ProxyAccessPerms()
                    {
                        Recordkey = "TMTA",
                        PrcpDescription = "Time Approval",
                        PrcpGroup = "EM",
                        PrcpEnabled = "Y",
                        PrcpWorklistCatSpecProc = "SSHRTA"
                    }

                };

            MockRecordsAsync("PROXY.ACCESS.PERMS", proxyWorkflows);
        }

        private void BuildProxyGroups()
        {
            proxyGroups = new ApplValcodes()
            {
                Recordkey = "PROXY.ACCESS.GROUPS",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "SF",
                        ValExternalRepresentationAssocMember = "Student Finance"
                    }
                }
            };
            MockRecordAsync<ApplValcodes>("UT.VALCODES", proxyGroups);
        }

        private void BuildEmployeeProxyGroups()
        {
            proxyGroups = new ApplValcodes()
            {
                Recordkey = "PROXY.ACCESS.GROUPS",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "EM",
                        ValExternalRepresentationAssocMember = "Employee Proxy"
                    }
                }
            };
            MockRecordAsync<ApplValcodes>("UT.VALCODES", proxyGroups);
        }

        private ProxyRepository BuildValidRepository()
        {
            var proxyDefaults = new ProxyDefaults()
            {
                Recordkey = "PROXY.DEFAULTS",
                PrxdProxyEnabled = "Y",
                PrxdDisclosureReleaseDoc = "RELEASEDOC",
                PrxdProxyEmailDocument = "EMAILDOC",
                PrxdDisclosureReleaseText = "This is some text.",
                PrxdRelationships = new List<string>() { "PAR", "EMP", "SPO" },
                PrxdAddNewProxyAllowed = "Y",
                PrxdDemographicsEntityAssociation = new List<ProxyDefaultsPrxdDemographics>()
                {
                    new ProxyDefaultsPrxdDemographics() { PrxdDemoElementsAssocMember = "ABC", PrxdDemoDescsAssocMember = "Field 1", PrxdDemoRqmtAssocMember = "R" },
                    new ProxyDefaultsPrxdDemographics() { PrxdDemoElementsAssocMember = "DEF", PrxdDemoDescsAssocMember = "Field 2", PrxdDemoRqmtAssocMember = "O" },
                    new ProxyDefaultsPrxdDemographics() { PrxdDemoElementsAssocMember = "GHI", PrxdDemoDescsAssocMember = "Field 3", PrxdDemoRqmtAssocMember = "N" },
                },
                PrxdGrantAccessText = "This is some text displayed above the Add a Proxy section.",
                PrxdHeaderText = "This is some header text for the View/Grant Third Party Access view.",
                PrxdReauthorizationText = "This is some reauthorization text."
            };
            MockRecordAsync<ProxyDefaults>("UT.PARMS", proxyDefaults);

            proxyGroups = new ApplValcodes()
            {
                Recordkey = "PROXY.ACCESS.GROUPS",
                ValsEntityAssociation = new List<ApplValcodesVals>()
                {
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "SF",
                        ValExternalRepresentationAssocMember = "Student Finance"
                    },
                    new ApplValcodesVals()
                    {
                        ValInternalCodeAssocMember = "EM",
                        ValExternalRepresentationAssocMember = "Employee Proxy"
                    }
                }
            };
            MockRecordAsync<ApplValcodes>("UT.VALCODES", proxyGroups);

            proxyWorkflows = new Collection<ProxyAccessPerms>()
                {
                    new ProxyAccessPerms()
                    {
                        Recordkey = "SFAA",
                        PrcpDescription = "Account Activity",
                        PrcpGroup = "SF",
                        PrcpEnabled = "Y",
                        PrcpWorklistCatSpecProc = ""
                    },
                    new ProxyAccessPerms()
                    {
                        Recordkey = "SFMAP",
                        PrcpDescription = "Make a Payment",
                        PrcpGroup = "SF",
                        PrcpEnabled = "N",
                        PrcpWorklistCatSpecProc = ""
                    },
                    new ProxyAccessPerms()
                    {
                        Recordkey = "TMTA",
                        PrcpDescription = "Time Approval",
                        PrcpGroup = "EM",
                        PrcpEnabled = "Y",
                        PrcpWorklistCatSpecProc = "SSHRTA"
                    }

                };
            MockRecordsAsync("PROXY.ACCESS.PERMS", proxyWorkflows);

            var proxyAccesses = new Collection<ProxyAccess>()
            {
                new ProxyAccess()
                {
                    Recordkey = "1",
                    PracPrincipalWebUser = "0003900",
                    PracProxyWebUser = "0004100",
                    PracBeginDate = DateTime.Today,
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFMAP"
                },
                new ProxyAccess()
                {
                    Recordkey = "2",
                    PracPrincipalWebUser = "0003900",
                    PracProxyWebUser = "0004100",
                    PracBeginDate = DateTime.Today,
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFAA"
                },
                new ProxyAccess()
                {
                    Recordkey = "32",
                    PracProxyWebUser = "0003316",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/21/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFMAP",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "33",
                    PracProxyWebUser = "0003316",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/21/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFAA",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "34",
                    PracProxyWebUser = "0004000",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/25/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFMAP",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "35",
                    PracProxyWebUser = "0004000",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/25/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFAA",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "36",
                    PracProxyWebUser = "0004000",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/25/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "TMTA",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "37",
                    PracProxyWebUser = "0003316",
                    PracPrincipalWebUser = "0003315",
                    PracBeginDate = DateTime.Parse("08/21/2015"),
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "TMTA",
                    PracDisclosureReleaseDoc = null,
                    PracProxyApprovalEmail = null
                },
                new ProxyAccess()
                {
                    Recordkey = "38",
                    PracPrincipalWebUser = "0003900",
                    PracProxyWebUser = "0004100",
                    PracBeginDate = DateTime.Today,
                    PracEndDate = null,
                    PracReauthReqDate = null,
                    PracProxyAccessPermission = "SFAA"
                },
            };
            MockRecordsAsync<ProxyAccess>("PROXY.ACCESS", proxyAccesses);

            // Construct proxy repository
            proxyRepository = new ProxyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return proxyRepository;
        }
    }
}