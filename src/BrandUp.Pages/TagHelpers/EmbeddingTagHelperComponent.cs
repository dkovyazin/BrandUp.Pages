﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.TagHelpers
{
    public class EmbeddingTagHelperComponent : TagHelperComponent
    {
        const string LoadingClass = "bp-state-loading";

        private readonly IJsonHelper jsonHelper;

        [HtmlAttributeNotBound, ViewContext]
        public ViewContext ViewContext { get; set; }

        public EmbeddingTagHelperComponent(IJsonHelper jsonHelper)
        {
            this.jsonHelper = jsonHelper ?? throw new ArgumentNullException(nameof(jsonHelper));
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext.ViewData.Model is AppPageModel appPageModel)
            {
                if (string.Equals(context.TagName, "body", StringComparison.OrdinalIgnoreCase))
                {
                    string cssClass = null;
                    if (output.Attributes.TryGetAttribute("class", out TagHelperAttribute attribute))
                        cssClass = attribute.Value.ToString();

                    if (!string.IsNullOrEmpty(cssClass))
                        cssClass += " " + LoadingClass;
                    else
                        cssClass = LoadingClass;

                    if (!string.IsNullOrEmpty(appPageModel.CssClass))
                        cssClass += " " + appPageModel.CssClass;

                    output.Attributes.SetAttribute("class", cssClass);
                }

                if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
                {
                    output.PostContent.AppendHtml($"    <title>{appPageModel.Title}</title>{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Description))
                        output.PostContent.AppendHtml($"    <meta name=\"description\" content=\"{appPageModel.Description}\" />{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.Keywords))
                        output.PostContent.AppendHtml($"    <meta name=\"keywords\" content=\"{appPageModel.Keywords}\" />{Environment.NewLine}");

                    if (!string.IsNullOrEmpty(appPageModel.CanonicalLink))
                        output.PostContent.AppendHtml($"    <link rel=\"canonical\" href=\"{appPageModel.CanonicalLink}\" />{Environment.NewLine}");

                    var appClientModel = await appPageModel.GetAppClientModelAsync(ViewContext.HttpContext.RequestAborted);

                    output.PostContent.AppendHtml($"    <script type=\"text/javascript\">var appInitOptions = {jsonHelper.Serialize(appClientModel)}</script>{Environment.NewLine}");
                }
            }
        }
    }
}