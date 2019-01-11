using System;
using System.Collections;
using System.Collections.Generic;

using JavaScriptEngineSwitcher.ChakraCore.Resources;

namespace JavaScriptEngineSwitcher.ChakraCore.JsRt.Embedding
{
	/// <summary>
	/// Key for storage of embedded objects
	/// </summary>
	internal struct EmbeddedObjectKey : IEquatable<EmbeddedObjectKey>, IStructuralEquatable,
		IComparable, IComparable<EmbeddedObjectKey>, IStructuralComparable
	{
		/// <summary>
		/// Name of host type
		/// </summary>
		public readonly string HostTypeName;

		/// <summary>
		/// Instance of host type
		/// </summary>
		public readonly object HostObject;


		/// <summary>
		/// Constructs an instance of the key for storage of embedded objects
		/// </summary>
		/// <param name="hostObject">Instance of host type</param>
		public EmbeddedObjectKey(object hostObject)
		{
			HostTypeName = hostObject.GetType().AssemblyQualifiedName;
			HostObject = hostObject;
		}


		private static int CombineHashCodes(int h1, int h2)
		{
			return ((h1 << 5) + h1) ^ h2;
		}

		#region IEquatable<EmbeddedObjectKey> implementation

		public bool Equals(EmbeddedObjectKey other)
		{
			return EqualityComparer<string>.Default.Equals(HostTypeName, other.HostTypeName)
				&& EqualityComparer<object>.Default.Equals(HostObject, other.HostObject);
		}

		#endregion

		#region IStructuralEquatable implementation

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is EmbeddedObjectKey))
			{
				return false;
			}

			var embeddedObjectKey = (EmbeddedObjectKey)other;

			return comparer.Equals(HostTypeName, embeddedObjectKey.HostTypeName)
				&& comparer.Equals(HostObject, embeddedObjectKey.HostObject);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return CombineHashCodes(comparer.GetHashCode(HostTypeName), comparer.GetHashCode(HostObject));
		}

		#endregion

		#region IComparable implementation

		int IComparable.CompareTo(object other)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is EmbeddedObjectKey))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentHasIncorrectType, nameof(other), other.GetType().Name),
					nameof(other)
				);
			}

			return CompareTo((EmbeddedObjectKey)other);
		}

		#endregion

		#region IComparable<EmbeddedObjectKey> implementation

		public int CompareTo(EmbeddedObjectKey other)
		{
			int c = Comparer<string>.Default.Compare(HostTypeName, other.HostTypeName);
			if (c != 0)
			{
				return c;
			}

			return Comparer<object>.Default.Compare(HostObject, other.HostObject);
		}

		#endregion

		#region IStructuralComparable implementation

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is EmbeddedObjectKey))
			{
				throw new ArgumentException(
					string.Format(Strings.Common_ArgumentHasIncorrectType, nameof(other), other.GetType().Name),
					nameof(other)
				);
			}

			var embeddedObjectKey = (EmbeddedObjectKey)other;

			int c = comparer.Compare(HostTypeName, embeddedObjectKey.HostTypeName);
			if (c != 0)
			{
				return c;
			}

			return comparer.Compare(HostObject, embeddedObjectKey.HostObject);
		}

		#endregion

		#region Object overrides

		public override bool Equals(object obj)
		{
			return obj is EmbeddedObjectKey && Equals((EmbeddedObjectKey)obj);
		}

		public override int GetHashCode()
		{
			return CombineHashCodes(EqualityComparer<string>.Default.GetHashCode(HostTypeName),
				EqualityComparer<object>.Default.GetHashCode(HostObject));
		}

		public override string ToString()
		{
			return "(" + HostTypeName?.ToString() + ", " + HostObject?.ToString() + ")";
		}

		#endregion
	}
}