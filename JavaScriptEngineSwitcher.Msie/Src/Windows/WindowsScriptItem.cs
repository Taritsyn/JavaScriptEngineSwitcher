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
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Expando;
using Creek.Scripting;
using Microsoft.ClearScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;

namespace Lib.Scripting.Clearscript.Windows
{
    internal class WindowsScriptItem : ScriptItem, IDisposable
    {
        private readonly WindowsScriptEngine engine;
        private readonly IExpando target;
        private WindowsScriptItem holder;
        private bool disposed;

        private WindowsScriptItem(WindowsScriptEngine engine, IExpando target)
        {
           this.engine = engine;
           this.target = target;
        }

        public static object Wrap(WindowsScriptEngine engine, object obj)
        {
            Debug.Assert(!(obj is IScriptMarshalWrapper));

            if (obj == null)
            {
                return null;
            }

            var expando = obj as IExpando;
            if ((expando != null) && (obj.GetType().IsCOMObject))
            {
                return new WindowsScriptItem(engine, expando);
            }

            return obj;
        }

        private IScriptEngineException GetScriptError(Exception exception)
        {
            IScriptEngineException scriptError;
            if (TryGetScriptError(exception, out scriptError))
            {
                return scriptError;
            }

            return new ScriptEngineException(engine.Name, exception.Message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, exception);
        }

        private bool TryGetScriptError(Exception exception, out IScriptEngineException scriptError)
        {
            // WORKAROUND: Windows Script items often throw ugly exceptions. The code here
            // attempts to clean up specific cases.

            while (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }

            scriptError = exception as IScriptEngineException;
            if (scriptError != null)
            {
                return true;
            }

            var comException = exception as COMException;
            if (comException != null)
            {
                var hr = comException.ErrorCode;
                if ((hr == RawCOMHelpers.HResult.SCRIPT_E_REPORTED) && (engine.CurrentScriptFrame != null))
                {
                    scriptError = engine.CurrentScriptFrame.ScriptError ?? engine.CurrentScriptFrame.PendingScriptError;
                    if (scriptError != null)
                    {
                        return true;
                    }
                }
                else if (RawCOMHelpers.HResult.GetFacility(hr) == RawCOMHelpers.HResult.FACILITY_CONTROL)
                {
                    // These exceptions often have awful messages that include COM error codes.
                    // The engine itself may be able to provide a better message.

                    string message;
                    if (engine.RuntimeErrorMap.TryGetValue(RawCOMHelpers.HResult.GetCode(hr), out message) && (message != exception.Message))
                    {
                        scriptError = new ScriptEngineException(engine.Name, message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, exception.InnerException);
                        return true;
                    }
                }
                else if (hr == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND)
                {
                    // this usually indicates invalid object or property access in JScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, exception.InnerException);
                    return true;
                }
            }
            else
            {
                var argumentException = exception as ArgumentException;
                if ((argumentException != null) && (argumentException.ParamName == null))
                {
                    // this usually indicates invalid object or property access in VBScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, exception.InnerException);
                    return true;
                }
            }

            return false;
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        #region ScriptItem overrides

        public override ScriptEngine Engine
        {
            get { return engine; }
        }

        protected override bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            VerifyNotDisposed();

            var succeeded = DynamicHelpers.TryBindAndInvoke(binder, target, args, out result);
            if (!succeeded)
            {
                var exception = result as Exception;
                if ((exception != null) && (engine.CurrentScriptFrame != null))
                {
                    var scriptError = exception as IScriptEngineException;
                    if (scriptError != null)
                    {
                        engine.CurrentScriptFrame.ScriptError = scriptError;
                    }
                    else
                    {
                        engine.CurrentScriptFrame.ScriptError = GetScriptError(exception);
                    }
                }

                result = null;
                return false;
            }

            return true;
        }

        protected override object[] AdjustInvokeArgs(object[] args)
        {
            // WORKAROUND: JScript seems to require at least one argument to invoke a function
            return ((engine is JScriptEngine) && (args.Length < 1)) ? new object[] { Undefined.Value } : args;
        }

        #endregion

        #region IDynamic implementation

        public override object GetProperty(string name)
        {
            VerifyNotDisposed();

            var result = engine.MarshalToHost(engine.ScriptInvoke(() =>
            {
                try
                {
                    return target.InvokeMember(name, BindingFlags.GetProperty, null, target, MiscHelpers.GetEmptyArray<object>(), null, CultureInfo.InvariantCulture, null);
                }
                catch (Exception)
                {
                    if (target.GetMethod(name, BindingFlags.GetProperty) != null)
                    {
                        // Property retrieval failed, but a method with the given name exists;
                        // create a tear-off method. This currently applies only to VBScript.

                        return new ScriptMethod(this, name);
                    }

                    return Nonexistent.Value;
                }
            }), false);

            var resultScriptItem = result as WindowsScriptItem;
            if ((resultScriptItem != null) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, object value)
        {
            VerifyNotDisposed();

            engine.ScriptInvoke(() =>
            {
                var marshaledArgs = new[] { engine.MarshalToScript(value) };
                try
                {
                    target.InvokeMember(name, BindingFlags.SetProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                }
                catch (MissingMemberException)
                {
                    target.AddProperty(name);
                    target.InvokeMember(name, BindingFlags.SetProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                }
            });
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();

            return engine.ScriptInvoke(() =>
            {
                var field = target.GetField(name, BindingFlags.Default);
                if (field != null)
                {
                    target.RemoveMember(field);
                    return true;
                }

                var property = target.GetProperty(name, BindingFlags.Default);
                if (property != null)
                {
                    target.RemoveMember(property);
                    return true;
                }

                return false;
            });
        }

        public override string[] GetPropertyNames()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetProperties(BindingFlags.Default).Select(property => property.Name).ExcludeIndices().ToArray());
        }

        public override object GetProperty(int index)
        {
            VerifyNotDisposed();
            return GetProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        public override void SetProperty(int index, object value)
        {
            VerifyNotDisposed();
            SetProperty(index.ToString(CultureInfo.InvariantCulture), value);
        }

        public override bool DeleteProperty(int index)
        {
            VerifyNotDisposed();
            return DeleteProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetProperties(BindingFlags.Default).Select(property => property.Name).GetIndices().ToArray());
        }

        public override object Invoke(object[] args, bool asConstructor)
        {
            VerifyNotDisposed();

            if (asConstructor)
            {
                return engine.Script.EngineInternal.invokeConstructor(this, args);
            }

            return engine.Script.EngineInternal.invokeMethod(holder, this, args);
        }

        public override object InvokeMethod(string name, object[] args)
        {
            VerifyNotDisposed();

            try
            {
                return engine.MarshalToHost(engine.ScriptInvoke(() => target.InvokeMember(name, BindingFlags.InvokeMethod, null, target, engine.MarshalToScript(args), null, CultureInfo.InvariantCulture, null)), false);
            }
            catch (Exception exception)
            {
                IScriptEngineException scriptError;
                if (TryGetScriptError(exception, out scriptError))
                {
                    throw (Exception)scriptError;
                }

                throw;
            }
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public override object Unwrap()
        {
            return target;
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (!disposed)
            {
                Marshal.FinalReleaseComObject(target);
                disposed = true;
            }
        }

        #endregion
    }
}
