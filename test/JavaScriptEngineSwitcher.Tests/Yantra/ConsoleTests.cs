#if NET471 || NETCOREAPP3_1_OR_GREATER
using System;
using System.Globalization;
using System.Text;

using Xunit;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Yantra;

namespace JavaScriptEngineSwitcher.Tests.Yantra
{
	public class ConsoleTests
	{
		private IJsEngine CreateJsEngine(YantraJsConsoleCallback consoleCallback)
		{
			var jsEngine = new YantraJsEngine(new YantraSettings
			{
				ConsoleCallback = consoleCallback
			});

			return jsEngine;
		}

		[Fact]
		public void SupportsConsoleLogMethod()
		{
			// Arrange
			Type favoriteSchoolSubject = typeof(Math);
			var wikipediaPageUrl = new Uri("https://ru.wikipedia.org/wiki/%D0%92%D0%B0%D1%81%D1%8F_" +
				"%D0%9F%D1%83%D0%BF%D0%BA%D0%B8%D0%BD");
			var sb = new StringBuilder();
			var logger = new StringLogger(sb);

			const string input = @"var id = Symbol('id'),
	name = 'Василий Пупкин',
	address = { city: 'Тамбов', street: 'Магистральная', ""houseNumber"": '41к7', apartmentNumber: 115 },
	dateOfBirth = new Date(1990, 2, 15),
	isSingle = true,
	salary = 22000.82,
	email = null,
	website = undefined,
	icq = 698426795,
	pets = ['Мурзик', 'Шарик']
	;

console.log(website, email, address, pets, isSingle, icq, salary, name, id, dateOfBirth);

function calculateIncomeTax(salary) {
	var result = salary * 0.13;

	return result;
}

console.log('Функция для расчета подоходного налога:', calculateIncomeTax);
console.log('Папа у Васи силен в', favoriteSchoolSubject);
console.log('Страница в Википедии:', wikipediaPageUrl);";
			string targetOutput = "undefined null {\"city\":\"Тамбов\",\"street\":\"Магистральная\"," +
				"\"houseNumber\":\"41к7\",\"apartmentNumber\":115} [\"Мурзик\",\"Шарик\"] True 698426795 22000.82 " +
				"Василий Пупкин Symbol(id) 1990-03-14T21:00:00.0000000Z" + Environment.NewLine +
				"Функция для расчета подоходного налога: [Function: calculateIncomeTax]" + Environment.NewLine +
				"Папа у Васи силен в System.Math" + Environment.NewLine +
				"Страница в Википедии: https://ru.wikipedia.org/wiki/Вася_Пупкин" + Environment.NewLine
				;

			// Act
			using (var jsEngine = CreateJsEngine(consoleCallback: logger.Log))
			{
				jsEngine.EmbedHostType("favoriteSchoolSubject", favoriteSchoolSubject);
				jsEngine.EmbedHostObject("wikipediaPageUrl", wikipediaPageUrl);
				jsEngine.Execute(input);
			}

			string output = sb.ToString();

			logger.Dispose();
			sb.Clear();

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public void SupportsConsoleInfoMethod()
		{
			// Arrange
			var sb = new StringBuilder();
			var logger = new StringLogger(sb);

			const string input = @"var driveLetter = 'C', availableDiskSpace = 237;

console.info('There are', availableDiskSpace, 'megabytes available on', driveLetter, 'drive.');
console.info('Everything is going according to plan.');
console.info(driveLetter, 'drive has been formatted successfully!');";

			// Act
			IJsEngine jsEngine = null;
			JsRuntimeException exception = null;

			try
			{
				jsEngine = CreateJsEngine(consoleCallback: logger.Log);
				jsEngine.Execute(input);
			}
			catch (JsRuntimeException e)
			{
				exception = e;
			}
			finally
			{
				jsEngine?.Dispose();
				logger.Dispose();
				sb.Clear();
			}

			// Assert
			Assert.NotNull(exception);
			Assert.Equal("Runtime error", exception.Category);
			Assert.Equal("Method info not found in YantraJS.Core.Debug.JSConsole", exception.Description);
		}

		[Fact]
		public void SupportsConsoleWarnMethod()
		{
			// Arrange
			var sb = new StringBuilder();
			var logger = new StringLogger(sb);

			const string input = @"console.warn('Watch out, the doors are closing!');
console.warn('Watch yourself,', 'be careful!');
console.warn('It is forbidden to watch!');";
			string targetOutput = "warn: Watch out, the doors are closing!" + Environment.NewLine +
				"warn: Watch yourself, be careful!" + Environment.NewLine +
				"warn: It is forbidden to watch!" + Environment.NewLine
				;

			// Act
			using (var jsEngine = CreateJsEngine(consoleCallback: logger.Log))
			{
				jsEngine.Execute(input);
			}

			string output = sb.ToString();

			logger.Dispose();
			sb.Clear();

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public void SupportsConsoleErrorMethod()
		{
			// Arrange
			var sb = new StringBuilder();
			var logger = new StringLogger(sb);

			const string input = @"console.error('A terrible thing happened!');";
			string targetOutput = "error: A terrible thing happened!" + Environment.NewLine;

			// Act
			using (var jsEngine = CreateJsEngine(consoleCallback: logger.Log))
			{
				jsEngine.Execute(input);
			}

			string output = sb.ToString();

			logger.Dispose();
			sb.Clear();

			// Assert
			Assert.Equal(targetOutput, output);
		}

		private sealed class StringLogger : IDisposable
		{
			private StringBuilder _buffer;


			public StringLogger(StringBuilder buffer)
			{
				_buffer = buffer;
			}


			public void Log(string type, object[] args)
			{
				if (type != "log")
				{
					_buffer.AppendFormat("{0}: ", type);
				}

				for (int argIndex = 0; argIndex < args.Length; argIndex++)
				{
					if (argIndex > 0)
					{
						_buffer.Append(" ");
					}

					object arg = args[argIndex] ?? "null";
					var formattableArg = arg as IFormattable;

					if (formattableArg != null)
					{
						if (formattableArg is DateTime)
						{
							var dateTime = (DateTime)formattableArg;
							DateTime universalDateTime = dateTime.ToUniversalTime();

							_buffer.Append(universalDateTime.ToString("O", CultureInfo.InvariantCulture));
						}
						else
						{
							_buffer.Append(formattableArg.ToString("G", CultureInfo.InvariantCulture));
						}
					}
					else
					{
						_buffer.Append(arg.ToString());
					}
				}

				_buffer.AppendLine();
			}

			#region IDisposable implementation

			public void Dispose()
			{
				_buffer = null;
			}

			#endregion
		}
	}
}
#endif