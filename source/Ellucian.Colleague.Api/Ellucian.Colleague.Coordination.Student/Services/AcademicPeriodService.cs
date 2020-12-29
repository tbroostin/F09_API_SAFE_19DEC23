// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AcademicPeriodService : BaseCoordinationService, IAcademicPeriodService
    {
        private readonly ITermRepository _termRepository;
        private ILogger _logger;

        public AcademicPeriodService(
            ITermRepository termRepository,
            IAdapterRegistry adapterRegistry, 
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _termRepository = termRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Academic Periods
        /// </summary>
        /// <returns>Collection of AcademicPeriod DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod2>> GetAcademicPeriods2Async(bool bypassCache)
        {
            var academicPeriodCollection = new List<Ellucian.Colleague.Dtos.AcademicPeriod2>();

            var termEntities = await _termRepository.GetAsync(bypassCache);

            var academicPeriodEntities = _termRepository.GetAcademicPeriods(termEntities);

            if (academicPeriodEntities != null && academicPeriodEntities.Any())
            {
                foreach (var academicPeriod in academicPeriodEntities)
                {
                    academicPeriodCollection.Add(ConvertAcademicPeriodEntityToAcademicPeriodDto2(academicPeriod));
                }
            }

            return academicPeriodCollection;
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Academic Periods
        /// including census dates and registration status
        /// </summary>
        /// <param name="status">Academic Period registration matches open or closed</param>
        /// <returns>Collection of AcademicPeriod DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod3>> GetAcademicPeriods3Async(bool bypassCache, string registration = "")
        {
           
            var academicPeriodCollection = new List<Ellucian.Colleague.Dtos.AcademicPeriod3>();

            var termEntities = await _termRepository.GetAsync(bypassCache);

            var academicPeriodEntities = _termRepository.GetAcademicPeriods(termEntities);

            if (academicPeriodEntities != null && academicPeriodEntities.Any())
            {
                foreach (var academicPeriod in academicPeriodEntities)
                {
                    var termRegStatus = GetTermRegistrationStatus(academicPeriod);
                    if ((string.IsNullOrEmpty(registration)) || (termRegStatus.ToString().ToLower() == registration.ToLower()))
                    {
                        academicPeriodCollection.Add(ConvertAcademicPeriodEntityToAcademicPeriodDto3(academicPeriod, termRegStatus));
                    }
                }
            }

            return academicPeriodCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM v16.0.0</remarks>
        /// <summary>
        /// Gets all Academic Periods
        /// including census dates and registration status
        /// </summary>
        /// <param name="registration">Academic Period registration matches open or closed</param>
        /// <param name="termCode">Specific term filter</param>
        /// <param name="category">Specific category (term, subterm, year)</param>
        /// <returns>Collection of AcademicPeriod DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod4>> GetAcademicPeriods4Async(bool bypassCache, string registration = "", string termCode = "", 
            string category = "", DateTimeOffset? startOn = null, DateTimeOffset? endOn = null, Dictionary<string, string> filterQualifiers = null)
        {

            var academicPeriodCollection = new List<Ellucian.Colleague.Dtos.AcademicPeriod4>();

            var termEntities = await _termRepository.GetAsync(bypassCache);

            if (termEntities != null && termEntities.Any())
            {
                var academicPeriodEntities = _termRepository.GetAcademicPeriods(termEntities);

                // Filter on term code
                if (!string.IsNullOrEmpty(termCode))
                {
                    var restrictedTerms = academicPeriodEntities.Where(te => te.Code.Equals(termCode, StringComparison.OrdinalIgnoreCase)).ToList();
                    academicPeriodEntities = restrictedTerms;
                }
                // Filter on category
                if (!string.IsNullOrEmpty(category))
                {
                    if (category.Equals("term", StringComparison.OrdinalIgnoreCase))
                    {
                        var restrictedTerms = academicPeriodEntities.Where(te => te.Code.Equals(te.ReportingTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        academicPeriodEntities = restrictedTerms;
                    }
                    if (category.Equals("subterm", StringComparison.OrdinalIgnoreCase))
                    {
                        var restrictedTerms = academicPeriodEntities.Where(te => te.Code.ToUpper() != te.ReportingTerm.ToUpper()).ToList();
                        academicPeriodEntities = restrictedTerms;
                    }
                    if (category.ToLower() != "term" && category.ToLower() != "subterm")
                    {
                        return academicPeriodCollection;
                    }
                }
                if (academicPeriodEntities != null && academicPeriodEntities.Any() && ((startOn.HasValue) || (endOn.HasValue)))
                {
                    Expression<Func<AcademicPeriod, bool>> final = null;
                    var parameter = Expression.Parameter(typeof(AcademicPeriod), "s");

                    Expression startOnExpression = null;
                    if (startOn.HasValue)
                    {
                        string startOperator = "";
                        if (filterQualifiers != null && filterQualifiers.ContainsKey("StartOn"))
                        {
                            filterQualifiers.TryGetValue("StartOn", out startOperator);
                        }
                                                                     
                        var propertyExpression = Expression.PropertyOrField(parameter, "StartDate");
                        var constant = Expression.Constant(startOn.Value.Date, typeof(DateTime));

                        if (string.IsNullOrEmpty(startOperator))
                        {
                            startOnExpression = Expression.Equal(propertyExpression, constant);
                        }
                        else
                        {
                            switch (startOperator)
                            {
                                case ("GE"):
                                    startOnExpression = Expression.GreaterThanOrEqual(propertyExpression, constant); break;
                                case ("GT"):
                                    startOnExpression = Expression.GreaterThan(propertyExpression, constant); break;
                                case ("LE"):
                                    startOnExpression = Expression.LessThanOrEqual(propertyExpression, constant); break;
                                case ("LT"):
                                    startOnExpression = Expression.LessThan(propertyExpression, constant); break;
                                case ("NE"):
                                    startOnExpression = Expression.NotEqual(propertyExpression, constant); break;
                                case ("EQ"):
                                    startOnExpression = Expression.Equal(propertyExpression, constant); break;
                                default:
                                    startOnExpression = Expression.Equal(propertyExpression, constant); break;
                            }
                        }
                    }

                    Expression endOnExpression = null;
                    if (endOn.HasValue)
                    {
                        string endOperator = "";
                        if (filterQualifiers != null && filterQualifiers.ContainsKey("EndOn"))
                        {
                            filterQualifiers.TryGetValue("EndOn", out endOperator);
                        }

                        var propertyExpression = Expression.PropertyOrField(parameter, "EndDate");
                        var constant = Expression.Constant(endOn.Value.Date, typeof(DateTime));

                        if (string.IsNullOrEmpty(endOperator))
                        {
                            endOnExpression = Expression.Equal(propertyExpression, constant);
                        }
                        else
                        {
                            switch (endOperator)
                            {
                                case ("GE"):
                                    endOnExpression = Expression.GreaterThanOrEqual(propertyExpression, constant); break;
                                case ("GT"):
                                    endOnExpression = Expression.GreaterThan(propertyExpression, constant); break;
                                case ("LE"):
                                    endOnExpression = Expression.LessThanOrEqual(propertyExpression, constant); break;
                                case ("LT"):
                                    endOnExpression = Expression.LessThan(propertyExpression, constant); break;
                                case ("NE"):
                                    endOnExpression = Expression.NotEqual(propertyExpression, constant); break;
                                case ("EQ"):
                                    endOnExpression = Expression.Equal(propertyExpression, constant); break;
                                default:
                                    endOnExpression = Expression.Equal(propertyExpression, constant); break;
                            }
                        }
                    }

                    if (startOnExpression != null && endOnExpression == null)
                    {
                        final = Expression.Lambda<Func<AcademicPeriod, bool>>(startOnExpression, parameter);
                    }
                    else if (startOnExpression == null && endOnExpression != null)
                    {
                        final = Expression.Lambda<Func<AcademicPeriod, bool>>(endOnExpression, parameter);
                    }
                    else if (startOnExpression != null && endOnExpression != null)
                    {
                        var combinedExpression = Expression.And(startOnExpression, endOnExpression);
                        final = Expression.Lambda<Func<AcademicPeriod, bool>>(combinedExpression, parameter);
                    }

                    if (final != null)
                    {
                        academicPeriodEntities = academicPeriodEntities.AsQueryable().Where(final);
                    }
                }
            

                if (academicPeriodEntities != null && academicPeriodEntities.Any())
                {
                    foreach (var academicPeriod in academicPeriodEntities)
                    {
                        var termRegStatus = GetTermRegistrationStatus(academicPeriod);
                        if ((string.IsNullOrEmpty(registration)) || (termRegStatus.ToString().ToLower() == registration.ToLower()))
                        {
                            academicPeriodCollection.Add(ConvertAcademicPeriodEntityToAcademicPeriodDto4(academicPeriod, termRegStatus));
                        }
                    }
                }
            }

            return academicPeriodCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get an Academic Period from its GUID
        /// </summary>
        /// <returns>AcademicPeriod DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriod2> GetAcademicPeriodByGuid2Async(string guid)
        {
            try
            {
                var termEntities = await _termRepository.GetAsync(true);
                var academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
                return ConvertAcademicPeriodEntityToAcademicPeriodDto2(academicPeriods.Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get an Academic Period from its GUID
        /// including census dates and registration status
        /// </summary>
        /// <returns>AcademicPeriod DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriod3> GetAcademicPeriodByGuid3Async(string guid, bool bypassCache = false)
        {
            try
            {
                var termEntities = await _termRepository.GetAsync(bypassCache);
                var academicPeriods = _termRepository.GetAcademicPeriods(termEntities);

                var academicPeriod = academicPeriods.Where(rt => rt.Guid == guid).FirstOrDefault();

                return ConvertAcademicPeriodEntityToAcademicPeriodDto3(academicPeriod, GetTermRegistrationStatus(academicPeriod));
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM v16.0.0</remarks>
        /// <summary>
        /// Get an Academic Period from its GUID
        /// including census dates and registration status
        /// </summary>
        /// <returns>AcademicPeriod DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriod4> GetAcademicPeriodByGuid4Async(string guid, bool bypassCache = false)
        {
            try
            {
                var termEntities = await _termRepository.GetAsync(bypassCache);
                var academicPeriods = _termRepository.GetAcademicPeriods(termEntities);

                var academicPeriod = academicPeriods.Where(rt => rt.Guid == guid).FirstOrDefault();

                return ConvertAcademicPeriodEntityToAcademicPeriodDto4(academicPeriod, GetTermRegistrationStatus(academicPeriod));
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException("Academic Period not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Determine if a term is "open" or "closed" based on its start and end date for pre-registration, registration, add, and drop.
        /// Open if today is greater than/equal to earliest start date and less than/equal to latest start date.
        /// Closed if today is less than earliest start date or greater than latest end date.
        /// </summary>
        /// <returns>Academic registration status</returns>
        private Dtos.EnumProperties.TermRegistrationStatus? GetTermRegistrationStatus(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod academicPeriod)
        {
            Dtos.EnumProperties.TermRegistrationStatus termRegistrationStatus = new Dtos.EnumProperties.TermRegistrationStatus();
            if (academicPeriod.RegistrationDates != null)
            {
                var preRegistrationStartDate = academicPeriod.RegistrationDates.FirstOrDefault().PreRegistrationStartDate;
                var preRegistrationEndDate = academicPeriod.RegistrationDates.FirstOrDefault().PreRegistrationEndDate;
                var registrationStartDate = academicPeriod.RegistrationDates.FirstOrDefault().RegistrationStartDate;
                var registrationEndDate = academicPeriod.RegistrationDates.FirstOrDefault().RegistrationEndDate;
                var addStartDate = academicPeriod.RegistrationDates.FirstOrDefault().AddStartDate;
                var addEndDate = academicPeriod.RegistrationDates.FirstOrDefault().AddEndDate;
                var dropStartDate = academicPeriod.RegistrationDates.FirstOrDefault().DropStartDate;
                var dropEndDate = academicPeriod.RegistrationDates.FirstOrDefault().DropEndDate;

                /// Buid list of all term pre-registration, registration ,add, and drop dates
                List<DateTime?> termDates = new List<DateTime?>();
                if (preRegistrationStartDate != null)
                {
                    termDates.Add(preRegistrationStartDate);
                }
                if (registrationStartDate != null)
                {
                    termDates.Add(registrationStartDate);
                }
                if (addStartDate != null)
                {
                    termDates.Add(addStartDate);
                }
                if (dropStartDate != null)
                {
                    termDates.Add(dropStartDate);
                }
                if (preRegistrationEndDate != null)
                {
                    termDates.Add(preRegistrationEndDate);
                }
                if (registrationEndDate != null)
                {
                    termDates.Add(registrationEndDate);
                }
                if (addEndDate != null)
                {
                    termDates.Add(addEndDate);
                }
                if (dropEndDate != null)
                {
                    termDates.Add(dropEndDate);
                }
                // Find earliest and latest dates available.
                DateTime? minTermDate = null;
                DateTime? maxTermDate = null;
                foreach (DateTime date in termDates)
                {
                    if (minTermDate == null)
                    {
                        minTermDate = date;
                    }
                    if (maxTermDate == null)
                    {
                        maxTermDate = date;
                    }
                    if (date < minTermDate)
                        minTermDate = date;
                    if (date > maxTermDate)
                        maxTermDate = date;
                }

                // Determine if registration is open or closed.       
                // Open if today is greater than/equal to earliest start date and less than/equal to latest start date.
                // Closed if today is less than earliest start date or greater than latest end date.
                //Dtos.EnumProperties.TermRegistrationStatus termRegistrationStatus = new Dtos.EnumProperties.TermRegistrationStatus();
                if (DateTime.Now >= minTermDate && DateTime.Now <= maxTermDate)
                {
                    termRegistrationStatus = Dtos.EnumProperties.TermRegistrationStatus.Open;
                }
                if (DateTime.Now < minTermDate || DateTime.Now > maxTermDate)
                {
                    termRegistrationStatus = Dtos.EnumProperties.TermRegistrationStatus.Closed;
                }
            }
            return termRegistrationStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Academic Period domain entity to its corresponding Academic Periods DTO
        /// </summary>
        /// <param name="source">Academic Periods domain entity</param>
        /// <returns>EmailType DTO</returns>
        private Dtos.AcademicPeriod2 ConvertAcademicPeriodEntityToAcademicPeriodDto2(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod source)
        {
            var acadedmicPeriod = new Dtos.AcademicPeriod2();
            acadedmicPeriod.Id = source.Guid;
            acadedmicPeriod.Code = source.Code;
            acadedmicPeriod.Title = source.Description;
            acadedmicPeriod.Description = null;

            acadedmicPeriod.Start = source.StartDate;
            acadedmicPeriod.End = source.EndDate;

            var category = new Dtos.AcademicPeriodCategory2();
            category.Type = IsReportingTermEqualCode(source) ? Dtos.AcademicTimePeriod2.Term : Dtos.AcademicTimePeriod2.Subterm;
            if (!string.IsNullOrEmpty(source.ParentId))
            {
                category.ParentGuid = new GuidObject2(source.ParentId);
            }
            if (!string.IsNullOrEmpty(source.PrecedingId))
            {
                category.PrecedingGuid = new GuidObject2(source.PrecedingId);
            }
            acadedmicPeriod.Category = category;
            return acadedmicPeriod;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Academic Period domain entity to its corresponding Academic Periods DTO
        /// </summary>
        /// <param name="source">Academic Periods domain entity</param>
        /// <returns>EmailType DTO</returns>
        private Dtos.AcademicPeriod3 ConvertAcademicPeriodEntityToAcademicPeriodDto3(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod source, Dtos.EnumProperties.TermRegistrationStatus? termRegistrationStatus)
        {
            var academicPeriod = new Dtos.AcademicPeriod3();
            if (source == null)
            {
                // throw error
                throw new KeyNotFoundException("No academc period found to convert");
            }
            academicPeriod.Id = source.Guid;
            academicPeriod.Code = source.Code;
            academicPeriod.Title = source.Description;
            academicPeriod.Description = null;

            academicPeriod.Start = source.StartDate;
            academicPeriod.End = source.EndDate;

            if (source.RegistrationDates != null)
            {
                if (source.RegistrationDates.FirstOrDefault().CensusDates.Any())
                {
                    academicPeriod.CensusDates = source.RegistrationDates.FirstOrDefault().CensusDates;
                }
                if (termRegistrationStatus != null && termRegistrationStatus != Dtos.EnumProperties.TermRegistrationStatus.NotSet)
                {
                    academicPeriod.RegistrationStatus = termRegistrationStatus;
                }
            }

            var category = new Dtos.AcademicPeriodCategory2();
            switch (source.Category)
            {
                case "year":
                    category.Type = AcademicTimePeriod2.Year;
                    break;
                case "term":
                    category.Type = AcademicTimePeriod2.Term;
                    break;
                case "subterm":
                    category.Type = AcademicTimePeriod2.Subterm;
                    break;
                default:
                    category.Type = IsReportingTermEqualCode(source) ? Dtos.AcademicTimePeriod2.Term : Dtos.AcademicTimePeriod2.Subterm;
                    break;
            }
            if (!string.IsNullOrEmpty(source.ParentId))
            {
                category.ParentGuid = new GuidObject2(source.ParentId);
            }
            if (!string.IsNullOrEmpty(source.PrecedingId))
            {
                category.PrecedingGuid = new GuidObject2(source.PrecedingId);
            }
            academicPeriod.Category = category;

            return academicPeriod;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Academic Period domain entity to its corresponding Academic Periods DTO
        /// </summary>
        /// <param name="source">Academic Periods domain entity</param>
        /// <returns>EmailType DTO</returns>
        private Dtos.AcademicPeriod4 ConvertAcademicPeriodEntityToAcademicPeriodDto4(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod source, Dtos.EnumProperties.TermRegistrationStatus? termRegistrationStatus)
        {
            var academicPeriod = new Dtos.AcademicPeriod4();
            if (source == null)
            {
                // throw error
                throw new KeyNotFoundException("No academc period found to convert");
            }
            academicPeriod.Id = source.Guid;
            academicPeriod.Code = source.Code;
            academicPeriod.Title = source.Description;
            academicPeriod.Description = null;

            academicPeriod.Start = source.StartDate;
            academicPeriod.End = source.EndDate;

            if (source.RegistrationDates != null)
            {
                if (source.RegistrationDates.FirstOrDefault().CensusDates.Any())
                {
                    academicPeriod.CensusDates = source.RegistrationDates.FirstOrDefault().CensusDates;
                }
                if (termRegistrationStatus != null && termRegistrationStatus != Dtos.EnumProperties.TermRegistrationStatus.NotSet)
                {
                    academicPeriod.RegistrationStatus = termRegistrationStatus;
                }
            }

            var category = new Dtos.AcademicPeriodCategory3();
            switch (source.Category)
            {
                case "year":
                    category.Type = AcademicTimePeriod2.Year;
                    break;
                case "term":
                    category.Type = AcademicTimePeriod2.Term;
                    if (!string.IsNullOrEmpty(source.ParentId))
                    {
                        category.Parent = new Dtos.AcademicPeriodCategoryParent();
                        category.Parent.AcademicPeriod = new GuidObject2(source.ParentId);
                    }
                    else
                    {
                        category.Parent = new Dtos.AcademicPeriodCategoryParent();
                        category.Parent.Year = source.ReportingYear;
                    }
                    break;
                case "subterm":
                    category.Type = AcademicTimePeriod2.Subterm;
                    if (!string.IsNullOrEmpty(source.ParentId))
                    {
                        category.Parent = new Dtos.AcademicPeriodCategoryParent();
                        category.Parent.Id = source.ParentId;
                    }
                    break;
                default:
                    category.Type = IsReportingTermEqualCode(source) ? Dtos.AcademicTimePeriod2.Term : Dtos.AcademicTimePeriod2.Subterm;
                    if (!string.IsNullOrEmpty(source.ParentId))
                    {
                        category.Parent = new Dtos.AcademicPeriodCategoryParent();
                        category.Parent.Id = source.ParentId;
                    }
                    break;
            }
            // The schema doesn't allow for a preceding Id on year, unfortunately.
            // The data is there in case we ever add it to the schema, just change
            // the if statement to remove the category check.
            if (!string.IsNullOrEmpty(source.PrecedingId) && source.Category != "year")
            {
                category.PrecedingGuid = new GuidObject2(source.PrecedingId);
            }
            academicPeriod.Category = category;

            return academicPeriod;
        }

        /// <summary>
        /// Determine if a AcademicPeriods reporting term is the same as its code
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private bool IsReportingTermEqualCode(Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod academicPeriod)
        {
            if (academicPeriod == null)
            {
                return false;
            }
            return (academicPeriod.ReportingTerm == academicPeriod.Code);
        }
    }
}