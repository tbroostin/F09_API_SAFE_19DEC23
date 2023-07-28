using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Additional info for aid application
    /// </summary>
    [Serializable]
    public class AidApplicationAdditionalInfo
    {
        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>
        public string Id { get { return _id; } }
        private readonly string _id;
        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity.
        /// </summary>        
        public string AppDemoId { get { return _appDemoId; } }
        private readonly string _appDemoId;

        /// <summary>
        /// The Key to PERSON.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record.
        /// </summary>
        public string ApplicationType { get; set; }

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>
        public string AidYear { get; set; }

        /// <summary>
        /// The student's assigned ID.
        /// </summary>
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// The student's State Student Identification Number (SSID).
        /// </summary>
        public string StudentStateId { get; set; }

        /// <summary>
        /// Whether the student is in foster care.
        /// </summary>
        public bool? FosterCare { get; set; }

        /// <summary>
        /// The county specified on the application.
        /// </summary>
        public string ApplicationCounty { get; set; }

        /// <summary>
        /// The state associated to the wardship.
        /// </summary>
        public string WardshipState { get; set; }

        /// <summary>
        /// The Chafee Consideration indicator.
        /// </summary>
        public bool? ChafeeConsideration { get; set; }

        /// <summary>
        /// Indicates the application transaction that was used to populate BOGG.ACYR data.  
        /// </summary>
        public bool? CreateCcpgRecord { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User1 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User2 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User3 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User4 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User5 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User6 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User7 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User8 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User9 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User10 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User11 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User12 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User13 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        public string User14 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User15 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User16 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User17 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User18 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User19 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        public DateTime? User21 { get; set; }

        /// <summary>
        /// constructor to initialize properties
        /// </summary>
        /// <param name="id">Id of the record</param>
        /// <param name="appDemoId">AppDemo Id</param>
        public AidApplicationAdditionalInfo(string id, string appDemoId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (appDemoId is null)
            {
                throw new ArgumentNullException("appDemoId");
            }

            _id = id;
            _appDemoId = appDemoId;
        }
    }
}
