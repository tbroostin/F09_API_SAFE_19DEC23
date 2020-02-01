// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// An Extended Data representation.
    /// </summary>
    [DataContract]
    public class ExtendedData: BaseModel2
    {
        /// <summary>
        /// Code item constructor
        /// </summary>
        public ExtendedData() : base() { }
    }
}
