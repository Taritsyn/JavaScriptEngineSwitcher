using System;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Yantra
{
	/// <summary>
	/// JS engine factory collection extensions
	/// </summary>
	public static class JsEngineFactoryCollectionExtensions
	{
		/// <summary>
		/// Adds a instance of <see cref="YantraJsEngineFactory"/> to
		/// the specified <see cref="JsEngineFactoryCollection"/>
		/// </summary>
		/// <param name="source">Instance of <see cref="JsEngineFactoryCollection"/></param>
		/// <returns>Instance of <see cref="JsEngineFactoryCollection"/></returns>
		public static JsEngineFactoryCollection AddYantra(this JsEngineFactoryCollection source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			source.Add(new YantraJsEngineFactory());

			return source;
		}
	}
}