// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentConfigurationService
    {
        Task<Dtos.Student.StudentProfileConfiguration> GetStudentProfileConfigurationAsync();
    }
}
