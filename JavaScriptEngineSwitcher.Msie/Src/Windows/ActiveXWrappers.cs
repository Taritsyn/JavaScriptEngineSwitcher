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
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    #region ActiveScriptWrapper

    internal abstract class ActiveScriptWrapper
    {
        public static ActiveScriptWrapper Create(string progID)
        {
            if (Environment.Is64BitProcess)
            {
                return new ActiveScriptWrapper64(progID);
            }

            return new ActiveScriptWrapper32(progID);
        }

        public abstract void SetScriptSite(IActiveScriptSite site);

        public abstract void SetScriptState(ScriptState state);

        public abstract void GetScriptState(out ScriptState state);

        public abstract void InitNew();

        public abstract void GetScriptDispatch(string itemName, out object dispatch);

        public abstract void AddNamedItem(string name, ScriptItemFlags flags);

        public abstract void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo);

        public abstract void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags);

        public abstract void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts);

        public abstract void EnumStackFrames(out IEnumDebugStackFrames enumFrames);

        public abstract void CollectGarbage(ScriptGCType type);

        public abstract void Close();
    }

    internal class ActiveScriptWrapper32 : ActiveScriptWrapper
    {
        // ReSharper disable NotAccessedField.Local

        private IntPtr pActiveScript;
        private IntPtr pActiveScriptParse;
        private IntPtr pActiveScriptDebug;
        private IntPtr pActiveScriptGarbageCollector;
        private IntPtr pDebugStackFrameSniffer;

        private IActiveScript activeScript;
        private IActiveScriptParse32 activeScriptParse;
        private IActiveScriptDebug32 activeScriptDebug;
        private IActiveScriptGarbageCollector activeScriptGarbageCollector;
        private IDebugStackFrameSnifferEx32 debugStackFrameSniffer;

        // ReSharper restore NotAccessedField.Local

        private delegate uint RawInterruptScriptThread(
            [In] IntPtr pThis,
            [In] uint scriptThreadID,
            [In] ref EXCEPINFO excepInfo,
            [In] ScriptInterruptFlags flags
        );

        private delegate uint RawEnumCodeContextsOfPosition(
            [In] IntPtr pThis,
            [In] uint sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );

        public ActiveScriptWrapper32(string progID)
        {
            pActiveScript = RawCOMHelpers.CreateInstance<IActiveScript>(progID);
            pActiveScriptParse = RawCOMHelpers.QueryInterface<IActiveScriptParse32>(pActiveScript);
            pActiveScriptDebug = RawCOMHelpers.QueryInterface<IActiveScriptDebug32>(pActiveScript);
            pActiveScriptGarbageCollector = RawCOMHelpers.QueryInterfaceNoThrow<IActiveScriptGarbageCollector>(pActiveScript);
            pDebugStackFrameSniffer = RawCOMHelpers.QueryInterface<IDebugStackFrameSnifferEx32>(pActiveScript);

            activeScript = (IActiveScript)Marshal.GetObjectForIUnknown(pActiveScript);
            activeScriptParse = (IActiveScriptParse32)activeScript;
            activeScriptDebug = (IActiveScriptDebug32)activeScript;
            activeScriptGarbageCollector = activeScript as IActiveScriptGarbageCollector;
            debugStackFrameSniffer = (IDebugStackFrameSnifferEx32)activeScript;
        }

        public override void SetScriptSite(IActiveScriptSite site)
        {
            activeScript.SetScriptSite(site);
        }

        public override void SetScriptState(ScriptState state)
        {
            activeScript.SetScriptState(state);
        }

        public override void GetScriptState(out ScriptState state)
        {
            activeScript.GetScriptState(out state);
        }

        public override void InitNew()
        {
            activeScriptParse.InitNew();
        }

        public override void GetScriptDispatch(string itemName, out object dispatch)
        {
            activeScript.GetScriptDispatch(itemName, out dispatch);
        }

        public override void AddNamedItem(string name, ScriptItemFlags flags)
        {
            activeScript.AddNamedItem(name, flags);
        }

        public override void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo)
        {
            activeScriptParse.ParseScriptText(code, itemName, context, delimiter, sourceContext.ToUInt32(), startingLineNumber, flags, pVarResult, out excepInfo);
        }

        public override void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawInterruptScriptThread>(pActiveScript, 14);
            del(pActiveScript, scriptThreadID, ref excepInfo, flags);
        }

        public override void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawEnumCodeContextsOfPosition>(pActiveScriptDebug, 5);
            RawCOMHelpers.HResult.Check(del(pActiveScriptDebug, sourceContext.ToUInt32(), offset, length, out enumContexts));
        }

        public override void EnumStackFrames(out IEnumDebugStackFrames enumFrames)
        {
            debugStackFrameSniffer.EnumStackFrames(out enumFrames);
        }

        public override void CollectGarbage(ScriptGCType type)
        {
            if (activeScriptGarbageCollector != null)
            {
                activeScriptGarbageCollector.CollectGarbage(type);
            }
        }

        public override void Close()
        {
            activeScript.Close();

            debugStackFrameSniffer = null;
            activeScriptGarbageCollector = null;
            activeScriptDebug = null;
            activeScriptParse = null;
            activeScript = null;

            RawCOMHelpers.ReleaseAndEmpty(ref pDebugStackFrameSniffer);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptGarbageCollector);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptDebug);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptParse);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScript);
        }
    }

    internal class ActiveScriptWrapper64 : ActiveScriptWrapper
    {
        // ReSharper disable NotAccessedField.Local

        private IntPtr pActiveScript;
        private IntPtr pActiveScriptParse;
        private IntPtr pActiveScriptDebug;
        private IntPtr pActiveScriptGarbageCollector;
        private IntPtr pDebugStackFrameSniffer;

        private IActiveScript activeScript;
        private IActiveScriptParse64 activeScriptParse;
        private IActiveScriptDebug64 activeScriptDebug;
        private IActiveScriptGarbageCollector activeScriptGarbageCollector;
        private IDebugStackFrameSnifferEx64 debugStackFrameSniffer;

        // ReSharper restore NotAccessedField.Local

        private delegate uint RawInterruptScriptThread(
            [In] IntPtr pThis,
            [In] uint scriptThreadID,
            [In] ref EXCEPINFO excepInfo,
            [In] ScriptInterruptFlags flags
        );

        private delegate uint RawEnumCodeContextsOfPosition(
            [In] IntPtr pThis,
            [In] ulong sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );

        public ActiveScriptWrapper64(string progID)
        {
            pActiveScript = RawCOMHelpers.CreateInstance<IActiveScript>(progID);
            pActiveScriptParse = RawCOMHelpers.QueryInterface<IActiveScriptParse64>(pActiveScript);
            pActiveScriptDebug = RawCOMHelpers.QueryInterface<IActiveScriptDebug64>(pActiveScript);
            pActiveScriptGarbageCollector = RawCOMHelpers.QueryInterfaceNoThrow<IActiveScriptGarbageCollector>(pActiveScript);
            pDebugStackFrameSniffer = RawCOMHelpers.QueryInterface<IDebugStackFrameSnifferEx64>(pActiveScript);

            activeScript = (IActiveScript)Marshal.GetObjectForIUnknown(pActiveScript);
            activeScriptParse = (IActiveScriptParse64)activeScript;
            activeScriptDebug = (IActiveScriptDebug64)activeScript;
            activeScriptGarbageCollector = activeScript as IActiveScriptGarbageCollector;
            debugStackFrameSniffer = (IDebugStackFrameSnifferEx64)activeScript;
        }

        public override void SetScriptSite(IActiveScriptSite site)
        {
            activeScript.SetScriptSite(site);
        }

        public override void SetScriptState(ScriptState state)
        {
            activeScript.SetScriptState(state);
        }

        public override void GetScriptState(out ScriptState state)
        {
            activeScript.GetScriptState(out state);
        }

        public override void InitNew()
        {
            activeScriptParse.InitNew();
        }

        public override void GetScriptDispatch(string itemName, out object dispatch)
        {
            activeScript.GetScriptDispatch(itemName, out dispatch);
        }

        public override void AddNamedItem(string name, ScriptItemFlags flags)
        {
            activeScript.AddNamedItem(name, flags);
        }

        public override void ParseScriptText(string code, string itemName, object context, string delimiter, UIntPtr sourceContext, uint startingLineNumber, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo)
        {
            activeScriptParse.ParseScriptText(code, itemName, context, delimiter, sourceContext.ToUInt64(), startingLineNumber, flags, pVarResult, out excepInfo);
        }

        public override void InterruptScriptThread(uint scriptThreadID, ref EXCEPINFO excepInfo, ScriptInterruptFlags flags)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawInterruptScriptThread>(pActiveScript, 14);
            del(pActiveScript, scriptThreadID, ref excepInfo, flags);
        }

        public override void EnumCodeContextsOfPosition(UIntPtr sourceContext, uint offset, uint length, out IEnumDebugCodeContexts enumContexts)
        {
            var del = RawCOMHelpers.GetMethodDelegate<RawEnumCodeContextsOfPosition>(pActiveScriptDebug, 5);
            RawCOMHelpers.HResult.Check(del(pActiveScriptDebug, sourceContext.ToUInt64(), offset, length, out enumContexts));
        }

        public override void EnumStackFrames(out IEnumDebugStackFrames enumFrames)
        {
            debugStackFrameSniffer.EnumStackFrames(out enumFrames);
        }

        public override void CollectGarbage(ScriptGCType type)
        {
            if (activeScriptGarbageCollector != null)
            {
                activeScriptGarbageCollector.CollectGarbage(type);
            }
        }

        public override void Close()
        {
            activeScript.Close();

            debugStackFrameSniffer = null;
            activeScriptGarbageCollector = null;
            activeScriptDebug = null;
            activeScriptParse = null;
            activeScript = null;

            RawCOMHelpers.ReleaseAndEmpty(ref pDebugStackFrameSniffer);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptGarbageCollector);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptDebug);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScriptParse);
            RawCOMHelpers.ReleaseAndEmpty(ref pActiveScript);
        }
    }

    #endregion

    #region ProcessDebugManagerWrapper

    internal abstract class ProcessDebugManagerWrapper
    {
        public static ProcessDebugManagerWrapper Create()
        {
            if (Environment.Is64BitProcess)
            {
                return new ProcessDebugManagerWrapper64();
            }

            return new ProcessDebugManagerWrapper32();
        }

        public abstract void CreateApplication(out DebugApplicationWrapper applicationWrapper);

        public abstract void AddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie);

        public abstract void RemoveApplication(uint cookie);
    }

    internal class ProcessDebugManagerWrapper32 : ProcessDebugManagerWrapper
    {
        private readonly IProcessDebugManager32 processDebugManager;

        public ProcessDebugManagerWrapper32()
        {
            processDebugManager = (IProcessDebugManager32)MiscHelpers.CreateCOMObject("ProcessDebugManager");
        }

        public override void CreateApplication(out DebugApplicationWrapper applicationWrapper)
        {
            IDebugApplication32 debugApplication;
            processDebugManager.CreateApplication(out debugApplication);
            applicationWrapper = DebugApplicationWrapper.Create(debugApplication);
        }

        public override void AddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie)
        {
            processDebugManager.AddApplication(DebugApplicationWrapper32.Unwrap(applicationWrapper), out cookie);
        }

        public override void RemoveApplication(uint cookie)
        {
            processDebugManager.RemoveApplication(cookie);
        }
    }

    internal class ProcessDebugManagerWrapper64 : ProcessDebugManagerWrapper
    {
        private readonly IProcessDebugManager64 processDebugManager;

        public ProcessDebugManagerWrapper64()
        {
            processDebugManager = (IProcessDebugManager64)MiscHelpers.CreateCOMObject("ProcessDebugManager");
        }

        public override void CreateApplication(out DebugApplicationWrapper applicationWrapper)
        {
            IDebugApplication64 debugApplication;
            processDebugManager.CreateApplication(out debugApplication);
            applicationWrapper = DebugApplicationWrapper.Create(debugApplication);
        }

        public override void AddApplication(DebugApplicationWrapper applicationWrapper, out uint cookie)
        {
            processDebugManager.AddApplication(DebugApplicationWrapper64.Unwrap(applicationWrapper), out cookie);
        }

        public override void RemoveApplication(uint cookie)
        {
            processDebugManager.RemoveApplication(cookie);
        }
    }

    #endregion

    #region DebugApplicationWrapper

    internal abstract class DebugApplicationWrapper
    {
        public static DebugApplicationWrapper Create(IDebugApplication64 debugApplication)
        {
            return new DebugApplicationWrapper64(debugApplication);
        }

        public static DebugApplicationWrapper Create(IDebugApplication32 debugApplication)
        {
            return new DebugApplicationWrapper32(debugApplication);
        }

        public abstract void SetName(string name);

        public abstract void GetRootNode(out IDebugApplicationNode node);

        public abstract void CreateApplicationNode(out IDebugApplicationNode node);

        public abstract uint GetDebugger(out IApplicationDebugger debugger);

        public abstract void Close();
    }

    internal class DebugApplicationWrapper32 : DebugApplicationWrapper
    {
        private readonly IDebugApplication32 debugApplication;

        public DebugApplicationWrapper32(IDebugApplication32 debugApplication)
        {
           this.debugApplication = debugApplication;
        }

        public static IDebugApplication32 Unwrap(DebugApplicationWrapper wrapper)
        {
            var wrapper32 = wrapper as DebugApplicationWrapper32;
            return (wrapper32 != null) ? wrapper32.debugApplication : null;
        }

        public override void SetName(string name)
        {
            debugApplication.SetName(name);
        }

        public override void GetRootNode(out IDebugApplicationNode node)
        {
            debugApplication.GetRootNode(out node);
        }

        public override void CreateApplicationNode(out IDebugApplicationNode node)
        {
            debugApplication.CreateApplicationNode(out node);
        }

        public override uint GetDebugger(out IApplicationDebugger debugger)
        {
            return debugApplication.GetDebugger(out debugger);
        }

        public override void Close()
        {
            debugApplication.Close();
        }
    }

    internal class DebugApplicationWrapper64 : DebugApplicationWrapper
    {
        private readonly IDebugApplication64 debugApplication;

        public DebugApplicationWrapper64(IDebugApplication64 debugApplication)
        {
           this.debugApplication = debugApplication;
        }

        public static IDebugApplication64 Unwrap(DebugApplicationWrapper wrapper)
        {
            var wrapper64 = wrapper as DebugApplicationWrapper64;
            return (wrapper64 != null) ? wrapper64.debugApplication : null;
        }

        public override void SetName(string name)
        {
            debugApplication.SetName(name);
        }

        public override void GetRootNode(out IDebugApplicationNode node)
        {
            debugApplication.GetRootNode(out node);
        }

        public override void CreateApplicationNode(out IDebugApplicationNode node)
        {
            debugApplication.CreateApplicationNode(out node);
        }

        public override uint GetDebugger(out IApplicationDebugger debugger)
        {
            return debugApplication.GetDebugger(out debugger);
        }

        public override void Close()
        {
            debugApplication.Close();
        }
    }

    #endregion
}
