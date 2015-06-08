// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50 // Cannot accurately resolve the location of the documentation XML file in coreclr.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public static class TagHelperUseageDescriptorFactory
    {
        private static readonly Dictionary<string, IEnumerable<XElement>> XMLFileMembersCache =
            new Dictionary<string, IEnumerable<XElement>>(StringComparer.Ordinal);
        private static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();

        public static TagHelperUseageDescriptor CreateDescriptor([NotNull] TypeInfo typeInfo)
        {
            var lookupName = $"T:{typeInfo.FullName}";
            return CreateDescriptorCore(typeInfo.Assembly, lookupName);
        }

        public static TagHelperUseageDescriptor CreateDescriptor([NotNull] PropertyInfo propertyInfo)
        {
            var declaringTypeInfo = propertyInfo.DeclaringType.GetTypeInfo();
            var lookupName = $"P:{declaringTypeInfo.FullName}.{propertyInfo.Name}";
            return CreateDescriptorCore(declaringTypeInfo.Assembly, lookupName);
        }

        private static TagHelperUseageDescriptor CreateDescriptorCore(Assembly typeAssembly, string lookupName)
        {
            var typeAssemblyLocation = typeAssembly.Location;

            if (string.IsNullOrWhiteSpace(typeAssemblyLocation) && !string.IsNullOrWhiteSpace(typeAssembly.CodeBase))
            {
                var uri = new UriBuilder(typeAssembly.CodeBase);

                // Normalize the path to a UNC path. This will remove things like file:// from start of the uri.Path.
                typeAssemblyLocation = Uri.UnescapeDataString(uri.Path);

                // Still couldn't resolve a valid typeAssemblyLocation.
                if (string.IsNullOrWhiteSpace(typeAssemblyLocation))
                {
                    return null;
                }
            }

            IEnumerable<XElement> members = null;
            if (!XMLFileMembersCache.TryGetValue(typeAssemblyLocation, out members))
            {
                var xmlDocumentationFile = GetXmlDocumentationFile(typeAssemblyLocation);

                // We only want to process the file if it exists. In the case it doesn't, a null value will be added
                // to the cache to not constantly look for new XML files.
                if (xmlDocumentationFile != null)
                {
                    var xmlDocumentation = XDocument.Load(xmlDocumentationFile.FullName);
                    var documentationMembers = xmlDocumentation.Root.Element("members");
                    members = documentationMembers.Elements("member");
                }

                XMLFileMembersCache.Add(typeAssemblyLocation, members);
            }

            // Members will be null if there is no associated XML file for the provided typeAssembly.
            if (members != null)
            {
                // XML file processing is defined by: https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx
                var associatedMemeber = members
                    .FirstOrDefault(element =>
                        string.Equals(element.Attribute("name").Value, lookupName, StringComparison.Ordinal));

                if (associatedMemeber != null)
                {
                    var summary = associatedMemeber.Element("summary")?.Value.Trim();
                    var remarks = associatedMemeber.Element("remarks")?.Value.Trim();

                    return new TagHelperUseageDescriptor(summary, remarks);
                }
            }

            return null;
        }

        private static FileInfo GetXmlDocumentationFile(string typeAssemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(typeAssemblyLocation) ||
                typeAssemblyLocation.IndexOfAny(InvalidPathCharacters) != -1)
            {
                return null;
            }

            try
            {
                var assemblyDirectory = Path.GetDirectoryName(typeAssemblyLocation);
                var assemblyName = Path.GetFileName(typeAssemblyLocation);
                var assemblyXMLDocumentationName = Path.ChangeExtension(assemblyName, ".xml");
                var culture = CultureInfo.CurrentCulture;

                var assemblyXMLDocumentationFile = new FileInfo(
                    Path.Combine(assemblyDirectory, assemblyXMLDocumentationName));

                // If there's not an XML file side-by-side the .dll it may exist in a culture specific directory.
                if (!assemblyXMLDocumentationFile.Exists)
                {
                    var fallbackDirectories = GetCultureFallbackDirectories();
                    assemblyXMLDocumentationFile = fallbackDirectories
                        .Select(fallbackDiretory =>
                            new FileInfo(
                                Path.Combine(assemblyDirectory, fallbackDiretory, assemblyXMLDocumentationName)))
                        .FirstOrDefault(file => file.Exists);
                }

                return assemblyXMLDocumentationFile;
            }
            catch (PathTooLongException)
            {
                // Could not resolve XML file.
                return null;
            }
        }

        private static IEnumerable<string> GetCultureFallbackDirectories()
        {
            var culture = CultureInfo.CurrentCulture;

            // Following the fall-back process defined by:
            // https://msdn.microsoft.com/en-us/library/sb6a8618.aspx#cpconpackagingdeployingresourcesanchor1
            do
            {
                yield return culture.Name;

                culture = culture.Parent;
            } while (culture != null && culture != CultureInfo.InvariantCulture);
        }
    }
}
#endif