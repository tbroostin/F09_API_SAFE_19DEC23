using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CampusOrgFacultyRole
    {
        public string Id { get; private set; }
        public decimal? IntendedLoad { get; private set; }
        public string Role { get; private set; }

        // This is the reference to the roledescription for Campus Org Members
        public string Assignment { get; private set; }

        public string ShortTitle { get; private set; }

        public string Name { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public CampusOrgFacultyRole(string id, string role, decimal? intendedLoad, string assignment, DateTime? startDate, DateTime? endDate)
        {
            Id = id;
            Role = role;
            IntendedLoad = intendedLoad;
            Assignment = assignment;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
