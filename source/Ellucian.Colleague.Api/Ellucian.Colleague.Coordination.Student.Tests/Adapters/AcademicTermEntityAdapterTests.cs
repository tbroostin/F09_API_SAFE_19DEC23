// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    [TestClass]
    public class AcademicTermEntityAdapterTests
    {
        Ellucian.Colleague.Domain.Student.Entities.AcademicTerm academicTermEntity;
        AcademicTermEntityAdapter adapter;
        IAdapterRegistry adapterRegistry;

        [TestInitialize]
        public async void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // mock up various adapters
            var academicCreditAdapter = new AcademicCreditEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Dtos.Student.AcademicCredit>()).Returns(academicCreditAdapter);

            var midtermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(midtermGradeAdapter);

            var gradingTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>()).Returns(gradingTypeAdapter);

            var replacedStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, Ellucian.Colleague.Dtos.Student.ReplacedStatus>()).Returns(replacedStatusAdapter);

            var replacementStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, Ellucian.Colleague.Dtos.Student.ReplacementStatus>()).Returns(replacementStatusAdapter);

            academicTermEntity = new Domain.Student.Entities.AcademicTerm();
            academicTermEntity.AcademicCredits = (await new TestAcademicCreditRepository().GetAsync()).Where(a=>a.SubjectCode == "MATH").ToList();
            academicTermEntity.ContinuingEducationUnits = 6m;
            academicTermEntity.Credits = 12m;
            academicTermEntity.GradePointAverage = 3.25m;
            academicTermEntity.TermId = "2014FA";

            adapter = new AcademicTermEntityAdapter(adapterRegistry, loggerMock.Object);
        }

        [TestMethod]
        public void AcademicTermEntityAdapter_ConvertsAllProperties()
        {
            AcademicTerm academicTermDto = adapter.MapToType(academicTermEntity);
            Assert.AreEqual(academicTermEntity.AcademicCredits.Count(), academicTermDto.AcademicCredits.Count());
            Assert.AreEqual(academicTermEntity.ContinuingEducationUnits, academicTermDto.ContinuingEducationUnits);
            Assert.AreEqual(academicTermEntity.Credits, academicTermDto.Credits);
            Assert.AreEqual(academicTermEntity.GradePointAverage, academicTermDto.GradePointAverage);
            Assert.AreEqual(academicTermEntity.TermId, academicTermDto.TermId);
            foreach (var academicCredit in academicTermEntity.AcademicCredits)
            {
                Assert.IsNotNull(academicTermDto.AcademicCredits.Where(a => a.Id == academicCredit.Id));
            }
        }
    }
}
