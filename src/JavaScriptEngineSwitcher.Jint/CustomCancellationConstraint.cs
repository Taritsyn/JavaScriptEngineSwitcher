using System.Threading;

using OriginalConstraint = Jint.Constraint;
using OriginalExecutionCanceledException = Jint.Runtime.ExecutionCanceledException;

namespace JavaScriptEngineSwitcher.Jint
{
	internal class CustomCancellationConstraint : OriginalConstraint
	{
		private CancellationToken _cancellationToken;


		public CustomCancellationConstraint(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}


		public override void Check()
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				throw new OriginalExecutionCanceledException();
			}
		}

		public void Reset(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}

		public override void Reset()
		{
		}
	}
}