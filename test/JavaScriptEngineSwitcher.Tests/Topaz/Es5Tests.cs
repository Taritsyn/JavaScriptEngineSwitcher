#if NET6_0_OR_GREATER
using System;

using Xunit;

using JavaScriptEngineSwitcher.Core;

namespace JavaScriptEngineSwitcher.Tests.Topaz
{
	public class Es5Tests : Es5TestsBase
	{
		protected override string EngineName
		{
			get { return "TopazJsEngine"; }
		}


		#region Array methods

		[Fact]
		public override void SupportsArrayEveryMethod()
		{ }

		[Fact]
		public override void SupportsArrayFilterMethod()
		{ }

		[Fact]
		public override void SupportsArrayIndexOfMethod()
		{ }

		[Fact]
		public override void SupportsArrayIsArrayMethod()
		{ }

		[Fact]
		public override void SupportsArrayLastIndexOfMethod()
		{ }

		[Fact]
		public override void SupportsArraySomeMethod()
		{ }

		#endregion

		#region Date methods

		[Fact]
		public override void SupportsDateNowMethod()
		{ }

		[Fact]
		public override void SupportsDateToIsoStringMethod()
		{ }

		#endregion

		#region Function methods

		[Fact]
		public override void SupportsFunctionBindMethod()
		{ }

		#endregion

		#region Object methods

		[Fact]
		public override void SupportsObjectCreateMethod()
		{ }

		[Fact]
		public override void SupportsObjectKeysMethod()
		{ }

		#endregion

		#region String methods

		[Fact]
		public override void SupportsStringSplitMethod()
		{ }

		[Fact]
		public override void SupportsStringTrimMethod()
		{ }

		#endregion
	}
}
#endif