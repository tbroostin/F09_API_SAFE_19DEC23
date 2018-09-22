// Copyright 2017 Ellucian Company L.P. and its affiliates

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
            var studentPaymentDomainEntity = await studentPaymentRepository.GetByIdAsync(id);

            if (studentPaymentDomainEntity == null)
            {
                throw new KeyNotFoundException("Student payment could not be found. ");
            }

            // Convert the student payment object into DTO.
            return await BuildStudentPaymentDtoAsync2(studentPaymentDomainEntity);
        }
        /// <summary>
        /// Returns all student payments for the data model version 6
        /// </summary>
        /// <returns>Collection of StudentPayments</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentPayment2>, int>> GetAsync2(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string fundSource = "", string paymentType = "", string fundDestination = "")
        {
            CheckViewStudentPaymentsPermission();

            var studentPaymentDtos = new List<Dtos.StudentPayment2>();
            string personId = "";
            string term = "";
            string distrCode = "";
            string arType = "";
            if (!string.IsNullOrEmpty(student))
            {
                personId = await personRepository.GetPersonIdFromGuidAsync(student);
                if (string.IsNullOrEmpty(personId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                var termEntity = (await termRepository.GetAsync(bypassCache)).FirstOrDefault(t => t.RecordGuid == academicPeriod);
                if (termEntity == null || string.IsNullOrEmpty(termEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                term = termEntity.Code;
            }
            if (!string.IsNullOrEmpty(fundSource))
            {
                var distrCodeEntity = (await studentReferenceDataRepository.GetDistrMethodCodesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == fundSource);
                if (distrCodeEntity == null || string.IsNullOrEmpty(distrCodeEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                distrCode = distrCodeEntity.Code;
            }

            if (!string.IsNullOrEmpty(fundDestination))
            {
                var arTypeEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == fundDestination);
                if (arTypeEntity == null || string.IsNullOrEmpty(arTypeEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, 0);
                }
                arType = arTypeEntity.Code;
            }            

            // Get the student payments domain entity from the repository
            var studentPaymentDomainTuple = await studentPaymentRepository.GetAsync2(offset, limit, bypassCache, personId, term, distrCode, paymentType, arType);
            var studentPaymentDomainEntities = studentPaymentDomainTuple.Item1;
            var totalRecords = studentPaymentDomainTuple.Item2;

            if (studentPaymentDomainEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(new List<Dtos.StudentPayment2>(), 0);
            }

            // Convert the student payments and all its child objects into DTOs.
            foreach (var entity in studentPaymentDomainEntities)
            {
                if (entity != null)
                {
                    var paymentDto = await BuildStudentPaymentDtoAsync2(entity, bypassCache);
                    studentPaymentDtos.Add(paymentDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentPayment2>, int>(studentPaymentDtos, totalRecords);
        }
        

        /// <summary>
        /// Create a single student payments for the data model version 6
        /// </summary>
        /// <returns>A single StudentPayment</returns>
        public async Task<Dtos.StudentPayment2> CreateAsync2(Dtos.StudentPayment2 studentPayment)
        {
            CheckCreateStudentPaymentsPermission();

            studentPaymentRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentPaymentDto = new Dtos.StudentPayment2();

            var studentPaymentEntity = await BuildStudentPaymentEntityAsync2(studentPayment);
            var entity = await studentPaymentRepository.CreateAsync2(studentPaymentEntity);

            studentPaymentDto = await BuildStudentPaymentDtoAsync2(entity);

            return studentPaymentDto;
        }
        
        private async Task<Dtos.StudentPayment2> BuildStudentPaymentDtoAsync2(Ellucian.Colleague.Domain.Student.Entities.StudentPayment studentPaymentEntity, bool bypassCache = true)
        {
            var studentPaymentDto = new Dtos.StudentPayment2();

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
            if (!string.IsNullOrEmpty(studentPaymentEntity.DistributionCode))
            {
                var distrMethodCodeEntity = (await studentReferenceDataRepository.GetDistrMethodCodesAsync(bypassCache)).FirstOrDefault(dm => dm.Code == studentPaymentEntity.DistributionCode);
                if (distrMethodCodeEntity != null)
                {
                    studentPaymentDto.FundingSource = new GuidObject2(distrMethodCodeEntity.Guid);
                } else
                {
                    throw new Exception("Could not resolve an issue with fundingSource. Entity: 'CASH.RCPTS', Record: '" + studentPaymentEntity.Guid);
                }
            }
            if (!string.IsNullOrEmpty(studentPaymentEntity.AccountsReceivableTypeCode))
            {
                var accountReceivalbeTypesEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Code == studentPaymentEntity.AccountsReceivableTypeCode);
                if (accountReceivalbeTypesEntity != null)
                {
                    studentPaymentDto.FundingDestination = new GuidObject2(accountReceivalbeTypesEntity.Guid);
                }
                else
                {
                    throw new Exception("Could not resolve an issue with fundingDestination. Entity: 'CASH.RCPTS', Record: '" + studentPaymentEntity.Guid);
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

            //if (studentPaymentEntity.GlPosted != null)
            //{
            //    studentPaymentDto.GlPosting = studentPaymentEntity.GlPosted == true ? Dtos.EnumProperties.GlPosting.posted : Dtos.EnumProperties.GlPosting.notPosted;
            //} else
            //{
            //    throw new Exception("There was an issue with generalLedgerPosting, Entity: 'CASH.RCPTS', Record: '" + studentPaymentEntity.Guid);
            //}
            

            return studentPaymentDto;
        }

        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentPayment> BuildStudentPaymentEntityAsync2(Dtos.StudentPayment2 studentPaymentDto, bool bypassCache = true)
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
            string distrMethod = "";
            string arType = "";
            if (studentPaymentDto.FundingSource != null && !string.IsNullOrEmpty(studentPaymentDto.FundingSource.Id))
            {
                var distrMethodEntity = (await studentReferenceDataRepository.GetDistrMethodCodesAsync(bypassCache)).FirstOrDefault(dm => dm.Guid == studentPaymentDto.FundingSource.Id);
                if (distrMethodEntity != null)
                {
                    distrMethod = distrMethodEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The distrMethodEntity id '{0}' is not valid. ", studentPaymentDto.FundingSource.Id), "studentPayments.FundingSource.id");
                }
            }
            if (studentPaymentDto.FundingDestination != null && !string.IsNullOrEmpty(studentPaymentDto.FundingDestination.Id))
            {
                var arTypeEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentPaymentDto.FundingDestination.Id);
                if (arTypeEntity != null)
                {
                    arType = arTypeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountReceivableType id '{0}' is not valid. ", studentPaymentDto.FundingDestination.Id), "studentPayments.accountReceivableType.id");
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
                DistributionCode = distrMethod,
                AccountsReceivableTypeCode = arType,
                Comments = studentPaymentDto.Comments,
                Term = term,
                PaymentAmount = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Value : 0,
                PaymentCurrency = (studentPaymentDto.Amount != null) ? studentPaymentDto.Amount.Currency.ToString() : string.Empty
                //GlPosted = studentPaymentDto.GlPosting == Dtos.EnumProperties.GlPosting.posted ? true : false
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

        #region Student Payments shared methods
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
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentPayments);

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
