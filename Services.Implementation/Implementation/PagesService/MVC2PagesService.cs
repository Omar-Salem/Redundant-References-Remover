using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Services.Contracts;
using EnvDTE;

namespace Services.Implementation
{
    public class MVC2PagesService : WebFormPagesService
    {
        #region Constructor

        public MVC2PagesService(IRegexService regexService)
            : base(regexService)
        {
        }

        #endregion Constructor

        #region Overridden Methods

        protected override IEnumerable<string> GetControls(string pageContent)
        {
            return _regexService.GetPartialViews(pageContent).Concat(_regexService.GetWebControls(pageContent));
        }

        #endregion Overriden Methods
    }
}