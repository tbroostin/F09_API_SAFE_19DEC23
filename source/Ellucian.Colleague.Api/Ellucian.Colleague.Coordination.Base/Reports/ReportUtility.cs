// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Reports;
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Data;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Coordination.Base.Utility
{
    public class ReportUtility : IReportUtility
    {
        /// <summary>
        /// Builds a collection of ReportParameter objects from a collection of resource file paths
        /// </summary>
        /// <param name="resourceFilePaths">Collection of resource file physical file paths</param>
        /// <returns>Collection of ReportParameter objects</returns>
        public List<ReportParameter> BuildReportParametersFromResourceFiles(IEnumerable<string> resourceFilePaths)
        {
            if (resourceFilePaths == null || resourceFilePaths.Count() == 0)
            {
                throw new ArgumentNullException("resourceFilePaths");
            }

            var parameters = new List<ReportParameter>();
            foreach (var pathToResourceFile in resourceFilePaths)
            {
                using (ResXResourceReader resourceFile = new ResXResourceReader(pathToResourceFile))
                {
                    foreach (DictionaryEntry entry in resourceFile)
                    {
                        if (entry.Key is string && entry.Value is string)
                        {
                            parameters.Add(BuildReportParameter(entry.Key as string, entry.Value as string));
                        }
                    }
                }
            }
            return parameters;
        }

        /// <summary>
        /// Converts null/empty string report parameter values to the statement's default value in order to prevent statement crashes
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public ReportParameter BuildReportParameter(string parameterName, object parameterValue)
        {
            var cleanParameterValue = parameterValue ?? " ";
            if (cleanParameterValue.ToString().Length < 1)
            {
                cleanParameterValue = " ";
            }
            return new ReportParameter(parameterName, cleanParameterValue.ToString());
        }

        /// <summary>
        /// Determines if a file exists for the given path
        /// </summary>
        /// <param name="path">Path to a file</param>
        /// <returns>Flag indicating whether or not the file exists</returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Converts the given array to dataset
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Dataset</returns>
        public DataSet ConvertToDataSet(Object[] values)
        {
            DataSet ds = new DataSet();

            if (values == null)
            {
                return ds;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(values.GetType());
                StringWriter writer = new StringWriter();
                xmlSerializer.Serialize(writer, values);
                StringReader reader = new StringReader(writer.ToString());
                ds.ReadXml(reader);
            }
            catch (Exception)
            {
                throw;
            }

            return ds;
        }
    }
}
