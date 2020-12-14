// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class InstantEnrollmentRepository : BaseColleagueRepository, IInstantEnrollmentRepository
    {
        public InstantEnrollmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Use default cache timeout value
            CacheTimeout = Level1CacheTimeoutValue;
        }
        /// <summary>
        /// Accepts demographic information and a list of course sections, performs a mock registration, and return list of sections with the associated cost
        /// </summary>
        /// <param name="proposedRegistration">A <see cref="InstantEnrollmentProposedRegistration"/> containing the information necessary to estimate costs.</param>
        /// <returns>A <see cref="InstantEnrollmentProposedRegistrationResult"/> containing the estimated costs of the sections.</returns>
        public async Task<InstantEnrollmentProposedRegistrationResult> GetProposedRegistrationResultAync(InstantEnrollmentProposedRegistration proposedRegistration)
        {
            List<StudentPhones> studentPhones = null;
            InstantEnrollmentProposedRegistrationResult proposedRegistrationResult = null;
            if (proposedRegistration == null)
            {
                throw new ArgumentNullException("proposedRegistration", "proposed registration parameter is required in order to mock registration and retrieve section's associated cost");
            }

            if (proposedRegistration.ProposedSections == null || proposedRegistration.ProposedSections.Count == 0)
            {
                throw new ArgumentException("ProposedSections", "proposed registration parameter should have proposed sections to register for and retrieve associated cost");
            }

            var sectionsToRegister = proposedRegistration.ProposedSections.Where(x => x != null).Select(x => new ProposedSectionInformation()
            {
                ProposedSection = x.SectionId,
                ProposedSectionCredit = x.AcademicCredits,
                ProposedSectionMktgSrc = x.MarketingSource,
                ProposedSectionRegReason = x.RegistrationReason

            }).ToList<ProposedSectionInformation>();

            if (proposedRegistration.PersonDemographic != null && proposedRegistration.PersonDemographic.PersonPhones != null)
            {
                studentPhones = proposedRegistration.PersonDemographic.PersonPhones.Where(x => x != null).Select(x => new StudentPhones()
                {
                    StudentPhoneNumbers = x.Number,
                    StudentPhoneExtensions = x.Extension,
                    StudentPhoneTypes = x.TypeCode
                }).ToList<StudentPhones>();
            }
            var request = new InstEnrollProposedRgstrtnRequest()
            {
                StudentId = proposedRegistration.PersonId,
                AcadProgram = proposedRegistration.AcademicProgram,
                Catalog = proposedRegistration.Catalog,
                StudentPhones = studentPhones,
                ProposedSectionInformation = sectionsToRegister,
                EducationalGoal = proposedRegistration.EducationalGoal
            };

            if (proposedRegistration.PersonDemographic != null)
            {
                request.StudentEmailAddress = proposedRegistration.PersonDemographic.EmailAddress;
                request.StudentPrefix = proposedRegistration.PersonDemographic.Prefix;
                request.StudentGivenName = proposedRegistration.PersonDemographic.FirstName;
                request.StudentMiddleName = proposedRegistration.PersonDemographic.MiddleName;
                request.StudentFamilyName = proposedRegistration.PersonDemographic.LastName;
                request.StudentSuffix = proposedRegistration.PersonDemographic.Suffix;
                request.StudentAddress = proposedRegistration.PersonDemographic.AddressLines != null ? proposedRegistration.PersonDemographic.AddressLines.ToList<string>() : new List<string>();
                request.StudentCity = proposedRegistration.PersonDemographic.City;
                request.StudentState = proposedRegistration.PersonDemographic.State;
                request.StudentPostalCode = proposedRegistration.PersonDemographic.ZipCode;
                request.StudentCounty = proposedRegistration.PersonDemographic.CountyCode;
                request.StudentCountry = proposedRegistration.PersonDemographic.CountryCode;
                request.StudentBirthDate = proposedRegistration.PersonDemographic.BirthDate;
                request.StudentRacialGroups = proposedRegistration.PersonDemographic.RacialGroups.ToList();
                request.StudentEthnics = proposedRegistration.PersonDemographic.EthnicGroups.ToList();
                request.StudentGender = proposedRegistration.PersonDemographic.Gender;
                request.StudentCitizenship = proposedRegistration.PersonDemographic.CitizenshipCountryCode;
                request.StudentTaxId = proposedRegistration.PersonDemographic.GovernmentId;
            }
            //send request by calling CTX
            var response = await transactionInvoker.ExecuteAsync<InstEnrollProposedRgstrtnRequest, InstEnrollProposedRgstrtnResponse>(request);
            //convert response to InstantEnrollmentProposedRegistrationResult entity
            //convert registered sections in response to registered sections for instant enrollment entity
            if (response != null)
            {
                List<InstantEnrollmentRegistrationBaseRegisteredSection> instantEnrollmentRegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
                if (response.RegisteredSectionInformation != null)
                {
                    var sects = response.RegisteredSectionInformation
                        .Where(x => x != null && !String.IsNullOrEmpty(x.RegisteredSection))
                        .Select(x => new InstantEnrollmentRegistrationBaseRegisteredSection(x.RegisteredSection, x.RegisteredSectionCost))
                        .ToList();
                    if (sects != null)
                    {
                        instantEnrollmentRegisteredSections.AddRange(sects);
                    }
                }
                //convert response messages to instant enrollment messages entity
                List<InstantEnrollmentRegistrationBaseMessage> instantEnrollmentRegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
                if (response.RegistrationMessages != null)
                {
                    var msgs = response.RegistrationMessages
                        .Where(x => x != null && !String.IsNullOrEmpty(x.Messages))
                        .Select(x => new InstantEnrollmentRegistrationBaseMessage(x.MessageSections, x.Messages))
                        .ToList();
                    if (msgs != null)
                    {
                        instantEnrollmentRegistrationMessages.AddRange(msgs);
                    }
                }
                proposedRegistrationResult = new InstantEnrollmentProposedRegistrationResult(response.ErrorOccurred, instantEnrollmentRegisteredSections, instantEnrollmentRegistrationMessages);
            }
            else
            {
                logger.Info("InstantEnrollmentProposedRegistrationResult response from CTX is null, hence will return nothing");
            }

            return proposedRegistrationResult;
        }

        /// <summary>
        /// Registers a student for classes, creating the student if necessary, when the total cost for registration is zero.
        /// </summary>
        /// <param name="zeroCostRegistration">A <see cref="InstantEnrollmentZeroCostRegistration"/> containing the information necessary to register for classes.</param>
        /// <returns>A <see cref=InstantEnrollmentZeroCostRegistrationResult"/> containing the results of the registration.</returns>
        public async Task<InstantEnrollmentZeroCostRegistrationResult> GetZeroCostRegistrationResultAsync(InstantEnrollmentZeroCostRegistration zeroCostRegistration)
        {
            InstEnrollZeroCostRgstrtnRequest request = null;
            List<ZeroCostStudentPhones> studentPhones = null;
            InstantEnrollmentZeroCostRegistrationResult zeroCostRegistrationResult = null;

            if (zeroCostRegistration == null)
            {
                throw new ArgumentNullException("zeroCostRegistration", "the zero cost registration parameter is required in order to register");
            }

            if (zeroCostRegistration.ProposedSections == null || zeroCostRegistration.ProposedSections.Count == 0)
            {
                throw new ArgumentNullException("zeroCostRegistration.ProposedSections", "the proposed sections zero cost registration parameter requires at least one section in order to register");
            }

            try
            {
                var sectionsToRegister = zeroCostRegistration.ProposedSections
                    .Where(x => x != null)
                    .Select(x => new ZeroCostProposedSectionInformation()
                    {
                        ProposedSection = x.SectionId,
                        ProposedSectionCredit = x.AcademicCredits,
                        ProposedSectionRegReason = x.RegistrationReason,
                        ProposedSectionMktgSrc = x.MarketingSource
                    }).ToList();

                if (zeroCostRegistration.PersonDemographic != null && zeroCostRegistration.PersonDemographic.PersonPhones != null)
                {
                    studentPhones = zeroCostRegistration.PersonDemographic.PersonPhones
                        .Where(x => x != null)
                        .Select(x => new ZeroCostStudentPhones()
                        {
                            StudentPhoneNumber = x.Number,
                            StudentPhoneExtension = x.Extension,
                            StudentPhoneType = x.TypeCode
                        })
                    .ToList();
                }

                request = new InstEnrollZeroCostRgstrtnRequest()
                {
                    StudentId = zeroCostRegistration.PersonId,
                    AcadProgram = zeroCostRegistration.AcademicProgram,
                    Catalog = zeroCostRegistration.Catalog,
                    ZeroCostStudentPhones = studentPhones,
                    ZeroCostProposedSectionInformation = sectionsToRegister
                };

                if (zeroCostRegistration.PersonDemographic != null)
                {
                    request.StudentEmailAddress = zeroCostRegistration.PersonDemographic.EmailAddress;
                    request.StudentPrefix = zeroCostRegistration.PersonDemographic.Prefix;
                    request.StudentGivenName = zeroCostRegistration.PersonDemographic.FirstName;
                    request.StudentMiddleName = zeroCostRegistration.PersonDemographic.MiddleName;
                    request.StudentFamilyName = zeroCostRegistration.PersonDemographic.LastName;
                    request.StudentSuffix = zeroCostRegistration.PersonDemographic.Suffix;
                    request.StudentAddress = zeroCostRegistration.PersonDemographic.AddressLines != null ? zeroCostRegistration.PersonDemographic.AddressLines.ToList() : new List<string>();
                    request.StudentCity = zeroCostRegistration.PersonDemographic.City;
                    request.StudentState = zeroCostRegistration.PersonDemographic.State;
                    request.StudentPostalCode = zeroCostRegistration.PersonDemographic.ZipCode;
                    request.StudentCounty = zeroCostRegistration.PersonDemographic.CountyCode;
                    request.StudentCountry = zeroCostRegistration.PersonDemographic.CountryCode;
                    request.StudentBirthDate = zeroCostRegistration.PersonDemographic.BirthDate;
                    request.StudentEthnics = zeroCostRegistration.PersonDemographic.EthnicGroups.ToList();
                    request.StudentGender = zeroCostRegistration.PersonDemographic.Gender;
                    request.StudentCitizenship = zeroCostRegistration.PersonDemographic.CitizenshipCountryCode;
                    request.StudentRacialGroups = zeroCostRegistration.PersonDemographic.RacialGroups.ToList();
                    request.StudentTaxId = zeroCostRegistration.PersonDemographic.GovernmentId;
                }

                request.EducationalGoal = zeroCostRegistration.EducationalGoal;

                //send request by calling CTX
                var response = await transactionInvoker.ExecuteAsync<InstEnrollZeroCostRgstrtnRequest, InstEnrollZeroCostRgstrtnResponse>(request);

                //convert response to InstantEnrollmentZeroCostRegistrationResult entity
                if (response != null)
                {
                    var instantEnrollmentRegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
                    if (response.ZeroCostRegisteredSectionInformation != null)
                    {
                        var sects = response.ZeroCostRegisteredSectionInformation
                            .Where(x => x != null && !String.IsNullOrEmpty(x.RegisteredSection))
                            .Select(x => new InstantEnrollmentRegistrationBaseRegisteredSection(x.RegisteredSection, x.RegisteredSectionCost))
                            .ToList();
                        if (sects != null)
                        {
                            instantEnrollmentRegisteredSections.AddRange(sects);
                        }
                    }

                    var instantEnrollmentRegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
                    if (response.ZeroCostRegistrationMessages != null)
                    {
                        var msgs = response.ZeroCostRegistrationMessages
                            .Where(x => x != null && !String.IsNullOrEmpty(x.Message))
                            .Select(x => new InstantEnrollmentRegistrationBaseMessage(x.MessageSection, x.Message))
                            .ToList();
                        if (msgs != null)
                        {
                            instantEnrollmentRegistrationMessages.AddRange(msgs);
                        }
                    }

                    // CTX doesn't return a person ID
                    if (string.IsNullOrWhiteSpace(response.StudentId))
                    {
                        logger.Info("There was no Person Id returned from the InstEnrollZeroCostRgstrtn CTX.");
                    }
                    else
                    {
                        // CTX doesn't return a username for a person
                        if (string.IsNullOrWhiteSpace(response.AUserName))
                        {
                            logger.Info("There was no Username returned from the InstEnrollZeroCostRgstrtn CTX.");
                        }
                    }

                    zeroCostRegistrationResult = new InstantEnrollmentZeroCostRegistrationResult(response.ErrorOccurred,
                        instantEnrollmentRegisteredSections,
                        instantEnrollmentRegistrationMessages,
                        response.StudentId,
                        response.AUserName);
                }
                else
                {
                    logger.Info("InstantEnrollmentZeroCostRegistrationResult the response from the InstEnrollZeroCostRgstrtn CTX is null.");
                }

            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to process a zero cost instant enrollment registration for person.");
                logger.Error(ex, exceptionMsg);
                throw;
            }


            return zeroCostRegistrationResult;
        }

        /// <summary>
        /// Registers a student for classes, creating the student if necessary, and pays the cost of the classes with an electronic transfer.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentEcheckRegistration"/> containing the information necessary to register and pay for classes.</param>
        /// <returns>A <see cref=InstantEnrollmentEcheckRegistrationResult"/> containing the results of the registration.</returns>
        public async Task<InstantEnrollmentEcheckRegistrationResult> GetEcheckRegistrationResultAsync(InstantEnrollmentEcheckRegistration criteria)
        {
            List<EcheckStudentPhones> studentPhones = null;
            InstantEnrollmentEcheckRegistrationResult echeckRegistrationResult = null;
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (criteria.ProposedSections == null || criteria.ProposedSections.Count == 0)
            {
                throw new ArgumentException("criteria", "proposed registration parameter should have proposed sections to register for and retrieve associated cost");
            }

            var sectionsToRegister = criteria.ProposedSections.Where(x => x != null).Select(x => new EcheckProposedSectionInformation()
            {
                ProposedSection = x.SectionId,
                ProposedSectionCredit = x.AcademicCredits,
                ProposedSectionRegReason = x.RegistrationReason,
                ProposedSectionMktgSrc = x.MarketingSource
            }).ToList<EcheckProposedSectionInformation>();

            if (criteria.PersonDemographic != null && criteria.PersonDemographic.PersonPhones != null)
            {
                studentPhones = criteria.PersonDemographic.PersonPhones
                    .Where(x => x != null)
                    .Select(x => new EcheckStudentPhones()
                    {
                        StudentPhoneNumber = x.Number,
                        StudentPhoneExtension = x.Extension,
                        StudentPhoneType = x.TypeCode
                    })
                .ToList<EcheckStudentPhones>();
            }
            var request = new InstEnrollEcheckRgstrtnRequest()
            {
                StudentId = criteria.PersonId,
                AcadProgram = criteria.AcademicProgram,
                Catalog = criteria.Catalog,
                EcheckStudentPhones = studentPhones,
                EcheckProposedSectionInformation = sectionsToRegister
            };

            if (criteria.PersonDemographic != null)
            {
                request.StudentEmailAddress = criteria.PersonDemographic.EmailAddress;
                request.StudentPrefix = criteria.PersonDemographic.Prefix;
                request.StudentGivenName = criteria.PersonDemographic.FirstName;
                request.StudentMiddleName = criteria.PersonDemographic.MiddleName;
                request.StudentFamilyName = criteria.PersonDemographic.LastName;
                request.StudentSuffix = criteria.PersonDemographic.Suffix;
                request.StudentAddress = criteria.PersonDemographic.AddressLines.ToList<string>();
                request.StudentCity = criteria.PersonDemographic.City;
                request.StudentState = criteria.PersonDemographic.State;
                request.StudentPostalCode = criteria.PersonDemographic.ZipCode;
                request.StudentCounty = criteria.PersonDemographic.CountyCode;
                request.StudentCountry = criteria.PersonDemographic.CountryCode;
                request.StudentBirthDate = criteria.PersonDemographic.BirthDate;
                request.StudentEthnics = criteria.PersonDemographic.EthnicGroups.ToList();
                request.StudentGender = criteria.PersonDemographic.Gender;
                request.StudentCitizenship = criteria.PersonDemographic.CitizenshipCountryCode;
                request.StudentRacialGroups = criteria.PersonDemographic.RacialGroups.ToList();
                request.StudentTaxId = criteria.PersonDemographic.GovernmentId;
            }
            request.EducationalGoal = criteria.EducationalGoal;

            // add the financial information
            request.PaymentAmt = criteria.PaymentAmount;
            request.ConvenienceFeeAmt = criteria.ConvenienceFeeAmount;
            request.ConvenienceFeeGlNo = criteria.ConvenienceFeeGlAccount;
            request.ConvenienceFeeDescr = criteria.ConvenienceFeeDesc;
            request.PaymentMethod = criteria.PaymentMethod;
            request.NameOnBankAccount = criteria.BankAccountOwner;
            request.RoutingNumber = criteria.BankAccountRoutingNumber;
            request.AccountNumber = criteria.BankAccountNumber;
            request.CheckNumber = criteria.BankAccountCheckNumber;
            request.AccountType = criteria.BankAccountType;
            request.ProviderAccount = criteria.ProviderAccount;
            // add the payer information
            request.DriversLicense = criteria.GovernmentId;
            request.LicenseState = criteria.GovernmentIdState;
            request.PayerEmailAddress = criteria.PayerEmailAddress;
            request.PayerAddress = criteria.PayerAddress;
            request.PayerCity = criteria.PayerCity;
            request.PayerState = criteria.PayerState;
            request.PayerPostalCode = criteria.PayerPostalCode;

            //send request by calling CTX
            var response = await transactionInvoker.ExecuteAsync<InstEnrollEcheckRgstrtnRequest, InstEnrollEcheckRgstrtnResponse>(request);
            //convert response to InstantEnrollmentEcheckRegistrationResult entity
            //convert registered sections in response to registered sections for instant enrollment entity
            if (response != null)
            {
                List<InstantEnrollmentRegistrationBaseRegisteredSection> echeckRegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
                if (response.EcheckRegisteredSectionInformation != null)
                {
                    var sects = response.EcheckRegisteredSectionInformation
                        .Where(x => x != null && !String.IsNullOrEmpty(x.RegisteredSection))
                        .Select(x => new InstantEnrollmentRegistrationBaseRegisteredSection(x.RegisteredSection, x.RegisteredSectionCost))
                        .ToList();
                    if (sects != null)
                    {
                        echeckRegisteredSections.AddRange(sects);
                    }
                }
                //convert response messages to instant enrollment messages entity
                List<InstantEnrollmentRegistrationBaseMessage> instantEnrollmentRegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
                if (response.EcheckRegistrationMessages != null)
                {
                    var msgs = response.EcheckRegistrationMessages
                        .Where(x => x != null && !String.IsNullOrEmpty(x.Message))
                        .Select(x => new InstantEnrollmentRegistrationBaseMessage(x.MessageSection, x.Message))
                        .ToList();
                    if (msgs != null)
                    {
                        instantEnrollmentRegistrationMessages.AddRange(msgs);
                    }
                }

                // CTX doesn't return a person ID
                if (string.IsNullOrWhiteSpace(response.StudentId))
                {
                    logger.Info("There was no Person Id returned from the InstEnrollEcheckRgstrtn CTX.");
                }
                else
                {
                    // CTX doesn't return a username for a person
                    if (string.IsNullOrWhiteSpace(response.AUserName))
                    {
                        logger.Info("There was no Username returned from the InstEnrollEcheckRgstrtn CTX.");
                    }
                }
                echeckRegistrationResult = new InstantEnrollmentEcheckRegistrationResult(response.ErrorOccurred,
                    echeckRegisteredSections, 
                    instantEnrollmentRegistrationMessages, 
                    response.StudentId,
                    response.CashReceiptId, 
                    response.AUserName);
            }
            else
            {
                logger.Info("InstantEnrollmentEcheckRegistrationResult response from CTX is null, hence will return nothing");
            }

            return echeckRegistrationResult;

        }

        /// <summary>
        /// Starts a payment gateway transaction for instant enrollment. 
        /// Starting a payment gateway transaction involves creating an EC.PAY.TRANS record and returning a url to redirect the user to the external
        /// payment provider.
        /// In the case of instant enrollment the CTX also registers the student and creates the person and student records if needed.
        /// Call backs to other CTXs from the Payment Gateway web server will complete the interaction after the user either completes or cancels the payment
        /// at the external provider. Garbage collection WAGC will cancel the interaction if a timeout elapses before the user completes the interaction at the 
        /// external payment provider. Either through garbage collection or a callback from the payment gateway web server, the person and student creation as 
        /// well as the registration will be reversed if the interaction is canceled.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentPaymentGatewayRegistration"/> containing the information necessary to register and pay for classes.</param>
        /// 
        /// <returns>A <see cref="InstantEnrollmentStartPaymentGatewayRegistrationResult"/> containing any error messages or the payment provider URL to which to redirect the user.</returns>
        public async Task<InstantEnrollmentStartPaymentGatewayRegistrationResult> StartInstantEnrollmentPaymentGatewayTransactionAsync(InstantEnrollmentPaymentGatewayRegistration proposedRegistration)
        {
            InstantEnrollmentStartPaymentGatewayRegistrationResult result = null;

            if (proposedRegistration == null)
            {
                throw new ArgumentNullException("proposedRegistration");
            }

            var startPaymentGatewayRequest = new InstantEnrollmentPaymentGatewayRegRequest();
            startPaymentGatewayRequest.AcadProgram = proposedRegistration.AcademicProgram;
            startPaymentGatewayRequest.Catalog = proposedRegistration.Catalog;
            startPaymentGatewayRequest.EducationalGoal = proposedRegistration.EducationalGoal;
            startPaymentGatewayRequest.PaymentAmt = proposedRegistration.PaymentAmount;
            startPaymentGatewayRequest.PaymentMethod = proposedRegistration.PaymentMethod;
            startPaymentGatewayRequest.GlDistribution = proposedRegistration.GlDistribution;
            startPaymentGatewayRequest.ProviderAccount = proposedRegistration.ProviderAccount;
            startPaymentGatewayRequest.ConvenienceFeeAmt = proposedRegistration.ConvenienceFeeAmount;
            startPaymentGatewayRequest.ConvenienceFeeDesc = proposedRegistration.ConvenienceFeeDesc;
            startPaymentGatewayRequest.ConvenienceFeeGlNo = proposedRegistration.ConvenienceFeeGlAccount;
            var sectionsToRegister = proposedRegistration.ProposedSections.Where(x => x != null).Select(x => new PmtGatewayProposedSectionInformation()
            {
                ProposedSections = x.SectionId,
                ProposedSectionCredits = (decimal?)x.AcademicCredits,
                ProposedSectionMktgSrc = x.MarketingSource,
                ProposedSectionRegReason = x.RegistrationReason
            }).ToList<PmtGatewayProposedSectionInformation>();
            startPaymentGatewayRequest.PmtGatewayProposedSectionInformation = sectionsToRegister;
            startPaymentGatewayRequest.ReturnUrl = proposedRegistration.ReturnUrl;
            startPaymentGatewayRequest.StudentId = proposedRegistration.PersonId;
            if (proposedRegistration.PersonDemographic != null)
            {
                startPaymentGatewayRequest.StudentAddress = proposedRegistration.PersonDemographic.AddressLines.ToList<string>();
                startPaymentGatewayRequest.StudentBirthDate = proposedRegistration.PersonDemographic.BirthDate;
                startPaymentGatewayRequest.StudentCitizenship = proposedRegistration.PersonDemographic.CitizenshipCountryCode;
                startPaymentGatewayRequest.StudentCity = proposedRegistration.PersonDemographic.City;
                startPaymentGatewayRequest.StudentCountry = proposedRegistration.PersonDemographic.CountryCode;
                startPaymentGatewayRequest.StudentCounty = proposedRegistration.PersonDemographic.CountyCode;
                startPaymentGatewayRequest.StudentEmailAddress = proposedRegistration.PersonDemographic.EmailAddress;
                startPaymentGatewayRequest.StudentEthnics = proposedRegistration.PersonDemographic.EthnicGroups.ToList();
                startPaymentGatewayRequest.StudentFamilyName = proposedRegistration.PersonDemographic.LastName;
                startPaymentGatewayRequest.StudentGender = proposedRegistration.PersonDemographic.Gender;
                startPaymentGatewayRequest.StudentGivenName = proposedRegistration.PersonDemographic.FirstName;
                startPaymentGatewayRequest.StudentMiddleName = proposedRegistration.PersonDemographic.MiddleName;
                startPaymentGatewayRequest.StudentPostalCode = proposedRegistration.PersonDemographic.ZipCode;
                startPaymentGatewayRequest.StudentPrefix = proposedRegistration.PersonDemographic.Prefix;
                startPaymentGatewayRequest.StudentRacialGroups = proposedRegistration.PersonDemographic.RacialGroups.ToList();
                startPaymentGatewayRequest.StudentState = proposedRegistration.PersonDemographic.State;
                startPaymentGatewayRequest.StudentSuffix = proposedRegistration.PersonDemographic.Suffix;
                startPaymentGatewayRequest.StudentTaxId = proposedRegistration.PersonDemographic.GovernmentId;

                if (proposedRegistration.PersonDemographic != null && proposedRegistration.PersonDemographic.PersonPhones != null)
                {
                    var studentPhones = proposedRegistration.PersonDemographic.PersonPhones.Where(x => x != null).Select(x => new PmtGatewayStudentPhones()
                    {
                        StudentPhoneNumbers = x.Number,
                        StudentPhoneExtensions = x.Extension,
                        StudentPhoneTypes = x.TypeCode
                    }).ToList<PmtGatewayStudentPhones>();
                    startPaymentGatewayRequest.PmtGatewayStudentPhones = studentPhones;
                }
            }

            var response = await transactionInvoker.ExecuteAsync<InstantEnrollmentPaymentGatewayRegRequest, InstantEnrollmentPaymentGatewayRegResponse>(startPaymentGatewayRequest);

            if (response != null)
            {
                List<string> messages = null;
                if (response.PmtGatewayRegistrationMessages != null)
                {
                    messages = response.PmtGatewayRegistrationMessages.Where(m => m != null).Select(m => m.Messages).ToList<string>();
                }
                result = new InstantEnrollmentStartPaymentGatewayRegistrationResult(messages, response.ExtlPaymentUrl);
            }
            else
            {
                throw new Exception("InstantEnrollmentProposedRegistrationResult response from CTX is null.");
            }

            return result;
        }

        /// <summary>
        /// Retrieves instant enrollment payment acknowledgement paragraph text for a given <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/></param>
        /// <returns>Instant enrollment payment acknowledgement paragraph text</returns>
        public async Task<IEnumerable<string>> GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(InstantEnrollmentPaymentAcknowledgementParagraphRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "An instant enrollment payment acknowledgement paragraph request is required to get instant enrollment payment acknowledgement paragraph text.");
            }
            List<string> paragraphText = new List<string>();
            try
            {
                var ctxRequest = new InstantEnrollmentBuildAcknowledgementParagraphRequest()
                {
                    StudentId = request.PersonId,
                    CashRcptsId = request.CashReceiptId
                };
                    
                var ctxResponse = await transactionInvoker.ExecuteAsync<InstantEnrollmentBuildAcknowledgementParagraphRequest, InstantEnrollmentBuildAcknowledgementParagraphResponse>(ctxRequest);
                // CTX returns null object
                if (ctxResponse == null)
                {
                    throw new ApplicationException("Request to retrieve instant enrollment payment acknowledgement paragraph text returned a null response.");
                }
                // CTX returns error
                if (!string.IsNullOrEmpty(ctxResponse.AError))
                {
                    string errorTextMsg = string.Format("Request to retrieve instant enrollment payment acknowledgement paragraph text returned error for person {0}", request.PersonId);
                    if (!string.IsNullOrEmpty(request.CashReceiptId))
                    {
                        errorTextMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                    }
                    errorTextMsg += string.Format(": {0}", ctxResponse.AError);
                    logger.Error(errorTextMsg);
                    throw new ApplicationException(errorTextMsg);
                }
                // CTX returns null paragraph text
                if (ctxResponse.ParaText == null)
                {
                    string nullTextMsg = string.Format("Request to retrieve instant enrollment payment acknowledgement paragraph text returned null paragraph text for person {0}", request.PersonId);
                    if (!string.IsNullOrEmpty(request.CashReceiptId))
                    {
                        nullTextMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                    }
                    logger.Info(nullTextMsg);
                    return paragraphText;
                }
                // CTX returns empty paragraph text
                if (!ctxResponse.ParaText.Any())
                {
                    string emptyTextMsg = string.Format("Request to retrieve instant enrollment payment acknowledgement paragraph text returned empty paragraph text for person {0}", request.PersonId);
                    if (!string.IsNullOrEmpty(request.CashReceiptId))
                    {
                        emptyTextMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                    }
                    logger.Info(emptyTextMsg);
                    return paragraphText;
                }
                paragraphText.AddRange(ctxResponse.ParaText);
                return paragraphText;
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to retrieve instant enrollment payment acknowledgement paragraph text for person {0}", request.PersonId);
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                }
                logger.Error(ex, exceptionMsg);
                throw;
            }
        }

        /// <summary>
        /// Query persons matching the criteria using the ELF duplicate checking criteria configured for Instant Enrollment.
        /// </summary>
        /// <param name="criteria">The <see cref="PersonMatchCriteriaInstantEnrollment">criteria</see> to use when searching for people</param>
        /// <returns>Result of a person biographic/demographic matching inquiry for Instant Enrollment</returns>
        public async Task<InstantEnrollmentPersonMatchResult> GetMatchingPersonResultsInstantEnrollmentAsync(PersonMatchCriteriaInstantEnrollment criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "Criteria required to query");

            var matchRequest = new Transactions.GetPersonMatchInstEnrlRequest();
            matchRequest.ABirthDate = criteria.BirthDate;
            matchRequest.ACitizenshipCountry = criteria.CitizenshipCountryCode;
            matchRequest.ACity = criteria.City;
            matchRequest.ACountry = criteria.CountryCode;
            matchRequest.ACounty = criteria.CountyCode;
            matchRequest.AEmailAddress = criteria.EmailAddress;
            matchRequest.AFirstName = criteria.FirstName;
            matchRequest.AGender = criteria.Gender;
            matchRequest.AlAddressLines = criteria.AddressLines.ToList<string>();
            matchRequest.ALastName = criteria.LastName;
            matchRequest.AMiddleName = criteria.MiddleName;
            matchRequest.APrefix = criteria.Prefix;
            matchRequest.ASsn = criteria.GovernmentId;
            matchRequest.AState = criteria.State;
            matchRequest.ASuffix = criteria.Suffix;
            matchRequest.AZip = criteria.ZipCode;

            var matchResponse = await transactionInvoker.ExecuteAsync<Transactions.GetPersonMatchInstEnrlRequest, Transactions.GetPersonMatchInstEnrlResponse>(matchRequest);
            if (matchResponse == null)
            {
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (matchResponse.AlErrorMessages.Any() == true)
            {
                var errorMessage = "Error(s) occurred during person matching:";
                errorMessage += string.Join(Environment.NewLine, matchResponse.AlErrorMessages);
                logger.Error(errorMessage);
                throw new InvalidOperationException("An error occurred during person matching");
            }
            if (matchResponse.ADuplicateGovtId == true)
            {
                var duplicateGovtIdMessage = "The government ID the user submitted is already assigned to another user in the system.";
                logger.Info(duplicateGovtIdMessage);
            }
            var result = new InstantEnrollmentPersonMatchResult(null, matchResponse.ADuplicateGovtId);
            if (matchResponse.MatchedPersons != null)
            {
                var personMatches = matchResponse.MatchedPersons.Where(x => x != null).Select(x => new PersonMatchResult(x.AlMatchedPersonIds, x.AlMatchedScores, x.AlMatchedCategories)).ToList();
                result = new InstantEnrollmentPersonMatchResult(personMatches, matchResponse.ADuplicateGovtId);
            }
            return result;
        }

        /// <summary>
        /// Retrieves instant enrollment payment cash receipt acknowledgement for a given <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/></param>
        /// <returns>the cash receipt and sections for an instant enrollment payment</returns>
        public async Task<InstantEnrollmentCashReceiptAcknowledgement> GetInstantEnrollmentCashReceiptAcknowledgementAsync(InstantEnrollmentCashReceiptAcknowledgementRequest request)
        {
            try
            {
                // Build the input arguments to the transaction
                var ctxRequest = new InstEnrollGetCashReceiptAckInfoRequest()
                {
                    InEcPayTransId = request.TransactionId,
                    IoCashRcptsId = request.CashReceiptId,
                };

                var ctxResponse = await transactionInvoker.ExecuteAsync<InstEnrollGetCashReceiptAckInfoRequest, InstEnrollGetCashReceiptAckInfoResponse>(ctxRequest);

                // CTX returns null object
                if (ctxResponse == null)
                {
                    throw new ApplicationException("The request to retrieve an instant enrollment cash receipt acknowledgement returned a null response.");
                }

                // throw exception when CTX returns a message indicating there is an exeption in the data
                if (!string.IsNullOrEmpty(ctxResponse.OutExceptionMessage))
                {
                    string errorTextMsg = "The request to retrieve instant enrollment cash receipt acknowledgement returned an error";
                    if (!string.IsNullOrEmpty(request.TransactionId))
                    {
                        errorTextMsg += string.Format(" for e-commerce transaction id {0}", request.TransactionId);
                    }
                    if (!string.IsNullOrEmpty(request.CashReceiptId))
                    {
                        errorTextMsg += string.Format(" for cash receipt id {0}", request.CashReceiptId);
                    }
                    errorTextMsg += string.Format(": {0}", ctxResponse.OutExceptionMessage);
                    logger.Error(errorTextMsg);
                    throw new ApplicationException(errorTextMsg);
                }

                return MapCashReceiptAckInfoResponseToCashReceiptAcknowledgement(ctxResponse);
            }
            catch (Exception ex)
            {
                string exceptionMsg = "An error occurred while attempting to retrieve an instant enrollment cash receipt acknowledgement";
                if (!string.IsNullOrEmpty(request.TransactionId))
                {
                    exceptionMsg += string.Format(" for e-commerce transaction id {0}", request.TransactionId);
                }
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt id {0}", request.CashReceiptId);
                }
                logger.Error(ex, exceptionMsg);
                throw;
            }
        }

        private InstantEnrollmentCashReceiptAcknowledgement MapCashReceiptAckInfoResponseToCashReceiptAcknowledgement(InstEnrollGetCashReceiptAckInfoResponse cashReceiptAckInfoResponse)
        {
            var cashReceiptAcknowledgement = new InstantEnrollmentCashReceiptAcknowledgement();

            // Base receipt information
            cashReceiptAcknowledgement.CashReceiptsId = cashReceiptAckInfoResponse.IoCashRcptsId;
            cashReceiptAcknowledgement.ReceiptNo = cashReceiptAckInfoResponse.OutRcptNo;
            cashReceiptAcknowledgement.ReceiptDate = cashReceiptAckInfoResponse.OutRcptDate;
            cashReceiptAcknowledgement.ReceiptTime = cashReceiptAckInfoResponse.OutRcptTime;
            cashReceiptAcknowledgement.ReceiptPayerId = cashReceiptAckInfoResponse.OutRcptPayerId;
            cashReceiptAcknowledgement.ReceiptPayerName = cashReceiptAckInfoResponse.OutRcptPayerName;

            // Merchant information
            cashReceiptAcknowledgement.MerchantNameAddress = cashReceiptAckInfoResponse.OutMerchantNameAddr;
            cashReceiptAcknowledgement.MerchantPhone = cashReceiptAckInfoResponse.OutMerchantPhone;
            cashReceiptAcknowledgement.MerchantEmail = cashReceiptAckInfoResponse.OutMerchantEmail;


            // e-commerce processing status
            switch (cashReceiptAckInfoResponse.OutEcProcStatus)
            {
                case "S":
                    {
                        cashReceiptAcknowledgement.Status = EcommerceProcessStatus.Success;
                        break;
                    }
                case "F":
                    {
                        cashReceiptAcknowledgement.Status = EcommerceProcessStatus.Failure;
                        break;
                    }
                case "C":
                    {
                        cashReceiptAcknowledgement.Status = EcommerceProcessStatus.Canceled;
                        break;
                    }
                default:
                    {
                        cashReceiptAcknowledgement.Status = EcommerceProcessStatus.None;
                        break;
                    }
            };


            // username and any creation errors
            cashReceiptAcknowledgement.Username = cashReceiptAckInfoResponse.OutNewLoginId;
            cashReceiptAcknowledgement.UsernameCreationErrors = cashReceiptAckInfoResponse.OutLoginCreationErrors;


            // Convenience fees
            if (cashReceiptAckInfoResponse.OutConvenienceFees != null)
            {
                foreach (OutConvenienceFees fee in cashReceiptAckInfoResponse.OutConvenienceFees)
                {
                    cashReceiptAcknowledgement.ConvenienceFees.Add(
                        new Domain.Student.Entities.InstantEnrollment.ConvenienceFee()
                        {
                            Code = fee.OutConvFeeCode,
                            Description = fee.OutConvFeeDesc,
                            Amount = fee.OutConvFeeAmt
                        }
                     );
                }
            }

            // Payments tendered
            if (cashReceiptAckInfoResponse.OutPaymentMethods != null)
            {
                foreach (OutPaymentMethods payment in cashReceiptAckInfoResponse.OutPaymentMethods)
                {
                    cashReceiptAcknowledgement.PaymentMethods.Add(
                        new Domain.Student.Entities.InstantEnrollment.PaymentMethod()
                        {
                            PayMethodCode = payment.OutRcptPayMethods,
                            PayMethodDescription = payment.OutRcptPayMethodDescs,
                            ControlNumber = payment.OutRcptControlNos,
                            ConfirmationNumber = payment.OutRcptConfirmNos,
                            TransactionNumber = payment.OutRcptTransNos,
                            TransactionDescription = payment.OutRcptTransDescs,
                            TransactionAmount = payment.OutRcptTransAmts
                        }
                     );
                }
            }

            // Successfully registered sections
            if (cashReceiptAckInfoResponse.RegisteredSections != null)
            {
                foreach (var registeredSection in cashReceiptAckInfoResponse.RegisteredSections)
                {
                    cashReceiptAcknowledgement.RegisteredSections.Add(
                        new InstantEnrollmentRegistrationPaymentGatewayRegisteredSection(
                            sectionId: registeredSection.OutRegisteredSectionIds,
                            sectionCost: registeredSection.OutRegisteredSectionCosts,
                            academicCredits: registeredSection.OutRegisteredSectionCredits,
                            ceus: registeredSection.OutRegisteredSectionCeus)
                     );
                }
            }

            // Unsuccessfully registered sections
            if (cashReceiptAckInfoResponse.FailedSections != null)
            {
                foreach (var failedSection in cashReceiptAckInfoResponse.FailedSections)
                {
                    cashReceiptAcknowledgement.FailedSections.Add(
                        new InstantEnrollmentRegistrationPaymentGatewayFailedSection(
                            sectionId: failedSection.OutFailedSectionIds,
                            message: failedSection.OutFailedSectionMessages)
                     );
                }
            }

            return cashReceiptAcknowledgement;
        }

    }
}



