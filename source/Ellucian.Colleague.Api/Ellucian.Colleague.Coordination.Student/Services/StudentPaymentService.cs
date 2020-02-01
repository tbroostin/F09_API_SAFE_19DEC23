// Copyright 2017-2019 Ellucian Company L.P. and its affiliates

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Linq;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Implements the IStudentPaymentService
    /// </summary>
    [RegisterType]
    public class StudentPaymentService : BaseCoordinationService, IStudentPaymentService
    {
        private IStudentPaymentRepository studentPaymentRepository;
        private IPersonRepository personRepository;
        private IReferenceDataRepository referenceDataRepository;
        private IStudentReferenceDataRepository studentReferenceDataRepository;
        private readonly ITermRepository termRepository;
        private readonly IConfigurationRepository configurationRepository;

        // Constructor to initialize the private attributes
        public StudentPaymentService(IStudentPaymentRepository studentPaymentRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            ITermRepository termRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository:configurationRepository)
        {
            this.studentPaymentRepository = studentPaymentRepository;
            this.personRepository = personRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.studentReferenceDataRepository = studentReferenceDataRepository;
            this.termRepository = termRepository;
            this.configurationRepository = configurationRepository;
        }

        #region Student Payments V6
        /// <summary>
        /// Returns the DTO for the specified student payments
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Dtos.StudentPayment> GetByIdAsync(string id)
        {
            CheckViewStudentPaymentsPermission();
            // Get the student payments domain entity from the repository
            var studentPaymentDomainEntity = await studentPaymentRepository.GetByIdAsync(id);

            if (studentPaymentDomainEntity == null)
            {
                throw new ArgumentNullException("StudentPaymentDomainEntity", "StudentPaymentDomainEntity cannot be null. ");
            }

            // Convert the student payment object into DTO.
            return await BuildStudentPaymentDtoAsync(studentPaymentDomainEntity);
        }
        /// <summary>
        /// Returns all student payments for the data model version 6
        /// </summary>
        /// <returns>Collection of StudentPayments</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentPayment>, int>> GetAsync(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string accountingCode = "", string paymentType = "")
        {
            CheckViewStudentPaymentsPermission();

            var studentPaymentDtos = new List<Dtos.StudentPayment>();
            string personId = "";
            string term = "";
            string arCode = "";
            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(student);
                    if (string.IsNullOrEmpty(personId))
                    {
                        return new Tuple<IEnumerable<Dtos.StudentPayment>, int>(studentPaymentDtos, 0);
                    }
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment>, int>(studentPaymentDtos, 0);
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                var termEntity = (await termRepository.GetAsync(bypassCache)).FirstOrDefault(t => t.RecordGuid == academicPeriod);
                if (termEntity == null || string.IsNullOrEmpty(termEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment>, int>(studentPaymentDtos, 0);
                }
                term = termEntity.Code;
            }
            if (!string.IsNullOrEmpty(accountingCode))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == accountingCode);
                if (arCodeEntity == null || string.IsNullOrEmpty(arCodeEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment>, int>(studentPaymentDtos, 0);
                }
                arCode = arCodeEntity.Code;
            }

            // Get the student payments domain entity from the repository
            var studentPaymentDomainTuple = await studentPaymentRepository.GetAsync(offset, limit, bypassCache, personId, term, arCode, paymentType);
            var studentPaymentDomainEntities = studentPaymentDomainTuple.Item1;
            var totalRecords = studentPaymentDomainTuple.Item2;

            if (studentPaymentDomainEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.StudentPayment>, int>(new List<Dtos.StudentPayment>(), 0);
            }

            // Convert the student payments and all its child objects into DTOs.
            foreach (var entity in studentPaymentDomainEntities)
            {
                if (entity != null)
                {
                    var paymentDto = await BuildStudentPaymentDtoAsync(entity, bypassCache);
                    studentPaymentDtos.Add(paymentDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentPayment>,int>(studentPaymentDtos, totalRecords);
        }

        /// <summary>
        /// Update a single student payments for the data model version 6
        /// </summary>
        /// <returns>A single StudentPayment</returns>
        public async Task<Dtos.StudentPayment> UpdateAsync(string id, Dtos.StudentPayment studentPayment)
        {
            CheckCreateStudentPaymentsPermission();

            var studentPaymentDto = new Dtos.StudentPayment();

            var studentPaymentEntity = await BuildStudentPaymentEntityAsync(studentPayment);
            var entity = await studentPaymentRepository.UpdateAsync(id, studentPaymentEntity);
            
            studentPaymentDto = await BuildStudentPaymentDtoAsync(entity);

            return studentPaymentDto;
        }

        /// <summary>
        /// Create a single student payments for the data model version 6
        /// </summary>
        /// <returns>A single StudentPayment</returns>
        public async Task<Dtos.StudentPayment> CreateAsync(Dtos.StudentPayment studentPayment)
        {
            CheckCreateStudentPaymentsPermission();

            studentPaymentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentPaymentDto = new Dtos.StudentPayment();

            var studentPaymentEntity = await BuildStudentPaymentEntityAsync(studentPayment);
            var entity = await studentPaymentRepository.CreateAsync(studentPaymentEntity);

            studentPaymentDto = await BuildStudentPaymentDtoAsync(entity);

            return studentPaymentDto;
        }

        /// <summary>
        /// Delete a single student payments for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payments GUID</param>
        /// <returns></returns>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide a student payments guid for deletion. ");
            }

            await studentPaymentRepository.DeleteAsync(id);
        }

        private async Task<Dtos.StudentPayment> BuildStudentPaymentDtoAsync(Ellucian.Colleague.Domain.Student.Entities.StudentPayment studentPaymentEntity, bool bypassCache = true)
        {
            var studentPaymentDto = new Dtos.StudentPayment();

            studentPaymentDto.Person = new GuidObject2((!string.IsNullOrEmpty(studentPaymentEntity.PersonId)) ?
                await personRepository.GetPersonGuidFromIdAsync(studentPaymentEntity.PersonId) :
                string.Empty);
            studentPaymentDto.Id = studentPaymentEntity.Guid;
            if (string.IsNullOrEmpty(studentPaymentDto.Id)) studentPaymentDto.Id = "00000000-0000-0000-0000-000000000000";
            if (!string.IsNullOrEmpty(studentPaymentEntity.Term))
            {
                var termEntity = (await termRepository.GetAsync()).FirstOrDefault(t => t.Code == studentPaymentEntity.Term);
                if (termEntity != null && !string.IsNullOrEmpty(termEntity.RecordGuid))
                {
                    studentPaymentDto.AcademicPeriod = new GuidObject2(termEntity.RecordGuid);
                }
            }
            if (!string.IsNullOrEmpty(studentPaymentEntity.AccountsReceivableCode))
            {
                var accountingCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(acc => acc.Code == studentPaymentEntity.AccountsReceivableCode);
                if (accountingCodeEntity != null)
                {
                    studentPaymentDto.AccountingCode = new GuidObject2(accountingCodeEntity.Guid);
                }
            }
            if (!string.IsNullOrEmpty(studentPaymentEntity.AccountsReceivableTypeCode))
            {
                var accountReceivalbeTypesEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Code == studentPaymentEntity.AccountsReceivableTypeCode);
                if (accountReceivalbeTypesEntity != null)
                {
                    studentPaymentDto.AccountReceivableType = new GuidObject2(accountReceivalbeTypesEntity.Guid);
                }
            }
            studentPaymentDto.PaidOn = studentPaymentEntity.PaymentDate;
            studentPaymentDto.PaymentType = !string.IsNullOrEmpty(studentPaymentEntity.PaymentType) ?
                ConvertPaymentTypes(studentPaymentEntity.PaymentType) :
                Dtos.EnumProperties.StudentPaymentTypes.notset;
            studentPaymentDto.Comments = studentPaymentEntity.Comments != null && studentPaymentEntity.Comments.Any() ?
                studentPaymentEntity.Comments :
                null;

            studentPaymentDto.Amount = new Dtos.DtoProperties.AmountDtoProperty()
            {
                Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentPaymentEntity.PaymentCurrency),
                Value = studentPaymentEntity.PaymentAmount
            };

            return studentPaymentDto;
        }

        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentPayment> BuildStudentPaymentEntityAsync(Dtos.StudentPayment studentPaymentDto, bool bypassCache = true)
        {
            if (studentPaymentDto.Person == null || string.IsNullOrEmpty(studentPaymentDto.Person.Id))
            {
                throw new ArgumentNullException("studentPayment.student.id", "The Student id cannot be null. ");
            }
            if (studentPaymentDto.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                throw new ArgumentNullException("studentPayment.paymentType", "The paymentType must be set and cannot be null. ");
            }

            var personId = await personRepository.GetPersonIdFromGuidAsync(studentPaymentDto.Person.Id);
            var paymentType = studentPaymentDto.PaymentType.ToString();
            var paymentDate = studentPaymentDto.PaidOn;
            string arCode = "";
            string arType = "";
            if (studentPaymentDto.AccountingCode != null && !string.IsNullOrEmpty(studentPaymentDto.AccountingCode.Id))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentPaymentDto.AccountingCode.Id);
                if (arCodeEntity != null)
                {
                    arCode = arCodeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountingCode id '{0}' is not valid. ", studentPaymentDto.AccountingCode.Id), "studentPayments.accountingCode.id");
                }
            }
            if (studentPaymentDto.AccountReceivableType != null && !string.IsNullOrEmpty(studentPaymentDto.AccountReceivableType.Id))
            { 
                var arTypeEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentPaymentDto.AccountReceivableType.Id);
                if (arTypeEntity != null)
                {
                    arType = arTypeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountReceivableType id '{0}' is not valid. ", studentPaymentDto.AccountReceivableType.Id), "studentPayments.accountReceivableType.id");
                }
            }
            var termEntity = (studentPaymentDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentPaymentDto.AcademicPeriod.Id)) ?
                (await termRepository.GetAsync(bypassCache)).FirstOrDefault(acc => acc.RecordGuid == studentPaymentDto.AcademicPeriod.Id) :
                null;
            if (termEntity == null)
            {
                if (studentPaymentDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentPaymentDto.AcademicPeriod.Id))
                {
                    throw new ArgumentException(string.Format("The Academic Period id {0} is invalid. ", studentPaymentDto.AcademicPeriod.Id), "studentPayment.academicPeriod.id");
                }
                throw new ArgumentException("The Academic Period is required for Colleague. ", "studentPayment.academicPeriod");
            }
            var term = termEntity.Code;

            var studentPaymentEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentPayment(personId, paymentType, paymentDate)
            {
                Guid = (studentPaymentDto.Id != null && !string.IsNullOrEmpty(studentPaymentDto.Id)) ? studentPaymentDto.Id : string.Empty,
                AccountsReceivableCode = arCode,
                AccountsReceivableTypeCode = arType,
                Comments = studentPaymentDto.Comments,
                Term = term,
                PaymentAmount = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Value : 0,
                PaymentCurrency = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Currency.ToString() : string.Empty
            };

            studentPaymentEntity.ChargeFromElevate = false;
            if (studentPaymentDto.MetadataObject != null && studentPaymentDto.MetadataObject.CreatedBy != null)
                studentPaymentEntity.ChargeFromElevate = studentPaymentDto.MetadataObject.CreatedBy == "Elevate" ? true : false;

            try
            {
                studentPaymentEntity.PaymentID = (studentPaymentDto.Id != null && !string.IsNullOrEmpty(studentPaymentDto.Id)) ? (await referenceDataRepository.GetGuidLookupResultFromGuidAsync(studentPaymentDto.Id)).PrimaryKey : string.Empty;
            }
            catch
            {
                // Do nothing if the GUID doesn't already exist, just leave the payment item id blank.
            }
            return studentPaymentEntity;
        }

        #endregion

        #region Student Payments V11
        /// <summary>
        /// Returns the DTO for the specified student payments
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Dtos.StudentPayment2> GetByIdAsync2(string id)
        {
            CheckViewStudentPaymentsPermission();
            // Get the student payments domain entity from the repository
            Ellucian.Colleague.Domain.Student.Entities.StudentPayment studentPaymentDomainEntity = null;

            try
            {
                studentPaymentDomainEntity = await studentPaymentRepository.GetByIdAsync(id);

            }
            catch (RepositoryException ex)
            {
                throw new KeyNotFoundException(string.Concat("No student payment was found for guid '", id, "'."));
            }


            if (studentPaymentDomainEntity == null)
            {
                throw new KeyNotFoundException(string.Concat("No student payment was found for guid '", id, "'."));
            }

            var ids = new List<string>() { studentPaymentDomainEntity.PersonId };
            var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(ids);

            // Convert the student payment object into DTO.
            StudentPayment2 studentPaymentDto = null;
            try
            {
                studentPaymentDto = await BuildStudentPaymentDtoAsync2(studentPaymentDomainEntity, personGuidCollection, true);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error", studentPaymentDomainEntity.Guid, studentPaymentDomainEntity.RecordKey);
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return studentPaymentDto;
        }


        /// <summary>
        /// Returns all student payments
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="student">Records from AR.PAY.ITEMS.INTG where ARP.INTG.PERSON.ID matches the student specified in the filter.</param>
        /// <param name="academicPeriod">Records from AR.PAY.ITEMS.INTG where ARP.INTG.TERM matches the academic period specified in the filter..</param>
        /// <param name="fundSource">Records from AR.PAY.ITEMS.INTG where ARP.INTG.DISTR.MTHD (guid)  matches the fundingSource specified in the filter.</param>
        /// <param name="paymentType">Records from AR.PAY.ITEMS.INTG where ARP.INTG.PAYMENT.TYPE matches the payment type specified in the filter</param>
        /// <param name="fundDestination">Records from AR.PAY.ITEMS.INTG where ARP.INTG.AR.TYPE (guid) matches the fundingDestination specified in the filter.</param>
        /// <param name="usage">Records from AR.PAY.ITEMS.INTG where Usage matches "taxReportingOnly".</param>
        /// <returns>Collection of StudentPayments</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentPayment2>, int>> GetAsync2(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string fundSource = "", string paymentType = "", string fundDestination = "", string usage = "")
        {
            CheckViewStudentPaymentsPermission();

            var studentPaymentDtos = new List<Dtos.StudentPayment2>();
            var personId = string.Empty;
            var term = string.Empty;
            var distrCode = string.Empty;
            var arType = string.Empty;

            #region filters
            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(student);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                if (string.IsNullOrEmpty(personId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }

            if (!string.IsNullOrEmpty(academicPeriod))
            {
                try
                {
                    term = await termRepository.GetAcademicPeriodsCodeFromGuidAsync(academicPeriod);
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                if (string.IsNullOrEmpty(term))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }

            if (!string.IsNullOrEmpty(fundSource))
            {
                try
                {
                    distrCode = await studentReferenceDataRepository.GetDistrMethodCodeFromGuidAsync(fundSource);
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                if (string.IsNullOrEmpty(distrCode))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }

            if (!string.IsNullOrEmpty(fundDestination))
            {
                try
                {
                    arType = await studentReferenceDataRepository.GetAccountReceivableTypesCodeFromGuidAsync(fundDestination);
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                if (string.IsNullOrEmpty(arType))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }


            if (!string.IsNullOrEmpty(usage))
            {
                try
                {
                    var enumChargType = (Dtos.EnumProperties.StudentPaymentUsageTypes)Enum.Parse(typeof(Dtos.EnumProperties.StudentPaymentUsageTypes), usage);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }
            #endregion

            #region get domain entity and supporting data
            // Get the student payments domain entity from the repository
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentDomainTuple = null;
            try
            {
                studentPaymentDomainTuple = await studentPaymentRepository.GetAsync2(offset, limit, bypassCache, personId, term, distrCode, paymentType, arType, usage);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            if (studentPaymentDomainTuple == null || studentPaymentDomainTuple.Item1 == null)
            {
                return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
            }

            var studentPaymentDomainEntities = studentPaymentDomainTuple.Item1;

            var ids = studentPaymentDomainEntities
                   .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                   .Select(x => x.PersonId).Distinct().ToList();

            var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(ids);
            #endregion

            #region  Convert the student payments and all its child objects into DTOs.
            foreach (var studentPaymentDomainEntity in studentPaymentDomainEntities)
            {
                try
                {
                    studentPaymentDtos.Add(await BuildStudentPaymentDtoAsync2(studentPaymentDomainEntity, personGuidCollection, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error", studentPaymentDomainEntity.Guid, studentPaymentDomainEntity.RecordKey);
                }
            }
          
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, studentPaymentDomainTuple.Item2);
        }


        /// <summary>
        /// Create a single student payments 
        /// </summary>
        /// <returns>A single StudentPayment</returns>
        public async Task<Dtos.StudentPayment2> CreateAsync2(Dtos.StudentPayment2 studentPayment)
        {
            CheckCreateStudentPaymentsPermission();

            Dtos.StudentPayment2 studentPaymentDto = null;
            studentPaymentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            #region create domain entity from request
            Domain.Student.Entities.StudentPayment studentPaymentEntityRequest = null;
            try
            {
                studentPaymentEntityRequest = await BuildStudentPaymentEntityAsync2(studentPayment);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created.  Error extracting request. " + ex.Message, "Global.Internal.Error", 
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.Guid)  ? studentPaymentEntityRequest.Guid : null,
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.RecordKey) ? studentPaymentEntityRequest.RecordKey : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            #region Create record from domain entity
            Domain.Student.Entities.StudentPayment studentPaymentEntityResponse = null;
            try
            {
                studentPaymentEntityResponse = await studentPaymentRepository.CreateAsync2(studentPaymentEntityRequest);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.Guid) ? studentPaymentEntityRequest.Guid : null,
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.RecordKey) ? studentPaymentEntityRequest.RecordKey : null);
                throw IntegrationApiException;
            }

            if (studentPaymentEntityResponse == null)
            {
                IntegrationApiExceptionAddError("Possible error creating record.  No additional details available.   Please check to see if record was created.", "Global.Internal.Error",
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.Guid) ? studentPaymentEntityRequest.Guid : null,
                    studentPaymentEntityRequest != null && !string.IsNullOrEmpty(studentPaymentEntityRequest.RecordKey) ? studentPaymentEntityRequest.RecordKey : null);
                throw IntegrationApiException;
            }
            #endregion

            #region Build DTO response
            var ids = new List<string>() { studentPaymentEntityResponse.PersonId };
            var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(ids);

            try
            {
                studentPaymentDto = await BuildStudentPaymentDtoAsync2(studentPaymentEntityResponse, personGuidCollection, true);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record created. Error building response. " + ex.Message, "Global.Internal.Error", studentPaymentEntityResponse.Guid, studentPaymentEntityResponse.RecordKey);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion

            return studentPaymentDto;
        }

        /// <summary>
        /// BuildStudentPaymentDtoAsync2
        /// </summary>
        /// <param name="studentPaymentEntity"></param>
        /// <param name="personGuidCollection"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Dtos.StudentPayment2> BuildStudentPaymentDtoAsync2(Ellucian.Colleague.Domain.Student.Entities.StudentPayment studentPaymentEntity,
            Dictionary<string, string> personGuidCollection,  bool bypassCache = true)
        {

            if (studentPaymentEntity == null)
            {
                IntegrationApiExceptionAddError("StudentPayment domain entity is required.", "Missing.Request.Body");
                return null;
            }
            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError("An error occurred extracting person guids.", "GUID.Not.Found", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
                return null;
            }

            var studentPaymentDto = new Dtos.StudentPayment2();
            studentPaymentDto.Id = string.IsNullOrEmpty(studentPaymentEntity.Guid) ? Guid.Empty.ToString() : studentPaymentEntity.Guid;

            if (string.IsNullOrEmpty(studentPaymentEntity.PersonId))
            {

                IntegrationApiExceptionAddError("Missing student record. 'ARP.INTG.PERSON.ID'", "Bad.Data", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
            }
            else
            {
                var personGuid = string.Empty;
                personGuidCollection.TryGetValue(studentPaymentEntity.PersonId, out personGuid);

                if (string.IsNullOrEmpty(personGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Person guid not found, PersonId: '{0}'", studentPaymentEntity.PersonId), "GUID.Not.Found", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
                }
                else
                {
                    studentPaymentDto.Person = new GuidObject2(personGuid);
                }
            }
            
            if (!string.IsNullOrEmpty(studentPaymentEntity.Term))
            {
                var acadPeriodGuid = string.Empty;
                try
                {
                    acadPeriodGuid = await termRepository.GetAcademicPeriodsGuidAsync(studentPaymentEntity.Term);
                }
                catch(Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
                }               
                if (!string.IsNullOrEmpty(acadPeriodGuid))
                {
                    studentPaymentDto.AcademicPeriod = new GuidObject2(acadPeriodGuid);
                }
            }

            if (!string.IsNullOrEmpty(studentPaymentEntity.DistributionCode))
            {
                var distrMethodGuid = string.Empty;
                try
                {
                    distrMethodGuid = await studentReferenceDataRepository.GetDistrMethodGuidAsync(studentPaymentEntity.DistributionCode);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError("Could not resolve an issue with fundingSource.Entity: 'CASH.RCPTS'", "GUID.Not.Found", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
                }
                if (!string.IsNullOrEmpty(distrMethodGuid))
                {
                    studentPaymentDto.FundingSource = new GuidObject2(distrMethodGuid);
                }
            }

            if (!string.IsNullOrEmpty(studentPaymentEntity.AccountsReceivableTypeCode))
            {

                var accountsReceivableTypeGuid = string.Empty;
                try
                {
                    accountsReceivableTypeGuid = await studentReferenceDataRepository.GetAccountReceivableTypesGuidAsync(studentPaymentEntity.AccountsReceivableTypeCode);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError("Could not resolve an issue with fundingDestination. Entity: 'CASH.RCPTS'", "GUID.Not.Found", studentPaymentEntity.Guid, studentPaymentEntity.RecordKey);
                }
                if (!string.IsNullOrEmpty(accountsReceivableTypeGuid))
                {
                    studentPaymentDto.FundingDestination = new GuidObject2(accountsReceivableTypeGuid);
                }
            }


            studentPaymentDto.PaidOn = studentPaymentEntity.PaymentDate;
            studentPaymentDto.PaymentType = !string.IsNullOrEmpty(studentPaymentEntity.PaymentType) ?
                ConvertPaymentTypes(studentPaymentEntity.PaymentType) :
                Dtos.EnumProperties.StudentPaymentTypes.notset;
            studentPaymentDto.Comments = studentPaymentEntity.Comments != null && studentPaymentEntity.Comments.Any() ?
                studentPaymentEntity.Comments :
                null;

            if (!string.IsNullOrEmpty(studentPaymentEntity.PaymentCurrency))
            {
                studentPaymentDto.Amount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentPaymentEntity.PaymentCurrency),
                    Value = studentPaymentEntity.PaymentAmount
                };
            }

            if (!string.IsNullOrEmpty(studentPaymentEntity.Usage))
            {
                var reportingDetail = new Dtos.DtoProperties.StudentPaymentsReportingDtoProperty
                {
                    Usage = Dtos.EnumProperties.StudentPaymentUsageTypes.taxReportingOnly
                };
                if (studentPaymentEntity.OriginatedOn != null && studentPaymentEntity.OriginatedOn.HasValue)
                {
                    reportingDetail.OriginatedOn = studentPaymentEntity.OriginatedOn;
                }
                else if (studentPaymentEntity.PaymentDate != null && studentPaymentEntity.PaymentDate != DateTime.MinValue)
                {
                    reportingDetail.OriginatedOn = studentPaymentEntity.PaymentDate.Date;
                }
                studentPaymentDto.ReportingDetail = reportingDetail;
            }

            if (!string.IsNullOrEmpty(studentPaymentEntity.OverrideDescription))
            {
                studentPaymentDto.OverrideDescription = studentPaymentEntity.OverrideDescription;
            }

            return studentPaymentDto;
        }


        /// <summary>
        /// BuildStudentPaymentEntityAsync2
        /// </summary>
        /// <param name="studentPaymentDto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentPayment> BuildStudentPaymentEntityAsync2(Dtos.StudentPayment2 studentPaymentDto, bool bypassCache = true)
        {
            if (studentPaymentDto == null)
            {
                IntegrationApiExceptionAddError("StudentPayment body is required.", "Missing.Request.Body");
                return null;
            }


            if (studentPaymentDto.Person == null || string.IsNullOrEmpty(studentPaymentDto.Person.Id))
            {
                //throw new ArgumentNullException("studentPayment.student.id", "The Student id cannot be null. ");
                IntegrationApiExceptionAddError("student.id is required.", "Missing.Required.Property", studentPaymentDto.Id);
            }
            if (studentPaymentDto.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                //throw new ArgumentNullException("studentPayment.paymentType", "The paymentType must be set and cannot be null. ");
                IntegrationApiExceptionAddError("studentPayment.paymentType is required.", "Missing.Required.Property", studentPaymentDto.Id);
            }
            if (studentPaymentDto.ReportingDetail != null)
            {
                if (studentPaymentDto.ReportingDetail.Usage != null && studentPaymentDto.ReportingDetail.Usage != Dtos.EnumProperties.StudentPaymentUsageTypes.taxReportingOnly)
                {
                     IntegrationApiExceptionAddError(string.Format("The usage attribute of '{0}' is not permitted when submitting usage associated with this charge.", studentPaymentDto.ReportingDetail.Usage.ToString()), "Validation.Exception", studentPaymentDto.Id);
                    //throw new ArgumentException(string.Format("The usage attribute of '{0}' is not permitted when submitting usage associated with this charge.", studentPaymentDto.ReportingDetail.Usage.ToString()));
                }
                else
                {
                    if (studentPaymentDto.ReportingDetail.Usage == null)
                    {
                        if (studentPaymentDto.ReportingDetail.OriginatedOn != null && studentPaymentDto.ReportingDetail.OriginatedOn.HasValue)
                        {
                            IntegrationApiExceptionAddError("The originatedOn is not permitted without usage set to 'taxReportingOnly'.", "Validation.Exception", studentPaymentDto.Id);
                            //throw new ArgumentException("The originatedOn is not permitted without usage set to 'taxReportingOnly'.");
                        }
                    }
                    else
                    {
                        if (studentPaymentDto.ReportingDetail.OriginatedOn == null || !studentPaymentDto.ReportingDetail.OriginatedOn.HasValue)
                        {
                            IntegrationApiExceptionAddError("The usage set to 'taxReportingOnly' is not permitted without originatedOn .", "Validation.Exception", studentPaymentDto.Id);
                            //throw new ArgumentException("The usage set to 'taxReportingOnly' is not permitted without originatedOn .");
                        }
                        else
                        {
                            // The spec doesn't specify any validation on date against other dates though student-charges does.
                            // Commenting out this code for now.  SRM - 11/6/2019
                            //if (studentPaymentDto.ReportingDetail.OriginatedOn.Value.Date > studentPaymentDto.PaidOn.Value.Date)
                            //{
                            //    //IntegrationApiExceptionAddError("The originatedOn date must be on or before the chargeableOn date.", "Validation.Exception", studentPaymentDto.Id);
                            //    throw new ArgumentException("The originatedOn date must be on or before the paidOn date.");
                            //}
                            //if (studentPaymentDto.ReportingDetail.OriginatedOn.Value.Date > DateTime.Now.Date)
                            //{
                            //    //IntegrationApiExceptionAddError("The originatedOn date may only be set to a date on or before the current date.", "Validation.Exception", studentPaymentDto.Id);
                            //    throw new ArgumentException("The originatedOn date may only be set to a date on or before the current date.");
                            //}
                        }
                    }
                }
            }

            var personId = string.Empty;

            if (studentPaymentDto.Person != null && !string.IsNullOrEmpty(studentPaymentDto.Person.Id))
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(studentPaymentDto.Person.Id);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Concat("Error retrieving person id from guid '", studentPaymentDto.Person.Id, "'."), "Validation.Exception", studentPaymentDto.Id);
                }

                if (string.IsNullOrEmpty(personId))
                {
                    IntegrationApiExceptionAddError(string.Concat("Error retrieving person id from guid '", studentPaymentDto.Person.Id, "'."), "Validation.Exception", studentPaymentDto.Id);
                }
                       
            }

            var paymentType = studentPaymentDto.PaymentType.ToString();
            var paymentDate = studentPaymentDto.PaidOn;
            var distrMethod = string.Empty;
            var arType = string.Empty;
            var term = string.Empty;
            if (studentPaymentDto.FundingSource != null && !string.IsNullOrEmpty(studentPaymentDto.FundingSource.Id))
            {
                try
                {
                    distrMethod = await studentReferenceDataRepository.GetDistrMethodCodeFromGuidAsync(studentPaymentDto.FundingSource.Id);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", studentPaymentDto.Id);
                }
            }


            if (studentPaymentDto.FundingDestination != null && !string.IsNullOrEmpty(studentPaymentDto.FundingDestination.Id))
            {             
                try
                {
                    arType = await studentReferenceDataRepository.GetAccountReceivableTypesCodeFromGuidAsync(studentPaymentDto.FundingDestination.Id);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", studentPaymentDto.Id);
                }
            }

            if ((studentPaymentDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentPaymentDto.AcademicPeriod.Id)))
            {
                try
                {
                    term = await termRepository.GetAcademicPeriodsCodeFromGuidAsync(studentPaymentDto.AcademicPeriod.Id);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", studentPaymentDto.Id);
                }
            }

            var chargeFromElevate = false;
            if (studentPaymentDto.MetadataObject != null && studentPaymentDto.MetadataObject.CreatedBy != null)
                chargeFromElevate = studentPaymentDto.MetadataObject.CreatedBy == "Elevate" ? true : false;

            var paymentID = string.Empty;
            try
            {
                paymentID = (studentPaymentDto.Id != null && !string.IsNullOrEmpty(studentPaymentDto.Id)) ?
                    (await referenceDataRepository.GetGuidLookupResultFromGuidAsync(studentPaymentDto.Id)).PrimaryKey : string.Empty;
            }
            catch
            {
                // Do nothing if the GUID doesn't already exist, just leave the payment item id blank.
            }

            Ellucian.Colleague.Domain.Student.Entities.StudentPayment studentPaymentEntity = null;
            try
            {
                studentPaymentEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentPayment(personId, paymentType, paymentDate)
                {
                    Guid = (studentPaymentDto.Id != null && !string.IsNullOrEmpty(studentPaymentDto.Id)) ? studentPaymentDto.Id : string.Empty,
                    DistributionCode = distrMethod,
                    AccountsReceivableTypeCode = arType,
                    Comments = studentPaymentDto.Comments,
                    Term = term,
                    PaymentAmount = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Value : 0,
                    PaymentCurrency = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Currency.ToString() : string.Empty,
                    Usage = (studentPaymentDto.ReportingDetail != null && studentPaymentDto.ReportingDetail.Usage != null && studentPaymentDto.ReportingDetail.Usage != Dtos.EnumProperties.StudentPaymentUsageTypes.notset) ? studentPaymentDto.ReportingDetail.Usage.ToString() : string.Empty,
                    OriginatedOn = (studentPaymentDto.ReportingDetail != null && studentPaymentDto.ReportingDetail.OriginatedOn != null && studentPaymentDto.ReportingDetail.OriginatedOn.HasValue) ? studentPaymentDto.ReportingDetail.OriginatedOn.Value : new DateTime?(),
                    OverrideDescription = studentPaymentDto.OverrideDescription,
                    ChargeFromElevate = chargeFromElevate,
                    PaymentID = paymentID
                };
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Error creating StudentPayment entity.  Student, Payment Type and Paid on date are all required. ", "Validation.Exception", studentPaymentDto.Id);
               
            }

            return studentPaymentEntity;
        }

        #endregion

        #region Student Payments shared methods
        /// <summary>
        /// Convert paymnet type string into StudentPaymentTypes enumeration.
        /// </summary>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        private Dtos.EnumProperties.StudentPaymentTypes ConvertPaymentTypes(string paymentType)
        {
            switch (paymentType.ToLowerInvariant())
            {
                case "financialAid":
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.financialAid;
                    }
                case "deposit":
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.deposit;
                    }
                case "sponsor":
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                    }
                case "payroll":
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.payroll;
                    }
                case "cash":
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.cash;
                    }
                default:
                    {
                        return Dtos.EnumProperties.StudentPaymentTypes.notset;
                    }
            }
        }
       
        /// <summary>
        /// Helper method to determine if the user has permission to view Student Payments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentPaymentsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentPayments) || HasPermission(StudentPermissionCodes.CreateStudentPayments);

            // User is not allowed to create or update Student payments without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view Student Payments.", CurrentUser.UserId));
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Payments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateStudentPaymentsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.CreateStudentPayments);

            // User is not allowed to create or update Student payments without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to create Student Payments.", CurrentUser.UserId));
            }
        }

        #endregion
    }
}
