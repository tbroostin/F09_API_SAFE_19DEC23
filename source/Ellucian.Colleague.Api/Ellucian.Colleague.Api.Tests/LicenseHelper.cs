// Copyright 2018 Ellucian Company L.P.and its affiliates.
using System.IO;

namespace Ellucian.Colleague.Api.Tests
{
    public static class LicenseHelper
    {
        private static readonly string dataFolder = "App_Data";
        private static readonly string licenseFileName = "ellucian.license";

        public static void CopyLicenseFile(string testDeploymentDirectory)
        {
            string licensePath = Path.Combine(testDeploymentDirectory, dataFolder);
            string testLicenseFilePath = Path.Combine(licensePath, licenseFileName);

            // Check if the file exists; if so, nothing need to be; otherwise, copy it from the source location
            if (!File.Exists(Path.Combine(licensePath, licenseFileName)))
            {
                if (!Directory.Exists(licensePath))
                {
                    // Create the target/test directory
                    Directory.CreateDirectory(licensePath);
                }

                string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string testSourceFile = Path.Combine(projectDirectory, "TestData", dataFolder, licenseFileName);
                File.Copy(testSourceFile, testLicenseFilePath);
            }
        }
    }
}
