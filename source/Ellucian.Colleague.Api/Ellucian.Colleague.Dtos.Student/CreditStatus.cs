// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Valid academic credit statuses
    /// </summary>
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum CreditStatus
    {
        /// <summary>
        /// New credit
        /// </summary>
        New,
        /// <summary>
        /// Added credit
        /// </summary>
        Add,
        /// <summary>
        /// Dropped credit
        /// </summary>
        Dropped,
        /// <summary>
        /// Withdrawn credit
        /// </summary>
        Withdrawn,
        /// <summary>
        /// Deleted credit
        /// </summary>
        Deleted, 
        /// <summary>
        /// Cancelled credit
        /// </summary>
        Cancelled, 
        /// <summary>
        /// Transfer or Noncourse credit
        /// </summary>
        TransferOrNonCourse,
        /// <summary>
        /// Preliminary credit
        /// </summary>
        Preliminary, 
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }
}