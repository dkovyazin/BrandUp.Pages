﻿using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Content
{
    public static class IContentMetadataProviderExtensions
    {
        public static IEnumerable<IContentMetadataProvider> GetDerivedMetadataWithHierarhy(this IContentMetadataProvider contentMetadata, bool includeCurrent)
        {
            if (includeCurrent)
                yield return contentMetadata;

            foreach (var derivedContentMetadata in contentMetadata.DerivedContents)
            {
                yield return derivedContentMetadata;

                foreach (var childDerivedContentMetadata in GetDerivedMetadataWithHierarhy(derivedContentMetadata, false))
                    yield return childDerivedContentMetadata;
            }
        }

        public static bool TryGetField<TField>(this IContentMetadataProvider contentMetadata, string fieldName, out TField field)
            where TField : Fields.Field
        {
            if (!contentMetadata.TryGetField(fieldName, out Fields.Field f))
            {
                field = null;
                return false;
            }

            if (!(f is TField))
                throw new ArgumentException();

            field = (TField)f;
            return true;
        }
    }
}