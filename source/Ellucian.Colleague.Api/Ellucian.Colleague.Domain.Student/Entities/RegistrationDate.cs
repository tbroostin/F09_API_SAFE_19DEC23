using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationDate
    {
        private readonly DateTime? _registrationStartDate;
        private readonly DateTime? _registrationEndDate;
        private readonly DateTime? _preRegistrationStartDate;
        private readonly DateTime? _preRegistrationEndDate;
        private readonly DateTime? _addStartDate;
        private readonly DateTime? _addEndDate;
        private readonly DateTime? _dropStartDate;
        private readonly DateTime? _dropEndDate;
        private readonly DateTime? _dropGradeRequiredDate;
        private readonly string _location;
        private readonly List<DateTime?> _censusDates;

        public DateTime? RegistrationStartDate { get { return _registrationStartDate; } }
        public DateTime? RegistrationEndDate { get { return _registrationEndDate; } }
        public DateTime? PreRegistrationStartDate { get { return _preRegistrationStartDate; } }
        public DateTime? PreRegistrationEndDate { get { return _preRegistrationEndDate; } }
        public DateTime? AddStartDate { get { return _addStartDate; } }
        public DateTime? AddEndDate { get { return _addEndDate; } }
        public DateTime? DropStartDate { get { return _dropStartDate; } }
        public DateTime? DropEndDate { get { return _dropEndDate; } }
        public DateTime? DropGradeRequiredDate { get { return _dropGradeRequiredDate; } }
        public string Location { get { return _location; } }
        public List<DateTime?> CensusDates { get { return _censusDates; } }

        public RegistrationDate(string location, DateTime? registrationStartDate, DateTime? registrationEndDate,
            DateTime? preRegistrationStartDate, DateTime? preRegistrationEndDate,
            DateTime? addStartDate, DateTime? addEndDate,
            DateTime? dropStartDate, DateTime? dropEndDate,
            DateTime? dropGradeRequiredDate,
            List<DateTime?> censusDates)
        {
            _location = location;
            _registrationStartDate = registrationStartDate;
            _registrationEndDate = registrationEndDate;
            _preRegistrationStartDate = preRegistrationStartDate;
            _preRegistrationEndDate = preRegistrationEndDate;
            _addStartDate = addStartDate;
            _addEndDate = addEndDate;
            _dropStartDate = dropStartDate;
            _dropEndDate = dropEndDate;
            _dropGradeRequiredDate = dropGradeRequiredDate;
            _censusDates = censusDates;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            RegistrationDate other = obj as RegistrationDate;
            if (other == null)
            {
                return false;
            }
            return other.Location.Equals(_location);
        }
    }
}
