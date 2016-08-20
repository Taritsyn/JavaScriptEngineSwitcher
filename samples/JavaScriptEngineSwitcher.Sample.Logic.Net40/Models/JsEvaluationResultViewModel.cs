using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using JavaScriptEngineSwitcher.Sample.Resources;

namespace JavaScriptEngineSwitcher.Sample.Logic.Models
{
	/// <summary>
	/// JS evaluation result view model
	/// </summary>
	public sealed class JsEvaluationResultViewModel
	{
		/// <summary>
		/// Gets or sets a result value
		/// </summary>
		[Display(Name = "DisplayName_ResultValue", ResourceType = typeof(EvaluationStrings))]
		[DataType(DataType.MultilineText)]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a list of errors
		/// </summary>
		public IList<JsEvaluationErrorViewModel> Errors
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of JS evaluation result view model
		/// </summary>
		public JsEvaluationResultViewModel()
		{
			Value = string.Empty;
			Errors = new List<JsEvaluationErrorViewModel>();
		}
	}
}