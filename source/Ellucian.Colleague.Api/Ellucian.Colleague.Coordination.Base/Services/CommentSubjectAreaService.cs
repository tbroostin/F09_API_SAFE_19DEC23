// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;


namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CommentSubjectAreaService : ICommentSubjectAreaService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public CommentSubjectAreaService(IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all comment subject areas
        /// </summary>
        /// <returns>Collection of CommentSubjectArea DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommentSubjectArea>> GetCommentSubjectAreaAsync(bool ignoreCache)
        {
            var commentSubjectAreaCollection = new List<Ellucian.Colleague.Dtos.CommentSubjectArea>();

            var remarkTypeEntities = await _referenceDataRepository.GetRemarkTypesAsync(ignoreCache);
            if (remarkTypeEntities != null && remarkTypeEntities.Any())
            {
                foreach (var remarkType in remarkTypeEntities)
                {
                    commentSubjectAreaCollection.Add(ConvertRemarkTypesEntityToCommentSubjectAreaDto(remarkType));
                }
            }
            return commentSubjectAreaCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a comment subject area from its GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>CommentSubjectArea DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CommentSubjectArea> GetCommentSubjectAreaByIdAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a CommentSubjectArea.");
            }

            try
            {
                return ConvertRemarkTypesEntityToCommentSubjectAreaDto((await _referenceDataRepository.GetRemarkTypesAsync(true)).First(gs => gs.Guid == guid));       
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Comment Subject Area not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Comment Subject Area not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a RemarkTypes domain entity to its corresponding CommentSubjectArea DTO
        /// </summary>
        /// <param name="source">RemarkType domain entity</param>
        /// <returns>CommentSubjectArea DTO</returns>
        private Ellucian.Colleague.Dtos.CommentSubjectArea ConvertRemarkTypesEntityToCommentSubjectAreaDto(Ellucian.Colleague.Domain.Base.Entities.RemarkType source)
        {
            var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();

            commentSubjectArea.Id = source.Guid;
            commentSubjectArea.Code = source.Code;
            commentSubjectArea.Title = source.Description;
            commentSubjectArea.Description = null;

            return commentSubjectArea;
        }
    }
}