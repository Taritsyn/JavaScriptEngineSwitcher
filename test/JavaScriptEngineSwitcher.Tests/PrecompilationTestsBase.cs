using System.IO;
using System.Reflection;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class PrecompilationTestsBase : FileSystemTestsBase
	{
		#region Execution of precompiled scripts

		[Fact]
		public virtual void ExecutionOfPrecompiledCodeIsCorrect()
		{
			// Arrange
			const string libraryCode = @"function declensionOfNumerals(number, titles) {
	var result,
		titleIndex,
		cases = [2, 0, 1, 1, 1, 2],
		caseIndex
		;

	if (number % 100 > 4 && number % 100 < 20) {
		titleIndex = 2;
	}
	else {
		caseIndex = number % 10 < 5 ? number % 10 : 5;
		titleIndex = cases[caseIndex];
	}

	result = titles[titleIndex];

	return result;
}

function declinationOfSeconds(number) {
	return declensionOfNumerals(number, ['секунда', 'секунды', 'секунд']);
}";
			const string functionName = "declinationOfSeconds";

			const int input0 = 0;
			const string targetOutput0 = "секунд";

			const int input1 = 1;
			const string targetOutput1 = "секунда";

			const int input2 = 42;
			const string targetOutput2 = "секунды";

			const int input3 = 600;
			const string targetOutput3 = "секунд";

			// Act
			bool supportsScriptPrecompilation = false;
			IPrecompiledScript precompiledCode = null;

			string output0 = string.Empty;
			string output1 = string.Empty;
			string output2 = string.Empty;
			string output3 = string.Empty;

			using (var jsEngine = CreateJsEngine())
			{
				supportsScriptPrecompilation = jsEngine.SupportsScriptPrecompilation;
				if (supportsScriptPrecompilation)
				{
					precompiledCode = jsEngine.Precompile(libraryCode, "declinationOfSeconds.js");

					jsEngine.Execute(precompiledCode);
					output0 = jsEngine.CallFunction<string>(functionName, input0);
				}
			}

			if (supportsScriptPrecompilation)
			{
				using (var firstJsEngine = CreateJsEngine())
				{
					firstJsEngine.Execute(precompiledCode);
					output1 = firstJsEngine.CallFunction<string>(functionName, input1);
				}

				using (var secondJsEngine = CreateJsEngine())
				{
					secondJsEngine.Execute(precompiledCode);
					output2 = secondJsEngine.CallFunction<string>(functionName, input2);
				}

				using (var thirdJsEngine = CreateJsEngine())
				{
					thirdJsEngine.Execute(precompiledCode);
					output3 = thirdJsEngine.CallFunction<string>(functionName, input3);
				}
			}

			// Assert
			if (supportsScriptPrecompilation)
			{
				Assert.Equal(targetOutput0, output0);
				Assert.Equal(targetOutput1, output1);
				Assert.Equal(targetOutput2, output2);
				Assert.Equal(targetOutput3, output3);
			}
		}

		[Fact]
		public virtual void ExecutionOfPrecompiledFileIsCorrect()
		{
			// Arrange
			string filePath = Path.GetFullPath(Path.Combine(_baseDirectoryPath, "../SharedFiles/declinationOfMinutes.js"));
			const string functionName = "declinationOfMinutes";

			const int input0 = 0;
			const string targetOutput0 = "минут";

			const int input1 = 1;
			const string targetOutput1 = "минута";

			const int input2 = 22;
			const string targetOutput2 = "минуты";

			const int input3 = 88;
			const string targetOutput3 = "минут";

			// Act
			bool supportsScriptPrecompilation = false;
			IPrecompiledScript precompiledFile = null;

			string output0 = string.Empty;
			string output1 = string.Empty;
			string output2 = string.Empty;
			string output3 = string.Empty;

			using (var jsEngine = CreateJsEngine())
			{
				supportsScriptPrecompilation = jsEngine.SupportsScriptPrecompilation;
				if (supportsScriptPrecompilation)
				{
					precompiledFile = jsEngine.PrecompileFile(filePath);

					jsEngine.Execute(precompiledFile);
					output0 = jsEngine.CallFunction<string>(functionName, input0);
				}
			}

			if (supportsScriptPrecompilation)
			{
				using (var firstJsEngine = CreateJsEngine())
				{
					firstJsEngine.Execute(precompiledFile);
					output1 = firstJsEngine.CallFunction<string>(functionName, input1);
				}

				using (var secondJsEngine = CreateJsEngine())
				{
					secondJsEngine.Execute(precompiledFile);
					output2 = secondJsEngine.CallFunction<string>(functionName, input2);
				}

				using (var thirdJsEngine = CreateJsEngine())
				{
					thirdJsEngine.Execute(precompiledFile);
					output3 = thirdJsEngine.CallFunction<string>(functionName, input3);
				}
			}

			// Assert
			if (supportsScriptPrecompilation)
			{
				Assert.Equal(targetOutput0, output0);
				Assert.Equal(targetOutput1, output1);
				Assert.Equal(targetOutput2, output2);
				Assert.Equal(targetOutput3, output3);
			}
		}

		[Fact]
		public virtual void ExecutionOfPrecompiledResourceByNameAndTypeIsCorrect()
		{
			// Arrange
			const string resourceName = "Resources.declinationOfHours.js";
			const string functionName = "declinationOfHours";

			const int input0 = 0;
			const string targetOutput0 = "часов";

			const int input1 = 1;
			const string targetOutput1 = "час";

			const int input2 = 24;
			const string targetOutput2 = "часа";

			const int input3 = 48;
			const string targetOutput3 = "часов";

			// Act
			bool supportsScriptPrecompilation = false;
			IPrecompiledScript precompiledResource = null;

			string output0 = string.Empty;
			string output1 = string.Empty;
			string output2 = string.Empty;
			string output3 = string.Empty;

			using (var jsEngine = CreateJsEngine())
			{
				supportsScriptPrecompilation = jsEngine.SupportsScriptPrecompilation;
				if (supportsScriptPrecompilation)
				{
					precompiledResource = jsEngine.PrecompileResource(resourceName, typeof(CommonTestsBase));

					jsEngine.Execute(precompiledResource);
					output0 = jsEngine.CallFunction<string>(functionName, input0);
				}
			}

			if (supportsScriptPrecompilation)
			{
				using (var firstJsEngine = CreateJsEngine())
				{
					firstJsEngine.Execute(precompiledResource);
					output1 = firstJsEngine.CallFunction<string>(functionName, input1);
				}

				using (var secondJsEngine = CreateJsEngine())
				{
					secondJsEngine.Execute(precompiledResource);
					output2 = secondJsEngine.CallFunction<string>(functionName, input2);
				}

				using (var thirdJsEngine = CreateJsEngine())
				{
					thirdJsEngine.Execute(precompiledResource);
					output3 = thirdJsEngine.CallFunction<string>(functionName, input3);
				}
			}

			// Assert
			if (supportsScriptPrecompilation)
			{
				Assert.Equal(targetOutput0, output0);
				Assert.Equal(targetOutput1, output1);
				Assert.Equal(targetOutput2, output2);
				Assert.Equal(targetOutput3, output3);
			}
		}

		[Fact]
		public virtual void ExecutionOfPrecompiledResourceByNameAndAssemblyIsCorrect()
		{
			// Arrange
			const string resourceName = "JavaScriptEngineSwitcher.Tests.Resources.declinationOfDays.js";
			const string functionName = "declinationOfDays";

			const int input0 = 0;
			const string targetOutput0 = "дней";

			const int input1 = 1;
			const string targetOutput1 = "день";

			const int input2 = 3;
			const string targetOutput2 = "дня";

			const int input3 = 80;
			const string targetOutput3 = "дней";

			// Act
			bool supportsScriptPrecompilation = false;
			IPrecompiledScript precompiledResource = null;

			string output0 = string.Empty;
			string output1 = string.Empty;
			string output2 = string.Empty;
			string output3 = string.Empty;

			using (var jsEngine = CreateJsEngine())
			{
				supportsScriptPrecompilation = jsEngine.SupportsScriptPrecompilation;
				if (supportsScriptPrecompilation)
				{
					precompiledResource = jsEngine.PrecompileResource(resourceName,
						typeof(CommonTestsBase).GetTypeInfo().Assembly);

					jsEngine.Execute(precompiledResource);
					output0 = jsEngine.CallFunction<string>(functionName, input0);
				}
			}

			if (supportsScriptPrecompilation)
			{
				using (var firstJsEngine = CreateJsEngine())
				{
					firstJsEngine.Execute(precompiledResource);
					output1 = firstJsEngine.CallFunction<string>(functionName, input1);
				}

				using (var secondJsEngine = CreateJsEngine())
				{
					secondJsEngine.Execute(precompiledResource);
					output2 = secondJsEngine.CallFunction<string>(functionName, input2);
				}

				using (var thirdJsEngine = CreateJsEngine())
				{
					thirdJsEngine.Execute(precompiledResource);
					output3 = thirdJsEngine.CallFunction<string>(functionName, input3);
				}
			}

			// Assert
			if (supportsScriptPrecompilation)
			{
				Assert.Equal(targetOutput0, output0);
				Assert.Equal(targetOutput1, output1);
				Assert.Equal(targetOutput2, output2);
				Assert.Equal(targetOutput3, output3);
			}
		}

		#endregion
	}
}