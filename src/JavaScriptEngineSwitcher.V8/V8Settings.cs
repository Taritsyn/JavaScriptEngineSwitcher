using System;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Settings of the V8 JS engine
	/// </summary>
	public sealed class V8Settings
	{
		/// <summary>
		/// Gets or sets a flag for whether to add the
		/// <c><see href="https://microsoft.github.io/ClearScript/2024/03/21/performance-api.html">Performance</see></c>
		/// object to the script engine's global namespace
		/// </summary>
		/// <remarks>
		/// This object provides a set of low-level native facilities for performance-sensitive scripts.
		/// </remarks>
		public bool AddPerformanceObject
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to allow the usage of reflection API in the script code
		/// </summary>
		/// <remarks>
		/// This affects <see cref="Object.GetType"/>, <see cref="Exception.GetType"/>,
		/// <see cref="Exception.TargetSite"/> and <see cref="Delegate.Method"/>.
		/// By default, any attempt to access these members from the script code will throw an exception.
		/// </remarks>
		public bool AllowReflection
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to the script engine is to wait for a debugger connection
		/// and schedule a pause before executing the first line of application script code
		/// </summary>
		/// <remarks>
		/// This property is ignored if value of the <see cref="EnableDebugging"/> property is <c>false</c>.
		/// </remarks>
		public bool AwaitDebuggerAndPauseOnStart
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
		/// Gets or sets a flag for whether to disable dynamic method binding
		/// </summary>
		/// <remarks>
		/// When this property is set to <c>true</c>, the script engine bypasses the default method
		/// binding algorithm and uses reflection-based method binding instead. This approach
		/// abandons support for generic type inference and other features, but it avoids engaging
		/// the dynamic infrastructure.
		/// </remarks>
		public bool DisableDynamicBinding
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
		/// Gets or sets a flag for whether to enable script debugging features
		/// (allows a TCP-based debugging)
		/// </summary>
		public bool EnableDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a flag for whether to enable remote script debugging
		/// </summary>
		/// <remarks>
		/// This property is ignored if value of the <see cref="EnableDebugging"/>
		/// property is <c>false</c>.
		/// </remarks>
		public bool EnableRemoteDebugging
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a heap expansion multiplier
		/// </summary>
		/// <remarks>
		/// When set to a value greater than <c>1</c>, this property enables on-demand heap expansion,
		/// which automatically increases the maximum heap size by the specified multiplier
		/// whenever the script engine is close to exceeding the current limit. Note that a buggy
		/// or malicious script can still cause an application to fail by exhausting its address
		/// space or total available memory. On-demand heap expansion is recommended for use in
		/// conjunction with heap size monitoring (see <see cref="MaxHeapSize"/> property to help
		/// contain runaway scripts).
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
		/// This property is effective only when heap size monitoring is enabled (see
		/// <see cref="MaxHeapSize"/> property)
		/// </remarks>
		public TimeSpan HeapSizeSampleInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a maximum amount of <c>ArrayBuffer</c> memory the runtime may allocate
		/// </summary>
		/// <remarks>
		/// This property is specified in bytes. <c>ArrayBuffer</c> memory is allocated outside
		/// the runtime's heap and released when its garbage collector reclaims the corresponding
		/// JavaScript <c>ArrayBuffer</c> object. Leave this property at its default value to
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
		/// Note that <c>ArrayBuffer</c> memory is allocated outside the runtime's heap and is
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
		/// Gets or sets a flag for whether to set native timers to the highest available resolution
		/// while the current script engine's instance is active
		/// </summary>
		/// <remarks>
		/// This property is ignored if value of the <c><see cref="AddPerformanceObject"/></c> property
		/// is <c>false</c>. It is only a hint and may be ignored on some systems. On platforms that
		/// support it, this property can degrade overall system performance or power efficiency, so
		/// caution is recommended.
		/// </remarks>
		public bool SetTimerResolution
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of the V8 settings
		/// </summary>
		public V8Settings()
		{
			AddPerformanceObject = false;
			AllowReflection = false;
			AwaitDebuggerAndPauseOnStart = false;
			DebugPort = 9222;
			DisableDynamicBinding = false;
			DisableGlobalMembers = false;
			EnableDebugging = false;
			EnableRemoteDebugging = false;
			HeapExpansionMultiplier = 0;
			HeapSizeSampleInterval = TimeSpan.Zero;
			MaxArrayBufferAllocation = ulong.MaxValue;
			MaxHeapSize = UIntPtr.Zero;
			MaxNewSpaceSize = 0;
			MaxOldSpaceSize = 0;
			MaxStackUsage = UIntPtr.Zero;
			SetTimerResolution = false;
		}
	}
}