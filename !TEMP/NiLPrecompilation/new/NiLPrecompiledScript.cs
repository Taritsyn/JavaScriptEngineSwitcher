using System;
using System.Runtime.CompilerServices;
using System.Threading;

using OriginalScript = NiL.JS.Script;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.NiL
{
	/// <summary>
	/// Represents a pre-compiled script that can be executed by different instances of the NiL JS engine
	/// </summary>
	internal sealed class NiLPrecompiledScript : IPrecompiledScript
	{
		/// <summary>
		/// Source code of the script
		/// </summary>
		private readonly string _code;

		/// <summary>
		/// First script
		/// </summary>
		/// <remarks>
		/// The first script is stored in a dedicated field, because we expect to be able to satisfy
		/// most requests from it.
		/// </remarks>
		private OriginalScript _firstScript;

		/// <summary>
		/// Array of the retained scripts
		/// </summary>
		private readonly OriginalScript[] _scripts;


		/// <summary>
		/// Constructs an instance of pre-compiled script
		/// </summary>
		/// <param name="code">The source code of the script</param>
		/// <param name="script">The script</param>
		public NiLPrecompiledScript(string code, OriginalScript script)
		{
			int poolSize = Environment.ProcessorCount * 2;

			_code = code;
			_firstScript = script;
			_scripts = new OriginalScript[poolSize - 1];
		}


		public OriginalScript Rent()
		{
			// Examine the first script.
			// If that fails, then `RentViaScan` method will look at the remaining scripts.
			OriginalScript script = _firstScript;
			if (script == null || script != Interlocked.CompareExchange(ref _firstScript, null, script))
			{
				script = RentViaScan();
			}

			return script;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private OriginalScript RentViaScan()
		{
			OriginalScript[] scripts = _scripts;
			int scriptCount = scripts.Length;

			for (int scriptIndex = 0; scriptIndex < scriptCount; scriptIndex++)
			{
				OriginalScript script = scripts[scriptIndex];
				if (script != null)
				{
					if (script == Interlocked.CompareExchange(ref scripts[scriptIndex], null, script))
					{
						return script;
					}
				}
			}

			return OriginalScript.Parse(_code);
		}

		public void Return(OriginalScript script)
		{
			if (script == null)
			{
				return;
			}

			if (_firstScript == null)
			{
				_firstScript = script;
			}
			else
			{
				ReturnViaScan(script);
			}
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private void ReturnViaScan(OriginalScript script)
		{
			OriginalScript[] scripts = _scripts;
			int scriptCount = scripts.Length;

			for (int scriptIndex = 0; scriptIndex < scriptCount; scriptIndex++)
			{
				if (scripts[scriptIndex] == null)
				{
					scripts[scriptIndex] = script;
					break;
				}
			}
		}

		#region IPrecompiledScript implementation

		/// <summary>
		/// Gets a name of JS engine for which the pre-compiled script was created
		/// </summary>
		public string EngineName
		{
			get { return NiLJsEngine.EngineName; }
		}

		#endregion
	}
}