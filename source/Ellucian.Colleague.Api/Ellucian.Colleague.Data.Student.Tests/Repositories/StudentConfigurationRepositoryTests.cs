// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using System.Threading;
using System;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentConfigurationRepositoryTests
    {

        [TestClass]
        public class GraduationConfigurationTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            Collection<GraduationQuestions> graduationQuestionsResponseData;
            StwebDefaults stwebDefaults;
            Defaults defaults;
            DaDefaults daDefaults;
            StudentConfigurationRepository studentConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                dataAccessorMock = new Mock<IColleagueDataReader>();
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                loggerMock = new Mock<ILogger>();
                // Collection of data accessor responses
                graduationQuestionsResponseData = BuildGraduationQuestionsResponse();
                stwebDefaults = BuildStwebDefaultsResponse();
                defaults = BuildDefaultsResponse();
                daDefaults = BuildDaDefaultsResponse();
                studentConfigurationRepository = BuildValidStudentConfigurationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                graduationQuestionsResponseData = null;
                stwebDefaults = null;
                studentConfigurationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsKeyNotFoundExceptionIfDataReaderReturnsNull()
            {
                // Set up repo response for stwebDefaults
                StwebDefaults nullResponse = null;
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                try
                {
                    var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                }
                catch
                {

                    throw;
                }
            }

            [TestMethod]
            public async Task ReturnsEmptyListIfNoQuestionsReturned()
            {
                // Null repo response for graduation questions
                Collection<GraduationQuestions> nullResponse = null;
                dataAccessorMock.Setup<Task<Collection<GraduationQuestions>>>(acc => acc.BulkReadRecordAsync<GraduationQuestions>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(0, graduationConfiguration.ApplicationQuestions.Count());
            }

            [TestMethod]
            public async Task ReturnsValidConfiguration()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(12, graduationConfiguration.ApplicationQuestions.Count());
                Assert.AreEqual(3, graduationConfiguration.GraduationTerms.Count());
                Assert.AreEqual(stwebDefaults.StwebGradCapgownSizesUrl, graduationConfiguration.CapAndGownSizingLink);
                Assert.AreEqual(stwebDefaults.StwebGradCapgownUrl, graduationConfiguration.CapAndGownLink);
                Assert.AreEqual(stwebDefaults.StwebGradCommencementUrl, graduationConfiguration.CommencementInformationLink);
                Assert.AreEqual(stwebDefaults.StwebGradPhoneticUrl, graduationConfiguration.PhoneticSpellingLink);
                Assert.AreEqual(stwebDefaults.StwebGradDiffProgramUrl, graduationConfiguration.ApplyForDifferentProgramLink);
                Assert.AreEqual(stwebDefaults.StwebGradMaxGuests, graduationConfiguration.MaximumCommencementGuests);                
            }

            [TestMethod]
            public async Task ReturnsExpectedShowCapAndGownFlagValueTest()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                var showCapAndGown = !string.IsNullOrEmpty(stwebDefaults.StwebGradOvrCmcmtCapgown) && stwebDefaults.StwebGradOvrCmcmtCapgown.ToUpper() == "Y";
                Assert.AreEqual(showCapAndGown, graduationConfiguration.OverrideCapAndGownDisplay);
            }

            [TestMethod]
            public async Task ReturnsExpectedShowCapAndGownFlagValueWithLittleyTest()
            {
                stwebDefaults.StwebGradOvrCmcmtCapgown = "y";
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));

                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();

                Assert.IsTrue(graduationConfiguration.OverrideCapAndGownDisplay);

            }

            [TestMethod]
            public async Task ReturnsExpectedShowCapAndGownFlagValueWithEmptyTest()
            {
                stwebDefaults.StwebGradOvrCmcmtCapgown = "";
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));

                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();

                Assert.IsFalse(graduationConfiguration.OverrideCapAndGownDisplay);

            }

            [TestMethod]
            public async Task ReturnsExpectedShowCapAndGownFlagValueWithLittlenTest()
            {
                stwebDefaults.StwebGradOvrCmcmtCapgown = "n";
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));

                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();

                Assert.IsFalse(graduationConfiguration.OverrideCapAndGownDisplay);

            }

            [TestMethod]
            public async Task ReturnsValidConfiguration_HiddenQuestions()
            {
                var hiddenQuestions = new Collection<GraduationQuestions>();
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "DIPLOMA_NAME", GradqHide = "y", GradqIsRequired = "Y" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "ATTEND_COMMENCEMENT", GradqHide = "", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "HOMETOWN", GradqHide = "Y", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "PICKUP_DIPLOMA", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "CAP_SIZE", GradqHide = "Y", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "GOWN_SIZE", GradqHide = "Y", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "PHONETIC_SPELLING", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "COMMENCEMENT_LOCATION", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "NAME_IN_PROGRAM", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "NUMBER_GUESTS", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "MILITARY_STATUS", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "SPECIAL_ACCOMMODATIONS", GradqHide = "N", GradqIsRequired = "N" });
                hiddenQuestions.Add(new GraduationQuestions() { Recordkey = "JUNK", GradqHide = "N", GradqIsRequired = "N" });
                dataAccessorMock.Setup<Task<Collection<GraduationQuestions>>>(acc => acc.BulkReadRecordAsync<GraduationQuestions>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(hiddenQuestions));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));

                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(8, graduationConfiguration.ApplicationQuestions.Count());
            }

            [TestMethod]
            public async Task ReturnsLinksWithNoValueMarks()
            {
                // Set up repo response for stwebDefaults
                StwebDefaults stWebDefaultsResponse = new StwebDefaults();
                stWebDefaultsResponse.StwebGradCapgownSizesUrl = "https://capandgownsizes.com/other" + DmiString.sVM + "/stuff&more";
                stWebDefaultsResponse.StwebGradCapgownUrl = "www.capandgownorders.com" + DmiString.sVM;
                stWebDefaultsResponse.StwebGradCommencementUrl = DmiString.sVM + "commencementurl";
                stWebDefaultsResponse.StwebGradDiffProgramUrl = "gradwith" + DmiString.sVM + "different" + DmiString.sVM + "program.com";
                stWebDefaultsResponse.StwebGradMaxGuests = 10;
                stWebDefaultsResponse.StwebGradPhoneticUrl = "phoneticsUrl.com";
                stWebDefaultsResponse.StwebGradTerms = new List<string>() { "term1", "term2", "term3" };
                stWebDefaultsResponse.StwebGradRequirePayment = "N";
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stWebDefaultsResponse));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(stwebDefaults.StwebGradCapgownSizesUrl, graduationConfiguration.CapAndGownSizingLink);
                Assert.AreEqual(stwebDefaults.StwebGradCapgownUrl, graduationConfiguration.CapAndGownLink);
                Assert.AreEqual(stwebDefaults.StwebGradCommencementUrl, graduationConfiguration.CommencementInformationLink);
                Assert.AreEqual(stwebDefaults.StwebGradPhoneticUrl, graduationConfiguration.PhoneticSpellingLink);
                Assert.AreEqual(stwebDefaults.StwebGradDiffProgramUrl, graduationConfiguration.ApplyForDifferentProgramLink);
                Assert.AreEqual(stwebDefaults.StwebGradMaxGuests, graduationConfiguration.MaximumCommencementGuests);
            }

            [TestMethod]
            public async Task ReturnsLinksWithNoSpaces()
            {
                // Set up repo response for stwebDefaults (added spaces that hopefully get stripped)
                StwebDefaults stWebDefaultsResponse = new StwebDefaults();
                stWebDefaultsResponse.StwebGradCapgownSizesUrl = "https://cap" + " " + "andgownsizes.com/other" + "/stuff&more";
                stWebDefaultsResponse.StwebGradCapgownUrl = "www.cap" + " " + "and" + " " +  "gownorders.com";
                stWebDefaultsResponse.StwebGradCommencementUrl = "commencement" + "url" + " ";
                stWebDefaultsResponse.StwebGradDiffProgramUrl = " " + "gradwithdifferentprogram.com";
                stWebDefaultsResponse.StwebGradMaxGuests = 10;
                stWebDefaultsResponse.StwebGradPhoneticUrl = "phoneticsUrl.com";
                stWebDefaultsResponse.StwebGradTerms = new List<string>() { "term1", "term2", "term3" };
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stWebDefaultsResponse));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(stwebDefaults.StwebGradCapgownSizesUrl, graduationConfiguration.CapAndGownSizingLink);
                Assert.AreEqual(stwebDefaults.StwebGradCapgownUrl, graduationConfiguration.CapAndGownLink);
                Assert.AreEqual(stwebDefaults.StwebGradCommencementUrl, graduationConfiguration.CommencementInformationLink);
                Assert.AreEqual(stwebDefaults.StwebGradPhoneticUrl, graduationConfiguration.PhoneticSpellingLink);
                Assert.AreEqual(stwebDefaults.StwebGradDiffProgramUrl, graduationConfiguration.ApplyForDifferentProgramLink);
                Assert.AreEqual(stwebDefaults.StwebGradMaxGuests, graduationConfiguration.MaximumCommencementGuests);
            }

            [TestMethod]
            public async Task ReturnValidTerms()
            {
                // Set up repo response for stwebDefaults
                StwebDefaults stWebDefaultsResponse = new StwebDefaults();
                stWebDefaultsResponse.StwebGradCapgownSizesUrl = "https://capandgownsizes.com/other/stuff&more";
                stWebDefaultsResponse.StwebGradCapgownUrl = "www.capandgownorders.com";
                stWebDefaultsResponse.StwebGradCommencementUrl = "commencementurl";
                stWebDefaultsResponse.StwebGradDiffProgramUrl = "gradwithdifferentprogram.com";
                stWebDefaultsResponse.StwebGradMaxGuests = 10;
                stWebDefaultsResponse.StwebGradPhoneticUrl = "phoneticsUrl.com";
                stWebDefaultsResponse.StwebGradTerms = new List<string>() { "term1", "term2", "term3", null, "" };
                stWebDefaultsResponse.StwebGradRequirePayment = "Y";
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stWebDefaultsResponse));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var graduationConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(3, graduationConfiguration.GraduationTerms.Count());
                foreach (var term in graduationConfiguration.GraduationTerms)
                {
                    Assert.IsNotNull(term);
                    Assert.AreNotEqual("", term);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task DefaultLookup_ThrowsExceptionForNullReturnedByDefaults()
            {
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                await studentConfigurationRepository.GetGraduationConfigurationAsync();
            }

            [TestMethod]
            public async Task DefaultLookup_EmailAddressTypeReturnedDefaults()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var studentRepositoryDto = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(studentRepositoryDto.DefaultWebEmailType, defaults.DefaultWebEmailType);
            }

            [TestMethod]
            public async Task DefaultLookup_EmptyEmailAddressTypeReturnedDefaults()
            {
                defaults.DefaultWebEmailType = string.Empty;
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(defaults));
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var studentRepositoryDto = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(studentRepositoryDto.DefaultWebEmailType, defaults.DefaultWebEmailType);
                Assert.AreEqual(studentRepositoryDto.DefaultWebEmailType, string.Empty);
            }

            [TestMethod]
            public async Task StwebDEfaults_GradNotificationParagraph()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                var studentRepositoryDto = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(stwebDefaults.StwebGradNotifyPara, studentRepositoryDto.EmailGradNotifyPara);
            }

            [TestMethod]
            public async Task StwebDEfaults_Null_GradNotificationParagraph()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                stwebDefaults.StwebGradNotifyPara = null;
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));
                var studentRepositoryDto = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.IsNull(studentRepositoryDto.EmailGradNotifyPara);
            }
          
            [TestMethod]
            public async Task DefaultLookup_NullEmailAddressTypeReturnedDefaults()
            {
                dataAccessorMock.Setup<Task<Data.Student.DataContracts.DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(daDefaults));
                defaults.DefaultWebEmailType = null;
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(defaults));
                var studentRepositoryDto = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                Assert.AreEqual(studentRepositoryDto.DefaultWebEmailType, defaults.DefaultWebEmailType);
                Assert.IsNull(studentRepositoryDto.DefaultWebEmailType);
            }
            
            private StudentConfigurationRepository BuildValidStudentConfigurationRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for stwebDefault and graduationQuestions
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));
                dataAccessorMock.Setup<Task<Collection<GraduationQuestions>>>(acc => acc.BulkReadRecordAsync<GraduationQuestions>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(graduationQuestionsResponseData));
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(defaults));
                StudentConfigurationRepository repository = new StudentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }

            private Collection<GraduationQuestions> BuildGraduationQuestionsResponse()
            {
                var gradQuestions = new Collection<GraduationQuestions>();
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "DIPLOMA_NAME", GradqHide = "N", GradqIsRequired = "Y" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "ATTEND_COMMENCEMENT", GradqHide = "", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "HOMETOWN", GradqHide = "n", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "PICKUP_DIPLOMA", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "CAP_SIZE", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "GOWN_SIZE", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "PHONETIC_SPELLING", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "COMMENCEMENT_LOCATION", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "NAME_IN_PROGRAM", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "NUMBER_GUESTS", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "MILITARY_STATUS", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "SPECIAL_ACCOMMODATIONS", GradqHide = "N", GradqIsRequired = "N" });
                gradQuestions.Add(new GraduationQuestions() { Recordkey = "JUNK", GradqHide = "N", GradqIsRequired = "N" });
                return gradQuestions;
            }

            private StwebDefaults BuildStwebDefaultsResponse()
            {
                var defaults = new StwebDefaults();
                defaults.StwebGradCapgownSizesUrl = "https://capandgownsizes.com/other/stuff&more";
                defaults.StwebGradCapgownUrl = "www.capandgownorders.com";
                defaults.StwebGradCommencementUrl = "commencementurl";
                defaults.StwebGradDiffProgramUrl = "gradwithdifferentprogram.com";
                defaults.StwebGradMaxGuests = 10;
                defaults.StwebGradPhoneticUrl = "phoneticsUrl.com";
                defaults.StwebGradTerms = new List<string>() { "term1", "term2", "term3" };
                defaults.StwebGradOvrCmcmtCapgown = "Y";
                defaults.StwebGradNotifyPara = "GNOTIFY";
                return defaults;
            }

            private Defaults BuildDefaultsResponse()
            {
                 var defaults = new Defaults();
                 defaults.DefaultWebEmailType = "PRI";
                 return defaults;
            }

            private DaDefaults BuildDaDefaultsResponse()
            {
                var daDefaults = new DaDefaults();
                daDefaults.DaHideAntCmplDtInSsMp = "Y";
                return daDefaults;
            }

        }

        [TestClass]
        public class StudentRequestConfigurationAsyncTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            StwebDefaults stwebDefaults;
            Defaults defaults;
            StudentConfigurationRepository studentConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Collection of data accessor responses
                stwebDefaults = BuildStwebDefaultsResponse();
                defaults = BuildDefaultsResponse();
                studentConfigurationRepository = BuildValidStudentConfigurationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                stwebDefaults = null;
                studentConfigurationRepository = null;
            }

            [TestMethod]
            public async Task GetStudentRequestConfiguration_ReturnsProperties()
            {              
                var requestConfiguration = await studentConfigurationRepository.GetStudentRequestConfigurationAsync();
                Assert.IsTrue(requestConfiguration.SendTranscriptRequestConfirmation);
                Assert.IsTrue(requestConfiguration.SendEnrollmentRequestConfirmation);
                Assert.AreEqual(defaults.DefaultWebEmailType, requestConfiguration.DefaultWebEmailType);
            }

            [TestMethod]
            public async Task DefaultValuesIfDataReaderReturnsNull()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                StwebDefaults nullResponse = null;
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                Data.Base.DataContracts.Defaults nullDefaultResponse = null;
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Data.Base.DataContracts.Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullDefaultResponse));
                // Act
                var requestConfiguration = await studentConfigurationRepository.GetStudentRequestConfigurationAsync();
                // Assert
                Assert.IsFalse(requestConfiguration.SendTranscriptRequestConfirmation);
                Assert.IsFalse(requestConfiguration.SendEnrollmentRequestConfirmation);
                Assert.IsNull(requestConfiguration.DefaultWebEmailType);
            }

            private StwebDefaults BuildStwebDefaultsResponse()
            {
                var defaults = new StwebDefaults();
                defaults.StwebTranNotifyPara = "TNOTIFY";
                defaults.StwebEnrlNotifyPara = "ENOTIFY";
                return defaults;
            }

            private Defaults BuildDefaultsResponse()
            {
                var defaults = new Defaults();
                defaults.DefaultWebEmailType = "PRI";
                return defaults;
            }

            private StudentConfigurationRepository BuildValidStudentConfigurationRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for stwebDefault and graduationQuestions
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));
                dataAccessorMock.Setup<Task<Data.Base.DataContracts.Defaults>>(acc => acc.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(defaults));
                StudentConfigurationRepository repository = new StudentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }
        }

        [TestClass]
        public class FacultyGradingConfigurationAsyncTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            StwebDefaults stwebDefaults;
            Defaults defaults;
            StudentConfigurationRepository studentConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Collection of data accessor responses
                stwebDefaults = BuildStwebDefaultsResponse();
                defaults = BuildDefaultsResponse();
                studentConfigurationRepository = BuildValidStudentConfigurationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                stwebDefaults = null;
                studentConfigurationRepository = null;
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_ReturnsTrueProperties()
            {
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.IsTrue(facultyGradingConfiguration.IncludeCrosslistedStudents);
                Assert.IsTrue(facultyGradingConfiguration.IncludeDroppedWithdrawnStudents);
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_ReturnsFalseProperties()
            {
                // Set up repo response for stwebDefault so that values are not "Y"
                var otherDefaults = new StwebDefaults();
                otherDefaults.StwebGradeDropsFlag = "N";
                otherDefaults.StwebGradeInclXlist = "X";
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(otherDefaults));
                
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.IsFalse(facultyGradingConfiguration.IncludeCrosslistedStudents);
                Assert.IsFalse(facultyGradingConfiguration.IncludeDroppedWithdrawnStudents);
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_StwebMidtermGradeCountValid()
            {

                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.AreEqual(5, facultyGradingConfiguration.NumberOfMidtermGrades);
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_StwebMidtermGradeCountNull()
            {
                var testdefaults = new StwebDefaults();
                testdefaults.StwebGradeDropsFlag = "Y";
                testdefaults.StwebGradeInclXlist = "y";
                testdefaults.StwebMidtermGradeCount = null;
                // Set up repo response for stwebDefault
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(testdefaults));
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.AreEqual(0, facultyGradingConfiguration.NumberOfMidtermGrades);
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_StwebMidtermGradeCountNotNumeric()
            {
                var testdefaults = new StwebDefaults();
                testdefaults.StwebGradeDropsFlag = "Y";
                testdefaults.StwebGradeInclXlist = "y";
                testdefaults.StwebMidtermGradeCount = "X";
                // Set up repo response for stwebDefault
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(testdefaults));
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.AreEqual(0, facultyGradingConfiguration.NumberOfMidtermGrades);
            }

            [TestMethod]
            public async Task GetFacultyGradingConfiguration_StwebMidtermGradeCountOver6()
            {
                var testdefaults = new StwebDefaults();
                testdefaults.StwebGradeDropsFlag = "Y";
                testdefaults.StwebGradeInclXlist = "y";
                testdefaults.StwebMidtermGradeCount = "7";
                // Set up repo response for stwebDefault
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(testdefaults));
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                Assert.AreEqual(0, facultyGradingConfiguration.NumberOfMidtermGrades);
            }

            [TestMethod]
            public async Task DefaultValuesIfDataReaderReturnsNull()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                StwebDefaults nullResponse = null;
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                // Act
                var facultyGradingConfiguration = await studentConfigurationRepository.GetFacultyGradingConfigurationAsync();
                // Assert
                Assert.IsFalse(facultyGradingConfiguration.IncludeCrosslistedStudents);
                Assert.IsFalse(facultyGradingConfiguration.IncludeDroppedWithdrawnStudents);
                Assert.AreEqual(0, facultyGradingConfiguration.AllowedGradingTerms.Count());
                Assert.AreEqual(0, facultyGradingConfiguration.NumberOfMidtermGrades);
            }

            private StwebDefaults BuildStwebDefaultsResponse()
            {
                var defaults = new StwebDefaults();
                defaults.StwebGradeDropsFlag = "Y";
                defaults.StwebGradeInclXlist = "y";
                defaults.StwebMidtermGradeCount = "5";
                return defaults;
            }

            private Defaults BuildDefaultsResponse()
            {
                var defaults = new Defaults();
                defaults.DefaultWebEmailType = "PRI";
                return defaults;
            }

            private StudentConfigurationRepository BuildValidStudentConfigurationRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for stwebDefault
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));
                
                
                StudentConfigurationRepository repository = new StudentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }
        }

        [TestClass]
        public class CourseCatalogConfigurationAsyncTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            CatalogSearchDefaults catalogSearchDefaults;
            StwebDefaults stwebDefaults;
            StudentConfigurationRepository studentConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Collection of data accessor responses
                stwebDefaults = BuildStwebDefaultsResponse();
                catalogSearchDefaults = BuildCatalogSearchDefaultsResponse();
                studentConfigurationRepository = BuildValidStudentConfigurationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                stwebDefaults = null;
                catalogSearchDefaults = null;
                studentConfigurationRepository = null;
            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_ReturnsValidProperties()
            {
                var courseCatalogConfiguration = await studentConfigurationRepository.GetCourseCatalogConfigurationAsync();
                Assert.IsTrue(courseCatalogConfiguration is Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration);
                Assert.AreEqual(stwebDefaults.StwebRegStartDate, courseCatalogConfiguration.EarliestSearchDate);
                Assert.AreEqual(stwebDefaults.StwebRegEndDate, courseCatalogConfiguration.LatestSearchDate);
                Assert.AreEqual(3, courseCatalogConfiguration.CatalogFilterOptions.Count());
                Assert.AreEqual(2, courseCatalogConfiguration.CatalogFilterOptions.Where(c => c.IsHidden == true).Count());
                Assert.AreEqual(Ellucian.Colleague.Domain.Student.Entities.CatalogFilterType.CourseTypes, courseCatalogConfiguration.CatalogFilterOptions[1].Type);
                Assert.IsFalse(courseCatalogConfiguration.ShowCourseSectionFeeInformation);
            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_StwebDefaultsReturnsNull()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                StwebDefaults nullResponse = null;
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));
                dataAccessorMock.Setup<Task<CatalogSearchDefaults>>(acc => acc.ReadRecordAsync<CatalogSearchDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(catalogSearchDefaults));

                // Act
                var courseCatalogConfiguration = await studentConfigurationRepository.GetCourseCatalogConfigurationAsync();
                // Assert
                Assert.IsNull(courseCatalogConfiguration.EarliestSearchDate);
                Assert.IsNull(courseCatalogConfiguration.LatestSearchDate);
                Assert.AreEqual(3, courseCatalogConfiguration.CatalogFilterOptions.Count());
            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_CatalogSearchDefaultsReturnsNull()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));

                CatalogSearchDefaults nullCatalogSearchDefaultsResponse = null;
                dataAccessorMock.Setup<Task<CatalogSearchDefaults>>(acc => acc.ReadRecordAsync<CatalogSearchDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullCatalogSearchDefaultsResponse));
                // Act
                var courseCatalogConfiguration = await studentConfigurationRepository.GetCourseCatalogConfigurationAsync();
                // Assert
                Assert.AreEqual(stwebDefaults.StwebRegStartDate, courseCatalogConfiguration.EarliestSearchDate);
                Assert.AreEqual(stwebDefaults.StwebRegEndDate, courseCatalogConfiguration.LatestSearchDate);
                Assert.AreEqual(0, courseCatalogConfiguration.CatalogFilterOptions.Count());

            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_StwebShowCatSecOtherFee_Y()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                StwebDefaults response = new StwebDefaults()
                {
                    StwebRegStartDate = DateTime.Today.AddDays(-30),
                    StwebRegEndDate = DateTime.Today.AddDays(30),
                    StwebShowCatSecOtherFee = "Y"
                };
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(response));
                // Act
                var courseCatalogConfiguration = await studentConfigurationRepository.GetCourseCatalogConfigurationAsync();
                // Assert
                Assert.AreEqual(response.StwebRegStartDate, courseCatalogConfiguration.EarliestSearchDate);
                Assert.AreEqual(response.StwebRegEndDate, courseCatalogConfiguration.LatestSearchDate);
                Assert.AreEqual(3, courseCatalogConfiguration.CatalogFilterOptions.Count());
                Assert.IsTrue(courseCatalogConfiguration.ShowCourseSectionFeeInformation);
            }

            [TestMethod]
            public async Task GetCourseCatalogConfigurationAsync_StwebShowCatSecOtherFee_Null()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                StwebDefaults response = new StwebDefaults()
                {
                    StwebRegStartDate = DateTime.Today.AddDays(-30),
                    StwebRegEndDate = DateTime.Today.AddDays(30),
                };
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(response));
                // Act
                var courseCatalogConfiguration = await studentConfigurationRepository.GetCourseCatalogConfigurationAsync();
                // Assert
                Assert.AreEqual(response.StwebRegStartDate, courseCatalogConfiguration.EarliestSearchDate);
                Assert.AreEqual(response.StwebRegEndDate, courseCatalogConfiguration.LatestSearchDate);
                Assert.AreEqual(3, courseCatalogConfiguration.CatalogFilterOptions.Count());
                Assert.IsTrue(courseCatalogConfiguration.ShowCourseSectionFeeInformation);
            }

            private StwebDefaults BuildStwebDefaultsResponse()
            {
                var defaults = new StwebDefaults();
                defaults.StwebRegStartDate = DateTime.Now.AddDays(-30);
                defaults.StwebRegEndDate = DateTime.Now;
                defaults.StwebShowCatSecOtherFee = "N";
                return defaults;
            }

            private CatalogSearchDefaults BuildCatalogSearchDefaultsResponse()
            {
                var defaults = new CatalogSearchDefaults();
                defaults.ClsdSearchElementsEntityAssociation = new List<CatalogSearchDefaultsClsdSearchElements>();
                defaults.ClsdSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdSearchElements() { ClsdSearchElementAssocMember = "AVAILABILITY", ClsdHideAssocMember = "Y" });
                defaults.ClsdSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdSearchElements() { ClsdSearchElementAssocMember = "COURSE_TYPES", ClsdHideAssocMember = "" });
                defaults.ClsdSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdSearchElements() { ClsdSearchElementAssocMember = "LOCATIONS", ClsdHideAssocMember = "y" });
                // And one invalid one that will not be converted:
                defaults.ClsdSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdSearchElements() { ClsdSearchElementAssocMember = "JUNK", ClsdHideAssocMember = "Y" });
                return defaults;
            }

            private StudentConfigurationRepository BuildValidStudentConfigurationRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Set up repo response for stwebDefault
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(stwebDefaults));
                dataAccessorMock.Setup<Task<CatalogSearchDefaults>>(acc => acc.ReadRecordAsync<CatalogSearchDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(catalogSearchDefaults));
                StudentConfigurationRepository repository = new StudentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }
        }

        [TestClass]
        public class RegistrationConfigurationAsyncTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            RegDefaults regDefaults;
            StwebDefaults StwebDefaults;
            StudentConfigurationRepository studentConfigurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Collection of data accessor responses
                studentConfigurationRepository = BuildValidStudentConfigurationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                regDefaults = null;
                studentConfigurationRepository = null;
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnsValidProperties()
            {
                regDefaults = new RegDefaults() {  Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "y", RgdAddAuthStartOffset = 3};
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, regConfiguration.AddAuthorizationStartOffsetDays);
                
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnsValidProperties2()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "Y", RgdAddAuthStartOffset = 0 };
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, regConfiguration.AddAuthorizationStartOffsetDays);

            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_RegDefaultsReturnsNull()
            {
                // Arrange: Set up repo response for null stwebDefaults data contract and null Defaults data contract.
                RegDefaults nullResponse = null;
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(nullResponse));

                // Act
                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                loggerMock.Verify(l => l.Info("Unable to access registration defaults from ST.PARMS. REG.DEFAULTS. Default values will be assumed for purpose of building registration configuration in API." + Environment.NewLine
                      + "You can build a REG.DEFAULTS record by accessing the RGPD form in Colleague UI."));
                Assert.IsNotNull(regConfiguration);
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsFalse(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, regConfiguration.AddAuthorizationStartOffsetDays);

            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_InapplicableOffsetDays()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "n", RgdAddAuthStartOffset = 3 };
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsFalse(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, regConfiguration.AddAuthorizationStartOffsetDays);

            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_DefaultValues()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = null, RgdAddAuthStartOffset = null };
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsFalse(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, regConfiguration.AddAuthorizationStartOffsetDays);

            }
            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_RequireAuthorizationNo()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "N", RgdAddAuthStartOffset = null };
                // Set up repo response for regDefault
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsFalse(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(0, regConfiguration.AddAuthorizationStartOffsetDays);

            }
            //stwebdefaults for drop reasons retreival
            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_ReturnsValidPropertiesForDropReasons()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "y", RgdAddAuthStartOffset = 3 };
                StwebDefaults = new StwebDefaults() { Recordkey = "STWEB.DEFAULTS", StwebDropRsnPromptFlag = "Y", StwebDropRsnRequiredFlag = "Y" };
                // Set up repo response for StwebDefaults
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(),true)).Returns(Task.FromResult(StwebDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, regConfiguration.AddAuthorizationStartOffsetDays);
                Assert.IsTrue(regConfiguration.PromptForDropReason);
                Assert.IsTrue(regConfiguration.RequireDropReason);

            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_StwebDfltsIsNull()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "y", RgdAddAuthStartOffset = 3 };
                StwebDefaults = null;
                // Set up repo response for StwebDefaults
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(StwebDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, regConfiguration.AddAuthorizationStartOffsetDays);
                Assert.IsFalse(regConfiguration.PromptForDropReason);
                Assert.IsFalse(regConfiguration.RequireDropReason);
                loggerMock.Verify(l => l.Info("Unable to access registration defaults from ST.PARMS. STWEB.DEFAULTS."));
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_PromptandrequiredForReasonIsEmpty()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "y", RgdAddAuthStartOffset = 3 };
                StwebDefaults = new StwebDefaults() { Recordkey = "STWEB.DEFAULTS", StwebDropRsnPromptFlag = null, StwebDropRsnRequiredFlag = string.Empty };

                // Set up repo response for StwebDefaults
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(StwebDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, regConfiguration.AddAuthorizationStartOffsetDays);
                Assert.IsFalse(regConfiguration.PromptForDropReason);
                Assert.IsFalse(regConfiguration.RequireDropReason);
            }

            [TestMethod]
            public async Task GetRegistrationConfigurationAsync_PromptForReasonIsN()
            {
                regDefaults = new RegDefaults() { Recordkey = "REG.DEFAULTS", RgdRequireAddAuthFlag = "y", RgdAddAuthStartOffset = 3 };
                StwebDefaults = new StwebDefaults() { Recordkey = "STWEB.DEFAULTS", StwebDropRsnPromptFlag = "y", StwebDropRsnRequiredFlag ="n" };

                // Set up repo response for StwebDefaults
                dataAccessorMock.Setup<Task<RegDefaults>>(acc => acc.ReadRecordAsync<RegDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(regDefaults));
                dataAccessorMock.Setup<Task<StwebDefaults>>(acc => acc.ReadRecordAsync<StwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(StwebDefaults));

                var regConfiguration = await studentConfigurationRepository.GetRegistrationConfigurationAsync();
                Assert.IsTrue(regConfiguration is Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration);
                Assert.IsTrue(regConfiguration.RequireFacultyAddAuthorization);
                Assert.AreEqual(3, regConfiguration.AddAuthorizationStartOffsetDays);
                Assert.IsTrue(regConfiguration.PromptForDropReason);
                Assert.IsFalse(regConfiguration.RequireDropReason);
            }

            private StudentConfigurationRepository BuildValidStudentConfigurationRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));

                // Set up data accessor for the transaction factory 
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);


                StudentConfigurationRepository repository = new StudentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }
        }
    }
}
