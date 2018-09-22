// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class TranscriptGroupingServiceTests
    {
        [TestClass]
        public class GetTranscriptGroupings
        {
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping> allTranscriptGroupings;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping, Ellucian.Colleague.Dtos.Student.TranscriptGrouping> transcriptGroupingDtoAdapter;
            private TranscriptGroupingService transcriptGroupingService;

            [TestInitialize]
            public void Initialize()
            {
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
                allTranscriptGroupings = new List<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping>();

                for (int i = 0; i < 10; i++)
                {
                    var tg = new Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping(i.ToString(), i.ToString(), i % 2 == 0 ? true : false);
                    allTranscriptGroupings.Add(tg);
                }

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                transcriptGroupingDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping, Ellucian.Colleague.Dtos.Student.TranscriptGrouping>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping, Ellucian.Colleague.Dtos.Student.TranscriptGrouping>()).Returns(transcriptGroupingDtoAdapter);

                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TranscriptGrouping>>(allTranscriptGroupings));
                
                transcriptGroupingService = new TranscriptGroupingService(adapterRegistryMock.Object, transcriptGroupingRepositoryMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transcriptGroupingRepository = null;
                allTranscriptGroupings = null;
                adapterRegistry = null;
                transcriptGroupingService = null;
            }

            [TestMethod]
            public async Task TranscriptGroupingService_GetSelectableTranscriptGroupings()
            {
                var transcriptGroupingDTOs = await transcriptGroupingService.GetSelectableTranscriptGroupingsAsync();

                Assert.AreEqual(allTranscriptGroupings.Where(x => x.IsUserSelectable == true).Count(), transcriptGroupingDTOs.Count());

                // for every transcript grouping in the set of domain entities, ensure a similar DTO exists
                foreach (var tg in allTranscriptGroupings.Where(x => x.IsUserSelectable == true))
                {
                    Assert.AreEqual(tg.Id, transcriptGroupingDTOs.Single(x => x.Id == tg.Id).Id);
                }
            }                
        }
    }
}
