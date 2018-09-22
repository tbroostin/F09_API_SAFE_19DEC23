using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArCodesRepository
    {
        private static Collection<ArCodes> _arCodes = new Collection<ArCodes>();
        public static Collection<ArCodes> ArCodes
        {
            get
            {
                if (_arCodes.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arCodes;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] inputData = GetData();
            for (int i = 0; i < inputData.Length / 3; i++)
            {
                string code = inputData[i, 0].Trim();
                string desc = inputData[i, 1].Trim();
                int outPriority;
                int? priority = (Int32.TryParse(inputData[i, 2].Trim(), out outPriority)) ? outPriority : (int?)null;

                ArCodes record = new ArCodes() { Recordkey = code, ArcDesc = desc, ArcPriority = priority };
                _arCodes.Add(record);
            }
        }

        private static string[,] GetData()
        {
            string[,] arCodesData = {
                                    
                                        {"ACTFE", "Student Activity Fee          ", "040"},
                                        {"ADDFE", "Add Fee                       ", "010"},
                                        {"APPFE", "Application Fee               ", "050"},
                                        {"ATHFE", "Athletic Fee                  ", "050"},
                                        {"AUXFE", "Auxiliary Service Fee         ", "050"},
                                        {"BKBAG", "Bookstore-Bookbags            ", "999"},
                                        {"BKBKS", "Bookstore-Books General       ", "999"},
                                        {"BKBUS", "Bookstore-Bus Tickets         ", "999"},
                                        {"BKCLO", "Bookstore-Clothing            ", "999"},
                                        {"BKMEA", "Bookstore-Meal Tickets        ", "999"},
                                        {"BKNEW", "Bookstore-New Books           ", "020"},
                                        {"BKNTX", "Bookstore-Non Taxable         ", "999"},
                                        {"BKOTH", "Bookstore-Other               ", "999"},
                                        {"BKSUP", "Bookstore-Supplies            ", "999"},
                                        {"BKTCH", "Bookstore-Supplies Technical  ", "ABC"},
                                        {"BKUSE", "Bookstore-Used Books          ", "   "},
                                        {"CEAPP", "CE Application Fee            ", "050"},
                                        {"CEBTF", "Business Training Center Fee  ", "020"},
                                        {"CEBTT", "Business Training Cntr Tuition", "010"},
                                        {"CECOF", "Community Education Fee       ", "020"},
                                        {"CECOT", "Community Education Tuition   ", "010"},
                                        {"CEFEE", "Continuing Education Fee      ", "020"},
                                        {"CEISC", "CE Instructor Salary Charge   ", "020"},
                                        {"CEMAC", "CE Materials Charge           ", "020"},
                                        {"CERRC", "CE Room Rental Charge         ", "020"},
                                        {"CETUI", "Continuing Education Tuition  ", "010"},
                                        {"CEWDF", "Workforce Development Fee     ", "020"},
                                        {"CEWDT", "Workforce Development Tuition ", "010"},
                                        {"CHGRT", "Change Returned Ar Code       ", "070"},
                                        {"COMPR", "Comprehensive Fee             ", "010"},
                                        {"CRSFE", "Course Fee                    ", "020"},
                                        {"DAMAG", "Residence Life Damages        ", "060"},
                                        {"DRPFE", "Drop Fee                      ", "010"},
                                        {"ENROL", "Enrollment Fee                ", "020"},
                                        {"FCHR2", "Finance Charge - Datatel Ccd  ", "060"},
                                        {"FCHRG", "Finance Charge                ", "060"},
                                        {"GRAPP", "Graduate Application Fee      ", "050"},
                                        {"GRDFE", "Graduation Fee                ", "050"},
                                        {"HLTFE", "Health Service Fee            ", "040"},
                                        {"HRDEN", "Hr Dental Arrears Processing  ", "070"},
                                        {"HRLIF", "Hr Life Insurance Processing  ", "070"},
                                        {"HRMED", "Hr Medical Arrears Processing ", "070"},
                                        {"IDREP", "ID Card Replacement Fee       ", "060"},
                                        {"INTCH", "Monthly Interest Charge       ", "060"},
                                        {"KEYFE", "Key Replacement Fee           ", "060"},
                                        {"LABFE", "Laboratory Fee                ", "020"},
                                        {"LAWAP", "Law School Application Fee    ", "050"},
                                        {"LAWFE", "Law Library Fee               ", "040"},
                                        {"LBART", "Lab Fee-Art                   ", "020"},
                                        {"LBBIO", "Lab Fee-Biology               ", "020"},
                                        {"LBCHE", "Lab Fee-Chemistry             ", "020"},
                                        {"LBCMP", "Lab Fee-Computer Sciences     ", "020"},
                                        {"LRGFE", "Late Registration Fee         ", "060"},
                                        {"MALPR", "Malpractice Insurance Fee     ", "050"},
                                        {"MATFE", "Materials Fee                 ", "020"},
                                        {"MEALS", "Meal Plan Charges             ", "030"},
                                        {"NONAR", "Non-Ar Charge Code            ", "070"},
                                        {"NSFFE", "Nonsufficient Funds Fee       ", "060"},
                                        {"PFLAT", "Payment Plan Flat Late Fee    ", "001"},
                                        {"PHONE", "Phone Installation            ", "050"},
                                        {"PPCT ","Payment Plan Percent Late Fee  ", "090"},
                                        {"PPLAN", "Payment Plan Setup Charge     ", "001"},
                                        {"PRKFE", "Parking Fee                   ", "050"},
                                        {"PRKFN", "Parking Fine                  ", "060"},
                                        {"REGFE", "Registration Fee              ", "020"},
                                        {"RESHL", "Residence Hall Charges        ", "030"},
                                        {"RRENT", "Room Rental                   ", "030"},
                                        {"SUNFE", "Student Union Fee             ", "040"},
                                        {"T4A01", "T4A Box Code 28.04            ", "050"},
                                        {"TECFE", "Technology Fee                ", "020"},
                                        {"TRNFE", "Transcript Fee                ", "050"},
                                        {"TROFE", "Transcript Overnight Fee      ", "050"},
                                        {"TUIFR", "Tuition Forfeiture            ", "010"},
                                        {"TUIFT", "Tuition-Full Time             ", "010"},
                                        {"TUIGR", "Tuition-Graduate              ", "010"},
                                        {"TUIIN", "Tuition-In State              ", "010"},
                                        {"TUILW", "Tuition-Law School            ", "010"},
                                        {"TUIOT", "Tuition-Out of State          ", "010"},
                                        {"TUIOV", "Tuition-Overload              ", "010"},
                                        {"TUIPT", "Tuition-Part Time             ", "010"},
                                        {"WADMN", "Waiver-Administrative         ", "010"},
                                        {"WAIVE", "Waiver                        ", "010"},
                                        {"WEMPD", "Waiver-Dependent of Employee  ", "010"},
                                        {"WEMPL", "Waiver-Employees              ", "010"},
                                        {"WHLTH", "Waiver-Health Service Fee     ", "010"},
                                        {"WHSST", "Waiver-High School Students   ", "010"},
                                        {"WSENR", "Waiver-Senior Citizens        ", "010"},
                                        {"WTHFE", "Withdrawal Fee                ", "050"},
                                    };
            return arCodesData;
        }
    }
}
