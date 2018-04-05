#if NET40
using System;
using System.Reflection;

namespace JavaScriptEngineSwitcher.Core.Polyfills.System.Reflection
{
	/// <summary>
	/// Represents type declarations for class types, interface types, array types, value types,
	/// enumeration types, type parameters, generic type definitions, and open
	/// or closed constructed generic types.
	/// </summary>
	public class TypeInfo
	{
		/// <summary>
		/// Target type
		/// </summary>
		private readonly Type _type;

		/// <summary>
		/// Gets a <see cref="Assembly"/> in which the type is declared.
		/// For generic types, gets the <see cref="Assembly"/> in which the generic type is defined.
		/// </summary>
		public Assembly Assembly
		{
			get { return _type.Assembly; }
		}

		/// <summary>
		/// Gets a value indicating whether the current type is a generic type
		/// </summary>
		public bool IsGenericType
		{
			get { return _type.IsGenericType; }
		}

		/// <summary>
		/// Gets a value indicating whether the current type is a value type
		/// </summary>
		public bool IsValueType
		{
			get { return _type.IsValueType; }
		}


		/// <summary>
		/// Constructs an instance of the <see cref="TypeInfo"/> representation of the specified type
		/// </summary>
		/// <param name="type">The type</param>
		public TypeInfo(Type type)
		{
			_type = type;
		}


		/// <summary>
		/// Determines whether the specified object is an instance of the current type
		/// </summary>
		/// <param name="o">The object to compare with the current type</param>
		/// <returns>true if the current type is in the inheritance hierarchy of the object represented
		/// by <code>o</code>, or if the current type is an interface that <code>o</code> supports.
		/// false if neither of these conditions is the case, or if <code>o</code> is null, or if
		/// the current type is an open generic type
		/// (that is, <code>System.Type.ContainsGenericParameters</code> returns true).</returns>
		public bool IsInstanceOfType(object o)
		{
			return _type.IsInstanceOfType(o);
		}
	}
}
#endif