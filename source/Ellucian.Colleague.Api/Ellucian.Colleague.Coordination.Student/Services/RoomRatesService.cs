//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class RoomRatesService : StudentCoordinationService, IRoomRatesService
    {

        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public RoomRatesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IStudentRepository studentRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _studentReferenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all room-rates
        /// </summary>
        /// <returns>Collection of RoomRates DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RoomRates>> GetRoomRatesAsync(bool bypassCache = false)
        {
            var roomRatesCollection = new List<Ellucian.Colleague.Dtos.RoomRates>();

            var roomRatesEntities = await _studentReferenceDataRepository.GetRoomRatesAsync(bypassCache);
            if (roomRatesEntities != null && roomRatesEntities.Any())
            {
                foreach (var roomRates in roomRatesEntities)
                {
                    roomRatesCollection.Add(ConvertRoomRatesEntityToDto(roomRates));
                }
            }
            return roomRatesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a RoomRates from its GUID
        /// </summary>
        /// <returns>RoomRates DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RoomRates> GetRoomRatesByGuidAsync(string guid)
        {
            try
            {
                return ConvertRoomRatesEntityToDto((await _studentReferenceDataRepository.GetRoomRatesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("room-rates not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("room-rates not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RoomRates domain entity to its corresponding RoomRates DTO
        /// </summary>
        /// <param name="source">RoomRates domain entity</param>
        /// <returns>RoomRates DTO</returns>
        private Ellucian.Colleague.Dtos.RoomRates ConvertRoomRatesEntityToDto(RoomRate source)
        {
            var roomRates = new Ellucian.Colleague.Dtos.RoomRates();

            roomRates.Id = source.Guid;
            roomRates.Code = source.Code;
            roomRates.Title = source.Description;
            roomRates.Description = null;
            roomRates.StartOn = source.StartDate;
            roomRates.EndOn = source.EndDate;
            roomRates.AccountingCode = !string.IsNullOrEmpty(source.AccountingCode) ? new GuidObject2(source.AccountingCode) : null;
            roomRates.CancelAccountingCode = !string.IsNullOrEmpty(source.CancelAccountingCode) ? new GuidObject2(source.CancelAccountingCode) : null;
            roomRates.Rate = BuildRoomRatesRate(source);
                                                                                                 
            //roomRates.Period= ConvertRoomRatesPeriodDomainEnumToRoomRatesPeriodDtoEnum(source.RoomRatesPeriod);
                                                            
            return roomRates;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.RoomRatesRate> BuildRoomRatesRate(Ellucian.Colleague.Domain.Student.Entities.RoomRate source)
        {
            List<Ellucian.Colleague.Dtos.RoomRatesRate> ratesRates = new List<RoomRatesRate>();
            if (source.DayRate != null)
            {
                //foreach (var daily in source.DayRate)
                //{
                //    RoomRatesRate rate = new RoomRatesRate();
                //    rate.RateProperty = new RoomRatesRateDtoProperty();
                //    rate.RateProperty.Value = daily;
                //    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                //    rate.Period = RoomRatesPeriod.Day;

                //    ratesRates.Add(rate);
                //}

                RoomRatesRate rate = new RoomRatesRate();
                rate.RateProperty = new RoomRatesRateDtoProperty();
                rate.RateProperty.Value = source.DayRate;
                if (rate.RateProperty.Value != null)
                {
                    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                    rate.Period = RoomRatesPeriod.Day;
                    ratesRates.Add(rate);
                }

                //ratesRates.Add(rate);
            }
            if (source.WeeklyRate != null)
            {
                //foreach (var weekly in source.WeeklyRate)
                //{
                //    RoomRatesRate rate = new RoomRatesRate();
                //    rate.RateProperty = new RoomRatesRateDtoProperty();
                //    rate.RateProperty.Value = weekly;
                //    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                //    rate.Period = RoomRatesPeriod.Week;

                //    ratesRates.Add(rate);
                //}

                RoomRatesRate rate = new RoomRatesRate();
                rate.RateProperty = new RoomRatesRateDtoProperty();
                rate.RateProperty.Value = source.WeeklyRate;
                if (rate.RateProperty.Value != null)
                {
                    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                    rate.Period = RoomRatesPeriod.Week;
                    ratesRates.Add(rate);
                }

                //ratesRates.Add(rate);
            }
            if (source.MonthlyRate != null)
            {
                //foreach (var monthly in source.MonthlyRate)
                //{
                //    RoomRatesRate rate = new RoomRatesRate();
                //    rate.RateProperty = new RoomRatesRateDtoProperty();
                //    rate.RateProperty.Value = monthly;
                //    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                //    rate.Period = RoomRatesPeriod.Month;

                //    ratesRates.Add(rate);
                //}

                RoomRatesRate rate = new RoomRatesRate();
                rate.RateProperty = new RoomRatesRateDtoProperty();
                rate.RateProperty.Value = source.MonthlyRate;
                if (rate.RateProperty.Value != null)
                {
                    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                    rate.Period = RoomRatesPeriod.Month;
                    ratesRates.Add(rate);
                }

                //ratesRates.Add(rate);
            }
            if (source.TermRate != null)
            {
                //foreach (var term in source.TermRate)
                //{
                //    RoomRatesRate rate = new RoomRatesRate();
                //    rate.RateProperty = new RoomRatesRateDtoProperty();
                //    rate.RateProperty.Value = term;
                //    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                //    rate.Period = RoomRatesPeriod.Term;

                //    ratesRates.Add(rate);
                //}

                RoomRatesRate rate = new RoomRatesRate();
                rate.RateProperty = new RoomRatesRateDtoProperty();
                rate.RateProperty.Value = source.TermRate;
                if (rate.RateProperty.Value != null)
                {
                    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                    rate.Period = RoomRatesPeriod.Term;
                    ratesRates.Add(rate);
                }

                //ratesRates.Add(rate);
            }
            if (source.AnnualRate != null)
            {
                //foreach (var annually in source.AnnualRate)
                //{
                //    RoomRatesRate rate = new RoomRatesRate();
                //    rate.RateProperty = new RoomRatesRateDtoProperty();
                //    rate.RateProperty.Value = annually;
                //    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                //    rate.Period = RoomRatesPeriod.Year;

                //    ratesRates.Add(rate);
                //}

                RoomRatesRate rate = new RoomRatesRate();
                rate.RateProperty = new RoomRatesRateDtoProperty();
                rate.RateProperty.Value = source.AnnualRate;
                if (rate.RateProperty.Value != null)
                {
                    rate.RateProperty.Currency = GetCurrencyIsoCode("", GetHostCountry().ToString());
                    rate.Period = RoomRatesPeriod.Year;
                    ratesRates.Add(rate);
                }

                //ratesRates.Add(rate);
            }

            return ratesRates;
        }

        private string _hostCountry = null;
        private async Task<string> GetHostCountry()
        {
            if (_hostCountry == null)
            {
                _hostCountry = await _studentReferenceDataRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }

        /// <summary>
        ///  Get Currency ISO Code
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="hostCountry"></param>
        /// <returns><see cref="CurrencyIsoCode"></returns>
        private static RoomRatesCurrency GetCurrencyIsoCode(string currencyCode, string hostCountry = "USA")
        {
            var currency = RoomRatesCurrency.USD;

            try
            {
                if (!(string.IsNullOrEmpty(currencyCode)))
                {
                    currency = (Dtos.EnumProperties.RoomRatesCurrency)Enum.Parse(typeof(Dtos.EnumProperties.RoomRatesCurrency), currencyCode);
                }
                else
                {
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? RoomRatesCurrency.CAD :
                        RoomRatesCurrency.USD;
                }
            }
            catch (Exception)
            {
                currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? RoomRatesCurrency.CAD :
                    RoomRatesCurrency.USD;
            }

            return currency;
        }

        ///// <summary>
        /////  Get Currency ISO Code
        ///// </summary>
        ///// <param name="currencyCode"></param>
        ///// <param name="hostCountry"></param>
        ///// <returns><see cref="CurrencyIsoCode"></returns>
        //private static CurrencyIsoCode GetCurrencyIsoCode(string currencyCode, string hostCountry = "USA")
        //{
        //    var currency = CurrencyIsoCode.USD;

        //    try
        //    {
        //        if (!(string.IsNullOrEmpty(currencyCode)))
        //        {
        //            currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), currencyCode);
        //        }
        //        else
        //        {
        //            currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
        //                CurrencyIsoCode.USD;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
        //            CurrencyIsoCode.USD;
        //    }

        //    return currency;
        //}

   }
}