using System;
using System.Collections.Generic;
#if !NETCOREAPP1_0
using System.Drawing;
#endif
using System.IO;
using System.Linq;

using Xunit;

using JavaScriptEngineSwitcher.Tests.Interop;
using JavaScriptEngineSwitcher.Tests.Interop.Animals;
#if NETCOREAPP1_0
using JavaScriptEngineSwitcher.Tests.Interop.Drawing;
#endif

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class InteropTestsBase : FileSystemTestsBase
	{
		#region Embedding of objects

		#region Objects with fields

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region Objects with properties

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
		}

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
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
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void EmbeddingOfInstanceOfAnonymousTypeWithPropertiesIsCorrect()
		{
			// Arrange
			var person = new
			{
				FirstName = "John",
				LastName = "Doe",
				Address = new
				{
					StreetAddress = "103 Elm Street",
					City = "Atlanta",
					State = "GA",
					PostalCode = 30339
				}
			};

			const string input1 = "person.FirstName";
			const string targetOutput1 = "John";

			const string input2 = "person.LastName";
			const string targetOutput2 = "Doe";

			const string input3 = "person.Address.StreetAddress";
			const string targetOutput3 = "103 Elm Street";

			const string input4 = "person.Address.City";
			const string targetOutput4 = "Atlanta";

			const string input5 = "person.Address.State";
			const string targetOutput5 = "GA";

			const string input6 = "person.Address.PostalCode";
			const int targetOutput6 = 30339;

			// Act
			string output1;
			string output2;
			string output3;
			string output4;
			string output5;
			int output6;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("person", person);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
				output3 = jsEngine.Evaluate<string>(input3);
				output4 = jsEngine.Evaluate<string>(input4);
				output5 = jsEngine.Evaluate<string>(input5);
				output6 = jsEngine.Evaluate<int>(input6);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
		}

		#endregion

		#region Objects with methods

		[Fact]
		public virtual void EmbeddingOfInstanceOfBuiltinValueTypeWithMethodsIsCorrect()
		{
			// Arrange
			var color = Color.FromArgb(84, 139, 212);

			const string input1 = "color.GetHue()";
			const double targetOutput1 = 214.21875;

			const string input2 = "color.GetSaturation()";
			const double targetOutput2 = 0.59813;

			const string input3 = "color.GetBrightness()";
			const double targetOutput3 = 0.58039;

			// Act
			double output1;
			double output2;
			double output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("color", color);

				output1 = Math.Round(jsEngine.Evaluate<double>(input1), 5);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 5);
				output3 = Math.Round(jsEngine.Evaluate<double>(input3), 5);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
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
			Assert.Contains(output, targetOutput);
		}

		[Fact]
		public virtual void EmbeddingOfInstanceOfCustomValueTypeWithMethodsIsCorrect()
		{
			// Arrange
			var programmerDayDate = new Date(2015, 9, 13);

			const string input1 = "programmerDay.GetDayOfYear()";
			const int targetOutput1 = 256;

			const string input2 = @"var smileDay = programmerDay.AddDays(6);
smileDay.GetDayOfYear();";
			const int targetOutput2 = 262;

			// Act
			int output1;
			int output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("programmerDay", programmerDayDate);
				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void EmbeddingOfInstanceOfCustomReferenceTypeWithMethodIsCorrect()
		{
			// Arrange
			var fileManager = new FileManager();
			string filePath = Path.GetFullPath(Path.Combine(_baseDirectoryPath, "../SharedFiles/link.txt"));

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
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void CallingOfMethodOfCustomReferenceTypeWithInterfaceParameterIsCorrect()
		{
			// Arrange
			var animalTrainer = new AnimalTrainer();
			var cat = new Cat();
			var dog = new Dog();

			const string input1 = "animalTrainer.ExecuteVoiceCommand(cat)";
			const string targetOutput1 = "Meow!";

			const string input2 = "animalTrainer.ExecuteVoiceCommand(dog)";
			const string targetOutput2 = "Woof!";

			// Act
			string output1;
			string output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("animalTrainer", animalTrainer);
				jsEngine.EmbedHostObject("cat", cat);
				jsEngine.EmbedHostObject("dog", dog);
				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region Delegates

		[Fact]
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
			Assert.NotEmpty(output);
			Assert.True(output.Length == targetOutputLength);
		}

		[Fact]
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
			Assert.Equal(targetOutput, output);
		}

		[Fact]
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
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Removal

		[Fact]
		public virtual void RemovingOfEmbeddedInstanceOfCustomReferenceTypeIsCorrect()
		{
			// Arrange
			var person = new Person("Vasya", "Pupkin");

			// Act
			Exception currentException = null;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostObject("person", person);

				try
				{
					jsEngine.RemoveVariable("person");
				}
				catch (Exception e)
				{
					currentException = e;
				}
			}

			// Assert
			Assert.Null(currentException);
		}

		#endregion

		#endregion


		#region Embedding of types

		#region Creating of instances

		[Fact]
		public virtual void CreatingAnInstanceOfEmbeddedBuiltinValueTypeIsCorrect()
		{
			// Arrange
			Type pointType = typeof(Point);

			const string input = "(new Point()).ToString()";
			const string targetOutput = "{X=0,Y=0}";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Point", pointType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void CreatingAnInstanceOfEmbeddedBuiltinReferenceTypeIsCorrect()
		{
			// Arrange
			Type uriType = typeof(Uri);

			const string input = @"var baseUri = new Uri('https://github.com'),
	relativeUri = 'Taritsyn/MsieJavaScriptEngine'
	;

(new Uri(baseUri, relativeUri)).ToString()";
			const string targetOutput = "https://github.com/Taritsyn/MsieJavaScriptEngine";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Uri", uriType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void CreatingAnInstanceOfEmbeddedCustomValueTypeIsCorrect()
		{
			// Arrange
			Type point3DType = typeof(Point3D);

			const string input = "(new Point3D(2, 5, 14)).ToString()";
			const string targetOutput = "{X=2,Y=5,Z=14}";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Point3D", point3DType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void CreatingAnInstanceOfEmbeddedCustomReferenceTypeIsCorrect()
		{
			// Arrange
			Type personType = typeof(Person);

			const string input = "(new Person('Vanya', 'Tutkin')).ToString()";
			const string targetOutput = "{FirstName=Vanya,LastName=Tutkin}";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Person", personType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with constants

		[Fact]
		public virtual void EmbeddingOfBuiltinReferenceTypeWithConstantsIsCorrect()
		{
			// Arrange
			Type mathType = typeof(Math);

			const string input1 = "Math2.PI";
			const double targetOutput1 = 3.1415926535897931d;

			const string input2 = "Math2.E";
			const double targetOutput2 = 2.7182818284590451d;

			// Act
			double output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Math2", mathType);

				output1 = jsEngine.Evaluate<double>(input1);
				output2 = jsEngine.Evaluate<double>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void EmbeddingOfCustomValueTypeWithConstantsIsCorrect()
		{
			// Arrange
			Type predefinedStringsType = typeof(PredefinedStrings);

			const string input1 = "PredefinedStrings.VeryLongName";
			const string targetOutput1 = "Very Long Name";

			const string input2 = "PredefinedStrings.AnotherVeryLongName";
			const string targetOutput2 = "Another Very Long Name";

			const string input3 = "PredefinedStrings.TheLastVeryLongName";
			const string targetOutput3 = "The Last Very Long Name";

			// Act
			string output1;
			string output2;
			string output3;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("PredefinedStrings", predefinedStringsType);

				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
				output3 = jsEngine.Evaluate<string>(input3);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
		}

		[Fact]
		public virtual void EmbeddingOfCustomReferenceTypeWithConstantIsCorrect()
		{
			// Arrange
			Type base64EncoderType = typeof(Base64Encoder);

			const string input = "Base64Encoder.DATA_URI_MAX";
			const int targetOutput = 32768;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Base64Encoder", base64EncoderType);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with fields

		[Fact]
		public virtual void EmbeddingOfBuiltinValueTypeWithFieldIsCorrect()
		{
			// Arrange
			Type guidType = typeof(Guid);

			const string input = "Guid.Empty.ToString()";
			const string targetOutput = "00000000-0000-0000-0000-000000000000";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Guid", guidType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfBuiltinReferenceTypeWithFieldIsCorrect()
		{
			// Arrange
			Type bitConverterType = typeof(BitConverter);

			const string input = "BitConverter.IsLittleEndian";
			const bool targetOutput = true;

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("BitConverter", bitConverterType);
				output = (bool)jsEngine.Evaluate(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfCustomValueTypeWithFieldIsCorrect()
		{
			// Arrange
			Type point3DType = typeof(Point3D);

			const string input = "Point3D.Empty.ToString()";
			const string targetOutput = "{X=0,Y=0,Z=0}";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Point3D", point3DType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfCustomReferenceTypeWithFieldIsCorrect()
		{
			// Arrange
			Type simpleSingletonType = typeof(SimpleSingleton);

			const string input = "SimpleSingleton.Instance.ToString()";
			const string targetOutput = "[simple singleton]";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("SimpleSingleton", simpleSingletonType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with properties

		[Fact]
		public virtual void EmbeddingOfBuiltinValueTypeWithPropertyIsCorrect()
		{
			// Arrange
			Type colorType = typeof(Color);

			const string input = "Color.OrangeRed.ToString()";
			const string targetOutput = "Color [OrangeRed]";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Color", colorType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfBuiltinReferenceTypeWithPropertyIsCorrect()
		{
			// Arrange
			Type environmentType = typeof(Environment);

			const string input = "Environment.NewLine";
			string[] targetOutput = { "\r", "\r\n", "\n", "\n\r" };

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Environment", environmentType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Contains(output, targetOutput);
		}

		[Fact]
		public virtual void EmbeddingOfCustomValueTypeWithPropertyIsCorrect()
		{
			// Arrange
			Type dateType = typeof(Date);

			const string initCode = "var currentDate = Date2.Today;";

			const string inputYear = "currentDate.Year";
			const string inputMonth = "currentDate.Month";
			const string inputDay = "currentDate.Day";

			DateTime targetOutput = DateTime.Today;

			// Act
			DateTime output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Date2", dateType);
				jsEngine.Execute(initCode);

				var outputYear = jsEngine.Evaluate<int>(inputYear);
				var outputMonth = jsEngine.Evaluate<int>(inputMonth);
				var outputDay = jsEngine.Evaluate<int>(inputDay);

				output = new DateTime(outputYear, outputMonth, outputDay);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfCustomReferenceTypeWithPropertyIsCorrect()
		{
			// Arrange
			Type bundleTableType = typeof(BundleTable);
			const string updateCode = "BundleTable.EnableOptimizations = false;";

			const string input = "BundleTable.EnableOptimizations";
			const bool targetOutput = false;

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("BundleTable", bundleTableType);
				jsEngine.Execute(updateCode);

				output = jsEngine.Evaluate<bool>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Types with methods

		[Fact]
		public virtual void EmbeddingOfBuiltinValueTypeWithMethodIsCorrect()
		{
			// Arrange
			Type dateTimeType = typeof(DateTime);

			const string input = "DateTime.DaysInMonth(2016, 2)";
			const int targetOutput = 29;

			// Act
			int output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("DateTime", dateTimeType);
				output = jsEngine.Evaluate<int>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfBuiltinReferenceTypeWithMethodsIsCorrect()
		{
			// Arrange
			Type mathType = typeof(Math);

			const string input1 = "Math2.Max(5.37, 5.56)";
			const double targetOutput1 = 5.56;

			const string input2 = "Math2.Log10(23)";
			const double targetOutput2 = 1.36172783601759;

			// Act
			double output1;
			double output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Math2", mathType);
				output1 = jsEngine.Evaluate<double>(input1);
				output2 = Math.Round(jsEngine.Evaluate<double>(input2), 14);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void EmbeddingOfCustomValueTypeWithMethodIsCorrect()
		{
			// Arrange
			var dateType = typeof(Date);

			const string input = "Date2.IsLeapYear(2016)";
			const bool targetOutput = true;

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Date2", dateType);
				output = jsEngine.Evaluate<bool>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void EmbeddingOfCustomReferenceTypeWithMethodIsCorrect()
		{
			// Arrange
			Type base64EncoderType = typeof(Base64Encoder);

			const string input = "Base64Encoder.Encode('https://github.com/Taritsyn/MsieJavaScriptEngine')";
			const string targetOutput = "aHR0cHM6Ly9naXRodWIuY29tL1Rhcml0c3luL01zaWVKYXZhU2NyaXB0RW5naW5l";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Base64Encoder", base64EncoderType);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Removal

		[Fact]
		public virtual void RemovingOfEmbeddedCustomReferenceTypeIsCorrect()
		{
			// Arrange
			Type personType = typeof(Person);

			// Act
			Exception currentException = null;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.EmbedHostType("Person", personType);

				try
				{
					jsEngine.RemoveVariable("Person");
				}
				catch (Exception e)
				{
					currentException = e;
				}
			}

			// Assert
			Assert.Null(currentException);
		}

		#endregion

		#endregion
	}
}