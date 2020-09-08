// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertWorkCaseSetReminder DTO to RetentionAlertWorkCaseSetReminder entity
    /// </summary>
    public class RetentionAlertCaseSetReminderDtoToEntityAdapter : AutoMapperAdapter<Dtos.Student.RetentionAlertWorkCaseSetReminder, Domain.Base.Entities.RetentionAlertWorkCaseSetReminder>
    {
        public RetentionAlertCaseSetReminderDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }        

        public override Domain.Base.Entities.RetentionAlertWorkCaseSetReminder MapToType(Dtos.Student.RetentionAlertWorkCaseSetReminder source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Domain.Base.Entities.RetentionAlertWorkCaseSetReminder(source.UpdatedBy, source.ReminderDate, source.Summary, source.Notes);
        }
    }
}