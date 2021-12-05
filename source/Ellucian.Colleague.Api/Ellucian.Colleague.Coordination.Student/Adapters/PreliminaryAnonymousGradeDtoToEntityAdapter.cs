// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for converting a <see cref="Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade"/> to a <see cref="Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade"/>
    /// </summary>
    public class PreliminaryAnonymousGradeDtoToEntityAdapter : BaseAdapter<Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade, Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade>
    {
        /// <summary>
        /// Creates a new <see cref="PreliminaryAnonymousGradeDtoToEntityAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">Interface to adapter registry</param>
        /// <param name="logger">Interface to logger</param>
        public PreliminaryAnonymousGradeDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Creates a <see cref="Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade"/> from a <see cref="Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade"/>
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade"/> to be converted</param>
        /// <returns>A <see cref="Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade"/></returns>
        /// <exception cref="ArgumentNullException">The source preliminary anonymous grade cannot be null.</exception>
        public override Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade MapToType(Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source preliminary anonymous grade cannot be null.");
            }

            Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade entity = new Domain.Student.Entities.AnonymousGrading.PreliminaryAnonymousGrade(source.AnonymousGradingId,
                source.FinalGradeId,
                source.CourseSectionId,
                source.StudentCourseSectionId,
                source.FinalGradeExpirationDate);
            return entity;
        }
    }
}
