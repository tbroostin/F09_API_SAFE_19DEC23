// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class creates a list of GeneralLedgerUser objects. The list of objects allows
    /// for testing users that have access to all, a subset, and none of the general ledger
    /// expense accounts in the general ledger repository. It also allows for a test of a 
    /// user that has access to a general ledger expense account that does not exist in the
    /// general ledger repository.
    /// </summary>
    public class TestGeneralLedgerUserRepository : IGeneralLedgerUserRepository
    {
        public List<GeneralLedgerUser> GeneralLedgerUsers;
        string classificationName = "GL.CLASS";
        IEnumerable<string> expenseValues = new List<string>() { "5", "7" };
        private TestGlAccountRepository testGlAccountRepository = null;

        public TestGeneralLedgerUserRepository()
        {
            GeneralLedgerUsers = new List<GeneralLedgerUser>();
            testGlAccountRepository = new TestGlAccountRepository();

            // This user has a subset of general ledger expense accounts
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000001", "LastName"));
            var expenseIds = new List<string>();
            var revenueIds = new List<string>();
            var allIds = new List<string>();
            expenseIds.Add("1000005308001");
            expenseIds.Add("1010005308001");
            expenseIds.Add("1020005308001");
            expenseIds.Add("1080005308001");
            expenseIds.Add("10_00_01_01_33333_51001");
            allIds.Add("1000005308001");
            allIds.Add("1010005308001");
            allIds.Add("1020005308001");
            allIds.Add("1080005308001");
            allIds.Add("10_00_01_01_33333_51001");
            GeneralLedgerUsers[0].AddExpenseAccounts(expenseIds);
            GeneralLedgerUsers[0].AddAllAccounts(allIds);

            // This user has no general ledger expense accounts
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000002", "LastName"));

            // This user has access to general ledger expense accounts that dont exist
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000003", "LastName"));
            GeneralLedgerUsers[2].AddExpenseAccounts(new List<string>() { "10ZZZZ5308001" });
            GeneralLedgerUsers[2].AddAllAccounts(new List<string>() { "10ZZZZ5308001" });

            // This user has all general ledger expense accounts
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000004", "LastName"));
            expenseIds = new List<string>();
            allIds = new List<string>();
            expenseIds.Add("1000005308001");
            expenseIds.Add("1010005308001");
            expenseIds.Add("1020005308001");
            expenseIds.Add("1030005308001");
            expenseIds.Add("1080005308001");
            GeneralLedgerUsers[3].AddExpenseAccounts(expenseIds);
            allIds.Add("1000005308001");
            allIds.Add("1010005308001");
            allIds.Add("1020005308001");
            allIds.Add("1030005308001");
            allIds.Add("1080005308001");

            revenueIds.Add("1000004000001");
            revenueIds.Add("1010004000101");
            revenueIds.Add("1010004000201");
            revenueIds.Add("1010004000301");
            revenueIds.Add("1010004000401");
            GeneralLedgerUsers[3].AddRevenueAccounts(revenueIds);
            allIds.Add("1000004000001");
            allIds.Add("1010004000101");
            allIds.Add("1010004000201");
            allIds.Add("1010004000301");
            allIds.Add("1010004000401");
            GeneralLedgerUsers[3].AddAllAccounts(allIds);
            GeneralLedgerUsers[3].SetGlAccessLevel(GlAccessLevel.Full_Access);

            // This user has the full GL access role active
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000005", "Thorne"));
            expenseIds = new List<string>();
            expenseIds.Add("1000005308001");
            expenseIds.Add("1010005308001");
            expenseIds.Add("1020005308001");
            expenseIds.Add("1030005308001");
            expenseIds.Add("1080005308001");
            GeneralLedgerUsers[4].AddExpenseAccounts(expenseIds);

            // This user has at least one active GL role
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000006", "Kleehammer"));
            expenseIds = new List<string>();
            expenseIds.Add("1000005308001");
            expenseIds.Add("1010005308001");
            expenseIds.Add("1020005308001");
            GeneralLedgerUsers[5].AddExpenseAccounts(expenseIds);

            // This user has no active GL roles
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000007", "Longerbeam"));

            // This user has an expired GL access record
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000008", "Longerbeam"));

            // This user does not have a GL access record
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000009", "Longerbeam"));

            // This user does not have a staff login ID
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000010", "Longerbeam"));

            // This user does not have a staff record
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000011", "Longerbeam"));

            // This user has the full GL access role active and an additional role active
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000012", "Thorne"));

            // This user has the full GL access role active and an additional role active
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000013", "Thorne"));

            // This user has the full GL access role active and an additional role active
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000014", "Thorne"));

            // This user has the full GL access role active and an additional role active
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000015", "Thorne"));

            // This user has the full GL access role which is null and has no other active role
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000016", "Thorne"));

            // This user has three active roles where the 2nd role has no start date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000017", "Thorne"));

            // This user has three active roles where the 2nd role has no end date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000018", "Thorne"));

            // This user has three active roles where the 3rd role has no end date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000019", "Thorne"));

            // This user has three active roles where the 3rd role has no start date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000020", "Thorne"));

            // This user has one active role where the role has no start date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000021", "Thorne"));

            // This user has a GLUSERS record with no start date
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000022", "Thorne"));

            // This user has one active full access role with a valid start and end date.
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000023", "Thorne"));

            // This user has one active Web full access role with a valid start and end date.
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000024", "Thorne"));

            // This user has the full GL access role active with a GL user end date.
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000025", "Thorne"));
            expenseIds = new List<string>();
            expenseIds.Add("1000005308001");
            expenseIds.Add("1010005308001");
            expenseIds.Add("1020005308001");
            expenseIds.Add("1030005308001");
            expenseIds.Add("1080005308001");
            GeneralLedgerUsers[4].AddExpenseAccounts(expenseIds);

            // This user has a future GL access record
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000026", "Longerbeam"));

            // This user has a full access role that starts today
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000027", "Thorne"));

            // This GL User effectively has one cost center with five subtotals.
            // Cost Center: 10000001
            //   Subtotals: 00
            //              01
            //              02
            //              03
            //              04
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000028", "Kleehammer"));

            // This GL User has five cost centers:
            //  Cost Center - Subtotal
            //  - 10000001  - 00
            //  - 10000002  - 01
            //  - 10000003  - 01
            //  - 10000004  - 02
            //  - 10000005  - 02
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000029", "Kleehammer"));
            expenseIds = new List<string>()
            {
                "1000000051001",
                "1000000151002",
                "1000000151003",
                "1000000251004",
                "1000000251005",
            };
            var user = GeneralLedgerUsers.Where(x => x.Id == "0000029").FirstOrDefault();
            user.AddExpenseAccounts(expenseIds);

            // This user has a full access role that will return no GL accounts
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000030", "Thorne"));

            // This user has a full access role that will return no GL accounts from GL.ACCTS.ROLES
            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000031", "Thorne"));

            GeneralLedgerUsers.Add(new GeneralLedgerUser("0000032", "Kleehammer"));

            // Set up a user who has revenue and expense accounts
            var glUser = new GeneralLedgerUser("0000033", "Kleehammer");
            expenseIds = new List<string>();
            expenseIds.Add("11_00_02_01_20601_53011");
            expenseIds.Add("11_00_02_01_20601_53013");
            expenseIds.Add("11_00_02_01_20601_53014");
            expenseIds.Add("11_00_02_01_20601_53021");
            glUser.AddExpenseAccounts(expenseIds);

            revenueIds = new List<string>();
            revenueIds.Add("11_00_02_01_20601_40000");
            revenueIds.Add("11_00_02_01_20601_40001");
            revenueIds.Add("11_00_02_01_20601_40002");
            revenueIds.Add("11_00_02_01_20601_40003");
            glUser.AddRevenueAccounts(revenueIds);
            glUser.AddAllAccounts(expenseIds);
            glUser.AddAllAccounts(revenueIds);
            GeneralLedgerUsers.Add(glUser);

            // User with no GL access
            GeneralLedgerUsers.Add(new GeneralLedgerUser("9999999", "Kleehammer"));
        }

        public async Task<GeneralLedgerUser> GetGeneralLedgerUserAsync(string id, string fullAccessRole, string classificationName, IEnumerable<string> expenseValues)
        {
            // get the first general ledger user that matches the id passed into the method.
            return await Task.Run(() => GeneralLedgerUsers.FirstOrDefault(x => x.Id == id));
        }

        public async Task<GeneralLedgerUser> GetGeneralLedgerUserAsync2(string id, string fullAccessRole, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            return await Task.Run(() => GeneralLedgerUsers.FirstOrDefault(x => x.Id == id));
        }

        public async Task<bool> CheckOverride(string id)
        {
            return await Task.Run(() => true);
        }
    }
}