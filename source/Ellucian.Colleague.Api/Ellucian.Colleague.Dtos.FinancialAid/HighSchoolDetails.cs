using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
      {
      /// <summary>
      /// HighSchool information
      /// </summary>
      public class HighSchoolDetails
            {
            /// <summary>
            /// <see cref="AidApplicationsHSGradtype"/> High school diploma or equivalent
            /// </summary>
            [JsonProperty("gradType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.HS.GRAD.TYPE", DataDescription = "Hight School diploma or equivalent.", DataMaxLength = 25)]
            public AidApplicationsHSGradtype? GradType { get; set; }

            /// <summary>
            /// High school name
            /// </summary>
            [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.HS.NAME", DataDescription = "High school name.")]
            public string Name { get; set; }

            /// <summary>
            /// High school city
            /// </summary>
            [JsonProperty("city", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.HS.CITY", DataDescription = "High school city.")]
            public string City { get; set; }

            /// <summary>
            /// High school state
            /// </summary>
            [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.HS.STATE", DataDescription = "High school state.")]
            public string State { get; set; }

            /// <summary>
            /// High school code
            /// </summary>
            [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.HS.CODE", DataDescription = "High school code.")]
            public string Code { get; set; }
            }
      }
