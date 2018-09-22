using System.Collections.Generic;
using System;
using System.Linq;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Generic data page
    /// </summary>
    public class DataPage<T>
    {
        /// <summary>
        /// Default constructor appeases serializer in implementing class
        /// </summary>
        public DataPage()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allItems">All items for this data page of type T</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="pageIndex">Page index</param>
        public DataPage(IEnumerable<T> allItems, int pageSize, int pageIndex)
        {
            if (allItems == null)
            {
                throw new NullReferenceException("Paging results cannot be null");
            }

            // Force a default when the count per page is too low
            if (pageSize < 1)
            {
                pageSize = Int16.MaxValue;
            }

            var allPageCount = (int)Math.Ceiling((decimal)allItems.Count() / pageSize);

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            if (pageIndex > allPageCount)
            {
                pageIndex = allPageCount;
            }

            TotalItems = allItems.Count();
            TotalPages = (int)Math.Ceiling((decimal)TotalItems / pageSize);
            PageSize = pageSize;
            CurrentPageIndex = pageIndex;
            CurrentPageItems = allItems.Skip(PageSize * (CurrentPageIndex - 1)).Take(PageSize);
        }

        /// <summary>
        /// Total items over all pages
        /// </summary>
        public int TotalItems { get; set; }
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>
        /// Generic list of items to appear on this page
        /// </summary>
        public IEnumerable<T> CurrentPageItems { get; set; }
    }
}