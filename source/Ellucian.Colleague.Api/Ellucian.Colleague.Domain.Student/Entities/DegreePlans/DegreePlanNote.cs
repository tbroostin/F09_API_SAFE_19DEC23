// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    [Serializable]
    public class DegreePlanNote
    {
        private DateTimeOffset? _Date;
        private string _PersonId;
        private string _Text;
        private PersonType _PersonType;

        /// <summary>
        /// A unique identifier. 0 if a new note.
        /// </summary>
        private int _Id;
        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id == 0)
                {
                    _Id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// Date/time note added
        /// </summary>
        public DateTimeOffset? Date { get { return _Date; } }

        /// <summary>
        /// The person who added this note
        /// </summary>
        public string PersonId { get { return _PersonId; } }

        /// <summary>
        /// Note text
        /// </summary>
        public string Text {get {return _Text;}}

        /// <summary>
        /// The person type of the person who is adding the notes
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonType PersonType { get { return _PersonType; } }

        /// <summary>
        /// Base constructor for a degree plan note. Used only for new notes.
        /// User's personId and date/time determined by the database transaction.
        /// </summary>
        /// <param name="text"></param>
        public DegreePlanNote(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text", "Text is required for a degree plan note");
            }
            _Id = 0;
            _Text = text;
        }

        /// <summary>
        /// constructor for a degree plan note. Used only for new notes.
        /// User's personId and date/time determined by the database transaction.
        /// Person type is advisor or student based on who is adding this new comment
        /// </summary>
        /// <param name="text"></param>
        public DegreePlanNote(string text, PersonType personType)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text", "Text is required for a degree plan note");
            }
            _Id = 0;
            _Text = text;
            _PersonType = personType;
        }

        /// <summary>
        /// Constructor for an existing degree plan note, read from the repository
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="personId"></param>
        /// <param name="date"></param>
        /// <param name="text"></param>
        public DegreePlanNote(int id, string personId, DateTimeOffset? date, string text)
            : this(text)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "PersonId is required for a degree plan note.");
            }
            if (date == null)
            {
                throw new ArgumentNullException("date", "Date is required for a degree plan note");
            }
            _Id = id;
            _PersonId = personId;
            _Date = date;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            DegreePlanNote otherNote = obj as DegreePlanNote;
            if (otherNote == null)
            {
                return false;
            }
            return otherNote.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
