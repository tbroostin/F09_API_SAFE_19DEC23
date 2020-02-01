// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class ProgramEvaluationEntityToDto2Adapter : BaseAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>
    {
        /// <summary>
        /// This class maps a program evaluation entity to an outbound program evaluation DTO
        /// </summary>
        ///
        public ProgramEvaluationEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Dtos.Planning.ProgramEvaluation2 MapToType(Domain.Student.Entities.ProgramEvaluation Source)
        {

            var programEvaluationDto = new Dtos.Planning.ProgramEvaluation2();

            programEvaluationDto.ProgramCode = Source.ProgramCode;
            programEvaluationDto.CatalogCode = Source.CatalogCode;
            programEvaluationDto.Credits = Source.Credits;
            programEvaluationDto.InstitutionalCredits = Source.InstitutionalCredits;
            programEvaluationDto.InProgressCredits = Source.InProgressCredits;
            programEvaluationDto.PlannedCredits = Source.PlannedCredits;
            programEvaluationDto.InstGpa = Source.InstGpa;
            programEvaluationDto.CumGpa = Source.CumGpa;
            programEvaluationDto.InstitutionalCreditsModificationMessage = Source.InstitutionalCreditsModificationMessage;
            programEvaluationDto.InstitutionalGpaModificationMessage = Source.InstitutionalGpaModificationMessage;
            programEvaluationDto.OverallCreditsModificationMessage = Source.OverallCreditsModificationMessage;
            programEvaluationDto.OverallGpaModificationMessage = Source.OverallGpaModificationMessage;

            //programEvaluationDto.AllCredit = new List<AcademicCredit>();
            programEvaluationDto.RequirementResults = new List<RequirementResult2>();
                
            // map requirement results
            if (Source.RequirementResults.Count > 0)
            {
                RequirementResultEntityToRequirementResult2DtoAdapter requirementResult2DtoAdapter = new RequirementResultEntityToRequirementResult2DtoAdapter(adapterRegistry, logger);
                programEvaluationDto.RequirementResults.AddRange(Source.RequirementResults.Select(rr => requirementResult2DtoAdapter.MapToType(rr)));
            }

            if (Source.ProgramRequirements != null)
            {
                programEvaluationDto.ProgramRequirements = new ProgramRequirementsDtoAdapter(adapterRegistry, logger).MapToType(Source.ProgramRequirements);
            }

            programEvaluationDto.OtherPlannedCredits = new List<Dtos.Student.Requirements.PlannedCredit>();
            if (Source.OtherPlannedCredits.Count() > 0)
            {
                var evaluationPlannedCreditDtoAdapter = adapterRegistry.GetAdapter<Domain.Student.Entities.Requirements.PlannedCredit, Dtos.Student.Requirements.PlannedCredit>();
                foreach (var plannedCredit in Source.OtherPlannedCredits)
                {
                    var evalPlannedCourse = new Dtos.Student.Requirements.PlannedCredit();
                    evalPlannedCourse = evaluationPlannedCreditDtoAdapter.MapToType(plannedCredit);
                    evalPlannedCourse.CourseId = plannedCredit.Course.Id;
                    programEvaluationDto.OtherPlannedCredits.Add(evalPlannedCourse);
                }
            }

            programEvaluationDto.OtherAcademicCredits = Source.OtherAcademicCredits.ToList();

            return programEvaluationDto;
        }
    }
}
