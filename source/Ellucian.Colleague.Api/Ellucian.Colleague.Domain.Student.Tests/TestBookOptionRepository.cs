using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestBookOptionRepository
    {
        private string[,] bookOptions = {
                                            //CODE   DESCRIPTION    IsRequired
                                            {"R",   "Required",        "1"},
                                            {"C",   "Recommended",     "2"},
                                            {"O",   "Optional",        "2"}
                                      };


        public IEnumerable<BookOption> Get()
        {
            var bookOpts = new List<BookOption>();

            // There are 3 fields for each bookOption in the array
            var items = bookOptions.Length / 3;

            for (int x = 0; x < items; x++)
            {
                bookOpts.Add(new BookOption(bookOptions[x, 0], bookOptions[x, 1], bookOptions[x, 2] == "1"));
            }
            return bookOpts;
        }
    }
}