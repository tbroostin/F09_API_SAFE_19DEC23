// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment person demographic dto to the corresponding entity
    /// </summary>

    public class InstantEnrollmentPersonDemographicsDtoToEntityAdapter :
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic,
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>
    {
        public InstantEnrollmentPersonDemographicsDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Converts a InstantEnrollmentPersonDemographic DTO to a InstantEnrollmentPersonDemographic entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic"/> to convert</param>
        /// <returns>the corresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic"/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            List<Domain.Base.Entities.Phone> phoneList = new List<Domain.Base.Entities.Phone>();
            if (source.PersonPhones != null)
            {
                var phoneAdapter = adapterRegistry.GetAdapter<Dtos.Base.Phone,
                    Domain.Base.Entities.Phone>();
                var phones = source.PersonPhones.Where(x => !String.IsNullOrEmpty(x.Number)).Select(x => phoneAdapter.MapToType(x)).ToList();
                if (phones != null)
                {
                    phoneList.AddRange(phones);
                }
            }

            List<string> addrList = new List<string>();
            if (source.AddressLines != null)
            {
                addrList.AddRange(source.AddressLines);
            }

            List<string> raceList = new List<string>();
            if (source.RacialGroups != null)
            {
                raceList.AddRange(source.RacialGroups);
            }

            List<string> ethnicityList = new List<string>();
            if (source.EthnicGroups != null)
            {
                ethnicityList.AddRange(source.EthnicGroups);
            }

            var entity = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic(
                source.FirstName,
                source.LastName
                )
            {
                BirthDate = source.BirthDate,
                CitizenshipCountryCode = source.CitizenshipCountryCode,
                City = source.City,
                CountryCode = source.CountryCode,
                CountyCode = source.CountyCode,
                AddressLines = addrList,
                EmailAddress = source.EmailAddress,
                Gender = source.Gender,
                MiddleName = source.MiddleName,
                PersonPhones = phoneList,
                Prefix = source.Prefix,
                State = source.State,
                Suffix = source.Suffix,
                ZipCode = source.ZipCode,
                GovernmentId = source.GovernmentId
            };

            foreach(var race in raceList)
            {
                entity.AddRacialGroup(race);
            }
            foreach(var eth in ethnicityList)
            {
                entity.AddEthnicGroup(eth);
            }
            return entity;
        }
    }
}
