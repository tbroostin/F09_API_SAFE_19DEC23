// Copyright 2016 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Implements the IStudentChargeService
    /// </summary>
    [RegisterType]
    public class StudentChargeService : BaseCoordinationService, IStudentChargeService
    {
        private IStudentChargeRepository studentChargeRepository;
        private IPersonRepository personRepository;
        private IReferenceDataRepository referenceDataRepository;
        private IStudentReferenceDataRepository studentReferenceDataRepository;
        private readonly ITermRepository termRepository;
        private readonly IConfigurationRepository configurationRepository;

        // Constructor to initialize the private attributes
        public StudentChargeService(IStudentChargeRepository studentChargeRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            ITermRepository termRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.studentChargeRepository = studentChargeRepository;
            this.personRepository = personRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.studentReferenceDataRepository = studentReferenceDataRepository;
            this.termRepository = termRepository;
            this.configurationRepository = configurationRepository;
        }

        private IEnumerable<Term> _allTerms = null;
        private IEnumerable<Domain.Student.Entities.AccountingCode> _allArCodes = null;
        private IEnumerable<Domain.Student.Entities.AccountReceivableType> _allArTypes = null;

        #region EEDM Student charge V6
        /// <summary>
        /// Returns the DTO for the specified student charges
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Dtos.StudentCharge> GetByIdAsync(string id)
        {
            CheckViewStudentChargesPermission();
            // Get the student charges domain entity from the repository
            var studentChargeDomainEntity = await studentChargeRepository.GetByIdAsync(id);

            if (studentChargeDomainEntity == null)
            {
                throw new KeyNotFoundException("Student Charge not found for GUID " + id);
            }

            // Convert the student charge object into DTO.
            return await BuildStudentChargeDtoAsync(studentChargeDomainEntity);
        }

        /// <summary>
        /// Returns all student charges for the data model version 6
        /// </summary>
        /// <returns>Collection of StudentCharges</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCharge>, int>> GetAsync(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string accountingCode = "", string chargeType = "")
        {
            CheckViewStudentChargesPermission();

            var studentChargeDtos = new List<Dtos.StudentCharge>();
            string personId = "";
            string term = "";
            string arCode = "";

            if (!string.IsNullOrEmpty(student))
            {
                personId = await personRepository.GetPersonIdFromGuidAsync(student);
                if (string.IsNullOrEmpty(personId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, 0);
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                var termEntity = (await termRepository.GetAsync(bypassCache)).FirstOrDefault(t => t.RecordGuid == academicPeriod);
                if (termEntity == null || string.IsNullOrEmpty(termEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, 0);
                }
                term = termEntity.Code;
            }
            if (!string.IsNullOrEmpty(accountingCode))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == accountingCode);
                if (arCodeEntity == null || string.IsNullOrEmpty(arCodeEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, 0);
                }
                arCode = arCodeEntity.Code;
            }
            if (!string.IsNullOrEmpty(chargeType))
            {
                try
                {
                    var enumChargType = (Dtos.EnumProperties.StudentChargeTypes)Enum.Parse(typeof(Dtos.EnumProperties.StudentChargeTypes), chargeType);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, 0);
                }
            }

            // Get the student charges domain entity from the repository
            var studentChargeDomainTuple = await studentChargeRepository.GetAsync(offset, limit, bypassCache, personId, term, arCode, "", chargeType);
            var studentChargeDomainEntities = studentChargeDomainTuple.Item1;
            var totalRecords = studentChargeDomainTuple.Item2;

            if (studentChargeDomainEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, 0);
            }

            // Convert the student charges and all its child objects into DTOs.
            foreach (var entity in studentChargeDomainEntities)
            {
                if (entity != null)
                {
                    var chargeDto = await BuildStudentChargeDtoAsync(entity, bypassCache);
                    studentChargeDtos.Add(chargeDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentCharge>, int>(studentChargeDtos, totalRecords);
        }

        /// <summary>
        /// Create a single student charges for the data model version 6
        /// </summary>
        /// <returns>A single StudentCharge</returns>
        public async Task<Dtos.StudentCharge> CreateAsync(Dtos.StudentCharge studentCharge)
        {
            CheckCreateStudentChargesPermission();

            ValidateStudentCharges(studentCharge);

            studentChargeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentChargeDto = new Dtos.StudentCharge();

            var studentChargeEntity = await BuildStudentChargeEntityAsync(studentCharge);
            var entity = await studentChargeRepository.CreateAsync(studentChargeEntity);

            studentChargeDto = await BuildStudentChargeDtoAsync(entity);

            return studentChargeDto;
        }

        private async Task<Dtos.StudentCharge> BuildStudentChargeDtoAsync(Ellucian.Colleague.Domain.Student.Entities.StudentCharge studentChargeEntity, bool bypassCache = true)
        {
            var studentChargeDto = new Dtos.StudentCharge();

            studentChargeDto.Person = new GuidObject2((!string.IsNullOrEmpty(studentChargeEntity.PersonId)) ?
                await personRepository.GetPersonGuidFromIdAsync(studentChargeEntity.PersonId) :
                string.Empty);
            studentChargeDto.Id = studentChargeEntity.Guid;
            if (string.IsNullOrEmpty(studentChargeDto.Id))
            {
                studentChargeDto.Id = "00000000-0000-0000-0000-000000000000";
            }

            if (!string.IsNullOrEmpty(studentChargeEntity.Term))
            {
                var termEntity = (await termRepository.GetAsync()).FirstOrDefault(t => t.Code == studentChargeEntity.Term);
                if (termEntity != null && !string.IsNullOrEmpty(termEntity.RecordGuid))
                {
                    studentChargeDto.AcademicPeriod = new GuidObject2(termEntity.RecordGuid);
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableCode))
            {
                var accountingCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableCode);
                if (accountingCodeEntity != null)
                {
                    studentChargeDto.AccountingCode = new GuidObject2(accountingCodeEntity.Guid);
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableTypeCode))
            {
                var accountReceivalbeTypesEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableTypeCode);
                if (accountReceivalbeTypesEntity != null)
                {
                    studentChargeDto.AccountReceivableType = new GuidObject2(accountReceivalbeTypesEntity.Guid);
                }
            }
            studentChargeDto.ChargeableOn = studentChargeEntity.ChargeDate.Date;
            studentChargeDto.ChargeType = !string.IsNullOrEmpty(studentChargeEntity.ChargeType) ?
                ConvertChargeTypes(studentChargeEntity.ChargeType) :
                Dtos.EnumProperties.StudentChargeTypes.notset;
            studentChargeDto.Comments = studentChargeEntity.Comments != null && studentChargeEntity.Comments.Any() ?
                studentChargeEntity.Comments :
                null;

            if (studentChargeEntity.UnitCost != 0 && !string.IsNullOrEmpty(studentChargeEntity.UnitCurrency))
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    UnitCost = new Dtos.DtoProperties.ChargedAmountUnitCostDtoProperty()
                    {
                        Quantity = studentChargeEntity.UnitQuantity,
                        Cost = new Dtos.DtoProperties.AmountDtoProperty()
                        {
                            Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.UnitCurrency),
                            Value = studentChargeEntity.UnitCost
                        }
                    }
                };
            }
            else
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    Amount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.ChargeCurrency),
                        Value = studentChargeEntity.ChargeAmount
                    }
                };
            }
            return studentChargeDto;
        }

        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentCharge> BuildStudentChargeEntityAsync(Dtos.StudentCharge studentChargeDto, bool bypassCache = true)
        {
            if (studentChargeDto.Person == null || string.IsNullOrEmpty(studentChargeDto.Person.Id))
            {
                throw new ArgumentNullException("studentCharge.student.id", "The Student id cannot be null. ");
            }
            if (studentChargeDto.ChargeType == Dtos.EnumProperties.StudentChargeTypes.notset)
            {
                throw new ArgumentNullException("studentCharge.chargeType", "The chargeType must be set and cannot be null. ");
            }

            var personId = await personRepository.GetPersonIdFromGuidAsync(studentChargeDto.Person.Id);
            var chargeType = studentChargeDto.ChargeType.ToString();
            var chargeDate = studentChargeDto.ChargeableOn.HasValue ? new DateTime(studentChargeDto.ChargeableOn.Value.Date.Year, studentChargeDto.ChargeableOn.Value.Date.Month, studentChargeDto.ChargeableOn.Value.Date.Day) : DateTime.Today.Date;
            string arCode = "";
            string arType = "";
            if (studentChargeDto.AccountingCode != null && !string.IsNullOrEmpty(studentChargeDto.AccountingCode.Id))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.AccountingCode.Id);
                if (arCodeEntity != null)
                {
                    arCode = arCodeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountingCode id '{0}' is not valid. ", studentChargeDto.AccountingCode.Id), "studentCharges.accountingCode.id");
                }
            }
            if (studentChargeDto.AccountReceivableType != null && !string.IsNullOrEmpty(studentChargeDto.AccountReceivableType.Id))
            {
                var arTypeEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.AccountReceivableType.Id);
                if (arTypeEntity != null)
                {
                    arType = arTypeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountReceivableType id '{0}' is not valid. ", studentChargeDto.AccountReceivableType.Id), "studentCharges.accountReceivableType.id");
                }
            }
            var termEntity = (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id)) ?
                (await termRepository.GetAsync(bypassCache)).FirstOrDefault(acc => acc.RecordGuid == studentChargeDto.AcademicPeriod.Id) :
                null;
            if (termEntity == null)
            {
                if (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id))
                {
                    throw new ArgumentException(string.Format("The Academic Period id {0} is invalid. ", studentChargeDto.AcademicPeriod.Id), "studentCharge.academicPeriod.id");
                }
                throw new ArgumentException("The Academic Period is required for Colleague. ", "studentCharge.academicPeriod");
            }
            var term = termEntity.Code;

            var studentChargeEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentCharge(personId, chargeType, chargeDate)
            {
                Guid = (studentChargeDto.Id != null && !string.IsNullOrEmpty(studentChargeDto.Id)) ? studentChargeDto.Id : string.Empty,
                AccountsReceivableCode = arCode,
                AccountsReceivableTypeCode = arType,
                Comments = studentChargeDto.Comments,
                Term = term,
                ChargeAmount = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Value : 0,
                ChargeCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Currency.ToString() : string.Empty,
                UnitQuantity = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null) ? studentChargeDto.ChargedAmount.UnitCost.Quantity : 0,
                UnitCost = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Value : 0,
                UnitCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Currency.ToString() : string.Empty
            };

            studentChargeEntity.ChargeFromElevate = false;
            if (studentChargeDto.MetadataObject != null && studentChargeDto.MetadataObject.CreatedBy != null)
            {
                studentChargeEntity.ChargeFromElevate = studentChargeDto.MetadataObject.CreatedBy == "Elevate" ? true : false;
            }

            try
            {
                studentChargeEntity.InvoiceItemID = (studentChargeDto.Id != null && !string.IsNullOrEmpty(studentChargeDto.Id)) ? (await referenceDataRepository.GetGuidLookupResultFromGuidAsync(studentChargeDto.Id)).PrimaryKey : string.Empty;
            }
            catch
            {
                // Do nothing if the GUID doesn't already exist, just leave the invoice item id blank.
            }
            return studentChargeEntity;
        }

        /// <summary>
        /// Helper method to validate Student Charges.
        /// </summary>
        private void ValidateStudentCharges(Dtos.StudentCharge studentCharge)
        {
            if (studentCharge.AcademicPeriod == null)
            {
                throw new ArgumentNullException("studentCharges.academicPeriod", "The academic period is required when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount == null)
            {
                throw new ArgumentNullException("studentCharges.chargedAmount", "The charged amount cannot be null when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount.Amount == null && studentCharge.ChargedAmount.UnitCost == null)
            {
                throw new ArgumentNullException("studentCharges.chargeAmount", "The charged amount must contain either amount or unitCost when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.UnitCost != null)
            {
                throw new ArgumentException("Both amount and unitCost can not be used together. ", "studentCharges.ChargedAmount");
            }
            if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentCharges.chargedAmount.amount.currency");
            }
            if (studentCharge.ChargedAmount.Amount != null && (studentCharge.ChargedAmount.Amount.Value == null || studentCharge.ChargedAmount.Amount.Value == 0))
            {
                throw new ArgumentException("A charge amount of zero is not allowed.", "studentCharges.chargedAmount.amount.value");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && studentCharge.ChargedAmount.UnitCost.Cost != null && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentCharges.chargedAmount.unitCost.currency");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Quantity == null || studentCharge.ChargedAmount.UnitCost.Quantity <= 0))
            {
                throw new ArgumentException("The charged amount unit cost quantity must be greater than 0 when using unit costs. ", "studentCharge.chargedAmount.unitCost.quantity");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Cost == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == 0))
            {
                throw new ArgumentException("A charge amount of zero is not allowed.", "studentCharges.chargedAmount.unitCost.cost.value");
            }
            if (studentCharge.ChargeType == Dtos.EnumProperties.StudentChargeTypes.notset)
            {
                throw new ArgumentException("The chargeType is either invalid or empty and is required when submitting a student charge. ", "studentCharges.chargeType");
            }
            if (studentCharge.Person == null || string.IsNullOrEmpty(studentCharge.Person.Id))
            {
                throw new ArgumentNullException("studentCharges.student.id", "The student id is required when submitting a student charge. ");
            }
        }
        #endregion

        #region EEDM Student charge V11
        /// <summary>
        /// Returns the DTO for the specified student charges
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Dtos.StudentCharge1> GetByIdAsync1(string id)
        {
            CheckViewStudentChargesPermission();
            // Get the student charges domain entity from the repository
            var studentChargeDomainEntity = await studentChargeRepository.GetByIdAsync(id);

            if (studentChargeDomainEntity == null)
            {
                throw new KeyNotFoundException("Student Charge not found for GUID " + id);
            }

            // Convert the student charge object into DTO.
            return await BuildStudentChargeDtoAsync1(studentChargeDomainEntity);
        }

        /// <summary>
        /// Returns all student charges for the data model version 6
        /// </summary>
        /// <returns>Collection of StudentCharges</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCharge1>, int>> GetAsync1(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string fundingDestination = "", string fundingSource = "", string chargeType = "")
        {
            CheckViewStudentChargesPermission();

            var studentChargeDtos = new List<Dtos.StudentCharge1>();
            string personId = "";
            string term = "";
            string arCode = "";
            string arType = "";
            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(student);
                }
                catch
                {
                    personId = string.Empty;
                }
                if (string.IsNullOrEmpty(personId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, 0);
                    //throw new ArgumentException(string.Format("Invalid id '{0}' used in filter parameter 'student'. ", student), "student");
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                var termEntity = (await termRepository.GetAsync(bypassCache)).FirstOrDefault(t => t.RecordGuid == academicPeriod);
                if (termEntity == null || string.IsNullOrEmpty(termEntity.Code))
                {
                    //throw new ArgumentException(string.Format("Invalid id '{0}' used in filter parameter 'academicPeriod'. ", academicPeriod), "academicPeriod");
                    return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, 0);
                }
                term = termEntity.Code;
            }
            if (!string.IsNullOrEmpty(fundingDestination))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == fundingDestination);
                if (arCodeEntity == null || string.IsNullOrEmpty(arCodeEntity.Code))
                {
                    //throw new ArgumentException(string.Format("Invalid id '{0}' used in filter parameter 'fundingDestination'. ", fundingDestination), "fundingDestination");
                    return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, 0);
                }
                arCode = arCodeEntity.Code;
            }
            if (!string.IsNullOrEmpty(fundingSource))
            {
                var accountReceivalbeTypesEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(at => at.Guid == fundingSource);

                if (accountReceivalbeTypesEntity == null || string.IsNullOrEmpty(accountReceivalbeTypesEntity.Code))
                {
                    //throw new ArgumentException(string.Format("Invalid id '{0}' used in filter parameter 'fundingSource'. ", fundingSource), "fundingSource");
                    return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, 0);
                }
                arType = accountReceivalbeTypesEntity.Code;
            }
            if (!string.IsNullOrEmpty(chargeType))
            {
                try
                {
                    var enumChargType = (Dtos.EnumProperties.StudentChargeTypes)Enum.Parse(typeof(Dtos.EnumProperties.StudentChargeTypes), chargeType);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, 0);
                    //throw new ArgumentException(string.Format("Invalid enumeration '{0}' used in filter parameter 'chargeType'. ", chargeType), "chargeType");
                }
            }

            // Get the student charges domain entity from the repository
            var studentChargeDomainTuple = await studentChargeRepository.GetAsync(offset, limit, bypassCache, personId, term, arCode, arType, chargeType);
            var studentChargeDomainEntities = studentChargeDomainTuple.Item1;
            var totalRecords = studentChargeDomainTuple.Item2;

            if (studentChargeDomainEntities == null)
            {
                throw new ArgumentNullException("StudentChargeDomainEntity", "StudentChargeDomainEntity cannot be null. ");
            }

            // Convert the student charges and all its child objects into DTOs.
            foreach (var entity in studentChargeDomainEntities)
            {
                if (entity != null)
                {
                    var chargeDto = await BuildStudentChargeDtoAsync1(entity, bypassCache);
                    studentChargeDtos.Add(chargeDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentCharge1>, int>(studentChargeDtos, totalRecords);
        }

        /// <summary>
        /// Create a single student charges for the data model version 6
        /// </summary>
        /// <returns>A single StudentCharge</returns>
        public async Task<Dtos.StudentCharge1> CreateAsync1(Dtos.StudentCharge1 studentCharge)
        {
            CheckCreateStudentChargesPermission();

            ValidateStudentCharges1(studentCharge);

            studentChargeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentChargeDto = new Dtos.StudentCharge1();

            var studentChargeEntity = await BuildStudentChargeEntityAsync1(studentCharge);
            var entity = await studentChargeRepository.CreateAsync(studentChargeEntity);

            studentChargeDto = await BuildStudentChargeDtoAsync1(entity);

            return studentChargeDto;
        }

        private async Task<Dtos.StudentCharge1> BuildStudentChargeDtoAsync1(Ellucian.Colleague.Domain.Student.Entities.StudentCharge studentChargeEntity, bool bypassCache = true)
        {
            var studentChargeDto = new Dtos.StudentCharge1();

            studentChargeDto.Person = new GuidObject2((!string.IsNullOrEmpty(studentChargeEntity.PersonId)) ?
                await personRepository.GetPersonGuidFromIdAsync(studentChargeEntity.PersonId) :
                string.Empty);
            studentChargeDto.Id = studentChargeEntity.Guid;
            if (string.IsNullOrEmpty(studentChargeDto.Id))
            {
                studentChargeDto.Id = "00000000-0000-0000-0000-000000000000";
            }

            if (!string.IsNullOrEmpty(studentChargeEntity.Term))
            {
                var termEntity = (await GetTerms()).FirstOrDefault(t => t.Code == studentChargeEntity.Term);
                if (termEntity != null && !string.IsNullOrEmpty(termEntity.RecordGuid))
                {
                    studentChargeDto.AcademicPeriod = new GuidObject2(termEntity.RecordGuid);
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableCode))
            {
                var accountingCodeEntity = (await GetArCodes(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableCode);
                if (accountingCodeEntity != null)
                {
                    studentChargeDto.FundingDestination = new GuidObject2(accountingCodeEntity.Guid);
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableTypeCode))
            {
                var accountReceivalbeTypesEntity = (await GetArTypes(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableTypeCode);
                if (accountReceivalbeTypesEntity != null)
                {
                    studentChargeDto.FundingSource = new GuidObject2(accountReceivalbeTypesEntity.Guid);
                }
            }
            studentChargeDto.ChargeableOn = studentChargeEntity.ChargeDate.Date;
            studentChargeDto.ChargeType = !string.IsNullOrEmpty(studentChargeEntity.ChargeType) ?
                ConvertChargeTypes(studentChargeEntity.ChargeType) :
                Dtos.EnumProperties.StudentChargeTypes.notset;
            studentChargeDto.Comments = studentChargeEntity.Comments != null && studentChargeEntity.Comments.Any() ?
                studentChargeEntity.Comments :
                null;

            if (studentChargeEntity.UnitCost != 0 && !string.IsNullOrEmpty(studentChargeEntity.UnitCurrency))
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    UnitCost = new Dtos.DtoProperties.ChargedAmountUnitCostDtoProperty()
                    {
                        Quantity = studentChargeEntity.UnitQuantity,
                        Cost = new Dtos.DtoProperties.AmountDtoProperty()
                        {
                            Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.UnitCurrency),
                            Value = studentChargeEntity.UnitCost
                        }
                    }
                };
            }
            else
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    Amount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.ChargeCurrency),
                        Value = studentChargeEntity.ChargeAmount
                    }
                };
            }
            return studentChargeDto;
        }

        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentCharge> BuildStudentChargeEntityAsync1(Dtos.StudentCharge1 studentChargeDto, bool bypassCache = true)
        {
            if (studentChargeDto.Person == null || string.IsNullOrEmpty(studentChargeDto.Person.Id))
            {
                throw new ArgumentNullException("studentCharge.student.id", "The Student id cannot be null. ");
            }
            if (studentChargeDto.ChargeType == Dtos.EnumProperties.StudentChargeTypes.notset)
            {
                throw new ArgumentNullException("studentCharge.chargeType", "The chargeType must be set and cannot be null. ");
            }

            var personId = await personRepository.GetPersonIdFromGuidAsync(studentChargeDto.Person.Id);
            var chargeType = studentChargeDto.ChargeType.ToString();
            var chargeDate = studentChargeDto.ChargeableOn.HasValue ? new DateTime(studentChargeDto.ChargeableOn.Value.Date.Year, studentChargeDto.ChargeableOn.Value.Date.Month, studentChargeDto.ChargeableOn.Value.Date.Day) : DateTime.Today.Date;
            string arCode = "";
            string arType = "";
            if (studentChargeDto.FundingDestination != null && !string.IsNullOrEmpty(studentChargeDto.FundingDestination.Id))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.FundingDestination.Id);
                if (arCodeEntity != null)
                {
                    arCode = arCodeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountingCode id '{0}' is not valid. ", studentChargeDto.FundingDestination.Id), "studentCharges.accountingCode.id");
                }
            }
            if (studentChargeDto.FundingSource != null && !string.IsNullOrEmpty(studentChargeDto.FundingSource.Id))
            {
                var arTypeEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.FundingSource.Id);
                if (arTypeEntity != null)
                {
                    arType = arTypeEntity.Code;
                }
                else
                {
                    throw new ArgumentException(string.Format("The accountReceivableType id '{0}' is not valid. ", studentChargeDto.FundingSource.Id), "studentCharges.accountReceivableType.id");
                }
            }
            var termEntity = (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id)) ?
                (await termRepository.GetAsync(bypassCache)).FirstOrDefault(acc => acc.RecordGuid == studentChargeDto.AcademicPeriod.Id) :
                null;
            if (termEntity == null)
            {
                if (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id))
                {
                    throw new ArgumentException(string.Format("The Academic Period id {0} is invalid. ", studentChargeDto.AcademicPeriod.Id), "studentCharge.academicPeriod.id");
                }
                throw new ArgumentException("The Academic Period is required for Colleague. ", "studentCharge.academicPeriod");
            }
            var term = termEntity.Code;

            var studentChargeEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentCharge(personId, chargeType, chargeDate)
            {
                Guid = (studentChargeDto.Id != null && !string.IsNullOrEmpty(studentChargeDto.Id)) ? studentChargeDto.Id : string.Empty,
                AccountsReceivableCode = arCode,
                AccountsReceivableTypeCode = arType,
                Comments = studentChargeDto.Comments,
                Term = term,
                ChargeAmount = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Value : 0,
                ChargeCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Currency.ToString() : string.Empty,
                UnitQuantity = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null) ? studentChargeDto.ChargedAmount.UnitCost.Quantity : 0,
                UnitCost = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Value : 0,
                UnitCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Currency.ToString() : string.Empty
            };

            studentChargeEntity.ChargeFromElevate = false;
            if (studentChargeDto.MetadataObject != null && studentChargeDto.MetadataObject.CreatedBy != null)
            {
                studentChargeEntity.ChargeFromElevate = studentChargeDto.MetadataObject.CreatedBy == "Elevate" ? true : false;
            }

            //try
            //{
            //    studentChargeEntity.InvoiceItemID = (studentChargeDto.Id != null && !string.IsNullOrEmpty(studentChargeDto.Id)) ? (await referenceDataRepository.GetGuidLookupResultFromGuidAsync(studentChargeDto.Id)).PrimaryKey : string.Empty;
            //}
            //catch
            //{
            //    // Do nothing if the GUID doesn't already exist, just leave the invoice item id blank.
            //}
            return studentChargeEntity;
        }

        /// <summary>
        /// Helper method to validate Student Charges.
        /// </summary>
        private void ValidateStudentCharges1(Dtos.StudentCharge1 studentCharge)
        {
            if (studentCharge.AcademicPeriod == null)
            {
                throw new ArgumentNullException("studentCharges.academicPeriod", "The academic period is required when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount == null)
            {
                throw new ArgumentNullException("studentCharges.chargedAmount", "The charged amount cannot be null when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount.Amount == null && studentCharge.ChargedAmount.UnitCost == null)
            {
                throw new ArgumentNullException("studentCharges.chargeAmount", "The charged amount must contain either amount or unitCost when submitting a student charge. ");
            }
            if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.UnitCost != null)
            {
                throw new ArgumentException("Both amount and unitCost can not be used together. ", "studentCharges.ChargedAmount");
            }
            if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentCharges.chargedAmount.amount.currency");
            }
            if (studentCharge.ChargedAmount.Amount != null && (studentCharge.ChargedAmount.Amount.Value == null || studentCharge.ChargedAmount.Amount.Value == 0))
            {
                throw new ArgumentException("A charge amount of zero is not allowed.", "studentCharges.chargedAmount.amount.value");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && studentCharge.ChargedAmount.UnitCost.Cost != null && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentCharges.chargedAmount.unitCost.currency");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Quantity == null || studentCharge.ChargedAmount.UnitCost.Quantity <= 0))
            {
                throw new ArgumentException("The charged amount unit cost quantity must be greater than 0 when using unit costs. ", "studentCharge.chargedAmount.unitCost.quantity");
            }
            if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Cost == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == 0))
            {
                throw new ArgumentException("A charge amount of zero is not allowed.", "studentCharges.chargedAmount.unitCost.cost.value");
            }
            if (studentCharge.ChargeType == Dtos.EnumProperties.StudentChargeTypes.notset)
            {
                throw new ArgumentException("The chargeType is either invalid or empty and is required when submitting a student charge. ", "studentCharges.chargeType");
            }
            if (studentCharge.Person == null || string.IsNullOrEmpty(studentCharge.Person.Id))
            {
                throw new ArgumentNullException("studentCharges.student.id", "The student id is required when submitting a student charge. ");
            }
        }
        #endregion

        #region EEDM Student charge V16.0.0
        /// <summary>
        /// Returns the DTO for the specified student charges
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Dtos.StudentCharge2> GetStudentChargesByIdAsync(string id)
        {
            CheckViewStudentChargesPermission();
            // Get the student charges domain entity from the repository
            var studentChargeDomainEntity = await studentChargeRepository.GetByIdAsync(id);

            if (studentChargeDomainEntity == null)
            {
                throw new KeyNotFoundException("Student Charge not found for GUID " + id);
            }

            // Convert the student charge object into DTO.
            var studentChargeDto = await BuildStudentChargeDto2Async(studentChargeDomainEntity);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return studentChargeDto;
        }

        /// <summary>
        /// Returns all student charges for the data model version 6
        /// </summary>
        /// <returns>Collection of StudentCharges</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentCharge2>, int>> GetStudentChargesAsync(int offset, int limit, bool bypassCache, string student = "", string academicPeriod = "", string fundingDestination = "", string fundingSource = "", string usage = "")
        {
            CheckViewStudentChargesPermission();

            var studentChargeDtos = new List<Dtos.StudentCharge2>();
            string personId = "";
            string term = "";
            string arCode = "";
            string arType = "";
            if (!string.IsNullOrEmpty(student))
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(student);
                }
                catch
                {
                    // Do not throw an exception when the person id is invalid or missing.
                    // Just return an empty set.
                    personId = string.Empty;
                }
                if (string.IsNullOrEmpty(personId))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, 0);
                }
            }
            if (!string.IsNullOrEmpty(academicPeriod))
            {
                var termEntity = (await termRepository.GetAsync(bypassCache)).FirstOrDefault(t => t.RecordGuid == academicPeriod);
                if (termEntity == null || string.IsNullOrEmpty(termEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, 0);
                }
                term = termEntity.Code;
            }
            if (!string.IsNullOrEmpty(fundingDestination))
            {
                var arCodeEntity = (await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache)).FirstOrDefault(ac => ac.Guid == fundingDestination);
                if (arCodeEntity == null || string.IsNullOrEmpty(arCodeEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, 0);
                }
                arCode = arCodeEntity.Code;
            }
            if (!string.IsNullOrEmpty(fundingSource))
            {
                var accountReceivalbeTypesEntity = (await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache)).FirstOrDefault(at => at.Guid == fundingSource);

                if (accountReceivalbeTypesEntity == null || string.IsNullOrEmpty(accountReceivalbeTypesEntity.Code))
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, 0);
                }
                arType = accountReceivalbeTypesEntity.Code;
            }
            if (!string.IsNullOrEmpty(usage))
            {
                try
                {
                    var enumChargType = (Dtos.EnumProperties.StudentChargeUsageTypes)Enum.Parse(typeof(Dtos.EnumProperties.StudentChargeUsageTypes), usage);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, 0);
                }
            }

            // Get the student charges domain entity from the repository
            var studentChargeDomainTuple = await studentChargeRepository.GetAsync(offset, limit, bypassCache, personId, term, arCode, arType, "", usage);
            var studentChargeDomainEntities = studentChargeDomainTuple.Item1;
            var totalRecords = studentChargeDomainTuple.Item2;

            if (studentChargeDomainEntities == null)
            {
                throw new ArgumentNullException("StudentChargeDomainEntity", "StudentChargeDomainEntity cannot be null. ");
            }

            // Convert the student charges and all its child objects into DTOs.
            foreach (var entity in studentChargeDomainEntities)
            {
                if (entity != null)
                {
                    var chargeDto = await BuildStudentChargeDto2Async(entity, bypassCache);
                    studentChargeDtos.Add(chargeDto);
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.StudentCharge2>, int>(studentChargeDtos, totalRecords);
        }

        /// <summary>
        /// Create a single student charges for the data model version 6
        /// </summary>
        /// <returns>A single StudentCharge</returns>
        public async Task<Dtos.StudentCharge2> CreateStudentChargesAsync(Dtos.StudentCharge2 studentCharge)
        {
            CheckCreateStudentChargesPermission();

            var guid = studentCharge.Id;
            var sourceId = string.Empty;

            ValidateStudentCharges2(studentCharge, guid, sourceId);

            studentChargeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentChargeDto = new Dtos.StudentCharge2();

            var studentChargeEntity = await BuildStudentChargeEntity2Async(studentCharge, guid, sourceId);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            try
            {
                var entity = await studentChargeRepository.CreateAsync(studentChargeEntity);
                studentChargeDto = await BuildStudentChargeDto2Async(entity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Create.Update.Exception", guid, sourceId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return studentChargeDto;
        }

        private async Task<Dtos.StudentCharge2> BuildStudentChargeDto2Async(Domain.Student.Entities.StudentCharge studentChargeEntity, bool bypassCache = true)
        {
            Dtos.StudentCharge2 studentChargeDto = new Dtos.StudentCharge2();
            try
            {
                studentChargeDto.Person = new GuidObject2((!string.IsNullOrEmpty(studentChargeEntity.PersonId)) ?
                    await personRepository.GetPersonGuidFromIdAsync(studentChargeEntity.PersonId) :
                    string.Empty);
            }
            catch (KeyNotFoundException ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", studentChargeEntity.Guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Concat("Person GUID for ID " + studentChargeEntity.PersonId + " not found.", ": ", ex.Message), "Validation.Exception");
            }

            studentChargeDto.Id = studentChargeEntity.Guid;
            if (string.IsNullOrEmpty(studentChargeDto.Id))
            {
                studentChargeDto.Id = "00000000-0000-0000-0000-000000000000";
            }

            if (!string.IsNullOrEmpty(studentChargeEntity.Term))
            {
                var termEntity = (await GetTerms(bypassCache)).FirstOrDefault(t => t.Code == studentChargeEntity.Term);
                if (termEntity != null && !string.IsNullOrEmpty(termEntity.RecordGuid))
                {
                    studentChargeDto.AcademicPeriod = new GuidObject2(termEntity.RecordGuid);
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("Academic Period GUID for Term '{0}' not found", studentChargeEntity.Term), "Validation.Exception");
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableCode))
            {
                var accountingCodeEntity = (await GetArCodes(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableCode);
                if (accountingCodeEntity != null)
                {
                    studentChargeDto.FundingDestination = new GuidObject2(accountingCodeEntity.Guid);
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("Funding Destination GUID for AR code '{0}' not found", studentChargeEntity.Term), "Validation.Exception");
                }
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.AccountsReceivableTypeCode))
            {
                var accountReceivalbeTypesEntity = (await GetArTypes(bypassCache)).FirstOrDefault(acc => acc.Code == studentChargeEntity.AccountsReceivableTypeCode);
                if (accountReceivalbeTypesEntity != null)
                {
                    studentChargeDto.FundingSource = new GuidObject2(accountReceivalbeTypesEntity.Guid);
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("Funding Source GUID for AR type '{0}' not found", studentChargeEntity.Term), "Validation.Exception");
                }
            }
            studentChargeDto.ChargeableOn = studentChargeEntity.ChargeDate.Date;
            studentChargeDto.Comments = studentChargeEntity.Comments != null && studentChargeEntity.Comments.Any() ?
                studentChargeEntity.Comments :
                null;

            if (!string.IsNullOrEmpty(studentChargeEntity.Usage) || (studentChargeEntity.OriginatedOn != null && studentChargeEntity.OriginatedOn.HasValue))
            {
                studentChargeDto.ReportingDetail = new Dtos.DtoProperties.StudentChargesReportingDtoProperty()
                {
                    Usage = Dtos.EnumProperties.StudentChargeUsageTypes.taxReportingOnly,
                    OriginatedOn = studentChargeEntity.OriginatedOn != null && studentChargeEntity.OriginatedOn.HasValue ? studentChargeEntity.OriginatedOn : studentChargeEntity.ChargeDate.Date
                };
            }
            if (!string.IsNullOrEmpty(studentChargeEntity.OverrideDescription))
            {
                studentChargeDto.OverrideDescription = studentChargeEntity.OverrideDescription;
            }

            if (studentChargeEntity.UnitCost != 0 && !string.IsNullOrEmpty(studentChargeEntity.UnitCurrency))
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    UnitCost = new Dtos.DtoProperties.ChargedAmountUnitCostDtoProperty()
                    {
                        Quantity = studentChargeEntity.UnitQuantity,
                        Cost = new Dtos.DtoProperties.AmountDtoProperty()
                        {
                            Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.UnitCurrency),
                            Value = studentChargeEntity.UnitCost
                        }
                    }
                };
            }
            else
            {
                studentChargeDto.ChargedAmount = new Dtos.DtoProperties.ChargedAmountDtoProperty()
                {
                    Amount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), studentChargeEntity.ChargeCurrency),
                        Value = studentChargeEntity.ChargeAmount
                    }
                };
            }
            return studentChargeDto;
        }

        private async Task<Ellucian.Colleague.Domain.Student.Entities.StudentCharge> BuildStudentChargeEntity2Async(Dtos.StudentCharge2 studentChargeDto, string guid, string sourceId, bool bypassCache = true)
        {
            string personId = string.Empty;
            string arCode = string.Empty;
            string arType = string.Empty;
            string term = string.Empty;

            if (studentChargeDto.Person == null || string.IsNullOrEmpty(studentChargeDto.Person.Id))
            {
                IntegrationApiExceptionAddError("The Student id cannot be null. ", "Validation.Exception", guid, sourceId);
            }
            else
            {
                try
                {
                    personId = await personRepository.GetPersonIdFromGuidAsync(studentChargeDto.Person.Id);
                }
                catch (KeyNotFoundException ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Validation.Exception", guid, sourceId);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Validation.Exception", guid, sourceId);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Concat("Person GUID " + studentChargeDto.Person.Id + " not found.", ": ", ex.Message), "Validation.Exception", guid, sourceId);
                }
            }
            var chargeDate = studentChargeDto.ChargeableOn.HasValue ? new DateTime(studentChargeDto.ChargeableOn.Value.Date.Year, studentChargeDto.ChargeableOn.Value.Date.Month, studentChargeDto.ChargeableOn.Value.Date.Day) : DateTime.Today.Date;

            if (studentChargeDto.FundingDestination != null && !string.IsNullOrEmpty(studentChargeDto.FundingDestination.Id))
            {
                var arCodeEntity = (await GetArCodes(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.FundingDestination.Id);
                if (arCodeEntity != null)
                {
                    arCode = arCodeEntity.Code;
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("The accountingCode id '{0}' is not valid. ", studentChargeDto.FundingDestination.Id), "Validation.Exception", guid, sourceId);
                }
            }
            if (studentChargeDto.FundingSource != null && !string.IsNullOrEmpty(studentChargeDto.FundingSource.Id))
            {
                var arTypeEntity = (await GetArTypes(bypassCache)).FirstOrDefault(acc => acc.Guid == studentChargeDto.FundingSource.Id);
                if (arTypeEntity != null)
                {
                    arType = arTypeEntity.Code;
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Format("The accountReceivableType id '{0}' is not valid. ", studentChargeDto.FundingSource.Id), "Validation.Exception", guid, sourceId);
                }
            }
            var termEntity = (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id)) ?
                (await GetTerms(bypassCache)).FirstOrDefault(acc => acc.RecordGuid == studentChargeDto.AcademicPeriod.Id) :
                null;
            if (termEntity == null)
            {
                if (studentChargeDto.AcademicPeriod != null && !string.IsNullOrEmpty(studentChargeDto.AcademicPeriod.Id))
                {
                    IntegrationApiExceptionAddError(string.Format("The Academic Period id {0} is invalid. ", studentChargeDto.AcademicPeriod.Id), "Validation.Exception", guid, sourceId);
                }
                else
                {
                    IntegrationApiExceptionAddError("The Academic Period is required for Colleague. ", "Validation.Exception", guid, sourceId);
                }
            }
            else
            {
                term = termEntity.Code;
            }

            var studentChargeEntity = new Ellucian.Colleague.Domain.Student.Entities.StudentCharge(personId, chargeDate)
            {
                Guid = (studentChargeDto.Id != null && !string.IsNullOrEmpty(studentChargeDto.Id)) ? studentChargeDto.Id : string.Empty,
                AccountsReceivableCode = arCode,
                AccountsReceivableTypeCode = arType,
                Comments = studentChargeDto.Comments,
                Term = term,
                ChargeAmount = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Value : 0,
                ChargeCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.Amount != null) ? studentChargeDto.ChargedAmount.Amount.Currency.ToString() : string.Empty,
                UnitQuantity = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null) ? studentChargeDto.ChargedAmount.UnitCost.Quantity : 0,
                UnitCost = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Value : 0,
                UnitCurrency = (studentChargeDto.ChargedAmount != null && studentChargeDto.ChargedAmount.UnitCost != null && studentChargeDto.ChargedAmount.UnitCost.Cost != null) ? studentChargeDto.ChargedAmount.UnitCost.Cost.Currency.ToString() : string.Empty,
                Usage = (studentChargeDto.ReportingDetail != null && studentChargeDto.ReportingDetail.Usage != null && studentChargeDto.ReportingDetail.Usage != Dtos.EnumProperties.StudentChargeUsageTypes.notset) ? studentChargeDto.ReportingDetail.Usage.ToString() : string.Empty,
                OriginatedOn = (studentChargeDto.ReportingDetail != null && studentChargeDto.ReportingDetail.OriginatedOn != null && studentChargeDto.ReportingDetail.OriginatedOn.HasValue) ? studentChargeDto.ReportingDetail.OriginatedOn.Value : new DateTime?(),
                OverrideDescription = studentChargeDto.OverrideDescription
            };

            studentChargeEntity.ChargeFromElevate = false;
            if (studentChargeDto.MetadataObject != null && studentChargeDto.MetadataObject.CreatedBy != null)
            {
                studentChargeEntity.ChargeFromElevate = studentChargeDto.MetadataObject.CreatedBy == "Elevate" ? true : false;
            }

            return studentChargeEntity;
        }

        /// <summary>
        /// Helper method to validate Student Charges.
        /// </summary>
        private void ValidateStudentCharges2(Dtos.StudentCharge2 studentCharge, string guid, string sourceId)
        {
            if (studentCharge.AcademicPeriod == null)
            {
                IntegrationApiExceptionAddError("The academic period is required when submitting a student charge. ", "Validation.Exception", guid, sourceId);
            }
            if (studentCharge.ChargedAmount == null)
            {
                IntegrationApiExceptionAddError("The charged amount cannot be null when submitting a student charge. ", "Validation.Exception", guid, sourceId);
            }
            else
            {
                if (studentCharge.ChargedAmount.Amount == null && studentCharge.ChargedAmount.UnitCost == null)
                {
                    IntegrationApiExceptionAddError("The charged amount must contain either amount or unitCost when submitting a student charge. ", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.UnitCost != null)
                {
                    IntegrationApiExceptionAddError("Both amount and unitCost can not be used together. ", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.Amount != null && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
                {
                    IntegrationApiExceptionAddError("The currency code must be set to either 'USD' or 'CAD'. ", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.Amount != null && (studentCharge.ChargedAmount.Amount.Value == null || studentCharge.ChargedAmount.Amount.Value == 0))
                {
                    IntegrationApiExceptionAddError("A charge amount of zero is not allowed.", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.UnitCost != null && studentCharge.ChargedAmount.UnitCost.Cost != null && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentCharge.ChargedAmount.UnitCost.Cost.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
                {
                    IntegrationApiExceptionAddError("The currency code must be set to either 'USD' or 'CAD'. ", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Quantity == null || studentCharge.ChargedAmount.UnitCost.Quantity <= 0))
                {
                    IntegrationApiExceptionAddError("The charged amount unit cost quantity must be greater than 0 when using unit costs. ", "Validation.Exception", guid, sourceId);
                }
                if (studentCharge.ChargedAmount.UnitCost != null && (studentCharge.ChargedAmount.UnitCost.Cost == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == null || studentCharge.ChargedAmount.UnitCost.Cost.Value == 0))
                {
                    IntegrationApiExceptionAddError("A charge amount of zero is not allowed.", "Validation.Exception", guid, sourceId);
                }
            }
            if (studentCharge.FundingDestination == null || string.IsNullOrEmpty(studentCharge.FundingDestination.Id))
            {
                IntegrationApiExceptionAddError("The fundingDestination is required when submitting a student charge.", "Validation.Exception", guid, sourceId);
            }
            if (studentCharge.Person == null || string.IsNullOrEmpty(studentCharge.Person.Id))
            {
                IntegrationApiExceptionAddError("The student id is required when submitting a student charge. ", "Validation.Exception", guid, sourceId);
            }
            if (studentCharge.ReportingDetail != null)
            {
                if (studentCharge.ReportingDetail.Usage != null && studentCharge.ReportingDetail.Usage != Dtos.EnumProperties.StudentChargeUsageTypes.taxReportingOnly)
                {
                    IntegrationApiExceptionAddError(string.Format("The usage attribute of '{0}' is not permitted when submitting usage associated with this charge.", studentCharge.ReportingDetail.Usage.ToString()), "Validation.Exception", guid, sourceId);
                }
                else
                {
                    if (studentCharge.ReportingDetail.Usage == null)
                    {
                        if (studentCharge.ReportingDetail.OriginatedOn != null && studentCharge.ReportingDetail.OriginatedOn.HasValue)
                        {
                            IntegrationApiExceptionAddError("The originatedOn is not permitted without usage set to 'taxReportingOnly'.", "Validation.Exception", guid, sourceId);
                        }
                    }
                    else
                    {
                        if (studentCharge.ReportingDetail.OriginatedOn == null || !studentCharge.ReportingDetail.OriginatedOn.HasValue)
                        {
                            IntegrationApiExceptionAddError("The usage set to 'taxReportingOnly' is not permitted without originatedOn .", "Validation.Exception", guid, sourceId);
                        }
                        else
                        {
                            if (studentCharge.ReportingDetail.OriginatedOn.Value.Date > studentCharge.ChargeableOn.Value.Date)
                            {
                                IntegrationApiExceptionAddError("The originatedOn date must be on or before the chargeableOn date.", "Validation.Exception", guid, sourceId);
                            }
                            if (studentCharge.ReportingDetail.OriginatedOn.Value.Date > DateTime.Now.Date)
                            {
                                IntegrationApiExceptionAddError("The originatedOn date may only be set to a date on or before the current date.", "Validation.Exception", guid, sourceId);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Shared methods

        /// <summary>
        /// Update a single student charges for the data model version 6
        /// </summary>
        /// <returns>A single StudentCharge</returns>
        public async Task<Dtos.StudentCharge> UpdateAsync(string id, Dtos.StudentCharge studentCharge)
        {
            CheckCreateStudentChargesPermission();

            ValidateStudentCharges(studentCharge);

            studentChargeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var studentChargeDto = new Dtos.StudentCharge();

            var studentChargeEntity = await BuildStudentChargeEntityAsync(studentCharge);
            var entity = await studentChargeRepository.UpdateAsync(id, studentChargeEntity);

            studentChargeDto = await BuildStudentChargeDtoAsync(entity);

            return studentChargeDto;
        }

        /// <summary>
        /// Delete a single student charges for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charges GUID</param>
        /// <returns></returns>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide a student charges guid for deletion. ");
            }

            await studentChargeRepository.DeleteAsync(id);
        }

        private async Task<IEnumerable<Term>> GetTerms(bool bypassCache = false)
        {
            if (_allTerms == null)
            {
                _allTerms = await termRepository.GetAsync(bypassCache);
            }

            return _allTerms;
        }

        private async Task<IEnumerable<Domain.Student.Entities.AccountingCode>> GetArCodes(bool bypassCache = false)
        {
            if (_allArCodes == null)
            {
                _allArCodes = await studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache);
            }

            return _allArCodes;
        }

        private async Task<IEnumerable<Domain.Student.Entities.AccountReceivableType>> GetArTypes(bool bypassCache = false)
        {
            if (_allArTypes == null)
            {
                _allArTypes = await studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache);
            }

            return _allArTypes;
        }

        private Dtos.EnumProperties.StudentChargeTypes ConvertChargeTypes(string chargeType)
        {
            switch (chargeType.ToLowerInvariant())
            {
                case "tuition":
                    {
                        return Dtos.EnumProperties.StudentChargeTypes.tuition;
                    }
                case "fee":
                    {
                        return Dtos.EnumProperties.StudentChargeTypes.fee;
                    }
                case "housing":
                    {
                        return Dtos.EnumProperties.StudentChargeTypes.housing;
                    }
                case "meal":
                    {
                        return Dtos.EnumProperties.StudentChargeTypes.meal;
                    }
                default:
                    {
                        return Dtos.EnumProperties.StudentChargeTypes.notset;
                    }
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Charges.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentChargesPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentCharges) || HasPermission(StudentPermissionCodes.CreateStudentCharges);

            // User is not allowed to create or update Student charges without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view Student Charges.", CurrentUser.UserId));
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student Charges.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateStudentChargesPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.CreateStudentCharges);

            // User is not allowed to create or update Student charges without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to create Student Charges.", CurrentUser.UserId));
            }
        }
        #endregion
    }
}
