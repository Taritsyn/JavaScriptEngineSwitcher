﻿// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a scriptable collection of host types.
    /// </summary>
    /// <remarks>
    /// Host type collections provide convenient scriptable access to all the types defined in one
    /// or more host assemblies. They are hierarchical collections where leaf nodes represent types
    /// and parent nodes represent namespaces. For example, if an assembly contains a type named
    /// "Acme.Gadgets.Button", the corresponding collection will have a property named "Acme" whose
    /// value is an object with a property named "Gadgets" whose value is an object with a property
    /// named "Button" whose value represents the <c>Acme.Gadgets.Button</c> host type. Use
    /// <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see> to expose a host
    /// type collection to script code.
    /// </remarks>
    public class HostTypeCollection : PropertyBag
    {
        private static readonly Predicate<Type> defaultFilter = type => true; 

        /// <summary>
        /// Initializes a new host type collection.
        /// </summary>
        public HostTypeCollection()
            : base(true)
        {
        }

        /// <summary>
        /// Initializes a new host type collection with types from one or more assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(params Assembly[] assemblies)
            : base(true)
        {
            MiscHelpers.VerifyNonNullArgument(assemblies, "assemblies");
            Array.ForEach(assemblies, AddAssembly);
        }

        /// <summary>
        /// Initializes a new host type collection with types from one or more assemblies. The
        /// assemblies are specified by name.
        /// </summary>
        /// <param name="assemblyNames">The names of the assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(params string[] assemblyNames)
            : base(true)
        {
            MiscHelpers.VerifyNonNullArgument(assemblyNames, "assemblyNames");
            Array.ForEach(assemblyNames, AddAssembly);
        }

        /// <summary>
        /// Initializes a new host type collection with selected types from one or more assemblies.
        /// </summary>
        /// <param name="filter">A filter for selecting the types to add.</param>
        /// <param name="assemblies">The assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(Predicate<Type> filter, params Assembly[] assemblies)
        {
            MiscHelpers.VerifyNonNullArgument(assemblies, "assemblies");
            Array.ForEach(assemblies, assembly => AddAssembly(assembly, filter));
        }

        /// <summary>
        /// Initializes a new host type collection with selected types from one or more assemblies.
        /// The assemblies are specified by name.
        /// </summary>
        /// <param name="filter">A filter for selecting the types to add.</param>
        /// <param name="assemblyNames">The names of the assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(Predicate<Type> filter, params string[] assemblyNames)
        {
            MiscHelpers.VerifyNonNullArgument(assemblyNames, "assemblyNames");
            Array.ForEach(assemblyNames, assemblyName => AddAssembly(assemblyName, filter));
        }

        /// <summary>
        /// Adds types from an assembly to a host type collection.
        /// </summary>
        /// <param name="assembly">The assembly that contains the types to add.</param>
        public void AddAssembly(Assembly assembly)
        {
            MiscHelpers.VerifyNonNullArgument(assembly, "assembly");
            foreach (var type in assembly.GetTypes().Where(type => type.IsImportable()))
            {
                AddType(type);
            }
        }

        /// <summary>
        /// Adds types from an assembly to a host type collection. The assembly is specified by name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the types to add.</param>
        public void AddAssembly(string assemblyName)
        {
            MiscHelpers.VerifyNonBlankArgument(assemblyName, "assemblyName", "Invalid assembly name");
            AddAssembly(Assembly.Load(AssemblyHelpers.GetFullAssemblyName(assemblyName)));
        }

        /// <summary>
        /// Adds selected types from an assembly to a host type collection.
        /// </summary>
        /// <param name="assembly">The assembly that contains the types to add.</param>
        /// <param name="filter">A filter for selecting the types to add.</param>
        public void AddAssembly(Assembly assembly, Predicate<Type> filter)
        {
            MiscHelpers.VerifyNonNullArgument(assembly, "assembly");

            var activeFilter = filter ?? defaultFilter;
            foreach (var type in assembly.GetTypes().Where(type => type.IsImportable() && activeFilter(type)))
            {
                AddType(type);
            }
        }

        /// <summary>
        /// Adds selected types from an assembly to a host type collection. The assembly is
        /// specified by name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the types to add.</param>
        /// <param name="filter">A filter for selecting the types to add.</param>
        public void AddAssembly(string assemblyName, Predicate<Type> filter)
        {
            MiscHelpers.VerifyNonBlankArgument(assemblyName, "assemblyName", "Invalid assembly name");
            AddAssembly(Assembly.Load(AssemblyHelpers.GetFullAssemblyName(assemblyName)), filter);
        }

        /// <summary>
        /// Adds a type to a host type collection.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public void AddType(Type type)
        {
            MiscHelpers.VerifyNonNullArgument(type, "type");
            AddType(HostType.Wrap(type));
        }

        /// <summary>
        /// Adds a type to a host type collection. The type is specified by name.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to add.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        public void AddType(string typeName, params Type[] typeArgs)
        {
            AddType(TypeHelpers.ImportType(typeName, null, false, typeArgs));
        }

        /// <summary>
        /// Adds a type to a host type collection. The type is specified by type name and assembly name.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to add.</param>
        /// <param name="assemblyName">The name of the assembly that contains the type to add.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        public void AddType(string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddType(TypeHelpers.ImportType(typeName, assemblyName, true, typeArgs));
        }

        /// <summary>
        /// Locates a namespace within a host type collection.
        /// </summary>
        /// <param name="name">The full name of the namespace to locate.</param>
        /// <returns>The node that represents the namespace if it was found, <c>null</c> otherwise.</returns>
        public PropertyBag GetNamespaceNode(string name)
        {
            MiscHelpers.VerifyNonNullArgument(name, "name");

            PropertyBag namespaceNode = this;

            var segments = name.Split('.');
            foreach (var segment in segments)
            {
                object node;
                if (!namespaceNode.TryGetValue(segment, out node))
                {
                    return null;
                }

                namespaceNode = node as PropertyBag;
                if (namespaceNode == null)
                {
                    return null;
                }
            }

            return namespaceNode;
        }

        private void AddType(HostType hostType)
        {
            MiscHelpers.VerifyNonNullArgument(hostType, "hostType");
            foreach (var type in hostType.Types)
            {
                var namespaceNode = GetNamespaceNode(type);
                if (namespaceNode != null)
                {
                    AddTypeToNamespaceNode(namespaceNode, type);
                }
            }
        }

        private PropertyBag GetNamespaceNode(Type type)
        {
            var locator = type.GetLocator();

            var segments = locator.Split('.');
            if (segments.Length < 1)
            {
                return null;
            }

            PropertyBag namespaceNode = this;
            foreach (var segment in segments.Take(segments.Length - 1))
            {
                PropertyBag innerNode;

                object node;
                if (!namespaceNode.TryGetValue(segment, out node))
                {
                    innerNode = new PropertyBag(true);
                    namespaceNode.SetPropertyNoCheck(segment, innerNode);
                }
                else
                {
                    innerNode = node as PropertyBag;
                    if (innerNode == null)
                    {
                        throw new OperationCanceledException(MiscHelpers.FormatInvariant("Namespace conflicts with '{0}' at '{1}'", node.GetFriendlyName(), locator));
                    }
                }

                namespaceNode = innerNode;
            }

            return namespaceNode;
        }

        private static void AddTypeToNamespaceNode(PropertyBag node, Type type)
        {
            object value;
            var name = type.GetRootName();
            if (!node.TryGetValue(name, out value))
            {
                node.SetPropertyNoCheck(name, HostType.Wrap(type));
                return;
            }

            var hostType = value as HostType;
            if (hostType != null)
            {
                var types = new[] { type }.Concat(hostType.Types).ToArray();

                var groups = types.GroupBy(testType => testType.GetGenericParamCount()).ToIList();
                if (groups.Any(group => group.Count() > 1))
                {
                    types = groups.Select(ResolveTypeConflict).ToArray();
                }

                node.SetPropertyNoCheck(name, HostType.Wrap(types));
                return;
            }

            if (value is PropertyBag)
            {
                throw new OperationCanceledException(MiscHelpers.FormatInvariant("Type conflicts with namespace at '{0}'", type.GetLocator()));
            }

            throw new OperationCanceledException(MiscHelpers.FormatInvariant("Type conflicts with '{0}' at '{1}'", value.GetFriendlyName(), type.GetLocator()));
        }

        private static Type ResolveTypeConflict(IEnumerable<Type> types)
        {
            var typeList = types.ToIList();
            return typeList.SingleOrDefault(type => type.IsPublic) ?? typeList[0];
        }
    }
}
