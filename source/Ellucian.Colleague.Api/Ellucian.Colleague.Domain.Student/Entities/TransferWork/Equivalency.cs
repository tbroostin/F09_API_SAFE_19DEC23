// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{
    /// <summary>
    /// Student Credit Equivalency
    /// </summary>
    [Serializable]
    public class Equivalency
    {
        private List<ExternalCourseWork> _externalCourseWork;
        private List<ExternalNonCourseWork> _externalNonCourseWork;
        private List<EquivalentCoursCredit> _equivalentCourseCredits;
        private List<EquivalentGeneralCredit> _equivalentGeneralCredits;
        private List<string> _academicPrograms;

        /// <summary>
        /// List of external course work
        /// </summary>
        public ReadOnlyCollection<ExternalCourseWork> ExternalCourseWork { get; private set; }

        /// <summary>
        /// List of external non course work.
        /// </summary>
        public ReadOnlyCollection<ExternalNonCourseWork> ExternalNonCourseWork { get; private set; }

        /// <summary>
        /// List of institutional course equivalency credits
        /// </summary>
        public ReadOnlyCollection<EquivalentCoursCredit> EquivalentCourseCredits { get; private set; }

        /// <summary>
        /// List of institutional general equivalency credits
        /// </summary>
        public ReadOnlyCollection<EquivalentGeneralCredit> EquivalentGeneralCredits { get; private set; }

        /// <summary>
        /// List of Academic Programs this equivalency is restricted to.
        /// </summary>
        public ReadOnlyCollection<string> AcademicPrograms { get; private set; }

        public Equivalency()
        {
            _externalCourseWork = new List<ExternalCourseWork>();
            _externalNonCourseWork = new List<ExternalNonCourseWork>();
            _equivalentCourseCredits = new List<EquivalentCoursCredit>();            
            _equivalentGeneralCredits = new List<EquivalentGeneralCredit>();
            _academicPrograms = new List<string>();

            ExternalCourseWork = _externalCourseWork.AsReadOnly();
            ExternalNonCourseWork = _externalNonCourseWork.AsReadOnly();
            EquivalentCourseCredits = _equivalentCourseCredits.AsReadOnly();
            EquivalentGeneralCredits = _equivalentGeneralCredits.AsReadOnly();
            AcademicPrograms = _academicPrograms.AsReadOnly();
        }   

        public void AddExternalCourseWork(ExternalCourseWork externalTransferWork)
        {
            if (externalTransferWork == null)
            {
                throw new ArgumentNullException("externalTransferWork is required.");
            }
            _externalCourseWork.Add(externalTransferWork);
        }

        public void AddExternalNonCourseWork(ExternalNonCourseWork externalNonCourseWork)
        {
            if (externalNonCourseWork == null)
            {
                throw new ArgumentNullException("externalNonCourseWork is required.");
            }
            _externalNonCourseWork.Add(externalNonCourseWork);
        }

        public void AddEquivalentCourseCredit(EquivalentCoursCredit equivalentCourseCredit)
        {
            if (equivalentCourseCredit == null)
            {
                throw new ArgumentNullException("equivalentWork is required.");
            }
            _equivalentCourseCredits.Add(equivalentCourseCredit);
        }

        public void AddEquivalentGeneralCredit(EquivalentGeneralCredit equivalentGeneralCredit)
        {
            if (equivalentGeneralCredit == null)
            {
                throw new ArgumentNullException("equivalentWork is required.");
            }
            _equivalentGeneralCredits.Add(equivalentGeneralCredit);
        }

        public void AddAcademicProgram(string acadameicProgram)
        {
            if (string.IsNullOrEmpty(acadameicProgram))
            {
                throw new ArgumentNullException("academicProgram is required.");
            }
            _academicPrograms.Add(acadameicProgram);
        }
    }

}
