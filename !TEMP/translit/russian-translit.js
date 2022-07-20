var transliterate = (function () {
	'use strict';

	/**
	* Сопоставления русских и латинских символов, сгруппированные по типам (системам) транслитерации
	*
	* @private {Object}
	*/
	var characterMappings = {
		// Основной
		'': ,

		// Буквы-цифры
		'': ,

		// ГОСТ 16876-71
		'': ,

		// ГОСТ 7.79-2000
		'': ,

		// СЭВ 1362-78
		'': ,

		// LC
		'': ,

		// BGN
		'': ,

		// BSI
		'': ,

		// Сходно с МВД
		'': ,

		// Как на загранпаспорт
		'': ;

	/**
	* Производит транслитерацию русского текста с кириллицы на латиницу
	*
	* @param {String} value - Текст, содержащий символы русского (кириллического) алфавита
	* @param {String} type - Код типа (системы) транслитерации
	* @returns {String} Текст, содержащий только символы латинского алфавита
	* @expose
	*/
	function transliterate(value, type) {
		var charCount,
			charIndex,
			charValue,
			newCharValue,
			characterMapping,
			result
			;

		value = value || '';
		type = type || 'basic';

		charCount = value.length;
		if (charCount === 0) {
			return value;
		}

		characterMapping = characterMappings[type];
		if (typeof characterMapping === 'undefined') {
			return value;
		}

		result = '';

		for (charIndex = 0; charIndex < charCount; charIndex++) {
			charValue = value.charAt(charIndex);
			newCharValue = typeof characterMapping[charValue] !== 'undefined' ?
				characterMapping[charValue] : charValue;
			result += newCharValue;
		}

		return result;
	}

	return transliterate;
}());