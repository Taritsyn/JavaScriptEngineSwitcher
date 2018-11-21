using Xunit;

using JavaScriptEngineSwitcher.Core.Helpers;

namespace JavaScriptEngineSwitcher.Tests
{
	public class ErrorFormattingTests
	{
		[Fact]
		public void GettingSourceFragmentFromLineIsCorrect()
		{
			// Arrange
			const string input1 = "";
			const string targetOutput1 = input1;

			const string input2 = "	    \n";
			const string targetOutput2 = input2;

			const string input3 = "var @variable3 = 678;";
			const string targetOutput3 = input3;

			const string input4 = "	Math.hasOwnProperty(\"log2\")||(Math.log2=function(n){" +
				"return Math.log(@n)*Math.LOG2E});";
			const string targetOutput4 = "…Math.hasOwnProperty(\"log2\")||(Math.log2=function(n){" +
				"return Math.log(@n)*Math.LOG2E});";

			const string input5 = "function mix(destination,source){var propertyName;destination=destination||{};" +
				"for(propertyName in source){if(source.hasOwnProperty(propertyName){" +
				"destination[propertyName]=source[propertyName]}}return destination}"
				;
			const string targetOutput5 = "… in source){if(source.hasOwnProperty(propertyName){" +
				"destination[propertyName]=source[propertyName]}}r…";

			const string input6 = "Object.hasOwnProperty(\"assign)||(Object.assign=function(n){" +
				"var u,i,f,t,r;if(typeof n==\"undefined\"||n===null)" +
				"throw new TypeError(\"Object.assign: argument is not an Object.\");" +
				"for(u=Object(n),f=arguments.length,i=1;i<f;i++)" +
				"if(t=arguments[i],typeof t!=\"undefined\"&&t!==null)for(r in t)" +
				"Object.prototype.hasOwnProperty.call(t,r)&&(u[r]=t[r]);return u});"
				;
			const string targetOutput6 = "Object.hasOwnProperty(\"assign)||(Object.assign=function(n){" +
				"var u,i,f,t,r;if(typeof n==\"undefined\"||n…";

			const string input7 = "function base64_encode(a){" +
				"var b=\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=\";" +
				"var c,o2,o3,h1,h2,h3,h4,bits,i=0,enc='';do{c=a.charCodeAt(i++);o2=a.charCodeAt(i++);" +
				"o3=a.charCodeAt(i++);bits=c<<16|o2<<8|o3;h1=bits>>18&0x3f;h2=bits>>12&0x3f;" +
				"h3=bits>>6&0x3f;h4=bits&0x3f;enc+=b.charAt(h1)+b.charAt(h2)+b.charAt(h3)+b.charAt(h4)}" +
				"while(i<a.length);switch(a.length%3){case 1:enc=enc.slice(0,-2)+'==';break;" +
				"case 2:enc=enc.slice(0,-1)+'=';break}return @enc}"
				;
			const string targetOutput7 = "…(a.length%3){case 1:enc=enc.slice(0,-2)+'==';break;" +
				"case 2:enc=enc.slice(0,-1)+'=';break}return @enc}";

			// Act
			string output1 = TextHelpers.GetTextFragmentFromLine(input1, 1, 100);
			string output2 = TextHelpers.GetTextFragmentFromLine(input2, 1, 100);
			string output3 = TextHelpers.GetTextFragmentFromLine(input3, 5, 100);
			string output4 = TextHelpers.GetTextFragmentFromLine(input4, 70, 85);
			string output5 = TextHelpers.GetTextFragmentFromLine(input5, 145, 100);
			string output6 = TextHelpers.GetTextFragmentFromLine(input6, 23, 100);
			string output7 = TextHelpers.GetTextFragmentFromLine(input7, 465, 100);

			// Assert
			Assert.Equal(targetOutput1, output1);
			Assert.Equal(targetOutput2, output2);
			Assert.Equal(targetOutput3, output3);
			Assert.Equal(targetOutput4, output4);
			Assert.Equal(targetOutput5, output5);
			Assert.Equal(targetOutput6, output6);
			Assert.Equal(targetOutput7, output7);
		}
	}
}