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

namespace Ellucian.Colleague.Coordination.Student.Base
{
    [RegisterType]
    public class CommentSubjectAreaService : ICommentSubjectAreaService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public CommentSubjectAreaService(IStudentReferenceDataRepository studentReferenceDataRepository, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
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

            var applicantRemarkTypeEntities = await _studentReferenceDataRepository.GetApplicantRemarkTypesAsync(ignoreCache);
            if (applicantRemarkTypeEntities != null && applicantRemarkTypeEntities.Any())
            {
                foreach (var applicantRemarkType in applicantRemarkTypeEntities)
                {
                    commentSubjectAreaCollection.Add(ConvertAppRemarkTypesEntityToCommentSubjectAreaDto(applicantRemarkType));
                }
            }
           
            var studentRemarkTypeEntities = await _studentReferenceDataRepository.GetStudentRemarkTypesAsync(ignoreCache);
            if (studentRemarkTypeEntities != null && studentRemarkTypeEntities.Any())
            {
                foreach (var studentRemarkType in studentRemarkTypeEntities)
                {
                    commentSubjectAreaCollection.Add(ConvertStudentRemarkTypesEntityToCommentSubjectAreaDto(studentRemarkType));
                }
            }

            var facultyRemarkTypeEntities = await _studentReferenceDataRepository.GetFacultyRemarkTypesAsync(ignoreCache);
            if (facultyRemarkTypeEntities != null && facultyRemarkTypeEntities.Any())
            {
                foreach (var facultyRemarkType in facultyRemarkTypeEntities)
                {
                    commentSubjectAreaCollection.Add(ConvertFacultyRemarkTypesEntityToCommentSubjectAreaDto(facultyRemarkType));
                }
            }

            return commentSubjectAreaCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
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
                var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();

                var remarkType = (await _referenceDataRepository.GetRemarkTypesAsync(true)).FirstOrDefault(gs => gs.Guid == guid);
                if (remarkType != null)
                    return ConvertRemarkTypesEntityToCommentSubjectAreaDto(remarkType);

                var applicantRemarkType = (await _studentReferenceDataRepository.GetApplicantRemarkTypesAsync(true)).FirstOrDefault(gs => gs.Guid == guid);
                if (applicantRemarkType != null)
                    return ConvertAppRemarkTypesEntityToCommentSubjectAreaDto(applicantRemarkType);

                var studentRemarkType = (await _studentReferenceDataRepository.GetStudentRemarkTypesAsync(true)).FirstOrDefault(gs => gs.Guid == guid);
                if (studentRemarkType != null)
                    return ConvertStudentRemarkTypesEntityToCommentSubjectAreaDto(studentRemarkType);

                var facultyRemarkType = (await _studentReferenceDataRepository.GetFacultyRemarkTypesAsync(true)).FirstOrDefault(gs => gs.Guid == guid);
                if (facultyRemarkType != null)
                    return ConvertFacultyRemarkTypesEntityToCommentSubjectAreaDto(facultyRemarkType);

                return commentSubjectArea;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Comment Subject Area not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Comment Subject Area not found for GUID " + guid, ex);
            }
        }
  
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a RemarkTypes domain entity to its corresponding CommentSubjectArea DTO
        /// </summary>
        /// <param name="source">RemarkType domain entity</param>
        /// <returns>CommentSubjectArea DTO</returns>
        private Ellucian.Colleague.Dtos.CommentSubjectArea ConvertRemarkTypesEntityToCommentSubjectAreaDto(Ellucian.Colleague.Domain.Base.Entities.RemarkType source)
        {
            var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();
            if (source != null)
            {
                commentSubjectArea.Id = source.Guid;
                commentSubjectArea.Code = source.Code;
                commentSubjectArea.Title = source.Description;
                commentSubjectArea.Description = null;
            }
            return commentSubjectArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an applicant remark types domain entity to its corresponding CommentSubjectArea DTO
        /// </summary>
        /// <param name="source">ApplicantRemarkType domain entity</param>
        /// <returns>CommentSubjectArea DTO</returns>
        private Ellucian.Colleague.Dtos.CommentSubjectArea ConvertAppRemarkTypesEntityToCommentSubjectAreaDto(Ellucian.Colleague.Domain.Student.Entities.ApplicantRemarkType source)
        {
            var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();
            if (source != null)
            {
                commentSubjectArea.Id = source.Guid;
                commentSubjectArea.Code = source.Code;
                commentSubjectArea.Title = source.Description;
                commentSubjectArea.Description = null;
            }
            return commentSubjectArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a StudentRemarkTypes domain entity to its corresponding CommentSubjectArea DTO
        /// </summary>
        /// <param name="source">StudentRemarkType domain entity</param>
        /// <returns>CommentSubjectArea DTO</returns>
        private Ellucian.Colleague.Dtos.CommentSubjectArea ConvertStudentRemarkTypesEntityToCommentSubjectAreaDto(Ellucian.Colleague.Domain.Student.Entities.StudentRemarkType source)
        {
            var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();
            if (source != null)
            {
                commentSubjectArea.Id = source.Guid;
                commentSubjectArea.Code = source.Code;
                commentSubjectArea.Title = source.Description;
                commentSubjectArea.Description = null;
            }
            return commentSubjectArea;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a FacultyRemarkType domain entity to its corresponding CommentSubjectArea DTO
        /// </summary>
        /// <param name="source">FacultyRemarkType domain entity</param>
        /// <returns>CommentSubjectArea DTO</returns>
        private Ellucian.Colleague.Dtos.CommentSubjectArea ConvertFacultyRemarkTypesEntityToCommentSubjectAreaDto(Ellucian.Colleague.Domain.Student.Entities.FacultyRemarkType source)
        {
            var commentSubjectArea = new Ellucian.Colleague.Dtos.CommentSubjectArea();
            if (source != null)
            {
                commentSubjectArea.Id = source.Guid;
                commentSubjectArea.Code = source.Code;
                commentSubjectArea.Title = source.Description;
                commentSubjectArea.Description = null;
            }
            return commentSubjectArea;
        }
    }
}