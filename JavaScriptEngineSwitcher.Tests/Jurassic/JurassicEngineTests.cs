namespace JavaScriptEngineSwitcher.Tests.Jurassic
{
	using System;
	using System.IO;
	using System.Reflection;

	using NUnit.Framework;

	using Core;

	[TestFixture]
	public class JurassicJsEngineTests
	{
		private IJsEngine _jsEngine;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_jsEngine = JsEngineSwitcher.Current.CreateJsEngineInstance("JurassicJsEngine");
		}

		[Test]
		public void EvaluationOfExpressionIsCorrect()
		{
			// Arrange
			const string input = "'Hello, ' + \"Vasya\" + '!';";
			const string targetOutput = "Hello, Vasya!";

			// Act
			var output = (string)_jsEngine.Evaluate(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void GenericEvaluationOfExpressionIsCorrect()
		{
			// Arrange
			const string input = "5 * 8 - 12;";
			const int targetOutput = 28;

			// Act
			var output = _jsEngine.Evaluate<int>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void ExecutionOfCodeIsCorrect()
		{
			// Arrange
			const string jsLibraryCode = @"function add(num1, num2) {
				return (num1 + num2);
			}";
			const string input = "add(7, 9);";
			const int targetOutput = 16;

			// Act
			_jsEngine.Execute(jsLibraryCode);
			var output = _jsEngine.Evaluate<int>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void ExecutionOfFileIsCorrect()
		{
			// Arrange
			string jsLibraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
				@"../../Resources/square.js");
			const string input = "square(6);";
			const int targetOutput = 36;

			// Act
			_jsEngine.ExecuteFile(jsLibraryFilePath);
			var output = _jsEngine.Evaluate<int>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void ExecutionOfResourceByTypeIsCorrect()
		{
			// Arrange
			const string jsLibraryResourceName = "JavaScriptEngineSwitcher.Tests.Resources.cube.js";
			const string input = "cube(5);";
			const int targetOutput = 125;

			// Act
			_jsEngine.ExecuteResource(jsLibraryResourceName, GetType());
			var output = _jsEngine.Evaluate<int>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void ExecutionOfResourceByAssemblyIsCorrect()
		{
			// Arrange
			const string jsLibraryResourceName = "JavaScriptEngineSwitcher.Tests.Resources.power.js";
			const string input = "power(4, 3);";
			const int targetOutput = 64;

			// Act
			_jsEngine.ExecuteResource(jsLibraryResourceName, Assembly.GetExecutingAssembly());
			var output = _jsEngine.Evaluate<int>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public void CallingOfFunctionIsCorrect()
		{
			// Arrange
			const string jsFunctionCode = @"function greeting(name) {
	return 'Hello, ' + name + '!';
}";

			// Act
			_jsEngine.Execute(jsFunctionCode);
			var output = (string)_jsEngine.CallFunction("greeting", "Vovan");

			// Assert
			Assert.AreEqual("Hello, Vovan!", output);
		}

		[Test]
		public void CallingOfFunctionWithManyParametersIsCorrect()
		{
			// Arrange
			const string jsFunctionCode = @"function concatenate() {
	var result = '', 
		argumentIndex,
		argumentCount = arguments.length
		;

	for (argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++) {
		result += arguments[argumentIndex];
	}

	return result;
}";

			// Act
			_jsEngine.Execute(jsFunctionCode);
			var result = (string)_jsEngine.CallFunction("concatenate",
				"Peace, " + "labor" + ", may!", "\n",
				"Peace", ", ", "labor, ", "may", "!");

			// Assert
			Assert.AreEqual("Peace, labor, may!\nPeace, labor, may!", result);
		}

		[Test]
		public void GenericCallingOfFunctionIsCorrect()
		{
			// Arrange
			const string jsFunctionCode = @"function calculateTax(sum, taxRateInPercent) {
	return sum * taxRateInPercent / 100;
}";

			// Act
			_jsEngine.Execute(jsFunctionCode);
			var result = _jsEngine.CallFunction<double>("calculateTax", 30275, 13);

			// Assert
			Assert.AreEqual(3935.75, result);
		}

		[Test]
		public void GenericCallingOfFunctionWithManyParametersIsCorrect()
		{
			// Arrange
			const string jsFunctionCode = @"function sum() {
	var result = 0, 
		argumentIndex,
		argumentCount = arguments.length
		;

	for (argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++) {
		result += arguments[argumentIndex];
	}

	return result;
}";

			// Act
			_jsEngine.Execute(jsFunctionCode);
			var result = _jsEngine.CallFunction<double>("sum", 22000, 8.5, 0.05, 3);

			// Assert
			Assert.AreEqual(22011.55, result);
		}

		[Test]
		public void EcmaScript5IsSupported()
		{
			// Arrange
			const string input1 = "'	foo '.trim();";
			const string targetOutput1 = "foo";

			const string input2 = "[ 'w', 'e', 'b', 'm', 'a', 'r', 'k', " +
				"'u', 'p', 'm', 'i', 'n' ].lastIndexOf('m')";
			const int targetOutput2 = 9;

			const string input3A = @"var obj = new Object();
obj['foo'] = 'bar';";
			const string input3B = "JSON.stringify(obj);";
			const string targetOutput3 = "{\"foo\":\"bar\"}";

			// Act
			var output1 = _jsEngine.Evaluate<string>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);

			_jsEngine.Execute(input3A);
			var output3 = _jsEngine.Evaluate<string>(input3B);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
		}

		[Test]
		public void GettingAndSettingVariableIsCorrect()
		{
			// Arrange
			const string variableName = "word";

			const string input1 = "Hooray";
			const string targetOutput1 = "Hooray!";

			const string input2 = "Hurrah";

			// Act
			_jsEngine.SetVariableValue(variableName, input1);
			bool wordExists = _jsEngine.HasVariable(variableName);
			_jsEngine.Execute(string.Format("{0} += '!';", variableName));
			var output1 = (string)_jsEngine.GetVariableValue(variableName);

			_jsEngine.SetVariableValue(variableName, input2);
			var output2 = (string)_jsEngine.GetVariableValue(variableName);

			// Assert
			Assert.AreEqual(true, wordExists);
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(input2, output2);
		}

		[Test]
		public void GenericGettingAndSettingVariableIsCorrect()
		{
			// Arrange
			const string variableName = "price";

			const double input1 = 2.20;
			const double targetOutput1 = 2.17;

			const double input2 = 3.50;

			// Act
			_jsEngine.SetVariableValue(variableName, input1);
			bool priceExists = _jsEngine.HasVariable(variableName);
			_jsEngine.Execute(string.Format("{0} -= 0.03; {0} = {0}.toFixed(2);", variableName));
			var output1 = _jsEngine.GetVariableValue<double>(variableName);

			_jsEngine.SetVariableValue(variableName, input2);
			var output2 = _jsEngine.GetVariableValue<double>(variableName);

			// Assert
			Assert.AreEqual(true, priceExists);
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(input2, output2);
		}

		[Test]
		public void RemovingVariableIsCorrect()
		{
			// Arrange
			const string variableName = "price";
			const double input = 120.55;

			// Act
			_jsEngine.SetVariableValue(variableName, input);
			bool priceBeforeRemovingExists = _jsEngine.HasVariable(variableName);
			_jsEngine.RemoveVariable(variableName);
			bool priceAfterRemovingExists = _jsEngine.HasVariable(variableName);

			// Assert
			Assert.IsTrue(priceBeforeRemovingExists);
			Assert.IsFalse(priceAfterRemovingExists);
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			if (_jsEngine != null)
			{
				_jsEngine.Dispose();
			}
		}
	}
}