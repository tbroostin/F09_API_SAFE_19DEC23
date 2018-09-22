// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's relationship information.
    /// </summary>
    [RegisterType]
    public class RelationshipRepository : BaseColleagueRepository, IRelationshipRepository
    {
        private List<Domain.Base.Entities.Relationship> _relationships;

        #region Non EEDM

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public RelationshipRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            :base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Gets the relationships between one entity (person or organization) and another entity
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <returns>An enumeration of <see cref="Domain.Base.Entities.Relationship"/></returns>
        public async Task<IEnumerable<Domain.Base.Entities.Relationship>> GetPersonRelationshipsAsync(string id)
        {
            if (_relationships != null)
            {
                return _relationships;
            }
            else
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id");
                }

                _relationships = new List<Domain.Base.Entities.Relationship>();
                // explicit ordering required by the business rules that determine the implicit primary relationship when
                // there is no explicit primary elationship defined.
                var rels = await DataReader.BulkReadRecordAsync<Data.Base.DataContracts.Relationship>("RELATIONSHIP", string.Format("RS.ID1 = {0} OR RS.ID2 = {1} BY RS.START.DATE BY RELATIONSHIP.ID", id, id));
                if (rels != null)
                {
//                    _relationships.AddRange(rels.Select(x => new Domain.Base.Entities.Relationship(x.RsId1, x.RsId2, x.RsRelationType, x.RsPrimaryRelationshipFlag.ToUpper() == "Y", x.RsStartDate, x.RsEndDate)));
                    _relationships.AddRange(rels.Select(x => BuildRelationship(x)));
                }
                return _relationships;    
            }
        }

        /// <summary>
        /// Gets a collection of unique IDs for persons and organizations with whom the supplied person has a relationship
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <returns>Collection of unique IDs for persons and organizations with whom the supplied person has a relationship</returns>
        public async Task<IEnumerable<string>> GetRelatedPersonIdsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            List<string> personIds = new List<string>();
            var relRecords = await this.DataReader.BulkReadRecordAsync<Data.Base.DataContracts.Relationship>("RELATIONSHIP", string.Format("RS.ID1 = {0} OR RS.ID2 = {1} BY RS.START.DATE BY RELATIONSHIP.ID", id, id));
            if (relRecords == null || !relRecords.Any())
            {
                return personIds;
            }

            personIds.AddRange(relRecords.Select(r => r.RsId1));
            personIds.AddRange(relRecords.Select(r => r.RsId2));
            personIds = personIds.Distinct().ToList();

            return personIds;
        }

        /// <summary>
        /// Creates the given relationship between the two given entities
        /// </summary>
        /// <param name="isTheId">P1, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="relationshipType">The relationship in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="ofTheId">P2, in the phrase 'P1 is the "relationship type" of P2 </param>
        /// <returns>the created <see cref="Domain.Base.Entities.Relationship"/></returns>
        public async Task<Domain.Base.Entities.Relationship> PostRelationshipAsync(string isTheId, string relationshipType, string ofTheId)
        {
            if (string.IsNullOrEmpty(isTheId))
            {
                throw new ArgumentNullException("isTheId");
            }
            if (string.IsNullOrEmpty(relationshipType))
            {
                throw new ArgumentNullException("relationshipType");
            }
            if (string.IsNullOrEmpty(ofTheId))
            {
                throw new ArgumentNullException("ofTheId");
            }

            var request = new CreateRelationshipsRequest();
            request.RelationsToCreate = new List<RelationsToCreate>() {new RelationsToCreate() { IsTheIds = isTheId,RelationTypes = relationshipType, OfTheIds = ofTheId } };

            var response = new CreateRelationshipsResponse();
            try
            {
                response = await transactionInvoker.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(request);
                if (response.ErrorInd || response.RelationshipIdentifiers == null || response.RelationshipIdentifiers.Count != 1)
                {
                    foreach (var msg in response.Messages)
                    {
                        logger.Error(msg);
                    }

                    var message = "Error failure encountered by Colleague Transaction CREATE.RELATIONSHIPS.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }
                var data = await DataReader.ReadRecordAsync<Relationship>(response.RelationshipIdentifiers[0]);
                if (data == null)
                {
                    var message = "Could not read relationship record.";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                return BuildRelationship (data);
            }
            catch (Exception ex)
                {
                var message = "Error processing Colleague Transaction CREATE.RELATIONSHIPS.";
                logger.Error(ex, message);
                throw new RepositoryException(message, ex);
            }
        }

        private Domain.Base.Entities.Relationship BuildRelationship(Relationship data)
        {
            return new Domain.Base.Entities.Relationship(data.RsId1, data.RsId2, data.RsRelationType, data.RsPrimaryRelationshipFlag.ToUpper() == "Y", data.RsStartDate, data.RsEndDate) { Guid = data.RecordGuid };
        }

        private Domain.Base.Entities.Relationship BuildRelationship2(Relationship data)
        {
            return new Domain.Base.Entities.Relationship(data.Recordkey, data.RsId1, data.RsId2, data.RsRelationType, data.RsPrimaryRelationshipFlag.ToUpper() == "Y", data.RsStartDate, data.RsEndDate) { Guid = data.RecordGuid };
        }

        #endregion

        #region EEDM Changes
        /// <summary>
        /// Returns all relationships
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetAllAsync(int offset, int limit, bool bypassCache, List<string> guardianRelTypesWithInverse)
        {
            string criteria = "WITH RS.ID1.CORP.INDICATOR NE 'Y' AND RS.ID2.CORP.INDICATOR NE 'Y'";

            if (guardianRelTypesWithInverse != null && guardianRelTypesWithInverse.Any())
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    string guardianRelTypeCriteria = "";
                    foreach (var guardianRelType in guardianRelTypesWithInverse)
                    {
                        guardianRelTypeCriteria += string.Format(" AND WITH RS.RELATION.TYPE NE '{0}'", guardianRelType);
                    }
                    criteria = string.Format("{0}{1}", criteria, guardianRelTypeCriteria);
                }
            }

            string[] relationshipIds = null;
            if ((guardianRelTypesWithInverse == null || !guardianRelTypesWithInverse.Any()))
            {
                relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria);
            }
            else
            {
                relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria, guardianRelTypesWithInverse.ToArray());
            }

            var totalCount = relationshipIds.Count();

            Array.Sort(relationshipIds);

            var subList = relationshipIds.Skip(offset).Take(limit).ToArray();

            //Get paged data for relationship
            var pageData = await DataReader.BulkReadRecordAsync<Relationship>("RELATIONSHIP", subList);

            var combinedList = pageData.Select(i => i.RsId1).Union(pageData.Select(i => i.RsId2));

            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

            //Get comment data
            var commentData = await DataReader.BulkReadRecordAsync<Relation>("RELATION", "WITH RELATION.COMMENTS NE ''");

            List<Domain.Base.Entities.Relationship> relList = new List<Domain.Base.Entities.Relationship>();

            foreach (var item in pageData)
            {
                Domain.Base.Entities.Relationship relationship = BuildRelationship(item);
                relationship.Status = item.RsStatus;
                relationship.Comment = BuildComment(item.RsId1 + "*" + item.RsId2, commentData);
                relationship.SubjectPersonGuid = GetGuid(item.RsId1, personCollection);
                relationship.RelationPersonGuid = GetGuid(item.RsId2, personCollection);
                relationship.SubjectPersonGender = GetGender(item.RsId1, personCollection);
                relationship.RelationPersonGender = GetGender(item.RsId2, personCollection);
                relList.Add(relationship);
            }
            return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>,int>(relList, totalCount);
        }

        /// <summary>
        /// Get a list of personal relationships using criteria
        /// </summary>
        /// <returns>A list of personal relationships Entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetRelationshipsAsync(int offset, int limit, List<string> guardianRelTypesWithInverse, string subjectPerson = "", string relatedPerson = "",
            string directRelationshipType = "", string directRelationshipDetailId = "")
        {
            List<Domain.Base.Entities.Relationship> personalRelationships = new List<Domain.Base.Entities.Relationship>();
            string criteria = "WITH RS.ID1.CORP.INDICATOR NE 'Y' AND RS.ID2.CORP.INDICATOR NE 'Y'";
            if (!string.IsNullOrEmpty(subjectPerson))
            {
                criteria += " AND WITH RS.ID1 = '" + subjectPerson + "'";
            }
            if (!string.IsNullOrEmpty(relatedPerson))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH RS.ID2 = '" + relatedPerson + "'";
            }
            if (!string.IsNullOrEmpty(directRelationshipType))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH RS.RELATION.TYPE = '" + directRelationshipType + "'";
            }
            if (!string.IsNullOrEmpty(directRelationshipDetailId) && !directRelationshipDetailId.Equals(directRelationshipType))
            {
                if (!string.IsNullOrEmpty(criteria) && !string.IsNullOrEmpty(directRelationshipType))
                {
                    criteria += " OR ";
                }
                else if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH RS.RELATION.TYPE = '" + directRelationshipDetailId + "'";
            }

            if (guardianRelTypesWithInverse != null && guardianRelTypesWithInverse.Any())
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    string guardianRelTypeCriteria = string.Empty;
                    foreach (var guardianRelType in guardianRelTypesWithInverse)
                    {
                        guardianRelTypeCriteria += string.Format(" AND WITH RS.RELATION.TYPE NE '{0}'", guardianRelType);
                    }
                    criteria = string.Format("{0}{1}", criteria, guardianRelTypeCriteria);
                }
            }

            int totalCount = 0;
            string[] relationshipIds = null;
            if (!string.IsNullOrEmpty(criteria))
            {
                if ((guardianRelTypesWithInverse == null || !guardianRelTypesWithInverse.Any()))
                {
                    relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria);
                }
                else
                {
                    relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria, guardianRelTypesWithInverse.ToArray());
                }

                totalCount = relationshipIds.Count();

                Array.Sort(relationshipIds);

                var subList = relationshipIds.Skip(offset).Take(limit).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<Relationship>("RELATIONSHIP", subList);

                var relationshipData = new List<Relationship>();
                relationshipData.AddRange(bulkData);

                var combinedList = relationshipData.Select(i => i.RsId1).Union(relationshipData.Select(i => i.RsId2));

                //Get comment data
                var commentData = await DataReader.BulkReadRecordAsync<Relation>("RELATION", "WITH RELATION.COMMENTS NE ''");

                var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

                foreach (var item in relationshipData) {

                    Domain.Base.Entities.Relationship relationship = BuildRelationship(item);
                    //relationship.Guid = item.RecordGuid;
                    relationship.Status = item.RsStatus;
                    relationship.Comment = BuildComment(item.RsId1 + "*" + item.RsId2, commentData);
                    relationship.SubjectPersonGuid = GetGuid(item.RsId1, personCollection);
                    relationship.RelationPersonGuid = GetGuid(item.RsId2, personCollection);
                    relationship.SubjectPersonGender = GetGender(item.RsId1, personCollection);
                    relationship.RelationPersonGender = GetGender(item.RsId2, personCollection);
                    personalRelationships.Add(relationship);
                }
            }

            return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(personalRelationships, totalCount);
        }

        /// <summary>
        /// Get a list of personal relationships using criteria
        /// </summary>
        /// <returns>A list of person relationships Entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetRelationships2Async(int offset, int limit, string person = "", string relationType = "", string inverseRelationType = "")
        {
            List<Domain.Base.Entities.Relationship> personalRelationships = new List<Domain.Base.Entities.Relationship>();
            string criteria = "WITH RS.ID1.CORP.INDICATOR NE 'Y' AND RS.ID2.CORP.INDICATOR NE 'Y'";
            if (!string.IsNullOrEmpty(person))
            {
                criteria += " AND WITH (RS.ID1 = '" + person + "' OR RS.ID2 = '" + person + "')";
            }
            if (!string.IsNullOrEmpty(relationType))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH (RS.RELATION.TYPE = '" + relationType + "' OR RS.RELATION.TYPE = '" + inverseRelationType + "')";
            }
            int totalCount = 0;
            string[] relationshipIds = null;
            if (!string.IsNullOrEmpty(criteria))
            {
               relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria);
               totalCount = relationshipIds.Count();

                Array.Sort(relationshipIds);

                var subList = relationshipIds.Skip(offset).Take(limit).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<Relationship>("RELATIONSHIP", subList);

                var relationshipData = new List<Relationship>();
                relationshipData.AddRange(bulkData);

                var combinedList = relationshipData.Select(i => i.RsId1).Union(relationshipData.Select(i => i.RsId2));

                //Get comment data
                var relationIds = new List<string>();
                foreach (var rel in relationshipData)
                    relationIds.Add(string.Concat(rel.RsId1, "*", rel.RsId2));
                var commentData = await DataReader.BulkReadRecordAsync<Relation>(relationIds.ToArray());

                var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

                foreach (var item in relationshipData)
                {

                    Domain.Base.Entities.Relationship relationship = BuildRelationship2(item);
                    relationship.Status = item.RsStatus;
                    relationship.Comment = BuildComment(item.RsId1 + "*" + item.RsId2, commentData);
                    relationship.SubjectPersonGuid = GetGuid(item.RsId1, personCollection);
                    relationship.RelationPersonGuid = GetGuid(item.RsId2, personCollection);
                    personalRelationships.Add(relationship);
                }
            }

            return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(personalRelationships, totalCount);
        }

        /// <summary>
        /// Get a list of nonpersonal relationships using criteria
        /// </summary>
        /// <returns>A list of nonperson relationships Entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetNonPersonRelationshipsAsync(int offset, int limit, string orgId, string instId, string person, string relationshipType, string inverseRelationshipType, bool bypassCache)

        {
            List<Domain.Base.Entities.Relationship> personalRelationships = new List<Domain.Base.Entities.Relationship>();
            string criteria = "WITH RS.ID1.CORP.INDICATOR EQ 'Y' OR RS.ID2.CORP.INDICATOR EQ 'Y'";
            if (!string.IsNullOrEmpty(person))
            {
                criteria += " AND WITH (RS.ID1 EQ '" + person + "' OR RS.ID2 EQ '" + person + "')";
            }
            if (!string.IsNullOrEmpty(orgId))
            {
                //check to make sure orgId is not an institution
                var instCollection = await DataReader.BulkReadRecordAsync<Institutions>(new string[] { orgId }.ToArray());
                if (IsInstitution(orgId, instCollection))
                {
                    //throw new ArgumentException("Invalid organization '" + orgId + "' in the arguments");
                    return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(personalRelationships, 0);
                }
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH (RS.ID1 EQ '" + orgId + "' OR RS.ID2 EQ '" + orgId + "')";
            }
            if (!string.IsNullOrEmpty(instId))
            {
                //check to make sure instId is an institution
                var instCollection = await DataReader.BulkReadRecordAsync<Institutions>(new string[] { instId }.ToArray());
                if (!IsInstitution(instId, instCollection))
                {
                    //throw new ArgumentException("Invalid institution '" + instId + "' in the arguments");
                    return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(personalRelationships, 0);
                }
                    if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH (RS.ID1 EQ '" + instId + "' OR RS.ID2 EQ '" + instId + "')";
            }
            if (!string.IsNullOrEmpty(relationshipType))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH (RS.RELATION.TYPE EQ '" + relationshipType + "' OR RS.RELATION.TYPE EQ '" + inverseRelationshipType + "')";
            }
            int totalCount = 0;
            string[] relationshipIds = null;
            if (!string.IsNullOrEmpty(criteria))
            {
                relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria);
                totalCount = relationshipIds.Count();

                Array.Sort(relationshipIds);

                var subList = relationshipIds.Skip(offset).Take(limit).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<Relationship>("RELATIONSHIP", subList);

                var relationshipData = new List<Relationship>();
                relationshipData.AddRange(bulkData);

                var combinedList = relationshipData.Select(i => i.RsId1).Union(relationshipData.Select(i => i.RsId2));

                //Get comment data
                var relationIds = new List<string>();
                foreach (var rel in relationshipData)
                    relationIds.Add(string.Concat(rel.RsId1, "*", rel.RsId2));
                var commentData = await DataReader.BulkReadRecordAsync<Relation>(relationIds.ToArray());

                var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

                var instCollection = await DataReader.BulkReadRecordAsync<Institutions>(combinedList.ToArray());

                foreach (var item in relationshipData)
                {
                    Domain.Base.Entities.Relationship relationship = BuildRelationship2(item);
                    relationship.Status = item.RsStatus;
                    relationship.Comment = BuildComment(item.RsId1 + "*" + item.RsId2, commentData);

                    //if Rs.Id1 is a corp, then it becomes the subject. 
                    if (IsCorp(item.RsId1, personCollection))
                    {
                        relationship.SubjectPersonGuid = GetGuid(item.RsId1, personCollection);
                        relationship.RelationPersonGuid = GetGuid(item.RsId2, personCollection);
                        if (IsInstitution(item.RsId1, instCollection))
                            relationship.SubjectPersonInstFlag = true;
                        else
                            relationship.SubjectPersonOrgFlag = true;
                        if (IsCorp(item.RsId2, personCollection))
                        {
                            if (IsInstitution(item.RsId2, instCollection))
                                relationship.RelationPersonInstFlag = true;
                            else
                                relationship.RelationPersonOrgFlag = true;
                        }
                    }
                    else if (IsCorp(item.RsId2, personCollection))
                    {
                        relationship.SubjectPersonGuid = GetGuid(item.RsId2, personCollection);
                        relationship.RelationPersonGuid = GetGuid(item.RsId1, personCollection);
                        if (IsInstitution(item.RsId2, instCollection))
                            relationship.SubjectPersonInstFlag = true;
                        else
                            relationship.SubjectPersonOrgFlag = true;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Concat("The nonpersonal-relationships resource requires atleast one member of the relationship to be an organization/person. Entity:'RELATIONSHIP', Record ID:'", item.Recordkey, "'"));
                    }
                    personalRelationships.Add(relationship);
                }
            }

            return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(personalRelationships, totalCount);
        }


        /// <summary>
        /// Return relationship entity based on guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.Relationship> GetPersonRelationshipByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Relationship id is required.");
            }

            var relationshipId = await GetRecordKeyFromGuidAsync(id);

            if (string.IsNullOrEmpty(relationshipId))
            {
                throw new KeyNotFoundException(string.Concat("Personal relationship record not found. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }

            var relationshipContract = await DataReader.ReadRecordAsync<Relationship>("RELATIONSHIP", relationshipId);
            if (relationshipContract == null)
            {
                throw new KeyNotFoundException(string.Concat("Personal relationship record not found. Entity:'RELATIONSHIP', Record ID:'", relationshipId, "'"));
            }
            var combinedList = new string[] { relationshipContract.RsId1, relationshipContract.RsId2 };
            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

            var isCorporation = personCollection.Any(i => !string.IsNullOrEmpty(i.PersonCorpIndicator) && i.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase));
            if (isCorporation)
            {
                throw new InvalidOperationException("The personal-relationships resource requires that each member of the relationship be a person");
            }

            var relationshipEnity = BuildRelationship(relationshipContract);
            relationshipEnity.Status = relationshipContract.RsStatus;
            relationshipEnity.SubjectPersonGuid = GetGuid(relationshipContract.RsId1, personCollection);
            relationshipEnity.RelationPersonGuid = GetGuid(relationshipContract.RsId2, personCollection);
            var commentData = await DataReader.BulkReadRecordAsync<Relation>("RELATION", "WITH RELATION.COMMENTS NE ''");   //Get comment data
            relationshipEnity.Comment = BuildComment(relationshipContract.RsId1 + "*" + relationshipContract.RsId2, commentData);

            return relationshipEnity;
        }

        /// <summary>
        /// Return relationship entity based on guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.Relationship> GetPersonRelationshipById2Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Person relationship id is required.");
            }

            var relationshipId = await GetPersonRelationshipsIdFromGuidAsync(id);

            if (string.IsNullOrEmpty(relationshipId))
            {
                throw new KeyNotFoundException("Person relationship record not found for id: " + id);
            }

            var relationshipContract = await DataReader.ReadRecordAsync<Relationship>("RELATIONSHIP", relationshipId);
            if (relationshipContract == null)
            {
                throw new KeyNotFoundException("Did not find person relationship for id: " + id);
            }
            var combinedList = new string[] { relationshipContract.RsId1, relationshipContract.RsId2 };
            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());
            var isCorporation = personCollection.Any(i => !string.IsNullOrEmpty(i.PersonCorpIndicator) && i.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase));
            if (isCorporation)
            {
                throw new InvalidOperationException("The person-relationships resource requires that each member of the relationship be a person");
            }

            var relationshipEnity = BuildRelationship2(relationshipContract);
            relationshipEnity.Status = relationshipContract.RsStatus;
            relationshipEnity.SubjectPersonGuid = GetGuid(relationshipContract.RsId1, personCollection);
            relationshipEnity.RelationPersonGuid = GetGuid(relationshipContract.RsId2, personCollection);
            relationshipEnity.SubjectPersonGender = GetGender(relationshipContract.RsId1, personCollection);
            relationshipEnity.RelationPersonGender = GetGender(relationshipContract.RsId2, personCollection);
            //get comment data
            var commentId = string.Concat(relationshipContract.RsId1, "*", relationshipContract.RsId2);
            var commentData = await DataReader.ReadRecordAsync<Relation>(commentId);   
            if (commentData != null && !string.IsNullOrEmpty(commentData.RelationComments))
            {
                relationshipEnity.Comment = commentData.RelationComments;
            }
            return relationshipEnity;
        }

        /// <summary>
        /// Return nonrelationship entity based on guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.Relationship> GetNonPersonRelationshipByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Nonperson relationship id is required.");
            }

            var relationshipId = await GetRecordKeyFromGuidAsync(id);

            if (string.IsNullOrEmpty(relationshipId))
            {
                throw new KeyNotFoundException("Nonperson relationship record not found for id: " + id);
            }

            var relationshipContract = await DataReader.ReadRecordAsync<Relationship>("RELATIONSHIP", relationshipId);
            if (relationshipContract == null)
            {
                throw new KeyNotFoundException("Did not find nonperson relationship for id: " + id);
            }
            var combinedList = new string[] { relationshipContract.RsId1, relationshipContract.RsId2 };
            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());
            var instCollection = await DataReader.BulkReadRecordAsync<Institutions>(combinedList.ToArray());
            var isCorporation = personCollection.Any(i => !string.IsNullOrEmpty(i.PersonCorpIndicator) && i.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase) );
            if (!isCorporation)
            {
                throw new InvalidOperationException("The nonperson relationships requires the one of the relationship holder be an organization or an institution");
            }
            var relationshipEnity = BuildRelationship2(relationshipContract);
            relationshipEnity.Status = relationshipContract.RsStatus;
            if (IsCorp(relationshipContract.RsId1, personCollection))
            {
                relationshipEnity.SubjectPersonGuid = GetGuid(relationshipContract.RsId1, personCollection);
                relationshipEnity.RelationPersonGuid = GetGuid(relationshipContract.RsId2, personCollection);
                if (IsInstitution(relationshipContract.RsId1, instCollection))
                    relationshipEnity.SubjectPersonInstFlag = true;
                else
                    relationshipEnity.SubjectPersonOrgFlag = true;
                if (IsCorp(relationshipContract.RsId2, personCollection))
                {
                    if (IsInstitution(relationshipContract.RsId2, instCollection))
                        relationshipEnity.RelationPersonInstFlag = true;
                    else
                        relationshipEnity.RelationPersonOrgFlag = true;
                }
            }
            // this means RsId1 is a person
            else if (IsCorp(relationshipContract.RsId2, personCollection))
            {
                relationshipEnity.SubjectPersonGuid = GetGuid(relationshipContract.RsId2, personCollection);
                relationshipEnity.RelationPersonGuid = GetGuid(relationshipContract.RsId1, personCollection);
                if (IsInstitution(relationshipContract.RsId2, instCollection))
                    relationshipEnity.SubjectPersonInstFlag = true;
                else
                    relationshipEnity.SubjectPersonOrgFlag = true;
            }
            else
            {
                throw new InvalidOperationException(string.Concat("The nonpersonal-relationships resource requires atleast one member of the relationship to be an organization/person. Entity:'RELATIONSHIP', Record ID:'", relationshipContract.Recordkey, "'"));
            }
            var commentId = string.Concat(relationshipContract.RsId1, "*", relationshipContract.RsId2);
            var commentData = await DataReader.ReadRecordAsync<Relation>(commentId);   //Get comment data
            if (commentData != null && !string.IsNullOrEmpty(commentData.RelationComments))
            {
                relationshipEnity.Comment = commentData.RelationComments;
            }

            return relationshipEnity;
        }

        /// <summary>
        /// Returns all relationships
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>> GetAllGuardiansAsync(int offset, int limit, string person, List<string> guardianRelTypesWithInverse)
        {
            /*
                 var date = await this.GetUnidataFormattedDate(DateTime.Today.ToString());
            
                 Leaving it in here because in future there is a chance that these dates will be addded
                 string.Format("WITH RS.ID1.CORP.INDICATOR NE 'Y' AND RS.ID2.CORP.INDICATOR NE 'Y' AND (RS.END.DATE EQ '' OR RS.END.DATE GT '{0}')", date);
            */
            string criteria = "WITH RS.ID1.CORP.INDICATOR NE 'Y' AND RS.ID2.CORP.INDICATOR NE 'Y'";

            if (!string.IsNullOrEmpty(person))
            {
                string rsIdCriteria = "{0} AND (RS.ID1 EQ '{1}' OR RS.ID2 EQ '{1}')";
                criteria = string.Format(rsIdCriteria, criteria, person);
            }

            if (guardianRelTypesWithInverse != null && guardianRelTypesWithInverse.Any())
            {
                criteria = string.Format("{0} AND RS.RELATION.TYPE EQ '?'", criteria);
            }
           
            string[] relationshipIds = null;

            relationshipIds = await DataReader.SelectAsync("RELATIONSHIP", criteria, guardianRelTypesWithInverse.ToArray());

            var totalCount = relationshipIds.Count();

            Array.Sort(relationshipIds);

            var subList = relationshipIds.Skip(offset).Take(limit).ToArray();

            //Get paged data for relationship
            var pageData = await DataReader.BulkReadRecordAsync<Relationship>("RELATIONSHIP", subList);

            var combinedList = pageData.Select(i => i.RsId1).Union(pageData.Select(i => i.RsId2));

            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

            List<Domain.Base.Entities.Relationship> relList = new List<Domain.Base.Entities.Relationship>();

            foreach (var item in pageData)
            {
                Domain.Base.Entities.Relationship relationship = BuildRelationship(item);
                relationship.Guid = item.RecordGuid;
                relationship.Status = item.RsStatus;
                relationship.SubjectPersonGuid = GetGuid(item.RsId1, personCollection);
                relationship.RelationPersonGuid = GetGuid(item.RsId2, personCollection);
                relList.Add(relationship);
            }
            return new Tuple<IEnumerable<Domain.Base.Entities.Relationship>, int>(relList, totalCount);
        }

        /// <summary>
        /// Gets person guardians based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.Relationship> GetPersonGuardianRelationshipByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Person guardian relationship id is required.");
            }
            var relationshipId = await GetRecordKeyFromGuidAsync(id);
            if (string.IsNullOrEmpty(relationshipId))
            {
                throw new KeyNotFoundException("Person guardian relationship record not found for id: " + id);
            }

            var relationshipContract = await DataReader.ReadRecordAsync<DataContracts.Relationship>("RELATIONSHIP", relationshipId);

            if (relationshipContract == null)
            {
                throw new KeyNotFoundException("Did not find personal relationship for id: " + id);
            }

            var combinedList = new string[] { relationshipContract.RsId1, relationshipContract.RsId2 };
            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());

            var relationshipEnity = BuildRelationship(relationshipContract);
            relationshipEnity.Guid = relationshipContract.RecordGuid;
            relationshipEnity.Status = relationshipContract.RsStatus;
            relationshipEnity.SubjectPersonGuid = GetGuid(relationshipContract.RsId1, personCollection);
            relationshipEnity.RelationPersonGuid = GetGuid(relationshipContract.RsId2, personCollection);

            return relationshipEnity;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetPersonRelationshipsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                //throw new KeyNotFoundException("RELATIONSHIP GUID " + guid + " not found.");
                return string.Empty;
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("RELATIONSHIP GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "RELATIONSHIP")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, RELATIONSHIP", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }



        /// <summary>
        /// Update person relationship
        /// </summary>
        /// <param name="Relationship">personrelationship entity</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        public async Task<Domain.Base.Entities.Relationship> UpdatePersonRelationshipsAsync(Domain.Base.Entities.Relationship personRelationshipsEntity)
        {
            if (personRelationshipsEntity == null)
                throw new ArgumentNullException("personRelationshipsEntity", "Must provide an personRelationshipsEntity to update.");
            if (string.IsNullOrEmpty(personRelationshipsEntity.Guid))
                throw new ArgumentNullException("personRelationshipsEntity", "Must provide the guid of the personRelationshipsEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var personRelationshipsEntityId = await GetPersonRelationshipsIdFromGuidAsync(personRelationshipsEntity.Guid);

            if (!string.IsNullOrEmpty(personRelationshipsEntityId))
            {

                var updateRequest = await BuildPersonRelationshipsUpdateRequest(personRelationshipsEntity);
                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(updateRequest);

                if (updateResponse.ErrorOccurred)
                {
                    var errorMessage = string.Format("Error(s) occurred updating personRelationships '{0}':", personRelationshipsEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    foreach (var error in updateResponse.PersonRelationshipErrors)
                    {
                        if (!string.IsNullOrEmpty(error.ErrorCodes))
                            exception.AddError(new RepositoryError(error.ErrorCodes, error.ErrorMessages));
                        else
                            exception.AddError(new RepositoryError(error.ErrorMessages));
                        logger.Error(error.ErrorMessages);
                    }
                    throw exception;
                }

                // get the updated entity from the database
                return await GetPersonRelationshipById2Async(personRelationshipsEntity.Guid);
            }
            // perform a create instead
            return await CreatePersonRelationshipsAsync(personRelationshipsEntity);

        }

        private async Task<UpdatePersonRelationshipRequest> BuildPersonRelationshipsUpdateRequest(Domain.Base.Entities.Relationship personRelationshipsEntity)
        {
            var combinedList = new string[] { personRelationshipsEntity.PrimaryEntity, personRelationshipsEntity.OtherEntity };
            var personCollection = await DataReader.BulkReadRecordAsync<Person>(combinedList.ToArray());
            var isCorporation = personCollection.Any(i => !string.IsNullOrEmpty(i.PersonCorpIndicator) && i.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase));
            if (isCorporation)
            {
                throw new InvalidOperationException("The person-relationships resource requires that each member of the relationship be a person");
            }
            if (personRelationshipsEntity.Guid == Guid.Empty.ToString())
                personRelationshipsEntity.Guid = string.Empty;

            var request = new UpdatePersonRelationshipRequest
            {
                RelationshipGuid = personRelationshipsEntity.Guid,
                RelationshipId = personRelationshipsEntity.Id,
                SubjectPersonId = personRelationshipsEntity.PrimaryEntity,
                RelatedPersonId = personRelationshipsEntity.OtherEntity,
                DirectRelationship = personRelationshipsEntity.RelationshipType,
                ReciprocalRelationship = personRelationshipsEntity.InverseRelationshipType,
                Status = personRelationshipsEntity.Status,
                StartDate = personRelationshipsEntity.StartDate,
                EndDate = personRelationshipsEntity.EndDate,
                Comments = new List<string>() { personRelationshipsEntity.Comment }
            };
            return request;
        }

        /// <summary>
        /// Create person relationship
        /// </summary>
        /// <param name="Relationship">personrelationship entity</param>
        /// <returns>Domain.Base.Entities.Relationship</returns>
        public async Task<Domain.Base.Entities.Relationship> CreatePersonRelationshipsAsync(Domain.Base.Entities.Relationship personRelationshipsEntity)
        {
            if (personRelationshipsEntity == null)
                throw new ArgumentNullException("personRelationshipsEntity", "Must provide an personRelationshipsEntity to update.");

            var createRequest = await BuildPersonRelationshipsUpdateRequest(personRelationshipsEntity);
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(createRequest);

            if (createResponse.ErrorOccurred)
            {
                var errorMessage = string.Format("Error(s) occurred creating personRelationships '{0}':", personRelationshipsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                foreach (var error in createResponse.PersonRelationshipErrors)
                {
                    if (!string.IsNullOrEmpty(error.ErrorCodes))
                        exception.AddError(new RepositoryError(error.ErrorCodes, error.ErrorMessages));
                    else
                        exception.AddError(new RepositoryError(error.ErrorMessages));
                    logger.Error(error.ErrorMessages);
                }
                throw exception;
            }
            // get the updated entity from the database
            return await GetPersonRelationshipById2Async(createResponse.RelationshipGuid);
        }

        /// <summary>
        /// Delete person relationship
        /// </summary>
        /// <param name="id">personrelationship guid</param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.Relationship> DeletePersonRelationshipAsync(string id)
        {
           var request = new DeleteRelationshipRequest
            {
                RelationshipId = id
            };
            var response = await transactionInvoker.ExecuteAsync<DeleteRelationshipRequest, DeleteRelationshipResponse>(request);
            if (response.DeleteRelationshipErrors != null && response.DeleteRelationshipErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting person relationship with Id '" + id+ "'");
                foreach (var error in response.DeleteRelationshipErrors)
                {
                    if (!string.IsNullOrEmpty(error.ErrorCode))
                        exception.AddError(new RepositoryError(error.ErrorCode, error.ErrorMsg));
                    else
                        exception.AddError(new RepositoryError(error.ErrorMsg));
                    logger.Error(error.ErrorMsg);
                }
                 throw exception;
            }
            return null;
        }           
    

        /// <summary>
        /// Gets guid from person collection
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="personCollection"></param>
        /// <returns></returns>
        private string GetGuid(string recordKey, Collection<Person> personCollection)
        {
            string guid = string.Empty;
            var record = personCollection.FirstOrDefault(i => i.Recordkey.Equals(recordKey, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(i.RecordGuid));

            if (record != null && string.IsNullOrEmpty(record.RecordGuid))
            {
                throw new KeyNotFoundException("No guid found for record key: " + recordKey);
            }
            guid = record.RecordGuid;
            return guid;
        }

        /// <summary>
        /// Gets gender from person collection
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="personCollection"></param>
        /// <returns></returns>
        private string GetGender(string recordKey, Collection<Person> personCollection)
        {
            string gender = string.Empty;
            var record = personCollection.FirstOrDefault(i => i.Recordkey.Equals(recordKey, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(i.Gender));
            if (record != null)
            {
                gender = record.Gender;
            }
            return gender;
        }


        /// <summary>
        /// Checks to see an ID is an organzation/coorporation
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="personCollection"></param>
        /// <returns></returns>
        private bool IsCorp(string recordKey, Collection<Person> personCollection)
        {
            bool isCorp = false;
            var record = personCollection.FirstOrDefault(i => i.Recordkey.Equals(recordKey, StringComparison.OrdinalIgnoreCase));
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.PersonCorpIndicator) && record.PersonCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    isCorp = true;
            }
            return isCorp;
        }


        /// <summary>
        /// Checks to see an ID is an institution
        /// </summary>
        /// <param name="recordKey"></param>
        /// <param name="instCollection"></param>
        /// <returns></returns>
        private bool IsInstitution(string recordKey, Collection<Institutions> instCollection)
        {
            bool instflag = false;
            var record = instCollection.FirstOrDefault(i => i.Recordkey.Equals(recordKey, StringComparison.OrdinalIgnoreCase));
            if (record != null)
            {
                instflag = true;
            }
            return instflag;
        }

        
        /// <summary>
        /// Returns default guardian relationship types set up in CDHP
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDefaultGuardianRelationshipTypesAsync(bool bypassCache)
        {
            if (bypassCache)
            {

                return await BuildDefaultGuardianRelationshipTypes();
            }
            else
            {
                return await GetOrAddToCacheAsync<List<string>>("ALLGUARDIANRELATIONTYPES", async () =>
                {
                    return await BuildDefaultGuardianRelationshipTypes();
                });
            }
        }

        /// <summary>
        /// Builds the list of guardian relationships strings
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> BuildDefaultGuardianRelationshipTypes()
        {
            List<string> defaultGuardianRelations = new List<string>();
            var ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            if (ldmDefaults.LdmdGuardianRelTypes.Any())
            {
                defaultGuardianRelations.AddRange(ldmDefaults.LdmdGuardianRelTypes.ToList());
            }

            return defaultGuardianRelations.Any() ? defaultGuardianRelations : null;
        }

        /// <summary>
        /// Builds comments
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commentData"></param>
        /// <returns></returns>
        private string BuildComment(string id, IEnumerable<Relation> commentData)
        {
            string comment = null;
            if (commentData != null)
            {
                var relation = commentData.FirstOrDefault(x => x.Recordkey.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (relation != null && !string.IsNullOrEmpty(relation.RelationComments))
                {
                    comment = relation.RelationComments;
                }
            }
            return comment;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await InternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        //private ApplValcodes waitlistStatuses;
        private Ellucian.Colleague.Data.Base.DataContracts.IntlParams _internationalParameters;

        private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> InternationalParametersAsync()
        {

            if (_internationalParameters == null)
            {
                // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
                _internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                     async () =>
                     {
                         Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                         if (intlParams == null)
                         {
                             var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                             logger.Info(errorMessage);
                             // If we cannot read the international parameters default to US with a / delimiter.
                             // throw new Exception(errorMessage);
                             Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                             newIntlParams.HostShortDateFormat = "MDY";
                             newIntlParams.HostDateDelimiter = "/";
                             intlParams = newIntlParams;
                         }
                         return intlParams;
                     }, Level1CacheTimeoutValue);
            }
            return _internationalParameters;

        }
        #endregion        
    }
}
