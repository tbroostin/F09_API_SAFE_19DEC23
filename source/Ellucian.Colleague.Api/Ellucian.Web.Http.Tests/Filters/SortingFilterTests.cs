// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Tests.Filters.TestData;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;

namespace Ellucian.Web.Http.Tests.Filters
{
    [TestClass]
    public class SortingFilterTests
    {
        SortingFilterTestData testData;
        IEnumerable<Org> unsortedList;
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            testData = new SortingFilterTestData();
            unsortedList = testData.GetAllOrgs();

            _loggerMock = MockLogger.Instance;

            _logger = _loggerMock.Object;

        }

        [TestMethod]
        public void TestBasicSorting_Asc()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_OrgName();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "orgName", "asc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.OrgID, ((Org)actual).OrgID);
                Assert.AreEqual(expected.OrgName, ((Org)actual).OrgName);
            }

        }

        [TestMethod]
        public void TestBasicSorting_Desc()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_OrgNameDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "orgName", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.OrgID, ((Org)actual).OrgID);
                Assert.AreEqual(expected.OrgName, ((Org)actual).OrgName);
            }

        }

        [TestMethod]
        public void TestBasicSorting_WithNullClassValues()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_SubDeptNameDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.subdept.SubDeptName", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual((expected.Dept.SubDept ==null ? String.Empty : expected.Dept.SubDept.SubDeptName), (((Org)actual).Dept.SubDept == null ? String.Empty : ((Org)actual).Dept.SubDept.SubDeptName));
            }

        }

        [TestMethod]
        public void TestBasicSorting_WithNullFieldValues()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_DeptDateDesc_DeptIdDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D,%20%7B%22{2}%22:%22{3}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.somedate", "desc", "dept.DeptID", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual((expected.Dept.SomeDate == null ? (DateTime?)null : expected.Dept.SomeDate), (((Org)actual).Dept.SomeDate == null ? (DateTime?)null : ((Org)actual).Dept.SomeDate));
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBasicSorting_IncorrectSortingOrder()
        {
            //Arrange
            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "orgName", "asc1");

            //Act
            Ellucian.Web.Http.Extensions.HttpRequestMessageExtensions.GetSortingParameters(CreateRequest(jsonQueryString, HttpMethod.Get));
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void TestBasicSorting_IncorrectJsonQuery()
        {
            //Arrange
            string requestURI = "http://testurl/orgs?sort=%7B%22{0}%22:%22{1}%22%7D]"; //incorrectly formatted json query
            string jsonQueryString = String.Format(requestURI, "orgName", "asc");

            //Act
            Ellucian.Web.Http.Extensions.HttpRequestMessageExtensions.GetSortingParameters(CreateRequest(jsonQueryString, HttpMethod.Get));
        }

        [TestMethod]
        public void TestNestedSorting()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_CourseIdDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.student.course.courseID", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.Course.CourseID, ((Org)actual).Dept.Student.Course.CourseID);
                Assert.AreEqual(expected.Dept.Student.Course.CourseActive, ((Org)actual).Dept.Student.Course.CourseActive);
                Assert.AreEqual(expected.Dept.Student.Course.CourseName, ((Org)actual).Dept.Student.Course.CourseName);
            }

        }

        [TestMethod]
        public void TestNestedSorting_WithDataMemberField()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_StudentAvgScoreDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.student.fieldAvgScore", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.Course.CourseID, ((Org)actual).Dept.Student.Course.CourseID);
                Assert.AreEqual(expected.Dept.Student.Course.CourseActive, ((Org)actual).Dept.Student.Course.CourseActive);
                Assert.AreEqual(expected.Dept.Student.Course.CourseName, ((Org)actual).Dept.Student.Course.CourseName);
            }

        }

        [TestMethod]
        public void TestNestedSorting_MultipleProperties()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_StudentDOBDesc_StudentNameDesc();

            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D,%20%7B%22{2}%22:%22{3}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.student.dob", "desc", "dept.student.name", "desc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.Name, ((Org)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.ID, ((Org)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.GPA, ((Org)actual).Dept.Student.GPA);
                Assert.AreEqual(expected.Dept.Student.DOB, ((Org)actual).Dept.Student.DOB);
            }

        }

        [TestMethod]
        public void TestNestedSorting_MultipleProperties_WithJsonProperty()
        {
            //Arrange
            List<Org> expectedSortedList = testData.GetOrgs_SortedBy_JsonCourseId_CourseNameDesc_CourseActive();

            //"?sort=[%7B%22State%22:%22desc%22%7D,%20%7B%22PostalCode%22:%22asc%22%7D,%20%7B%22Code%22:%22desc%22%7D]"
            string requestURI = "http://testurl/orgs?sort=[%7B%22{0}%22:%22{1}%22%7D,%20%7B%22{2}%22:%22{3}%22%7D,%20%7B%22{4}%22:%22{5}%22%7D]";
            string jsonQueryString = String.Format(requestURI, "dept.Student.Course.jsonCourseId", "asc", "Dept.Student.course.CourseName", "desc", "Dept.Student.Course.courseactive", "asc");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, unsortedList);

            //Act
            var Filter = new SortingFilter(_logger);
            Filter.OnActionExecuted(executedContext);

            //After the filter is executed, the context response.content should contain the sorted list:
            IEnumerable<object> actualSortedList = null;
            executedContext.Response.TryGetContentValue(out actualSortedList);

            //Assert
            Assert.AreEqual(expectedSortedList.Count(), actualSortedList.Count());

            for (int i = 0; i < expectedSortedList.Count(); i++)
            {
                var expected = expectedSortedList[i];
                var actual = actualSortedList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.Course.CourseID, ((Org)actual).Dept.Student.Course.CourseID);
                Assert.AreEqual(expected.Dept.Student.Course.CourseName, ((Org)actual).Dept.Student.Course.CourseName);
                Assert.AreEqual(expected.Dept.Student.Course.CourseActive, ((Org)actual).Dept.Student.Course.CourseActive);
            }

        }

        private HttpRequestMessage CreateRequest(string url, HttpMethod method)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = method;

            return request;
        }

        private HttpResponseMessage CreateResponse(IEnumerable<object> fullList)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Content = new ObjectContent<IEnumerable<object>>(fullList, new JsonMediaTypeFormatter());
            return httpResponseMessage;
        }

        private HttpActionExecutedContext CreateHttpActionExecutedContext(string jsonRequestString, IEnumerable<object> fullList)
        {
            HttpRequestMessage request = CreateRequest(jsonRequestString, HttpMethod.Get);

            HttpResponseMessage response = CreateResponse(fullList);

            HttpActionContext actionContext = new HttpActionContext() { ControllerContext = new HttpControllerContext() { Request = request }, Response = response };

            HttpActionExecutedContext httpActionExecutedContext = new HttpActionExecutedContext(actionContext, null);

            return httpActionExecutedContext;
        }

    }
}
