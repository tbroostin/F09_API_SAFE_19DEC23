// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SectionTextbookDtoToEntityAdapter : BaseAdapter<Dtos.Student.SectionTextbook, Domain.Student.Entities.SectionTextbook>
    {
        public SectionTextbookDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Domain.Student.Entities.SectionTextbook MapToType(Dtos.Student.SectionTextbook source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source section textbook can not be null.");
            }

            if(source.Textbook == null)
            {
                throw new ArgumentNullException("source.Textbook", "The source book can not be null.");
            }

            SectionBookAction bookAction = SectionBookAction.Add;
            switch (source.Action)
            {
                case Dtos.Student.SectionBookAction.Remove:
                    bookAction = SectionBookAction.Remove;
                    break;
                case Dtos.Student.SectionBookAction.Add:
                    bookAction = SectionBookAction.Add;
                    break;
                case Dtos.Student.SectionBookAction.Update:
                    bookAction = SectionBookAction.Update;
                    break;
                default:
                    break;
            }
            var bookEntity = new Book(source.Textbook.Id, source.Textbook.Isbn, source.Textbook.Title, source.Textbook.Author, source.Textbook.Publisher,
                source.Textbook.Copyright, source.Textbook.Edition, source.Textbook.IsActive, source.Textbook.PriceUsed, source.Textbook.Price, 
                source.Textbook.Comment, source.Textbook.ExternalComments, source.Textbook.AlternateID1, source.Textbook.AlternateID2, source.Textbook.AlternateID3);

            var textbookEntity = new Domain.Student.Entities.SectionTextbook(bookEntity, source.SectionId, source.RequirementStatusCode, bookAction);
            return textbookEntity;
        }
    }
}
