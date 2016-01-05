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
	}
}