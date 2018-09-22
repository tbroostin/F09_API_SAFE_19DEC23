// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAcademicDisciplineRepository
    {
        private readonly string[,] _otherMajors = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "ENGL", "English"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "MATH", "Mathematics"},
                                            {"99d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "ROCK", "Geology"}
                                      };
        private readonly string[,] _otherMinors = {
                                            //GUID   CODE   DESCRIPTION
                                            {"dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "HIST", "History"}, 
                                            {"31d8aa32-dbe6-3b89-a1c4-2cad39e232e4", "ACCT", "Accounting"}
                                      };
        private readonly string[,] _otherSpecials = {
                                            //GUID   CODE   DESCRIPTION
                                            {"72b7737b-27db-4a06-944b-97d00c29b3db", "CERT", "Certification"}, 
                                            {"31d8aa32-dbe6-83j7-a1c4-2cad39e232e4", "SCIE", "Sciences"}
                                      };

        public IEnumerable<OtherMajor> GetOtherMajors()
        {
            var otherMajorList = new List<OtherMajor>();

            // There are 3 fields for each email type in the array
            var items = _otherMajors.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherMajorList.Add(new OtherMajor(_otherMajors[x, 0], _otherMajors[x, 1], _otherMajors[x, 2]));
            }
            return otherMajorList;
        }

        public IEnumerable<OtherMinor> GetOtherMinors()
        {
            var otherMinorList = new List<OtherMinor>();

            // There are 3 fields for each email type in the array
            var items = _otherMinors.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherMinorList.Add(new OtherMinor(_otherMinors[x, 0], _otherMinors[x, 1], _otherMinors[x, 2]));
            }
            return otherMinorList;
        }

        public IEnumerable<OtherSpecial> GetOtherSpecials()
        {
            var otherSpecialList = new List<OtherSpecial>();

            // There are 3 fields for each email type in the array
            var items = _otherSpecials.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherSpecialList.Add(new OtherSpecial(_otherSpecials[x, 0], _otherSpecials[x, 1], _otherSpecials[x, 2]));
            }
            return otherSpecialList;
        }

        public IEnumerable<AcademicDiscipline> GetAcademicDisciplines()
        {
            var otherAcademicDisciplines = new List<AcademicDiscipline>();

            // There are 3 fields for each email type in the array
            var items = _otherSpecials.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherAcademicDisciplines.Add(new AcademicDiscipline(_otherSpecials[x, 0], _otherSpecials[x, 1], _otherSpecials[x, 2], AcademicDisciplineType.Concentration));
            }

            items = _otherMajors.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherAcademicDisciplines.Add(new AcademicDiscipline(_otherMajors[x, 0], _otherMajors[x, 1], _otherMajors[x, 2], AcademicDisciplineType.Major) { ActiveMajor = true });
            }

            items = _otherMinors.Length / 3;

            for (var x = 0; x < items; x++)
            {
                otherAcademicDisciplines.Add(new AcademicDiscipline(_otherMinors[x, 0], _otherMinors[x, 1], _otherMinors[x, 2], AcademicDisciplineType.Minor));
            }

            return otherAcademicDisciplines;
        }
    }
}