using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Web.Http.Configuration
{
    public interface ISettingsRepository
    {
        Settings Get();
        void Update(Settings settings);
    }
}
