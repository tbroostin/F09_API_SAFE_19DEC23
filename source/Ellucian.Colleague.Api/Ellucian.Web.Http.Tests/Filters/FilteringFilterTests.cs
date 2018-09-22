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
using slf4net;

namespace Ellucian.Web.Http.Tests.Filters
{
    [TestClass]
    public class FilteringFilterTests
    {
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;
        FilteringFilterTestData clsTestData;
        IEnumerable<TestCourse> fullCourseList;
        IEnumerable<TestOrg> fullOrgList;
        IEnumerable<TestDept> fullDeptList;

        [TestInitialize]
        public void Initialize()
        {
            clsTestData = new FilteringFilterTestData();
            fullCourseList = clsTestData.ListCourses;
            fullOrgList = clsTestData.ListOrgs;
            fullDeptList = clsTestData.ListDepts;

            _loggerMock = MockLogger.Instance;
            _logger = _loggerMock.Object;

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

        [TestMethod]
        public void TestEqualOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = new List<TestCourse>{ clsTestData.ExpectedResult_TestEqualOp() };
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a{1}%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "12645873358");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act    
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            //After the filter is executed, the context response.content should contain the filtered list:
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID,((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestGTOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestGTOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22{0}%22+%3a+%7b+%22%24{1}%22+%3a+{2}+%7d+%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "gt", "6562456323");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }


