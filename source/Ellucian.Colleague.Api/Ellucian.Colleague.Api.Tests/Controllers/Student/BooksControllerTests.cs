// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Net.Http.Headers;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class BooksControllerTests
    {
        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        private BooksController booksController;
        private Mock<IBookRepository> BookRepositoryMock;
        private IBookRepository bookRepository;
        private IAdapterRegistry AdapterRegistry;
        private IDictionary<string, Ellucian.Colleague.Domain.Student.Entities.Book> allBooks;
        ILogger logger = new Mock<ILogger>().Object;
        private Ellucian.Colleague.Domain.Student.Entities.Book book = null;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            BookRepositoryMock = new Mock<IBookRepository>();
            bookRepository = BookRepositoryMock.Object;
            logger = new Mock<ILogger>().Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Book, Book>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allBooks = await new TestBookRepository().GetAsync();
            book = allBooks.Values.Where(b => b.Id == "1000").FirstOrDefault();

            booksController = new BooksController(AdapterRegistry, bookRepository, logger) { Request = new HttpRequestMessage() };
            booksController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            booksController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Book, Book>();

            BookRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult( book));
            BookRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(allBooks));
            BookRepositoryMock.Setup(x => x.GetBooksByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Book>() { allBooks.Values.FirstOrDefault() });
            BookRepositoryMock.Setup(x => x.GetNonCachedBooksByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Book>() { allBooks.Values.FirstOrDefault(), allBooks.Values.LastOrDefault() });
            BookRepositoryMock.Setup(x => x.GetBooksByQueryStringAsync(It.IsAny<string>())).ReturnsAsync(new List<Domain.Student.Entities.Book>() { allBooks.Values.ElementAt(1) });
        }

        [TestCleanup]
        public void Cleanup()
        {
            booksController = null;
            bookRepository = null;
        }

        [TestClass]
        public class GetAsync : BooksControllerTests
        {
            [TestMethod]
            public async Task GetABook_BookProperties()
            {
                var bookItem = await booksController.GetAsync("1000");
                Assert.IsTrue(bookItem is Book);
                Assert.AreEqual(book.Price, bookItem.Price);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BooksController_Exception_ReturnsHttpResponseException_BadRequest_GetAsync_Id()
            {
                try
                {
                    BookRepositoryMock.Setup(x => x.GetAsync("1000")).Throws(new ApplicationException());
                    var books = await booksController.GetAsync("1000");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BooksController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    BookRepositoryMock.Setup(x => x.GetAsync("1000"))
                    .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await booksController.GetAsync("1000");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }

        [TestClass]
        public class QueryBooksByPostAsync : BooksControllerTests
        {
            BookQueryCriteria criteria;

            [TestInitialize]
            public void QueryBooksByPostAsync_Initialize()
            {
                criteria = new BookQueryCriteria()
                {
                    Ids = new List<string>() { "101", "102" },
                    QueryString = "1-2345-6789-0"
                };
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryBooksByPostAsync_Null_Ids_and_QueryString()
            {
                criteria.Ids = null;
                criteria.QueryString = null;
                var books = await booksController.QueryBooksByPostAsync(criteria);
            }

            [TestMethod]
            public async Task QueryBooksByPostAsync_Ids_Cache()
            {
                criteria.QueryString = null;
                booksController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var books = await booksController.QueryBooksByPostAsync(criteria);
                Assert.AreEqual(1, books.Count());
            }

            [TestMethod]
            public async Task QueryBooksByPostAsync_Ids_No_Cache()
            {
                criteria.QueryString = null;
                booksController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var books = await booksController.QueryBooksByPostAsync(criteria);
                Assert.AreEqual(2, books.Count());
            }

            [TestMethod]
            public async Task QueryBooksByPostAsync_QueryString()
            {
                criteria.Ids = null;
                booksController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var books = await booksController.QueryBooksByPostAsync(criteria);
                Assert.AreEqual(1, books.Count());
            }

            [TestMethod]
            public async Task QueryBooksByPostAsync_Ids_and_QueryString_Cache()
            {
                booksController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var books = await booksController.QueryBooksByPostAsync(criteria);
                Assert.AreEqual(2, books.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryBooksByPostAsync_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    BookRepositoryMock.Setup(x => x.GetAsync(criteria.Ids.First())).Throws(new ArgumentNullException());
                    BookRepositoryMock.Setup(x => x.GetNonCachedBooksByIdsAsync(criteria.Ids)).Throws(new ArgumentNullException());
                    BookRepositoryMock.Setup(x => x.GetBooksByQueryStringAsync(criteria.QueryString)).Throws(new ArgumentNullException());
                    var books = await booksController.QueryBooksByPostAsync(criteria);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
