// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories {
    /// <summary>
    /// Interface for important number repositories
    /// </summary>
    public interface IImportantNumberRepository 
    {
        /// <summary>
        /// Important Numbers
        /// </summary>
        IEnumerable<ImportantNumber> Get();

        /// <summary>
        /// Important Number Categories
        /// </summary>
        IEnumerable<ImportantNumberCategory> ImportantNumberCategories { get; }
    }
}