        [TestMethod]
        public void TestLTOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestLTOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22{0}%22+%3a+%7b+%22%24{1}%22+%3a+{2}+%7d+%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "LT", "986245748145256");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestGTEOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestGTEOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22{0}%22+%3a+%7b+%22%24{1}%22+%3a+{2}+%7d+%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "gtE", "6562456323");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestLTEOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestLTEOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22{0}%22+%3a+%7b+%22%24{1}%22+%3a+{2}+%7d+%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "LtE", "12645873358");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestNEOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestNEOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22{0}%22+%3a+%7b+%22%24{1}%22+%3a+%22{2}%22+%7d+%7d";
            string jsonQueryString = String.Format(requestURI, "coursename", "ne", "Database");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList); 

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestOROp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestOROp();
            string requestURI = "http://testurl/orgs?filter=%7b%22%24{0}%22%3a%5b%7b%22{1}%22%3a%7b%22%24{2}%22%3a%22{3}%22%7d%7d%2c%7b%22{4}%22%3a%7b%22%24{5}%22%3a%22{6}%22%7d%7d%5d%7d";
            string jsonQueryString = String.Format(requestURI, "or", "coursename", "ne", "Database", "courseid", "ne", "986245748145256");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);
           
            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList); 

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestANDOp()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = clsTestData.ExpectedResult_TestANDOp();
            string requestURI = "http://testurl/orgs?filter=%7b%22%24{0}%22%3a%5b%7b%22{1}%22%3a%7b%22%24{2}%22%3a%22{3}%22%7d%7d%2c%7b%22{4}%22%3a%7b%22%24{5}%22%3a%22{6}%22%7d%7d%5d%7d";
            string jsonQueryString = String.Format(requestURI, "AnD", "coursename", "ne", "Chemistry101", "coursename", "ne", "Algorithms");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);
           
            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList); 

            //Assert
            int count = ExpectedResultList.Count();

            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestNestedPropWithNULLValues()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestNestedPropWithNULLValues();
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a{1}%7d";
            string jsonQueryString = String.Format(requestURI, "Dept.Student.Course.CourseID", "12645873358");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.OrgID, ((TestOrg)actual).OrgID);
                Assert.AreEqual(expected.OrgName, ((TestOrg)actual).OrgName);
            }
        }

        [TestMethod]
        public void TestNestedPropWithSingleValue()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestNestedPropWithSingleValue(); //{ Dept : { DeptCode: 1.2012}}
            string requestURI = "http://testurl/orgs?filter=%7b%22Dept%22+%3a+%7b%22deptcode%22+%3a+%221.2012%22%7d%7d";
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(requestURI, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.OrgID, ((TestOrg)actual).OrgID);
                Assert.AreEqual(expected.OrgName, ((TestOrg)actual).OrgName);
            }
        }


        [TestMethod]
        public void TestNestedPropWithMultipleValues()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestNestedPropWithMultipleValues(); //{ Dept : {DeptName: "Dept3", DeptCode: 192.8}}
            string requestURI = "http://testurl/orgs?filter=%7b%22dept%22+%3a+%7b%22deptname%22+%3a+%22Dept3%22%2c+%22deptcode%22+%3a+%22192.8%22%7d%7d";
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(requestURI, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.OrgID, ((TestOrg)actual).OrgID);
                Assert.AreEqual(expected.OrgName, ((TestOrg)actual).OrgName);
            }
        }

        [TestMethod]
        public void TestDataMemberProp()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestDataMember();
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a%22{1}%22%7d";
            string jsonQueryString = String.Format(requestURI, "dept.Department", "Dept1");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.DeptID, ((TestOrg)actual).Dept.DeptID);
                Assert.AreEqual(expected.Dept.DeptName, ((TestOrg)actual).Dept.DeptName);
                Assert.AreEqual(expected.Dept.DeptCode, ((TestOrg)actual).Dept.DeptCode);
            }
        }

        [TestMethod]
        public void TestAndOr()
        {
            
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestAndOrOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22Dept.Student.active%22+%3a+%22true%22%2c+%22%24or%22+%3a+%5b%7b+%22dept.student.id%22+%3a+359+%7d%2c+%7b+%22dept.student.gpa%22+%3a++3.25++%7d%5d+%7d";

            string jsonQueryString = requestURI; // String.Format(requestURI, "Dept.Student.active", "true", "dept.student.id", "359", "dept.student.id", "3.25");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }


        [TestMethod]
        public void TestOrAnd()
        {

            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedResult_TestAndOrOp();
            string requestURI = "http://testurl/orgs?filter=%7b+%22%24or%22+%3a+%5b%7b+%22dept.student.id%22+%3a+359+%7d%2c+%7b+%22dept.student.gpa%22+%3a++3.25++%7d%5d%2c+%22Dept.Student.active%22+%3a+%22true%22+%7d";

            string jsonQueryString = requestURI; // String.Format(requestURI, "Dept.Student.active", "true", "dept.student.id", "359", "dept.student.id", "3.25");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestArrayMatchExact()
        {

            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_MatchArrayExact();
            string requestURI = "http://testurl/orgs?filter=%7b%22dept.student.hobbies%22+%3a+%5b%22Knit%22%2c+%22Sew%22%2c+%22Hockey%22%5d%7d";

            string jsonQueryString = requestURI;
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestArrayMatchAll()
        {

            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_MatchArrayAll();
            string requestURI = "http://testurl/orgs?filter=%7b%22dept.student.hobbies%22+%3a+%7b%22%24all%22+%3a+%5b%22Knit%22%2c+%22Sew%22%5d%7d%7d";
            //string requestURI = "http://testurl/orgs?filter=%7b%22Colors%22+%3a+%7b%22%24all%22+%3a+%5b%22red%22%2c+%22blue%22%5d%7d%7d";


            string jsonQueryString = requestURI;
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestArrayMatchExact_Decimal()
        {

            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_MatchArrayExact_Decimal();
            string requestURI = "http://testurl/orgs?filter=%7b%22dept.student.course.RelatedSubCourseIds%22+%3a+%5b%221.11%22%2c+%221.21%22%2c+%221.31%22%5d%7d";

            string jsonQueryString = requestURI;
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.Course.CourseID, ((TestOrg)actual).Dept.Student.Course.CourseID);
                Assert.AreEqual(expected.Dept.Student.Course.CourseName, ((TestOrg)actual).Dept.Student.Course.CourseName);
                Assert.AreEqual(expected.Dept.Student.Course.CourseActive, ((TestOrg)actual).Dept.Student.Course.CourseActive);
            }
        }

        [TestMethod]
        public void TestArrayMatchAll_Decimal()
        {

            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_MatchArrayAll_Decimal();
            string requestURI = "http://testurl/orgs?filter=%7b%22dept.student.course.RelatedSubCourseIds%22+%3a+%7b%22%24all%22+%3a+%5b%222.14%22%2c+%222.13%22%5d%7d%7d";

            string jsonQueryString = requestURI;
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestFindNullValues()
        {

            //Arrange - get all students with null hobbies
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_FindNullValues();
            string requestURI = "http://testurl/orgs?filter= %7b+%22{0}%22+%3a+{1}+%7d";
            // %7b+%22{0}%22+%3a+{1}+%7d - without double quotes; %7b+%22{0}%22+%3a+%22{1}%22+%7d - with quotes
            string jsonQueryString = String.Format(requestURI, "Dept.Student.Hobbies", "null");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestEnumWithEnum()
        {
            //Arrange - get all students with country=singapore
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_FindEnum();
            string requestURI = "http://testurl/orgs?filter= %7b+%22{0}%22+%3a+%22{1}%22+%7d";
            string jsonQueryString = String.Format(requestURI, "Dept.Student.homecountry", "Singapore");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestEnumWithEnumMember()
        {
            //Arrange - get all students with country=united states
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_FindEnumWithEnumMember();
            string requestURI = "http://testurl/orgs?filter= %7b+%22{0}%22+%3a+%22{1}%22+%7d";
            string jsonQueryString = String.Format(requestURI, "Dept.Student.homecountry", "sweden");

            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act            
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestANDOpWithDuplicateKeys()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = new List<TestCourse>();
            string requestURI = "http://testurl/orgs?filter=%7B%22{0}%22:%22{1}%22,%20%22{2}%22:%22{3}%22%7D";
            string jsonQueryString = String.Format(requestURI, "coursename", "Chemistry101", "coursename", "Algorithms");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert

            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            
        }

        [TestMethod]
        public void TestANDOpWithMultipleDuplicateKeys()
        {
            //Arrange
            List<TestCourse> ExpectedResultList = new List<TestCourse>();
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a%22{1}%22%2c+%22{2}%22%3a%22{3}%22%2c+%22{4}%22%3a%22{5}%22%7d";
            string jsonQueryString = String.Format(requestURI, "coursename", "Chemistry101", "coursename", "Algorithms", "coursename", "Maths");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
        }

        [TestMethod]
        public void TestANDOpWithMultipleDuplicateKeysWithEqualityComparer()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_TestANDOpWithMultipleDuplicateKeysWithEqualityComparer();
            //{%7b%22code%22%3a%7b%22%24eq%22%3a%22CD%22%7d%2c+%22code%22%3a%7b%22%24eq%22%3a%22DT%22%7d%2c%22code%22%3a%7b%22%24eq%22%3a%22CE%22%7d%2c+%22code%22%3a%7b%22%24eq%22%3a%22RT%22%7d+%7d}
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a%22{1}%22%2c+%22{2}%22%3a%22{3}%22%2c+%22{4}%22%3a%22{5}%22%7d";
            string jsonQueryString = String.Format(requestURI, "dept.student.course.coursename", "Chemistry101", "dept.student.course.coursename", "Chemistry101", "dept.student.course.coursename", "Chemistry101");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            for (int i = 0; i < ActualResultList.Count(); i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestANDOpWithDuplicateArraysMatchAll()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ExpectedList_TestANDOpWithDuplicateArraysMatchAll();
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a%7b%22%24all%22%3a+%5b%22{1}%22%2c%22{2}%22%5d%7d%2c+%22{3}%22%3a%7b%22%24all%22%3a+%5b%22{4}%22%2c%22{5}%22%5d%7d%2c+%22{6}%22%3a%7b%22%24all%22%3a+%5b%22{7}%22%2c%22{8}%22%5d%7d%7d";
            string jsonQueryString = String.Format(requestURI, "Dept.Student.Hobbies", "Ballet", "Baseball", "Dept.Student.Hobbies", "Ballet", "Baseball", "Dept.Student.Hobbies", "Baseball", "Ballet");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            for (int i = 0; i < ActualResultList.Count(); i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

        [TestMethod]
        public void TestEqualOpWithIgnoreFilter()
        {
            //Arrange
            List<TestCourse> ExpectedResultList =  clsTestData.ListCourses ; //Results should not be filtered with IgnoreFilter=true
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a{1}%7d";
            string jsonQueryString = String.Format(requestURI, "courseid", "12645873358");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullCourseList);

            //Act    
            var Filter = new FilteringFilter(_logger);
            Filter.IgnoreFiltering = true;

            Filter.OnActionExecuted(executedContext);
            //After the filter is executed, the context response.content should contain the filtered list:
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            int count = ExpectedResultList.Count();
            //Count
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            //Items
            for (int i = 0; i < count; i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.CourseID, ((TestCourse)actual).CourseID);
                Assert.AreEqual(expected.CourseActive, ((TestCourse)actual).CourseActive);
                Assert.AreEqual(expected.CourseName, ((TestCourse)actual).CourseName);
            }
        }

        [TestMethod]
        public void TestANDOpWithDuplicateArraysMatchAllWithIgnoreFilter()
        {
            //Arrange
            List<TestOrg> ExpectedResultList = clsTestData.ListOrgs;
            string requestURI = "http://testurl/orgs?filter=%7b%22{0}%22%3a%7b%22%24all%22%3a+%5b%22{1}%22%2c%22{2}%22%5d%7d%2c+%22{3}%22%3a%7b%22%24all%22%3a+%5b%22{4}%22%2c%22{5}%22%5d%7d%2c+%22{6}%22%3a%7b%22%24all%22%3a+%5b%22{7}%22%2c%22{8}%22%5d%7d%7d";
            string jsonQueryString = String.Format(requestURI, "Dept.Student.Hobbies", "Ballet", "Baseball", "Dept.Student.Hobbies", "Ballet", "Baseball", "Dept.Student.Hobbies", "Baseball", "Ballet");
            HttpActionExecutedContext executedContext = CreateHttpActionExecutedContext(jsonQueryString, fullOrgList);

            //Act
            var Filter = new FilteringFilter(_logger);
            Filter.IgnoreFiltering = true;
            Filter.OnActionExecuted(executedContext);
            IEnumerable<object> ActualResultList = null;
            executedContext.Response.TryGetContentValue(out ActualResultList);

            //Assert
            Assert.AreEqual(ExpectedResultList.Count(), ActualResultList.Count());
            for (int i = 0; i < ActualResultList.Count(); i++)
            {
                var expected = ExpectedResultList[i];
                var actual = ActualResultList.ToList()[i];

                Assert.AreEqual(expected.Dept.Student.ID, ((TestOrg)actual).Dept.Student.ID);
                Assert.AreEqual(expected.Dept.Student.Name, ((TestOrg)actual).Dept.Student.Name);
                Assert.AreEqual(expected.Dept.Student.Active, ((TestOrg)actual).Dept.Student.Active);
            }
        }

    }
}
