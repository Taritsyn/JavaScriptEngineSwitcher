using OriginalRuntimeException = Jint.Runtime.JintException;

namespace JavaScriptEngineSwitcher.Jint
{
	public class ScriptExecutionCanceledException : OriginalRuntimeException
	{
		public ScriptExecutionCanceledException()
			 : base("The script execution was canceled.")
		{ }
	}
}