// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class RestrictionConfigurationAdapterTests
    {
        int start = 0;
        int end = 100;
        Domain.Base.Entities.AlertStyle style = Domain.Base.Entities.AlertStyle.Critical;



        RestrictionConfiguration rcDto;
        Ellucian.Colleague.Domain.Base.Entities.RestrictionConfiguration rcEntity;
        Domain.Base.Entities.SeverityStyleMapping ssmEntity;
        RestrictionConfigurationAdapter rcAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            rcAdapter = new RestrictionConfigurationAdapter(adapterRegistryMock.Object, loggerMock.Object);

            rcEntity = new Domain.Base.Entities.RestrictionConfiguration();
            ssmEntity = new Domain.Base.Entities.SeverityStyleMapping(start, end, style);

            rcEntity.AddItem(ssmEntity);
            rcDto = rcAdapter.MapToType(rcEntity);
        }

        [TestMethod]
        public void RestrictionConfigurationAdapterTest_Mapping_Start()
        {
            Assert.AreEqual(start, rcDto.Mapping[0].SeverityStart);
        }

        [TestMethod]
        public void RestrictionConfigurationAdapterTest_Mapping_End()
        {
            Assert.AreEqual(end, rcDto.Mapping[0].SeverityEnd);
        }

        [TestMethod]
        public void RestrictionConfigurationAdapterTest_Mapping_Style()
        {
            Assert.AreEqual(style.ToString(), rcDto.Mapping[0].Style.ToString());
        }
    }
}
