// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Person Name Type
    /// </summary>
    [Serializable]
    public class PersonNameTypeItem : GuidCodeItem
    {
         private PersonNameType _type;
         public PersonNameType Type { get { return _type; } }

        /// <summary>
         /// Initializes a new instance of the <see cref="PersonNameTypeItem"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The person name type category</param>
         public PersonNameTypeItem(string guid, string code, string description, PersonNameType type)
             : base(guid, code, description)
         {
             _type = type;
         } 
    }
}