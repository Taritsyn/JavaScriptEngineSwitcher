using System.Threading;

using IOriginalConstraint = Jint.IConstraint;

namespace JavaScriptEngineSwitcher.Jint
{
	internal sealed class CustomCancellationConstraint : IOriginalConstraint
	{
		private CancellationToken _cancellationToken;


		public CustomCancellationConstraint(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}


		public void Check()
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				throw new ScriptExecutionCanceledException();
			}
		}

		public void Reset(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}

		public void Reset()
		{ }
	}
}