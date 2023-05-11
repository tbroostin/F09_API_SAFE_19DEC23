// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class AccountDueService : BaseCoordinationService, IAccountDueService
    {
        private readonly IAccountDueRepository _accountRepository;
        private readonly IAccountsReceivableRepository _arRepository;
        private readonly ITermRepository _termRepository;
        private readonly IFinanceConfigurationRepository _configRepository;
        private IEnumerable<Term> _terms;
        private IEnumerable<FinancialPeriod> _periods;
        private Ellucian.Colleague.Domain.Finance.Entities.DueDateOverrides _dueDateOverrides;

        public AccountDueService(IAdapterRegistry adapterRegistry, IAccountDueRepository accountRepository, IAccountsReceivableRepository arRepository, 
            ITermRepository termRepository, IFinanceConfigurationRepository configRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _accountRepository = accountRepository;
            _arRepository = arRepository;
            _termRepository = termRepository;
            _configRepository = configRepository;
            _periods = _configRepository.GetFinancialPeriods();
            _dueDateOverrides = _configRepository.GetDueDateOverrides();
        }

        private void CheckPermission(string studentId)
        {
            bool hasAdminPermission = HasPermission(FinancePermissionCodes.ViewStudentAccountActivity);

            // They're allowed to see another's data if they have the admin permission
            if (!CurrentUser.IsPerson(studentId) && !hasAdminPermission && !HasProxyAccessForPerson(studentId))
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }
        }

        public Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue GetAccountDue(string studentId)
        {
            CheckPermission(studentId);
            try
            {
                var accountDueItems = _accountRepository.Get(studentId);
                var colleagueTimezone = _configRepository.GetFinanceConfiguration().ColleagueTimezone;
                DueDateOverrideProcessor.OverrideTermDueDates(_dueDateOverrides, accountDueItems, colleagueTimezone);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue>();
                var accountDueDto = adapter.MapToType(accountDueItems);

                // Get the deposits due for this student with an outstanding balance
                var depositDueItems = _arRepository.GetDepositsDue(studentId).Where(x => x.Balance > 0);
                if (depositDueItems.Count() > 0)
                {
                    string distribution = _arRepository.GetDistribution(studentId, String.Empty, "DD");
                    AddDepositsDue(depositDueItems, accountDueDto, distribution);
                }
                accountDueDto.Balance = accountDueDto.AccountTerms.Sum(at => at.Amount);

                return accountDueDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }

        }

        public Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod GetAccountDuePeriod(string studentId)
        {
            CheckPermission(studentId);

            try
            {
                var accountDueItems = _accountRepository.GetPeriods(studentId);

                DueDateOverrideProcessor.OverridePeriodDueDates(_dueDateOverrides, accountDueItems);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod>();
                var accountDueDto = adapter.MapToType(accountDueItems);
                accountDueDto.Balance = 0m;

                // Get the deposits due for this student with an outstanding balance
                var depositDueItems = _arRepository.GetDepositsDue(studentId).Where(x => x.Balance > 0);
                if (depositDueItems.Count() > 0)
                {
                    string distribution = _arRepository.GetDistribution(studentId, String.Empty, "DD");
                    AddDepositsDue(depositDueItems.Where(x => x.DueDate.Date <= accountDueDto.Past.EndDate.Value.Date),
                        accountDueDto.Past, distribution);
                    AddDepositsDue(depositDueItems.Where(x => x.DueDate.Date >= accountDueDto.Current.StartDate.Value.Date && x.DueDate.Date <= accountDueDto.Current.EndDate.Value.Date),
                        accountDueDto.Current, distribution);
                    AddDepositsDue(depositDueItems.Where(x => x.DueDate.Date >= accountDueDto.Future.StartDate.Value.Date),
                        accountDueDto.Future, distribution);
                }
                accountDueDto.Past.Balance = accountDueDto.Past.AccountTerms.Sum(at => at.Amount);
                accountDueDto.Current.Balance = accountDueDto.Current.AccountTerms.Sum(at => at.Amount);
                accountDueDto.Future.Balance = accountDueDto.Future.AccountTerms.Sum(at => at.Amount);

                accountDueDto.Balance = accountDueDto.Past.Balance + accountDueDto.Current.Balance + accountDueDto.Future.Balance;

                return accountDueDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        #region Private helper methods

        private void AddDepositsDue(
            IEnumerable<Ellucian.Colleague.Domain.Finance.Entities.DepositDue> depositDueItems,
            Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue accountDueDto,
            string distribution)
        {
            if ((depositDueItems.Count() == 0) || (accountDueDto == null))
            {
                return;
            }

            // Get the adapter to map the domain entity to the DTO for deposits due
            var depositDueAdapter = _adapterRegistry.GetAdapter<DepositDue, Ellucian.Colleague.Dtos.Finance.DepositDue>();

            bool newAccountTerm = false;
            // Process each deposit due separately
            foreach (var item in depositDueItems)
            {
                // Build the base DepositDue DTO
                var depositDueDto = depositDueAdapter.MapToType(item);
                // Add the distribution code to the DTO
                depositDueDto.Distribution = distribution;

                // Add the deposit due to the proper AccountTerm
                bool nonTermActivityPresent = accountDueDto.AccountTerms.Any(at => at.TermId == FinanceTimeframeCodes.NonTerm);
                string nonTermIdentifier = nonTermActivityPresent ? FinanceTimeframeCodes.NonTerm : "";
                int loc = accountDueDto.AccountTerms.FindIndex(t => t.TermId == (String.IsNullOrEmpty(item.TermId) ? nonTermIdentifier : item.TermId));
                if (loc >= 0)
                {
                    // Term exists - update existing AccountTerm
                    accountDueDto.AccountTerms[loc].DepositDueItems.Add(depositDueDto);
                    accountDueDto.AccountTerms[loc].Amount += item.Balance;
                }
                else
                {
                    // Term does not exist - add a new AccountTerm to the AccountDue
                    var term = new Ellucian.Colleague.Dtos.Finance.AccountDue.AccountTerm();
                    term.Amount += item.Balance;
                    term.TermId = item.TermId;
                    term.Description = GetTermDescription(item.TermId);
                    term.DepositDueItems.Add(depositDueDto);
                    accountDueDto.AccountTerms.Add(term);
                    newAccountTerm = true;
                }
            }
            // If a new account term was added, then resort the terms
            if (newAccountTerm)
            {
                accountDueDto.AccountTerms = accountDueDto.AccountTerms.OrderBy(x => GetTermSortOrder(x.TermId)).ToList();
            }
            return;
        }

        private string GetTermDescription(string termId)
        {
            string description = String.Empty;
            if (String.IsNullOrEmpty(termId))
            {
                description = "Other";
            }
            else
            {
                var term = GetTerm(termId);
                if (term != null)
                {
                    description = term.Description;
                }
            }
            return description;
        }

        private string GetTermSortOrder(string termId)
        {
            string order = null;
            if (String.IsNullOrEmpty(termId) || (termId == "NON-TERM"))
            {
                // Sort non-term entries to the end
                order += DateTime.MaxValue.ToString("s");               // Max date value
                order += "999";                                         // Max sequence number
                order += DateTime.MaxValue.ToString("s");               // Max date value
                order += "zzzzzzz";                                     // Max code value
            }
            else
            {
                var term = GetTerm(termId);
                if (term != null)
                {
                    order = term.StartDate.ToString("s");               // First is start date
                    order += term.Sequence.ToString().PadLeft(3, '0');  // Next is sequence number
                    order += term.EndDate.ToString("s");                // End date
                    order += term.Code;                                 // Term code
                }
            }

            return order;
        }

        private Term GetTerm(string termId)
        {
            Term term = null;
            if (!String.IsNullOrEmpty(termId))
            {
                if (_terms == null)
                {
                    _terms = _termRepository.Get();
                }
                var terms = _terms.Where(x => x.Code == termId);
                if (terms != null)
                {
                    term = terms.SingleOrDefault();
                }
            }

            return term;
        }

        #endregion
    }
}
