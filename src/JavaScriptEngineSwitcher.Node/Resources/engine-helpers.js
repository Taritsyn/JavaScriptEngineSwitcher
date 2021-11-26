/*jshint esversion: 6*/
/*globals global, module, require*/
const GENERATED_FUNCTION_CALL_FILE_NAME = "JavaScriptEngineSwitcher.Node.Resources.generated-function-call.js";
let vm = require('vm');
let contexts = new Map();

class UsageError extends Error {
	constructor(...args) {
		super(...args);
		this.name = 'UsageError';

		Error.captureStackTrace(this, UsageError);
	}
}

function getContext(engineId) {
	if (!contexts.has(engineId)) {
		throw new UsageError("JavaScriptEngineSwitcher.Node module cannot work correctly " +
			"when the Node JS service is configured to work in the multi-process mode of " +
			"Jering.Javascript.NodeJS library (https://github.com/JeringTech/Javascript.NodeJS#concurrency), " +
			"because in this mode it is not possible to store a state of JS engine."
		);
	}

	return contexts.get(engineId);
}

module.exports = {
	addContext: (callback, engineId, useBuiltinLibrary) => {
		let sandboxPrototype = useBuiltinLibrary ? global : null;
		let sandbox = Object.create(sandboxPrototype);

		let context = vm.createContext(sandbox);
		contexts.set(engineId, context);

		callback(null);
	},

	removeСontext: (callback, engineId) => {
		contexts.delete(engineId);
		callback(null);
	},

	evaluate: (callback, engineId, expression, documentName, timeout) => {
		let context = getContext(engineId);
		let options = { filename: documentName };
		if (timeout > 0) {
			options.timeout = timeout;
		}
		let result = vm.runInContext(expression, context, options);

		callback(null, result);
	},

	execute: (callback, engineId, code, documentName, timeout) => {
		let context = getContext(engineId);
		let options = { filename: documentName };
		if (timeout > 0) {
			options.timeout = timeout;
		}

		vm.runInContext(code, context, options);

		callback(null);
	},

	callFunction: (callback, engineId, functionName, args, timeout) => {
		let context = getContext(engineId);
		let result;

		if (timeout <= 0) {
			let functionValue = context[functionName];
			result = functionValue.apply(null, args);
		}
		else {
			let options = {
				filename: GENERATED_FUNCTION_CALL_FILE_NAME,
				timeout: timeout
			};
			let argCount = args.length;
			let expression;

			if (argCount > 0) {
				expression = functionName;
				expression += '(';

				for (let argIndex = 0; argIndex < argCount; argIndex++) {
					if (argIndex > 0) {
						expression += ', ';
					}

					expression += JSON.stringify(args[argIndex]);
				}

				expression += ');';
			}
			else {
				expression = `${functionName}();`;
			}

			result = vm.runInContext(expression, context, options);
		}

		callback(null, result);
	},

	hasVariable: (callback, engineId, variableName) => {
		let context = getContext(engineId);
		let result = typeof context[variableName] !== 'undefined';

		callback(null, result);
	},

	getVariableValue: (callback, engineId, variableName) => {
		let context = getContext(engineId);
		let result = context[variableName];

		callback(null, result);
	},

	setVariableValue: (callback, engineId, variableName, value) => {
		let context = getContext(engineId);
		context[variableName] = value;

		callback(null);
	},

	removeVariable: (callback, engineId, variableName) => {
		let context = getContext(engineId);
		if (typeof context[variableName] !== 'undefined') {
			delete context[variableName];
		}

		callback(null);
	}
};