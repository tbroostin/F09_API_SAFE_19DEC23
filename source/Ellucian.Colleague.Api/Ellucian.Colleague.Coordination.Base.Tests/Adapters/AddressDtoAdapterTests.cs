using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class AddressDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public AddressDtoAdapter adapter;

        public Dtos.Base.Address addressDto;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistryMock.Setup(r => r.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>())
                .Returns(() => new AutoMapperAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>(adapterRegistryMock.Object, loggerMock.Object));

            adapter = new AddressDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            addressDto = new Dtos.Base.Address()
            {
                AddressId = "123",
                PersonId = "0001234",
                AddressLines = new List<string>() { "10 main street","Apt 11"},
                AddressModifier = "c/o current resident",
                City = "anywhere",
                Country = "CANADA",
                CountryCode = "CAN",
                County = "OTT",
                EffectiveEndDate = new DateTime(2020,1,5),
                EffectiveStartDate = new DateTime(2015,10,1),
                IsPreferredAddress = true,
                IsPreferredResidence = true,
                PostalCode = "99876-XTKS",
                RouteCode = "RR1",
                State = "OTTAWA",
                Type = "Home",
                TypeCode = "HO",
                PhoneNumbers = new List<Dtos.Base.Phone>() 
                { 
                    new Dtos.Base.Phone() { Number = "703-998-9000", Extension = "1234", TypeCode = "BU"},
                    new Dtos.Base.Phone() { Number = "203-888-1000", Extension = "", TypeCode = "HO"} 
                }
            };
        }

        [TestMethod]
        public void IsBaseAdapter()
        {
            Assert.IsInstanceOfType(adapter, typeof(BaseAdapter<Dtos.Base.Address, Domain.Base.Entities.Address>));
        }

        [TestMethod]
        public void SourceWithAddressIdConvertsToAddressEntity()
        {
            var addressEntity = adapter.MapToType(addressDto);

            Assert.AreEqual(addressDto.AddressId, addressEntity.AddressId);
            Assert.AreEqual(addressDto.PersonId, addressEntity.PersonId);
            for (int i = 0; i < addressDto.AddressLines.Count(); i++)
            {
                Assert.AreEqual(addressDto.AddressLines.ElementAt(i), addressEntity.AddressLines.ElementAt(i));
            }
            Assert.AreEqual(addressDto.AddressModifier, addressEntity.AddressModifier);
            Assert.AreEqual(addressDto.City, addressEntity.City);
            Assert.AreEqual(addressDto.Country, addressEntity.Country);
            Assert.AreEqual(addressDto.CountryCode, addressEntity.CountryCode);
            Assert.AreEqual(addressDto.County, addressEntity.County);
            Assert.AreEqual(addressDto.EffectiveEndDate, addressEntity.EffectiveEndDate);
            Assert.AreEqual(addressDto.EffectiveStartDate, addressEntity.EffectiveStartDate);
            Assert.AreEqual(addressDto.IsPreferredAddress, addressEntity.IsPreferredAddress);
            Assert.AreEqual(addressDto.IsPreferredResidence, addressEntity.IsPreferredResidence);
            Assert.AreEqual(addressDto.PostalCode, addressEntity.PostalCode);
            Assert.AreEqual(addressDto.RouteCode, addressEntity.RouteCode);
            Assert.AreEqual(addressDto.State, addressEntity.State);
            Assert.AreEqual(addressDto.Type, addressEntity.Type);
            Assert.AreEqual(addressDto.TypeCode, addressEntity.TypeCode);
            for (int i = 0; i < addressDto.PhoneNumbers.Count(); i++)
            {
                Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).Extension, addressEntity.PhoneNumbers.ElementAt(i).Extension);
                Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).Number, addressEntity.PhoneNumbers.ElementAt(i).Number);
                Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).TypeCode, addressEntity.PhoneNumbers.ElementAt(i).TypeCode);
            }
        }

        [TestMethod]
        public void SourceWithOutAddressIdConvertsToAddressEntity()
        {
            addressDto = new Dtos.Base.Address()
            {
                AddressId = null,
                PersonId = "0001234",
                AddressLines = new List<string>() { "10 main street", "Apt 11" },
                City = "anywhere",
                EffectiveStartDate = new DateTime(2015, 10, 1),
                IsPreferredAddress = false,
                IsPreferredResidence = true,
                PostalCode = "88765",
                State = "UT",
                Type = "Home,Web-obtained",
                TypeCode = "HO,WB",
            };
            var addressEntity = adapter.MapToType(addressDto);

            Assert.IsNull(addressEntity.AddressId);
            Assert.AreEqual(addressDto.PersonId, addressEntity.PersonId);
            if (addressDto.AddressLines != null)
            {
                for (int i = 0; i < addressDto.AddressLines.Count(); i++)
                {
                    Assert.AreEqual(addressDto.AddressLines.ElementAt(i), addressEntity.AddressLines.ElementAt(i));
                }
            }
            Assert.AreEqual(addressDto.AddressModifier, addressEntity.AddressModifier);
            Assert.AreEqual(addressDto.City, addressEntity.City);
            Assert.AreEqual(addressDto.Country, addressEntity.Country);
            Assert.AreEqual(addressDto.CountryCode, addressEntity.CountryCode);
            Assert.AreEqual(addressDto.County, addressEntity.County);
            Assert.AreEqual(addressDto.EffectiveEndDate, addressEntity.EffectiveEndDate);
            Assert.AreEqual(addressDto.EffectiveStartDate, addressEntity.EffectiveStartDate);
            Assert.AreEqual(addressDto.IsPreferredAddress, addressEntity.IsPreferredAddress);
            Assert.AreEqual(addressDto.IsPreferredResidence, addressEntity.IsPreferredResidence);
            Assert.AreEqual(addressDto.PostalCode, addressEntity.PostalCode);
            Assert.AreEqual(addressDto.RouteCode, addressEntity.RouteCode);
            Assert.AreEqual(addressDto.State, addressEntity.State);
            Assert.AreEqual(addressDto.Type, addressEntity.Type);
            Assert.AreEqual(addressDto.TypeCode, addressEntity.TypeCode);
            if (addressDto.PhoneNumbers != null)
            {
            for (int i = 0; i < addressDto.PhoneNumbers.Count(); i++)
                {
                    Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).Extension, addressEntity.PhoneNumbers.ElementAt(i).Extension);
                    Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).Number, addressEntity.PhoneNumbers.ElementAt(i).Number);
                    Assert.AreEqual(addressDto.PhoneNumbers.ElementAt(i).TypeCode, addressEntity.PhoneNumbers.ElementAt(i).TypeCode);
                }
            }
        }
    }
}
