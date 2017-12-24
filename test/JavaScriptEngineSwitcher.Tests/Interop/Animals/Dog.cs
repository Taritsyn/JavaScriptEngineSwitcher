namespace JavaScriptEngineSwitcher.Tests.Interop.Animals
{
	public sealed class Dog : IAnimal
	{
		public string Cry()
		{
			return "Woof!";
		}
	}
}