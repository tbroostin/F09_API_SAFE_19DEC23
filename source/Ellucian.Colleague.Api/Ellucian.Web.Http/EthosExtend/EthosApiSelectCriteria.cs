// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Represents a single element of data for an extended property and the details of it and where it is in Colleague
    /// </summary>
    [Serializable]
    public class EthosApiSelectCriteria
    {
        /// <summary>
        /// Select Connector, such as "WITH" or "AND"
        /// </summary>
        public string SelectConnector { get; private set; }

        /// <summary>
        /// Select Column Name
        /// </summary>
        public string SelectColumn { get; private set; }

        /// <summary>
        /// Select Operation such as "EQ" or "NE"
        /// </summary>
        public string SelectOper { get; private set; }

        /// <summary>
        /// Select Value, including the quotes.
        /// </summary>
        public string SelectValue { get; private set; }

        /// <summary>
        /// constructor for the row of extended data
        /// </summary>
        /// <param name="selectConnector">Select Connector</param>
        /// <param name="selectColumn">Select Column</param>
        /// <param name="selectOper">Select Operation</param>
        /// <param name="selectValue">Select Value (with quotes)</param>
        public EthosApiSelectCriteria(string selectConnector, string selectColumn, string selectOper, string selectValue)
        {
            SelectConnector = selectConnector;
            SelectColumn = selectColumn;
            SelectOper = selectOper;
            SelectValue = selectValue;
        }

    }
}