using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class InstructionalPlatform : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionalPlatform"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the InstructionalPlatform item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="title">The Title</param>
        public InstructionalPlatform(string guid, string code, string description)
            : base(guid, code, description)
        { }

    }
}
