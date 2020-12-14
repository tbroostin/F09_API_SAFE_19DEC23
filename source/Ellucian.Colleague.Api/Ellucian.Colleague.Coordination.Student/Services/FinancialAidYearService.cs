//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class FinancialAidYearService : BaseCoordinationService, IFinancialAidYearService
    {
        private IStudentReferenceDataRepository financialAidReferenceDataRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidYearService(IAdapterRegistry adapterRegistry,
            IStudentReferenceDataRepository financialAidReferenceDataRepository, 
            ITermRepository termRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _termRepository = termRepository;
            this.configurationRepository = configurationRepository;
        }

        //Get collection of academic terms for the named query.
        private IEnumerable<Domain.Student.Entities.Term> _academicPeriods = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> AcademicPeriodsAsync( bool bypassCache )
        {
            if( _academicPeriods == null )
            {
                _academicPeriods = await _termRepository.GetAsync( bypassCache );
            }
            return _academicPeriods;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all financial aid years
        /// </summary>
        /// <returns>Collection of FinancialAidYear DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidYear>> GetFinancialAidYearsAsync(string academicPeriodId = "", bool bypassCache = false)
        {
            var financialAidYearCollection = new List<Ellucian.Colleague.Dtos.FinancialAidYear>();
            IEnumerable<Domain.Student.Entities.FinancialAidYear> financialAidYearEntities = null;

            if( !string.IsNullOrEmpty( academicPeriodId ) )
            {
                if(string.IsNullOrWhiteSpace(academicPeriodId))
                {
                    return financialAidYearCollection;
                }
                //Get terms. If there are no terms defined then return an empty set.
                var terms = await AcademicPeriodsAsync( bypassCache );
                if( terms == null || !terms.Any() )
                {
                    return financialAidYearCollection;
                }

                //Get term based on the guid. If no term is found then return an empty set.
                var term = terms.FirstOrDefault( t => t.RecordGuid.Equals( academicPeriodId, StringComparison.InvariantCultureIgnoreCase ) );
                if( term == null )
                {
                    return financialAidYearCollection;
                }

                //Get FA Years. If none defined then return an empty set.
                var faYrs = term.FinancialAidYears.Where( fay => fay.HasValue );
                if( faYrs == null || !faYrs.Any() )
                {
                    return financialAidYearCollection;
                }
                //Convert years to a string array for filter.
                string[] yrs = faYrs.Select( i => i.ToString() ).ToArray();
                //Otherwise continue & get financial aid years which matched with fa years defined in a term.
                var tempFaYrs = await financialAidReferenceDataRepository.GetFinancialAidYearsAsync( bypassCache );
                if( tempFaYrs != null && tempFaYrs.Any() )
                {
                    financialAidYearEntities = tempFaYrs.Where( t => !string.IsNullOrWhiteSpace( t.Code ) && yrs.Contains( t.Code ) ).ToList();
                    if( financialAidYearEntities == null || !financialAidYearEntities.Any() )
                    {
                        return financialAidYearCollection;
                    }

                    //Convert & return DTO's.
                    foreach( var financialAidYear in financialAidYearEntities )
                    {
                        financialAidYearCollection.Add( ConvertFinancialAidYearEntityToDto( financialAidYear ) );
                    }
                }
            }
            else
            {
                financialAidYearEntities = await financialAidReferenceDataRepository.GetFinancialAidYearsAsync( bypassCache );
                if( financialAidYearEntities != null && financialAidYearEntities.Any() )
                {
                    foreach( var financialAidYear in financialAidYearEntities )
                    {
                        financialAidYearCollection.Add( ConvertFinancialAidYearEntityToDto( financialAidYear ) );
                    }
                }
            }
            if( IntegrationApiException != null )
            {
                throw IntegrationApiException;
            }

            return financialAidYearCollection;

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an financial aid Year from its GUID
        /// </summary>
        /// <returns>FinancialAidYear DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidYear> GetFinancialAidYearByGuidAsync(string guid)
        {
            try
            {
                var finYear = await financialAidReferenceDataRepository.GetFinancialAidYearAsync( guid );
                if(finYear == null)
                {
                    IntegrationApiExceptionAddError( string.Format( "Financial aid year not found for GUID '{0}'", guid ), "GUID.Not.Found", guid, string.Empty, System.Net.HttpStatusCode.NotFound );
                    throw IntegrationApiException;
                }
                //GetFinancialAidYearAsync
                var dto = ConvertFinancialAidYearEntityToDto( finYear );
                if( IntegrationApiException != null )
                {
                    throw IntegrationApiException;
                }
                return dto;
            }
            catch( IntegrationApiException e)
            {
                throw;
            }
            catch(RepositoryException e)
            {
                IntegrationApiExceptionAddError( string.Format( "Financial aid year not found for GUID '{0}'", guid ), "GUID.Not.Found", guid, string.Empty, System.Net.HttpStatusCode.NotFound );
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Financial aid year not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Financial Aid Year domain entity to its corresponding FinancialAidYear DTO
        /// </summary>
        /// <param name="source">FinancialAidYear domain entity</param>
        /// <returns>FinancialAidYear DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialAidYear ConvertFinancialAidYearEntityToDto(Domain.Student.Entities.FinancialAidYear source)
        {
            var financialAidYear = new Ellucian.Colleague.Dtos.FinancialAidYear();

            financialAidYear.Id = source.Guid;
            financialAidYear.Code = source.Code;
            financialAidYear.Title = source.Description;
            if (!string.IsNullOrEmpty(source.Code))
            {
                try
                {
                    var hostCountry = source.HostCountry;
                    switch (hostCountry.ToString())
                    {
                        case "USA":
                            financialAidYear.Start = new DateTime(Int32.Parse(source.Code), 07, 01);
                            financialAidYear.End = new DateTime(Int32.Parse(source.Code) + 1, 06, 30);
                            break;
                        default:
                            financialAidYear.Start = null;
                            financialAidYear.End = null;
                            break;
                    }
                }
                catch (Exception e)
                {
                    IntegrationApiExceptionAddError(string.Format("Code not defined for financial aid year for guid '{0}' with title '{1}'", source.Guid, source.Description ), "GUID.Not.Found", source.Guid, source.Code);
                }
            }
            financialAidYear.Description = null;
            switch (source.status)
            {
                case "D":
                    financialAidYear.Status = FinancialAidYearStatus.Inactive;
                    break;
                default:
                    financialAidYear.Status = FinancialAidYearStatus.Active;
                    break;
            }

            return financialAidYear;
        }
    }
}