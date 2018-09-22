/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PayableDepositDirective
    {
        #region PROPERTIES
        /// <summary>
        /// The unique system-generated id of this PayableDepositDirective
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The unique Id of the payee, either a Person or an Organization associated with this PayableDepositDirective
        /// </summary>
        public string PayeeId { get; private set; }

        /// The Routing Id of the bank associated with this PayableDepositDirective, if it's a United States Bank. 
        /// </summary>
        public string RoutingId
        {
            get
            {
                return routingId;
            }
            set
            {
                var routingString = value;
                if (routingString.Count() != 9)
                {
                    throw new ArgumentOutOfRangeException("Routing Id must be 9 characters.");
                }

                int result;
                if (!int.TryParse(routingString, out result))
                {
                    throw new ArgumentException("Routing Id must contain only numeric characters");
                }

                var checkSum = 0;

                for (int i = 0; i < routingString.Count(); i += 3)
                {
                    checkSum += int.Parse(routingString.ElementAt(i).ToString()) * 3
                        + int.Parse(routingString.ElementAt(i + 1).ToString()) * 7 + int.Parse(routingString.ElementAt(i + 2).ToString());
                }

                if (checkSum != 0 && checkSum % 10 == 0)
                {
                    routingId = value;
                }
                else
                {
                    throw new ApplicationException(string.Format("The routingId is invalid {0}.", value));
                }
            }
        }
        private string routingId;

        /// <summary>
        /// The Institution Id of the bank associated with this PayableDepositDirective, if it's a Canadian Bank.
        /// Must be 3 characters long.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if not 3 characters</exception>
        public string InstitutionId
        {
            get
            {
                return institutionId;
            }
            set
            {
                var institutionIdString = value;
                if (institutionIdString.Count() != 3)
                {
                    throw new ArgumentOutOfRangeException("Institution Id must be 3 characters.");
                }
                institutionId = value;
            }
        }
        private string institutionId;

        /// <summary>
        /// The Branch Number of the bank associated with this PayableDepositDirective, if it's a Canadian Bank.
        /// Must be 5 characters long.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if not 5 characters long.</exception>
        public string BranchNumber
        {
            get
            {
                return branchNumber;
            }
            set
            {
                var branchNumberString = value;
                if (branchNumberString.Count() != 5)
                {
                    throw new ArgumentOutOfRangeException("Transit number must be 5 characters.");
                }
                branchNumber = value;
            }
        }
        private string branchNumber;

        /// <summary>
        /// The official name of the bank associated with this PayableDepositDirective
        /// </summary>
        public string BankName { get; private set; }

        /// <summary>
        /// The type of the bank account associated with this PayableDepositDirective
        /// </summary>
        public BankAccountType BankAccountType { get; private set; }

        /// <summary>
        /// The Bank Account Id of a new PayableDepositDirective or an updated Bank Account Id of an existing but unverified PayableDepositDirective.
        /// </summary>
        public string NewAccountId { get; private set; }

        /// <summary>
        /// The Last four characters of Bank Account Id of a new PayableDepositDirective or an updated Bank Account Id of an existing but unverified PayableDepositDirective.
        /// </summary>
        public string AccountIdLastFour { get; private set; }

        /// <summary>
        /// The user defined nickname of the bank account associated with this PayableDepositDirective. The length of the nickname is restricted to 50 characters.
        /// </summary>
        public string Nickname
        {
            get
            {
                return nickname;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // replace interword spaces with a single space and trim whitespace from ends
                    value = Regex.Replace(value, @"\s\s+", " ").Trim();

                    // throw exception in case of special characters
                    if (Regex.IsMatch(value, @"([<>])")) // we are disallowing angle brackets
                    {
                        var message = "Invalid character is in Nickname string";
                        throw new FormatException(message);
                    }
                }
                nickname = value;
            }
        }
        private string nickname;

        /// <summary>
        /// Indicates whether this bank account has been verified by the business office. "Verified" is also known as "prenoted" in back-office parlance.
        /// </summary>
        public bool IsVerified { get; private set; }

        /// <summary>
        /// Address Id associated with this PayableDepositDirective
        /// </summary>
        public string AddressId { get; private set; }

        /// <summary>
        /// The date this deposit should become active
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// The last date on which this deposit should be active
        /// </summary>
        public DateTime? EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("EndDate cannot be before than StartDate");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// Specifies whether this deposit is electronic
        /// </summary>
        public bool IsElectronicPaymentRequested { get; private set; }

        /// <summary>
        /// This record's timestamp
        /// </summary>
        public Timestamp Timestamp { get; private set; }
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Create a US PayableDepositDirective
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payeeId"></param>
        /// <param name="routingId"></param>
        /// <param name="bankName"></param>
        /// <param name="bankAccountType"></param>
        /// <param name="accountIdLastFour"></param>
        /// <param name="nickname"></param>
        /// <param name="isVerified"></param>
        /// <param name="addressId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isElectronicPaymentRequested"></param>
        /// <param name="timestamp"></param>
        public PayableDepositDirective(
            string id,
            string payeeId, 
            string routingId,
            string bankName,
            BankAccountType bankAccountType,
            string accountIdLastFour,
            string nickname,
            bool isVerified,
            string addresssId,
            DateTime startDate,
            DateTime? endDate,
            bool isElectronicPaymentRequested,
            Timestamp timestamp) 
        {
            if (string.IsNullOrEmpty(payeeId))
            {
                throw new ArgumentNullException("payeeId");
            }
            if (string.IsNullOrEmpty(routingId))
            {
                throw new ArgumentNullException("routingId");
            }

            this.Id = id;
            this.PayeeId = payeeId;
            this.RoutingId = routingId;
            this.BankName = bankName;
            this.BankAccountType = bankAccountType;
            this.AccountIdLastFour = accountIdLastFour;
            this.Nickname = nickname;
            this.IsVerified = isVerified;
            this.AddressId = string.IsNullOrWhiteSpace(addresssId) ? null : addresssId;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.IsElectronicPaymentRequested = isElectronicPaymentRequested;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Create a Canadian PayableDepositDirective
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payeeId"></param>
        /// <param name="institutionId"></param>
        /// <param name="branchNumber"></param>
        /// <param name="bankName"></param>
        /// <param name="bankAccountType"></param>
        /// <param name="accountIdLastFour"></param>
        /// <param name="nickname"></param>
        /// <param name="isVerified"></param>
        /// <param name="addressId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isElectronicPaymentRequested"></param>
        /// <param name="timestamp"></param>
        public PayableDepositDirective(
            string id,
            string payeeId,
            string institutionId,
            string branchNumber,
            string bankName,
            BankAccountType bankAccountType,
            string accountIdLastFour,
            string nickname,
            bool isVerified,
            string addresssId,
            DateTime startDate,
            DateTime? endDate,
            bool isElectronicPaymentRequested,
            Timestamp timestamp)
        {
            if (payeeId == null)
            {
                throw new ArgumentNullException("payeeId");
            }
            if (string.IsNullOrEmpty(institutionId))
            {
                throw new ArgumentNullException("institutionId");
            }

            if (string.IsNullOrEmpty(branchNumber))
            {
                throw new ArgumentNullException("branchNumber");
            }

            this.Id = id;
            this.PayeeId = payeeId;
            this.InstitutionId = institutionId;
            this.BranchNumber = branchNumber;
            this.BankName = bankName;
            this.BankAccountType = bankAccountType;
            this.AccountIdLastFour = accountIdLastFour;
            this.Nickname = nickname;
            this.IsVerified = isVerified;
            this.AddressId = string.IsNullOrWhiteSpace(addresssId) ? null : addresssId;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.IsElectronicPaymentRequested = isElectronicPaymentRequested;
            this.Timestamp = timestamp;
        }
        #endregion

        #region METHODS
        public void SetNewAccountId(string newAccountId)
        {
            if (string.IsNullOrWhiteSpace(newAccountId))
                throw new ArgumentNullException("newAccountId");

            this.NewAccountId = newAccountId;
        }

        /// <summary>
        /// Two PayableDepositDirectives are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var directive = obj as PayableDepositDirective;
            if (directive.Id == this.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the hash code of this PayableDepositDirective based on the Id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion
    }
}
