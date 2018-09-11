using System;
using System.IO;
using appFBLA2019;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace appFBLA2019_Tests
{
    [TestClass]
    public class DatabaseTests
    {
        private const string sqLiteExtension = ".db3";
        
        [TestMethod]
        public void TestFileCreation()
        {
            string fileName = "newFile";
            DBHandler.SelectDatabase(fileName);

            string path = Path.Combine(
                          Environment.GetFolderPath(
                              Environment.SpecialFolder.LocalApplicationData), fileName + sqLiteExtension);

            Assert.IsTrue(Directory.GetFiles(path).Length != 0);
        }
    }
}
