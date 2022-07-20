using OriginalScript = NiL.JS.Script;

		protected override IPrecompiledScript InnerPrecompile(string code, string documentName)
		{
			OriginalScript script = OriginalScript.Parse(code);

			return new NiLPrecompiledScript(code, script);
		}

		protected override void InnerExecute(IPrecompiledScript precompiledScript)
		{
			var nilPrecompiledScript = precompiledScript as NiLPrecompiledScript;
			if (nilPrecompiledScript == null)
			{
				throw new WrapperUsageException(
					string.Format(CoreStrings.Usage_CannotConvertPrecompiledScriptToInternalType,
						typeof(NiLPrecompiledScript).FullName),
					Name, Version
				);
			}

			OriginalScript script = nilPrecompiledScript.Rent();

			try
			{
				lock (_synchronizer)
				{
					script.Evaluate(_jsContext);
				}
			}
			catch (OriginalException e)
			{
				throw WrapJsException(e);
			}
			finally
			{
				nilPrecompiledScript.Return(script);
			}
		}