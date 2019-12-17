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

		public string Patronymic
		{
			get;
			set;
		}


		public Person()
			: this(string.Empty, string.Empty)
		{ }

		public Person(string firstName, string lastName)
			: this(firstName, lastName, string.Empty)
		{ }

		public Person(string firstName, string lastName, string patronymic)
		{
			FirstName = firstName;
			LastName = lastName;
			Patronymic = patronymic;
		}


		public override string ToString()
		{
			return string.Format("{{FirstName={0},LastName={1}}}", FirstName, LastName);
		}
	}
}