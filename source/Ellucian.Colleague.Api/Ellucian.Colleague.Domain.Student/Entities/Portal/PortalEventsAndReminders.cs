// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Portal events and reminders for a user
    /// </summary>
    [Serializable]
    public class PortalEventsAndReminders
    {
        private List<PortalEvent> _portalEvents = new List<PortalEvent>();

        /// <summary>
        /// Collection of <see cref="PortalEvent"/>
        /// </summary>
        public ReadOnlyCollection<PortalEvent> Events { get; private set; }

        private List<PortalReminder> _portalReminders = new List<PortalReminder>();

        /// <summary>
        /// Collection of <see cref="PortalReminder"/>
        /// </summary>
        public ReadOnlyCollection<PortalReminder> Reminders { get; private set; }

        /// <summary>
        /// Host short date format for the Colleague environment
        /// <remarks>This is the Host Short Date Format from the INTL form</remarks>
        /// </summary>
        public string HostShortDateFormat { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortalEventsAndReminders"/> object
        /// </summary>
        public PortalEventsAndReminders(string hostShortDateFormat)
        {
            if (string.IsNullOrWhiteSpace(hostShortDateFormat))
            {
                throw new ArgumentNullException("hostShortDateFormat", "A host short date format is required when creating Portal event and reminder information.");
            }
            HostShortDateFormat = hostShortDateFormat;
            Events = _portalEvents.AsReadOnly();
            Reminders = _portalReminders.AsReadOnly();
        }

        /// <summary>
        /// Adds a <see cref="PortalEvent"> to the list of Events</see>
        /// </summary>
        /// <param name="portalEvent"><see cref="PortalEvent"/> to be added</param>
        public void AddPortalEvent(PortalEvent portalEvent)
        {
            if (portalEvent != null && !_portalEvents.Any(pe => pe.Id == portalEvent.Id))
            {
                _portalEvents.Add(portalEvent);
            }
        }

        /// <summary>
        /// Adds a <see cref="PortalReminder"> to the list of Reminders</see>
        /// </summary>
        /// <param name="portalReminder"><see cref="PortalReminder"/> to be added</param>
        public void AddPortalReminder(PortalReminder portalReminder)
        {
            if (portalReminder != null && !_portalReminders.Any(pr => pr.Id == portalReminder.Id))
            {
                _portalReminders.Add(portalReminder);
            }
        }
    }
}
