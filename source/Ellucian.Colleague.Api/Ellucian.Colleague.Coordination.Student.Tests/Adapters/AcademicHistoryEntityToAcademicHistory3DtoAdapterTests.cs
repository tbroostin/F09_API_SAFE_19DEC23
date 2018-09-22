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
    public class AcademicHistoryEntityToAcademicHistory3DtoAdapterTests
    {
        Ellucian.Colleague.Domain.Student.Entities.AcademicHistory academicHistoryEntity;
        AcademicHistoryEntityToAcademicHistory3DtoAdapter adapter;
        IAdapterRegistry adapterRegistry;

        [TestInitialize]
        public async void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();

            // mock up various adapters
            var academicTermAdapter = new AcademicTermEntityAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Dtos.Student.AcademicTerm>()).Returns(academicTermAdapter);

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

            var gradeRestrictionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>()).Returns(gradeRestrictionAdapter);

            var academicCredits = (await new TestAcademicCreditRepository().GetAsync()).Where(a => a.SubjectCode == "MATH");
            var gradeRestriction = new Ellucian.Colleague.Domain.Student.Entities.GradeRestriction(true);
            academicHistoryEntity = new Domain.Student.Entities.AcademicHistory(academicCredits, gradeRestriction, null);

            adapter = new AcademicHistoryEntityToAcademicHistory3DtoAdapter(adapterRegistry, loggerMock.Object);
        }

        [TestMethod]
        public void AcademicHistoryEntity3Adapter_SuccessfullyConvertsToDto()
        {
            AcademicHistory3 academicHistoryDto = adapter.MapToType(academicHistoryEntity);
            Assert.IsTrue(academicHistoryDto.GradeRestriction.IsRestricted);
            Assert.IsTrue(academicHistoryDto.AcademicTerms.Count() > 0);
        }
    }
}
