// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters 
{
    /// <summary>
    /// Adapter to convert from StudentSectionsAttendances Entity to Dto
    /// </summary>
    public class StudentSectionsAttendancesEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.StudentSectionsAttendances, Dtos.Student.StudentSectionsAttendances> {
        public StudentSectionsAttendancesEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>();
        }

        public override Dtos.Student.StudentSectionsAttendances MapToType(Domain.Student.Entities.StudentSectionsAttendances Source)
        {
            if(Source==null)
            {
                throw new ArgumentNullException("Source- StudentSectionsAttendances entity cannot be null");
            }
            var studentSectionsAttendancesDto = new Ellucian.Colleague.Dtos.Student.StudentSectionsAttendances();
            studentSectionsAttendancesDto.StudentId = Source.StudentId;
            var studentAttendaceAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentAttendance, Ellucian.Colleague.Dtos.Student.StudentAttendance>();
            if (Source.SectionWiseAttendances != null)
            {
                foreach (var sectionAttendance in Source.SectionWiseAttendances)
                {
                    if (!studentSectionsAttendancesDto.SectionWiseAttendances.ContainsKey(sectionAttendance.Key))
                    {
                        studentSectionsAttendancesDto.SectionWiseAttendances.Add(sectionAttendance.Key, new List<Ellucian.Colleague.Dtos.Student.StudentAttendance>());
                    }
                    if (sectionAttendance.Value != null)
                    {
                        foreach (var attendance in sectionAttendance.Value)
                        {
                            studentSectionsAttendancesDto.SectionWiseAttendances[sectionAttendance.Key].Add(studentAttendaceAdapter.MapToType(attendance));
                        }
                    }
                }
            }
           
            return studentSectionsAttendancesDto;
        }
    }
}
