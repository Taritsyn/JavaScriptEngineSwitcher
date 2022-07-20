using System;
using System.Collections.Generic;
using System.IO;

using NiL.JS;
using NiL.JS.Core;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.NiL
{
	internal sealed class Es2015ModuleLoader : IDisposable
	{
		//private Context _jsContext;
		private Dictionary<string, Module> _moduleCache = new Dictionary<string, Module>();

		/// <summary>
		/// Flag indicating whether this object is disposed
		/// </summary>
		private readonly InterlockedStatedFlag _disposedFlag = new InterlockedStatedFlag();


		public Es2015ModuleLoader(/*Context jsContext*/)
		{
			//_jsContext = jsContext;
		}


		public void ResolveModule(Module sender, ResolveModuleEventArgs e)
		{
			string parentModulePath = sender.FilePath;
			string relativeModulePath = e.ModulePath;
			string absolutePath = Path.Combine(Path.GetDirectoryName(parentModulePath), relativeModulePath);
			Module module;

			if (_moduleCache.ContainsKey(absolutePath))
			{
				module = _moduleCache[absolutePath];
			}
			else
			{
				string code = File.ReadAllText("." + absolutePath);
				GlobalContext context = sender.Context.GlobalContext;
				module = new Module(absolutePath, code, context);

				_moduleCache[absolutePath] = module;
			}
			module.Run();

			e.Module = module;
		}

		#region IDisposable implementation

		/// <summary>
		/// Disposes a module loader
		/// </summary>
		public void Dispose()
		{
			if (_disposedFlag.Set())
			{
				if (_moduleCache != null)
				{
					_moduleCache.Clear();
					_moduleCache = null;
				}
			}
		}

		#endregion
	}
}