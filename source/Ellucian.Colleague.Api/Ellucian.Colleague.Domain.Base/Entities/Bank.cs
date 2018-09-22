/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Bank
    {
        #region PROPERTIES
        /// <summary>
        /// The name of the bank
        /// </summary>
        public string Name { get; private set;}
        //private string name;

        /// <summary>
        /// The routing number of a US bank or the institution id of a CA bank
        /// </summary>
        public string Id { get; private set; }
        //private string id;


        /// <summary>
        /// The RoutingId of the bank. If a US Bank, this will be the same as the ID attribute. If Canadian, this attribute
        /// will be null;
        /// </summary>
        public string RoutingId { get; private set; }

        /// <summary>
        /// The InstitutionId of the Canadian Bank. If a US bank, this attribute will be null
        /// </summary>
        public string InstitutionId { get; private set; }

        /// <summary>
        /// The BranchTransitNumber of the Canadian Bank. If a US Bank, this attribute will be null
        /// </summary>
        public string BranchTransitNumber { get; private set; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Creates an instance of a US Bank
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="routingId"></param>
        public Bank(string id, string name, string routingId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            this.Id = id;
            this.Name = name;
            this.RoutingId = ValidateRoutingNumber(routingId);
        }

        /// <summary>
        /// Creates an instance of a Canadian Bank
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="routingId"></param>
        public Bank(string id, string name, string institutionId, string branchTransitNumber)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }


            this.Id = id;
            this.Name = name;
            this.InstitutionId = ValidateInstitutionNumber(institutionId);
            this.BranchTransitNumber = ValidateBranchTransitNumber(branchTransitNumber);
        }


        /// <summary>
        /// Creates an instance of a bank
        /// If the id is nine characters and valid, it signifies a US routing number
        /// If the id is three characters, it signifies a Canadian Institution number
        /// </summary>
        /// <param name="bankName"></param>
        /// <param name="id"></param>
        //[Obsolete]
        //public Bank(string name, string id)
        //{
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        throw new ArgumentNullException("name");
        //    }
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new ArgumentNullException("id");
        //    }
            
        //    this.Name = name;

        //    if (id.Length == 9)
        //    {
        //        this.Id = ValidateRoutingNumber(id);
        //    }
        //    else if(id.Length == 3)
        //    {
        //        this.Id = ValidateInstitutionNumber(id);
        //    }
        //    else 
        //    {
        //        // The id passed in was not of a valid length
        //        throw new ArgumentException("id");
        //    }
        //}
        #endregion

        #region HELPERS
        /// <summary>
        /// Private helper function to validate a US routing number
        /// </summary>
        /// <param name="routingId"></param>
        /// <returns></returns>
        private string ValidateRoutingNumber(string routingId)
        {
            if (string.IsNullOrWhiteSpace(routingId))
            {
                throw new ArgumentNullException("routingId");
            }

            if (routingId.Length != 9)
            {
                throw new ArgumentOutOfRangeException("RoutingId must be 9 numeric characters");
            }

            int result;
            if (!int.TryParse(routingId, out result))
            {
                throw new ArgumentOutOfRangeException("routingId must contain only numeric characters");
            }

            var checkSum = 0;

            for (int i = 0; i < routingId.Count(); i += 3)
            {
                checkSum += int.Parse(routingId.ElementAt(i).ToString()) * 3
                    + int.Parse(routingId.ElementAt(i + 1).ToString()) * 7 + int.Parse(routingId.ElementAt(i + 2).ToString());
            }

            if (checkSum != 0 && checkSum % 10 == 0)
            {
                return routingId;
            }
            else
            {
                throw new ArgumentException(string.Format("The routingId is invalid {0}.", routingId));
            }
        }

        /// <summary>
        /// Validates a Canadian institution Number
        /// </summary>
        /// <param name="institutionNumber"></param>
        /// <returns></returns>
        private string ValidateInstitutionNumber(string institutionId)
        {
            if (string.IsNullOrWhiteSpace(institutionId))
            {
                throw new ArgumentNullException("institutionId");
            }

            if (institutionId.Length != 3)
            {
                throw new ArgumentOutOfRangeException("institutionId must be 3 characters");
            }

            int result;
            if (!int.TryParse(institutionId, out result))
            {
                throw new ArgumentException("institutionId must contain only numeric characters");
            }

            return institutionId;
        }

        private string ValidateBranchTransitNumber(string branchTransitNumber)
        {
            if (string.IsNullOrWhiteSpace(branchTransitNumber))
            {
                throw new ArgumentNullException("branchTransitNumber");
            }
            if (branchTransitNumber.Length != 5)
            {
                throw new ArgumentOutOfRangeException("branchTransitNumber must be 5 characters");
            }
            return branchTransitNumber;
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var bank = obj as Bank;

            return bank.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("id: {0}; name: {1}", Id, Name);
        }
    }
}
