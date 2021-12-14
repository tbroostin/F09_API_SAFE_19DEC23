// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class ContentKeyServiceTests : GenericUserFactory
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;
        public Mock<IEncryptionKeyRepository> encrRepositoryMock;
        public ContentKeyService contentKeyService;

        private ContentKey fakeContentKey;
        private ContentKeyRequest fakeContentKeyRequest;
        private EncrKey fakeEncrKey;
        private EncrKey fakePassProtectedEncrKey;

        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new UserFactory();
            encrRepositoryMock = new Mock<IEncryptionKeyRepository>();

            fakeContentKey = new ContentKey()
            {
                EncryptionKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe",
                EncryptedKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                Key = Convert.FromBase64String("KRiO9vG1XXoWgtb9GkTp3McpoRcL8yMcST/TFETXdf4=")
            };
            fakeContentKeyRequest = new ContentKeyRequest()
            {
                EncryptionKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe",
                EncryptedKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==")
            };
            fakeEncrKey = new EncrKey("7c655b4d-c425-4aff-af98-337004ec8cfe", "test-key",
                "-----BEGIN PRIVATE KEY-----" + Environment.NewLine
                + "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDP4N29rRFIFLrR" + Environment.NewLine
                + "/CCP17iyhrRlhzQx/2egC4wp4Ck7l9IDoSR5Oc7oBVnQ+YQNoruXpPLop7aagSl2" + Environment.NewLine
                + "Dw9pwH5Y9IrzScydBo2edz6cgbbD2r96E/eftBEgsDxC/Hc4fh3cV6K0dMwuXzJr" + Environment.NewLine
                + "zfrnHEJ39UwuY6P/cuq1Kn3swE5aWP829HeqyRI0bbcXuimDE0eTJ9Lc9qzmxR/r" + Environment.NewLine
                + "cDWDYvnZVcCttXKk3owoW7EpHoJOYpM5hTkK7XQuHFLg0XiHT3PLYFcljQ0I72ny" + Environment.NewLine
                + "pHEOGEQsXrnfmBqKG4+75qm7TH+R8MFMXvje3U5BO6UD2ykUt3rvwniYii92qwOQ" + Environment.NewLine
                + "yZ2B25o9AgMBAAECggEAYH47z7DHRdNBiPlk0ABNnqkCkeI5qz+oBVV24XVJDn2B" + Environment.NewLine
                + "oeep+4+G6JKbR3KbBo1UUKbjjnVTQNLVwRRqjicpPvd8jEPkc7s3/6fQa2uWv8EJ" + Environment.NewLine
                + "goxENGCsVqUXw+xXFuULzVzsjKFuqdPMntgFMEQg4nf9vcbIuGnKYv/vZfc6J5sz" + Environment.NewLine
                + "6bzkoyczRQL6cYZvsUDUqwXAiAzGu5hZbPtTQRo0vNT9GwiBmWFlu8r4yVTVUO/p" + Environment.NewLine
                + "w6d/64WAEOInjABRh7+6DcAldXkS58O20nHUYr3F37UDzYgCIULw46JzT8JlVhLg" + Environment.NewLine
                + "N+5squR3WEcGnnJouzDzJr7+c58p3wGMMPF3et/drQKBgQDntWXM+1BOF7mzDTpG" + Environment.NewLine
                + "XZs+xuhx/jCGhwOzPAXcq1Z3sNBEGQL3+SMD0DEthtVBxSixubFGXICV+akwvTLx" + Environment.NewLine
                + "/SRq/HEv2Miayg0yswisdc0JoYY+P00aVGrk8fVJNtVdjvUVr+qtLpQeafLVzswr" + Environment.NewLine
                + "9GzOCRcA6xsci1fNm6oPyQmC3wKBgQDlq+muzNNRo0k4+h/gQQ7cgnneOaw/5xsH" + Environment.NewLine
                + "pAGmrEKv5REybPJlYZglQwM8QwDq69vz1NP6D7OvigW/mpHdU1UNqPANW+Bse4kV" + Environment.NewLine
                + "x5wuhWiigAzBDBopX/NqYdXIrfR3+CjIMOB6GJMno2SWx3MnZA3L8orfpIAI7c3T" + Environment.NewLine
                + "22Ag6x/CYwKBgQCJy0iVFEd3iYh3wMANJJG0TZniYKX++r/qkSFzT7mGSHIybSVk" + Environment.NewLine
                + "zpZSKDd2uZ6NFHDU8HdKPqyBhA1n3Lw1SLOlpCazq5nw44PhyLK5zPx4Y8RvtDlo" + Environment.NewLine
                + "FRfUu0eBmMhecSuzEADhqLeRNrShDfBBm4QxKxqxAyAGY599uLrz1DyBxQKBgHrb" + Environment.NewLine
                + "9cvOQhTf1mGmW7ro0nxfR7X7AAvHIwx5TXDNoXbagNKKuThGds8oA+kOpsUEmsra" + Environment.NewLine
                + "xPJ1x9dVbDHNC85rr4n5H0DmLy2ZAAIon4G7V/flq+zw/mW3sEzuPSB2/dnXZGmC" + Environment.NewLine
                + "y/JEhyOjIkIOO6mMulypSGTOaLdeDscQCWJSpNClAoGABxPey+9n/DZHqnNyWT56" + Environment.NewLine
                + "zjmJm/hc/lY+/8kKjw+0tAcoFAQnX90r+/j+Wuzbh+mTF0Exr1DRPABpJzRTRUPx" + Environment.NewLine
                + "ObLa6hvbUtrkHTFEeeekX9kNF4sZo1H+POLp0Hh/0lwa+4M1H7X8lOKkxCKAsf12" + Environment.NewLine
                + "oFId+bBVX6x0ZMcITmg8DQE=" + Environment.NewLine
                + "-----END PRIVATE KEY-----",
                1, EncrKeyStatus.Active);
            fakePassProtectedEncrKey = new EncrKey("7c655b4d-c425-4aff-af98-337004ec8cfe", "test-key",
                "-----BEGIN ENCRYPTED PRIVATE KEY-----" + Environment.NewLine
                + "MIICxjBABgkqhkiG9w0BBQ0wMzAbBgkqhkiG9w0BBQwwDgQI+g9Nav3RxQACAggA" + Environment.NewLine
                + "MBQGCCqGSIb3DQMHBAi8KTfjfQtl0gSCAoCPGuOFOaowFN5kr6dBLVQ3uuM3nPim" + Environment.NewLine
                + "TdohABYkd2Z7P3S31QnVjyq6GKhEItChJ/6VUJp76BXKwLYuNNe02wRs1dB7SbyY" + Environment.NewLine
                + "ZxQLmmRUJ2W8cSSMnd8yYKUpC3ElSoD1m78mkfpwMzwSpcRY8cprEIx0g9B3GKJV" + Environment.NewLine
                + "EvtkcvuRvQLGNSRczENBZGXz5EY1t/WAwHLJM1X/slfKvZ1xDCmNQalWw5OO5EWc" + Environment.NewLine
                + "tY60CS1Oc1EPgB6eF3El5SedETg0n/LZgyjo0t0RbBXSFEW2z4hbNWegeSMQIyyq" + Environment.NewLine
                + "GesriNLm8v2v3Q1YbPt6nrLqKMfaLw6aID2Bgiy7EG/Q19zQKNFjukC8gI5QhUsx" + Environment.NewLine
                + "AwM/LknfAcsBh+XcMM3E4/JJiHlFApdWnwMW0qJreoLSqodxMK36LvorMkxX8iP+" + Environment.NewLine
                + "Nd05yWJkz0pfmB8mDD0rIHP7+6ChtmOAbLF5hOMN/502ISRGWuL/F//0wnzRcHcA" + Environment.NewLine
                + "etZV9OBdB0hhPbc8o598biuIO7zi7qTlV8F5cBpfU28wCh3i5JOVOccFjDmzCiOY" + Environment.NewLine
                + "QQ63A8unAzijInmcZ6Ew5rzYqO9Ry68nENseIP9zKaYqOXsMR5CA1GKZSapmCsUS" + Environment.NewLine
                + "7NiCIhP/MuCDsx/i9femP8XsT+A2+eGsaPyvgNOmYwS0uqCa6UYkEkD8nE7dFHTx" + Environment.NewLine
                + "cT4HsvBNGqLAArgtckgh9tVC/pkm59cSRtA1QyIAHWRnZ47Ael1M92Re/ph0UFlk" + Environment.NewLine
                + "Ab7CCJRA6cSGi5Lh13wd7hiscvmbdsPzLtL5njyH/5vVM0md6Hfzr59N3UcPy5K6" + Environment.NewLine
                + "o/r9w8Tj/O+zIGaSfq80s8chON304rsufpgYtP0pc8IsY8dqDqcoeLT2" + Environment.NewLine
                + "-----END ENCRYPTED PRIVATE KEY-----",
                1, EncrKeyStatus.Active);
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            encrRepositoryMock = null;
            contentKeyService = null;
        }

        public void BuildContentKeyService()
        {
            contentKeyService = new ContentKeyService(adapterRegistryMock.Object, encrRepositoryMock.Object, currentUserFactory,
                roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetContentKeyAsyncTests : ContentKeyServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(fakeEncrKey);
                BuildContentKeyService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task ContentKeyService_GetContentKeyAsync_Success()
            {
                var actual = await contentKeyService.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
                Assert.AreEqual(fakeContentKey.EncryptionKeyId, actual.EncryptionKeyId);
                Assert.IsNotNull(actual.EncryptedKey);
                Assert.IsNotNull(actual.Key);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ContentKeyService_GetContentKeyAsync_PassProtectedKeyException()
            {
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(fakePassProtectedEncrKey);
                BuildContentKeyService();

                try
                {
                    await contentKeyService.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Error occurred reading the encryption key", e.Message);
                    throw;
                }                
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ContentKeyService_GetContentKeyAsync_NullIdException()
            {
                await contentKeyService.GetContentKeyAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ContentKeyService_GetContentKeyAsync_EmptyIdException()
            {
                await contentKeyService.GetContentKeyAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ContentKeyService_GetContentKeyAsync_KeyNotFoundException()
            {
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                BuildContentKeyService();
                await contentKeyService.GetContentKeyAsync("fakekey");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ContentKeyService_GetContentKeyAsync_KeyInactiveException()
            {
                fakeEncrKey.Status = EncrKeyStatus.Inactive;
                await contentKeyService.GetContentKeyAsync(fakeContentKey.EncryptionKeyId);
            }
        }

        [TestClass]
        public class PostContentKeyAsyncTests : ContentKeyServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(fakeEncrKey);
                BuildContentKeyService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task ContentKeyService_PostContentKeyAsync_Success()
            {
                var actual = await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
                var actualJson = JsonConvert.SerializeObject(actual);
                var expectedJson = JsonConvert.SerializeObject(fakeContentKey);
                Assert.AreEqual(expectedJson, actualJson);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task ContentKeyService_PostContentKeyAsync_PassProtectedKeyException()
            {
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(fakePassProtectedEncrKey);
                BuildContentKeyService();

                try
                {
                    await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Error occurred reading the encryption key", e.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ContentKeyService_PostContentKeyAsync_NullRequestException()
            {
                await contentKeyService.PostContentKeyAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ContentKeyService_PostContentKeyAsync_NullKeyIdException()
            {
                fakeContentKeyRequest.EncryptionKeyId = null;
                await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ContentKeyService_PostContentKeyAsync_NullKeyException()
            {
                fakeContentKeyRequest.EncryptedKey = null;
                await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ContentKeyService_PostContentKeyAsync_KeyNotFoundException()
            {
                encrRepositoryMock.Setup(r => r.GetKeyAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                BuildContentKeyService();
                await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ContentKeyService_PostContentKeyAsync_KeyInactiveException()
            {
                fakeEncrKey.Status = EncrKeyStatus.Inactive;
                await contentKeyService.PostContentKeyAsync(fakeContentKeyRequest);
            }
        }
    }
}