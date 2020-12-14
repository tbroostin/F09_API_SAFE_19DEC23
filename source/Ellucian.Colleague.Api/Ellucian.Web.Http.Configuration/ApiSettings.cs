// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ellucian.Web.Http.Configuration
{
    public class ApiSettings
    {
        /// <summary>
        /// The in-process cache name from the Colleague valcode WEBAPI.CACHE.PROVIDERS
        /// </summary>
        public const string INPROC_CACHE = "INPROC";

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id == 0)
                { _Id = value; }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (string.IsNullOrEmpty(_Name))
                { _Name = value; }
                else
                {
                    throw new ArgumentException("Name cannot be changed");
                }
            }
        }

        private int _Version;
        public int Version
        {
            get { return _Version; }
            set
            {
                if (_Version == 0)
                { _Version = value; }
                else
                {
                    throw new ArgumentException("Version cannot be changed");
                }
            }
        }

        public string PhotoURL { get; set; }

        public string PhotoType { get; set; }

        public Dictionary<string, string> PhotoHeaders { get; set; }

        public bool PhotoConfiguration
        {
            get
            {
                {
                    if (string.IsNullOrEmpty(PhotoURL) || string.IsNullOrEmpty(PhotoType))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// The path to the report logo image file
        /// </summary>
        public string ReportLogoPath { get; set; }

        /// <summary>
        /// The path to the report watermark image file
        /// </summary>
        public string UnofficialWatermarkPath { get; set; }

        /// <summary>
        /// Bucket size for DataReader bulk read requests to prevent memory overflow.
        /// </summary>
        /// <value>
        /// The size of the bulk read.
        /// </value>
        public int BulkReadSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the "link" and "self" response headers in API responses utilizing paging filters
        /// </summary>
        public bool IncludeLinkSelfHeaders { get; set; }

        /// <summary>
        /// Gets or sets the colleague server time zone.
        /// </summary>
        /// <value>
        /// The colleague time zone.
        /// </value>
        public string ColleagueTimeZone { get; set; }

        /// <summary>
        /// Gets or sets the cache provider for this Web API instance.
        /// </summary>
        /// <value>
        /// The cache provider.
        /// </value>
        public string CacheProvider { get; set; }

        /// <summary>
        /// Gets or sets the cache host (for distributed cache provider)
        /// </summary>
        /// <value>
        /// The cache host.
        /// </value>
        public string CacheHost { get; set; }

        /// <summary>
        /// Gets or sets the cache port (for distributed cache provider)
        /// </summary>
        /// <value>
        /// The cache port.
        /// </value>
        public ushort? CachePort { get; set; }

        /// <summary>
        /// Gets or sets the list of supported cache providers.
        /// </summary>
        /// <value>
        /// The cache providers valcode.
        /// </value>
        public List<KeyValuePair<string, string>> SupportedCacheProviders { get; set; }

        /// <summary>
        /// Gets or sets the list of valid debug trace levels.
        /// </summary>
        /// <value>
        /// The debug trace levels.
        /// </value>
        public List<KeyValuePair<string, TraceLevel?>> DebugTraceLevels { get; set; }

        /// <summary>
        /// If true, everytime config settings changes are saved, 
        /// API config data will be backed up to the Colleague DB.
        /// Note: The backup action requires certain user permissions.
        /// </summary>
        public bool EnableConfigBackup { get; set; }

        /// <summary>
        /// Max size, in bytes, a file attachment HTTP request can be.
        /// </summary>
        public long AttachRequestMaxSize { get; set; }

        /// <summary>
        /// True if the detailed health check API is enabled
        /// </summary>
        public bool DetailedHealthCheckApiEnabled { get; set; }

        /// <summary>
        /// Default constructor for deserialization
        /// </summary>
        public ApiSettings()
        {
        }

        public ApiSettings(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Name must be specified.");
            }
            _Id = 0;
            _Name = name;
            _Version = 0;
            PhotoHeaders = new Dictionary<string, string>();
            BulkReadSize = 5000;
            IncludeLinkSelfHeaders = false;
            ColleagueTimeZone = TimeZoneInfo.Local.Id; // default to API server's time zone.
            EnableConfigBackup = false;
            AttachRequestMaxSize = 26214400;  // 25 MB
            DetailedHealthCheckApiEnabled = false;
        }

        public ApiSettings(int id, string name, int version) : this(name)
        {
            Id = id;
            Version = version;
        }
    }
}
