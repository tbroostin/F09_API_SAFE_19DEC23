// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PreferredSectionRepository : BaseColleagueRepository, IPreferredSectionRepository
    {
        public PreferredSectionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 0;
        }

        public async Task<PreferredSectionsResponse> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("studentId", "Student Id may not be null or empty");
            }
            GetPreferredSectionsRequest getRequest = new GetPreferredSectionsRequest();
            getRequest.StudentId = studentId;
            GetPreferredSectionsResponse getResponse = await transactionInvoker.ExecuteAsync<GetPreferredSectionsRequest, GetPreferredSectionsResponse>(getRequest);

            List<PreferredSection> sections = new List<PreferredSection>();
            List<PreferredSectionMessage> messages = new List<PreferredSectionMessage>();
            foreach (var prefSec in getResponse.PreferredSections)
            {
                sections.Add(new PreferredSection(studentId, prefSec.CourseSectionId, prefSec.Credits));
            }
            foreach (var msg in getResponse.PrefSecGetMessages)
            {
                messages.Add(new PreferredSectionMessage(msg.MessageSectionId, msg.MessageText));
            }
            PreferredSectionsResponse response = new PreferredSectionsResponse(sections, messages);
            return response;
        }

        public async Task<IEnumerable<PreferredSectionMessage>> UpdateAsync(string studentId, List<PreferredSection> sections)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("studentId", "Student Id may not be null or empty");
            }
            if (sections == null || 0 == sections.Count())
            {
                throw new ArgumentException("sections", "Sections may not be null or empty");
            }
            // setup the update transaction
            UpdatePreferredSectionsRequest updateRequest = new UpdatePreferredSectionsRequest();
            updateRequest.StudentId = studentId;
            updateRequest.PreferredSectionList = new List<PreferredSectionList>();
            List<PreferredSectionMessage> results = new List<PreferredSectionMessage>();
            foreach (var section in sections)
            {
                if (!section.StudentId.Equals(studentId))
                {
                    results.Add(new PreferredSectionMessage(section.SectionId, "PreferredSection not for Student"));
                }
                else
                {
                    updateRequest.PreferredSectionList.Add(new PreferredSectionList() { CourseSectionId = section.SectionId, Credits = section.Credits });
                }
            }
            if (updateRequest.PreferredSectionList.Count() <= 0)
            {
                results.Add(new PreferredSectionMessage("sections", "No valid sections for update"));
            }
            else
            {
                UpdatePreferredSectionsResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdatePreferredSectionsRequest, UpdatePreferredSectionsResponse>(updateRequest);
                foreach (var msg in updateResponse.PrefSecUpdateMessages)
                {
                    results.Add(new PreferredSectionMessage(msg.MessageSectionId, msg.MessageText));
                }
            }
            return results;
        }

        public async Task<IEnumerable<PreferredSectionMessage>> DeleteAsync(string studentId, string sectionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("studentId", "Student Id may not be null or empty");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("sectionId", "Section Id may not be null or empty");
            }
            List<PreferredSectionMessage> results = new List<PreferredSectionMessage>();
            DeletePreferredSectionsRequest deleteRequest = new DeletePreferredSectionsRequest();
            deleteRequest.StudentId = studentId;
            deleteRequest.CourseSectionIds.Add(sectionId);
            DeletePreferredSectionsResponse deleteResponse = await transactionInvoker.ExecuteAsync<DeletePreferredSectionsRequest, DeletePreferredSectionsResponse>(deleteRequest);
            foreach (var msg in deleteResponse.PrefSecDeleteMessages)
            {
                results.Add(new PreferredSectionMessage(msg.MessageSectionId, msg.MessageText));
            }
            return results;
        }


/*        public IEnumerable<PreferredSectionMessage> Delete(string studentId, List<string> sectionIds)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("studentId", "Student Id may not be null or empty");
            }
            if (sectionIds == null || 0 == sectionIds.Count())
            {
                throw new ArgumentException("sectionIds", "SectionIds may not be null or empty");
            }
            List<PreferredSectionMessage> results = new List<PreferredSectionMessage>();
            DeletePreferredSectionsRequest deleteRequest = new DeletePreferredSectionsRequest();
            deleteRequest.StudentId = studentId;
            int pos = 0;
            foreach (var sectionId in sectionIds) 
            {
                pos += 1;
                if (string.IsNullOrEmpty(sectionId))
                {
                    results.Add(new PreferredSectionMessage("sectionId "+pos.ToString(), "SectionId may not be null or empty"));
                }
                else
                {
                    deleteRequest.CourseSectionIds.Add(sectionId);
                }
            }
            if (deleteRequest.CourseSectionIds.Count() <= 0)
            {
                results.Add(new PreferredSectionMessage("SectionIds", "No valid sectionId for delete"));
            }
            else
            {
                DeletePreferredSectionsResponse deleteResponse = transactionInvoker.Execute<DeletePreferredSectionsRequest, DeletePreferredSectionsResponse>(deleteRequest);
                foreach (var msg in deleteResponse.PrefSecDeleteMessages)
                {
                    results.Add(new PreferredSectionMessage(msg.MessageSectionId, msg.MessageText));
                }
            }
            return results;
        }
*/
    }
}
