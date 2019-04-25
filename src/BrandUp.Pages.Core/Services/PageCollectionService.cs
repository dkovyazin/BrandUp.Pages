﻿using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageCollectionService : IPageCollectionService
    {
        private readonly IPageCollectionRepositiry repositiry;
        private readonly IPageRepositiry pageRepositiry;
        private readonly IPageMetadataManager pageMetadataManager;

        public PageCollectionService(IPageCollectionRepositiry repositiry, IPageRepositiry pageRepositiry, IPageMetadataManager pageMetadataManager)
        {
            this.repositiry = repositiry ?? throw new ArgumentNullException(nameof(repositiry));
            this.pageRepositiry = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
            this.pageMetadataManager = pageMetadataManager ?? throw new ArgumentNullException(nameof(pageMetadataManager));
        }

        public Task<IPageCollection> CreateCollectionAsync(string title, string pageTypeName, PageSortMode sortMode, Guid? pageId)
        {
            return repositiry.CreateCollectionAsync(title, pageTypeName, sortMode, pageId);
        }

        public Task<IPageCollection> FindCollectiondByIdAsync(Guid id)
        {
            return repositiry.FindCollectiondByIdAsync(id);
        }

        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(Guid? pageId)
        {
            return repositiry.GetCollectionsAsync(pageId);
        }

        public Task<IEnumerable<IPageCollection>> GetCollectionsAsync(string pageTypeName, bool includeDerivedTypes)
        {
            if (pageTypeName == null)
                throw new ArgumentNullException(nameof(pageTypeName));

            var pageMetadata = pageMetadataManager.FindPageMetadataByName(pageTypeName);
            if (pageMetadata == null)
                throw new ArgumentException($"Тип страницы {pageTypeName} не существует.");

            var pageTypeNames = new List<string>
            {
                pageMetadata.Name
            };

            if (includeDerivedTypes)
            {
                foreach (var derivedPageMetadata in pageMetadata.GetDerivedMetadataWithHierarhy(false))
                    pageTypeNames.Add(derivedPageMetadata.Name);
            }

            return repositiry.GetCollectionsAsync(pageTypeNames.ToArray());
        }

        public Task<IPageCollection> UpdateCollectionAsync(Guid id, string title, PageSortMode pageSort)
        {
            return repositiry.UpdateCollectionAsync(id, title, pageSort);
        }

        public async Task<Result> DeleteCollectionAsync(IPageCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (await pageRepositiry.HasPagesAsync(collection.Id))
                return Result.Failed("Нельзя удалить коллекцию страниц, которая содержит страницы.");

            try
            {
                await repositiry.DeleteCollectionAsync(collection.Id);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failed(ex);
            }
        }
    }
}