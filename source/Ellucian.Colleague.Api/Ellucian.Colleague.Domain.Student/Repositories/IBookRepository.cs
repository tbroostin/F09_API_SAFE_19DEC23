// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IBookRepository
    {
        Task<Book> GetAsync(string id);
        Task<IDictionary<string, Book>> GetAsync();
        Task<IEnumerable<Book>> GetNonCachedBooksByIdsAsync(IEnumerable<string> bookIds);
        Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<string> bookIds);
        /// <summary>
        /// GetBooksByIds is used to retrieve a set of selected book ids by first checking for the item in cache
        /// then, if not finding any, looking for missing items directly in the database. 
        /// </summary>
        /// <param name="bookIds">Query string to be used for searches against book titles, authors, and International Standard Book Numbers (ISBN)</param>
        /// <returns>Books</returns>
        Task<IEnumerable<Book>> GetBooksByQueryStringAsync(string queryString);
        /// <summary>
        /// Create a book
        /// </summary>
        /// <param name="textbook"><see cref="SectionTextbook"/></param>
        /// <returns>A <see cref="Book"/> object.</returns>
        Task<Book> CreateBookAsync(SectionTextbook textbook);
    }
}
