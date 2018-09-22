// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A section transfer status code, which indicates if the section is transferable to another institution
    /// </summary>
    [Serializable]
    public class SectionTransferStatus : CodeItem
    {
        public SectionTransferStatus(string code, string description)
            : base(code, description)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            SectionTransferStatus other = obj as SectionTransferStatus;

            return other.Code.Equals(this.Code);
        }

        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }
    }
}
