// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
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
        Ellucian.Colleague.Domain.Base.Entities.RequiredDocumentCollectionMapping RequiredDocumentCollectionMappingEntity;
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
            var officeCodeCollectionAdapter = new AutoMapperAdapter<Domain.Base.Entities.OfficeCodeAttachmentCollection, OfficeCodeAttachmentCollection>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.OfficeCodeAttachmentCollection, OfficeCodeAttachmentCollection>()).Returns(officeCodeCollectionAdapter);
            var requiredDocumentCollectionMappingAdapter = new AutoMapperAdapter<Domain.Base.Entities.RequiredDocumentCollectionMapping, RequiredDocumentCollectionMapping>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.RequiredDocumentCollectionMapping, RequiredDocumentCollectionMapping>()).Returns(requiredDocumentCollectionMappingAdapter);

            RequiredDocumentCollectionMappingEntity = new Domain.Base.Entities.RequiredDocumentCollectionMapping() { RequestsWithoutOfficeCodeCollection = "NO_OFFICE_CODE", UnmappedOfficeCodeCollection = "UNMAPPED" };
            RequiredDocumentCollectionMappingEntity.AddOfficeCodeAttachment(new Domain.Base.Entities.OfficeCodeAttachmentCollection("OFFICEA", "COLLECTIONA"));
            RequiredDocumentConfigurationEntity = new Domain.Base.Entities.RequiredDocumentConfiguration()
            {
                SuppressInstance = false,
                PrimarySortField = Domain.Base.Entities.WebSortField.Status,
                SecondarySortField = Domain.Base.Entities.WebSortField.OfficeDescription,
                TextForBlankStatus = "",
                TextForBlankDueDate = "",
                RequiredDocumentCollectionMapping = RequiredDocumentCollectionMappingEntity
            };

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

        [TestMethod]
        public void RequiredDocumentConfigAdapterTests_RequiredDocumentCollectionMapping()
        {
            Assert.IsNotNull(RequiredDocumentConfigurationDto.RequiredDocumentCollectionMapping);
            var actualConfig = RequiredDocumentConfigurationDto.RequiredDocumentCollectionMapping;
            Assert.AreEqual(RequiredDocumentCollectionMappingEntity.RequestsWithoutOfficeCodeCollection, actualConfig.RequestsWithoutOfficeCodeCollection);
            Assert.AreEqual(RequiredDocumentCollectionMappingEntity.UnmappedOfficeCodeCollection, actualConfig.UnmappedOfficeCodeCollection);
            Assert.AreEqual(1, actualConfig.OfficeCodeMapping.Count);
            var expectedOfficeMap1 = RequiredDocumentCollectionMappingEntity.OfficeCodeMapping[0];
            var actualOfficeMap1 = actualConfig.OfficeCodeMapping[0];
            Assert.AreEqual(expectedOfficeMap1.OfficeCode, actualOfficeMap1.OfficeCode);
            Assert.AreEqual(expectedOfficeMap1.AttachmentCollection, actualOfficeMap1.AttachmentCollection);

        }
    }
}
