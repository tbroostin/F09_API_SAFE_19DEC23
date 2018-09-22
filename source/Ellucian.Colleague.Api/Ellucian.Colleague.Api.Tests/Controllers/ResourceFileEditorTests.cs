using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Resource;
using Ellucian.Web.Resource.Repositories;
using Ellucian.Colleague.Api.Controllers;
using Moq;
using slf4net;
using System.IO;
using Newtonsoft.Json;
using System.Web.Mvc;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Api.Tests.Controllers
{
    [TestClass]
    public class ResourceFileEditorTests
    {
        string resxFileDirectory ;
        string resxFileName ;
        string largeResxFileName ;
        private ResourceFileEditorController rfeController;
        private Mock<ILogger> loggerMock;
        private ApiSettings FakeApiSettings;

        IResourceRepository localRepo;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            resxFileName="TestResxFile.resx";
            largeResxFileName = "LargeResxFile.resx";

            FakeApiSettings = new ApiSettings(1, "fakeName", 1);

            resxFileDirectory =Path.Combine(AppDomain.CurrentDomain.BaseDirectory,  "..\\..\\..\\Ellucian.Colleague.Api.Tests\\Resources\\");
            localRepo = new LocalResourceRepository(resxFileDirectory);

            rfeController = new ResourceFileEditorController(localRepo, FakeApiSettings, loggerMock.Object);
        }

        [TestMethod]
        [Ignore]
        public void GetResourceItemsByFileTest()
        {
            //Check if the method retrieves the resx files from specified path
           
            JsonResult res =(JsonResult)rfeController.GetResourceItemsByFile(resxFileName);
            //Check if the resx file has the three name/value pairs
            ResourceFile resxFile = (ResourceFile)(res.Data);

            Assert.AreEqual(3, resxFile.ResourceFileEntries.Count);
            Assert.AreEqual("TestResxFile.resx", resxFile.ResourceFileName);

        }

        [TestMethod]
        [Ignore]
        public void SaveResxFile()
        {
            //Check saving a smaller file
            //Copy the small file to TestResources
            File.Copy(Path.Combine(resxFileDirectory, resxFileName), Path.Combine(resxFileDirectory, "TestResources", resxFileName),overwrite:true);

            ResourceFileModel resFile = new ResourceFileModel(Path.Combine(resxFileDirectory, "TestResources", resxFileName));
            JsonResult res = (JsonResult)rfeController.GetResourceItemsByFile(resxFileName);
            ResourceFile resxFile = (ResourceFile)(res.Data);
            resFile.ResourceFileEntries = new List<ResourceFileEntryModel>();

            foreach(ResourceFileEntry item in resxFile.ResourceFileEntries)
            {
                ResourceFileEntryModel entry = new ResourceFileEntryModel();
                entry.Key = item.Key;
                entry.OriginalValue = item.OriginalValue;
                entry.Value = "Changed";
                entry.Comment = item.Comment;
                 
                resFile.ResourceFileEntries.Add(entry);
            }

            //Save the file under TestResources
            LocalResourceRepository localRepository = new LocalResourceRepository(Path.Combine(resxFileDirectory, "TestResources"));

            ResourceFileEditorController rfeTestController = new ResourceFileEditorController(localRepository, FakeApiSettings, loggerMock.Object);
            string model = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(resFile);
            rfeTestController.SaveResourceFile(model);

            //Check if value of the file under TestResources has been modified
            JsonResult modifiedJsonresult = (JsonResult)rfeTestController.GetResourceItemsByFile(resxFileName);
            //Check if the resx file has the three name/value pairs
            ResourceFile modifedresxFile = (ResourceFile)(modifiedJsonresult.Data);

            foreach(ResourceFileEntry item in modifedresxFile.ResourceFileEntries)
            {
                Assert.AreEqual("Changed", item.Value);

            }


        }

        [TestMethod]
        [Ignore]
        public void SaveLargeResxFile()
        {
            //Check saving a larger file
            File.Copy(Path.Combine(resxFileDirectory, largeResxFileName), Path.Combine(resxFileDirectory, "TestResources", largeResxFileName), overwrite: true);

            ResourceFileModel resFile = new ResourceFileModel(Path.Combine(resxFileDirectory, "TestResources", largeResxFileName));
            JsonResult res = (JsonResult)rfeController.GetResourceItemsByFile(largeResxFileName);
            ResourceFile resxFile = (ResourceFile)(res.Data);
            resFile.ResourceFileEntries = new List<ResourceFileEntryModel>();

            foreach (ResourceFileEntry item in resxFile.ResourceFileEntries)
            {
                ResourceFileEntryModel entry = new ResourceFileEntryModel();
                entry.Key = item.Key;
                entry.OriginalValue = item.OriginalValue;
                entry.Value = "Changed to a different value";
                entry.Comment = item.Comment;

                resFile.ResourceFileEntries.Add(entry);
            }

            //Save the file under TestResources
            LocalResourceRepository localRepository = new LocalResourceRepository(Path.Combine(resxFileDirectory, "TestResources"));

            ResourceFileEditorController rfeTestController = new ResourceFileEditorController(localRepository, FakeApiSettings, loggerMock.Object);
            string model = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(resFile);

            rfeTestController.SaveResourceFile(model);

            //Check if value of the file under TestResources has been modified
            JsonResult modifiedJsonresult = (JsonResult)rfeTestController.GetResourceItemsByFile(largeResxFileName);
            //Check if the resx file has the three name/value pairs
            ResourceFile modifedresxFile = (ResourceFile)(modifiedJsonresult.Data);

            foreach (ResourceFileEntry item in modifedresxFile.ResourceFileEntries)
            {
                Assert.AreEqual("Changed to a different value", item.Value);

            }
        }
    }
}
