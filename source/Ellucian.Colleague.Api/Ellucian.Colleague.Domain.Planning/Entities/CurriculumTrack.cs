// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    [Serializable]
    public class CurriculumTrack
    {
        /// <summary>
        /// Unique identifier of this curriculum track
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Description of this curriculum track
        /// </summary>
        public string Description { get; private set; }

        public CurriculumTrack(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }

            Code = code;
            Description = description;
        }
    }
}
