// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BookRepository : BaseColleagueRepository, IBookRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public BookRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        public async Task<Book> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Book id may not be null or empty");
            }
            Book bookEntity;
            try
            {
                var allBooks = await GetAsync();
                if (!allBooks.TryGetValue(id, out bookEntity))
                {
                    // Not in cache. Go get it directly from db
                    var bookItems = await GetNonCachedBooksByIdsAsync(new List<string>() { id } );
                    bookEntity = bookItems.FirstOrDefault();
                } 
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Book not found for Id " + id);
            }
            return bookEntity;
        }


        /// <summary>
        /// Gets a cached collection of <see cref="Book"/> objects
        /// </summary>
        /// <returns>Collection of <see cref="Book"/> objects</returns>
        public async Task<IDictionary<string,Book>> GetAsync()
        {
            var bookDict = await GetOrAddToCacheAsync<Dictionary<string, Book>>("AllBooks",
                        async () =>
                        {
                logger.Info("Getting books from database... ");

                // Select from Books to get all the Ids
                var bookIds = await DataReader.SelectAsync("BOOKS", "");
                // Retrieve books in chunks from the database
                var bookData = new List<Books>();
                            for (int i = 0; i < bookIds.Count(); i += readSize)
                            {
                                var subList = bookIds.Skip(i).Take(readSize).ToArray();
                                var bulkData = await DataReader.BulkReadRecordAsync<Books>("BOOKS", subList);
                                bookData.AddRange(bulkData);
                            }

                            var bookList = BuildBooks(bookData.ToList());
                            return bookList;
                        }
                        );
            return bookDict;
        }

        /// <summary>
        /// Used to retrieve a set of books given a list of IDs directly from the database
        /// </summary>
        /// <param name="bookIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Book>> GetNonCachedBooksByIdsAsync(IEnumerable<string> bookIds)
        {
            var bookResult = new List<Book>();
            if (bookIds == null || !bookIds.Any())
            {
                return bookResult;
            }
            Collection<Books> bookData = await DataReader.BulkReadRecordAsync<Books>("BOOKS", bookIds.ToArray());
            var bookList = BuildBooks(bookData.ToList());
            if (bookList != null && bookList.Keys.Any())
            {
                bookResult = bookList.Values.ToList();
            }
            return bookResult;

        }

        /// <summary>
        /// GetBooksByIds is used to retrieve a set of selected book ids by first checking for the item in cache
        /// then, if not finding any, looking for missing items directly in the database. 
        /// </summary>
        /// <param name="bookIds">List of Book Ids identifying books to retrieve</param>
        /// <returns>Books</returns>
        public async Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<string> bookIds)
        {
            var books = new List<Book>();
            List<string> missingBookIds = new List<string>();
            if ((bookIds != null) && bookIds.Any())
            {
                var cachedBooks = await GetAsync();
                foreach (var id in bookIds)
                {
                    if (cachedBooks.ContainsKey(id))
                    {
                        books.Add(cachedBooks[id]);
                    } else
                    {
                        missingBookIds.Add(id);
                    }
                }

                if (missingBookIds.Any())
                {
                    var otherBooks = await GetNonCachedBooksByIdsAsync(missingBookIds.ToArray());
                    if (otherBooks != null && otherBooks.Any()) { books.AddRange(otherBooks); }
                }
            }
            return books;
        }

        /// <summary>
        /// Create a book
        /// </summary>
        /// <param name="textbook"><see cref="SectionTextbook"/></param>
        /// <returns>A <see cref="Book"/> object.</returns>
        public async Task<Book> CreateBookAsync(SectionTextbook textbook)
        {
            if (textbook == null)
            {
                throw new ArgumentNullException("textbook", "Textbook can not be null.");
            }

            if (string.IsNullOrEmpty(textbook.SectionId))
            {
                throw new ArgumentNullException("textbook.SectionId", "Section Id may not be null or empty.");
            }
            if (textbook.Textbook == null)
            {
                throw new ArgumentNullException("textbook.Textbook", "The book being updated can not be null.");
            }

            var request = new CreateBookRequest();
            request.InBookAuthor = textbook.Textbook.Author;
            request.InBookComment = textbook.Textbook.Comment;
            request.InBookCopyrightDate = textbook.Textbook.Copyright;
            request.InBookEdition = textbook.Textbook.Edition;
            request.InBookExternalComments.Add(textbook.Textbook.ExternalComments);
            request.InBookIsbn = textbook.Textbook.Isbn;
            request.InBookPrice = textbook.Textbook.Price;
            request.InBookPublisher = textbook.Textbook.Publisher;
            request.InBookTitle = textbook.Textbook.Title;
            request.InBookUsedPrice = textbook.Textbook.PriceUsed;

            var response = await transactionInvoker.ExecuteAsync<CreateBookRequest, CreateBookResponse>(request);

            if (response.OutErrorMsgs != null && response.OutErrorMsgs.Count > 0)
            {
                string error = "An error occurred while trying to create book";
                logger.Error(error);
                response.OutErrorMsgs.ForEach(message => logger.Error(message));
                throw new ApplicationException(error);
            }

            var bookEntity = await GetAsync(response.OutBookId);
            return bookEntity;
        }

        /// <summary>
        /// GetBooksByIds is used to retrieve a set of selected book ids by first checking for the item in cache
        /// then, if not finding any, looking for missing items directly in the database. 
        /// </summary>
        /// <param name="bookIds">Query string to be used for searches against book titles, authors, and International Standard Book Numbers (ISBN)</param>
        /// <returns>Books</returns>
        public async Task<IEnumerable<Book>> GetBooksByQueryStringAsync(string queryString)
        {
            var searchResults = new List<Book>();
            List<string> missingBookIds = new List<string>();
            if (!string.IsNullOrEmpty(queryString))
            {
                var cachedBooks = await GetAsync();

                queryString = queryString.ToLower();
                string regexIsbn = null;
                regexIsbn = StripNonAlphanumerics(queryString);
                if (string.IsNullOrWhiteSpace(queryString))
                {
                    return searchResults;
                }

                searchResults = cachedBooks.Where(item => StripNonAlphanumerics(item.Value.Isbn).ToLower().Contains(regexIsbn.ToLower()) ||
                    item.Value.Author.ToLower().Contains(queryString) ||
                    item.Value.Title.ToLower().Contains(queryString)).Select(item => item.Value).ToList();
            }
            return searchResults;
        }

        private Dictionary<string, Book> BuildBooks(List<Books> bookData)
        {
            // Get all book details
            var bookDictionary = new Dictionary<string, Book>();
            if (bookData != null && bookData.Any())
            {
                foreach (var book in bookData)
                {
                    try
                    {
                        bool isActive = book.BookStatus == "A";
                        Book bookItem = new Book(book.Recordkey, book.BookIsbn, book.BookTitle, book.BookAuthor, book.BookPublisher,
                            book.BookCopyrightDate, book.BookEdition, isActive, book.BookUsedPrice,
                            book.BookPrice, book.BookComment, book.BookExternalComments,
                            book.BookAltId, book.BookAlt2Id, book.BookAlt3Id);
                        bookDictionary[bookItem.Id] = bookItem;

                    }
                    catch (Exception ex)
                    {
                        LogDataError("Book", book.Recordkey, bookData, ex);
                    } 
                }
               
            }
            return bookDictionary;
        }

        /// <summary>
        /// Removes whitespace and '-' values from input
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string StripNonAlphanumerics(string str)
        {
            Regex regexObj = new Regex(@"[\s-]");
            return regexObj.Replace(str, "");
        }
    }
}
