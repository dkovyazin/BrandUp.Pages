﻿using BrandUp.MongoDB;
using BrandUp.Pages.MongoDb.Documents;
using MongoDB.Driver;

namespace LandingWebSite.Models
{
    public class WebSiteDbContext : MongoDbContext, BrandUp.Pages.MongoDb.IPagesDbContext
    {
        public WebSiteDbContext(MongoDbContextOptions options) : base(options) { }

        public IMongoCollection<_migrations.MigrationVersionDocument> Migrations => GetCollection<_migrations.MigrationVersionDocument>();

        #region IPagesDbContext members

        public IMongoCollection<PageCollectionDocument> PageCollections => GetCollection<PageCollectionDocument>();
        public IMongoCollection<PageDocument> Pages => GetCollection<PageDocument>();
        public IMongoCollection<PageContentDocument> Contents => GetCollection<PageContentDocument>();
        public IMongoCollection<PageEditDocument> PageEditSessions => GetCollection<PageEditDocument>();
        public IMongoCollection<PageRecyclebinDocument> PageRecyclebin => GetCollection<PageRecyclebinDocument>();
        public IMongoCollection<PageUrlDocument> PageUrls => GetCollection<PageUrlDocument>();
        public IMongoCollection<ContentEditorDocument> ContentEditors => GetCollection<ContentEditorDocument>();

        #endregion
    }
}