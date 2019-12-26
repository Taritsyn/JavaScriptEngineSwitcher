using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#if NET451 || NET471 || NETSTANDARD || NETCOREAPP
using Microsoft.AspNetCore.Mvc.Rendering;
#elif NET40
using System.Web.Mvc;
#else
#error No implementation for this target
#endif

using JavaScriptEngineSwitcher.Sample.Resources;

namespace JavaScriptEngineSwitcher.Sample.Logic.Models
{
	/// <summary>
	/// JS evaluation view model
	/// </summary>
	public sealed class JsEvaluationViewModel
	{
		/// <summary>
		/// Gets or sets a name of JS engine
		/// </summary>
		[Display(Name = "DisplayName_EngineName", ResourceType = typeof(EvaluationStrings))]
		public string EngineName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a list of available JS engines
		/// </summary>
		public IEnumerable<SelectListItem> AvailableEngineList
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a expression
		/// </summary>
		[Display(Name = "DisplayName_Expression", ResourceType = typeof(EvaluationStrings))]
		[DataType(DataType.MultilineText)]
		[Required(ErrorMessageResourceName = "ErrorMessage_FormFieldIsNotFilled", ErrorMessageResourceType = typeof(EvaluationStrings))]
		[StringLength(1000000, ErrorMessageResourceName = "ErrorMessage_FormFieldValueTooLong", ErrorMessageResourceType = typeof(EvaluationStrings))]
		public string Expression
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a result of evaluation
		/// </summary>
		public JsEvaluationResultViewModel Result
		{
			get;
			set;
		}


		/// <summary>
		/// Constructs an instance of JS evaluation view model
		/// </summary>
		public JsEvaluationViewModel()
		{
			EngineName = string.Empty;
			AvailableEngineList = new List<SelectListItem>();
			Expression = string.Empty;
			Result = null;
		}
	}
}