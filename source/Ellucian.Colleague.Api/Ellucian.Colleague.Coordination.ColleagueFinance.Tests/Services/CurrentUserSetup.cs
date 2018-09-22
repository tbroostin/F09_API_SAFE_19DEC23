/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
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
                        Roles = new List<string>() { "EDIT_DD" },
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
                        Roles = new List<string>() { "Faculty", "VIEW.INSTITUTION.POSITION", "UPDATE.VENDORS" },
                        SessionFixationId = "abc123",
                    });
                }
            }
        }
    }
}
