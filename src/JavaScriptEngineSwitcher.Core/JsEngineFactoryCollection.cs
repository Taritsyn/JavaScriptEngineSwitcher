using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JavaScriptEngineSwitcher.Core
{
	/// <summary>
	/// Collection of JS engine factories
	/// </summary>
	public sealed class JsEngineFactoryCollection : IEnumerable<IJsEngineFactory>
	{
		/// <summary>
		/// Dictionary of factories
		/// </summary>
		private readonly Dictionary<string, IJsEngineFactory> _factories =
			new Dictionary<string, IJsEngineFactory>();

		/// <summary>
		/// Gets a number of factories in the collection
		/// </summary>
		public int Count
		{
			get { return _factories.Count; }
		}


		/// <summary>
		/// Gets all registered factories
		/// </summary>
		/// <returns>A read-only collection of all factories in the collection</returns>
		public ReadOnlyCollection<IJsEngineFactory> GetRegisteredFactories()
		{
			return new List<IJsEngineFactory>(_factories.Values).AsReadOnly();
		}

		/// <summary>
		/// Gets a factory by JS engine name
		/// </summary>
		/// <param name="engineName">Name of JS engine</param>
		/// <returns>Instance of corresponding JS engine factory or <c>null</c> if factory is not found</returns>
		public IJsEngineFactory Get(string engineName)
		{
			if (_factories.ContainsKey(engineName))
			{
				return _factories[engineName];
			}

			return null;
		}

		/// <summary>
		/// Adds a factory to the collection
		/// </summary>
		/// <param name="factory">The factory to add to the collection</param>
		public void Add(IJsEngineFactory factory)
		{
			if (factory is null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			string engineName = factory.EngineName;
			if (_factories.ContainsKey(engineName))
			{
				_factories[engineName] = factory;
			}
			else
			{
				_factories.Add(engineName, factory);
			}
		}

		/// <summary>
		/// Removes a single factory from the collection
		/// </summary>
		/// <param name="engineName">Name of JS engine</param>
		/// <returns>A boolean value indicating whether the factory was succesfully removed from the collection</returns>
		public bool Remove(string engineName)
		{
			if (engineName is null)
			{
				throw new ArgumentNullException(nameof(engineName));
			}

			return _factories.Remove(engineName);
		}

		/// <summary>
		/// Removes a single factory from the collection
		/// </summary>
		/// <param name="factory">The factory to remove from the collection</param>
		/// <returns>A boolean value indicating whether the factory was succesfully removed from the collection</returns>
		public bool Remove(IJsEngineFactory factory)
		{
			if (factory is null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			return _factories.Remove(factory.EngineName);
		}

		/// <summary>
		/// Removes all factories from the collection
		/// </summary>
		public void Clear()
		{
			_factories.Clear();
		}

		#region IEnumerable implementation

		public IEnumerator<IJsEngineFactory> GetEnumerator()
		{
			return _factories.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _factories.Values.GetEnumerator();
		}

		#endregion
	}
}