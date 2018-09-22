// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using slf4net;
using Ellucian.Web.Cache;

namespace Ellucian.Colleague.Data.Base.Tests
{
    [TestClass]
    public class RestrictionRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<IColleagueTransactionInvoker> mockManager;
        Mock<ObjectCache> localCacheMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        IEnumerable<Restriction> allRestrictions;
        Collection<Restrictions> restrsResponseData;
        Collection<Restrictions> firstOne;
        ReferenceDataRepository refDataRepo;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            allRestrictions = new TestRestrictionRepository().Get();
            restrsResponseData = BuildRestrictionsResponse(allRestrictions);
            var first = allRestrictions.First();
            firstOne = BuildRestrictionsResponse(new List<Restriction>() { first });
            refDataRepo = BuildRestrictionRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            allRestrictions = null;
            restrsResponseData = null;
            firstOne = null;
            refDataRepo = null;
        }

        //[TestMethod]
        //public void RestrictionCount_Valid()
        //{
        //    var allRestrictions = refDataRepo.Restrictions.ToList();
        //    Assert.AreEqual(3, allRestrictions.Count);
        //}

        //[TestMethod]
        //public void RestrictionProperties_Valid()
        //{
        //    var base1 = allRestrictions.ElementAt(0);
        //    var testRestrictions = refDataRepo.Restrictions;
        //    var test1 = testRestrictions.ElementAt(0);
        //    Assert.AreEqual(base1.Code, test1.Code);
        //    Assert.AreEqual(base1.Description, test1.Description);
        //    Assert.AreEqual(base1.Severity, test1.Severity);
        //    Assert.AreEqual(base1.OfficeUseOnly, test1.OfficeUseOnly);
        //    Assert.AreEqual(base1.Title, test1.Title);
        //    Assert.AreEqual(base1.Details, test1.Details);
        //    Assert.AreEqual(base1.FollowUpApplication, test1.FollowUpApplication);
        //    Assert.AreEqual(base1.FollowUpLinkDefinition, test1.FollowUpLinkDefinition);
        //    Assert.AreEqual(base1.FollowUpWebAdvisorForm, test1.FollowUpWebAdvisorForm);
        //    Assert.AreEqual(base1.FollowUpLabel, test1.FollowUpLabel);
        //    Assert.AreEqual(base1.MiscellaneousTextFlag, test1.MiscellaneousTextFlag);
        //    Assert.AreEqual(base1.Hyperlink, test1.Hyperlink);

        //    var base2 = allRestrictions.ElementAt(1);
        //    var test2 = testRestrictions.ElementAt(1);
        //    // element 1 has a null, which equates to true
        //    Assert.AreEqual(base2.OfficeUseOnly, test2.OfficeUseOnly);

        //    var base3 = allRestrictions.ElementAt(2);
        //    var test3 = testRestrictions.ElementAt(2);
        //    // element 2 has a null, which equates to false
        //    Assert.AreEqual(base3.MiscellaneousTextFlag, test3.MiscellaneousTextFlag);
        //}

        private ReferenceDataRepository BuildRestrictionRepository()
        {
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            loggerMock = new Mock<ILogger>();
            cacheProviderMock = new Mock<ICacheProvider>();
            localCacheMock = new Mock<ObjectCache>();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            var hyperlinkResponse1 = new GetRestrictionHyperlinksResponse();
            hyperlinkResponse1.LinkLabelsOut = new List<string>();
            hyperlinkResponse1.HyperlinksOut = new List<string>();
            List<string> restrictionIds = new List<string>();
            foreach (var restr in allRestrictions)
            {
                restrictionIds.Add(restr.Code);
                hyperlinkResponse1.HyperlinksOut.Add(restr.Hyperlink);
                hyperlinkResponse1.LinkLabelsOut.Add(restr.FollowUpLabel);
            }
            //mockManager.Setup(mgr => mgr.Execute<GetRestrictionHyperlinksRequest, GetRestrictionHyperlinksResponse>(
            //    It.Is<GetRestrictionHyperlinksRequest>(r => r.RestrictionIds.Count == 3)))
            //        .Returns(hyperlinkResponse1).Callback<GetRestrictionHyperlinksRequest>(req => updateRequest = req);
            mockManager.Setup(mgr => mgr.Execute<GetRestrictionHyperlinksRequest, GetRestrictionHyperlinksResponse>(
                It.Is<GetRestrictionHyperlinksRequest>(x => x.RestrictionIds.Count == 3))).Returns(hyperlinkResponse1);

            dataAccessorMock.Setup<Collection<Restrictions>>(acc => acc.BulkReadRecord<Restrictions>("RESTRICTIONS", "", true)).Returns(restrsResponseData);

            var temp1 = new List<string>() { allRestrictions.First().Code }.ToArray();
            dataAccessorMock.Setup<Collection<Restrictions>>(acc => acc.BulkReadRecord<Restrictions>("RESTRICTIONS", temp1, true)).Returns(firstOne);

            ReferenceDataRepository refDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return refDataRepo;
        }

        private Collection<Restrictions> BuildRestrictionsResponse(IEnumerable<Restriction> restrictions)
        {
            Collection<Restrictions> restData = new Collection<Restrictions>();
            foreach (var restr in restrictions)
            {
                Restrictions rest = new Restrictions();
                rest.RecordGuid = restr.Guid;
                rest.Recordkey = restr.Code;
                rest.RestDesc = restr.Description;
                rest.RestSeverity = restr.Severity;
                rest.RestPrtlDisplayFlag = (restr.OfficeUseOnly ? "N" : "Y");
                rest.RestPrtlDisplayDesc = restr.Title;
                rest.RestPrtlDisplayDescDtl = restr.Details;
                rest.RestPrtlFollowUpApp = restr.FollowUpApplication;
                rest.RestPrtlFollowUpLinkDef = restr.FollowUpLinkDefinition;
                rest.RestPrtlFollowUpWaForm = restr.FollowUpWebAdvisorForm;
                rest.RestPrtlFollowUpLabel = restr.FollowUpLabel;
                rest.RestPrtlFollowUpIsMtxt = (restr.MiscellaneousTextFlag ? "Y" : "N");
                restData.Add(rest);
            }
            return restData;
        }

    }
}
