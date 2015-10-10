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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        // ReSharper disable ClassNeverInstantiated.Local

        #region Nested type: Field

        private class Field : FieldInfo
        {
            private readonly string name;

            public Field(string name)
            {
               this.name = name;
            }

            #region FieldInfo overrides

            public override FieldAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override RuntimeFieldHandle FieldHandle
            {
                get { throw new NotImplementedException(); }
            }

            public override Type FieldType
            {
                get { throw new NotImplementedException(); }
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

            public override object GetValue(object obj)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object obj, object value, BindingFlags invokeFlags, Binder binder, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return MiscHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Nested type: Method

        private class Method : MethodInfo
        {
            private readonly string name;

            public Method(string name)
            {
               this.name = name;
            }

            #region MethodInfo overrides

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get { throw new NotImplementedException(); }
            }

            public override MethodAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get { throw new NotImplementedException(); }
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

            public override MethodInfo GetBaseDefinition()
            {
                throw new NotImplementedException();
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetParameters()
            {
                return MiscHelpers.GetEmptyArray<ParameterInfo>();
            }

            public override object Invoke(object obj, BindingFlags invokeFlags, Binder binder, object[] args, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return MiscHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Nested type: Property

        private class Property : PropertyInfo
        {
            private readonly string name;

            public Property(string name)
            {
               this.name = name;
            }

            #region PropertyInfo overrides

            public override PropertyAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanRead
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanWrite
            {
                get { throw new NotImplementedException(); }
            }

            public override Type PropertyType
            {
                get { throw new NotImplementedException(); }
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetIndexParameters()
            {
                return MiscHelpers.GetEmptyArray<ParameterInfo>();
            }

            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override object GetValue(object obj, BindingFlags invokeFlags, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object obj, object value, BindingFlags invokeFlags, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return MiscHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        // ReSharper restore ClassNeverInstantiated.Local

        #region Nested type: MemberMapBase

        private class MemberMapBase
        {
            protected const int CompactionThreshold = 1024 * 1024;
            protected static readonly TimeSpan CompactionInterval = TimeSpan.FromMinutes(5);
        }

        #endregion

        #region Nested type: MemberMap

        private class MemberMap<T> : MemberMapBase where T : MemberInfo
        {
            private readonly object dataLock = new object();
            private readonly Dictionary<string, WeakReference> map = new Dictionary<string, WeakReference>();
            private DateTime lastCompactionTime = DateTime.MinValue;

            public T GetMember(string name)
            {
                lock (dataLock)
                {
                    var result = GetMemberInternal(name);
                    CompactIfNecessary();
                    return result;
                }
            }

            public T[] GetMembers(string[] names)
            {
                lock (dataLock)
                {
                    var result = names.Select(GetMemberInternal).ToArray();
                    CompactIfNecessary();
                    return result;
                }
            }

            private T GetMemberInternal(string name)
            {
                T member;

                WeakReference weakRef;
                if (map.TryGetValue(name, out weakRef))
                {
                    member = weakRef.Target as T;
                    if (member == null)
                    {
                        member = (T)typeof(T).CreateInstance(name);
                        weakRef.Target = member;
                    }
                }
                else
                {
                    member = (T)typeof(T).CreateInstance(name);
                    map.Add(name, new WeakReference(member));
                }

                return member;
            }

            private void CompactIfNecessary()
            {
                if (map.Count >= CompactionThreshold)
                {
                    var now = DateTime.UtcNow;
                    if ((lastCompactionTime + CompactionInterval) <= now)
                    {
                        map.Where(pair => !pair.Value.IsAlive).ToList().ForEach(pair => map.Remove(pair.Key));
                        lastCompactionTime = now;
                    }
                }
            }
        }

        #endregion
    }
}
