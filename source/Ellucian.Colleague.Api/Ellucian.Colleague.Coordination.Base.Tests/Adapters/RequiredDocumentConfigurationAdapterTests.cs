// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class RequiredDocumentConfigurationAdapterTests
    {
        bool SuppressInstance;
        WebSortField PrimarySortField;
        WebSortField SecondarySortField;
        string TextForBlankStatus;
        string TextForBlankDueDate;
        RequiredDocumentConfiguration RequiredDocumentConfigurationDto;
        Ellucian.Colleague.Domain.Base.Entities.RequiredDocumentConfiguration RequiredDocumentConfigurationEntity;
        RequiredDocumentConfigurationAdapter RequiredDocumentConfigurationAdapter;

        [TestInitialize]
        public void Initialize()
        {
            SuppressInstance = false;
            PrimarySortField = WebSortField.Status;
            SecondarySortField = WebSortField.OfficeDescription;
            TextForBlankStatus = "";
            TextForBlankDueDate = "";

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            RequiredDocumentConfigurationAdapter = new RequiredDocumentConfigurationAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var webSortFieldAdapter = new AutoMapperAdapter<Domain.Base.Entities.WebSortField, WebSortField>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.WebSortField, WebSortField>()).Returns(webSortFieldAdapter);

            RequiredDocumentConfigurationEntity = new Domain.Base.Entities.RequiredDocumentConfiguration(false, Domain.Base.Entities.WebSortField.Status, Domain.Base.Entities.WebSortField.OfficeDescription, "", "");

            RequiredDocumentConfigurationDto = RequiredDocumentConfigurationAdapter.MapToType(RequiredDocumentConfigurationEntity);
        }

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_SuppressInstance()
        {
            Assert.AreEqual(SuppressInstance, RequiredDocumentConfigurationDto.SuppressInstance);
        }

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_PrimarySortField()
        {
            Assert.AreEqual(PrimarySortField, RequiredDocumentConfigurationDto.PrimarySortField);
        }

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_SecondarySortField()
        {
            Assert.AreEqual(SecondarySortField, RequiredDocumentConfigurationDto.SecondarySortField);
        }

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_TextForBlankStatus()
        {
            Assert.AreEqual(TextForBlankStatus, RequiredDocumentConfigurationDto.TextForBlankStatus);
        }

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_TestForBlankDueDate()
        {
            Assert.AreEqual(TextForBlankDueDate, RequiredDocumentConfigurationDto.TextForBlankDueDate);
        }
    }
}
