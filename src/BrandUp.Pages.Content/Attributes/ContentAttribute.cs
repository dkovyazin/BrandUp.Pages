﻿using System;

namespace BrandUp.Pages.Content
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContentAttribute : Attribute
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}