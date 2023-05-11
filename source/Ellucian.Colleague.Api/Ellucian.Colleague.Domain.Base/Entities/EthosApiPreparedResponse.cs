// Copyright 2022 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiPreparedResponse
    {
        /// <summary>
        /// The text of the prepared response
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// The Json Title to use in schema
        /// </summary>
        public string JsonTitle { get; private set; }

        /// <summary>
        /// The allowed options separated by a semi-colon
        /// </summary>
        public string Options { get; private set; }

        /// <summary>
        ///Default value to use
        /// </summary>
        public string DefaultOption { get; private set; }

        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// 
        public EthosApiPreparedResponse(string text, string title, string options, string defaultOption)
        {
            Text = text;
            JsonTitle = title;
            Options = options;
            DefaultOption = defaultOption;
        }
    }
}