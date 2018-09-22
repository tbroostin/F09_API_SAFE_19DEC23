// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Web.Hosting;
using System.Web.Security;
using System.Diagnostics;
using Ellucian.Logging;
using Ellucian.Colleague.Configuration;
using System.Web.Configuration;

namespace Ellucian.Web.Http.Configuration
{
    public class XmlSettingsRepository : ISettingsRepository
    {
        private readonly string FileName;
        private readonly string BackupFilename;

        private static string Settings = "settings";
        private static string Colleague = "colleague";
        private static string IpAddress = "ipAddress";
        private static string AccountName = "accountName";
        private static string Port = "port";
        private static string Secure = "secure";
        private static string HostnameOverride = "certificateHostnameOverride";
        private static string ConnectionPoolSize = "connectionPoolSize";
        private static string SharedSecret = "sharedSecret";

        private static string DasEnvironment = "dasEnvironment";
        private static string DasAddress = "dasAddress";
        private static string DasPort = "dasPort";
        private static string DasSecure = "dasSecure";
        private static string DasCertificateHostnameOverride = "dasCertificateHostnameOverride";
        private static string DasConnectionPoolSize = "dasConnectionPoolSize";
        private static string DasLogin = "dasLogin";
        private static string DasPassword = "dasPassword";
        private static string UseDasDatareader = "useDasDatareader";

        private static string Level = "logLevel";
        private static string ProfileName = "profileName";

        public XmlSettingsRepository()
        {
            FileName = HostingEnvironment.MapPath("~/App_Data/settings.config");
            BackupFilename = HostingEnvironment.MapPath("~/App_Data/settings.config.bak");
        }

        public Settings Get()
        {
            // Read from XML file
            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                Settings settings = Parse(fs);
                return settings;
            }
        }

        private static Settings Parse(Stream fs)
        {
            var doc = XDocument.Load(fs);
            var colleagueElement = doc.Descendants(Colleague).FirstOrDefault();
            var collSettings = new ColleagueSettings();
            var dmi = new DmiSettings();
            var das = new DasSettings();
            var general = new GeneralSettings();

            XElement elem = null;

            elem = doc.Descendants(AccountName).FirstOrDefault();
            if (elem != null)
            {
                dmi.AccountName = elem.Value;
            }

            elem = doc.Descendants(IpAddress).FirstOrDefault();
            if (elem != null)
            {
                dmi.IpAddress = elem.Value;
            }

            elem = doc.Descendants(Port).FirstOrDefault();
            if (elem != null)
            {
                int result = 0;
                int.TryParse(elem.Value, out result);
                dmi.Port = result;
            }

            elem = doc.Descendants(ConnectionPoolSize).FirstOrDefault();
            if (elem != null)
            {
                short result = 0;
                short.TryParse(elem.Value, out result);
                dmi.ConnectionPoolSize = result;
            }

            elem = doc.Descendants(Secure).FirstOrDefault();
            if (elem != null)
            {
                bool result = false;
                bool.TryParse(elem.Value, out result);
                dmi.Secure = result;
            }

            elem = doc.Descendants(HostnameOverride).FirstOrDefault();
            if (elem != null)
            {
                dmi.HostNameOverride = elem.Value;
            }

            elem = doc.Descendants(SharedSecret).FirstOrDefault();
            if (elem != null)
            {
                dmi.SharedSecret = Decrypt(elem.Value);
            }

            collSettings.DmiSettings = dmi;


            elem = doc.Descendants(DasEnvironment).FirstOrDefault();
            if (elem != null)
            {
                das.AccountName = elem.Value;
            }

            elem = doc.Descendants(DasAddress).FirstOrDefault();
            if (elem != null)
            {
                das.IpAddress = elem.Value;
            }

            elem = doc.Descendants(DasPort).FirstOrDefault();
            if (elem != null)
            {
                int result = 0;
                int.TryParse(elem.Value, out result);
                das.Port = result;
            }

            elem = doc.Descendants(DasConnectionPoolSize).FirstOrDefault();
            if (elem != null)
            {
                short result = 0;
                short.TryParse(elem.Value, out result);
                das.ConnectionPoolSize = result;
            }

            elem = doc.Descendants(DasSecure).FirstOrDefault();
            if (elem != null)
            {
                bool result = false;
                bool.TryParse(elem.Value, out result);
                das.Secure = result;
            }

            elem = doc.Descendants(DasCertificateHostnameOverride).FirstOrDefault();
            if (elem != null)
            {
                das.HostNameOverride = elem.Value;
            }

            elem = doc.Descendants(DasLogin).FirstOrDefault();
            if (elem != null)
            {
                das.DbLogin = Decrypt(elem.Value);
            }

            elem = doc.Descendants(DasPassword).FirstOrDefault();
            if (elem != null)
            {
                das.DbPassword = Decrypt(elem.Value);
            }

            collSettings.DasSettings = das;

            elem = doc.Descendants(UseDasDatareader).FirstOrDefault();
            if (elem != null)
            {
                bool result = false;
                bool.TryParse(elem.Value, out result);
                general.UseDasDatareader = result;
            }

            collSettings.GeneralSettings = general;

            var logLevel = SourceLevels.Off;
            var logElement = doc.Descendants(Level).FirstOrDefault();
            if (logElement != null)
            {
                logLevel = EnterpriseLibraryLoggerAdapter.LevelFromString(logElement.Value);
            }

            var profileName = string.Empty;
            elem = doc.Descendants(ProfileName).FirstOrDefault();
            if (elem != null)
            {
                profileName = elem.Value;
            }


            return new Settings(collSettings, logLevel) { ProfileName = profileName };
        }

