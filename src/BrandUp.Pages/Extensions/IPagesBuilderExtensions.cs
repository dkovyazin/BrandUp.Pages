﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrandUp.Pages.Builder
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder)
        {
            return AddRazorContentPage(builder, options => { });
        }

        public static IPagesBuilder AddRazorContentPage(this IPagesBuilder builder, Action<RazorContentPageOptions> optionAction)
        {
            var services = builder.Services;

            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddPageRoute(Url.RazorPageLinkGenerator.RazorPagePath, "{*url}");
            });

            services.AddTransient<Url.IPageLinkGenerator, Url.RazorPageLinkGenerator>();
            services.AddSingleton<Views.IViewLocator, Views.ViewLocator>();
            services.AddHttpContextAccessor();

            services.Configure(optionAction);

            builder.Services.AddTransient<ITagHelperComponent, TagHelpers.EmbeddingTagHelperComponent>();

            return builder;
        }
    }
}