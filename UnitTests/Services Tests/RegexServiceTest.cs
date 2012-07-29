using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Contracts;
using Services.Implementation;

namespace UnitTests
{
    [TestClass()]
    public class RegexServiceTest
    {
        #region Member Variables

        private readonly IRegexService target;

        #endregion Member Variables

        #region Constructor

        public RegexServiceTest()
        {
            target = new RegexService();
        }

        #endregion Constructor

        #region Test Methods

        [TestMethod()]
        public void GetReferencesTest()
        {
            //Arrange
            string pageContent = Resources.mvc3DefaultLayout;

            //Act
            IEnumerable<string> actual = target.GetReferences(pageContent);

            //Assert
            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(@"/Site.css", actual.ElementAt(0));
            Assert.AreEqual(@"/jquery-1.5.1.min.js", actual.ElementAt(1));
        }

        [TestMethod()]
        public void GetWebControlsTest()
        {
            //Arrange
            string content = Resources.webPage;

            //Act
            IEnumerable<string> actual = target.GetWebControls(content);

            //Assert
            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual("WebUserControl1.ascx", actual.First());
        }

        [TestMethod()]
        public void GetPartialViewsTest()
        {
            //Arrange
            string content = Resources.mvc3PageWithChild;

            //Act
            IEnumerable<string> actual = target.GetPartialViews(content);

            //Assert
            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual("Child", actual.First());
        }

        [TestMethod()]
        public void GetMultipleLayoutsTest()
        {
            //Arrange
            string content = Resources.mvc3MultipleLayouts;

            //Act
            IEnumerable<string> actual = target.GetMultipleLayouts(content);

            //Assert
            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual("/DefaultLayout.cshtml", actual.ElementAt(0));
            Assert.AreEqual("/AnotherLayout.cshtml", actual.ElementAt(1));
        }

        [TestMethod()]
        public void GetMasterPageNameTest()
        {
            //Arrange
            string content = Resources.webPage;
            string expected = @"/Site.master""";

            //Act
            string actual = target.GetMasterPageName(content);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDefaultLayoutPageNameTest()
        {
            //Arrange
            string content = Resources.mvc3ViewStart;
            string expected = "/_Layout.cshtml";

            //Act
            string actual = target.GetDefaultLayoutPageName(content);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Check_Page_Has_No_LayoutTest()
        {
            //Arrange
            string content = Resources.mvc3WithDefaultLayout;

            //Act
            bool actual = target.CheckPageHasNullLayout(content);

            //Act
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void Check_Page_Has_LayoutTest()
        {
            //Arrange
            string content = Resources.mvc3PageWithAnotherLayout;

            //Act
            bool actual = target.CheckPageHasNullLayout(content);

            //Act
            Assert.IsFalse(actual);
        }

        #endregion Test Methods
    }
}