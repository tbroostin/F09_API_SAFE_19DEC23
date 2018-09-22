// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PilotAcademicHistoryLevel
    {
        private string _AcademicLevelCode;
        public string AcademicLevelCode { get { return _AcademicLevelCode; } }
        private PilotAcademicHistory _AcademicLevelHistory;
        public PilotAcademicHistory AcademicLevelHistory { get { return _AcademicLevelHistory; } }
        private string _StudentId;
        public string StudentId { get { return _StudentId; } }

        public PilotAcademicHistoryLevel(string academicLevelCode, AcademicHistory academicHistory, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (academicLevelCode == null)
            {
                throw new ArgumentNullException("academicLevelCode");
            }
            _AcademicLevelCode = academicLevelCode;
            _AcademicLevelHistory = PilotAcademicHistory.FromAcademicHistory(academicHistory);
            _StudentId = studentId;
        }

        public PilotAcademicHistoryLevel(string academicLevelCode, PilotAcademicHistory academicHistory, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (academicLevelCode == null)
            {
                throw new ArgumentNullException("academicLevelCode");
            }
            _AcademicLevelCode = academicLevelCode;
            _AcademicLevelHistory = academicHistory;
            _StudentId = studentId;
        }

        public PilotAcademicHistoryLevel(AcademicHistoryLevel historyLevel)
        {
            if (historyLevel == null)
            {
                throw new ArgumentNullException("historyLevel");
            }
            _AcademicLevelCode = historyLevel.AcademicLevelCode;
            _AcademicLevelHistory = PilotAcademicHistory.FromAcademicHistory(historyLevel.AcademicLevelHistory);
            _StudentId = historyLevel.StudentId;
        }
    }
}
