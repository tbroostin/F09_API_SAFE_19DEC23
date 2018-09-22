﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Ellucian.Web.Http.Tests.Utilities.TestData
{
    public class DataPrivacyTestData
    {
        private const string SinglePersonObjectString = "{\"privacyStatus\":{\"privacyCategory\":\"unrestricted\"},\"names\":[{\"type\":{\"category\":\"legal\",\"detail\":{\"id\":\"56456805-30a6-4731-bf40-25602047b5b0\"}},\"fullName\":\"Miss Fran Green\",\"title\":\"Miss\",\"firstName\":\"Fran\",\"lastName\":\"Green\",\"preference\":\"preferred\"}],\"dateOfBirth\":\"1970-12-15\",\"gender\":\"female\",\"roles\":[{\"role\":\"student\"}],\"credentials\":[{\"type\":\"colleaguePersonId\",\"value\":\"0000261\"}],\"addresses\":[{\"address\":{\"id\":\"1b4f2e85-6bc0-4fce-88b0-19fbce699a86\"},\"type\":{\"addressType\":\"home\",\"detail\":{\"id\":\"40a45273-86ba-47b0-b1f5-4637a145e777\"}},\"startOn\":\"2001-10-08T00:00:00\",\"preference\":\"primary\"}],\"emails\":[{\"type\":{\"emailType\":\"other\",\"detail\":{\"id\":\"c7ab941a-0c8b-452c-8f12-6852cb0366c8\"}},\"preference\":\"primary\",\"address\":\"grangreen@yahoo.com\"}],\"metadata\":{},\"id\":\"8c7b8432-141d-4f87-b130-5c87dbdf6266\"}";
        private const string SinglePersonWithoutGenderDateOfBirthColleagueCredentials = "{ \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Miss Fran Green\", \"title\": \"Miss\", \"firstName\": \"Fran\", \"lastName\": \"Green\", \"preference\": \"preferred\" }], \"roles\": [{ \"role\": \"student\" }], \"addresses\": [{ \"address\": { \"id\": \"1b4f2e85-6bc0-4fce-88b0-19fbce699a86\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"preference\": \"primary\", \"address\": \"grangreen@yahoo.com\" }], \"id\": \"8c7b8432-141d-4f87-b130-5c87dbdf6266\" }";
        private const string FivePersonObjectString = "[{ \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Mr. James Green\", \"title\": \"Mr.\", \"firstName\": \"James\", \"lastName\": \"Green\", \"preference\": \"preferred\" }, { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"56a5d0df-ccc3-49f4-8794-4b3db3b6befa\" } }, \"fullName\": \"Janice Chosengreen\", \"firstName\": \"Janice\", \"lastName\": \"Chosengreen\" }, { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Jim Green\", \"firstName\": \"Jim\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1980-01-06\", \"gender\": \"male\", \"roles\": [{ \"role\": \"vendor\" }, { \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000262\" }], \"addresses\": [{ \"address\": { \"id\": \"63d6d456-1f95-4590-b640-6a10bebaa02a\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"metadata\": {  }, \"id\": \"3ee9d934-4654-4ae1-bcae-09265f96d0c0\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Ms. Linda Green\", \"title\": \"Ms.\", \"firstName\": \"Linda\", \"lastName\": \"Green\", \"preference\": \"preferred\" }], \"dateOfBirth\": \"1985-04-17\", \"gender\": \"female\", \"maritalStatus\": { \"maritalCategory\": \"single\", \"detail\": { \"id\": \"556ceb79-893d-4d66-ab00-79c96ccb0343\" } }, \"citizenshipCountry\": \"USA\", \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000263\" }], \"addresses\": [{ \"address\": { \"id\": \"9a81a44c-8963-4438-a645-80ae21361692\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"greenl@yahoo.com\" }], \"metadata\": {  }, \"id\": \"43e98714-7818-40cb-b150-d50cd960501d\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Ms. Margaret Green\", \"title\": \"Ms.\", \"firstName\": \"Margaret\", \"lastName\": \"Green\", \"preference\": \"preferred\" }, { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Peg Green\", \"firstName\": \"Peg\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1972-07-09\", \"gender\": \"female\", \"roles\": [{ \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000264\" }], \"addresses\": [{ \"address\": { \"id\": \"f8c8ee12-cb00-447a-84e7-7dcb5fc42e73\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"metadata\": {  }, \"id\": \"34a8548a-21a6-4482-96cf-2003114ed531\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Mr. Marc Green\", \"title\": \"Mr.\", \"firstName\": \"Marc\", \"lastName\": \"Green\", \"preference\": \"preferred\" }, { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Mark Green\", \"firstName\": \"Mark\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1979-05-27\", \"gender\": \"male\", \"maritalStatus\": { \"maritalCategory\": \"single\", \"detail\": { \"id\": \"556ceb79-893d-4d66-ab00-79c96ccb0343\" } }, \"citizenshipCountry\": \"USA\", \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000265\" }], \"addresses\": [{ \"address\": { \"id\": \"f5d7e660-d1ff-4456-b54c-c97ec4de3aaa\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"marcgreen@aol.com\" }], \"metadata\": {  }, \"id\": \"acbe81be-f069-401c-9aa2-844363c5312e\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [{ \"type\": { \"category\": \"legal\", \"detail\": { \"id\": \"56456805-30a6-4731-bf40-25602047b5b0\" } }, \"fullName\": \"Mr. Jonas Grumby\", \"title\": \"Mr.\", \"firstName\": \"Jonas\", \"lastName\": \"Grumby\", \"preference\": \"preferred\" }], \"dateOfBirth\": \"1980-11-25\", \"gender\": \"male\", \"maritalStatus\": { \"maritalCategory\": \"married\", \"detail\": { \"id\": \"530462cd-9964-4cf6-bd15-c2d3b5b67ffe\" } }, \"roles\": [{ \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000266\" }], \"addresses\": [{ \"address\": { \"id\": \"d45e24e8-af0d-4953-9abf-3888ea986663\" }, \"type\": { \"addressType\": \"home\", \"detail\": { \"id\": \"40a45273-86ba-47b0-b1f5-4637a145e777\" } }, \"startOn\": \"2001-10-08T00:00:00\", \"preference\": \"primary\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"jgrumby@hotmail.com\" }], \"metadata\": {  }, \"id\": \"1740e733-9468-4849-8ccb-d5a1044271b7\" }]";
        private const string FivePersonObjectStringWithoutHomeAddressAndLegalName = "[{ \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [ { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"56a5d0df-ccc3-49f4-8794-4b3db3b6befa\" } }, \"fullName\": \"Janice Chosengreen\", \"firstName\": \"Janice\", \"lastName\": \"Chosengreen\" }, { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Jim Green\", \"firstName\": \"Jim\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1980-01-06\", \"gender\": \"male\", \"roles\": [{ \"role\": \"vendor\" }, { \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000262\" }], \"id\": \"3ee9d934-4654-4ae1-bcae-09265f96d0c0\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"dateOfBirth\": \"1985-04-17\", \"gender\": \"female\", \"maritalStatus\": { \"maritalCategory\": \"single\", \"detail\": { \"id\": \"556ceb79-893d-4d66-ab00-79c96ccb0343\" } }, \"citizenshipCountry\": \"USA\", \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000263\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"greenl@yahoo.com\" }], \"id\": \"43e98714-7818-40cb-b150-d50cd960501d\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [ { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Peg Green\", \"firstName\": \"Peg\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1972-07-09\", \"gender\": \"female\", \"roles\": [{ \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000264\" }], \"id\": \"34a8548a-21a6-4482-96cf-2003114ed531\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"names\": [ { \"type\": { \"category\": \"personal\", \"detail\": { \"id\": \"03286dd6-8415-41fa-8ba1-38cedadef73a\" } }, \"fullName\": \"Mark Green\", \"firstName\": \"Mark\", \"lastName\": \"Green\" }], \"dateOfBirth\": \"1979-05-27\", \"gender\": \"male\", \"maritalStatus\": { \"maritalCategory\": \"single\", \"detail\": { \"id\": \"556ceb79-893d-4d66-ab00-79c96ccb0343\" } }, \"citizenshipCountry\": \"USA\", \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000265\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"marcgreen@aol.com\" }], \"id\": \"acbe81be-f069-401c-9aa2-844363c5312e\" }, { \"privacyStatus\": { \"privacyCategory\": \"unrestricted\" }, \"dateOfBirth\": \"1980-11-25\", \"gender\": \"male\", \"maritalStatus\": { \"maritalCategory\": \"married\", \"detail\": { \"id\": \"530462cd-9964-4cf6-bd15-c2d3b5b67ffe\" } }, \"roles\": [{ \"role\": \"student\" }], \"credentials\": [{ \"type\": \"colleaguePersonId\", \"value\": \"0000266\" }], \"emails\": [{ \"type\": { \"emailType\": \"other\", \"detail\": { \"id\": \"c7ab941a-0c8b-452c-8f12-6852cb0366c8\" } }, \"address\": \"jgrumby@hotmail.com\" }], \"id\": \"1740e733-9468-4849-8ccb-d5a1044271b7\" }]";
        
        public JContainer SinglePersonJContainer { get; private set; }
        public JContainer FivePersonJContainer { get; private set; }
        public List<string> EmptySettingsList { get; private set; }
        
        public DataPrivacyTestData()
        {
            SinglePersonJContainer = JObject.Parse(SinglePersonObjectString);
            FivePersonJContainer = JArray.Parse(FivePersonObjectString);
            EmptySettingsList = new List<string>();
        }

        public List<string> GetGenderDateOfBirthColleagueCredentialsSettings()
        {
            return new List<string>()
            {
               "gender",
               "dateOfBirth",
               "credentials.@type==colleaguePersonId"
            };
        }

        public List<string> GetNonExistentProperties()
        {
            return new List<string>()
            {
               "notReal",
               "reallyNotReal",
               "test.@type==insane",
               "airbase.@age.is==100"
            };
        }
        
        public JContainer GetGenderDateOfBirthColleagueCredentialsJContainer()
        {
            return JObject.Parse(SinglePersonWithoutGenderDateOfBirthColleagueCredentials);
        }

        public List<string> GetHomeAddressAndLegalNameSettings()
        {
            return new List<string>()
            {
               "names.@type.category==legal",
               "addresses.@type.addressType==home"
            };
        }

        public JContainer GetHomeAddressAndLegalNameJContainer()
        {
            return JArray.Parse(FivePersonObjectStringWithoutHomeAddressAndLegalName);
        }
    }
}



