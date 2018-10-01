﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Api.Controllers
{
    [ApiController]
    [Route("api/content/html")]
    public class HtmlFieldController : Controller
    {
        private readonly IPageEditingService editingService;
        private readonly IContentMetadataManager contentMetadataManager;

        public HtmlFieldController(IContentMetadataManager contentMetadataManager, IPageEditingService editingService)
        {
            this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
            this.editingService = editingService ?? throw new ArgumentNullException(nameof(editingService));
        }

        [HttpPut("{path?}")]
        public async Task<ActionResult> SetAsync([FromQuery(Name = "esid")]Guid editSessionId, string path, [FromQuery]string fieldName, [FromBody]string value)
        {
            var editSession = await editingService.FindEditSessionById(editSessionId);
            if (editSession == null)
                return BadRequest();

            var pageContentModel = await editingService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContentModel);

            var contentExplorer = pageContentExplorer.Navigate(path ?? string.Empty);

            if (!contentExplorer.Metadata.TryGetField(fieldName, out HtmlField field))
                return BadRequest();

            field.SetModelValue(contentExplorer.Content, !string.IsNullOrWhiteSpace(value) ? value : null);

            await editingService.SetContentAsync(editSession, pageContentExplorer.Content);

            return Accepted();
        }
    }
}