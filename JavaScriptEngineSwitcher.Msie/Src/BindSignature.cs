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

namespace Microsoft.ClearScript
{
    internal class BindSignature : IEquatable<BindSignature>
    {
        private readonly TargetInfo targetInfo;
        private readonly string name;
        private readonly Type[] typeArgs;
        private readonly ArgInfo[] argData;

        public BindSignature(HostTarget target, string name, Type[] typeArgs, object[] args)
        {
            targetInfo = new TargetInfo(target);
           this.typeArgs = typeArgs;
           this.name = name;

            argData = new ArgInfo[args.Length];
            for (var index = 0; index < args.Length; index++)
            {
                argData[index] = new ArgInfo(args[index]);
            }
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as BindSignature);
        }

        public override int GetHashCode()
        {
            var accumulator = new HashAccumulator();

            targetInfo.UpdateHash(ref accumulator);
            accumulator.Update(name);

            foreach (var type in typeArgs)
            {
                accumulator.Update(type);
            }

            foreach (var argInfo in argData)
            {
                argInfo.UpdateHash(ref accumulator);
            }

            return accumulator.HashCode;
        }

        #endregion

        #region IEquatable<BindSignature> implementation

        public bool Equals(BindSignature that)
        {
            if (that == null)
            {
                return false;
            }

            if (!targetInfo.Equals(that.targetInfo))
            {
                return false;
            }

            if (name != that.name)
            {
                return false;
            }

            if (typeArgs.Length != that.typeArgs.Length)
            {
                return false;
            }

            for (var index = 0; index < typeArgs.Length; index++)
            {
                if (typeArgs[index] != that.typeArgs[index])
                {
                    return false;
                }
            }

            if (argData.Length != that.argData.Length)
            {
                return false;
            }

            for (var index = 0; index < argData.Length; index++)
            {
                if (!argData[index].Equals(that.argData[index]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Nested type: TargetKind

        private enum TargetKind
        {
            Static,
            Null,
            Instance
        }

        #endregion

        #region Nested type: TargetInfo

        private class TargetInfo : IEquatable<TargetInfo>
        {
            private readonly TargetKind kind;
            private readonly Type targetType;
            private readonly Type instanceType;

            public TargetInfo(HostTarget target)
            {
                if (target is HostType)
                {
                    kind = TargetKind.Static;
                    targetType = target.Type;
                }
                else if (target.InvokeTarget == null)
                {
                    kind = TargetKind.Null;
                    targetType = target.Type;
                }
                else
                {
                    kind = TargetKind.Instance;
                    targetType = target.Type;

                    var tempType = target.InvokeTarget.GetType();
                    if (tempType != targetType)
                    {
                        instanceType = tempType;
                    }
                }
            }

            public void UpdateHash(ref HashAccumulator accumulator)
            {
                accumulator.Update((int)kind);
                accumulator.Update(targetType);
                accumulator.Update(instanceType);
            }

            #region Object overrides

            public override bool Equals(object obj)
            {
                return Equals(obj as TargetInfo);
            }

            public override int GetHashCode()
            {
                var accumulator = new HashAccumulator();
                UpdateHash(ref accumulator);
                return accumulator.HashCode;
            }

            #endregion

            #region IEquatable<TargetInfo> implementation

            public bool Equals(TargetInfo that)
            {
                return (that != null) && (kind == that.kind) && (targetType == that.targetType) && (instanceType == that.instanceType);
            }

            #endregion
        }

        #endregion

        #region Nested type: ArgKind

        private enum ArgKind
        {
            Null,
            ByValue,
            Out,
            Ref
        }

        #endregion

        #region Nested type: ArgInfo

        private class ArgInfo : IEquatable<ArgInfo>
        {
            private readonly ArgKind kind;
            private readonly Type type;

            public ArgInfo(object arg)
            {
                if (arg == null)
                {
                    kind = ArgKind.Null;
                    return;
                }

                var outArg = arg as IOutArg;
                if (outArg != null)
                {
                    kind = ArgKind.Out;
                    type = outArg.Type;
                    return;
                }

                var refArg = arg as IRefArg;
                if (refArg != null)
                {
                    kind = ArgKind.Ref;
                    type = refArg.Type;
                    return;
                }

                var hostType = arg as HostType;
                if (hostType != null)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostType);
                    return;
                }

                var hostMethod = arg as HostMethod;
                if (hostMethod != null)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostMethod);
                    return;
                }

                var hostIndexedProperty = arg as HostIndexedProperty;
                if (hostIndexedProperty != null)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(HostIndexedProperty);
                    return;
                }

                var scriptMethod = arg as ScriptMethod;
                if (scriptMethod != null)
                {
                    kind = ArgKind.ByValue;
                    type = typeof(ScriptMethod);
                    return;
                }

                var hostTarget = arg as HostTarget;
                if (hostTarget != null)
                {
                    kind = ArgKind.ByValue;
                    type = hostTarget.Type;
                    return;
                }

                kind = ArgKind.ByValue;
                type = arg.GetType();
            }

            public void UpdateHash(ref HashAccumulator accumulator)
            {
                accumulator.Update((int)kind);
                accumulator.Update(type);
            }

            #region Object overrides

            public override bool Equals(object obj)
            {
                return Equals(obj as ArgInfo);
            }

            public override int GetHashCode()
            {
                var accumulator = new HashAccumulator();
                UpdateHash(ref accumulator);
                return accumulator.HashCode;
            }

            #endregion

            #region IEquatable<ArgInfo> implementation

            public bool Equals(ArgInfo that)
            {
                return (that != null) && (kind == that.kind) && (type == that.type);
            }

            #endregion
        }

        #endregion

        #region Nested type: HashAccumulator

        private struct HashAccumulator
        {
            public int HashCode { get; private set; }

            public void Update(int value)
            {
                HashCode = unchecked((HashCode * 31) + value);
            }

            public void Update(object obj)
            {
                HashCode = unchecked((HashCode * 31) + ((obj != null) ? obj.GetHashCode() : 0));
            }
        }

        #endregion
    }
}
