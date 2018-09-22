// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestBankingInformationConfigurationRepository : IBankingInformationConfigurationRepository
    {
        #region TEST DATA
        public BankInfoParms dbBankInfoParameters = new BankInfoParms()
        {
            Recordkey = "1",
            BipTermsPara = "OpenSesame",
            BipDirDepRequired = "Y",
            BipPayableDepositEnabled = "Y",
            BipPrDepositEnabled = "Y",
            BipPrEffectDatePara = "SusiNgKapangyarihan",
            BipPrMessagePara = "PingPong",
            BipUseFedRoutingDir = "y",
            BipAcctAuthDisabled = "N"
        };
        public List<DocPara> dbDocsAndParagraphs = new List<DocPara>()
        {
            new DocPara()
            {
                Recordkey = "OpenSesame",
                ParaDescription = "The terms and conditions paragraph",
                ParaText = "The Grumman LLV was specifically designed for the United States Postal Service with Grumman winning the contract for production. The main design points of the vehicle in contract competition were serviceability, handling in confined areas, and overall economical operation. As its name suggests, the Grumman LLV is easily capable of twenty years of operation. The original design lifespan of the Grumman LLV specified by the U.S. Postal Service was 24 years, but in 2009 this was extended to thirty years. The body and final assembly is by Grumman, and the chassis (based on the 1983-05 S10 Blazer 2WD) is made by General Motors, with the powerplant (2.5L I-4 TBI \"Iron Duke\" and, in later production, General Motors 2.2L I-4 iron block/aluminum head engine), instrument cluster and front suspension similar to those used in the Chevrolet S-10 pickup",
            },
            new DocPara()
            {
                Recordkey = "SusiNgKapangyarihan",
                ParaDescription = "Ang mga salita ng mga batas at ibang bagay",
                ParaText = "saan ba ang aking mga isip dito? totoo, hindi ko alam.  alam mo ba?",
            },
            new DocPara()
            {
                Recordkey = "PingPong",
                ParaDescription = "Payroll Message Paragraph",
                ParaText = "Pickled chambray selfies, cray authentic chartreuse vegan blog portland pinterest mlkshk. Hella VHS poutine waistcoat irony. Raw denim sustainable bushwick, stumptown shoreditch taxidermy organic hoodie austin. Literally locavore yuccie listicle. Single-origin coffee microdosing selfies affogato yuccie roof party, lo-fi raw denim thundercats. Beard keytar cold-pressed direct trade you probably haven't heard of them, tattooed affogato. Migas everyday carry bespoke aesthetic, next level scenester pork belly organic typewriter selvage lo-fi iPhone kitsch letterpress."
            }
        };

        private string getDocParaText(string recordKey)
        {
            if (dbDocsAndParagraphs == null) return null;
            var docPara = dbDocsAndParagraphs.FirstOrDefault(para => para.Recordkey == recordKey);
            return docPara == null ? null : docPara.ParaText;
        }

        public async Task<BankingInformationConfiguration> GetBankingInformationConfigurationAsync()
        {
            var newConfiguration = new BankingInformationConfiguration();
            newConfiguration.IsDirectDepositEnabled = dbBankInfoParameters.BipPrDepositEnabled != null &&
                dbBankInfoParameters.BipPrDepositEnabled.Equals("Y", StringComparison.CurrentCultureIgnoreCase);

            newConfiguration.IsPayableDepositEnabled = dbBankInfoParameters.BipPayableDepositEnabled != null &&
                dbBankInfoParameters.BipPayableDepositEnabled.Equals("Y", StringComparison.CurrentCultureIgnoreCase);

            newConfiguration.AddEditAccountTermsAndConditions = getDocParaText(dbBankInfoParameters.BipTermsPara);

            newConfiguration.IsRemainderAccountRequired = dbBankInfoParameters.BipDirDepRequired != null &&
                dbBankInfoParameters.BipDirDepRequired.Equals("Y", StringComparison.CurrentCultureIgnoreCase);

            newConfiguration.PayrollEffectiveDateMessage = getDocParaText(dbBankInfoParameters.BipPrEffectDatePara);

            newConfiguration.PayrollMessage = getDocParaText(dbBankInfoParameters.BipPrMessagePara);

            newConfiguration.UseFederalRoutingDirectory = dbBankInfoParameters.BipUseFedRoutingDir != null &&
                dbBankInfoParameters.BipUseFedRoutingDir.Equals("Y", StringComparison.CurrentCultureIgnoreCase);


            return await Task.FromResult(newConfiguration);
        }

        #endregion
    }
}
