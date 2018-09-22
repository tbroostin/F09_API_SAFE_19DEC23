// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    public abstract class FinanceCoordinationTests
    {
        protected static string Id1 = "00012345";
        protected static string Id2 = "00012346";

        protected static ICurrentUser currentUser = new CurrentUser(CreateClaims(Id1));
        protected Role financeAdminRole = new Role(25, "Finance Admin");

        protected static Claims CreateClaims(string personId)
        {
            return new Claims()
            {
                ControlId = "123",
                Name = "Bob",
                PersonId = personId,
                SecurityToken = "124",
                SessionTimeout = 30,
                UserName = "bob",
                Roles = new List<string>() { "Finance Admin" },
                SessionFixationId = "abc123"
            };
        }

        public class CurrentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return currentUser;
                }
            }
        }

        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Johnny",
                        PersonId = "0000895",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0001233",
                            Permissions = new List<string>() { "SFAA", "SFMAP" },
                            FormattedName = "Parent"
                        }
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

        public class StudentUserFactoryWithDifferentProxy : ICurrentUserFactory
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
                            PersonId = "foo"
                        }
                    });
                }
            }
        }

        public class InvoiceUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Johnny",
                        PersonId = "0000895",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Invoice Creator" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class ViewAccountUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Johnny",
                        PersonId = "0000895",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "View Accounts" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public abstract class StudentUserTests
        {
            protected ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Johnny",
                        PersonId = "0000895",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public abstract class BasePaymentTests
        {
            protected Mock<IAccountActivityRepository> accountActivityRepoMock;
            protected Mock<IAccountsReceivableRepository> accountsReceivableRepoMock;
            protected Mock<IPaymentRepository> paymentRepoMock;
            protected Mock<IAccountDueRepository> accountRepoMock;
            protected Mock<IRoleRepository> roleRepoMock;
            protected ILogger logger = new Mock<ILogger>().Object;
            protected IAdapterRegistry adapterRegistry = new Mock<IAdapterRegistry>().Object;
            protected Role role = new Role(1, "Finance Admin");

            // Current user is Id1
            protected ICurrentUserFactory currentUserFactory = new CurrentUserFactory();
            protected ICurrentUser currentUser
            {
                get
                {
                    return currentUserFactory.CurrentUser;
                }
            }


            protected void DoInitialize()
            {
                paymentRepoMock = new Mock<IPaymentRepository>();
                accountRepoMock = new Mock<IAccountDueRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                accountActivityRepoMock = new Mock<IAccountActivityRepository>();
                accountsReceivableRepoMock = new Mock<IAccountsReceivableRepository>();
            }
        }
    }
}
