﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Content.Views;
using System.Collections.Generic;

namespace BrandUp.Pages.ContentModels
{
    [ContentModel(Title = ContentTypeTitle, Description = ContentTypeDescription)]
    [ViewDefinition("Default")]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";
        public const string ContentTypeDescription = "Test page description";

        [View]
        public string ViewName { get; set; }

        [Text(title: "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [ContentValue(title: "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [ContentList(title: "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

        public static TestPageContent CreateWithOnlyTitle(string title)
        {
            return Create(title, null, null);
        }
        public static TestPageContent Create(string title, PageHeaderContent header, IEnumerable<PageHeaderContent> headers)
        {
            return new TestPageContent
            {
                ViewName = "TestPage.default",
                Title = title,
                Header = header,
                Headers = headers != null ? new List<PageHeaderContent>(headers) : null
            };
        }
    }

    [ContentModel(Title = "Заголовок", Description = "Заголовок страницы")]
    public class PageHeaderContent
    {
        [Text(title: "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";
    }
}