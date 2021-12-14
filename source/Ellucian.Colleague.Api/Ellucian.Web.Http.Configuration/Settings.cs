// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Diagnostics;
using Ellucian.Colleague.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Ellucian.Web.Http.Configuration
{
    /// <summary>
    /// Web API settings.
    /// </summary>
    public class Settings
    {
        private ColleagueSettings colleagueSettings;
        private LogEventLevel logLevel;
        private string profileName;


        /// <summary>
        /// Colleague setting parameters
        /// </summary>
        public ColleagueSettings ColleagueSettings { get { return colleagueSettings; } }

        /// <summary>
        /// Log level
        /// </summary>
        public LogEventLevel LogLevel { get { return logLevel; } }
        public String ProfileName
        {
            get
            {
                return profileName;
            }
            set
            {
                profileName = value ?? string.Empty;
            }
        }
        
        public Settings(ColleagueSettings colleagueSettings, LogEventLevel logLevel)
        {
            if (colleagueSettings == null)
            {
                throw new ArgumentNullException("colleagueSettings");
            }
            this.colleagueSettings = colleagueSettings;
            this.logLevel = logLevel;
        }
    }
}
