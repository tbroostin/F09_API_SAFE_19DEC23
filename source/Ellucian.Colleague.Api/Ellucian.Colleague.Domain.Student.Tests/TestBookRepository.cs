// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestBookRepository : IBookRepository
    {
        private string[,] books = {

                                {"40917", "isbn1", "Finger Printing for the Ages", "Author1", "Publisher1", "2017",
                                 "edition1", "A", "200.00", "400.00", "comment1", "externalcomments1",
                                 "alternateid11", "alternateid21", "alternateid31"},
                                {"1000", "isbn2", "Admissions", "Author2", "Publisher2", "2017",
                                 "edition2", "A", "200.00", "300.00", "comment2", "externalcomments2",
                                 "alternateid12", "alternateid22", "alternateid32"},
                                {"1001", "isbn3", "Attaboy", "Author3", "Publisher3", "2017",
                                 "edition3", "", "20.20", "45.44", "comment3", "externalcomments3",
                                 "alternateid13", "alternateid23", "alternateid33"},
                                {"40677",  "isbn4", "The History of Chad", "Author4", "Publisher4", "2017",
                                 "edition4", "A", "21.05", "44.26", "comment4", "externalcomments4",
                                 "alternateid14", "alternateid24", "alternateid34"},
                                {"111",   "isbn5", "Animal Science", "Author5", "Publisher5", "2017",
                                 "edition5", "A", "15.00", "150.00", "comment5", "externalcomments5",
                                 "alternateid15", "alternateid25", "alternateid35"},
                                {"AOJU",  "isbn6",  "Administration of Justice", "Author6", "Publisher6", "2017",
                                 "edition6", "", "", "", "comment6", "externalcomments6",
                                 "alternateid16", "alternateid26", "alternateid36"},
                                {"231",  "1-902-3345-21",  "Some Book", "John Smith", "Publisher6", "2017",
                                 "edition6", "", "", "", "comment6", "externalcomments6",
                                 "alternateid16", "alternateid26", "alternateid36"},
                                };

        public List<Book> GetAllBooks()
        {
            var allBooks = new List<Book>();
            var items = books.Length / 15;

            for (int x = 0; x < items; x++)
            {
                var book = new Book(books[x, 0], books[x, 1], books[x, 2], books[x, 3], books[x, 4], books[x, 5],
                    books[x, 6], books[x, 7] == "A", null, null, books[x, 10], books[x, 11], books[x, 12],
                    books[x, 13], books[x, 14]);
                if (!string.IsNullOrEmpty(books[x, 9]))
                {
                    book.Price = decimal.Parse(books[x, 9]);
                }
                if (!string.IsNullOrEmpty(books[x, 8]))
                {
                    book.PriceUsed = decimal.Parse(books[x, 8]);
                }
                allBooks.Add(book);

            }
            return allBooks;
        }

        public async Task<IDictionary<string, Book>> GetAsync()
        {
            Dictionary<string, Book> bookDictionary = new Dictionary<string, Book>();
            // There are 15 fields for each book in the array (only 15 used at the moment)
            var allBooks = GetAllBooks();

            foreach (var book in allBooks)
            {
                bookDictionary[book.Id] = book;
            }
            return await Task.FromResult(bookDictionary);
        }

        public async Task<Book> GetAsync(string id)
        {
            Book book = (await GetAsync()).Values.Where(b => b.Id == id).FirstOrDefault();
            return book;
        }

        public async Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<string> bookIds)
        {
            var bookEntities = new List<Book>();
            var allBooks = await GetAsync();
            foreach (var id in bookIds)
            {
                Book bookEntity;
                if (!allBooks.TryGetValue(id, out bookEntity))
                {
                    // skip it
                }
            }
            return bookEntities;
        }

        public async Task<IEnumerable<Book>> GetNonCachedBooksByIdsAsync(IEnumerable<string> bookIds)
        {
            return await GetBooksByIdsAsync(bookIds);
        }

        public async Task<IEnumerable<Book>> GetBooksByQueryStringAsync(string queryString)
        {
            throw new NotImplementedException();
        }
        public async Task<Book> CreateBookAsync(SectionTextbook textbook)
        {
            throw new NotImplementedException();
        }
    }
}
