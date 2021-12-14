// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CommentsService : BaseCoordinationService, ICommentsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IRemarkRepository _remarkRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public CommentsService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            IRemarkRepository remarkRepository, IReferenceDataRepository referenceDataRepository, IPersonRepository personRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _remarkRepository = remarkRepository;
            _personRepository = personRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        #region Public Methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <returns>Collection of Comments DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Comments>, int>> GetCommentsAsync(int offset, int limit, string subjectMatter, string commentSubjectArea, bool bypassCache = false)
        {
            
            var commentsCollection = new List<Ellucian.Colleague.Dtos.Comments>();

            #region  Convert and validate all input parameters
            var newSubjectMatter = string.Empty;
            if (!string.IsNullOrEmpty(subjectMatter))
            {
                try
                {
                    newSubjectMatter = await _personRepository.GetPersonIdFromGuidAsync(subjectMatter);
                    if (string.IsNullOrEmpty(newSubjectMatter))
                    {
                        return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, 0);
                }
            }
            var newCommentSubjectArea = string.Empty;
            if (!string.IsNullOrEmpty(commentSubjectArea))
            {
                try
                {
                    newCommentSubjectArea = ConvertGuidToCode(await _referenceDataRepository.GetRemarkTypesAsync(true), commentSubjectArea);
                    if (string.IsNullOrEmpty(newCommentSubjectArea))
                    {
                        return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, 0);
                }
            }

            #endregion

            #region Repository

            Tuple<IEnumerable<Remark>, int> remarksEntities = null;

            try
            {
                remarksEntities = await _remarkRepository.GetRemarksAsync(offset, limit, newSubjectMatter, newCommentSubjectArea);
            }

            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (remarksEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, 0);
            }

            #endregion

            #region Convert

            var totalRecords = remarksEntities.Item2;


            var remarksDonorIds = remarksEntities.Item1
                     .Where(x => (!string.IsNullOrEmpty(x.RemarksDonorId)))
                     .Select(x => x.RemarksDonorId).Distinct().ToList();

            var remarksRemarksAuthorIds = remarksEntities.Item1
                    .Where(x => (!string.IsNullOrEmpty(x.RemarksAuthor)))
                    .Select(x => x.RemarksAuthor).Distinct().ToList();


            var personIds = remarksDonorIds.Union(remarksRemarksAuthorIds);


            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);


            foreach (var remarkEntity in remarksEntities.Item1)
            {
                if (remarkEntity.Guid != null)
                {
                    var remarkDto = await ConvertRemarkEntityToCommentsDtoAsync(remarkEntity, personGuidCollection, bypassCache);
                    commentsCollection.Add(remarkDto);
                }
            }

            #endregion

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a comment from its GUID
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        /// <returns>Comments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Comments> GetCommentByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a comments.");
            }

            Remark remark = null;
            try
            {
                remark = await _remarkRepository.GetRemarkByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No comments was found for GUID " + guid);

            }

            if (remark == null)
            {
                throw new KeyNotFoundException("No comments was found for GUID " + guid);
            }

            var personIds = new List<string> { remark.RemarksDonorId, remark.RemarksAuthor };
            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);

            var retVal = await ConvertRemarkEntityToCommentsDtoAsync(remark, personGuidCollection);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return retVal;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Create a new Remark
        /// </summary>
        /// <param name="comments">Comments DTO</param>
        /// <returns>Comments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Comments> PostCommentAsync(Dtos.Comments comments)
        {
            if (comments == null)
            {
                throw new ArgumentNullException("comment", "Comments body required.");
            }

            if (string.IsNullOrEmpty(comments.Id))
            {
                throw new ArgumentNullException("comment", "Comments id required.");
            }

         
            _remarkRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            #region create domain entity from request
            Remark remarkEntityRequest = null;

            try
            {
                remarkEntityRequest = await this.ConvertCommentsDtoToRemarkEntityAsync(comments);
            }

            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created.  Error extracting request. " + ex.Message, "Global.Internal.Error",
                   remarkEntityRequest != null && !string.IsNullOrEmpty(remarkEntityRequest.Guid) ? remarkEntityRequest.Guid : null,
                   remarkEntityRequest != null && !string.IsNullOrEmpty(remarkEntityRequest.Id) ? remarkEntityRequest.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Create record from domain entity

            Remark newEntity = null;

            try
            {
                newEntity = await _remarkRepository.UpdateRemarkAsync(remarkEntityRequest);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                     newEntity != null && !string.IsNullOrEmpty(newEntity.Guid) ? newEntity.Guid : null,
                     newEntity != null && !string.IsNullOrEmpty(newEntity.Id) ? newEntity.Id : null);
                throw IntegrationApiException;
            }

            #endregion

            #region Build DTO response

            Dtos.Comments newDto = null;

            try
            {
                var personIds = new List<string> { newEntity.RemarksDonorId, newEntity.RemarksAuthor };
                var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);


                newDto = await ConvertRemarkEntityToCommentsDtoAsync(newEntity, personGuidCollection);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record created. Error building response. " + ex.Message, "Global.Internal.Error", newEntity.Guid, newEntity.Id);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            return newDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete Remark by ID
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        public async Task DeleteCommentByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to delete a Comments.");
            }

            try
            {

                var comment = await _remarkRepository.GetRemarkByGuidAsync(guid);

                if (comment == null)
                {
                    throw new KeyNotFoundException();
                }

                await _remarkRepository.DeleteRemarkAsync(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No comments was found for GUID " + guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error", guid);

                throw IntegrationApiException;
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update comment  from its GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Comments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Comments> PutCommentAsync(string id, Dtos.Comments comments)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("guid", "GUID is required to update a Comment.");
            }

            if (comments == null)
            {
                throw new ArgumentNullException("comments", "Message body required to update a comment");
            }

            _remarkRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            #region create domain entity from request
            Remark remarkEntityRequest = null;

            try
            {
                remarkEntityRequest = await this.ConvertCommentsDtoToRemarkEntityAsync(comments);
            }

            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated.  Error extracting request. " + ex.Message, "Global.Internal.Error",
                   remarkEntityRequest != null && !string.IsNullOrEmpty(remarkEntityRequest.Guid) ? remarkEntityRequest.Guid : null,
                   remarkEntityRequest != null && !string.IsNullOrEmpty(remarkEntityRequest.Id) ? remarkEntityRequest.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Create record from domain entity

            Remark newEntity = null;

            try
            {
                newEntity = await _remarkRepository.UpdateRemarkAsync(remarkEntityRequest);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                     newEntity != null && !string.IsNullOrEmpty(newEntity.Guid) ? newEntity.Guid : null,
                     newEntity != null && !string.IsNullOrEmpty(newEntity.Id) ? newEntity.Id : null);
                throw IntegrationApiException;
            }

            #endregion

            #region Build DTO response

            Dtos.Comments newDto = null;

            try
            {
                var personIds = new List<string> { newEntity.RemarksDonorId, newEntity.RemarksAuthor };
                var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);


                newDto = await ConvertRemarkEntityToCommentsDtoAsync(newEntity, personGuidCollection);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record updated. Error building response. " + ex.Message, "Global.Internal.Error", newEntity.Guid, newEntity.Id);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion


            return newDto;

        }

        #endregion

        #region Private Methods

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Convert remarks domain entity to a comments DTO
        /// </summary>
        /// <param name="source">Remark domain entity</param>
        /// <returns>Comments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Comments> ConvertRemarkEntityToCommentsDtoAsync(Ellucian.Colleague.Domain.Base.Entities.Remark source,
             Dictionary<string, string> personGuidCollection, bool bypassCache = true)
        {
            var comments = new Ellucian.Colleague.Dtos.Comments();

            if (source == null)
            {
                IntegrationApiExceptionAddError("Remarks domain entity must be provided.");
                return comments;
            }

            comments.Id = source.Guid;

            comments.Confidentiality = ConvertConfidentialityTypeEnumToConfidentialityCategoryEnum(source.RemarksPrivateType);

            if (!string.IsNullOrEmpty(source.RemarksText))
                comments.Comment = source.RemarksText.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                      .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                      .Replace(Convert.ToChar(DynamicArray.SM), ' ');
            comments.EnteredOn = source.RemarksDate;



            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Concat("Person GUID not found for subjectMatter.person.id: '", source.RemarksDonorId, "'"), "GUID.Not.Found"
                    , source.Id, source.Guid);
            }
            else
            {
                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(source.RemarksDonorId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                    IntegrationApiExceptionAddError(string.Concat("Person GUID not found for subjectMatter.person.id: '", source.RemarksDonorId, "'"), "GUID.Not.Found"
                        , source.Id, source.Guid);
                }
                else
                {
                    var subjectMatter = new Dtos.DtoProperties.SubjectMatterDtoProperty();

                    if (source.RemarksInstIndicator == true)
                    {
                        subjectMatter.Institution = new Dtos.GuidObject2(personGuid);
                    }
                    else if (source.RemarksPersonCorpIndicator)
                    {
                        subjectMatter.Organization = new Dtos.GuidObject2(personGuid);
                    }
                    else
                    {
                        subjectMatter.Person = new Dtos.GuidObject2(personGuid);
                    }

                    comments.SubjectMatter = subjectMatter;
                }
            }

            if (!string.IsNullOrEmpty(source.RemarksIntgEnteredBy))
            {
                comments.EnteredBy = new Dtos.DtoProperties.EnteredByDtoProperty() { Name = source.RemarksIntgEnteredBy };

            }
            else if (!string.IsNullOrEmpty(source.RemarksAuthor))
            {

                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Person GUID not found for enteredBy.id: '", source.RemarksAuthor, "'"), "GUID.Not.Found"
                        , source.Id, source.Guid);
                }
                else
                {
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.RemarksAuthor, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person GUID not found for enteredBy.id: '", source.RemarksAuthor, "'"), "GUID.Not.Found"
                            , source.Id, source.Guid);
                    }
                    else
                    {
                        comments.EnteredBy = new Dtos.DtoProperties.EnteredByDtoProperty() { Id = personGuid };
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.RemarksType))
            {
                try
                {
                    var remarkTypeGuid = await _referenceDataRepository.GetRemarkTypesGuidAsync(source.RemarksType);

                    if (string.IsNullOrEmpty(remarkTypeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("No GUID found, Entity:'REMARK.TYPES', Record ID: '", source.RemarksType, "'"), "GUID.Not.Found"
                            , source.Id, source.Guid);
                    }
                    else
                    {
                        comments.CommentSubjectArea = new Dtos.GuidObject2(remarkTypeGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found", source.Id, source.Guid);
                }
            }

            if (!string.IsNullOrEmpty(source.RemarksCode))
            {

                try
                {
                    var remarkCodeGuid = await _referenceDataRepository.GetRemarkCodesGuidAsync(source.RemarksCode);

                    if (string.IsNullOrEmpty(remarkCodeGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("GNo GUID found, Entity:'REMARK.CODES', Record ID: '", source.RemarksCode, "'"), "GUID.Not.Found"
                            , source.Id, source.Guid);
                    }
                    else
                    {
                        comments.Source = new Dtos.GuidObject2(remarkCodeGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found", source.Id, source.Guid);
                }
            }


            return comments;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert a comments dto to a remark domain entity
        /// </summary>
        /// <param name="source">Comments DTO</param>
        /// <returns>Remark domain entity</returns>
        private async Task<Remark> ConvertCommentsDtoToRemarkEntityAsync(Dtos.Comments source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("Comments DTO must be provided.", "Validation.Exception");
                throw IntegrationApiException;
            }


            if (source.Id == null)
            {
                IntegrationApiExceptionAddError("Comments Id must be provided.", "Validation.Exception");
            }

            if ((source.Comment == null) || (string.IsNullOrEmpty(source.Comment)))
            {
                IntegrationApiExceptionAddError("comment text required.", "Validation.Exception");
            }

            if (source.SubjectMatter == null)
            {
                IntegrationApiExceptionAddError("subjectMatter is required.", "Validation.Exception");
            }
            else
            {
                if (source.SubjectMatter.InstitutionUnit != null)
                {
                    IntegrationApiExceptionAddError("Comments are not permitted for an institution unit.", "Validation.Exception");
                }

                string personId = string.Empty;
                if (source.SubjectMatter.Person != null)
                {
                    if (string.IsNullOrEmpty(source.SubjectMatter.Person.Id))
                    {
                        IntegrationApiExceptionAddError("subjectMatter.person.id is required.", "Validation.Exception");
                    }
                    else
                    {
                        // Verify that the input is a person
                        Dictionary<string, string> dataDictionary;
                        personId = await _personRepository.GetPersonIdFromGuidAsync(source.SubjectMatter.Person.Id);
                        var personDictionary = await _remarkRepository.GetPersonDictionaryCollectionAsync(new List<string>() { personId });
                        if (personDictionary.TryGetValue(personId, out dataDictionary))
                        {
                            string data = string.Empty;
                            if (dataDictionary.TryGetValue("PERSON.CORP.INDICATOR", out data))
                            {
                                if (data.Equals("Y", StringComparison.OrdinalIgnoreCase))
                                {
                                    IntegrationApiExceptionAddError("The requested subjectMatter is not defined as a person.", "Validation.Exception");
                                }
                                else
                                {
                                    if (dataDictionary.TryGetValue("WHERE.USED", out data))
                                    {
                                        if (data.Contains("INSTITUTIONS"))
                                        {
                                            IntegrationApiExceptionAddError("The requested subjectMatter is not defined as a person.", "Validation.Exception");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (source.SubjectMatter.Organization != null)
                    {
                        if (string.IsNullOrEmpty(source.SubjectMatter.Organization.Id))
                        {
                            IntegrationApiExceptionAddError("subjectMatter.organization.id is required.", "Validation.Exception");
                        }
                        else
                        {
                            // Verify that the input is an organization
                            Dictionary<string, string> dataDictionary;
                            personId = await _personRepository.GetPersonIdFromGuidAsync(source.SubjectMatter.Organization.Id);
                            var personDictionary = await _remarkRepository.GetPersonDictionaryCollectionAsync(new List<string>() { personId });
                            if (personDictionary.TryGetValue(personId, out dataDictionary))
                            {
                                string data = string.Empty;
                                if (dataDictionary.TryGetValue("PERSON.CORP.INDICATOR", out data))
                                {
                                    if (!data.Equals("Y", StringComparison.OrdinalIgnoreCase))
                                    {
                                        IntegrationApiExceptionAddError("The requested subjectMatter is not defined as an organization.", "Validation.Exception");
                                    }
                                    else
                                    {
                                        if (dataDictionary.TryGetValue("WHERE.USED", out data))
                                        {
                                            if (data.Contains("INSTITUTIONS"))
                                            {
                                                IntegrationApiExceptionAddError("The requested subjectMatter is not defined as an organization.", "Validation.Exception");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (source.SubjectMatter.Institution != null)
                        {
                            if (string.IsNullOrEmpty(source.SubjectMatter.Institution.Id))
                            {
                                IntegrationApiExceptionAddError("subjectMatter.institution.id is required.", "Validation.Exception");
                            }
                            else
                            {
                                // Verify that the input is an institution
                                Dictionary<string, string> dataDictionary;
                                personId = await _personRepository.GetPersonIdFromGuidAsync(source.SubjectMatter.Institution.Id);
                                var personDictionary = await _remarkRepository.GetPersonDictionaryCollectionAsync(new List<string>() { personId });
                                if (personDictionary.TryGetValue(personId, out dataDictionary))
                                {
                                    string data = string.Empty;
                                    if (dataDictionary.TryGetValue("WHERE.USED", out data))
                                    {
                                        if (!data.Contains("INSTITUTIONS"))
                                        {
                                            IntegrationApiExceptionAddError("The requested subjectMatter is not defined as an institution.", "Validation.Exception");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if ((source.SubjectMatter.Person != null && (source.SubjectMatter.Organization != null || source.SubjectMatter.Institution != null))
                    || (source.SubjectMatter.Organization != null && (source.SubjectMatter.Person != null || source.SubjectMatter.Institution != null))
                    || (source.SubjectMatter.Institution != null && (source.SubjectMatter.Person != null || source.SubjectMatter.Organization != null)))
                {
                    IntegrationApiExceptionAddError("Only one of subjectMatter.person, subjectMatter.organization or subjectMatter.institution can be defined.", "Validation.Exception");
                }
            }
            if ((source.CommentSubjectArea != null) && (source.CommentSubjectArea.Id == null))
            {
                IntegrationApiExceptionAddError("commentSubjectArea.id is required.", "Validation.Exception");
            }

            if ((source.Source != null) && (source.Source.Id == null))
            {
                IntegrationApiExceptionAddError("source.id is required.", "Validation.Exception");
            }

            var comments = new Remark(source.Id)
            {
                RemarksDate = source.EnteredOn,
                RemarksText = source.Comment
            };

            if (source.Source != null && !string.IsNullOrEmpty(source.Source.Id))
            {
                var remarksCode = ConvertGuidToCode(await _referenceDataRepository.GetRemarkCodesAsync(true), source.Source.Id);
                if (string.IsNullOrEmpty(remarksCode))
                {
                    IntegrationApiExceptionAddError(string.Concat("The source specified is not intended for use with comments or source not found for ID: ", source.Source.Id), "Validation.Exception");
                }
                comments.RemarksCode = remarksCode;
            }

            if (source.CommentSubjectArea != null && !string.IsNullOrEmpty(source.CommentSubjectArea.Id))
            {
                var remarksType = ConvertGuidToCode(await _referenceDataRepository.GetRemarkTypesAsync(true), source.CommentSubjectArea.Id);
                if (string.IsNullOrEmpty(remarksType))
                {
                    IntegrationApiExceptionAddError(string.Concat("commentSubjectArea not found for ID: ", source.CommentSubjectArea.Id), "Validation.Exception");
                }
                comments.RemarksType = remarksType;
            }

            comments.RemarksPrivateType = ConvertConfidentialityCategoryEnumToConfidentialityTypeEnum(source.Confidentiality);

            if (source.EnteredBy != null)
            {
                if (!string.IsNullOrEmpty(source.EnteredBy.Name))
                {
                    comments.RemarksIntgEnteredBy = source.EnteredBy.Name;
                }
                else if (string.IsNullOrEmpty(source.EnteredBy.Id))
                {
                    IntegrationApiExceptionAddError("enteredBy.id is required", "Validation.Exception");
                }
                else
                {
                    try
                    {
                        var id = await _personRepository.GetPersonIdFromGuidAsync(source.EnteredBy.Id);
                        if (string.IsNullOrEmpty(id))
                        {
                            IntegrationApiExceptionAddError(string.Concat("Person GUID not found for enteredBy.id: ", source.EnteredBy.Id), "Validation.Exception");
                        }
                        comments.RemarksAuthor = id;
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person GUID not found for enteredBy.id: ", source.EnteredBy.Id), "Validation.Exception");
                    }
                }
            }

            var personGuid = string.Empty;
            var personGuidSource = "person";
            if (source.SubjectMatter != null)
            {
                if (source.SubjectMatter.Person != null)
                {
                    if (string.IsNullOrEmpty(source.SubjectMatter.Person.Id))
                    {
                        IntegrationApiExceptionAddError("subjectMatter.person.id is required.", "Validation.Exception");
                    }
                    else
                    {
                        personGuid = source.SubjectMatter.Person.Id;
                    }
                }
                if (string.IsNullOrEmpty(personGuid))
                {
                    personGuidSource = "organization";
                    if (source.SubjectMatter.Organization != null)
                    {
                        if (string.IsNullOrEmpty(source.SubjectMatter.Organization.Id))
                        {
                            IntegrationApiExceptionAddError("subjectMatter.organization.id is required.", "Validation.Exception");
                        }
                        else
                        {
                            personGuid = source.SubjectMatter.Organization.Id;
                        }
                    }
                }
                if (string.IsNullOrEmpty(personGuid))
                {
                    personGuidSource = "institution";
                    if (source.SubjectMatter.Institution != null)
                    {
                        if (string.IsNullOrEmpty(source.SubjectMatter.Institution.Id))
                        {
                            IntegrationApiExceptionAddError("subjectMatter.institution.id is required.", "Validation.Exception");
                        }
                        else
                        {
                            personGuid = source.SubjectMatter.Institution.Id;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(personGuid))
            {
                IntegrationApiExceptionAddError("Either subjectMatter.person.id, subjectMatter.organization.id, or subjectMatter.institution.id is required.", "Validation.Exception");
            }
            else
            {
                try
                {
                    var id = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                    if (string.IsNullOrEmpty(id))
                    {
                        IntegrationApiExceptionAddError(string.Format("Person GUID not found for subjectMatter.{0}.id: '{1}'", personGuidSource, personGuid), "Validation.Exception");
                    }
                    comments.RemarksDonorId = id;
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Person GUID not found for subjectMatter.{0}.id: '{1}'", personGuidSource, personGuid), "Validation.Exception");
                }
            }

            return comments;
        }

        /// <summary>
        /// Convert ConfidentialityTypeEnum to ConfidentialityCategoryEnum
        /// </summary>
        /// <param name="confidentialityType"></param>
        /// <returns>Dtos.EnumProperties.ConfidentialCategory</returns>
        private Dtos.EnumProperties.ConfidentialCategory ConvertConfidentialityTypeEnumToConfidentialityCategoryEnum(ConfidentialityType? confidentialityType)
        {
            if (confidentialityType == null)
                return Dtos.EnumProperties.ConfidentialCategory.Public;

            switch (confidentialityType)
            {
                case ConfidentialityType.Public:
                    return Dtos.EnumProperties.ConfidentialCategory.Public;
                case ConfidentialityType.Private:
                    return Dtos.EnumProperties.ConfidentialCategory.Private;
                default:
                    return Dtos.EnumProperties.ConfidentialCategory.Public;
            }
        }

        /// <summary>
        /// Convert ConfidentialityCategoryEnum to ConfidentialityTypeEnum
        /// </summary>
        /// <param name="confidentialityCategory"></param>
        /// <returns>ConfidentialityType</returns>
        private ConfidentialityType ConvertConfidentialityCategoryEnumToConfidentialityTypeEnum(Dtos.EnumProperties.ConfidentialCategory? confidentialityCategory)
        {
            if (confidentialityCategory == null)
                return ConfidentialityType.Public;

            switch (confidentialityCategory)
            {
                case Dtos.EnumProperties.ConfidentialCategory.Public:
                    return ConfidentialityType.Public;
                case Dtos.EnumProperties.ConfidentialCategory.Private:
                    return ConfidentialityType.Private;
                default:
                    return ConfidentialityType.Public;
            }
        }

        #endregion

    }
}