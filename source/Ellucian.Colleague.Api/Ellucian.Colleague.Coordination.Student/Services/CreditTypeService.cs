// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CreditTypeService : ICreditTypeService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;

        public CreditTypeService(IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Gets all credit types
        /// </summary>
        /// <returns>Collection of CreditCategory DTO objects</returns>
        public IEnumerable<Ellucian.Colleague.Dtos.CreditCategory> GetCreditTypes()
        {
            var creditTypeCollection = new List<Ellucian.Colleague.Dtos.CreditCategory>();

            var creditTypeEntities = _studentReferenceDataRepository.CourseCreditTypes;
            if (creditTypeEntities != null && creditTypeEntities.Count() > 0)
            {
                foreach (var creditType in creditTypeEntities)
                {
                    creditTypeCollection.Add(ConvertCourseCreditTypeEntityToCreditCategoryDto(creditType));
                }
            }
            return creditTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts a CourseStatusItem domain entity to its corresponding CourseStatus DTO
        /// </summary>
        /// <param name="source">CourseStatusItem domain entity</param>
        /// <returns>CourseStatus DTO</returns>
        private Ellucian.Colleague.Dtos.CreditCategory ConvertCourseCreditTypeEntityToCreditCategoryDto(Ellucian.Colleague.Domain.Student.Entities.CourseCreditType source)
        {
            var creditType = new Ellucian.Colleague.Dtos.CreditCategory();

            creditType.Guid = source.Guid;
            creditType.Title = source.Code;
            creditType.Description = source.Description;
            creditType.CreditType = ConvertCreditTypeEntityToParentCategoryDto(source.CreditType);

            return creditType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN LDM</remarks>
        /// <summary>
        /// Converts a CreditType domain entity to its corresponding ParentCategory DTO
        /// </summary>
        /// <param name="source">CreditType domain entity</param>
        /// <returns>ParentCategory DTO</returns>
        private Ellucian.Colleague.Dtos.ParentCategory ConvertCreditTypeEntityToParentCategoryDto(Ellucian.Colleague.Domain.Student.Entities.CreditType source)
        {
            switch (source)
            {
                case Domain.Student.Entities.CreditType.ContinuingEducation:
                    return Dtos.ParentCategory.ContinuingEducation;
                case Domain.Student.Entities.CreditType.Exchange:
                    return Dtos.ParentCategory.Exchange;
                case Domain.Student.Entities.CreditType.Institutional:
                    return Dtos.ParentCategory.Institutional;
                case Domain.Student.Entities.CreditType.Other:
                    return Dtos.ParentCategory.Other;
                case Domain.Student.Entities.CreditType.Transfer:
                    return Dtos.ParentCategory.Transfer;
                default:
                    return Dtos.ParentCategory.None;
            }
        }
    }
}
