namespace JavaScriptEngineSwitcher.Tests
{
	using System;

	using NUnit.Framework;

	using Core;

	public abstract class Es5TestsBase
	{
		protected IJsEngine _jsEngine;

		[TestFixtureSetUp]
		public abstract void SetUp();

		#region Function methods
		[Test]
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
			_jsEngine.Execute(initCode);

			var output1 = _jsEngine.Evaluate<int>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}
		#endregion

		#region String methods
		[Test]
		public virtual void StringTrimMethodIsSupported()
		{
			// Arrange
			const string input = "'	foo '.trim();";
			const string targetOutput = "foo";

			// Act
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}
		#endregion

		#region Array methods
		[Test]
		public virtual void ArrayEveryMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";

			const string input1 = "engines.every(function (value, index, array) { return value.length > 1; });";
			const bool targetOutput1 = true;

			const string input2 = "engines.every(function (value, index, array) { return value.length < 10; });";
			const bool targetOutput2 = false;

			// Act
			_jsEngine.Execute(initCode);

			var output1 = _jsEngine.Evaluate<bool>(input1);
			var output2 = _jsEngine.Evaluate<bool>(input2);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}

		[Test]
		public virtual void ArraySomeMethodIsSupported()
		{
			// Arrange
			const string initCode = "var engines = ['Chakra', 'V8', 'SpiderMonkey', 'Jurassic'];";

			const string input = "engines.some(function (value, index, array) { return value.length < 10; });";
			const bool targetOutput = true;

			// Act
			_jsEngine.Execute(initCode);
			var output = _jsEngine.Evaluate<bool>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
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
			_jsEngine.Execute(initCode);
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
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
			_jsEngine.Execute(initCode);
			var output = _jsEngine.GetVariableValue<string>(resultVariableName);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
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
			_jsEngine.Execute(initCode);

			var output1 = _jsEngine.Evaluate<int>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);
			var output3 = _jsEngine.Evaluate<int>(input3);
			var output4 = _jsEngine.Evaluate<int>(input4);
			var output5 = _jsEngine.Evaluate<int>(input5);
			var output6 = _jsEngine.Evaluate<int>(input6);
			var output7 = _jsEngine.Evaluate<int>(input7);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
			Assert.AreEqual(targetOutput4, output4);
			Assert.AreEqual(targetOutput5, output5);
			Assert.AreEqual(targetOutput6, output6);
			Assert.AreEqual(targetOutput7, output7);
		}

		[Test]
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
			_jsEngine.Execute(initCode);

			var output1 = _jsEngine.Evaluate<int>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);
			var output3 = _jsEngine.Evaluate<int>(input3);
			var output4 = _jsEngine.Evaluate<int>(input4);
			var output5 = _jsEngine.Evaluate<int>(input5);
			var output6 = _jsEngine.Evaluate<int>(input6);
			var output7 = _jsEngine.Evaluate<int>(input7);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
			Assert.AreEqual(targetOutput4, output4);
			Assert.AreEqual(targetOutput5, output5);
			Assert.AreEqual(targetOutput6, output6);
			Assert.AreEqual(targetOutput7, output7);
		}

		[Test]
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
			_jsEngine.Execute(initCode);
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
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
			var output1 = _jsEngine.Evaluate<int>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}

		[Test]
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
			var output1 = _jsEngine.Evaluate<int>(input1);
			var output2 = _jsEngine.Evaluate<int>(input2);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}

		[Test]
		public virtual void ArrayIsArrayMethodIsSupported()
		{
			// Arrange
			const string input1 = "Array.isArray({ length: 0 });";
			const bool targetOutput1 = false;

			const string input2 = "Array.isArray([1, 2, 3, 4, 5]);";
			const bool targetOutput2 = true;

			// Act
			var output1 = _jsEngine.Evaluate<bool>(input1);
			var output2 = _jsEngine.Evaluate<bool>(input2);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
		}
		#endregion

		#region Date methods
		[Test]
		public virtual void DateNowMethodIsSupported()
		{
			// Arrange
			const string input = "Date.now();";
			DateTime targetOutput = DateTime.Now.ToUniversalTime();

			// Act
			var output = new DateTime(1970, 01, 01).AddMilliseconds(_jsEngine.Evaluate<double>(input));

			// Assert
			Assert.IsTrue(Math.Abs((targetOutput - output).TotalMilliseconds) < 100);
		}

		[Test]
		public virtual void DateToIsoStringMethodIsSupported()
		{
			// Arrange
			const string input = "(new Date(2013, 11, 10, 21, 36, 24)).toISOString();";
			const string targetOutput = "2013-12-10T17:36:24.000Z";

			// Act
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}
		#endregion

		#region Object methods
		[Test]
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
			var output1 = _jsEngine.Evaluate<string>(input1);
			var output2 = _jsEngine.Evaluate<string>(input2);
			var output3 = _jsEngine.Evaluate<string>(input3);

			_jsEngine.Execute(initCode4);
			var output4 = _jsEngine.Evaluate<string>(input4);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);
			Assert.AreEqual(targetOutput3, output3);
			Assert.AreEqual(targetOutput4, output4);
		}

		[Test]
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
			_jsEngine.Execute(initCode1);
			var output1 = _jsEngine.Evaluate(input1);

			_jsEngine.Execute(initCode2);
			var output2 = _jsEngine.Evaluate(input2);

			_jsEngine.Execute(initCode3);
			var output3A = _jsEngine.Evaluate<int>(input3A);
			var output3B = _jsEngine.Evaluate<string>(input3B);

			// Assert
			Assert.AreEqual(targetOutput1, output1);
			Assert.AreEqual(targetOutput2, output2);

			Assert.AreEqual(targetOutput3A, output3A);
			Assert.AreEqual(targetOutput3B, output3B);
		}
		#endregion

		#region JSON methods
		[Test]
		public virtual void JsonParseMethodIsSupported()
		{
			// Arrange
			const string initCode = "var obj = JSON.parse('{ \"foo\": \"bar\" }');";
			const string input = "obj.foo;";
			const string targetOutput = "bar";

			// Act
			_jsEngine.Execute(initCode);
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}

		[Test]
		public virtual void JsonStringifyMethodIsSupported()
		{
			// Arrange
			const string initCode = @"var obj = new Object();
obj['foo'] = 'bar';";
			const string input = "JSON.stringify(obj);";
			const string targetOutput = "{\"foo\":\"bar\"}";

			// Act
			_jsEngine.Execute(initCode);
			var output = _jsEngine.Evaluate<string>(input);

			// Assert
			Assert.AreEqual(targetOutput, output);
		}
		#endregion

		[TestFixtureTearDown]
		public virtual void TearDown()
		{
			if (_jsEngine != null)
			{
				_jsEngine.Dispose();
				_jsEngine = null;
			}
		}
	}
}