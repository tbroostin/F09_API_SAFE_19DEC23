// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertWorkCaseSetReminder DTO to RetentionAlertWorkCaseSetReminder entity
    /// </summary>
    public class RetentionAlertCaseManageRemindersDtoToEntityAdapter : AutoMapperAdapter<Dtos.Student.RetentionAlertWorkCaseManageReminders, Domain.Base.Entities.RetentionAlertWorkCaseManageReminders>
    {
        public RetentionAlertCaseManageRemindersDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.RetentionAlertWorkCaseManageReminder, Dtos.Student.RetentionAlertWorkCaseManageReminder>();
        }        

        public override Domain.Base.Entities.RetentionAlertWorkCaseManageReminders MapToType(Dtos.Student.RetentionAlertWorkCaseManageReminders source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var reminders = new List<Domain.Base.Entities.RetentionAlertWorkCaseManageReminder>();
            foreach (var item in source.Reminders)
            {
                reminders.Add(
                    new Domain.Base.Entities.RetentionAlertWorkCaseManageReminder()
                    {
                        CaseItemsId = item.CaseItemsId,
                        ClearReminderDateFlag = item.ClearReminderDate
                    });
            }

            return new Domain.Base.Entities.RetentionAlertWorkCaseManageReminders(source.UpdatedBy, reminders);
        }
    }
}