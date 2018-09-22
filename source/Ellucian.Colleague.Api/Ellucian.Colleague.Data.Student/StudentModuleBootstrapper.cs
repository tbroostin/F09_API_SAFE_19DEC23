using System.Web.Http;
using System.Xml.Serialization;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Http.Bootstrapping;
using Microsoft.Practices.Unity;

namespace Ellucian.Colleague.Data.Student
{
    /// <summary>
    /// Perform any IUnityContainer setup necessary for the student data module.
    /// 
    /// Called (by implementing <see cref="IModuleBootstrapper"/>) within the main <see cref="Bootstapper"/> IUnityContainer
    /// container creation code.
    /// </summary>
    public class StudentModuleBootstrapper : IModuleBootstrapper
    {   
        public void BootstrapModule(IUnityContainer container)
        {
            // add rule adapters
            RuleAdapterRegistry rar = container.Resolve<RuleAdapterRegistry>();
            rar.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            rar.Register<Course>("COURSES", new CourseRuleAdapter());

            // Change XML Serializer for PESC XML Transcript requests
            var xml = GlobalConfiguration.Configuration.Formatters.XmlFormatter;
            xml.SetSerializer<TranscriptRequest>(new XmlSerializer(typeof(TranscriptRequest)));
        }
    }
}
