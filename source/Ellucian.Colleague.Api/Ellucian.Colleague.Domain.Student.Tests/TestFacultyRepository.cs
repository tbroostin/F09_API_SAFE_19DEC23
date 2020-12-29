using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestFacultyRepository : IFacultyRepository
    {
        public async Task<IEnumerable<string>> SearchFacultyIdsAsync(bool facultyOnlyFlag = false, bool advisorOnlyFlag = true)
        {
            return await Task.FromResult(new List<string>() {"0000036", "0000045", "0000046", "0000047", "0000048",
                                       "0000049", "0000050", "0000051", "0000052", "0000053"});
        }

        public async Task<ICollection<Faculty>> GetAllAsync()
        {
            return await BuildFacultyRepositoryAsync();
        }

        public async Task<IEnumerable<Faculty>> GetAsync()
        {
            return await BuildFacultyRepositoryAsync();
        }

        public async Task<Faculty> GetAsync(string id)
        {
            return (await  BuildFacultyRepositoryAsync()).Where(f => f.Id == id).First();
        }

        public async Task<IEnumerable<Faculty>> GetAsync(IEnumerable<string> ids)
        {
            var faculty = new List<Faculty>();
            var facultyList = await BuildFacultyRepositoryAsync();
            foreach (var id in ids)
            {
                faculty.Add(facultyList.Where(f => f.Id == id).First());
            }
            return faculty;
        }

        public async Task<IEnumerable<Faculty>> GetFacultyByIdsAsync(IEnumerable<string> ids)
        {
            var faculty = new List<Faculty>();
            var facultyList = await BuildFacultyRepositoryAsync();
            foreach (var id in ids)
            {
                faculty.Add(facultyList.Where(f => f.Id == id).First());
            }
            return faculty;
        }

        //public ICollection<Faculty> GetMany(IEnumerable<string> ids)
        //{
        //    var faculty = new List<Faculty>();
        //    var facultyList = BuildFacultyRepository();
        //    foreach (var id in ids)
        //    {
        //        faculty.Add(facultyList.Where(f => f.Id == id).First());
        //    }
        //    return faculty;
        //}

        private async Task<ICollection<Faculty>> BuildFacultyRepositoryAsync()
        {
            var faculty = new List<Faculty>();
            string[,] facultyData = 
            {
                //ID, Last Name, First Name, Middle Name, PersonalPhone, PersonalPhoneType, PersonalPhoneExt, Email, EmailType
                {"0000036", "Smith", "Miltons", "Kristen", "703-222-3333", "CELL", "", "mSmith@yahoo.com", "PER"},
                {"0000045", "Aus", "Mark", "E", "", "", "", "", ""}, 
                {"0000046", "Beachtel", "Elizabeth", "R", "541-333-5555", "CELL", "Ext 5", "", ""},
                {"0000047", "Coile", "Carmon", "C", "", "", "", "", ""},
                {"0000048", "Denardi", "Anna", "B", "444-4444", "HOME", "", "aDenardi@ccc.com", "BUS"},
                {"0000049", "Eckberg", "Suzie", "L", "", "", "", "", ""},
                {"0000050", "Float", "Shelly", "X", "", "", "", "", ""},
                {"0000051", "Gualtieri", "Chris", "T", "", "", "", "", ""},
                {"0000052", "Hrabcak", "John", "B", "", "", "", "", ""},
                {"0000053", "Ivancic", "Jay", "T", "", "", "", "", ""}
            };
            int columns = 9;
            int count = facultyData.Length / columns;

            for (int x = 0; x < count; x++)
            {
                Faculty fac = new Faculty(facultyData[x, 0], facultyData[x, 1])
                {
                    ProfessionalName = facultyData[x, 1] + ", " + facultyData[x, 2],
                    FirstName = facultyData[x, 2]
                };
                if (fac.Id == "0000036")
                {
                    fac.ProfessionalName = "M.K. Smith";
                }
                if (!string.IsNullOrEmpty(facultyData[x, 7]) && !string.IsNullOrEmpty(facultyData[x, 8]))
                {
                    try
                    {
                        EmailAddress email = new EmailAddress(facultyData[x, 7], facultyData[x, 8]);
                        fac.AddEmailAddress(email);
                    }
                    catch (Exception)
                    {

                    }
                }

                if (!string.IsNullOrEmpty(facultyData[x, 4]) && !string.IsNullOrEmpty(facultyData[x, 5]))
                {
                    try
                    {
                        Phone personalPhone = new Phone(facultyData[x, 4], facultyData[x, 5], facultyData[x, 6]);
                        fac.AddPhone(personalPhone);
                    }
                    catch (Exception)
                    {

                    }
                }

                faculty.Add(fac);
            }

            return await Task.FromResult(faculty);
        }

        public Task<IEnumerable<FacultyOfficeHours>> GetFacultyOfficeHoursByIdsAsync(IEnumerable<string> facultyIds)
        {
            throw new NotImplementedException();
        }
    }
}
