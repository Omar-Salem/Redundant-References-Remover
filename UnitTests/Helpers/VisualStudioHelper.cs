// -----------------------------------------------------------------------
// <copyright file="VisualStudioHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EnvDTE;
    using Moq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class VisualStudioHelper
    {
        public static ProjectItem CreateProjectItem(string name)
        {
            var projectItem = new Mock<ProjectItem>(MockBehavior.Loose);
            projectItem.Setup(x => x.Name).Returns(name);

            return projectItem.Object;
        }

        public static ProjectItems CreateProjectItemsFrom(params ProjectItem[] oneOrMoreProjectItems)
        {
            var mockProjectItems = new Mock<ProjectItems>();
            List<ProjectItem> listOfProjectItems = new List<ProjectItem>();

            foreach (ProjectItem mockProjectItem in oneOrMoreProjectItems)
            {
                listOfProjectItems.Add(mockProjectItem);
            }

            mockProjectItems.Setup(x => x.Count).Returns(oneOrMoreProjectItems.Length);
            mockProjectItems.Setup(x => x.GetEnumerator()).Returns(listOfProjectItems.GetEnumerator());
            return mockProjectItems.Object;
        }
    }
}
