// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;
using System;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class BookRepositoryTests
    {
        BookRepository bookRepo;
        ApiSettings apiSettingsMock;
        string setIsbn = "123-456-789";
        string setCopyrightDate = "2017";
        string setAuthor = "Author";
        string setEdition = "1st Edition";
        string setPublisher = "Publisher";
        string setTitle = "Title";
        string setStatus = "A";
        string setExternalComments = "External Comments";
        string setComment = "Comment";
        string setBookAltId1 = "Alternative Id 1";
        string setBookAltId2 = "Alternative Id 2";
        string setBookAltId3 = "Alternative Id 3";
        TestBookRepository testDataRepository;
        IEnumerable<Book> allExpectedBooks;
        Mock<IColleagueTransactionInvoker> transInvokerMock;
        Mock<ILogger> loggerMock;
        CreateBookResponse bookResponse;
        Book resultBook;

        [TestInitialize]
        public void Initialize()
        {
            testDataRepository = new TestBookRepository();
            allExpectedBooks = testDataRepository.GetAllBooks();
            loggerMock = new Mock<ILogger>();
            transInvokerMock = new Mock<IColleagueTransactionInvoker>();
            bookRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            bookRepo = null;
        }

        [TestClass]
        public class GetAsync : BookRepositoryTests
        {
            [TestMethod]
            public async Task Get_Async_BookCount_Valid_FromCache()
            {
                var allBooks = await bookRepo.GetAsync();
                Assert.AreEqual(allExpectedBooks.Count(), allBooks.Count());
            }

            [TestMethod]
            public async Task GetAllBooks_Cached_CheckBookProperties()
            {
                var bookResults = await bookRepo.GetAsync();
                foreach (var expectedBook in allExpectedBooks)
                {
                    var bookResult = bookResults[expectedBook.Id];
                    Assert.IsNotNull(bookResult);
                    Assert.AreEqual(expectedBook.Id, bookResult.Id);
                    Assert.AreEqual(expectedBook.Price, bookResult.Price);
                    Assert.AreEqual(expectedBook.PriceUsed, bookResult.PriceUsed);
                    Assert.AreEqual(expectedBook.Title, bookResult.Title);
                    Assert.AreEqual(expectedBook.Isbn, bookResult.Isbn);
                    Assert.AreEqual(expectedBook.Copyright, bookResult.Copyright);
                    Assert.AreEqual(expectedBook.Author, bookResult.Author);
                    Assert.AreEqual(expectedBook.Edition, bookResult.Edition);
                    Assert.AreEqual(expectedBook.Publisher, bookResult.Publisher);
                    Assert.AreEqual(expectedBook.IsActive, bookResult.IsActive);
                    Assert.AreEqual(expectedBook.ExternalComments, bookResult.ExternalComments);
                    Assert.AreEqual(expectedBook.Comment, bookResult.Comment);
                    Assert.AreEqual(expectedBook.AlternateID1, bookResult.AlternateID1);
                    Assert.AreEqual(expectedBook.AlternateID2, bookResult.AlternateID2);
                    Assert.AreEqual(expectedBook.AlternateID3, bookResult.AlternateID3);
                }

            }
        }

        [TestClass]
        public class GetAsync_Single : BookRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSingleBook_NullId()
            {
                var book = await bookRepo.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSingleBook_EmptyId()
            {
                var book = await bookRepo.GetAsync(string.Empty);
            }

            [TestMethod]
            public async Task GetSingleBook_CheckBookProperties_Valid()
            {

                var expectedBook = allExpectedBooks.Where(b => b.Id == "40677").FirstOrDefault();
                var bookResult = await bookRepo.GetAsync("40677");
                Assert.IsNotNull(bookResult);
                Assert.AreEqual(expectedBook.Id, bookResult.Id);
                Assert.AreEqual(expectedBook.Price, bookResult.Price);
                Assert.AreEqual(expectedBook.PriceUsed, bookResult.PriceUsed);
                Assert.AreEqual(expectedBook.Title, bookResult.Title);
                Assert.AreEqual(expectedBook.Isbn, bookResult.Isbn);
                Assert.AreEqual(expectedBook.Copyright, bookResult.Copyright);
                Assert.AreEqual(expectedBook.Author, bookResult.Author);
                Assert.AreEqual(expectedBook.Edition, bookResult.Edition);
                Assert.AreEqual(expectedBook.Publisher, bookResult.Publisher);
                Assert.AreEqual(expectedBook.IsActive, bookResult.IsActive);
                Assert.AreEqual(expectedBook.ExternalComments, bookResult.ExternalComments);
                Assert.AreEqual(expectedBook.Comment, bookResult.Comment);
                Assert.AreEqual(expectedBook.AlternateID1, bookResult.AlternateID1);
                Assert.AreEqual(expectedBook.AlternateID2, bookResult.AlternateID2);
                Assert.AreEqual(expectedBook.AlternateID3, bookResult.AlternateID3);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSingleBook_IdNotFound()
            {
                var book = await bookRepo.GetAsync("Junk");
            }

            [TestMethod]
            public async Task GetBook_CheckBookProperties_NullPrice()
            {
                var book = await bookRepo.GetAsync("AOJU");
                Assert.IsNotNull(book);
                Assert.IsNull(book.Price);
            }
        }

        [TestClass]
        public class GetBooksByIdsAsync : BookRepositoryTests
        {
            [TestMethod]
            public async Task GetBooksByIdsAsync_CheckBookProperties()
            {
                IEnumerable<string> requestedBookIds = new List<string>() { "40917", "1000" };
                var bookResults = await bookRepo.GetBooksByIdsAsync(requestedBookIds);
                foreach (var bookId in requestedBookIds)
                {
                    var expectedBook = allExpectedBooks.Where(b => b.Id == bookId).FirstOrDefault();
                    var bookResult = bookResults.Where(b => b.Id == bookId).FirstOrDefault();
                    Assert.IsNotNull(bookResult);
                    Assert.AreEqual(expectedBook.Id, bookResult.Id);
                    Assert.AreEqual(expectedBook.Price, bookResult.Price);
                    Assert.AreEqual(expectedBook.PriceUsed, bookResult.PriceUsed);
                    Assert.AreEqual(expectedBook.Title, bookResult.Title);
                    Assert.AreEqual(expectedBook.Isbn, bookResult.Isbn);
                    Assert.AreEqual(expectedBook.Copyright, bookResult.Copyright);
                    Assert.AreEqual(expectedBook.Author, bookResult.Author);
                    Assert.AreEqual(expectedBook.Edition, bookResult.Edition);
                    Assert.AreEqual(expectedBook.Publisher, bookResult.Publisher);
                    Assert.AreEqual(expectedBook.IsActive, bookResult.IsActive);
                    Assert.AreEqual(expectedBook.ExternalComments, bookResult.ExternalComments);
                    Assert.AreEqual(expectedBook.Comment, bookResult.Comment);
                    Assert.AreEqual(expectedBook.AlternateID1, bookResult.AlternateID1);
                    Assert.AreEqual(expectedBook.AlternateID2, bookResult.AlternateID2);
                    Assert.AreEqual(expectedBook.AlternateID3, bookResult.AlternateID3);
                }

            }

            [TestMethod]
            public async Task GetBooksByIdsAsync_MissingIds()
            {
                IEnumerable<string> requestedBookIds = new List<string>() { "40917", "JUNK" };
                var bookResults = await bookRepo.GetBooksByIdsAsync(requestedBookIds);
                Assert.AreEqual(1, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByIdsAsync_NotAllInCache()
            {
                IEnumerable<string> requestedBookIds = new List<string>() { "40917", "777", "888" };
                var bookResults = await bookRepo.GetBooksByIdsAsync(requestedBookIds);
                Assert.AreEqual(3, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByIdsAsync_NullArgument()
            {
                var bookResults = await bookRepo.GetBooksByIdsAsync(null);
                Assert.AreEqual(0, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByIdsAsync_EmptyArgument()
            {
                var bookResults = await bookRepo.GetBooksByIdsAsync(new List<string>());
                Assert.AreEqual(0, bookResults.Count());
            }
        }

        [TestClass]
        public class GetBooksByQueryStringAsync : BookRepositoryTests
        {

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_NullArgument()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync(null);
                Assert.AreEqual(0, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_EmptyArgument()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync(string.Empty);
                Assert.AreEqual(0, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_NonAlphanumericsOnly()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync("/&%^");
                Assert.AreEqual(0, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_Matching_Isbn()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync("isb");
                Assert.AreEqual(allExpectedBooks.Count() - 1, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_Matching_Title()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync("Adm");
                Assert.AreEqual(2, bookResults.Count());
            }

            [TestMethod]
            public async Task GetBooksByQueryStringAsync_Matching_Author()
            {
                var bookResults = await bookRepo.GetBooksByQueryStringAsync("Author");
                Assert.AreEqual(allExpectedBooks.Count() - 1, bookResults.Count());
            }


            [TestMethod]
            public async Task GetBooksByQueryStringAsync_CheckBookProperties()
            {
                string queryString = "Adm";
                var bookResults = await bookRepo.GetBooksByQueryStringAsync(queryString);
                foreach (var expectedBook in bookResults)
                {
                    var bookResult = bookResults.Where(b => b.Id == expectedBook.Id).FirstOrDefault();
                    Assert.IsNotNull(bookResult);
                    Assert.AreEqual(expectedBook.Id, bookResult.Id);
                    Assert.AreEqual(expectedBook.Price, bookResult.Price);
                    Assert.AreEqual(expectedBook.PriceUsed, bookResult.PriceUsed);
                    Assert.AreEqual(expectedBook.Title, bookResult.Title);
                    Assert.AreEqual(expectedBook.Isbn, bookResult.Isbn);
                    Assert.AreEqual(expectedBook.Copyright, bookResult.Copyright);
                    Assert.AreEqual(expectedBook.Author, bookResult.Author);
                    Assert.AreEqual(expectedBook.Edition, bookResult.Edition);
                    Assert.AreEqual(expectedBook.Publisher, bookResult.Publisher);
                    Assert.AreEqual(expectedBook.IsActive, bookResult.IsActive);
                    Assert.AreEqual(expectedBook.ExternalComments, bookResult.ExternalComments);
                    Assert.AreEqual(expectedBook.Comment, bookResult.Comment);
                    Assert.AreEqual(expectedBook.AlternateID1, bookResult.AlternateID1);
                    Assert.AreEqual(expectedBook.AlternateID2, bookResult.AlternateID2);
                    Assert.AreEqual(expectedBook.AlternateID3, bookResult.AlternateID3);
                }

            }
        }

        [TestClass]
        public class GetNonCachedBooksByIdsAsync : BookRepositoryTests
        {
            [TestMethod]
            public async Task GetNonCachedBooksByIdsAsync_CheckBookProperties()
            {
                IEnumerable<string> requestedBookIds = new List<string>() { "40917", "1000" };
                var bookResults = await bookRepo.GetNonCachedBooksByIdsAsync(requestedBookIds);
                foreach (var bookId in requestedBookIds)
                {
                    var expectedBook = allExpectedBooks.Where(b => b.Id == bookId).FirstOrDefault();
                    var bookResult = bookResults.Where(b => b.Id == bookId).FirstOrDefault();
                    Assert.IsNotNull(bookResult);
                    Assert.AreEqual(expectedBook.Id, bookResult.Id);
                    Assert.AreEqual(expectedBook.Price, bookResult.Price);
                    Assert.AreEqual(expectedBook.PriceUsed, bookResult.PriceUsed);
                    Assert.AreEqual(expectedBook.Title, bookResult.Title);
                    Assert.AreEqual(expectedBook.Isbn, bookResult.Isbn);
                    Assert.AreEqual(expectedBook.Copyright, bookResult.Copyright);
                    Assert.AreEqual(expectedBook.Author, bookResult.Author);
                    Assert.AreEqual(expectedBook.Edition, bookResult.Edition);
                    Assert.AreEqual(expectedBook.Publisher, bookResult.Publisher);
                    Assert.AreEqual(expectedBook.IsActive, bookResult.IsActive);
                    Assert.AreEqual(expectedBook.ExternalComments, bookResult.ExternalComments);
                    Assert.AreEqual(expectedBook.Comment, bookResult.Comment);
                    Assert.AreEqual(expectedBook.AlternateID1, bookResult.AlternateID1);
                    Assert.AreEqual(expectedBook.AlternateID2, bookResult.AlternateID2);
                    Assert.AreEqual(expectedBook.AlternateID3, bookResult.AlternateID3);
                }

            }

            [TestMethod]
            public async Task GetNonCachedBooksByIdsAsync_MissingIds()
            {
                IEnumerable<string> requestedBookIds = new List<string>() { "40917", "JUNK" };
                var bookResults = await bookRepo.GetNonCachedBooksByIdsAsync(requestedBookIds);
                Assert.AreEqual(1, bookResults.Count());
            }

            [TestMethod]
            public async Task GetNonCachedBooksByIdsAsync_NullArgument()
            {
                var bookResults = await bookRepo.GetNonCachedBooksByIdsAsync(null);
                Assert.AreEqual(0, bookResults.Count());
            }

            [TestMethod]
            public async Task GetNonCachedBooksByIdsAsync_EmptyArgument()
            {
                var bookResults = await bookRepo.GetNonCachedBooksByIdsAsync(new List<string>());
                Assert.AreEqual(0, bookResults.Count());
            }
        }

        [TestClass]
        public class CreateBookAsync : BookRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateBookAsync_NullTextbook()
            {
                var result = await bookRepo.CreateBookAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateBookAsync_EmptySectionId()
            {
                var book = new Book("", "123456789", "Title", "Author", "Publisher", "2017",
                                "1st Edition", true, 10m, 20m, "Comment", "External Comments", "Alternative Id 1", "Alternative Id 2", "Alternative Id 3");
                var textbook = new SectionTextbook(book, string.Empty, "R", SectionBookAction.Add);
                var result = await bookRepo.CreateBookAsync(textbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateBookAsync_NullSectionId()
            {
                var book = new Book("", "123456789", "Title", "Author", "Publisher", "2017",
                                "1st Edition", true, 10m, 20m, "Comment", "External Comments", "Alternative Id 1", "Alternative Id 2", "Alternative Id 3");
                var textbook = new SectionTextbook(book, null, "R", SectionBookAction.Add);
                var result = await bookRepo.CreateBookAsync(textbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateBookAsync_NullBook()
            {
                var textbook = new SectionTextbook(null, "SEC1", "R", SectionBookAction.Add);
                var result = await bookRepo.CreateBookAsync(textbook);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateBookAsync_ResponseError()
            {
                bookResponse.OutErrorMsgs.Add("Error 1");
                bookResponse.OutError = true;
                var book = new Book("", "123456789", "Title", "Author", "Publisher", "2017",
                                "1st Edition", true, 10m, 20m, "Comment", "External Comments", "Alternative Id 1", "Alternative Id 2", "Alternative Id 3");
                var textbook = new SectionTextbook(book, "SEC1", "R", SectionBookAction.Add);
                var result = await bookRepo.CreateBookAsync(textbook);
                loggerMock.Verify(x => x.Error(bookResponse.OutErrorMsgs[0]));
            }

            [TestMethod]
            public async Task CreateBookAsync_BookCreated()
            {
                var book = new Book("", "123456789", "Title", "Author", "Publisher", "2017",
                                "1st Edition", true, 10m, 20m, "Comment", "External Comments", "Alternative Id 1", "Alternative Id 2", "Alternative Id 3");
                var textbook = new SectionTextbook(book, "SEC1", "R", SectionBookAction.Add);
                var result = await bookRepo.CreateBookAsync(textbook);

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(Book));
            }
        }

        #region Helper Methods

        private BookRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            apiSettingsMock = new ApiSettings("null");

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                ));




            bookResponse = new CreateBookResponse()
            {
                OutBookId = "40917",
                OutError = false,
                OutErrorMsgs = new List<string>()
            };
            // Set up Transaction Invoker for a CreateBookRequest
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transInvokerMock.Object);
            transInvokerMock.Setup(transInv => transInv.ExecuteAsync<CreateBookRequest, CreateBookResponse>(It.IsAny<CreateBookRequest>())).ReturnsAsync(bookResponse);



            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);      

            // Set up Book Response
            var bookResponseData = BuildBookResponse();

            // Return Ids when doing select
            // DataReader.SelectAsync("BOOKS", "")
            var bookIds = bookResponseData.Select(b => b.Recordkey).ToArray();
            dataAccessorMock.Setup(da => da.SelectAsync("BOOKS", "")).Returns(Task.FromResult(bookIds));

            // Set up response for "noncached" book requests
            string[] ncBookIds = (new List<string>() { "40917", "1000" }).ToArray();
            var ncBooks = bookResponseData.Where(b => b.Recordkey == "40917" || b.Recordkey == "1000");
            Collection<Books> ncBookResults = new Collection<Books>();
            foreach (var ncb in ncBooks)
            {
                ncBookResults.Add(ncb);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Books>("BOOKS", ncBookIds, true)).Returns(Task.FromResult(ncBookResults));

            // Set up response for "noncached" book requests - missing from cache
            string[] otherBookIds = (new List<string>() { "777", "888"}).ToArray();
            var nonCachedResponseData = BuildOtherBookResponse();
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Books>("BOOKS", otherBookIds,true)).Returns(Task.FromResult(nonCachedResponseData));

            // Set up response for "all" book requests
            dataAccessorMock.Setup<Task<Collection<Books>>>(acc => acc.BulkReadRecordAsync<Books>("BOOKS", "", true)).Returns(Task.FromResult(bookResponseData));

            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Books>("BOOKS", bookIds, true)).Returns(Task.FromResult(bookResponseData));
            //dataAccessorMock.Setup<Task<Collection<Books>>>(acc => acc.BulkReadRecordAsync<Books>("BOOKS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(bookResponseData));

            // Missing book - not returned from db
            string[] missingingBookIdList = (new List<string>() { "40917", "JUNK" }).ToArray();
            var oneBook = bookResponseData.Where(b => b.Recordkey == "40917").FirstOrDefault();
            Collection<Books> missingBookResults = new Collection<Books>() { oneBook };
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Books>("BOOKS", missingingBookIdList, true)).Returns(Task.FromResult(missingBookResults));

            string[] missingBookIds = (new List<string>() { "JUNK" }).ToArray();
            dataAccessorMock.Setup<Task<Collection<Books>>>(acc => acc.BulkReadRecordAsync<Books>("BOOKS", missingBookIds, true)).Returns(Task.FromResult(new Collection<Books>()));

            // Responses for getting a single book "40677"
            Books bookItem = bookResponseData.Where(b => b.Recordkey == "40677").FirstOrDefault();
            dataAccessorMock.Setup<Task<Books>>(acc => acc.ReadRecordAsync<Books>("BOOKS", "40677", true)).Returns(Task.FromResult(bookItem));

            // Construct referenceData repository
            bookRepo = new BookRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            return bookRepo;
        }

        private Collection<Books> BuildBookResponse()
        {
            Collection<Books> bookData = new Collection<Books>();
            foreach (var item in allExpectedBooks)
            {
                Books book = new Books();
                book.Recordkey = item.Id.ToString();
                book.BookPrice = item.Price;
                book.BookUsedPrice = item.PriceUsed;
                book.BookIsbn = item.Isbn;
                book.BookCopyrightDate = item.Copyright;
                book.BookAuthor = item.Author;
                book.BookEdition = item.Edition;
                book.BookPublisher = item.Publisher;
                book.BookTitle = item.Title;
                book.BookStatus = item.IsActive ? "A" : "I";
                book.BookExternalComments = item.ExternalComments;
                book.BookComment = item.Comment;
                book.BookAltId = item.AlternateID1;
                book.BookAlt2Id = item.AlternateID2;
                book.BookAlt3Id = item.AlternateID3;
                bookData.Add(book);
            }
 
            return bookData;
        }

        private Collection<Books> BuildOtherBookResponse()
        {
            Collection<Books> bookData = new Collection<Books>();
            List<string> recordData = new List<string>() { "777", "888" };
            decimal setPrice = 100.00m;
            foreach (var bk in recordData)
            {
                Books book = new Books();
                book.Recordkey = bk.ToString();
                book.BookPrice = setPrice;
                book.BookUsedPrice = setPrice - 20.00m;
                book.BookIsbn = setIsbn;
                book.BookCopyrightDate = setCopyrightDate;
                book.BookAuthor = setAuthor;
                book.BookEdition = setEdition;
                book.BookPublisher = setPublisher;
                book.BookTitle = setTitle;
                book.BookStatus = setStatus;
                book.BookExternalComments = setExternalComments;
                book.BookComment = setComment;
                book.BookAltId = setBookAltId1;
                book.BookAlt2Id = setBookAltId2;
                book.BookAlt3Id = setBookAltId3;

                setPrice += 10.00m;
                bookData.Add(book);
            }

            return bookData;
        }

        #endregion
    }
}
