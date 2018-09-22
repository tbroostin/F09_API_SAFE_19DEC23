// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Custom adapter for SectionPermission because this entity contains two lists with the same objects (StudentPetition) and auto mapper was
    /// incorrectly handling the lists 
    /// </summary>

    public class SectionPermissionEntityToDtoAdapter : BaseAdapter<Domain.Student.Entities.SectionPermission, Dtos.Student.SectionPermission>
    {
        public SectionPermissionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Student.SectionPermission MapToType(Domain.Student.Entities.SectionPermission Source)
        {
            var sectionPermission = new Ellucian.Colleague.Dtos.Student.SectionPermission();
            sectionPermission.SectionId = Source.SectionId;
            var studentPetitionAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>();
            foreach (var facultyConsent in Source.FacultyConsents)
            {
                sectionPermission.FacultyConsents.Add(studentPetitionAdapter.MapToType(facultyConsent));
            }
            foreach (var studentPetition in Source.StudentPetitions)
            {
                sectionPermission.StudentPetitions.Add(studentPetitionAdapter.MapToType(studentPetition));
            }
            return sectionPermission;
        }
    }
}
