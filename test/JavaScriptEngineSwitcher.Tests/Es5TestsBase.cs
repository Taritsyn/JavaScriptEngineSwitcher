using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests
{
	public abstract class Es5TestsBase : TestsBase
	{
		#region Array methods

		[Fact]
		public virtual void ArrayEveryMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";

			const string input1 = "engines.every(function (value, index, array) { return value.length > 1; });";
			const bool targetOutput1 = true;

			const string input2 = "engines.every(function (value, index, array) { return value.length < 10; });";
			const bool targetOutput2 = false;

			// Act
			bool output1;
			bool output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<bool>(input1);
				output2 = jsEngine.Evaluate<bool>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void ArrayFilterMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";
			const string input = @"engines
	.filter(
		function (value, index, array) {
			return value.length > 5;
		})
	.toString();";
			const string targetOutput = "Chakra,SpiderMonkey,Jurassic";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void ArrayForEachMethodIsSupported()
		{
			// Arrange
			const string resultVariableName = "enginesString";
			string initCode = string.Format(@"var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'],
	{0} = ''
	;

engines.forEach(function(value, index, array) {{
	if (index > 0) {{
		{0} += ';';
	}}
	{0} += value;
}});", resultVariableName);
			const string targetOutput = "Chakra;V8;SpiderMonkey;Jurassic";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.GetVariableValue<string>(resultVariableName);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void ArrayIndexOfMethodIsSupported()
		{
			// Arrange
			const string initCode = "var arr = [2, 5, 9, 2]";

			const string input1 = "arr.indexOf(2);";
			const int targetOutput1 = 0;

			const string input2 = "arr.indexOf(7);";
			const int targetOutput2 = -1;

			const string input3 = "arr.indexOf(2, 3)";
			const int targetOutput3 = 3;

			const string input4 = "arr.indexOf(2, 2);";
			const int targetOutput4 = 3;

			const string input5 = "arr.indexOf(2, -2);";
			const int targetOutput5 = 3;

			const string input6 = "arr.indexOf(2, -1);";
			const int targetOutput6 = 3;

			const string input7 = "[].lastIndexOf(2, 0);";
			const int targetOutput7 = -1;

			// Act
			int output1;
			int output2;
			int output3;
			int output4;
			int output5;
			int output6;
			int output7;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
				output4 = jsEngine.Evaluate<int>(input4);
				output5 = jsEngine.Evaluate<int>(input5);
				output6 = jsEngine.Evaluate<int>(input6);
				output7 = jsEngine.Evaluate<int>(input7);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
			Assert.Equal(targetOutput7, output7);
		}

		[Fact]
		public virtual void ArrayIsArrayMethodIsSupported()
		{
			// Arrange
			const string input1 = "Array.isArray({ length: 0 });";
			const bool targetOutput1 = false;

			const string input2 = "Array.isArray([1, 2, 3, 4, 5]);";
			const bool targetOutput2 = true;

			// Act
			bool output1;
			bool output2;

			using (var jsEngine = CreateJsEngine())
			{
				output1 = jsEngine.Evaluate<bool>(input1);
				output2 = jsEngine.Evaluate<bool>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void ArrayLastIndexOfMethodIsSupported()
		{
			// Arrange
			const string initCode = "var arr = [2, 5, 9, 2]";

			const string input1 = "arr.lastIndexOf(2);";
			const int targetOutput1 = 3;

			const string input2 = "arr.lastIndexOf(7);";
			const int targetOutput2 = -1;

			const string input3 = "arr.lastIndexOf(2, 3)";
			const int targetOutput3 = 3;

			const string input4 = "arr.lastIndexOf(2, 2);";
			const int targetOutput4 = 0;

			const string input5 = "arr.lastIndexOf(2, -2);";
			const int targetOutput5 = 0;

			const string input6 = "arr.lastIndexOf(2, -1);";
			const int targetOutput6 = 3;

			const string input7 = "[].lastIndexOf(2, 0);";
			const int targetOutput7 = -1;

			// Act
			int output1;
			int output2;
			int output3;
			int output4;
			int output5;
			int output6;
			int output7;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
				output3 = jsEngine.Evaluate<int>(input3);
				output4 = jsEngine.Evaluate<int>(input4);
				output5 = jsEngine.Evaluate<int>(input5);
				output6 = jsEngine.Evaluate<int>(input6);
				output7 = jsEngine.Evaluate<int>(input7);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
			Assert.Equal(targetOutput7, output7);
		}

		[Fact]
		public virtual void ArrayMapMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";
			const string input = @"engines
	.map(
		function (value, index, array) {
			return value + ' JS Engine';
		})
	.toString();";
			const string targetOutput = "Chakra JS Engine,V8 JS Engine,SpiderMonkey JS Engine,Jurassic JS Engine";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void ArrayReduceMethodIsSupported()
		{
			// Arrange
			const string input1 = @"[1, 2, 3, 4, 5].reduce(function (accum, value, index, array) {
	return accum + value;
});";
			const int targetOutput1 = 15;

			const string input2 = @"[1, 2, 3, 4, 5].reduce(function (accum, value, index, array) {
	return accum + value;
}, 3);";
			const int targetOutput2 = 18;

			// Act
			int output1;
			int output2;

			using (var jsEngine = CreateJsEngine())
			{
				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void ArrayReduceRightMethodIsSupported()
		{
			// Arrange
			const string input1 = @"[1, 2, 3, 4, 5].reduceRight(function (accum, value, index, array) {
	return accum - value;
});";
			const int targetOutput1 = -5;

			const string input2 = @"[1, 2, 3, 4, 5].reduceRight(function (accum, value, index, array) {
	return accum - value;
}, 7);";
			const int targetOutput2 = -8;

			// Act
			int output1;
			int output2;

			using (var jsEngine = CreateJsEngine())
			{
				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		[Fact]
		public virtual void ArraySomeMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";

			const string input = "engines.some(function (value, index, array) { return value.length < 10; });";
			const bool targetOutput = true;

			// Act
			bool output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.Evaluate<bool>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Date methods

		[Fact]
		public virtual void DateNowMethodIsSupported()
		{
			// Arrange
			const string input = "Date.now();";
			DateTime targetOutput = DateTime.Now.ToUniversalTime();

			// Act
			DateTime output;

			using (var jsEngine = CreateJsEngine())
			{
				output = new DateTime(1970, 01, 01).AddMilliseconds(jsEngine.Evaluate<double>(input));
			}

			// Assert
			Assert.True(Math.Abs((targetOutput - output).TotalMilliseconds) < 1000);
		}

		[Fact]
		public virtual void DateToIsoStringMethodIsSupported()
		{
			// Arrange
			const string input = @"(new Date(1386696984000)).toISOString();";
			const string targetOutput = "2013-12-10T17:36:24.000Z";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Function methods

		[Fact]
		public virtual void FunctionBindIsSupported()
		{
			// Arrange
			const string initCode = @"var a = 5,
	module = {
		a: 12,
		getA: function() { return this.a; }
	},
	getA = module.getA
	;";

			const string input1 = "getA();";
			const int targetOutput1 = 5;

			const string input2 = "getA.bind(module)();";
			const int targetOutput2 = 12;

			// Act
			int output1;
			int output2;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);

				output1 = jsEngine.Evaluate<int>(input1);
				output2 = jsEngine.Evaluate<int>(input2);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
		}

		#endregion

		#region JSON methods

		[Fact]
		public virtual void JsonParseMethodIsSupported()
		{
			// Arrange
			const string initCode = "var obj = JSON.parse('{ \"foo\": \"bar\" }');";
			const string input = "obj.foo;";
			const string targetOutput = "bar";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		[Fact]
		public virtual void JsonStringifyMethodIsSupported()
		{
			// Arrange
			const string initCode = @"var obj = new Object();
obj['foo'] = 'bar';";
			const string input = "JSON.stringify(obj);";
			const string targetOutput = "{\"foo\":\"bar\"}";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode);
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion

		#region Object methods

		[Fact]
		public virtual void ObjectCreateMethodIsSupported()
		{
			// Arrange
			const string initCode1 = "var obj1 = Object.create(null);";
			const string input1 = "obj1.prototype";
			Undefined targetOutput1 = Undefined.Value;

			const string initCode2 = "var obj2 = Object.create(Object.prototype);";
			const string input2 = "typeof obj2;";
			const string targetOutput2 = "object";

			const string initCode3 = @"var greeter = {
	id: 678,
	name: 'stranger',
	greet: function() {
		return 'Hello, ' + this.name + '!';
	}
};

var myGreeter = Object.create(greeter);
greeter.name = 'Vasya'";
			const string input3A = "myGreeter.id";
			const string input3B = "myGreeter.greet()";

			const int targetOutput3A = 678;
			const string targetOutput3B = "Hello, Vasya!";

			// Act
			object output1;
			object output2;
			int output3A;
			string output3B;

			using (var jsEngine = CreateJsEngine())
			{
				jsEngine.Execute(initCode1);
				output1 = jsEngine.Evaluate(input1);

				jsEngine.Execute(initCode2);
				output2 = jsEngine.Evaluate(input2);

				jsEngine.Execute(initCode3);
				output3A = jsEngine.Evaluate<int>(input3A);
				output3B = jsEngine.Evaluate<string>(input3B);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);

			Assert.Equal(targetOutput3A, output3A);
			Assert.Equal(targetOutput3B, output3B);
		}

		[Fact]
		public virtual void ObjectKeysMethodIsSupported()
		{
			// Arrange
			const string input1 = "Object.keys(['a', 'b', 'c']).toString();";
			const string targetOutput1 = "0,1,2";

			const string input2 = "Object.keys({ 0: 'a', 1: 'b', 2: 'c' }).toString();";
			const string targetOutput2 = "0,1,2";

			const string input3 = "Object.keys({ 100: 'a', 2: 'b', 7: 'c' }).toString();";
			const string targetOutput3 = "2,7,100";

			const string initCode4 = @"var myObj = function() { };
myObj.prototype = { getFoo: { value: function () { return this.foo } } };;
myObj.foo = 1;
";
			const string input4 = "Object.keys(myObj).toString();";
			const string targetOutput4 = "foo";

			// Act
			string output1;
			string output2;
			string output3;
			string output4;

			using (var jsEngine = CreateJsEngine())
			{
				output1 = jsEngine.Evaluate<string>(input1);
				output2 = jsEngine.Evaluate<string>(input2);
				output3 = jsEngine.Evaluate<string>(input3);

				jsEngine.Execute(initCode4);
				output4 = jsEngine.Evaluate<string>(input4);
			}

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
		}

		#endregion

		#region String methods

		[Fact]
		public virtual void StringSplitMethodIsCorrect()
		{
			// Arrange
			const string input1 = "'aaaa'.split(/a/).length;";
			const int targetOutput1 = 5;

			const string input2 = @"'|a|b|c|'.split(/\|/).length";
			const int targetOutput2 = 5;

			const string input3 = @"'1, 2, 3, 4'.split(/\s*(,)\s*/).length";
			const int targetOutput3 = 7;

			// Act
			int output1;
			int output2;
			int output3;

			using (var jsEngine = CreateJsEngine())
			{
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
		public virtual void StringTrimMethodIsSupported()
		{
			// Arrange
			const string input = "'	foo '.trim();";
			const string targetOutput = "foo";

			// Act
			string output;

			using (var jsEngine = CreateJsEngine())
			{
				output = jsEngine.Evaluate<string>(input);
			}

			// Assert
			Assert.Equal(targetOutput, output);
		}

		#endregion
	}
}