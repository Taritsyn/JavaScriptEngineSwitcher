using BenchmarkDotNet.Running;

namespace JavaScriptEngineSwitcher.Benchmarks
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<JsExecutionBenchmark>();
		}
	}
}