        public void Update(Settings settings)
        {
            var root = new XElement(Settings);

            var colleague = new XElement(Colleague);
            colleague.Add(new XElement(AccountName, settings.ColleagueSettings.DmiSettings.AccountName));
            colleague.Add(new XElement(IpAddress, settings.ColleagueSettings.DmiSettings.IpAddress));
            colleague.Add(new XElement(Port, settings.ColleagueSettings.DmiSettings.Port.ToString()));
            colleague.Add(new XElement(Secure, settings.ColleagueSettings.DmiSettings.Secure.ToString()));
            colleague.Add(new XElement(HostnameOverride, settings.ColleagueSettings.DmiSettings.HostNameOverride));
            colleague.Add(new XElement(ConnectionPoolSize, settings.ColleagueSettings.DmiSettings.ConnectionPoolSize.ToString()));
            colleague.Add(new XElement(SharedSecret, Encrypt(settings.ColleagueSettings.DmiSettings.SharedSecret)));
            
            colleague.Add(new XElement(DasEnvironment, settings.ColleagueSettings.DasSettings.AccountName));
            colleague.Add(new XElement(DasAddress, settings.ColleagueSettings.DasSettings.IpAddress));
            colleague.Add(new XElement(DasPort, settings.ColleagueSettings.DasSettings.Port.ToString()));
            colleague.Add(new XElement(DasSecure, settings.ColleagueSettings.DasSettings.Secure.ToString()));
            colleague.Add(new XElement(DasCertificateHostnameOverride, settings.ColleagueSettings.DasSettings.HostNameOverride));
            colleague.Add(new XElement(DasConnectionPoolSize, settings.ColleagueSettings.DasSettings.ConnectionPoolSize.ToString()));
            colleague.Add(new XElement(DasLogin, Encrypt(settings.ColleagueSettings.DasSettings.DbLogin)));
            colleague.Add(new XElement(DasPassword, Encrypt(settings.ColleagueSettings.DasSettings.DbPassword)));

            colleague.Add(new XElement(UseDasDatareader, settings.ColleagueSettings.GeneralSettings.UseDasDatareader.ToString()));
            
            root.Add(colleague);

            var logging = new XElement(Level, settings.LogLevel.ToString());
            root.Add(logging);

            var profileName = new XElement(ProfileName, settings.ProfileName);
            root.Add(profileName);

            // Save settings
            try
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    root.Save(fs);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new Exception("No access to update '" + FileName + "'. " +
                    "Verify the application pool is running as an identity with permissions to update the App_Data folder.", uae);
            }

            // Create settings backup copy
            try
            {
                File.Copy(FileName, BackupFilename, true);
            }
            catch (UnauthorizedAccessException uae)
            {
                throw new Exception("No access to update file '" + BackupFilename + "'. " +
                    "Verify the application pool is running as an identity with permissions to update the App_Data folder.", uae);
            }            

        }

        private static string Encrypt(string plainText)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            try
            {
                return MachineKey.Encode(plaintextBytes, MachineKeyProtection.All);
            }
            catch
            {
                return plainText;
            }
        }

        private static string Decrypt(string encrypted)
        {
            try
            {
                var decryptedBytes = MachineKey.Decode(encrypted, MachineKeyProtection.All);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return encrypted;
            }
        }
    }
}
