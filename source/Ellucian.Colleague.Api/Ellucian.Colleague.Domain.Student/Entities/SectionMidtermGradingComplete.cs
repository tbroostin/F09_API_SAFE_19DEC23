using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// All midterm grading complete indications for a section
    /// </summary>
    [Serializable]
    public class SectionMidtermGradingComplete
    {
        /// <summary>
        /// The section ID
        /// </summary>
        private string _sectionId;
        public string SectionId { get { return _sectionId; } }

        public SectionMidtermGradingComplete(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section ID must have a value");
            }
               
            _sectionId = sectionId;
            _midtermGrading1Complete = new List<GradingCompleteIndication>();
            _midtermGrading2Complete = new List<GradingCompleteIndication>();
            _midtermGrading3Complete = new List<GradingCompleteIndication>();
            _midtermGrading4Complete = new List<GradingCompleteIndication>();
            _midtermGrading5Complete = new List<GradingCompleteIndication>();
            _midtermGrading6Complete = new List<GradingCompleteIndication>();
        }

        ///<summary>
        /// Indications that midterm grading 1 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading1Complete;
        public List<GradingCompleteIndication> MidtermGrading1Complete { get { return _midtermGrading1Complete; } }
        public void AddMidtermGrading1Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading1Complete.Add(midtermGradingComplete);
        }

        ///<summary>
        /// Indications that midterm grading 2 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading2Complete;
        public List<GradingCompleteIndication> MidtermGrading2Complete { get { return _midtermGrading2Complete; } }
        public void AddMidtermGrading2Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading2Complete.Add(midtermGradingComplete);
        }

        ///<summary>
        /// Indications that midterm grading 3 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading3Complete;
        public List<GradingCompleteIndication> MidtermGrading3Complete { get { return _midtermGrading3Complete; } }
        public void AddMidtermGrading3Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading3Complete.Add(midtermGradingComplete);
        }

        ///<summary>
        /// Indications that midterm grading 4 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading4Complete;
        public List<GradingCompleteIndication> MidtermGrading4Complete { get { return _midtermGrading4Complete; } }
        public void AddMidtermGrading4Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading4Complete.Add(midtermGradingComplete);
        }

        ///<summary>
        /// Indications that midterm grading 5 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading5Complete;
        public List<GradingCompleteIndication> MidtermGrading5Complete { get { return _midtermGrading5Complete; } }
        public void AddMidtermGrading5Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading5Complete.Add(midtermGradingComplete);
        }

        ///<summary>
        /// Indications that midterm grading 6 is complete
        /// </summary>
        private List<GradingCompleteIndication> _midtermGrading6Complete;
        public List<GradingCompleteIndication> MidtermGrading6Complete { get { return _midtermGrading6Complete; } }
        public void AddMidtermGrading6Complete(string completeOperator, DateTimeOffset dateAndTime)
        {
            var midtermGradingComplete = new GradingCompleteIndication(completeOperator, dateAndTime);
            _midtermGrading6Complete.Add(midtermGradingComplete);
        }


    }
}
