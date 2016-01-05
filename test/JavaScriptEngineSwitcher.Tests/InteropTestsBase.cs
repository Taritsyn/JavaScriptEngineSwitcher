namespace JavaScriptEngineSwitcher.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;

	using NUnit.Framework;

	using Core;
	using Interop;

	[TestFixture]
	public abstract class InteropTestsBase : FileSystemTestsBase
	{
		protected abstract IJsEngine CreateJsEngine();

		#region Embedding of objects

		#region Objects with fields

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomValueTypeWithFieldsIsCorrect()
		{
			// Arrange
			var date = new Date(2015, 12, 29);
			const string updateCode = "date.Day += 2;";

			const string input1 = "date.Year";
			const int targetOutput1 = 2015;

			const string input2 = "date.Month";
			const int targetOutput2 = 12;

			const string input3 = "date.Day";
			const int targetOutput3 = 31;

			// Act
			int output1;
			int output2;
			int output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("date", date);
				jsEngine.Execute(updateCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomReferenceTypeWithFieldsIsCorrect()
		{
			// Arrange
			var product = new Product
			{
				Name = "Red T-shirt",
				Price = 995.00
			};

			const string updateCode = "product.Price *= 1.15;";

			const string input1 = "product.Name";
			const string targetOutput1 = "Red T-shirt";

			const string input2 = "product.Price";
			const double targetOutput2 = 1144.25;

			// Act
			string output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("product", product);
				jsEngine.Execute(updateCode);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<double>(input2);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}

		#endregion

		#region Objects with properties

		[Test]
		public virtual void EmbeddingOfInstanceOfBuiltinValueTypeWithPropertiesIsCorrect()
		{
			// Arrange
			var timeSpan = new TimeSpan(4840780000000);

			const string input1 = "timeSpan.Days";
			const int targetOutput1 = 5;

			const string input2 = "timeSpan.Hours";
			const int targetOutput2 = 14;

			const string input3 = "timeSpan.Minutes";
			const int targetOutput3 = 27;

			const string input4 = "timeSpan.Seconds";
			const int targetOutput4 = 58;

			// Act
			int output1;
			int output2;
			int output3;
			int output4;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("timeSpan", timeSpan);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
				output4 = jsEngine.Evaluate<int>(input4);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
			Assert.AreEqual(targetOutput4, output4);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfBuiltinReferenceTypeWithPropertiesIsCorrect()
		{
			// Arrange
			var uri = new Uri("https://github.com/Taritsyn/MsieJavaScriptEngine");

			const string input1 = "uri.Scheme";
			const string targetOutput1 = "https";

			const string input2 = "uri.Host";
			const string targetOutput2 = "github.com";

			const string input3 = "uri.PathAndQuery";
			const string targetOutput3 = "/Taritsyn/MsieJavaScriptEngine";

			// Act
			string output1;
			string output2;
			string output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("uri", uri);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
				output3 = jsEngine.Evaluate<string>(input3);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomValueTypeWithPropertiesIsCorrect()
		{
			// Arrange
			var temperature = new Temperature(-17.3, TemperatureUnits.Celsius);

			const string input1 = "temperature.Celsius";
			const double targetOutput1 = -17.3;

			const string input2 = "temperature.Kelvin";
			const double targetOutput2 = 255.85;

			const string input3 = "temperature.Fahrenheit";
			const double targetOutput3 = 0.86;

			// Act
			double output1;
			double output2;
			double output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("temperature", temperature);

				output1 = Math.Round(jsEngine.Evaluate<double>(input1), 2);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 2);
				output3 = Math.Round(jsEngine.Evaluate<double>(input3), 2);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomReferenceTypeWithPropertiesIsCorrect()
		{
			// Arrange
			var person = new Person("Vanya", "Ivanov");
			const string updateCode = "person.LastName = person.LastName.substr(0, 5) + 'ff';";

			const string input1 = "person.FirstName";
			const string targetOutput1 = "Vanya";

			const string input2 = "person.LastName";
			const string targetOutput2 = "Ivanoff";

			// Act
			string output1;
			string output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("person", person);
				jsEngine.Execute(updateCode);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}

		#endregion

		#region Objects with methods

		[Test]
		public virtual void EmbeddingOfInstanceOfBuiltinValueTypeWithMethodsIsCorrect()
		{
			// Arrange
			var color = Color.FromArgb(84, 139, 212);

			const string input1 = "color.GetHue()";
			const double targetOutput1 = 214.21875d;

			const string input2 = "color.GetSaturation()";
			const double targetOutput2 = 0.59813079999999996d;

			const string input3 = "color.GetBrightness()";
			const double targetOutput3 = 0.58039220000000002d;

			// Act
			double output1;
			double output2;
			double output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("color", color);

				output1 = Math.Round(jsEngine.Evaluate<double>(input1), 7);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 7);
				output3 = Math.Round(jsEngine.Evaluate<double>(input3), 7);
			}

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfBuiltinReferenceTypeWithMethodIsCorrect()
		{
			// Arrange
			var random = new Random();

			const string input = "random.Next(1, 3)";
			IEnumerable<int> targetOutput = Enumerable.Range(1, 3);

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("random", random);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.IsTrue(targetOutput.Contains(output));
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomValueTypeWithMethodIsCorrect()
		{
			// Arrange
			var programmerDayDate = new Date(2015, 9, 13);

			const string input = "programmerDay.GetDayOfYear()";
			const int targetOutput = 256;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("programmerDay", programmerDayDate);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfCustomReferenceTypeWithMethodIsCorrect()
		{
			// Arrange
			var fileManager = new FileManager();
			string filePath = Path.GetFullPath(Path.Combine(_baseDirectoryPath, "JavaScriptEngineSwitcher.Tests/Files/link.txt"));

			string input = string.Format("fileManager.ReadFile('{0}')", filePath.Replace(@"\", @"\\"));
			const string targetOutput = "http://www.panopticoncentral.net/2015/09/09/the-two-faces-of-jsrt-in-windows-10/";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("fileManager", fileManager);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		#endregion

		#region Delegates

		[Test]
		public virtual void EmbeddingOfInstanceOfDelegateWithoutParametersIsCorrect()
		{
			// Arrange
			var generateRandomStringFunc = new Func<string>(() =>
			{
				const string symbolString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
				int symbolStringLength = symbolString.Length;
				Random randomizer = new Random();
				string result = string.Empty;

				for (int i = 0; i < 20; i++)
				{
					int randomNumber = randomizer.Next(symbolStringLength);
					string randomSymbol = symbolString.Substring(randomNumber, 1);

					result += randomSymbol;
				}

				return result;
			});

			const string input = "generateRandomString()";
			const int targetOutputLength = 20;

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("generateRandomString", generateRandomStringFunc);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.IsNotNullOrEmpty(output);
			Assert.IsTrue(output.Length == targetOutputLength);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfDelegateWithOneParameterIsCorrect()
		{
			// Arrange
			var squareFunc = new Func<int, int>(a => a * a);

			const string input = "square(7)";
			const int targetOutput = 49;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("square", squareFunc);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public virtual void EmbeddingOfInstanceOfDelegateWithTwoParametersIsCorrect()
		{
			// Arrange
			var sumFunc = new Func<double, double, double>((a, b) => a + b);

			const string input = "sum(3.14, 2.20)";
			const double targetOutput = 5.34;

			// Act
			double output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("sum", sumFunc);
				output = jsEngine.Evaluate<double>(input);
			}

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		#endregion

		#endregion
	}
}