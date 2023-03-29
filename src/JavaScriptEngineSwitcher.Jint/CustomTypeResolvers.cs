using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using OriginalTypeResolver = Jint.Runtime.Interop.TypeResolver;

namespace JavaScriptEngineSwitcher.Jint
{
	/// <summary>
	/// Interop strategies for different values of the <see cref="JintSettings.AllowReflection"/> configuration property
	/// </summary>
	internal static class CustomTypeResolvers
	{
		private static readonly PropertyInfo[] _disallowedProperties =
		{
			typeof(Delegate).GetProperty("Method"),
			typeof(Exception).GetProperty("TargetSite")
		};

		private static readonly MethodInfo[] _disallowedMethods =
		{
			typeof(object).GetMethod("GetType"),
			typeof(Exception).GetMethod("GetType")
		};

		private static readonly Lazy<OriginalTypeResolver> _allowingReflection = new Lazy<OriginalTypeResolver>(
			() => new OriginalTypeResolver() { MemberFilter = _ => true });

		private static readonly Lazy<OriginalTypeResolver> _disallowingReflection = new Lazy<OriginalTypeResolver>(
			() => new OriginalTypeResolver() { MemberFilter = IsAllowedMember });

		/// <summary>
		/// Gets a interop strategy that allows the usage of reflection API in the script code
		/// </summary>
		public static OriginalTypeResolver AllowingReflection => _allowingReflection.Value;

		/// <summary>
		/// Gets a interop strategy that disallows the usage of reflection API in the script code
		/// </summary>
		public static OriginalTypeResolver DisallowingReflection => _disallowingReflection.Value;


		private static bool IsAllowedMember(MemberInfo member)
		{
			bool isAllowed = true;

			if (member is PropertyInfo)
			{
				isAllowed = IsAllowedProperty((PropertyInfo)member);
			}
			else if (member is MethodInfo)
			{
				isAllowed = IsAllowedMethod((MethodInfo)member);
			}

			return isAllowed;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private static bool IsAllowedProperty(PropertyInfo property)
		{
			bool isAllowed = !_disallowedProperties.Contains(property, MemberComparer<PropertyInfo>.Instance);

			return isAllowed;
		}

		[MethodImpl((MethodImplOptions)256 /* AggressiveInlining */)]
		private static bool IsAllowedMethod(MethodInfo method)
		{
			bool isAllowed = !_disallowedMethods.Contains(method, MemberComparer<MethodInfo>.Instance);

			return isAllowed;
		}


		private sealed class MemberComparer<T> : EqualityComparer<T>
			where T : MemberInfo
		{
			public static MemberComparer<T> Instance { get; } = new MemberComparer<T>();


			private MemberComparer()
			{ }


			#region MemberComparer overrides

			public override bool Equals(T x, T y)
			{
				return x.Module == y.Module && x.MetadataToken == y.MetadataToken;
			}

			public override int GetHashCode(T obj)
			{
				return obj != null ? obj.GetHashCode() : 0;
			}

			#endregion
		}
	}
}
