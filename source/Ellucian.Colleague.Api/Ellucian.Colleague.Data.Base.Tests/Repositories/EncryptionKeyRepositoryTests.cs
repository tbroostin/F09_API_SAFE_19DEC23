// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class EncryptionKeyRepositoryTests : BaseRepositorySetup
    {
        EncryptionKeyRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            repository = new EncryptionKeyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }

        [TestClass]
        public class EncryptionKeyRepository_GetAttachmentByIdAsyncTests : EncryptionKeyRepositoryTests
        {
            DataContracts.EncrKeys userData;

            [TestInitialize]
            public void EncryptionKeyRepository_GetKeyAsync_Initialize()
            {
                base.Initialize();

                userData = new DataContracts.EncrKeys()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    EncrkDesc = "key used for unit testing",
                    EncrkName = "test key",
                    EncrkStatus = "A",
                    EncrkVersion = 1,
                    EncrkKey = "-----BEGIN PRIVATE KEY-----" + Environment.NewLine
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
                        + "-----END PRIVATE KEY-----"
                };
                userData.buildAssociations();

                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.EncrKeys>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);
            }

            [TestMethod]
            public async Task EncryptionKeyRepository_GetKeyAsync_Success()
            {
                var actual = await repository.GetKeyAsync(userData.Recordkey);
                Assert.AreEqual(userData.Recordkey, actual.Id);
                Assert.AreEqual(userData.EncrkDesc, actual.Description);
                Assert.AreEqual(userData.EncrkKey, actual.Key);
                Assert.AreEqual(userData.EncrkName, actual.Name);
                Assert.AreEqual("Active", actual.Status.ToString());
                Assert.AreEqual(userData.EncrkVersion, actual.Version);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EncryptionKeyRepository_NullIdException()
            {
                await repository.GetKeyAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EncryptionKeyRepository_NoKeyFoundException()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.EncrKeys>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                try
                {
                    await repository.GetKeyAsync(userData.Recordkey);
                }
                catch (RepositoryException re)
                {
                    Assert.AreEqual("Encryption key not found", re.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EncryptionKeyRepository_InvalidEncryptionVersionException()
            {
                userData.EncrkVersion = 0;

                try
                {
                    await repository.GetKeyAsync(userData.Recordkey);
                }
                catch (RepositoryException re)
                {
                    Assert.AreEqual("Invalid encryption key version", re.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EncryptionKeyRepository_InvalidStatusException()
            {
                userData.EncrkStatus = "invalid status";

                try
                {
                    await repository.GetKeyAsync(userData.Recordkey);
                }
                catch (ArgumentException ae)
                {
                    Assert.AreEqual("Unknown status", ae.Message);
                    throw;
                }
            }
        }
    }
}