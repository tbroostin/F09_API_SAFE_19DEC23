// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Security;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Tests.UserFactories
{
    /// <summary>
    /// Define a user factory to simulate the user
    /// </summary>
    public abstract class GenericUserFactory
    {
        public class UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Student", "VIEW.ADDRESS", "UPDATE.ADDRESS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public class AddressUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "AddressUser",
                        Roles = new List<string>() { "VIEW.ADDRESS", "UPDATE.ADDRESS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public class TaxInformationUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.W2", "VIEW.1095C", "VIEW.1098", "VIEW.T4", "VIEW.T4A", "VIEW.T2202A", "VIEW.1099MISC", "VIEW.EMPLOYEE.W2", "VIEW.EMPLOYEE.1095C", "VIEW.STUDENT.1098", "VIEW.EMPLOYEE.T4", "VIEW.RECIPIENT.T4A", "VIEW.STUDENT.T2202A" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class PersonRelationshipUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "VIEW.PERSON.RELATIONSHIPS", "VIEW.NONPERSON.RELATIONSHIPS", "UPDATE.PERSON.RELATIONSHIPS", "DELETE.PERSON.RELATIONSHIPS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class PersonGuardianRelationshipUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "VIEW.PERSON.GUARDIAN" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class RelationshipUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "VIEW.RELATIONSHIP" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class PersonVisaUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "VIEW.PERSON.VISA", "UPDATE.PERSON.VISA" },
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
                        Name = "Greg",
                        PersonId = "0003943",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0003315"
                        }
                    });
                }
            }
        }

        protected Role financeAdminRole = new Role(25, "FINANCE ADMIN");

        public class FinanceAdminUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "FINANCE ADMIN" },
                        SessionFixationId = "xyz987"
                    });
                }
            }
        }
    }
}
