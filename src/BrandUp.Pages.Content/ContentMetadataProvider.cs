﻿using BrandUp.Pages.Content.Fields;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Pages.Content
{
    public class ContentMetadataProvider : IEquatable<ContentMetadataProvider>
    {
        #region Fields

        public const string ContentTypeNameDataKey = "_type";
        public static readonly string[] ContentTypePrefixes = new string[] { "Content", "Model" };
        private static readonly object[] ModelConstructorParameters = new object[0];
        private readonly ConstructorInfo modelConstructor = null;
        private readonly List<ContentMetadataProvider> derivedContents = new List<ContentMetadataProvider>();
        private readonly List<FieldProviderAttribute> fields = new List<FieldProviderAttribute>();
        private readonly Dictionary<string, int> fieldNames = new Dictionary<string, int>();
        private readonly ConstructorInfo contentConstructor;

        #endregion

        internal ContentMetadataProvider(ContentMetadataManager metadataManager, Type modelType, ContentMetadataProvider baseMetadata)
        {
            Manager = metadataManager;
            ModelType = modelType;
            BaseMetadata = baseMetadata;

            var contentModelAttribute = modelType.GetCustomAttribute<ContentAttribute>(false);
            if (contentModelAttribute == null)
                throw new ArgumentException($"Для типа модели контента \"{modelType}\" не определён атрибут {nameof(ContentAttribute)}.");

            if (!modelType.IsAbstract)
            {
                modelConstructor = modelType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                if (modelConstructor == null)
                    throw new InvalidOperationException($"Тип модели контента \"{modelType}\" не содержит публичный конструктор без параметров.");

                var genericContentType = typeof(Content<>);
                var contentType = genericContentType.MakeGenericType(ModelType);

                contentConstructor = contentType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(IContentDocument), modelType, typeof(ContentMetadataProvider) }, null);
                if (modelConstructor == null)
                    throw new InvalidOperationException($"Для типа модели \"{modelType}\" не получилось найти конструктор контента.");
            }

            if (baseMetadata != null)
                baseMetadata.derivedContents.Add(this);

            Name = contentModelAttribute.Name ?? GetTypeName(modelType);
            Title = contentModelAttribute.Title ?? Name;
            Description = contentModelAttribute.Description;
        }

        #region Properties

        public ContentMetadataManager Manager { get; }
        public Type ModelType { get; }
        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public ContentMetadataProvider BaseMetadata { get; }
        public IEnumerable<ContentMetadataProvider> DerivedContents => derivedContents;
        public IEnumerable<FieldProviderAttribute> Fields => fields;

        #endregion

        #region Methods

        private static string GetTypeName(Type type)
        {
            var name = type.Name;
            foreach (var namePrefix in ContentTypePrefixes)
            {
                if (name.EndsWith(namePrefix))
                    return type.Name.Substring(0, type.Name.LastIndexOf(namePrefix));
            }
            return name;
        }

        internal void InitializeFields()
        {
            var baseModelMetadata = BaseMetadata;
            if (baseModelMetadata != null)
            {
                foreach (var field in baseModelMetadata.fields)
                    AddField(field);
            }

            foreach (var fieldInfo in ModelType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var field = fieldInfo.GetCustomAttribute<FieldProviderAttribute>(false);
                if (field == null)
                    continue;

                InitializeField(field, fieldInfo);
            }

            foreach (var propertyInfo in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var field = propertyInfo.GetCustomAttribute<FieldProviderAttribute>(false);
                if (field == null)
                    continue;

                InitializeField(field, propertyInfo);
            }
        }
        private void InitializeField(FieldProviderAttribute field, MemberInfo fieldMember)
        {
            IModelBinding modelBinding;

            switch (fieldMember.MemberType)
            {
                case MemberTypes.Field:
                    modelBinding = new FieldModelBinding((FieldInfo)fieldMember);
                    break;
                case MemberTypes.Property:
                    modelBinding = new PropertyModelBinding((PropertyInfo)fieldMember);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            field.Initialize(this, modelBinding);

            AddField(field);
        }
        private void AddField(FieldProviderAttribute field)
        {
            var fIndex = fields.Count;

            fieldNames.Add(field.Name.ToLower(), fIndex);
            fields.Add(field);
        }
        [System.Diagnostics.DebuggerStepThrough]
        public bool TryGetField(string fieldName, out IFieldProvider field)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (!fieldNames.TryGetValue(fieldName.ToLower(), out int index))
            {
                field = null;
                return false;
            }
            field = fields[index];
            return true;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public object CreateModelInstance()
        {
            return modelConstructor.Invoke(new object[0]);
        }
        public IDictionary<string, object> ConvertContentModelToDictionary(object contentModel)
        {
            if (contentModel == null)
                throw new ArgumentNullException(nameof(contentModel));

            var contentModelType = contentModel.GetType();
            var isSubClass = contentModelType.IsSubclassOf(ModelType);
            if (contentModelType != ModelType && !isSubClass)
                throw new ArgumentException("Is not valid content model type.", nameof(contentModel));

            if (isSubClass)
            {
                var deriverMetadata = Manager.GetMetadata(contentModelType);
                return deriverMetadata.ConvertContentModelToDictionary(contentModel);
            }

            var result = new SortedDictionary<string, object>
            {
                { ContentTypeNameDataKey, Name }
            };

            foreach (var field in Fields)
            {
                var fieldValue = field.GetModelValue(contentModel);
                if (!field.HasValue(fieldValue))
                    continue;

                var dataValue = field.ConvetValueToData(fieldValue);
                result.Add(field.JsonPropertyName, dataValue);
            }

            return result;
        }
        public object ConvertDictionaryToContentModel(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.TryGetValue(ContentTypeNameDataKey, out object contentTypeNameValue))
            {
                var contentTypeName = (string)contentTypeNameValue;
                if (string.Compare(contentTypeName, Name, true) != 0)
                {
                    if (!Manager.TryGetMetadata(contentTypeName, out ContentMetadataProvider deriverMetadata))
                        throw new InvalidOperationException();
                    if (!deriverMetadata.ModelType.IsSubclassOf(ModelType))
                        throw new InvalidOperationException();

                    return deriverMetadata.ConvertDictionaryToContentModel(dictionary);
                }
            }

            var contentModel = CreateModelInstance();

            foreach (var kv in dictionary)
            {
                if (!TryGetField(kv.Key, out IFieldProvider field))
                    continue;

                var dataValue = kv.Value;
                var value = field.ConvetValueFromData(dataValue);
                field.SetModelValue(contentModel, value);
            }

            return contentModel;
        }
        public IContent ConvertDocumentToContent(IContentDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var contentModel = ConvertDictionaryToContentModel(document.Data);
            return (IContent)contentConstructor.Invoke(new object[] { document, contentModel, this });
        }
        public bool IsInherited(ContentMetadataProvider baseMetadataProvider)
        {
            if (baseMetadataProvider == null)
                throw new ArgumentNullException(nameof(baseMetadataProvider));

            return ModelType.IsSubclassOf(baseMetadataProvider.ModelType);
        }
        public bool IsInheritedOrEqual(ContentMetadataProvider baseMetadataProvider)
        {
            return this == baseMetadataProvider || IsInherited(baseMetadataProvider);
        }

        #endregion

        #region IEquatable members

        public bool Equals(ContentMetadataProvider other)
        {
            if (other == null || !(other is ContentMetadataProvider))
                return false;

            return ModelType == other.ModelType;
        }

        #endregion

        #region Object members

        public override int GetHashCode()
        {
            return ModelType.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ContentMetadataProvider);
        }
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Operators

        public static bool operator ==(ContentMetadataProvider x, ContentMetadataProvider y)
        {
            var xIsNull = Equals(x, null);
            var yIsNull = Equals(y, null);

            if (yIsNull && xIsNull)
                return true;

            if (xIsNull || yIsNull)
                return false;

            return x.Equals(y);
        }

        public static bool operator !=(ContentMetadataProvider x, ContentMetadataProvider y)
        {
            return !(x == y);
        }

        public static implicit operator Type(ContentMetadataProvider metadataProvider)
        {
            if (metadataProvider == null)
                return null;

            return metadataProvider.ModelType;
        }

        #endregion
    }
}