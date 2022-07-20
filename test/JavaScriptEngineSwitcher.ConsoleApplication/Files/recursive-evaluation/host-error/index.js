/*global evaluateFile */
(function () {
	'use strict';

	function calculateResult() {
		var math = evaluateFile('./match'),
			result = math.sum(math.cube(5), math.square(2), math.PI)
			;

		return result;
	}

	var exports = {
		calculateResult: calculateResult
	};

	return exports;
}());