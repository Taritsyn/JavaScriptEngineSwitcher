function power(x, y) {
	var result = x;

	for (var i = 2; i <= y; i++) {
		result *= x;
	}

	return result;
}