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
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal class HostIndexedProperty : HostTarget
    {
        private static readonly string[] auxMethodNames = { "get", "set" };

        private readonly HostItem target;
        private readonly string name;

        public HostIndexedProperty(HostItem target, string name)
        {
           this.target = target;
           this.name = name;
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("HostIndexedProperty:{0}", name);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type
        {
            get { return typeof(void); }
        }

        public override object Target
        {
            get { return this; }
        }

        public override object InvokeTarget
        {
            get { return null; }
        }

        public override object DynamicInvokeTarget
        {
            get { return null; }
        }

        public override HostTargetFlags Flags
        {
            get { return HostTargetFlags.None; }
        }

        public override string[] GetAuxMethodNames(BindingFlags bindFlags)
        {
            return auxMethodNames;
        }

        public override bool TryInvokeAuxMember(string memberName, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (memberName == "get")
                {
                    result = target.InvokeMember(name, BindingFlags.GetProperty, args, bindArgs, null);
                    return true;
                }

                if (memberName == "set")
                {
                    result = target.InvokeMember(name, BindingFlags.SetProperty, args, bindArgs, null);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvoke(BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = target.InvokeMember(name, invokeFlags.HasFlag(BindingFlags.SetField) ? BindingFlags.SetProperty : BindingFlags.GetProperty, args, bindArgs, null);
            return true;
        }

        #endregion
    }
}
