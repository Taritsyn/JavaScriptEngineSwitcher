using System;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Settings of the V8 JS engine
	/// </summary>
	public sealed class V8Settings
	{
		/// <summary>
		/// Gets or sets a flag for whether to the script engine is to wait for a debugger connection
		/// and schedule a pause before executing the first line of application script code.
		/// This property is ignored if value of the <see cref="EnableDebugging"/> property is false.
		/// </summary>
		public bool AwaitDebuggerAndPauseOnStart
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a TCP-based debugging)
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable remote script debugging.
		/// This property is ignored if value of the <see cref="EnableDebugging"/>
		/// property is false.
		/// </summary>
		public bool EnableRemoteDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a TCP port on which to listen for a debugger connection
		/// </summary>
		public ushort DebugPort
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to disable global members
		/// </summary>
		public bool DisableGlobalMembers
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a heap expansion multiplier
		/// </summary>
		/// <remarks>
		/// <para>
		/// When set to a value greater than 1, this property enables on-demand heap expansion,
		/// which automatically increases the maximum heap size by the specified multiplier
		/// whenever the script engine is close to exceeding the current limit. Note that a buggy
		/// or malicious script can still cause an application to fail by exhausting its address
		/// space or total available memory. On-demand heap expansion is recommended for use in
		/// conjunction with heap size monitoring (see <see cref="MaxHeapSize"/> property to help
		/// contain runaway scripts).
		/// </para>
		/// </remarks>
		public double HeapExpansionMultiplier
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a minimum time interval between consecutive heap size samples
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property is effective only when heap size monitoring is enabled (see
		/// <see cref="MaxHeapSize"/> property)
		/// </para>
		/// </remarks>
		public TimeSpan HeapSizeSampleInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum amount of <code>ArrayBuffer</code> memory the runtime may allocate.
		/// </summary>
		/// <remarks>
		/// This property is specified in bytes. <code>ArrayBuffer</code> memory is allocated outside
		/// the runtime's heap and released when its garbage collector reclaims the corresponding
		/// JavaScript <code>ArrayBuffer</code> object. Leave this property at its default value to
		/// enforce no limit.
		/// </remarks>
		public ulong MaxArrayBufferAllocation
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the executable code heap in mebibytes
		/// </summary>
		[Obsolete("Executable code now occupies the old object heap. Use a `MaxOldSpaceSize` property instead.")]
		public int MaxExecutableSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a soft limit for the size of the V8 runtime's heap in bytes
		/// </summary>
		/// <remarks>
		/// <para>
		/// When it is set to the default value, heap size monitoring is disabled, and
		/// scripts with memory leaks or excessive memory usage can cause unrecoverable
		/// errors and process termination.
		/// </para>
		/// <para>
		/// A V8 runtime unconditionally terminates the process when it exceeds its resource
		/// constraints. This property enables external heap size monitoring that can prevent
		/// termination in some scenarios. To be effective, it should be set to a value that
		/// is significantly lower than <see cref="MaxOldSpaceSize"/> property. Note that
		/// enabling heap size monitoring results in slower script execution.
		/// </para>
		/// <para>
		/// Exceeding this limit causes the V8 runtime to interrupt script execution and throw
		/// an exception.
		/// </para>
		/// <para>
		/// Note that <code>ArrayBuffer</code> memory is allocated outside the runtime's heap and is
		/// therefore not tracked by heap size monitoring. See <see cref="MaxArrayBufferAllocation"/>
		/// property for additional information.
		/// </para>
		/// </remarks>
		public UIntPtr MaxHeapSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the new object heap in mebibytes
		/// </summary>
		public int MaxNewSpaceSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum size of the old object heap in mebibytes
		/// </summary>
		public int MaxOldSpaceSize
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum amount by which the V8 runtime is permitted to grow
		/// the stack during script execution in bytes
		/// </summary>
		/// <remarks>
		/// <para>
		/// When it is set to the default value, no stack usage limit is enforced, and
		/// scripts with unchecked recursion or other excessive stack usage can cause
		/// unrecoverable errors and process termination.
		/// </para>
		/// <para>
		/// Note that the V8 runtime does not monitor stack usage while a host call is in progress.
		/// Monitoring is resumed when control returns to the runtime.
		/// </para>
		/// </remarks>
		public UIntPtr MaxStackUsage
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the V8 settings
		/// </summary>
		public V8Settings()
		{
			AwaitDebuggerAndPauseOnStart = false;
			EnableDebugging = false;
			EnableRemoteDebugging = false;
			DebugPort = 9222;
			DisableGlobalMembers = false;
			HeapExpansionMultiplier = 0;
			HeapSizeSampleInterval = TimeSpan.Zero;
			MaxArrayBufferAllocation = ulong.MaxValue;
			MaxHeapSize = UIntPtr.Zero;
			MaxNewSpaceSize = 0;
			MaxOldSpaceSize = 0;
			MaxStackUsage = UIntPtr.Zero;
		}
	}
}