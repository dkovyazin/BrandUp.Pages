﻿using BrandUp.Pages.Builder;
using BrandUp.Pages.ContentModels;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Pages.Services
{
    public class PageCollectionServiceTests : IAsyncLifetime
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private readonly IPageService pageService;
        private readonly IPageCollectionService pageCollectionService;
        private readonly IPageMetadataManager pageMetadataManager;

        public PageCollectionServiceTests()
        {
            var services = new ServiceCollection();

            services.AddPages()
                .AddContentTypesFromAssemblies(typeof(TestPageContent).Assembly)
                .AddFakes();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();

            pageService = serviceScope.ServiceProvider.GetService<IPageService>();
            pageCollectionService = serviceScope.ServiceProvider.GetService<IPageCollectionService>();
            pageMetadataManager = serviceScope.ServiceProvider.GetService<IPageMetadataManager>();
        }

        #region IAsyncLifetime members

        async Task IAsyncLifetime.InitializeAsync()
        {
            var pageCollectionRepository = serviceScope.ServiceProvider.GetService<IPageCollectionRepository>();
            var pageRepository = serviceScope.ServiceProvider.GetService<IPageRepository>();

            var pageType = pageMetadataManager.FindPageMetadataByContentType(typeof(TestPageContent));

            var pageCollection = await pageCollectionRepository.CreateCollectionAsync("Test collection", pageType.Name, PageSortMode.FirstOld, null);

            var mainPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(mainPage, "index");
            await pageRepository.UpdatePageAsync(mainPage);

            var testPage = await pageRepository.CreatePageAsync(pageCollection.Id, pageType.Name, "test", pageType.ContentMetadata.ConvertContentModelToDictionary(TestPageContent.CreateWithOnlyTitle("test")));
            await pageRepository.SetUrlPathAsync(testPage, "test");
            await pageRepository.UpdatePageAsync(testPage);
        }
        Task IAsyncLifetime.DisposeAsync()
        {
            serviceScope.Dispose();
            serviceProvider.Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region Test methods

        [Fact]
        public async Task CreateCollection_root()
        {
            var result = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null);

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Test collection", result.Data.Title);
            Assert.Equal("TestPage", result.Data.PageTypeName);
            Assert.Null(result.Data.PageId);
            Assert.Equal(PageSortMode.FirstOld, result.Data.SortMode);
        }

        [Fact]
        public async Task CreateCollection_bypage()
        {
            var defaultPage = await pageService.GetDefaultPageAsync();
            var result = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, defaultPage.Id);

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Test collection", result.Data.Title);
            Assert.Equal("TestPage", result.Data.PageTypeName);
            Assert.Equal(defaultPage.Id, result.Data.PageId);
            Assert.Equal(PageSortMode.FirstOld, result.Data.SortMode);
        }

        [Fact]
        public async Task CreateCollection_Fail_PageNotPublished()
        {
            var parentPageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();
            var page = await pageService.CreatePageAsync(parentPageCollection);

            var pageCollection = await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, page.Id);

            Assert.False(pageCollection.Succeeded);
        }

        [Fact]
        public async Task FindCollectiondById()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null)).Data;

            var findedPageCollection = await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id);

            Assert.NotNull(findedPageCollection);
            Assert.Equal(pageCollection.Id, findedPageCollection.Id);
        }

        [Fact]
        public async Task UpdateCollection()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null)).Data;

            pageCollection.SetTitle("New title");
            pageCollection.SetSortModel(PageSortMode.FirstNew);

            var result = await pageCollectionService.UpdateCollectionAsync(pageCollection);

            Assert.True(result.Succeeded);
            Assert.Equal("New title", pageCollection.Title);
            Assert.Equal(PageSortMode.FirstNew, pageCollection.SortMode);
        }

        [Fact]
        public async Task DeleteCollection()
        {
            var pageCollection = (await pageCollectionService.CreateCollectionAsync("Test collection", "TestPage", PageSortMode.FirstOld, null)).Data;

            var result = await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.True(result.Succeeded);
            Assert.Null(await pageCollectionService.FindCollectiondByIdAsync(pageCollection.Id));
        }

        [Fact]
        public async Task DeleteCollection_Fail_HavePages()
        {
            var pageCollection = (await pageCollectionService.GetCollectionsAsync(null)).First();

            var result = await pageCollectionService.DeleteCollectionAsync(pageCollection);

            Assert.False(result.Succeeded);
        }

        #endregion
    }
}