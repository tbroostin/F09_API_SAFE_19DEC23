// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestCatalogRepository : ICatalogRepository
    {
        private ICollection<Catalog> Catalogs = new List<Catalog>();

        public async Task<ICollection<Catalog>> GetAsync()
        {
            Populate();
            return await Task.FromResult(Catalogs);
        }

        public async Task<ICollection<Catalog>> GetAsync(bool bypassCache = false)
        {
            Populate();
            return await Task.FromResult(Catalogs);
        }

        public async Task<string> GetCatalogGuidAsync(string code)
        {
            Populate();
            return await Task.FromResult(Catalogs.FirstOrDefault(c=>c.Code == code).Guid);
        }

        private void Populate()
        {
            Catalogs.Add(new Catalog("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2010", new DateTime(2010, 1, 1)) { AcadPrograms = new List<string>() { "BA.MATH" } });
            Catalogs.Add(new Catalog("73244057-D1EC-4094-A0B7-DE602533E3A6", "2011", new DateTime(2011, 1, 1))
            {
                EndDate = new DateTime(2012, 12, 31),
                AcadPrograms = new List<string>() { "BA.MATH" }
            });
            Catalogs.Add(new Catalog("2012", new DateTime(2012, 1, 1)) { EndDate = new DateTime(2012, 12, 31) });
            Catalogs.Add(new Catalog("2013", new DateTime(2013, 1, 1)));
            Catalogs.Add(new Catalog("2014", new DateTime(2014, 1, 1)));
            Catalogs.Add(new Catalog("2015", new DateTime(2015, 1, 1)));
        }
    }
}