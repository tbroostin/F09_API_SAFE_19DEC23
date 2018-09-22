using System.Web.Http.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Api.Tests.App_Start
{
    [TestClass]
    public class FilterConfigTests
    {
        [TestMethod]
        public void RegisterGlobalFilters()
        {
            var globalFilterCollection = new HttpFilterCollection();

            FilterConfig.RegisterGlobalFilters(globalFilterCollection);

            Assert.IsTrue(globalFilterCollection.Count > 0);
        }
    }
}