namespace JavaScriptEngineSwitcher.Tests.Interop
{
	public sealed class Person
	{
		public string FirstName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}


		public Person()
			: this(string.Empty, string.Empty)
		{ }

		public Person(string firstName, string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
		}


		public override string ToString()
		{
			return string.Format("{{FirstName={0},LastName={1}}}", FirstName, LastName);
		}
	}
}