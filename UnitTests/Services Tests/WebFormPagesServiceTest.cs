using System.Collections.Generic;
using Entities;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Contracts;
using Services.Implementation;
using Moq;
using System.Linq;

namespace UnitTests
{
    [TestClass()]
    public class WebFormPagesServiceTest
    {
        #region Member Variables

        private readonly Mock<IRegexService> _mockRegexService;
        private readonly WebFormPagesService_Accessor _target;

        #endregion Member Variables

        #region Constructor

        public WebFormPagesServiceTest()
        {
            _mockRegexService = new Mock<IRegexService>();
            ProjectItem mockProjectItem = VisualStudioHelper.CreateProjectItem("");
            _target = new WebFormPagesService_Accessor(_mockRegexService.Object);
        }

        #endregion Constructor

        #region Test Methods

        [TestMethod()]
        public void AddToParentsRecursiveTest()
        {
            //Arrange
            Page e = new Page { Name = "e", Parents = new List<Page>() };
            Page c = new Page { Name = "c", Parents = new List<Page> { e } };
            Page b = new Page { Name = "b", Parents = new List<Page> { c, e } };
            Page a = new Page { Name = "a", Parents = new List<Page> { b } };

            //Act
            var parents = new Dictionary<string, Page>();
            _target.AddToParentsRecursive(a, parents);

            //Assert
            Assert.AreEqual(3, parents.Count);
        }

        [TestMethod()]
        public void FilterPagesTest()
        {
            //Arrange
            var foo = new Page { Name = "foo.aspx" };
            var bar = new Page { Name = "bar.ascx" };
            var guf = new Page { Name = "guf.Master" };
            var baz = new Page { Name = "baz.html" };
            IEnumerable<Page> pages = new Page[4] { foo, bar, guf, baz };

            //Arrange
            IEnumerable<Page> actual = _target.FilterPages(pages);

            //Act
            Assert.AreEqual(3, actual.Count());
        }

        [TestMethod()]
        public void GetControlsTest()
        {
            //Act
            IEnumerable<string> actual = _target.GetControls(It.IsAny<string>());

            //Assert
            _mockRegexService.Verify(r => r.GetWebControls(It.IsAny<string>()), Times.Once());
        }

        [TestMethod()]
        public void GetMasterPageTest()
        {
            //Act
            _target.GetMasterPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<Page>>(), It.IsAny<List<Page>>());

            //Assert
            _mockRegexService.Verify(r => r.GetMasterPageName(It.IsAny<string>()), Times.Once());
        }

        [TestMethod()]
        public void GetReferencesTest()
        {
            //Arrange
            Page page = new Page();
            string[] referencesList = new string[3] { "_foo", "_foo", "_bar" };
            _mockRegexService.Setup(p => p.GetReferences(It.IsAny<string>())).Returns(referencesList);

            //Act
            var actual = _target.GetReferences(page);

            //Assert
            _mockRegexService.Verify(r => r.GetReferences(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(2, actual.Count());
            Assert.AreEqual(actual["foo"], 2);
            Assert.AreEqual(actual["bar"], 1);
        }

        [TestMethod()]
        public void GetDuplicatesInLayoutAndFileTest()
        {
            //Arrange
            IPagesService pagesService = new WebFormPagesService(_mockRegexService.Object);
            var commonReference = "foo";
           
            Page parentPage = new Page
            {
                References = new Dictionary<string, int> { { commonReference, 1 }, { "bar", 1 } },
                Parents = new List<Page> ()
            };
            Page childPage = new Page
            {
                References = new Dictionary<string, int> { { commonReference, 1 }},
                Parents = new List<Page> { parentPage }
            };
            IEnumerable<Page> pagesList = new Page[2] { parentPage, childPage };

            //Act
            var result = pagesService.GetDuplicatesInLayoutAndFile(pagesList);

            //Assert
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.First().Duplicates.Count(), 1);
            Assert.AreEqual(result.First().Duplicates.ElementAt(0), commonReference);
        }

        [TestMethod()]
        public void GetDuplicatesInSameFileTest()
        {
            //Arrange
            IPagesService pagesService = new WebFormPagesService(_mockRegexService.Object);
            IEnumerable<Page> pagesList = new Page[2]
            {
                new Page{References=new Dictionary<string,int>{{"shouldn't appear",1},{"ref",2}}},
                new Page{References=new Dictionary<string,int>()}
            };

            //Act
            var result = pagesService.GetDuplicatesInSameFile(pagesList);

            //Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1, result.First().References.Count);
        }

        #endregion
    }
}