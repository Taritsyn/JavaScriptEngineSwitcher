(function () {
	'use strict';

	function sum() {
		var result = 0,
			i
			;

		for (i = 0; i < arguments.length; i++) {
			result += arguments[i];
		}

		return result;
	}

	function square(num) {
		return num * num;
	}

	function cube(num) {
		return num * num * num;
	}

	var exports = {
		PI: 3.14,
		sum: sum,
		square: square,
		cube: cube
	};

	return exports;
}());