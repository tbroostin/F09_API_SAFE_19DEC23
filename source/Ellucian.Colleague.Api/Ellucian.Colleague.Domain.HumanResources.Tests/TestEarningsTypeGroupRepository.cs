using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEarningsTypeGroupRepository
    {

        public List<EarntypeGroupings> earningsTypeGroupDataContracts = new List<EarntypeGroupings>()
        {
            new EarntypeGroupings()
            {
                Recordkey = "ADMIN",
                EtpgDesc = "Administration earn type group",
                EtpgEarntype = new List<string>() { "REG", "ADJ", "BLANK", "NULL", "JUR" },
                EtpgEarntypeDesc = new List<string>() { "Regular", "Adjuct", "", null, "Jury"},
                EtpgHolidayCalendar = "MAIN",
                EtpgUseInSelfService = "Y"
            },
            new EarntypeGroupings()
            {
                Recordkey = "NOTADMIN",
                EtpgDesc = "Not Admin",
                EtpgEarntype = new List<string>() { "EXT", "PER", "FUN", "", null },
                EtpgEarntypeDesc = new List<string>() { "Extra", "Personal", "Blank Test", "null test" },
                EtpgUseInSelfService = "N",
                EtpgHolidayCalendar = null
            }
        };

        public async Task<IDictionary<string, EarningsTypeGroup>> GetEarningsTypeGroupsAsync()
        {
            var dictionary = new Dictionary<string, EarningsTypeGroup>();
            foreach (var dataContract in earningsTypeGroupDataContracts)
            {
                try
                {
                    var group = new EarningsTypeGroup(dataContract.Recordkey, dataContract.EtpgDesc, dataContract.EtpgUseInSelfService == "Y")
                    {
                        HolidayCalendarId = dataContract.EtpgHolidayCalendar
                    };
                    if (!dictionary.ContainsKey(group.EarningsTypeGroupId))
                    {
                        dictionary.Add(group.EarningsTypeGroupId, group);

                        for (var i = 0; i < dataContract.EtpgEarntype.Count; i++)
                        {
                            try
                            {
                                group.TryAdd(new EarningsTypeGroupItem(dataContract.EtpgEarntype[i], dataContract.EtpgEarntypeDesc[i], group.EarningsTypeGroupId));
                            }
                            catch (Exception) { }

                        }
                    }
                }
                catch (Exception)
                {

                }

            }
            return await Task.FromResult(dictionary);
        }
    }
}
