// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class BackupConfiguration
    {
        /// <summary>
        /// ID of the backup config record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Namespace of the backup config record
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Configuration version of the backup config record
        /// </summary>
        public string ConfigVersion { get; set; }

        /// <summary>
        /// ID of the product to which this configuration belongs
        /// </summary>
        public string ProductId { get; set; }


        /// <summary>
        /// Version of the product to which this configuration belongs
        /// </summary>
        public string ProductVersion { get; set; }

        /// <summary>
        /// The configuration text data
        /// </summary>
        public string ConfigData { get; set; }

        /// <summary>
        /// The time which the backup config record was created/added to the Colleague DB.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

    }
}
