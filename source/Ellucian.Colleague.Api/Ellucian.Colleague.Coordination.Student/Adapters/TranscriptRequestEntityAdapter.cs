using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    // Complex objects require additional dependency mappings
    public class TranscriptRequestEntityAdapter : AutoMapperAdapter<Dtos.Student.Transcripts.TranscriptRequest, Domain.Student.Entities.Transcripts.TranscriptRequest>
    {
        public TranscriptRequestEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.Student.Entities.Transcripts.TranscriptRequest MapToType(Dtos.Student.Transcripts.TranscriptRequest Source)
        {
            // Automapper hates nulls

            // Main object

            Domain.Student.Entities.Transcripts.TranscriptRequest tr = new Domain.Student.Entities.Transcripts.TranscriptRequest();

            // First Level
            tr.TransmissionData = new Domain.Student.Entities.Transcripts.TransmissionData();
            tr.Request = new Domain.Student.Entities.Transcripts.Request();
            tr.UserDefinedExtensions = new Domain.Student.Entities.Transcripts.UserDefinedExtensions();

            // TransmissionData

            if (Source.TransmissionData != null)
            {
                tr.TransmissionData.CreatedDateTime = Source.TransmissionData.CreatedDateTime;
                tr.TransmissionData.DocumentID = Source.TransmissionData.DocumentID;
                tr.TransmissionData.RequestTrackingID = Source.TransmissionData.RequestTrackingID;
                tr.NoteMessage = Source.NoteMessage;

                if (Source.TransmissionData.Destination != null)
                {
                    tr.TransmissionData.Destination = new Domain.Student.Entities.Transcripts.Destination();

                    if (Source.TransmissionData.Destination.Organization != null)
                    {
                        tr.TransmissionData.Destination.Organization = new Domain.Student.Entities.Transcripts.Organization();

                        if (!string.IsNullOrEmpty(Source.TransmissionData.Destination.Organization.DUNS)) tr.TransmissionData.Destination.Organization.DUNS = Source.TransmissionData.Destination.Organization.DUNS;
                        if (!string.IsNullOrEmpty(Source.TransmissionData.Destination.Organization.OPEID)) tr.TransmissionData.Destination.Organization.OPEID = Source.TransmissionData.Destination.Organization.OPEID;
                        if (Source.TransmissionData.Destination.Organization.OrganizationName != null)
                        {
                            tr.TransmissionData.Destination.Organization.OrganizationName = new List<string>();
                            tr.TransmissionData.Destination.Organization.OrganizationName.AddRange(Source.TransmissionData.Destination.Organization.OrganizationName);
                        }
                    }
                }
                tr.TransmissionData.DocumentProcessCode = Source.TransmissionData.DocumentProcessCode;
                tr.TransmissionData.NoteMessage = Source.TransmissionData.NoteMessage;
                if (Source.TransmissionData.Source != null)
                {
                    tr.TransmissionData.Source = new Domain.Student.Entities.Transcripts.Source();

                    if (Source.TransmissionData.Source.Organization != null)
                    {
                        tr.TransmissionData.Source.Organization = new Domain.Student.Entities.Transcripts.Organization();

                        if (!string.IsNullOrEmpty(Source.TransmissionData.Source.Organization.DUNS)) tr.TransmissionData.Source.Organization.DUNS = Source.TransmissionData.Source.Organization.DUNS;
                        if (!string.IsNullOrEmpty(Source.TransmissionData.Source.Organization.OPEID)) tr.TransmissionData.Source.Organization.OPEID = Source.TransmissionData.Source.Organization.OPEID;
                        if (Source.TransmissionData.Source.Organization.OrganizationName != null)
                        {
                            tr.TransmissionData.Source.Organization.OrganizationName = new List<string>();
                            tr.TransmissionData.Source.Organization.OrganizationName.AddRange(Source.TransmissionData.Source.Organization.OrganizationName);
                        }
                    }
                }

            }

            // Request

            if (Source.Request != null)
            {
                tr.Request = new Domain.Student.Entities.Transcripts.Request();

                if (Source.Request.Recipient != null)
                {
                    tr.Request.Recipient = new Domain.Student.Entities.Transcripts.Recipient();

                    tr.Request.Recipient.DeliveryInstruction = Source.Request.Recipient.DeliveryInstruction;
                    tr.Request.Recipient.DeliveryMethod = (Domain.Student.Entities.Transcripts.DeliveryMethod)Source.Request.Recipient.DeliveryMethod;
                    if (Source.Request.Recipient.ElectronicDelivery != null)
                    {
                        tr.Request.Recipient.ElectronicDelivery = new Domain.Student.Entities.Transcripts.ElectronicDelivery()
                        {
                            ElectronicFormat = Source.Request.Recipient.ElectronicDelivery.ElectronicFormat,
                            ElectronicMethod = Source.Request.Recipient.ElectronicDelivery.ElectronicMethod
                        };
                    }
                    if (Source.Request.Recipient.Receiver != null)
                    {
                        tr.Request.Recipient.Receiver = new Domain.Student.Entities.Transcripts.Receiver();
                        if (Source.Request.Recipient.Receiver.RequestorReceiverOrganization != null)
                        {
                            tr.Request.Recipient.Receiver.RequestorReceiverOrganization = new Domain.Student.Entities.Transcripts.RequestorReceiverOrganization();
                            if (Source.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts != null)
                            {
                                tr.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts = BuildContacts(Source.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts);

                            }
                            tr.Request.Recipient.Receiver.RequestorReceiverOrganization.OrganizationName = Source.Request.Recipient.Receiver.RequestorReceiverOrganization.OrganizationName;
                            tr.Request.Recipient.Receiver.RequestorReceiverOrganization.OPEID = Source.Request.Recipient.Receiver.RequestorReceiverOrganization.OPEID;

                        }
                    }


                    tr.Request.Recipient.RushProcessingRequested = Source.Request.Recipient.RushProcessingRequested;
                    tr.Request.Recipient.StampSealEnvelopeIndicator = Source.Request.Recipient.StampSealEnvelopeIndicator;
                    tr.Request.Recipient.TranscriptCopies = Source.Request.Recipient.TranscriptCopies;
                    tr.Request.Recipient.TranscriptPurpose = (Domain.Student.Entities.Transcripts.TranscriptPurpose)Source.Request.Recipient.TranscriptPurpose;
                    tr.Request.Recipient.TranscriptType = (Domain.Student.Entities.Transcripts.TranscriptType)Source.Request.Recipient.TranscriptType;
                }
                if (Source.Request.RequestedStudent != null)
                {
                    tr.Request.RequestedStudent = new Domain.Student.Entities.Transcripts.RequestedStudent();
                    if (Source.Request.RequestedStudent.Attendance != null)
                    {
                        tr.Request.RequestedStudent.Attendance = new Domain.Student.Entities.Transcripts.Attendance();

                        if (Source.Request.RequestedStudent.Attendance.AcademicAwardsReported != null)
                        {
                            tr.Request.RequestedStudent.Attendance.AcademicAwardsReported = new List<Domain.Student.Entities.Transcripts.AcademicAwardsReported>();
                            foreach (var award in Source.Request.RequestedStudent.Attendance.AcademicAwardsReported)
                            {
                                tr.Request.RequestedStudent.Attendance.AcademicAwardsReported.Add(new Domain.Student.Entities.Transcripts.AcademicAwardsReported()
                                {
                                    AcademicAwardDate = award.AcademicAwardDate,
                                    AcademicAwardTitle = award.AcademicAwardTitle
                                });
                            }
                        }

                        tr.Request.RequestedStudent.Attendance.CurrentEnrollmentIndicator = Source.Request.RequestedStudent.Attendance.CurrentEnrollmentIndicator;
                        tr.Request.RequestedStudent.Attendance.EnrollDate = Source.Request.RequestedStudent.Attendance.EnrollDate;
                        tr.Request.RequestedStudent.Attendance.ExitDate = Source.Request.RequestedStudent.Attendance.ExitDate;

                        if (Source.Request.RequestedStudent.Attendance.School != null)
                        {
                            tr.Request.RequestedStudent.Attendance.School = new Domain.Student.Entities.Transcripts.School()
                            {
                                OPEID = Source.Request.RequestedStudent.Attendance.School.OPEID,
                                OrganizationName = Source.Request.RequestedStudent.Attendance.School.OrganizationName
                            };
                        }


                    }
                    if (Source.Request.RequestedStudent.Person != null)
                    {
                        tr.Request.RequestedStudent.Person = new Domain.Student.Entities.Transcripts.Person();
                        if (Source.Request.RequestedStudent.Person.AlternateName != null)
                        {
                            tr.Request.RequestedStudent.Person.AlternateName = new Domain.Student.Entities.Transcripts.AlternateName()
                            {
                                FirstName = Source.Request.RequestedStudent.Person.AlternateName.FirstName,
                                MiddleName = Source.Request.RequestedStudent.Person.AlternateName.MiddleName,
                                LastName = Source.Request.RequestedStudent.Person.AlternateName.LastName
                            };
                        }
                        if (Source.Request.RequestedStudent.Person.Birth != null)
                        {
                            tr.Request.RequestedStudent.Person.Birth = new Domain.Student.Entities.Transcripts.Birth() { BirthDate = Source.Request.RequestedStudent.Person.Birth.BirthDate };
                        }
                        if (Source.Request.RequestedStudent.Person.Contacts != null)
                        {
                            tr.Request.RequestedStudent.Person.Contacts = BuildContacts(Source.Request.RequestedStudent.Person.Contacts);
                        }
                        if (Source.Request.RequestedStudent.Person.Name != null)
                        {
                            tr.Request.RequestedStudent.Person.Name = new Domain.Student.Entities.Transcripts.Name()
                            {
                                FirstName = Source.Request.RequestedStudent.Person.Name.FirstName,
                                MiddleName = Source.Request.RequestedStudent.Person.Name.MiddleName,
                                LastName = Source.Request.RequestedStudent.Person.Name.LastName
                            };
                        }
                        tr.Request.RequestedStudent.Person.SchoolAssignedPersonID = Source.Request.RequestedStudent.Person.SchoolAssignedPersonID;
                        tr.Request.RequestedStudent.Person.SSN = Source.Request.RequestedStudent.Person.SSN;
                    }
                }
            }

            // UDIDS 

            if (Source.UserDefinedExtensions != null)
            {
                tr.UserDefinedExtensions = new Domain.Student.Entities.Transcripts.UserDefinedExtensions();
                tr.UserDefinedExtensions.AttachmentFlag = Source.UserDefinedExtensions.AttachmentFlag;
                tr.UserDefinedExtensions.AttachmentSpecialInstructions = Source.UserDefinedExtensions.AttachmentSpecialInstructions;
                tr.UserDefinedExtensions.AttachmentURL = Source.UserDefinedExtensions.AttachmentURL;
                if (Source.UserDefinedExtensions.EnrollmentDetail != null)
                {
                    tr.UserDefinedExtensions.EnrollmentDetail = new List<Domain.Student.Entities.Transcripts.EnrollmentDetail>();
                    foreach (var ed in Source.UserDefinedExtensions.EnrollmentDetail)
                    {
                        tr.UserDefinedExtensions.EnrollmentDetail.Add(new Domain.Student.Entities.Transcripts.EnrollmentDetail() 
                        {
                        BeginYear = ed.BeginYear,
                        EndYear = ed.EndYear,
                        NameOfProgram = ed.NameOfProgram
                        });
                    }
                }
                tr.UserDefinedExtensions.HoldForProgramId = Source.UserDefinedExtensions.HoldForProgramId;
                tr.UserDefinedExtensions.HoldForTermId = Source.UserDefinedExtensions.HoldForTermId;
                tr.UserDefinedExtensions.ReceivingInstitutionCeebId = Source.UserDefinedExtensions.ReceivingInstitutionCeebId;
                tr.UserDefinedExtensions.ReceivingInstitutionFiceId = Source.UserDefinedExtensions.ReceivingInstitutionFiceId;
                tr.UserDefinedExtensions.UnverifiedStudentId = Source.UserDefinedExtensions.UnverifiedStudentId;
            }

            tr.NoteMessage = Source.NoteMessage;
            return tr;
        }

        private List<Domain.Student.Entities.Transcripts.Contacts> BuildContacts(List<Dtos.Student.Transcripts.Contacts> src)
        {
            List<Domain.Student.Entities.Transcripts.Contacts> returnval = new List<Domain.Student.Entities.Transcripts.Contacts>();

            foreach (var con in src)
            {
                var lAddress = new Domain.Student.Entities.Transcripts.Address();
                if (con.Address != null)
                {
                    if (con.Address.AddressLine != null)
                    {
                        lAddress.AddressLine = new List<string>();
                        lAddress.AddressLine.AddRange(con.Address.AddressLine);

                    }
                    if (con.Address.AttentionLine != null)
                    {
                        lAddress.AttentionLine = new List<string>();
                        lAddress.AttentionLine.AddRange(con.Address.AttentionLine);
                    }
                    lAddress.City = con.Address.City;
                    lAddress.CountryCode = con.Address.CountryCode;
                    lAddress.PostalCode = con.Address.PostalCode;
                    lAddress.StateProvince = con.Address.StateProvince;
                    lAddress.StateProvinceCode = con.Address.StateProvinceCode;
                }
                var lEmail = new Domain.Student.Entities.Transcripts.Email();

                if (con.Email != null)
                {
                    lEmail.EmailAddress = con.Email.EmailAddress;
                }

                var lPhone = new Domain.Student.Entities.Transcripts.Phone();
                if (con.Phone != null)
                {
                    lPhone.AreaCityCode = con.Phone.AreaCityCode;
                    lPhone.PhoneNumber = con.Phone.PhoneNumber;
                }

                var lFaxPhone = new Domain.Student.Entities.Transcripts.Phone();
                if (con.FaxPhone != null)
                {
                    lFaxPhone.AreaCityCode = con.FaxPhone.AreaCityCode;
                    lFaxPhone.PhoneNumber = con.FaxPhone.PhoneNumber;
                }


                Domain.Student.Entities.Transcripts.Contacts contact = new Domain.Student.Entities.Transcripts.Contacts()
                {
                    Address = lAddress,
                    Email = lEmail,
                    Phone = lPhone,
                    FaxPhone = lFaxPhone
                };
                returnval.Add(contact);

            }
            return returnval;
        }
    }
}