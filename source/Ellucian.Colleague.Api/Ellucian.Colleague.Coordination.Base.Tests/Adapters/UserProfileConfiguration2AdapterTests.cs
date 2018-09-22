// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class UserProfileConfiguration2AdapterTests
    {
        bool CanUpdateEmailWithoutPermission;
        bool CanUpdatePhoneWithoutPermission;
        UserProfileViewUpdateOption CanViewUpdateChosenName;
        UserProfileViewUpdateOption CanViewUpdateNickname;
        UserProfileViewUpdateOption CanViewUpdateGenderIdentity;
        UserProfileViewUpdateOption CanViewUpdatePronoun;

        UserProfileConfiguration2 UserProfileConfigurationDto;
        Ellucian.Colleague.Domain.Base.Entities.UserProfileConfiguration2 UserProfileConfigurationEntity;
        UserProfileConfiguration2Adapter UserProfileConfigurationAdapter;

        [TestInitialize]
        public void Initialize()
        {
            CanUpdateEmailWithoutPermission = true;
            CanUpdatePhoneWithoutPermission = true;
            CanViewUpdateChosenName = UserProfileViewUpdateOption.Viewable;
            CanViewUpdateNickname = UserProfileViewUpdateOption.Updatable;
            CanViewUpdateGenderIdentity = UserProfileViewUpdateOption.NotAllowed;
            CanViewUpdatePronoun = UserProfileViewUpdateOption.Viewable;

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            UserProfileConfigurationAdapter = new UserProfileConfiguration2Adapter(adapterRegistryMock.Object, loggerMock.Object);

            var userProfileViewUpdateOptionAdapter = new AutoMapperAdapter<Domain.Base.Entities.UserProfileViewUpdateOption, UserProfileViewUpdateOption>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.UserProfileViewUpdateOption, UserProfileViewUpdateOption>()).Returns(userProfileViewUpdateOptionAdapter);

            UserProfileConfigurationEntity = new Domain.Base.Entities.UserProfileConfiguration2();

            //Need to set the variables within UserProfileConfigurationEntity to be able to compare them, but how?

            UserProfileConfigurationEntity.CanUpdateEmailWithoutPermission = true;
            UserProfileConfigurationEntity.CanUpdatePhoneWithoutPermission = true;
            UserProfileConfigurationEntity.CanViewUpdateChosenName = Ellucian.Colleague.Domain.Base.Entities.UserProfileViewUpdateOption.Viewable;
            UserProfileConfigurationEntity.CanViewUpdateNickname = Ellucian.Colleague.Domain.Base.Entities.UserProfileViewUpdateOption.Updatable;
            UserProfileConfigurationEntity.CanViewUpdateGenderIdentity = Ellucian.Colleague.Domain.Base.Entities.UserProfileViewUpdateOption.NotAllowed;
            UserProfileConfigurationEntity.CanViewUpdatePronoun = Ellucian.Colleague.Domain.Base.Entities.UserProfileViewUpdateOption.Viewable;

            UserProfileConfigurationDto = UserProfileConfigurationAdapter.MapToType(UserProfileConfigurationEntity);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanUpdateEmailWithoutPermission()
        {
            Assert.AreEqual(CanUpdateEmailWithoutPermission, UserProfileConfigurationDto.CanUpdateEmailWithoutPermission);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanUpdatePhoneWithoutPermission()
        {
            Assert.AreEqual(CanUpdatePhoneWithoutPermission, UserProfileConfigurationDto.CanUpdatePhoneWithoutPermission);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanViewUpdateChosenName()
        {
            Assert.AreEqual(CanViewUpdateChosenName, UserProfileConfigurationDto.CanViewUpdateChosenName);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanViewUpdateNickname()
        {
            Assert.AreEqual(CanViewUpdateNickname, UserProfileConfigurationDto.CanViewUpdateNickname);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanViewUpdateGenderIdentity()
        {
            Assert.AreEqual(CanViewUpdateGenderIdentity, UserProfileConfigurationDto.CanViewUpdateGenderIdentity);
        }

        [TestMethod]
        public void UserProfileConfigAdapterTests_CanViewUpdatePronoun()
        {
            Assert.AreEqual(CanViewUpdatePronoun, UserProfileConfigurationDto.CanViewUpdatePronoun);
        }
    }
}
