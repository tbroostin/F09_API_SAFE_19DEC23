using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// PhoneType
    /// </summary>
    [Serializable]
    public class PhoneType : GuidCodeItem
    {
         /// <summary>
         /// The <see cref="PhoneTypeCategory">type</see> of phone type for this entity
         /// </summary>
         private PhoneTypeCategory _phoneTypeCategory;
         public PhoneTypeCategory PhoneTypeCategory { get { return _phoneTypeCategory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneType"/> class.
        /// </summary>
        /// <param name="phoneTypeCategory">The phone type</param>
        public PhoneType(string guid, string code, string description, PhoneTypeCategory phoneTypeCategory)
            : base(guid, code, description)
        {
            _phoneTypeCategory = phoneTypeCategory;
        }
    }
}
