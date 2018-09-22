// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentFinancialAidAwardServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentFinancialAidAwards : CurrentUserSetup
        {   
            Mock<IStudentFinancialAidAwardRepository> awardRepositoryMock;
            Mock<IFinancialAidFundRepository> fundRepositoryMock;
            Mock<IStudentFinancialAidOfficeRepository> officeRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            Mock<ITermRepository> termRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<ICommunicationRepository> communicationRepositoryMock;
            Mock<ICurrentUserFactory> userFactoryMock;
            ICurrentUserFactory userFactory;
            Mock<IRoleRepository> roleRepoMock;
            private ILogger logger;

            StudentFinancialAidAwardService studentFinancialAidAwardService;

            IEnumerable<StudentFinancialAidAward> aidAwardEntities;
            IEnumerable<Dtos.StudentFinancialAidAward> aidAwardDtos;
            IEnumerable<Dtos.StudentFinancialAidAward2> aidAward2Dtos;

            IEnumerable<FinancialAidFundCategory> financialCategoryEntities;
            IEnumerable<Term> termEntities;
            IEnumerable<FinancialAidFund> aidFundEntities;
            IEnumerable<FinancialAidYear> aidYearEntities;
            IEnumerable<FinancialAidAwardPeriod> awardPeriodEntities;
            IEnumerable<string> notAwardCategories;

            Tuple<IEnumerable<StudentFinancialAidAward>, int> aidAwardEntitiesTuple;
            Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int> aidAwardDtoTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionViewAnyApplication;

            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");


            [TestInitialize]
            public void Initialize()
            {
                awardRepositoryMock = new Mock<IStudentFinancialAidAwardRepository>();
                fundRepositoryMock = new Mock<IFinancialAidFundRepository>();
                officeRepositoryMock = new Mock<IStudentFinancialAidOfficeRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                communicationRepositoryMock = new Mock<ICommunicationRepository>();                
                personRepositoryMock = new Mock<IPersonRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                userFactoryMock = new Mock<ICurrentUserFactory>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                userFactory = userFactoryMock.Object;
                userFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentFinancialAidAwards);
                personRole.AddPermission(permissionViewAnyApplication);

                BuildData();
                BuildMocks();

                studentFinancialAidAwardService = new StudentFinancialAidAwardService(adapterRegistryMock.Object,
                    awardRepositoryMock.Object, fundRepositoryMock.Object, personRepositoryMock.Object, referenceRepositoryMock.Object, officeRepositoryMock.Object,
                    communicationRepositoryMock.Object, termRepositoryMock.Object, baseConfigurationRepository, userFactory, roleRepoMock.Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                awardRepositoryMock = null;
                fundRepositoryMock = null;
                officeRepositoryMock = null;
                adapterRegistryMock = null;
                referenceRepositoryMock = null;
                termRepositoryMock = null;
                personRepositoryMock = null;
                communicationRepositoryMock = null;
                userFactoryMock = null;
                userFactory = null;
                roleRepoMock = null;
                logger = null;
                studentFinancialAidAwardService = null;
                aidAwardEntities = null;
                aidAwardDtos = null;
                aidAward2Dtos = null;
                financialCategoryEntities = null;
                termEntities = null;
                aidFundEntities = null;
                aidYearEntities = null;
                awardPeriodEntities = null;
                notAwardCategories = null;
                aidAwardEntitiesTuple = null;
                aidAwardDtoTuple = null;
                permissionViewAnyApplication = null;
            }

            private void BuildData()
            {
                //Dtos
                aidAwardDtos = new List<Dtos.StudentFinancialAidAward>() 
                {
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),    
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873", 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        AwardFund = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.StudentFinancialAidAward()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        AidYear = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                        AwardFund = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                aidAward2Dtos = new List<Dtos.StudentFinancialAidAward2>() 
                {
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323"),    
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        AwardFund = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873", 
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        AwardFund = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.StudentFinancialAidAward2()
                    {
                        Student = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        AidYear = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"),
                        AwardFund = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };

                //Entities
                aidAwardEntities = new List<StudentFinancialAidAward>() 
                {
                    new StudentFinancialAidAward("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "CODE1", "CODE1", "CODE1")
                    {
                        AwardHistory = new List<StudentAwardHistoryByPeriod>()
                        {
                            new StudentAwardHistoryByPeriod() 
                            {
                                AwardPeriod = "CODE1",
                                Status = "A",
                                StatusDate = new DateTime(2016, 02, 01),
                                Amount = (decimal) 100000000
                            },
                            new StudentAwardHistoryByPeriod() 
                            {
                                AwardPeriod = "CODE1",
                                Status = "A",
                                StatusDate = new DateTime(2015, 02, 02),
                                Amount = (decimal) 100000
                            }
                        }
                    },
                    new StudentFinancialAidAward("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "CODE2", "CODE2", "CODE2")
                    {
                    },
                    new StudentFinancialAidAward("bf67e156-8f5d-402b-8101-81b0a2796873", "CODE3", "CODE3", "CODE3")
                    {
                    },
                    new StudentFinancialAidAward("0111d6ef-5a86-465f-ac58-4265a997c136", "CODE4", "CODE4", "CODE4")
                    { 
                        AwardHistory = new List<StudentAwardHistoryByPeriod>()
                        {
                            new StudentAwardHistoryByPeriod() 
                            {
                                AwardPeriod = "CODE4",
                                Status = "A",
                                StatusDate = new DateTime(2016, 02, 02),
                                Amount = (decimal) 100000,
                                StatusChanges = new List<StudentAwardHistoryStatus>() 
                                {
                                    new StudentAwardHistoryStatus()
                                    {
                                        Status = "R",
                                        StatusDate = new DateTime(2016, 03, 02),
                                        Amount = (decimal) 3029023
                                    }
                                }
                            },
                            new StudentAwardHistoryByPeriod() 
                            {
                                AwardPeriod = "CODE4",
                                Status = "A",
                                StatusDate = new DateTime(2015, 02, 02),
                                Amount = (decimal) 100000,
                                StatusChanges = new List<StudentAwardHistoryStatus>() 
                                {
                                    new StudentAwardHistoryStatus()
                                    {
                                        Status = "R",
                                        StatusDate = new DateTime(2015, 03, 02),
                                        Amount = (decimal) 3029023
                                    }
                                }
                            }
                        }
                    },
                };

                //FinancialFundCategories
                financialCategoryEntities = new List<FinancialAidFundCategory>() 
                {
                    new FinancialAidFundCategory("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new FinancialAidFundCategory("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2"),
                    new FinancialAidFundCategory("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3"),
                    new FinancialAidFundCategory("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4")
                };

                //Terms
                termEntities = new List<Domain.Student.Entities.Term>() 
                {
                    new Term("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", new DateTime(2016, 02, 01), new DateTime(2016, 03, 01), 2016, 1, false, false, "WINTER", false),
                    new Term("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", new DateTime(2016, 03, 01), new DateTime(2016, 04, 01), 2016, 2, false, false, "WINTER", false),
                    new Term("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", new DateTime(2016, 04, 01), new DateTime(2016, 05, 01), 2016, 3, false, false, "WINTER", false),
                    new Term("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", new DateTime(2016, 05, 01), new DateTime(2016, 06, 01), 2016, 4, false, false, "WINTER", false)
                };

                //FinancialAidFunds
                aidFundEntities = new List<FinancialAidFund>() 
                {
                    new FinancialAidFund("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1"),
                    new FinancialAidFund("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE2", "DESC2"),
                    new FinancialAidFund("0ac28907-5a9b-4102-a0d7-5d3d9c585512", "CODE3", "DESC3"),
                    new FinancialAidFund("bb6c261c-3818-4dc3-b693-eb3e64d70d8b", "CODE4", "DESC4")
                };

                //FinancialAidYears
                aidYearEntities = new List<FinancialAidYear>() 
                {
                    new FinancialAidYear("e0c0c94c-53a7-46b7-96c4-76b12512c323", "CODE1", "DESC1", "STATUS1"),
                    new FinancialAidYear("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff", "CODE2", "DESC2", "STATUS2"),
                    new FinancialAidYear("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff", "CODE3", "DESC3", "STATUS3"),
                    new FinancialAidYear("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE4", "DESC4", "STATUS4")
                };

                //FinancialAidAwardPeriods
                awardPeriodEntities = new List<FinancialAidAwardPeriod>() 
                {
                    new FinancialAidAwardPeriod("b90812ee-b573-4acb-88b0-6999a050be4f", "CODE1", "DESC1", "STATUS1")
                    {
                        AwardTerms = new List<string>() 
                        {
                            "CODE1"
                        }
                    },
                    new FinancialAidAwardPeriod("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52", "CODE2", "DESC2", "STATUS2"),
                    new FinancialAidAwardPeriod("abe5524b-6704-4f09-b858-763ee2ab5fe4", "CODE3", "DESC3", "STATUS3"),
                    new FinancialAidAwardPeriod("2158ad73-3416-467b-99d5-1b7b92599389", "CODE4", "DESC4", "STATUS4")
                };

                //NotAwardedCategories
                notAwardCategories = new List<string>()
                {
                    "P", "E"
                };

                //Tuple
                aidAwardEntitiesTuple = new Tuple<IEnumerable<StudentFinancialAidAward>, int>(aidAwardEntities, aidAwardEntities.Count());
                aidAwardDtoTuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(aidAwardDtos, aidAwardDtos.Count());
            }

            private void BuildMocks()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                awardRepositoryMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(aidAwardEntities.FirstOrDefault());                
                referenceRepositoryMock.Setup(i => i.GetFinancialAidFundCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(financialCategoryEntities);
                fundRepositoryMock.Setup(i => i.GetFinancialAidFundsAsync(It.IsAny<bool>())).ReturnsAsync(aidFundEntities);
                referenceRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(aidYearEntities);
                awardRepositoryMock.Setup(i => i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(aidAwardEntitiesTuple);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("CODE1")).ReturnsAsync("d190d4b5-03b5-41aa-99b8-b8286717c956");
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("CODE2")).ReturnsAsync("d190d4b5-03b5-41aa-99b8-b8286717c956");
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("CODE3")).ReturnsAsync("cecdce5a-54a7-45fb-a975-5392a579e5bf");
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("CODE4")).ReturnsAsync("cecdce5a-54a7-45fb-a975-5392a579e5bf");
                referenceRepositoryMock.Setup(i => i.GetFinancialAidAwardPeriodsAsync(It.IsAny<bool>())).ReturnsAsync(awardPeriodEntities);
                referenceRepositoryMock.Setup(i => i.GetHostCountryAsync()).ReturnsAsync("USA");
                awardRepositoryMock.Setup(i => i.GetNotAwardedCategoriesAsync()).ReturnsAsync(notAwardCategories);
                termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(termEntities);
                termRepositoryMock.Setup(i => i.GetAsync()).ReturnsAsync(termEntities);
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardService__GetStudentFinancialAidAwardsAsync()
            {
                var studentFinancialAidAwards = await studentFinancialAidAwardService.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>());

                Assert.IsNotNull(studentFinancialAidAwards);

                foreach (var actual in studentFinancialAidAwards.Item1)
                {
                    var expected = aidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear.Id, actual.AidYear.Id);
                    Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                    Assert.AreEqual(expected.AwardFund.Id, actual.AwardFund.Id);
                }
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardService__GetStudentFinancialAidAwardByGuidAsync()
            {
                var id = "0111d6ef-5a86-465f-ac58-4265a997c136";
                var aidAwardEntity = aidAwardEntities.FirstOrDefault(i => i.Guid.Equals(id));
                awardRepositoryMock.Setup(i => i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(aidAwardEntity);

                var actual = await studentFinancialAidAwardService.GetByIdAsync(id, false);

                Assert.IsNotNull(actual);

                var expected = aidAwardDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear.Id, actual.AidYear.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.AwardFund.Id, actual.AwardFund.Id);
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardService__GetStudentFinancialAidAwards2Async()
            {
                var studentFinancialAidAwards = await studentFinancialAidAwardService.Get2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>());

                Assert.IsNotNull(studentFinancialAidAwards);

                foreach (var actual in studentFinancialAidAwards.Item1)
                {
                    var expected = aidAward2Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AidYear.Id, actual.AidYear.Id);
                    Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                    Assert.AreEqual(expected.AwardFund.Id, actual.AwardFund.Id);
                }
            }
        }
    }
}
