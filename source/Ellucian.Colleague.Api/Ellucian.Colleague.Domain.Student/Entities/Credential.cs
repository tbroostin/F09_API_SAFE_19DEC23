using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Credential
    {
        private readonly List<string> _honors;
        private readonly List<string> _awards;
        private readonly List<string> _certificates;
        private readonly List<string> _majors;
        private readonly List<string> _minors;
        private readonly List<string> _specializations;

        public List<string> Honors { get { return _honors; } }
        public List<string> Awards { get { return _awards; } }
        public List<string> Certificates { get { return _certificates; } }
        public List<string> Majors { get { return _majors; } }
        public List<string> Minors { get { return _minors; } }
        public List<string> Specializations { get { return _specializations; } }
        public string Degree { get; set; }
        public DateTime? DegreeDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CommencementDate { get; set; }
        public int? NumberOfYears { get; set; }
        public string Comments { get; set; }

        public Credential()
        {
            _honors = new List<string>();
            _awards = new List<string>();
            _certificates = new List<string>();
            _majors = new List<string>();
            _minors = new List<string>();
            _specializations = new List<string>();
        }
        public void AddHonor(string honor)
        {
            if (honor == null)
            {
                throw new ArgumentNullException("honor", "Honor must be specified");
            }
            if (_honors.Where(f => f.Equals(honor)).Count() > 0)
            {
                throw new ArgumentException("Honor already exists in this list");
            }
            _honors.Add(honor);
        }
        public void AddAward(string award)
        {
            if (award == null)
            {
                throw new ArgumentNullException("award", "Award must be specified");
            }
            if (_awards.Where(f => f.Equals(award)).Count() > 0)
            {
                throw new ArgumentException("Award already exists in this list");
            }
            _awards.Add(award);
        }
        public void AddCertificate(string certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate must be specified");
            }
            if (_certificates.Where(f => f.Equals(certificate)).Count() > 0)
            {
                throw new ArgumentException("Certificate already exists in this list");
            }
            _certificates.Add(certificate);
        }
        public void AddMajor(string major)
        {
            if (major == null)
            {
                throw new ArgumentNullException("major", "Major must be specified");
            }
            if (_majors.Where(f => f.Equals(major)).Count() > 0)
            {
                throw new ArgumentException("Major already exists in this list");
            }
            _majors.Add(major);
        }
        public void AddMinor(string minor)
        {
            if (minor == null)
            {
                throw new ArgumentNullException("minor", "Minor must be specified");
            }
            if (_minors.Where(f => f.Equals(minor)).Count() > 0)
            {
                throw new ArgumentException("Minor already exists in this list");
            }
            _minors.Add(minor);
        }
        public void AddSpecialization(string specialization)
        {
            if (specialization == null)
            {
                throw new ArgumentNullException("specialization", "Specialization must be specified");
            }
            if (_specializations.Where(f => f.Equals(specialization)).Count() > 0)
            {
                throw new ArgumentException("Specialization already exists in this list");
            }
            _specializations.Add(specialization);
        }
    }
}
