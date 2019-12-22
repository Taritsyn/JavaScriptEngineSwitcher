(function (jsEngineSwitcher, $, undefined) {
	"use strict";

	var $evaluationForm,
		$evaluationInputField,
		$evaluationInputClearButton,
		$evaluateButton
		;

	$(function () {
		$evaluationForm = $("form[data-form-type='evaluation-form']");
		$evaluationInputField = $(":input[data-control-type='evaluation-input-field']", $evaluationForm);
		$evaluationInputClearButton = $("<div class=\"evaluation-input-clear-button\" title=\"Clear text\"></div>");
		$evaluateButton = $(":input[data-control-type='evaluate-button']", $evaluationForm);

		$evaluationForm.on("submit", onEvaluationFormSubmitHandler);

		$evaluationInputClearButton.on("click", onEvaluationInputClearButtonClickHandler);
		$evaluationInputField.parent().append($evaluationInputClearButton);
		refreshEvaluationInputClearButton();
		$evaluationInputField
			.on("input propertychange keydown keyup paste", onEvaluationInputFieldChangeHandler)
			;

		$evaluateButton.removeAttr("disabled");
	});

	$(window).unload(function() {
		$evaluationForm.off("submit", onEvaluationFormSubmitHandler);

		$evaluationInputClearButton
			.off("click", onEvaluationInputClearButtonClickHandler)
			.remove()
			;

		$evaluationInputField
			.off("input propertychange keydown keyup paste", onEvaluationInputFieldChangeHandler)
			;

		$evaluationForm = null;
		$evaluationInputField = null;
		$evaluationInputClearButton = null;
		$evaluateButton = null;
	});

	var refreshEvaluationInputClearButton = function() {
		if ($.trim($evaluationInputField.val()).length > 0) {
			$evaluationInputClearButton.show();
		} else {
			$evaluationInputClearButton.hide();
		}

		if (jsEngineSwitcher.hasScrollbar($evaluationInputField.get(0))) {
			$evaluationInputClearButton.addClass("with-scrollbar");
		}
		else {
			$evaluationInputClearButton.removeClass("with-scrollbar");
		}
	};

	var onEvaluationFormSubmitHandler = function () {
		var $form = $(this);
		if ($form.valid()) {
			$evaluateButton.attr("disabled", "disabled");
			$("textarea[data-control-type='evaluation-output-field']", $form).val('');

			return true;
		}

		return false;
	};

	var onEvaluationInputFieldChangeHandler = function () {
		refreshEvaluationInputClearButton();
	};

	var onEvaluationInputClearButtonClickHandler = function() {
		$evaluationInputField.val("");

		var $button = $(this);
		$button.hide();
	};
}(jsEngineSwitcher, jQuery));