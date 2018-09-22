using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class SectionTextbookDtoToEntityAdapterTests
    {
        SectionTextbook sectionTextbookDto;
        SectionTextbookDtoToEntityAdapter sectionTextbookAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            sectionTextbookAdapter = new SectionTextbookDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            Book textbook = new Book()
            {
                Id = "Book1",
                Isbn = "Isbn",
                Title = "Title",
                Author = "Author",
                Publisher = "Publisher",
                Copyright = "Copyright",
                Edition = "Edition",
                IsActive = true,
                PriceUsed = 10m,
                Price = 20m,
                Comment = "Comment",
                ExternalComments = "External Comments",
                AlternateID1 = "altId1",
                AlternateID2 = "altId2",
                AlternateID3 = "altId3"
            };
            sectionTextbookDto = new SectionTextbook()
            {
                Textbook = textbook,
                SectionId = "SEC1",
                RequirementStatusCode = "R",
                Action = SectionBookAction.Add
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionTextbook_NullSource()
        {
            var textbookEntity = sectionTextbookAdapter.MapToType(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionTextbook_NullSourceTextbook()
        {
            sectionTextbookDto.Textbook = null;
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
        }

        [TestMethod]
        public void SectionTextbook_SectionId()
        {
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
            Assert.AreEqual(sectionTextbookDto.SectionId, textbookEntity.SectionId);
        }

        [TestMethod]
        public void SectionTextbook_RequirementStatus()
        {
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
            Assert.AreEqual(sectionTextbookDto.RequirementStatusCode, textbookEntity.RequirementStatusCode);
        }

        [TestMethod]
        public void SectionTextbook_AddAction()
        {
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
            Assert.AreEqual(Domain.Student.Entities.SectionBookAction.Add, textbookEntity.Action);
        }

        [TestMethod]
        public void SectionTextbook_UpdateAction()
        {
            sectionTextbookDto.Action = SectionBookAction.Update;
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
            Assert.AreEqual(Domain.Student.Entities.SectionBookAction.Update, textbookEntity.Action);
        }

        [TestMethod]
        public void SectionTextbook_RemoveAction()
        {
            sectionTextbookDto.Action = SectionBookAction.Remove;
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);
            Assert.AreEqual(Domain.Student.Entities.SectionBookAction.Remove, textbookEntity.Action);
        }

        [TestMethod]
        public void SectionTextbook_Textbook()
        {
            var textbookEntity = sectionTextbookAdapter.MapToType(sectionTextbookDto);

            Assert.IsInstanceOfType(textbookEntity.Textbook, typeof(Domain.Student.Entities.Book));

            Assert.AreEqual(sectionTextbookDto.Textbook.Id, textbookEntity.Textbook.Id);
            Assert.AreEqual(sectionTextbookDto.Textbook.Isbn, textbookEntity.Textbook.Isbn);
            Assert.AreEqual(sectionTextbookDto.Textbook.Title, textbookEntity.Textbook.Title);
            Assert.AreEqual(sectionTextbookDto.Textbook.Author, textbookEntity.Textbook.Author);
            Assert.AreEqual(sectionTextbookDto.Textbook.Publisher, textbookEntity.Textbook.Publisher);
            Assert.AreEqual(sectionTextbookDto.Textbook.Copyright, textbookEntity.Textbook.Copyright);
            Assert.AreEqual(sectionTextbookDto.Textbook.Edition, textbookEntity.Textbook.Edition);
            Assert.AreEqual(sectionTextbookDto.Textbook.IsActive, textbookEntity.Textbook.IsActive);
            Assert.AreEqual(sectionTextbookDto.Textbook.PriceUsed, textbookEntity.Textbook.PriceUsed);
            Assert.AreEqual(sectionTextbookDto.Textbook.Price, textbookEntity.Textbook.Price);
            Assert.AreEqual(sectionTextbookDto.Textbook.Comment, textbookEntity.Textbook.Comment);
            Assert.AreEqual(sectionTextbookDto.Textbook.ExternalComments, textbookEntity.Textbook.ExternalComments);
            Assert.AreEqual(sectionTextbookDto.Textbook.AlternateID1, textbookEntity.Textbook.AlternateID1);
            Assert.AreEqual(sectionTextbookDto.Textbook.AlternateID2, textbookEntity.Textbook.AlternateID2);
            Assert.AreEqual(sectionTextbookDto.Textbook.AlternateID3, textbookEntity.Textbook.AlternateID3);
        }
    }
}
