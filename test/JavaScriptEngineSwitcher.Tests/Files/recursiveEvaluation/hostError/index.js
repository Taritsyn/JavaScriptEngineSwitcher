/*global require */
(function () {
	'use strict';

	function calculateResult() {
		var math = require('./match'),
			result = math.sum(math.cube(5), math.square(2), math.PI)
			;

		return result;
	}

	var exports = {
		calculateResult: calculateResult
	};

	return exports;
}());