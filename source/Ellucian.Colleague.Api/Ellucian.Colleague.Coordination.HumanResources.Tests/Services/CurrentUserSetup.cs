/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Security;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    public abstract class CurrentUserSetup
    {
        protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

        public class EmployeeUserFactory : ICurrentUserFactory
        {

            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "EDIT_DD", "UPDATE.EMPLOYEE" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = ProxyClaim
                    });
                }
            }
            public EmployeeUserFactory() { }
            public ProxySubjectClaims ProxyClaim { get; set; }
            public EmployeeUserFactory(ProxySubjectClaims proxyClaim)
            {
                ProxyClaim = proxyClaim;
            }
        }

        public class PayScalesUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "VIEW.PAY.SCALES" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

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
                        Roles = new List<string>() { "Faculty", "VIEW.INSTITUTION.POSITION", "CREATE.UPDATE.INSTITUTION.JOB" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }

        public class PersonDecuctionArrangementsUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "CREATE.PAYROLL.DEDUCTION.ARRANGEMENTS" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }

        public class PersonEmployeeLeaveTransactionUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "VIEW.EMPL.LEAVE.TRANSACTIONS" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }

        public class PersonEmploymentProficienciesUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "VIEW.PERSON.EMPL.PROFICIENCIES" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }

        public class PersonEmployeeLeavePlansUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "VIEW.EMPL.LEAVE.PLANS" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }

        public class SupervisorUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Employee",
                        Roles = new List<string>() { "TIME MANAGEMENT SUPERVISOR" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = ProxyClaim
                    });
                }
            }

            public SupervisorUserFactory() { }
            public ProxySubjectClaims ProxyClaim { get; set; }
            public SupervisorUserFactory(ProxySubjectClaims proxyClaim)
            {
                ProxyClaim = proxyClaim;
            }
        }

        public class BenefitsEnrollmentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Natalie",
                        PersonId = "0014697",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Nataliegillon",
                        Roles = new List<string>() { "EMPLOYEE" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public class BenefitsEnrollmentDifferentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "124",
                        Name = "John",
                        PersonId = "0014698",
                        SecurityToken = "322",
                        SessionTimeout = 30,
                        UserName = "JohnDoe",
                        Roles = new List<string>() { "EMPLOYEE" },
                        SessionFixationId = "abc1234"
                    });
                }
            }        
        }

        public class GeneralEmployeeUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser {
                get {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "124",
                        Name = "John",
                        PersonId = "0014698",
                        SecurityToken = "322",
                        SessionTimeout = 30,
                        UserName = "JohnDoe",
                        Roles = new List<string>() { "EMPLOYEE", "VIEW.ORG.CHART"},
                        SessionFixationId = "abc1234"
                    });
                }
            }
        }
    }
}
