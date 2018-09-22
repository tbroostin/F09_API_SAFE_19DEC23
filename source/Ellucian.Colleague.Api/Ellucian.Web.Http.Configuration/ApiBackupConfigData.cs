// Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Web.Http.Configuration
{
    /// <summary>
    /// Stores all API settings for backing up.
    /// </summary>
    public class ApiBackupConfigData
    {
        public ApiSettings ApiSettings { get; set; }
        public Settings Settings { get; set; }
        public string ResourceFileChangeLogContent { get; set; }
        public string WebConfigAppSettingsMaxQueryAttributeLimit { get; set; }

    }
}
