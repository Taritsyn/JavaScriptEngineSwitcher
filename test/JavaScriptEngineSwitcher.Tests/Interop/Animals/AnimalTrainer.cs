namespace JavaScriptEngineSwitcher.Tests.Interop.Animals
{
	public sealed class AnimalTrainer
	{
		public string ExecuteVoiceCommand(IAnimal animal)
		{
			return animal.Cry();
		}
	}
}