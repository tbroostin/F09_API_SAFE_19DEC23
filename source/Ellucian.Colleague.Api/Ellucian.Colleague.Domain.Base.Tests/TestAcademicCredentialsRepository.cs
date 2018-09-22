// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAcademicCredentialsRepository
    {

        // Honors really belongs elsewhere but there is no TestReferenceDataRepository

        private readonly string[,] _otherHonors =
        {
            //GUID   CODE   DESCRIPTION
            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CL", "Cum Laude"},
            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "FE", "University Fellow"}
        };

        private readonly string[,] _otherDegrees =
        {
            //GUID   CODE   DESCRIPTION
            {"dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "AA", "Associate of Arts"},
            {"31d8aa32-dbe6-3b89-a1c4-2cad39e232e4", "BA", "Bachelor of Arts"}
        };

        private readonly string[,] _otherCcds =
        {
            //GUID   CODE   DESCRIPTION
            {"72b7737b-27db-4a06-944b-97d00c29b3db", "ELE", "Elementary Education"},
            {"31d8aa32-dbe6-83j7-a1c4-2cad39e232e4", "DI", "Diploma"}
        };

        public IEnumerable<OtherHonor> GetOtherHonors()
        {
            var otherHonorsList = new List<OtherHonor>();

            // There are 3 fields for each honor type in the array
            var items = _otherHonors.Length/3;

            for (var x = 0; x < items; x++)
            {
                otherHonorsList.Add(new OtherHonor(_otherHonors[x, 0], _otherHonors[x, 1], _otherHonors[x, 2]));
            }
            return otherHonorsList;
        }

        public IEnumerable<OtherDegree> GetOtherDegrees()
        {
            var otherDegreeList = new List<OtherDegree>();

            // There are 3 fields for each degree type in the array
            var items = _otherDegrees.Length/3;

            for (var x = 0; x < items; x++)
            {
                otherDegreeList.Add(new OtherDegree(_otherDegrees[x, 0], _otherDegrees[x, 1], _otherDegrees[x, 2]));
            }
            return otherDegreeList;
        }

        public IEnumerable<OtherCcd> GetOtherCcds()
        {
            var otherCcdList = new List<OtherCcd>();

            // There are 3 fields for each CCD type in the array
            var items = _otherCcds.Length/3;

            for (var x = 0; x < items; x++)
            {
                otherCcdList.Add(new OtherCcd(_otherCcds[x, 0], _otherCcds[x, 1], _otherCcds[x, 2]));
            }
            return otherCcdList;
        }

        public IEnumerable<AcadCredential> GetAcadCredentials()
        {
            var acadCredentialCollection = new List<AcadCredential>();

            // There are 3 fields for each CCD type in the array
            var items = _otherCcds.Length/3;

            for (var x = 0; x < items; x++)
            {
                acadCredentialCollection.Add(new AcadCredential(_otherCcds[x, 0], _otherCcds[x, 1], _otherCcds[x, 2], AcademicCredentialType.Certificate));
            }

            items = _otherDegrees.Length/3;

            for (var x = 0; x < items; x++)
            {
                acadCredentialCollection.Add(new AcadCredential(_otherDegrees[x, 0], _otherDegrees[x, 1], _otherDegrees[x, 2], AcademicCredentialType.Degree));
            }

            return acadCredentialCollection;
        }
    }
}