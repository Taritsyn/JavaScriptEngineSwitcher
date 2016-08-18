using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="ChakraCoreJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection" />
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection" /></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection" /></returns>
		public static JsEngineFactoryCollection AddChakraCore(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			source.Add(new ChakraCoreJsEngineFactory());

			return source;
		}
	}
}