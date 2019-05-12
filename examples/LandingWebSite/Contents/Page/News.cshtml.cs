﻿using BrandUp.Pages;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace LandingWebSite.Contents.Page
{
    [PageContent(Title = "News page")]
    public class NewsPageContent : PageContent
    {
        [Text(Placeholder = "Input page header")]
        public string Header { get; set; }

        [Text(Placeholder = "Input page sub header")]
        public string SubHeader { get; set; }

        [ContentList]
        public List<PageBlockContent> Blocks { get; set; }
    }
}