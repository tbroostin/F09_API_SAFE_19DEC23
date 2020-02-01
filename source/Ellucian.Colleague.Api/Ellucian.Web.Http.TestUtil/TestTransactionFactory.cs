// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Configuration;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.DMIF;
using slf4net;

namespace Ellucian.Web.Http.TestUtil
{
    public class TestTransactionFactory: IColleagueTransactionFactory
    {
        private StandardDmiSession session;
        private ILogger logger;
        private DmiSettings settings;

        public TestTransactionFactory(StandardDmiSession session, ILogger logger, DmiSettings settings)
        {
            this.session = session;
            this.logger = logger;
            this.settings = settings;
        }

        public IColleagueDataReader GetColleagueDataReader()
        {
            return new ColleagueDataReader(session, settings);
        }

        public IColleagueDataReader GetDataReader()
        {
            return GetDataReader(false);
        }

        public IColleagueDataReader GetDataReader(bool anonymous)
        {
            if (anonymous)
            {
                return new AnonymousColleagueDataReader(settings);
            }
            else
            {
                return new ColleagueDataReader(session, settings);
            }
        }

        public IColleagueTransactionInvoker GetTransactionInvoker()
        {
            return new ColleagueTransactionInvoker(session.SecurityToken, session.SenderControlId, logger, settings);
        }

        public DMIFileTransferClient GetDMIFClient()
        {
            return new DMIFileTransferClient(settings, logger);
        }
    }
}
