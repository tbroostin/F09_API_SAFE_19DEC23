// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all comment subject areas
        /// </summary>
        /// <returns>Collection of Comments DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Comments>, int>> GetCommentsAsync(int offset, int limit, string subjectMatter, string commentSubjectArea,bool bypassCache = false)
        {
            CheckUserCommentsViewPermissions();

            var commentsCollection = new List<Ellucian.Colleague.Dtos.Comments>();

            // Convert and validate all input parameters
            var newSubjectMatter = string.Empty;
            if (!string.IsNullOrEmpty(subjectMatter))
            {
                try
                {
                    newSubjectMatter = await _personRepository.GetPersonIdFromGuidAsync(subjectMatter);
                    if (string.IsNullOrEmpty(newSubjectMatter))
                    {
                        throw new ArgumentException(string.Concat("GUID not found for subjectMatter: ", subjectMatter));
                    }
                }
                catch (KeyNotFoundException e)
                {
                    return new Tuple<IEnumerable<Dtos.Comments>, int>(new List<Dtos.Comments>(), 0);
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
                        throw new ArgumentException(string.Concat("GUID not found for commentSubjectArea: ", commentSubjectArea));
                    }
                }
                catch (ArgumentException e)
                {
                    return new Tuple<IEnumerable<Dtos.Comments>, int>(new List<Dtos.Comments>(), 0);
                }
            }
            
            var remarksEntities = await _remarkRepository.GetRemarksAsync(offset, limit, newSubjectMatter, newCommentSubjectArea);
            var totalRecords = remarksEntities.Item2;

            foreach (var remarkEntity in remarksEntities.Item1)
            {
                if (remarkEntity.Guid != null)
                {
                    var remarkDto = await ConvertRemarkEntityToCommentsDtoAsync(remarkEntity, bypassCache);
                    commentsCollection.Add(remarkDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.Comments>, int>(commentsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a comment from its GUID
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        /// <returns>Comments DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Comments> GetCommentByIdAsync(string guid)
        {
            CheckUserCommentsViewPermissions();

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a Comments.");
            }

            try
            {
                return await ConvertRemarkEntityToCommentsDtoAsync(await _remarkRepository.GetRemarkByGuidAsync(guid));
            }
            catch (RepositoryException ex)
            {
                throw new KeyNotFoundException("Comment not found for GUID " + guid, ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Comment not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Comment not found for GUID " + guid, ex);
            }
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

            if (comments.Id == null)
            {
                throw new ArgumentNullException("comment", "Comments id required.");
            }

            try
            {
                CheckUserCommentsCreateUpdatePermissions();

                _remarkRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                var entity = await this.ConvertCommentsDtoToRemarkEntityAsync(comments);
                var newEntity = await _remarkRepository.UpdateRemarkAsync(entity);
                var newDto = await ConvertRemarkEntityToCommentsDtoAsync(newEntity);

                return newDto;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete Remark by ID
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        public async Task DeleteCommentByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to delete a Comment.");
            }

            try
            {
                CheckUserCommentsDeletePermissions();

                 await _remarkRepository.DeleteRemarkAsync(guid);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
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

            try
            {
                CheckUserCommentsCreateUpdatePermissions();

                _remarkRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                var entity = await this.ConvertCommentsDtoToRemarkEntityAsync(comments);
                var newEntity = await _remarkRepository.UpdateRemarkAsync(entity);
                var newDto = await ConvertRemarkEntityToCommentsDtoAsync(newEntity);

                return newDto;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Comments", ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Convert remarks entity to a comments DTO
        /// </summary>
        /// <param name="source">Remark domain entity</param>
        /// <returns>Comments DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Comments> ConvertRemarkEntityToCommentsDtoAsync(Ellucian.Colleague.Domain.Base.Entities.Remark source, bool bypassCache = true)
        {
            var comments = new Ellucian.Colleague.Dtos.Comments();

            if (source == null)
            {
                throw new ArgumentNullException("remarks", "Remarks entity must be provided.");
            }

            comments.Id = source.Guid;
            comments.Confidentiality = ConvertConfidentialityTypeEnumToConfidentialityCategoryEnum(source.RemarksPrivateType);
            if (!string.IsNullOrEmpty(source.RemarksText))
             comments.Comment =  source.RemarksText.Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                   .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                   .Replace(Convert.ToChar(DynamicArray.SM), ' ');
            comments.EnteredOn = source.RemarksDate;
            
            if (!string.IsNullOrEmpty(source.RemarksDonorId))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.RemarksDonorId);
                if (!string.IsNullOrEmpty(personGuid))
                {
                    comments.SubjectMatter = new Dtos.DtoProperties.SubjectMatterDtoProperty()
                    {
                        Person = new Dtos.GuidObject2(personGuid)
                    };
                }
            }

            if (!string.IsNullOrEmpty(source.RemarksIntgEnteredBy))
            {
                comments.EnteredBy = new Dtos.DtoProperties.EnteredByDtoProperty() { Name = source.RemarksIntgEnteredBy }; 
               
            }
            else if (!string.IsNullOrEmpty(source.RemarksAuthor))
            {
                var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.RemarksAuthor);
                if (!string.IsNullOrEmpty(personGuid))
                {
                    comments.EnteredBy = new Dtos.DtoProperties.EnteredByDtoProperty() { Id = personGuid }; 
                }
            }

            if (source.RemarksType != null)
            {
                var remarkType = (await _referenceDataRepository.GetRemarkTypesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.RemarksType);
                if (remarkType != null)
                {
                    comments.CommentSubjectArea = new Dtos.GuidObject2(remarkType.Guid);
                }
            }

            if (source.RemarksCode != null)
            {
                var remarkCode = (await _referenceDataRepository.GetRemarkCodesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.RemarksCode);
                if (remarkCode != null)
                {
                    comments.Source = new Dtos.GuidObject2(remarkCode.Guid);
                }
            }

            return comments;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Convert a comments dto to a remark entity
        /// </summary>
        /// <param name="source">Comments DTO</param>
        /// <returns>Remark entity</returns>
        private async Task<Ellucian.Colleague.Domain.Base.Entities.Remark> ConvertCommentsDtoToRemarkEntityAsync(Ellucian.Colleague.Dtos.Comments source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("comments", "Comments DTO must be provided.");
            }

            if (source.Id == null)
            {
                throw new ArgumentNullException("comments", "Comments Id must be provided.");
            }

            if ((source.Comment == null) || (string.IsNullOrEmpty(source.Comment)))
            {
                throw new ArgumentNullException("Comments text required.");
            }

            if (source.SubjectMatter == null)
            {
                throw new ArgumentNullException("Subject Matter is required.");
            }

            if  ( (source.SubjectMatter.Person != null) && (source.SubjectMatter.Person.Id == null))
            {
                throw new ArgumentNullException("Subject Matter ID is required.");
            }

            if ( (source.CommentSubjectArea != null) && (source.CommentSubjectArea.Id == null))
            {
                throw new ArgumentNullException("CommentSubjectArea ID is required.");
            }

            if ((source.Source != null) && (source.Source.Id == null))
            {
                throw new ArgumentNullException("Source ID is required.");
            }

            var comments = new Remark(source.Id); 
            comments.RemarksDate = source.EnteredOn;
            comments.RemarksText = source.Comment;

            if (source.Source != null)
            {
                var remarksCode = ConvertGuidToCode(await _referenceDataRepository.GetRemarkCodesAsync(true), source.Source.Id);
                if (string.IsNullOrEmpty(remarksCode))
                {
                    throw new ArgumentException( string.Concat("The source specified is not intended for use with comments or source not found for ID: ",source.Source.Id));
                }
                comments.RemarksCode = remarksCode;
            }

            if (source.CommentSubjectArea != null)
            {
                var remarksType = ConvertGuidToCode(await _referenceDataRepository.GetRemarkTypesAsync(true), source.CommentSubjectArea.Id);
                if (string.IsNullOrEmpty(remarksType))
                {
                    throw new ArgumentException( string.Concat("Comment Subject Area not found for ID: ",source.CommentSubjectArea.Id) );
                }
                comments.RemarksType = remarksType;
            }

            comments.RemarksPrivateType = ConvertConfidentialityCategoryEnumToConfidentialityTypeEnum(source.Confidentiality);

            if (source.EnteredBy != null)
            {
                if (source.EnteredBy.Name != null)
                {
                    comments.RemarksIntgEnteredBy = source.EnteredBy.Name;
                }
                else if (string.IsNullOrEmpty(source.EnteredBy.Id))
                {
                    throw new KeyNotFoundException("Source.EnteredBy.Id is required");
                }
                else
                {
                    var id = await _personRepository.GetPersonIdFromGuidAsync(source.EnteredBy.Id);
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new KeyNotFoundException(string.Concat("Required field EnteredBy.ID cannot find a matching GUID, EnterBy.Id is ", source.EnteredBy.Id));
                    }
                    comments.RemarksAuthor = id;
                }
            }

            if ((source.SubjectMatter != null) && (source.SubjectMatter.Person != null))
            {
                if (string.IsNullOrEmpty(source.SubjectMatter.Person.Id))
                {
                    throw new ArgumentException("SubjectMatter.Person.Id is required");
                }
                var id = await _personRepository.GetPersonIdFromGuidAsync(source.SubjectMatter.Person.Id);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Person not found for ID: ", source.SubjectMatter.Person.Id));
                }
                comments.RemarksDonorId = id;
            }
            return comments;
        }



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

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserCommentsViewPermissions()
        {
            // access is ok if the current user has the view comments permission
            if (!HasPermission(BasePermissionCodes.ViewComment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view comments.");
                throw new PermissionsException("User is not authorized to view comments.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserCommentsCreateUpdatePermissions()
        {
            // access is ok if the current user has the create/update comments permission
            if (!HasPermission(BasePermissionCodes.UpdateComment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create/update comments.");
                throw new PermissionsException("User is not authorized to create/update comments.");
            }
        }

        /// <summary>
        /// Provides an integration user permission to delete a hold (a.k.a. a record from STUDENT.RESTRICTIONS) in Colleague.
        /// </summary>
        private void CheckUserCommentsDeletePermissions()
        {
            // access is ok if the current user has the delete comments permission
            if (!HasPermission(BasePermissionCodes.DeleteComment))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to delete comments.");
                throw new PermissionsException("User is not authorized to delete comments.");
            }
        }
    }
}