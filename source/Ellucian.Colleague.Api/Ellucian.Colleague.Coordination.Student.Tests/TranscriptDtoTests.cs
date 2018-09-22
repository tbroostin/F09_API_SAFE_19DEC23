using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using System.IO;
using System.Xml;

namespace Ellucian.Colleague.Coordination.Student.Tests
{
    [TestClass]
    public class TranscriptDtoTests
    {
        private XmlSerializer Serializer;
        private TextReader Reader;
        private TranscriptRequest tr;


        [TestInitialize]
        public void Initialize()
        {
            Serializer = new XmlSerializer(typeof(TranscriptRequest));
            Reader = new StringReader(GetXML());
        }

        [TestCleanup]
        public void Cleanup()
        {
            Serializer = null;
            Reader = null;
        }



        [TestMethod]
        public void EnumsPopulate()
        {
            try
            {
                tr = (TranscriptRequest)Serializer.Deserialize(Reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
            }

            Assert.AreEqual(TranscriptType.Undergraduate, tr.Request.Recipient.TranscriptType);
            Assert.AreEqual(TranscriptPurpose.Admission, tr.Request.Recipient.TranscriptPurpose);
        }

        [TestMethod]
        public void ListsPopulate()
        {
            try
            {
                tr = (TranscriptRequest)Serializer.Deserialize(Reader);
                //  Console.WriteLine(tr.Attribute1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
            }

            Assert.AreEqual(2, tr.Request.RequestedStudent.Attendance.AcademicAwardsReported.Count());
        }

        [TestMethod]
        public void SerializerTestRealData()
        {
            try
            {
                tr = (TranscriptRequest)Serializer.Deserialize(Reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
            }

            Assert.IsNotNull(tr.TransmissionData.DocumentID);
        }





        // Test XML Data

        public string GetXML(string version = null)
        {
            switch (version)
            {
                case "2":
                    //   For specific tests if needed
                    return null;


                default:

                    string xml = "";

                    xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<ns2:TranscriptRequest xmlns:ns2=\"urn:org:pesc:message:TranscriptRequest:v1.2.0\" xmlns:ns3=\"urn:org:pesc:core:CoreMain:v1.12.0\">" + 
                            "<ns2:TransmissionData>" +
                            "	<DocumentID>TO5776_A_200703080930</DocumentID>" +
                            "	<CreatedDateTime>2007-03-08T09:30:15-05:00</CreatedDateTime>" +
                            "	<DocumentTypeCode>Request</DocumentTypeCode>" +
                            "	<TransmissionType>Original</TransmissionType>" +
                            "	<Source>" +
                            "		<Organization>" +
                            "			<DUNS>827034414</DUNS>" +
                            "			<OrganizationName>National Student Clearinghouse</OrganizationName>" +
                            "		</Organization>" +
                            "	</Source>" +
                            "	<Destination>" +
                            "	<Organization>" +
                            "		<OPEID>00123400</OPEID>" +
                            "		<OrganizationName>HOMETOWN UNIVERSITY</OrganizationName>" +
                            "		</Organization>" +
                            "	</Destination>" +
                            "	<DocumentProcessCode>PRODUCTION</DocumentProcessCode>" +
                            "</ns2:TransmissionData>" +
                            "<ns2:Request>" +
                            "	<CreatedDateTime>2007-02-28T10:21:00-05:00</CreatedDateTime>" +
                            "	<Requestor>" +
                            "		<Person>" +
                            "			<SSN>123456789</SSN>" +
                            "			<Birth>" +
                            "				<BirthDate>1972-11-25</BirthDate>" +
                            "			</Birth>" +
                            "			<Name>" +
                            "				<FirstName>JANE</FirstName>" +
                            "				<MiddleName>B</MiddleName>" +
                            "				<LastName>DOE</LastName>" +
                            "			</Name>" +
                            "			<Contacts>" +
                            "				<Address>" +
                            "					<AddressLine>123 MAIN ST</AddressLine>" +
                            "					<City>ANYTOWN</City>" +
                            "					<StateProvinceCode>VA</StateProvinceCode>" +
                            "					<PostalCode>20171</PostalCode>" +
                            "				</Address>" +
                            "				<Phone>" +
                            "					<AreaCityCode>703</AreaCityCode>" +
                            "					<PhoneNumber>5551212</PhoneNumber>" +
                            "				</Phone>" +
                            "				<Email>" +
                            "					<EmailAddress>someone@gmail.com</EmailAddress>" +
                            "				</Email>" +
                            "			</Contacts>" +
                            "		</Person>" +
                            "	</Requestor>" +
                            "	<RequestedStudent>" +
                            "		<Person>" +
                            "			<SSN>123456789</SSN>" +
                            "			<Birth>" +
                            "				<BirthDate>1972-11-25</BirthDate>" +
                            "			</Birth>" +
                            "			<Name>" +
                            "				<FirstName>JANE</FirstName>" +
                            "				<MiddleName>B</MiddleName>" +
                            "				<LastName>DOE</LastName>" +
                            "			</Name>" +
                            "			<AlternateName>" +
                            "				<FirstName>JANE</FirstName>" +
                            "				<MiddleName>A</MiddleName>" +
                            "				<LastName>BOE</LastName>" +
                            "			</AlternateName>" +
                            "		</Person>" +
                            "		<Attendance>" +
                            "			<School>" +
                            "				<OrganizationName>HOMETOWN UNIVERSITY</OrganizationName>" +
                            "				<OPEID>00123400</OPEID>" +
                            "			</School>" +
                            "			<EnrollDate>1990-01-01</EnrollDate>" +
                            "			<ExitDate>1995-12-31</ExitDate>" +
                            "			<AcademicAwardsReported>" +
                            "				<AcademicAwardTitle>B.A. Communication</AcademicAwardTitle>" +
                            "				<AcademicAwardDate>1995-01-01</AcademicAwardDate>" +
                            "			</AcademicAwardsReported>" +
                            "			<AcademicAwardsReported>" +
                            "				<AcademicAwardTitle>B.S. Communication2</AcademicAwardTitle>" +
                            "				<AcademicAwardDate>1995-01-02</AcademicAwardDate>" +
                            "			</AcademicAwardsReported>" +
                            "		</Attendance>" +
                            "		<ReleaseAuthorizedIndicator>true</ReleaseAuthorizedIndicator>" +
                            "		<ReleaseAuthorizedMethod>Signature</ReleaseAuthorizedMethod>" +
                            "	</RequestedStudent>" +
                            "	<Recipient>" +
                            "		<RequestTrackingID>123456-1</RequestTrackingID>" +
                            "		<Receiver>" +
                            "			<RequestorReceiverOrganization>" +
                            "				<OrganizationName>HOMETOWN COMMUNITY COLLEGE</OrganizationName>" +
                            "				<Contacts>" +
                            "					<Address>" +
                            "						<AddressLine>P.O. BOX 31127</AddressLine>" +
                            "						<City>ANYTOWN</City>" +
                            "						<StateProvinceCode>VA</StateProvinceCode>" +
                            "						<PostalCode>20171</PostalCode>" +
                            "						<AttentionLine>HCC TRANSCRIPT OFFICE</AttentionLine>" +
                            "					</Address>" +
                            "					<Phone>" +
                            "						<AreaCityCode>703</AreaCityCode>" +
                            "						<PhoneNumber>5551212</PhoneNumber>" +
                            "					</Phone>" +
                            "					<FaxPhone>" +
                            "						<AreaCityCode>703</AreaCityCode>" +
                            "						<PhoneNumber>5551212</PhoneNumber>" +
                            "					</FaxPhone>" +
                            "				</Contacts>" +
                            "			</RequestorReceiverOrganization>" +
                            "		</Receiver>" +
                            "		<TranscriptType>Undergraduate</TranscriptType>" +
                            "		<TranscriptPurpose>Admission</TranscriptPurpose>" +
                            "		<DeliveryMethod>Mail</DeliveryMethod>" +
                            "		<TranscriptCopies>1</TranscriptCopies>" +
                            "	</Recipient>" +
                            "</ns2:Request>" +
                            "<ns2:UserDefinedExtensions>" +
                            "<AttachmentUrl>www.test.org</AttachmentUrl>" +
                            "<AttachmentFlag>Y</AttachmentFlag>" +
                            "</ns2:UserDefinedExtensions>" +
                            "</ns2:TranscriptRequest>";



                    return xml;
            }
        }

    }
}
