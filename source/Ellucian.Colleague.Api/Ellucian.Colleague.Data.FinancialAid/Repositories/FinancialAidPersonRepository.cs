/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// FinancialAidPersonRepository class 
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidPersonRepository : PersonBaseRepository, IFinancialAidPersonRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transFactory"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public FinancialAidPersonRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transFactory, ILogger logger, ApiSettings apiSettings)
            :base(cacheProvider, transFactory, logger, apiSettings)
        {
        }

        /// <summary>
        /// Searches for financial aid persons by keyword (last, first last, first middle last name 
        /// combination or person id)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Set of FinancialAidPerson entities</returns>
        public async Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByKeywordAsync(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("criteria");
            }

            List<string> personIds = new List<string>();

            int personId;
            bool isId = int.TryParse(criteria, out personId);
            if (isId)
            {
                //Format the id according to the existing configuration
               string id = await PadIdPerPid2ParamsAsync(criteria);

               if ((await IsApplicantAsync(id)) || (await IsStudentAsync(id))){
                    personIds.Add(id);
                }
            }
            else
            {
                string lastName = "";
                string firstName = "";
                string middleName = "";
                ParseNames(criteria, ref lastName, ref firstName, ref middleName);
                if (string.IsNullOrEmpty(lastName))
                {
                    throw new ArgumentException("Either an id or a last name must be supplied.");
                }
                var foundIds = await SearchByNameAsync(lastName, firstName, middleName);
                foreach(var id in foundIds){
                    if ((await IsApplicantAsync(id)) || (await IsStudentAsync(id)))
                    {
                        personIds.Add(id);
                    }
                }
            }

            if (personIds.Any())
            {
                return await GetPersonsBaseAsync(personIds.AsEnumerable());
            }
            else
            {
                string message = string.Format("Could not locate a financial aid person for specified criteria: {0}", criteria);
                logger.Warn(message);
                throw new ApplicationException(message);
            }

        }

        /// <summary>
        /// Searches for persons for the specified person ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Set of PersonBase entities</returns>
        public async Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids");
            }

            List<string> personIds = new List<string>();

            foreach (var id in ids)
            {
                if ((await IsApplicantAsync(id)) || (await IsStudentAsync(id)))
                {
                    personIds.Add(id);
                }
            }

            if (personIds.Any())
            {
                return await GetPersonsBaseAsync(personIds.AsEnumerable());
            }
            else
            {
                string message = string.Format("Could not locate persons for specified ids");
                logger.Warn(message);
                throw new ApplicationException(message);
            }
        }

        #region Helpers
        /// <summary>
        /// Parses the first, middle, and last names from the string criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        private void ParseNames(string criteria, ref string lastName, ref string firstName, ref string middleName)
        {
            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = criteria.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = criteria.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }
        }
        #endregion
    }
}
