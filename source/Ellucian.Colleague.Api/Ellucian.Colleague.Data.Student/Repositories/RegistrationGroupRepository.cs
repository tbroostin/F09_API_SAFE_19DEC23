// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RegistrationGroupRepository : BaseColleagueRepository, IRegistrationGroupRepository
    {
        private const string _registrationGroupCacheName = "AllRegistrationGroups";

        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public RegistrationGroupRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            //f09 teresa@toad-code.com 01/14/22
            //background: Waitlist students are not "given permission to register" until after the "standard" section registration window is closed.
            //To make this work,  the registrar uses RGUC to change the "registration ADD end-date"  for the section.
            //Then they give the waitlist student permission-to-register, and the student has 2 days to register.
            //The trouble occurs when the student goes to SS to register.The section "registration ADD end-date" is on a 24 hour cache.

            //solution: for the first month of each term (Jan, May, Sept), there will only be a few students registering,
            //in those months, override the cache timer to 1 minute (instead of 24 hours)
            var month = DateTime.Now.Month;
            if (month == 1 || month == 5 || month == 9)
            {
                CacheTimeout = 1;
            }
            else
            {
                CacheTimeout = Level1CacheTimeoutValue;
            }
            
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Returns the registration group ID for a specific person ID.
        /// </summary>
        /// <param name="personId">The Colleague ID of the person</param>
        /// <returns>The Registration Group Id to use for this person.</returns>
        public async Task<string> GetRegistrationGroupIdAsync(string personId)
        {

            var regGroupId =await GetOrAddToCacheAsync<string>("RegistrationGroup_" + personId,
                async () =>
                {
                    // See if there are any specifically assigned groups for this person
                    var allRegistrationGroups = (await GetAllRegistrationGroupsAsync()).Values;
                    var assignedRegistrationGroups =
                        from r in allRegistrationGroups
                        from s in r.StaffAssignments
                        where s.StaffId == personId
                        select new { r.Id, s.StaffId, s.StartDate, s.EndDate };

                    // If I have any hits then see if we have a valid one (based on the dates) and take the one with earliest start date.  (This could be combined with above statement as well.)
                    var activeGroup = assignedRegistrationGroups.Where(a => (a.StartDate == null || a.StartDate <= DateTime.Today) && (a.EndDate == null || DateTime.Today < a.EndDate)).OrderBy(t => t.StartDate).FirstOrDefault();
                    if (activeGroup != null)
                    {
                        return activeGroup.Id;
                    }

                    // If none are valid get the STWEB.DEFAULTS value or it's default of "NAMELESS".
                    return (await  StwebDefaultsAsync()).StwebRegUsersId;
                }
                );
            return regGroupId;
        }

        /// <summary>
        /// Returns a Registration Group object for the id provided
        /// </summary>
        /// <param name="registrationGroupId">registration group Id</param>
        /// <returns>A registration group entity</returns>
        public async  Task<RegistrationGroup> GetRegistrationGroupAsync(string registrationGroupId)
        {
            if (string.IsNullOrEmpty(registrationGroupId))
            {
                throw new ArgumentNullException("registrationGroupId", "Registration Group Id is required.");
            }
            IDictionary<string, RegistrationGroup> registrationGroups = await GetAllRegistrationGroupsAsync();

            if (registrationGroups.ContainsKey(registrationGroupId))
            {
                return registrationGroups[registrationGroupId];
            }

            return null;
        }

        /// <summary>
        /// Gets all registration groups from cache (or will generate them from the database information). 
        /// </summary>
        /// <returns>GroupId-keyed Dictionary of RegistrationUser domain objects</returns>
        private async Task<IDictionary<string, RegistrationGroup>> GetAllRegistrationGroupsAsync()
        {
            var registrationGroupDict = await GetOrAddToCacheAsync<Dictionary<string, RegistrationGroup>>(_registrationGroupCacheName,
            async () =>
            {
                // Get all Reg Users
                Collection<Ellucian.Colleague.Data.Student.DataContracts.RegUsers> regUserData =await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegUsers>("REG.USERS", "");

                // Breaking this done in case there are thousands of these. Select from REG.USER.SECTIONS to get all the Ids
                var regUserSectionIds =await DataReader.SelectAsync("REG.USER.SECTIONS", "");
                // Retrieve all regUserSections in chunks from the database
                var regUserSectionData = new List<RegUserSections>();
                for (int i = 0; i < regUserSectionIds.Count(); i += readSize)
                {
                    var subList = regUserSectionIds.Skip(i).Take(readSize).ToArray();
                    var bulkData = await DataReader.BulkReadRecordAsync<RegUserSections>("REG.USER.SECTIONS", subList);
                    regUserSectionData.AddRange(bulkData);
                }

                // Get REG.USER.TERM.LOCATIONS
                Collection<Ellucian.Colleague.Data.Student.DataContracts.RegUserTermLocs> regUserTermLocsData =await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegUserTermLocs>("REG.USER.TERM.LOCS", "");

                // Get REG.USER.TERM.LOCATIONS
                Collection<Ellucian.Colleague.Data.Student.DataContracts.RegUserTerms> regUserTermsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegUserTerms>("REG.USER.TERMS", "");

                var registrationGroups = BuildRegistrationGroups(regUserData, regUserSectionData, regUserTermLocsData, regUserTermsData);
                return registrationGroups;
            }
            );
            return registrationGroupDict;
        }

        private Dictionary<string, RegistrationGroup> BuildRegistrationGroups(Collection<RegUsers> regUserData, List<RegUserSections> regUserSectionData, Collection<RegUserTermLocs> regUserTermLocsData, Collection<RegUserTerms> regUserTermsData)
        {
            // Put colleague data into a regUser Id dictionary to improve access
            //   For exammple, The records that come back in the regUserSectionData collection are keyed by RegUserID*section#
            //   We want the resulting dictionary to be keyed by RegUserId with a list of the appropriate items.
            var groupedRegUserSections = regUserSectionData != null ? regUserSectionData.GroupBy(r => r.Recordkey.Substring(0, r.Recordkey.IndexOf("*"))).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<RegUserSections>>();
            var groupedRegUserTermLocs = regUserTermLocsData != null ? regUserTermLocsData.GroupBy(r => r.RgutlRegUser).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<RegUserTermLocs>>();

            var registrationGroups = new Dictionary<string, RegistrationGroup>();
            if (regUserData != null)
            {
                foreach (var regUser in regUserData)
                {
                    try
                    {
                        var registrationGroup = new RegistrationGroup(regUser.Recordkey);
                        if (regUser.RguRegStaffEntityAssociation != null)
                        {
                            foreach (var regStaff in regUser.RguRegStaffEntityAssociation)
                            {
                                try
                                {
                                    var staffAssignment = new StaffAssignment(regStaff.RguStaffIdsAssocMember);
                                    staffAssignment.StartDate = regStaff.RguStaffRegStartDatesAssocMember;
                                    staffAssignment.EndDate = regStaff.RguStaffRegEndDatesAssocMember;
                                    registrationGroup.AddStaffAssignment(staffAssignment);
                                }
                                catch (Exception ex)
                                {
                                    if (logger.IsInfoEnabled)
                                    {
                                        logger.Error("RegUser recordkey " + regUser.Recordkey + " error adding Registration Group staff assignment for staff id " + regStaff.RguStaffIdsAssocMember);
                                        logger.Error("Exception message: " + ex.Message);
                                    }
                                }
                            }
                        }

                        // Next add in the RegistrationUserTerms specific to this registration group (for the applicable terms) 
                        if (regUserTermsData != null)
                        {
                            var matcihngUserTerm = regUserTermsData.Where(s => s.Recordkey.IndexOf(regUser.Recordkey) >= 0);
                            foreach (var regUserTerm in matcihngUserTerm)
                            {
                                var idParts = regUserTerm.Recordkey.Split('*');
                                if (idParts.Count() == 2)
                                {
                                    string termRegUserId = idParts[0];
                                    string termCode = idParts[1];
                                    // Only include terms with the correct reg user 
                                    // Doing the regUserId test again since the name could be a substring within another name and therefore incorrect items could have ended up in the "matching" list.
                                    if (termRegUserId == regUser.Recordkey)
                                    {
                                        try
                                        {
                                            var termRegistrationDate = new TermRegistrationDate(termCode, null,
                                                regUserTerm.RgutRegStartDate,
                                                regUserTerm.RgutRegEndDate,
                                                regUserTerm.RgutPreregStartDate,
                                                regUserTerm.RgutPreregEndDate,
                                                regUserTerm.RgutAddStartDate,
                                                regUserTerm.RgutAddEndDate,
                                                regUserTerm.RgutDropStartDate,
                                                regUserTerm.RgutDropEndDate,
                                                regUserTerm.RgutDropGradeReqdDate,
                                                null
                                                );

                                            registrationGroup.AddTermRegistrationDate(termRegistrationDate);
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error(ex, "Unable to get reg user term.");

                                        }
                                    }
                                }
                            }
                        }

                        // Next add in the RegistrationUserTermlocs specific to this registration group (for the applicable terms) 
                        if (regUserTermLocsData != null)
                        {
                            //var matcihngUserTermLocs = regUserTermLocsData.Where(s => s.Recordkey.IndexOf(regUser.Recordkey) >= 0);
                            if (groupedRegUserTermLocs.ContainsKey(regUser.Recordkey) && groupedRegUserTermLocs[regUser.Recordkey] != null)
                            {
                                foreach (var regUserTermLoc in groupedRegUserTermLocs[regUser.Recordkey])
                                {
                                    string termRegUserId = regUserTermLoc.RgutlRegUser;
                                    string termCode = regUserTermLoc.RgutlTerm;
                                    string location = regUserTermLoc.RgutlLocation;
                                    // Doing the regUserId test again since the name could be a substring within another name and therefore incorrect items could have ended up in the "matching" list.
                                    if (termRegUserId == regUser.Recordkey)
                                    {
                                        try
                                        {
                                            var termRegistrationDate = new TermRegistrationDate(termCode, location,
                                                regUserTermLoc.RgutlRegStartDate,
                                                regUserTermLoc.RgutlRegEndDate,
                                                regUserTermLoc.RgutlPreregStartDate,
                                                regUserTermLoc.RgutlPreregEndDate,
                                                regUserTermLoc.RgutlAddStartDate,
                                                regUserTermLoc.RgutlAddEndDate,
                                                regUserTermLoc.RgutlDropStartDate,
                                                regUserTermLoc.RgutlDropEndDate,
                                                regUserTermLoc.RgutlDropGradeReqdDate,
                                                null
                                                );

                                            registrationGroup.AddTermRegistrationDate(termRegistrationDate);
                                        }
                                            catch (Exception ex)
                                            {
                                                logger.Error(ex, "Unable to get reg user term.");
                                            }

                                        }

                                }
                            }
                        }

                        // Next add in the RegistrationUserSections specific to this registration group (for the applicable terms) 
                        if (regUserSectionData != null)
                        {
                            if (groupedRegUserSections.ContainsKey(regUser.Recordkey) && groupedRegUserSections[regUser.Recordkey] != null)
                            {
                                foreach (var regUserSection in groupedRegUserSections[regUser.Recordkey])
                                {
                                    var keyPieces = regUserSection.Recordkey.Split('*');
                                    if (keyPieces.Count() == 2)
                                    {
                                        string sectionRegUserId = keyPieces[0];
                                        string sectionId = keyPieces[1];
                                        // Only include sections with the correct reg user 
                                        if (sectionRegUserId == regUser.Recordkey)
                                        {
                                            try
                                            {
                                                // In this case the section registration item is created with no location.
                                                var sectionRegistrationDate = new SectionRegistrationDate(sectionId,
                                                string.Empty,
                                                regUserSection.RgucsRegStartDate,
                                                regUserSection.RgucsRegEndDate,
                                                regUserSection.RgucsPreregStartDate,
                                                regUserSection.RgucsPreregEndDate,
                                                regUserSection.RgucsAddStartDate,
                                                regUserSection.RgucsAddEndDate,
                                                regUserSection.RgucsDropStartDate,
                                                regUserSection.RgucsDropEndDate,
                                                regUserSection.RgucsDropGradeReqdDate,
                                                null
                                                );

                                                registrationGroup.AddSectionRegistrationDate(sectionRegistrationDate);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (logger.IsInfoEnabled)
                                                {
                                                    logger.Info("RegUser recordkey " + regUser.Recordkey + " error adding Registration Group section dates for section " + sectionId);
                                                    logger.Info("Exception message: " + ex.Message);
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }

                        // Now add in the registration group to the dictionary.
                        registrationGroups[regUser.Recordkey] = registrationGroup;
                    }
                    catch (Exception ex)
                    {
                        if (logger.IsInfoEnabled)
                        {
                            logger.Info("RegUsers recordkey " + regUser.Recordkey + " Error creating or adding RegUser for registration group.");
                            logger.Info("Exception message: " + ex.Message);
                        }
                    }
                }
            }
            return registrationGroups;
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> StwebDefaultsAsync()
        {
           
                // Giving this item a different cache name than the item in SectionRepository since the one in SectionRepository doesn't set he Nameless default....
                var result = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("RegUserWebDefaults",
                async () =>
                {
                    Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults =await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true);
                    if (stwebDefaults == null)
                    {
                        if (logger.IsInfoEnabled)
                        {
                            var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                            logger.Info(errorMessage);
                        }
                        stwebDefaults = new StwebDefaults();
                    }
                    // If the StWebDefaults record couldn't be read, or if the StwebRegUsersId is null/empty, assume a default RegUserId of "NAMELESS" as per S.GET.REG.USER.CTRL.DATA.
                    // We are not currently supporting the "CE" option since neither CHECK.REGISTRATION.ELIGIBILITY or REGISTER.FOR.SECTIONS does.
                    if (string.IsNullOrEmpty(stwebDefaults.StwebRegUsersId))
                    {
                        stwebDefaults.StwebRegUsersId = "NAMELESS";
                    }
                    return stwebDefaults;
                }, Level1CacheTimeoutValue);

                return result;
           
        }
    }
}
