// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Security;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Tests.UserFactories
{
    /// <summary>
    /// Define a user factory to simulate the user
    /// </summary>
    public abstract class StudentUserFactory
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
                        Roles = new List<string>() { "Student" },
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
                        PersonId = "1",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.W2", "VIEW.1095C", "VIEW.1098", "VIEW.T4", "VIEW.T4A", "VIEW.T2202A", "VIEW.EMPLOYEE.W2", "VIEW.EMPLOYEE.1095C", "VIEW.STUDENT.1098", "VIEW.EMPLOYEE.T4", "VIEW.RECIPIENT.T4A", "VIEW.STUDENT.T2202A" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class HousingAssignmentUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentHA",
                        Roles = new List<string>() { "VIEW.ROOM.ASSIGNMENT", "UPDATE.ROOM.ASSIGNMENT" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class SectionInstructorsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.SECTION.INSTRUCTORS", "UPDATE.SECTION.INSTRUCTORS", "DELETE.SECTION.INSTRUCTORS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class StudentCourseTransferUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.STUDENT.COURSE.TRANSFERS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class StudentTranscriptGradesUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.STUDENT.TRANSCRIPT.GRADES" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class FacultyUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "0000011",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Faculty",
                        Roles = new List<string>() { "VIEW.PER.EXTERNAL.EDUCATION", "VIEW.EXTERNAL.EDUCATION",
                            "VIEW.EXTERNAL.EDUC.CREDENTIALS", "VIEW.PER.EXT.EDUC.CREDENTIAL" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class HousingRequestUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "0000011",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "HousingRequestUser",
                        Roles = new List<string>() { "VIEW.HOUSING.REQS", "UPDATE.HOUSING.REQS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class AdmissionDecisionUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "0000011",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Faculty",
                        Roles = new List<string>() { "VIEW.ADMISSION.DECISIONS", "UPDATE.ADMISSION.DECISIONS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class AdmissionApplicationSupportingItemsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentHA",
                        Roles = new List<string>() { "VIEW.APPL.SUPPORTING.ITEMS", "UPDATE.APPL.SUPPORTING.ITEMS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class AdmissionApplicationUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.APPLICATIONS", "UPDATE.APPLICATIONS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

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

        public class GradeUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentGradeUser",
                        Roles = new List<string>() { "VIEW.STUDENT.INFORMATION"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class CampusInvolvementsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.CAMPUS.ORG.MEMBERS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
