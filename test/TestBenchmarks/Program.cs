using BenchmarkDotNet.Running;

namespace TestBenchmarks
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<InteropBenchmark>();
		}
	}
}