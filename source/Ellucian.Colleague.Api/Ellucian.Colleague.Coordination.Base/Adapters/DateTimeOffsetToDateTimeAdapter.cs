// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters {

    /// <summary>
    /// Adapter for DateTimeOffset to DateTime. Created to support obsolete
    /// adapters which need to convert DateTimeOffset properties to DateTime properties
    /// </summary>
    public class DateTimeOffsetToDateTimeAdapter : AutoMapperAdapter<DateTimeOffset, DateTime> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetToDateTimeAdapter" /> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public DateTimeOffsetToDateTimeAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
        }

        public override DateTime MapToType(DateTimeOffset Source)
        {
            return Source.DateTime;
        }
    }
}
