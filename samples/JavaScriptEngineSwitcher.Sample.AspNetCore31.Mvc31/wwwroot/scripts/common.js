var jsEngineSwitcher;

(function (jsEngineSwitcher, undefined) {
	"use strict";

	jsEngineSwitcher.registerNamespace = function (namespaceString) {
		var parts = namespaceString.split("."),
			parent = jsEngineSwitcher,
			i
			;

		if (parts[0] === "jsEngineSwitcher") {
			parts = parts.slice(1);
		}

		for (i = 0; i < parts.length; i += 1) {
			if (typeof parent[parts[i]] === "undefined") {
				parent[parts[i]] = {};
			}
			parent = parent[parts[i]];
		}

		return parent;
	};

	jsEngineSwitcher.hasScrollbar = function(elem) {
		return (elem.clientHeight < elem.scrollHeight);
	};
}(jsEngineSwitcher = jsEngineSwitcher || {}));