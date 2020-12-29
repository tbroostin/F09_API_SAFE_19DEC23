// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the types of course catalog for which sorting can occur on sections.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CatalogSortType
    {
        
        /// <summary>
        /// No need to sort
        /// </summary>
        None,
        /// <summary>
        /// Status has two values- Waitlisted or Open
        /// Section is Waitlisted when calculated Waitlisted property of a Section is >=1
        /// </summary>
        Status,
        /// <summary>
        /// This is combination of CourseName + Section number
        /// </summary>
        SectionName,
        /// <summary>
        /// Number of seats available, which is Available property in Section, If there is no capacity, Available property is null
        /// </summary>
        SeatsAvailable,
        /// <summary>
        /// Location of the section
        /// </summary>
        Location
        
    }
}
