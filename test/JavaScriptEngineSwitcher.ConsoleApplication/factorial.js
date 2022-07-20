debugger;

function factorial(value) {
	if (value <= 0) {
		throw new Error('The value must be greater than or equal to zero.');
	}

	if (value === 1) {
		return value;
	}

	var result = value * factorial(value - 1);

	return result;
}