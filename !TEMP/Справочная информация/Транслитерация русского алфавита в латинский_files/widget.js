/**
 * Loginza widget
 * @version 1.2.0
 * @updated 03.08.2011
 */
if ((typeof LOGINZA == "undefined") || !LOGINZA) {
	// инициализация объекта LOGINZA
    var LOGINZA = {
    	'loaded': false,
        'token_url': null,
        'selected_provider': null,
        'providers_set': null,
        'service_host': '//loginza.ru',
        'lang': null,
        'ajax': false,
        'mobile': false,
        'callback': null,
        'hash': ''
    };
}
// показать форму
LOGINZA.show = function () {
	// пред выбор провайдера
	LOGINZA.selected_provider = LOGINZA.getQueryStringValue(this, 'provider');
	// набор провайдеров в виджете
	LOGINZA.providers_set = LOGINZA.getQueryStringValue(this, 'providers_set');
	// получение token
	LOGINZA.token_url = LOGINZA.getQueryStringValue(this, 'token_url');
	// установка языка интерфейса
	LOGINZA.lang = LOGINZA.getQueryStringValue(this, 'lang');
	// мобильная версия
	LOGINZA.mobile = LOGINZA.getQueryStringValue(this, 'mobile');
	// определение устройства
	if (LOGINZA.mobile == 'auto') {
		var nav = window.navigator.userAgent;
		var mobua = ['iPhone', 'Android', 'iPad', 'Opera Mobi', 'Kindle/3.0'];
		LOGINZA.mobile = false;
		for (var i=0; i<mobua.length; i++){
			if (nav.indexOf(mobua[i]) >= 0) {
				LOGINZA.mobile = true;
				break;
			}
		}
	} else if (LOGINZA.mobile) {
		LOGINZA.mobile = true;
	} else {
		LOGINZA.mobile = false;
	}
	
	
	if (!LOGINZA.mobile && !LOGINZA.loaded) {
		var cldDiv = document.createElement("div");
		cldDiv.id = 'loginza_auth_form';
		cldDiv.style.overflow = 'visible';
		cldDiv.style.backgroundColor = 'transparent';
		cldDiv.style.zIndex = '10000';
		cldDiv.style.position = 'fixed';
		cldDiv.style.display = 'block';
		cldDiv.style.top = '0px';
		cldDiv.style.left = '0px';
		cldDiv.style.textAlign = 'center';
		cldDiv.style.height = '878px';
		cldDiv.style.width = '1247px';
		cldDiv.style.paddingTop = '125px';
		cldDiv.style.backgroundImage = 'url('+LOGINZA.service_host+'/img/widget/overlay.png)';
		
		var cntDiv = document.createElement("div");
		cntDiv.style.position = 'relative';
		cntDiv.style.display = 'inline';
		cntDiv.style.overflow = 'visible';
		
		var img = document.createElement("img");
		img.onclick = LOGINZA.close;
		img.style.position = 'relative';
		img.style.left = '348px';
		img.style.top = '-332px';
		img.style.cursor = 'hand';
		img.style.width = '7px';
		img.style.height = '7px';
		img.style.border = '0';
		img.alt = 'X';
		img.title = 'Close';
		img.src = LOGINZA.service_host+'/img/widget/close.gif';
		
		var iframe = document.createElement("iframe");
		iframe.id = 'loginza_main_ifr';
		iframe.width = '359';
		iframe.height = '350';
		
		if (LOGINZA.mobile) {
			iframe.width = '320';
			iframe.height = '480';
		}
		iframe.scrolling = 'no';
		iframe.frameBorder = '0';
		iframe.src = "javascript:'<html><body style=background-color:transparent><h1>Loading...</h1></body></html>'";
		
		// appends
		cntDiv.appendChild(img);
		cldDiv.appendChild(cntDiv);
		cldDiv.appendChild(iframe);
		
		try {
			cldDiv.style.paddingTop = (window.innerHeight-350)/2 + 'px';
		} catch (e) {
			cldDiv.style.paddingTop = '100px';
		}
		cldDiv.style.paddingLeft = 0;
		cldDiv.style.height = '2000px';
		cldDiv.style.width = document.body.clientWidth + 50 + 'px';
		// создание контейнера для формы
		document.body.appendChild(cldDiv);
		// форма загружена
		LOGINZA.loaded = true;
		
		// включена AJAX авторизация
		if (LOGINZA.ajax) {
			setInterval(LOGINZA.hashParser, 500);
		}
	}
	
	if (!LOGINZA.token_url) {
		alert('Error token_url value!');
	} else {
		var loginza_url = LOGINZA.service_host+'/api/widget.php?overlay=true&w='
		+document.body.clientWidth+
		'&token_url='+encodeURIComponent(LOGINZA.token_url)+
		'&provider='+encodeURIComponent(LOGINZA.selected_provider)+
		'&providers_set='+encodeURIComponent(LOGINZA.providers_set)+
		'&lang='+encodeURIComponent(LOGINZA.lang)+
		'&ajax='+(LOGINZA.ajax ? 'true' : 'false')+
		(LOGINZA.mobile ? '&mobile=true' : '');
		
		if (LOGINZA.mobile) {
			document.location = loginza_url;
		} else {
			// загрузка формы
			document.getElementById('loginza_main_ifr').setAttribute('src', loginza_url);
		}
	}
	
	if (!LOGINZA.mobile) {
		// показать форму
		document.getElementById('loginza_auth_form').style.display = '';
	}
	return false;
}
LOGINZA.close = function () {
	document.getElementById('loginza_auth_form').style.display = 'none';
}
// изменение размеров окна браузера
LOGINZA.resize = function () {
	var frm = document.getElementById('loginza_auth_form');
	if (frm) {
		frm.style.width = document.body.clientWidth + 50 + 'px';
		try {
			frm.style.paddingTop = (window.innerHeight-350)/2 + 'px';
		} catch (e) {
			frm.style.paddingTop = '100px';
		}
	}
}
// получение параметра из ссылки
LOGINZA.getQueryStringValue = function (link, key) {
	var url_str = link.href;
    var match = null;
    var query_str = url_str.match(/^[^?]*(?:\?([^#]*))?(?:$|#.*$)/)[1]
    var _query_regex = new RegExp("([^=]+)=([^&]*)&?", "g");
    while ((match = _query_regex.exec(query_str)) != null)
    {
        if (decodeURIComponent(match[1]) == key) {
            return decodeURIComponent(match[2]);
        }
    }
    return null;
}
LOGINZA.findClass = function (str, node) {
	if(document.getElementsByClassName) return (node || document).getElementsByClassName(str);
	else {
		var node = node || document, list = node.getElementsByTagName('*'), length = list.length, Class = str.split(/\s+/), classes = Class.length, array = [], i, j, key;
		for(i = 0; i < length; i++) {
			key = true;
			for(j = 0; j < classes; j++) if(list[i].className.search('\\b' + Class[j] + '\\b') == -1) key = false;
			if(key) array.push(list[i]);
		}
		return array;
	}
}
LOGINZA.addEvent = function (obj, type, fn){
	if (obj.addEventListener){
	      obj.addEventListener( type, fn, false);
	} else if(obj.attachEvent) {
	      obj.attachEvent( "on"+type, fn );
	} else {
	      obj["on"+type] = fn;
	}
}
LOGINZA.init = function () {
	// обработчик на открытие формы
	if (document.getElementById('loginza') && document.getElementById('loginza').href != undefined) {
		document.getElementById('loginza').onclick = LOGINZA.show;
	}
	var i, list = LOGINZA.findClass('loginza'), length = list.length;
	for(i = 0; i < length; i++) {
		if (list[i].href != undefined) {
			list[i].onclick = LOGINZA.show;
		}
	}
	// прочие обработчики
	LOGINZA.addEvent(window, 'resize', LOGINZA.resize);
	LOGINZA.addEvent(document, 'keydown', function(e) {
		e = e || window.event;
		if (e.keyCode == 27) {
			LOGINZA.close();
		}
		return true;
	});
}
LOGINZA.widget = function () {
	var iframeNode = document.getElementById('loginza_main_ifr');
	if (iframeNode.contentDocument) return iframeNode.contentDocument
	if (iframeNode.contentWindow) return iframeNode.contentWindow.document
	return iframeNode.document
}
LOGINZA.hashParser = function () {
	var func, param;
	try {
		var hash = LOGINZA.widget().location.hash.substr(1);
		var commands = hash.split(';');
		// набор якорь, функция для обработки нажатий по ссылкам
		var callbacks = [
		    ['token:', 'getToken']
		];
		// если хеш новый
		if (hash != LOGINZA.hash) {
			for (var k=0; k<commands.length; k++) {
				// вызов нужного callback в зависимости от переданного якоря
				for (var i=0; i<callbacks.length; i++) {
					func = callbacks[i][1];
					param = commands[k].substr(callbacks[i][0].length);
					
					if (commands[k].indexOf(callbacks[i][0])===0) {
						LOGINZA[func](param);
					}
				}
			}
			LOGINZA.hash = hash;
		}
	} catch (e) {}
}
LOGINZA.getToken = function (token) {
	LOGINZA.close();
	LOGINZA.callback(token);
}
LOGINZA.scriptMessage = function (event) {
	if (typeof LOGINZA[event.data] != 'undefined') {
		LOGINZA[event.data]();
	}
}
LOGINZA.redirect = function () {
	document.location = LOGINZA.service_host+'/api/redirect?rnd='+Math.random();
}
LOGINZA.addEvent(window, 'load', LOGINZA.init);
LOGINZA.addEvent(window, 'message', LOGINZA.scriptMessage);
