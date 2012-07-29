using System.Collections.Generic;
using System.Linq;
using Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services.Contracts;
using Services.Implementation;
using EnvDTE;

namespace UnitTests
{
    [TestClass()]
    public class MVCPagesServiceTest
    {
        #region Member Variables

        private readonly Mock<IRegexService> _mockRegexService;
        private readonly MVC3PagesService_Accessor _target;

        #endregion Member Variables

        #region Constructor

        public MVCPagesServiceTest()
        {
            _mockRegexService = new Mock<IRegexService>();
            ProjectItem mockProjectItem = VisualStudioHelper.CreateProjectItem("");
            _target = new MVC3PagesService_Accessor(_mockRegexService.Object);
        }

        #endregion Constructor

        #region Test Methods

        [TestMethod()]
        public void FilterPagesTest()
        {
            //Arrange
            var foo = new Page { Name = "foo.cshtml" };
            var bar = new Page { Name = "bar.vbhtml" };
            var guf = new Page { Name = "guf.html" };
            IEnumerable<Page> pages = new Page[3] { foo, bar, guf };

            //Arrange
            IEnumerable<Page> actual = _target.FilterPages(pages);

            //Act
            Assert.AreEqual(2, actual.Count());
        }

        [TestMethod()]
        public void GetControlsTest()
        {
            //Act
            IEnumerable<string> actual = _target.GetControls(It.IsAny<string>());

            //Assert
            _mockRegexService.Verify(r => r.GetPartialViews(It.IsAny<string>()), Times.Once());
        }

        [TestMethod()]
        public void GetDefaultLayoutPageTest()
        {
            //Arrange
            IEnumerable<Page> pages = new Page[1] { new Page { Name = "_ViewStart.cshtml", Content = "foo" } };
            _mockRegexService.Setup(r => r.GetDefaultLayoutPageName(It.IsAny<string>())).Returns("bar");

            //Act
            _target.GetDefaultLayoutPage(pages);

            //Assert
            _mockRegexService.Verify(r => r.GetDefaultLayoutPageName(It.IsAny<string>()), Times.Once());
        }

        [TestMethod()]
        public void GetMasterPageTest()
        {
            //Arrange
            string pageName = "Index.cshtml";
            string pageContent = string.Empty;
            var page = new Page { Name = "_Layout.cshtml" };
            IEnumerable<Page> pagesList = new Page[1] { page };
            List<Page> parentList = new List<Page>();
            _mockRegexService.Setup(r => r.GetMultipleLayouts(It.IsAny<string>())).Returns(new string[1] { page.Name });

            //Act
            _target.GetMasterPage(pageName, pageContent, pagesList, parentList);

            //Assert
            _mockRegexService.Verify(r => r.GetMultipleLayouts(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(parentList.Count, 1);
            Assert.AreEqual(parentList[0], page);
        }

        #endregion

        
    }
}