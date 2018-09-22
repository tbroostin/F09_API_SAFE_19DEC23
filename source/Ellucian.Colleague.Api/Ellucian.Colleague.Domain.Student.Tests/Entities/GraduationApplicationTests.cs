using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GraduationApplicationTests
    {
        [TestClass]
        public class GraduationApplication_Constructor
        {
            [TestMethod]
            public void GraduationApplicationEmpty()
            {
                var app = new GraduationApplication("Id","studentId","programCode");
                Assert.AreEqual("Id", app.Id);
                Assert.AreEqual("studentId", app.StudentId);
                Assert.AreEqual("programCode", app.ProgramCode);
                Assert.IsNull(app.AttendingCommencement);
                Assert.IsNull(app.GraduationTerm);
                Assert.IsNull(app.IncludeNameInProgram);
                Assert.IsNull(app.WillPickupDiploma);
                Assert.AreEqual(0, app.NumberOfGuests);
            }
        }
    }
}
