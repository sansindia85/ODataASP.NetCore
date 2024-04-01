﻿//---------------------------------------------------------------------
// <copyright file="CoreHelpers.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.OData.Services.ODataWCFService.Vocabularies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Annotations;
    using Microsoft.OData.Edm.Csdl;
    using Microsoft.OData.Edm.Expressions;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Edm.Library.Annotations;
    using Microsoft.OData.Edm.Library.Expressions;
    using Microsoft.OData.Edm.Library.Values;
    using Microsoft.OData.Edm.Validation;

    public static class CoreHelpers
    {
        #region Initialization

        public static readonly IEdmModel Instance;
        public static readonly IEdmValueTerm ResourcePathTerm;
        public static readonly IEdmValueTerm DereferenceableIDsTerm;
        public static readonly IEdmValueTerm ConventionalIDsTerm;
        public static readonly IEdmValueTerm PermissionsTerm;
        public static readonly IEdmValueTerm ImmutableTerm;
        public static readonly IEdmValueTerm ComputedTerm;
        public static readonly IEdmValueTerm AcceptableMediaTypesTerm;
        public static readonly IEdmValueTerm OptimisticConcurrencyTerm;
        public static readonly IEdmEnumType PermissionType;

        internal const string CoreResourcePath = "Org.OData.Core.V1.ResourcePath";
        internal const string CoreDereferenceableIDs = "Org.OData.Core.V1.DereferenceableIDs";
        internal const string CoreConventionalIDs = "Org.OData.Core.V1.ConventionalIDs";
        internal const string CorePermissions = "Org.OData.Core.V1.Permissions";
        internal const string CoreImmutable = "Org.OData.Core.V1.Immutable";
        internal const string CoreComputed = "Org.OData.Core.V1.Computed";
        internal const string CoreAcceptableMediaTypes = "Org.OData.Core.V1.AcceptableMediaTypes";
        internal const string CorePermission = "Org.OData.Core.V1.Permission";
        internal const string CoreOptimisticConcurrency = "Org.OData.Core.V1.OptimisticConcurrency";

        static CoreHelpers()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ODataSamples.Services.Core.Vocabularies.CoreVocabularies.xml"))
            {
                IEnumerable<EdmError> errors;
                CsdlReader.TryParse(new[] { XmlReader.Create(stream) }, out Instance, out errors);
            }

            ResourcePathTerm = Instance.FindDeclaredValueTerm(CoreResourcePath);
            DereferenceableIDsTerm = Instance.FindDeclaredValueTerm(CoreDereferenceableIDs);
            ConventionalIDsTerm = Instance.FindDeclaredValueTerm(CoreConventionalIDs);
            PermissionsTerm = Instance.FindDeclaredValueTerm(CorePermissions);
            ImmutableTerm = Instance.FindDeclaredValueTerm(CoreImmutable);
            ComputedTerm = Instance.FindDeclaredValueTerm(CoreComputed);
            AcceptableMediaTypesTerm = Instance.FindDeclaredValueTerm(CoreAcceptableMediaTypes);
            OptimisticConcurrencyTerm = Instance.FindDeclaredValueTerm(CoreOptimisticConcurrency);
            PermissionType = (IEdmEnumType)Instance.FindDeclaredType(CorePermission);
        }

        #endregion

        #region ResourcePath

        public static void SetResourcePathCoreAnnotation(this EdmModel model, IEdmEntitySet entitySet, string url)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (entitySet == null) throw new ArgumentNullException("entitySet");
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

            model.SetCoreAnnotation(entitySet, ResourcePathTerm, url);
        }

        public static void SetResourcePathCoreAnnotation(this EdmModel model, IEdmSingleton singleton, string url)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (singleton == null) throw new ArgumentNullException("singleton");
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

            model.SetCoreAnnotation(singleton, ResourcePathTerm, url);
        }

        public static void SetResourcePathCoreAnnotation(this EdmModel model, IEdmActionImport import, string url)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (import == null) throw new ArgumentNullException("import");
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

            model.SetCoreAnnotation(import, ResourcePathTerm, url);
        }

        public static void SetResourcePathCoreAnnotation(this EdmModel model, IEdmFunctionImport import, string url)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (import == null) throw new ArgumentNullException("import");
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

            model.SetCoreAnnotation(import, ResourcePathTerm, url);
        }

        #endregion

        #region DereferenceableIDs

        public static void SetDereferenceableIDsCoreAnnotation(this EdmModel model, IEdmEntityContainer container, bool value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (container == null) throw new ArgumentNullException("container");

            model.SetCoreAnnotation(container, DereferenceableIDsTerm, value);
        }

        #endregion

        #region ConventionalIDs

        public static void SetConventionalIDsCoreAnnotation(this EdmModel model, IEdmEntityContainer container, bool value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (container == null) throw new ArgumentNullException("container");

            model.SetCoreAnnotation(container, ConventionalIDsTerm, value);
        }

        #endregion

        #region Permissions

        public static void SetPermissionsCoreAnnotation(this EdmModel model, IEdmProperty property, CorePermission value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            var target = property;
            var term = PermissionsTerm;
            var name = new EdmEnumTypeReference(PermissionType, false).ToStringLiteral((long)value);
            var expression = new EdmEnumMemberReferenceExpression(PermissionType.Members.Single(m => m.Name == name));
            var annotation = new EdmAnnotation(target, term, expression);
            annotation.SetSerializationLocation(model, property.ToSerializationLocation());
            model.AddVocabularyAnnotation(annotation);
        }

        public static CorePermission? GetPermissions(this IEdmModel model, IEdmProperty property)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            return model.GetEnum<CorePermission>(property, PermissionsTerm);
        }

        #endregion

        #region Immutable

        public static void SetImmutableCoreAnnotation(this EdmModel model, IEdmProperty property, bool value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            model.SetCoreAnnotation(property, ImmutableTerm, value);
        }

        public static bool? GetImmutable(this IEdmModel model, IEdmProperty property)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            return model.GetBoolean(property, ImmutableTerm);
        }

        #endregion

        #region Computed

        public static void SetComputedCoreAnnotation(this EdmModel model, IEdmProperty property, bool value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            model.SetCoreAnnotation(property, ComputedTerm, value);
        }

        public static bool? GetComputed(this IEdmModel model, IEdmProperty property)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            return model.GetBoolean(property, ComputedTerm);
        }

        #endregion

        #region AcceptableMediaTypes

        public static void SetAcceptableMediaTypesCoreAnnotation(this EdmModel model, IEdmEntityType entityType, IEnumerable<string> acceptableMediaTypes)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (entityType == null) throw new ArgumentNullException("entityType");

            model.SetCoreAnnotation(entityType, AcceptableMediaTypesTerm, acceptableMediaTypes);
        }

        public static void SetAcceptableMediaTypesCoreAnnotation(this EdmModel model, IEdmProperty property, IEnumerable<string> acceptableMediaTypes)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (property == null) throw new ArgumentNullException("property");

            model.SetCoreAnnotation(property, AcceptableMediaTypesTerm, acceptableMediaTypes);
        }

        public static IEnumerable<string> GetAcceptableMediaTypes(this IEdmModel model, IEdmEntityType entityType)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (entityType == null) throw new ArgumentNullException("entityType");

            var annotation = model.FindVocabularyAnnotation(entityType, AcceptableMediaTypesTerm);
            if (annotation != null)
            {
                var collection = (IEdmCollectionExpression)annotation.Value;
                foreach (IEdmStringConstantExpression expression in collection.Elements)
                {
                    yield return expression.Value;
                }
            }
        }

        #endregion

        #region OptimisticConcurrency

        public static IEnumerable<string> GetOptimisticConcurrency(this IEdmModel model, IEdmEntitySet entitySet)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (entitySet == null) throw new ArgumentNullException("entitySet");

            var annotation = model.FindVocabularyAnnotation(entitySet, OptimisticConcurrencyTerm);
            if (annotation != null)
            {
                var collection = (IEdmCollectionExpression)annotation.Value;
                foreach (IEdmPathExpression expression in collection.Elements)
                {
                    var paths = new StringBuilder();
                    foreach (var path in expression.Path)
                    {
                        paths.AppendFormat("{0}.", path);
                    }

                    paths.Remove(paths.Length - 1, 1);

                    yield return paths.ToString();
                }
            }
        }

        #endregion

        #region Helpers

        private static void SetCoreAnnotation(this EdmModel model, IEdmVocabularyAnnotatable target, IEdmValueTerm term, string value)
        {
            var expression = new EdmStringConstant(value);
            var annotation = new EdmAnnotation(target, term, expression);
            annotation.SetSerializationLocation(model, target.ToSerializationLocation());
            model.AddVocabularyAnnotation(annotation);
        }

        private static void SetCoreAnnotation(this EdmModel model, IEdmVocabularyAnnotatable target, IEdmValueTerm term, IEnumerable<string> values)
        {
            if (values == null)
            {
                values = new string[0];
            }

            var expression = new EdmCollectionExpression(values.Select(value => new EdmStringConstant(value)));
            var annotation = new EdmAnnotation(target, term, expression);
            annotation.SetSerializationLocation(model, target.ToSerializationLocation());
            model.AddVocabularyAnnotation(annotation);
        }

        private static void SetCoreAnnotation(this EdmModel model, IEdmVocabularyAnnotatable target, IEdmValueTerm term, bool value)
        {
            var expression = new EdmBooleanConstant(value);
            var annotation = new EdmAnnotation(target, term, expression);
            annotation.SetSerializationLocation(model, target.ToSerializationLocation());
            model.AddVocabularyAnnotation(annotation);
        }

        private static bool? GetBoolean(this IEdmModel model, IEdmProperty property, IEdmValueTerm term)
        {
            var annotation = model.FindVocabularyAnnotation(property, term);
            if (annotation != null)
            {
                var booleanExpression = (IEdmBooleanConstantExpression)annotation.Value;
                return booleanExpression.Value;
            }

            return null;
        }

        private static T? GetEnum<T>(this IEdmModel model, IEdmProperty property, IEdmValueTerm term)
            where T : struct
        {
            var annotation = model.FindVocabularyAnnotation(property, term);
            if (annotation != null)
            {
                var enumMemberReference = (IEdmEnumMemberReferenceExpression)annotation.Value;
                var enumMember = enumMemberReference.ReferencedEnumMember;
                return (T)Enum.Parse(typeof(T), enumMember.Name);
            }

            return null;
        }

        private static EdmVocabularyAnnotationSerializationLocation ToSerializationLocation(this IEdmVocabularyAnnotatable target)
        {
            return target is IEdmEntityContainer ? EdmVocabularyAnnotationSerializationLocation.OutOfLine : EdmVocabularyAnnotationSerializationLocation.Inline;
        }

        private static IEdmValueAnnotation FindVocabularyAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmValueTerm term)
        {
            var result = default(IEdmValueAnnotation);

            var annotations = model.FindVocabularyAnnotations(target);
            if (annotations != null)
            {
                var annotation = annotations.FirstOrDefault(a => a.Term.Namespace == term.Namespace && a.Term.Name == term.Name);
                result = (IEdmValueAnnotation)annotation;
            }

            return result;
        }

        #endregion
    }
}
