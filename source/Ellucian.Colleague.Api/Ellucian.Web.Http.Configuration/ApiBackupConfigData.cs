// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Web.Http.Configuration
{
    /// <summary>
    /// Contains all API configuration Settings for backing up.
    /// </summary>
    public class ApiBackupConfigData
    {
        public ApiSettings ApiSettings { get; set; }
        public Settings Settings { get; set; }
        public string ResourceFileChangeLogContent { get; set; }
        public string WebConfigAppSettingsMaxQueryAttributeLimit { get; set; }
        public Dictionary<string, string> BinaryFiles { get; set; }
        public Dictionary<string, string> ResourceFiles { get; set; }
    }
}
