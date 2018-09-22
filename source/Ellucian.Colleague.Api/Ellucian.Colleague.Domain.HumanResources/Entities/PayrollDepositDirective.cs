/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayrollDepositDirective
    {
        #region PROPERTIES
        /// <summary>
        /// Database key of the record
        /// </summary>
        public string Id { get; private set;}
        /// <summary>
        /// Associated person or employee Id
        /// </summary>
        public string PersonId { get { return personId; } 
            private set 
            { 
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("personId");
                }
                personId = value;
            } 
        }
        /// <summary>
        /// Routing number of a US bank
        /// </summary>
        public string RoutingId { get { return routingId; } 
            private set
            {
                if(value == null)
                {
                    routingId = value;
                }
                int checkParse, i, checkSum = 0;                
                if(value.Length != 9)
                {
                    throw new ArgumentOutOfRangeException("Routing Id must be 9 characters.");
                }
                if(!Int32.TryParse(value, out checkParse))
                {
                    throw new ArgumentException("Routing Id must contain only numeric characters");
                }                            
                for (i = 0; i < value.Length; i += 3)
                {
                    checkSum += int.Parse(value.ElementAt(i).ToString()) * 3
                        + int.Parse(value.ElementAt(i + 1).ToString()) * 7 + int.Parse(value.ElementAt(i + 2).ToString());
                }
                if (checkSum != 0 && checkSum % 10 == 0)
                {
                    routingId = value;
                }
                else
                {
                    throw new ArgumentException(string.Format("The routingId is does not pass the check sum test {0}.", value));
                }
            } 
        }
        /// <summary>
        /// Institution numnber of a CA bank
        /// </summary>
        public string InstitutionId { get { return institutionId; }
            private set 
            {
                if (value != null && value.Length != 3) 
                {
                    throw new ArgumentOutOfRangeException("Institution Id must be three characters");
                }
                institutionId = value;
            }
        }
        /// <summary>
        /// Branch number of a CA bank
        /// </summary>
        public string BranchNumber { get { return branchNumber; } 
            private set 
            {
                if (value != null && value.Length != 5)
                {
                    throw new ArgumentOutOfRangeException("Branch number must be five characters");
                }
                branchNumber = value;
            } 
        }
        /// <summary>
        /// Name of a bank
        /// </summary>
        public string BankName { get; private set; }
        /// <summary>
        /// Account type of checking or savings
        /// </summary>
        public BankAccountType BankAccountType { get; private set; }
        /// <summary>
        /// Newly updated account number
        /// Only valid for creates or updates
        /// Needs to be set manually with exposed method
        /// </summary>
        public string NewAccountId { get; private set; }
        /// <summary>
        /// Last four characters of an account id
        /// </summary>
        public string AccountIdLastFour { get; private set; }
        /// <summary>
        /// User provided nickname of account
        /// </summary>
        public string Nickname { get; private set; }
        /// <summary>
        /// Whether the account has been prenoted
        /// </summary>
        public bool IsVerified { get; private set; }
        /// <summary>
        /// Deposit order priority
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Maximum monetary amount delegated to this deposit in a pay period
        /// </summary>
        public decimal? DepositAmount { get; private set; }
        /// <summary>
        /// Deposit start date
        /// </summary>
        public DateTime StartDate { get; private set; }
        /// <summary>
        /// Deposit end date, if existant
        /// </summary>
        public DateTime? EndDate { get; private set; }
        /// <summary>
        /// Timestamp metadata
        /// </summary>
        public Timestamp Timestamp { get; private set; }

        private string personId, routingId, institutionId, branchNumber;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Constructs a payroll deposit for a US bank account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        /// <param name="routingId"></param>
        /// <param name="bankName"></param>
        /// <param name="bankAccountType"></param>
        /// <param name="accountIdLastFour"></param>
        /// <param name="nickname"></param>
        /// <param name="isVerified"></param>
        /// <param name="priority"></param>
        /// <param name="depositAmount"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="timestamp"></param>
        public PayrollDepositDirective(
            string id, 
            string personId, 
            string routingId, 
            string bankName, 
            BankAccountType bankAccountType, 
            string accountIdLastFour,
            string nickname,
            bool isVerified,
            int priority,
            decimal? depositAmount,
            DateTime startDate,
            DateTime? endDate,
            Timestamp timestamp)
        {                        
            this.Id = id;
            this.PersonId = personId;
            this.RoutingId = routingId;
            this.BankName = bankName;
            this.BankAccountType = bankAccountType;
            this.AccountIdLastFour = accountIdLastFour;
            this.Nickname = nickname;
            this.IsVerified =  isVerified;
            this.Priority = priority;
            this.DepositAmount = depositAmount;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Timestamp = timestamp;
        }
        /// <summary>
        /// Constructs a payroll deposit for a Canadian bank account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        /// <param name="institutionId"></param>
        /// <param name="branchNumber"></param>
        /// <param name="bankName"></param>
        /// <param name="bankAccountType"></param>
        /// <param name="accountIdLastFour"
        /// <param name="nickname"></param>
        /// <param name="isVerified"></param>
        /// <param name="priority"></param>
        /// <param name="depositAmount"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="timestamp"></param>
        public PayrollDepositDirective(
            string id,
            string personId,
            string institutionId,
            string branchNumber,
            string bankName,
            BankAccountType bankAccountType,
            string accountIdLastFour,
            string nickname,
            bool isVerified,
            int priority,
            decimal? depositAmount,
            DateTime startDate,
            DateTime? endDate,
            Timestamp timestamp)
        {
            this.Id = id;
            this.PersonId = personId;
            this.InstitutionId = institutionId;
            this.BranchNumber = branchNumber;
            this.BankName = bankName;
            this.BankAccountType = bankAccountType;
            this.AccountIdLastFour = accountIdLastFour;
            this.Nickname = nickname;
            this.IsVerified = isVerified;
            this.Priority = priority;
            this.DepositAmount = depositAmount;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Timestamp = timestamp;
        }
        #endregion

        #region METHODS
        /// <summary>
        /// Sets the account id for the model
        /// Should only be used on create or update
        /// </summary>
        /// <param name="newAccountId"></param>
        public void SetNewAccountId(string newAccountId)
        {
            if (string.IsNullOrWhiteSpace(newAccountId))
                throw new ArgumentNullException("newAccountId");
            
            this.NewAccountId = newAccountId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payrollDepositDirective = obj as PayrollDepositDirective;

            if (payrollDepositDirective.Id == this.Id)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Id-{0}_Priority-{1}_Amount-{2}_StartDate-{3}", Id, Priority, DepositAmount, StartDate);
        }

        #endregion
    }
}
