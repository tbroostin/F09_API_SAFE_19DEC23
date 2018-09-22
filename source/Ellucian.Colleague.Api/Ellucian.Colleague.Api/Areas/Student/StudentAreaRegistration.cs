using System.Web.Mvc;

namespace Ellucian.Colleague.Api.Areas.Student
{
    /// <summary>
    /// Handle area registration for student area.
    /// </summary>
    public class StudentAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// Gets the area name.
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "Student";
            }
        }

        /// <summary>
        /// Performs the area registration specific to this area.
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Student_default",
                "Student/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
