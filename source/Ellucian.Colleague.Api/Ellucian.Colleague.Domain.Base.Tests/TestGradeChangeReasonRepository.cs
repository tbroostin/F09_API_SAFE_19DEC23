using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestGradeChangeReasonRepository
    {
        public IEnumerable<GradeChangeReason> Get()
        {
            var gradeChangeReason = new List<GradeChangeReason>
            {
                new GradeChangeReason("bf775687-6dfe-42ef-b7c0-aee3d9e681cf", "U", "Updated"),
                new GradeChangeReason("3cd7b3d2-665e-45ba-8496-912e69a113ce", "UN", "UnUpdated"),
                new GradeChangeReason("13B0F553-8E52-4E1A-91C0-5E7A345A6AEB", "OE", "Original Entry"),
                new GradeChangeReason("6a65cd15-aea4-4c1f-9cbe-9a75a4aafac8", "IC", "Instructor Correction"),
                new GradeChangeReason("ae54361c-fd86-4c12-9022-7667d43235bf", "EE", "Entry Error"),
                new GradeChangeReason("aabd8c45-b7e5-4858-b16d-a8bd5c63f0b2", "MC", "Make-up Work Complete")
            };

            return gradeChangeReason;
        }
    }
}
