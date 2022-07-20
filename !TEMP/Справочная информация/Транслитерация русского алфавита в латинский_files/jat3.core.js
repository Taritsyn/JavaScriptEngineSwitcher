/**
 * ------------------------------------------------------------------------
 * JA T3v2 System Plugin for J25 & J32
 * ------------------------------------------------------------------------
 * Copyright (C) 2004-2011 J.O.O.M Solutions Co., Ltd. All Rights Reserved.
 * @license – GNU/GPL, http://www.gnu.org/licenses/gpl.html
 * Author: J.O.O.M Solutions Co., Ltd
 * Websites: http://www.joomlart.com – http://www.joomlancers.com
 * ------------------------------------------------------------------------
 */

function switchFontSize (ckname,val) {
	var bd = document.getElementsByTagName('body');
	if (!bd || !bd.length) return;
	bd = bd[0];
	//var oldclass = 'fs'+CurrentFontSize;
	switch (val) {
		case 'inc':
			if (CurrentFontSize+1 < 7) {
				CurrentFontSize++;
			}
			break;
		case 'dec':
			if (CurrentFontSize-1 > 0) {
				CurrentFontSize--;
			}
			break;
		case 'reset':
		default:
			CurrentFontSize = DefaultFontSize;
	}
	var newclass = 'fs'+CurrentFontSize;
	bd.className = bd.className.replace(new RegExp('fs.?', 'g'), '');
	bd.className = trim(bd.className);
	bd.className += (bd.className?' ':'') + newclass;
	createCookie(ckname, CurrentFontSize, 365);
}

function switchTool (ckname, val) {
	createCookie(ckname, val, 365);
	window.location.reload();
}

function cpanel_reset () {
	var matches = document.cookie.match(new RegExp('(?:^|;)\\s*' + tmpl_name.escapeRegExp() + '_([^=]*)=([^;]*)', 'g'));
	if (!matches) return;
	for (var i = 0; i < matches.length; i++) {
		var ck = matches[i].match(new RegExp('(?:^|;)\\s*' + tmpl_name.escapeRegExp() + '_([^=]*)=([^;]*)'));
		if (ck) {
			createCookie (tmpl_name+'_'+ck[1], '', -1);
		}
	}

	if (window.location.href.indexOf('?') > -1) {
		window.location.href = window.location.href.substr(0,window.location.href.indexOf ('?'));
	} else {
		window.location.reload(true);
	}
}

function cpanel_apply () {
	var elems = document.getElementById('ja-cpanel-main').getElementsByTagName('*');

	var usersetting = {};
	for (var i = 0; i < elems.length; i++) {
		var el = elems[i];
		if (el.name && (match = el.name.match(/^user_(.*)$/))) {
			var name = match[1];
			var value = '';
			if (el.tagName.toLowerCase() == 'input' && (el.type.toLowerCase()=='radio' || el.type.toLowerCase()=='checkbox')) {
				if (el.checked) value = el.value;
			} else {
				value = el.value;
			}
			if (usersetting[name]) {
				if (value) usersetting[name] = value + ',' + usersetting[name];
			} else {
				usersetting[name] = value;
			}
		}
	}

	for (var k in usersetting) {
		name  = tmpl_name + '_' + k;
		value = usersetting[k].trim();
		if (value.length > 0) {
			createCookie(name, value, 365);
		}
	}

	if (window.location.href.indexOf ('?')>-1) {
		window.location.href = window.location.href.substr(0,window.location.href.indexOf ('?'));
	} else {
		window.location.reload(true);
	}
}

function createCookie(name,value,days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime()+(days*24*60*60*1000));
		var expires = "; expires="+date.toGMTString();
	} else {
		expires = "";
	}
	document.cookie = name+"="+value+expires+"; path=/";
}

function trim(str, chars) {
	return ltrim(rtrim(str, chars), chars);
}

function ltrim(str, chars) {
	chars = chars || "\\s";
	return str.replace(new RegExp("^[" + chars + "]+", "g"), "");
}

function rtrim(str, chars) {
	chars = chars || "\\s";
	return str.replace(new RegExp("[" + chars + "]+$", "g"), "");
}

function getScreenWidth () {
	var x = 0;
	if (self.innerHeight) {
			x = self.innerWidth;
	} else if (document.documentElement && document.documentElement.clientHeight) {
			x = document.documentElement.clientWidth;
	} else if (document.body) {
			x = document.body.clientWidth;
	}
	return x;
}

function equalHeight (els) {
	els = $$_(els);
	if (!els || els.length < 2) return;
	var maxh = 0;
	var els_ = [];
	els.each(function(el, i){
		if (!el) return;
		//els_[i] = getDeepestWrapper (el);
		els_[i] = el;
		var ch = els_[i].getCoordinates().height;
		maxh = (maxh < ch) ? ch : maxh;
	},this);

	els_.each(function(el, i) {
		if (!el) return;
		if (el.getStyle('padding-top')!=null && el.getStyle('padding-bottom')!=null) {
			if (maxh-el.getStyle('padding-top').toInt()-el.getStyle('padding-bottom').toInt() > 0) {
				el.setStyle('min-height', maxh-el.getStyle('padding-top').toInt()-el.getStyle('padding-bottom').toInt());
			}
		} else {
			if (maxh > 0) el.setStyle('min-height', maxh);
		}
	}, this);
}

function getDeepestWrapper (el) {
	while (el.getChildren().length == 1) {
		el = el.getChildren()[0];
	}
	return el;
}

function fixHeight (els, group1, group2) {
	els = $$_(els);
	group1 = $$_(group1);
	group2 = $$_(group2);
	if (!els || !group1) return;
	var height = 0;
	group1.each(function(el) {
		if (!el) return;
		height += el.getCoordinates().height;
	});
	if (group2) {
		group2.each(function (el){
			if (!el) return;
			height -= el.getCoordinates().height;
		});
	}
	els.each(function(el, i) {
		if (!el) return;
		if (el.getStyle('padding-top') != null && el.getStyle('padding-bottom') != null) {
			if (height-el.getStyle('padding-top').toInt()-el.getStyle('padding-bottom').toInt() > 0) {
				el.setStyle('min-height', height-el.getStyle('padding-top').toInt()-el.getStyle('padding-bottom').toInt());
			}
		} else {
			if (height > 0) {
				el.setStyle('min-height', height);
			}
		}
	});
}

function addFirstLastItem (el) {
	el = $(el);
	if (!el || !el.getChildren() || !el.getChildren().length) return;
	el.getChildren ()[0].addClass ('first-item');
	el.getChildren ()[el.getChildren ().length-1].addClass ('last-item');
}

function $$_ (els) {
	if (typeOf(els)=='string') return $$(els);
	var els_ = [];
	els.each (function (el){
		el = $(el);
		if (el) els_.push (el);
	});
	return els_;
}

$(document).addEvent('domready', function() {
	$$('[data-dismiss="alert"]').each(function(el) {
		el.addEvent('click', function() {
			el.getParent().destroy();

			if($('system-message').getChildren().length == 0){
				Joomla.removeMessages();
			}
		});
	});
});
