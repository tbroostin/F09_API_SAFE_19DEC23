/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface ICommunicationRepository
    {
        Communication CreateCommunication(Communication communication, IEnumerable<Communication> existingCommunications=null);

        Communication UpdateCommunication(Communication communication, IEnumerable<Communication> existingCommunications=null);

        IEnumerable<Communication> GetCommunications(string personId);

        Communication SubmitCommunication(Communication communication);
    }
}
