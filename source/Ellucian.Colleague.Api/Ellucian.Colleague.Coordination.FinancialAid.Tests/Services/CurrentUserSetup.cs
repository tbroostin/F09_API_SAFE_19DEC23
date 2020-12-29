//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    public abstract class CurrentUserSetup
    {
        public class StudentUserFactory : ICurrentUserFactory
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
                        UserName = "Student",
                        Roles = new List<string>() { "FINANCIAL AID COUNSELOR" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class StudentUserFactoryWithProxy : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Gregory",
                        PersonId = "0013914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0003914",
                            Permissions = new List<string>() { "FAAL"}
                        }
                    });
                }
            }
        }

        public class StudentUserFactoryWithDifferentProxy : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Gregory",
                        PersonId = "0013914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0003915"
                        }
                    });
                }
            }
        }

        protected Role counselorRole = new Role(26, "FINANCIAL AID COUNSELOR");

        public class CounselorUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "321",
                        Name = "Joe",
                        PersonId = "0718745",
                        SecurityToken = "9USSD9d9sdD.DS9983",
                        SessionTimeout = 30,
                        UserName = "JoeCounselor",
                        Roles = new List<string>() { "FINANCIAL AID COUNSELOR" },
                        SessionFixationId = "xyz987"
                    });
                }
            }
        }

        public class StudentFinAidUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000028",
                        ControlId = "123",
                        Name = "Bernard",
                        SecurityToken = "954",
                        SessionTimeout = 30,
                        UserName = "FixAssetsUser",
                        Roles = new List<string>() { "VIEW.STU.FA.ACAD.PROGRESS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
