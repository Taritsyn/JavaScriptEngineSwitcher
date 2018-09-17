using System;

using JavaScriptEngineSwitcher.Core.Utilities;

using JavaScriptEngineSwitcher.ChakraCore.Resources;

namespace JavaScriptEngineSwitcher.ChakraCore
{
	/// <summary>
	/// Settings of the ChakraCore JS engine
	/// </summary>
	public sealed class ChakraCoreSettings
	{
#if !NETSTANDARD1_3
		/// <summary>
		/// The stack size is sufficient to run the code of modern JavaScript libraries in 32-bit process
		/// </summary>
		const int STACK_SIZE_32 = 492 * 1024; // like 32-bit Node.js

		/// <summary>
		/// The stack size is sufficient to run the code of modern JavaScript libraries in 64-bit process
		/// </summary>
		const int STACK_SIZE_64 = 984 * 1024; // like 64-bit Node.js

		/// <summary>
		/// The maximum stack size in bytes
		/// </summary>
		private int _maxStackSize;

#endif
		/// <summary>
		/// Gets or sets a flag for whether to disable any background work (such as garbage collection)
		/// on background threads
		/// </summary>
		public bool DisableBackgroundWork
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable calls of <code>eval</code> function
		/// </summary>
		public bool DisableEval
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable executable page allocation
		/// </summary>
		/// <remarks>
		/// <para>
		/// This also implies that Native Code generation will be turned off.
		/// </para>
		/// <para>
		/// Note that this will break JavaScript stack decoding in tools like WPA since they
		/// rely on allocation of unique thunks to interpret each function and allocation of
		/// those thunks will be disabled as well.
		/// </para>
		/// </remarks>
		public bool DisableExecutablePageAllocation
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable Failfast fatal error on OOM
		/// </summary>
		public bool DisableFatalOnOOM
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable native code generation
		/// </summary>
		public bool DisableNativeCodeGeneration
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable all experimental features
		/// </summary>
		public bool EnableExperimentalFeatures
		{
			get;
			set;
		}
#if !NETSTANDARD1_3

		/// <summary>
		/// Gets or sets a maximum stack size in bytes
		/// </summary>
		/// <remarks>
		/// <para>Set a <code>0</code> to use the default maximum stack size specified in the header
		/// for the executable.
		/// </para>
		/// </remarks>
		public int MaxStackSize
		{
			get { return _maxStackSize; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(
						nameof(value),
						Strings.Engine_MaxStackSizeMustBeNonNegative
					);
				}

				_maxStackSize = value;
			}
		}
#endif

		/// <summary>
		/// Gets or sets a current memory limit for a runtime in bytes
		/// </summary>
		public UIntPtr MemoryLimit
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the ChakraCore settings
		/// </summary>
		public ChakraCoreSettings()
		{
			bool is64BitProcess = Utils.Is64BitProcess();

			DisableBackgroundWork = false;
			DisableEval = false;
			DisableExecutablePageAllocation = false;
			DisableFatalOnOOM = false;
			DisableNativeCodeGeneration = false;
			EnableExperimentalFeatures = false;
#if !NETSTANDARD1_3
			MaxStackSize = is64BitProcess ? STACK_SIZE_64 : STACK_SIZE_32;
#endif
			MemoryLimit = is64BitProcess ? new UIntPtr(ulong.MaxValue) : new UIntPtr(uint.MaxValue);
		}
	}
}