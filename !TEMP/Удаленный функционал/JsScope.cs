using System;

using JavaScriptEngineSwitcher.Core.Utilities;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt
{
	/// <summary>
	/// The scope automatically sets a context to current and resets the original context
	/// when disposed
	/// </summary>
	internal struct JsScope : IDisposable
	{
		/// <summary>
		/// The previous context
		/// </summary>
		private readonly JsContext _previousContext;

		/// <summary>
		/// Whether the structure has been disposed
		/// </summary>
		private StatedFlag _disposedFlag;


		/// <summary>
		/// Initializes a new instance of the <see cref="JsScope"/> struct
		/// </summary>
		/// <param name="context">The context to create the scope for</param>
		public JsScope(JsContext context)
		{
			_disposedFlag = new StatedFlag();
			_previousContext = JsContext.Current;

			JsContext.Current = context;
		}


		#region IDisposable implementation

		/// <summary>
		/// Disposes the scope and sets the previous context to current
		/// </summary>
		public void Dispose()
		{
			if (_disposedFlag.Set())
			{
				JsContext.Current = _previousContext;
			}
		}

		#endregion
	}
}