/**
 * @author alexander.farkas
 * @version 1.2
 */
(function($){
    $.testMedia = function(str){
        var date = new Date().getTime(), styleS, div = $('<div class="testMediaQuery' + date + '"></div>').css({
            visibility: 'hidden',
            position: 'absolute'
        }).appendTo('body'), style = document.createElement('style');
        style.setAttribute('type', 'text/css');
    	style.setAttribute('media', str);
        style = $(style).prependTo('head');
        styleS = document.styleSheets[0];
        if (styleS.cssRules || styleS.rules) {
            if (styleS.insertRule) {
                styleS.insertRule('.testMediaQuery' + date + ' {display:none !important;}', styleS.cssRules.length);
            } else if (styleS.addRule) {
                styleS.addRule('.testMediaQuery' + date, 'display:none');
            }
        }
        var ret = div.css('display') === 'none';
        div.remove();
        style.remove();
        return ret;
    };
    $.arrayInString = function(str, arr){
        var ret = -1;
        $.each(arr, function(i, item){
			if (str.indexOf(item) != -1) {
                ret = i;
                return false;
            }
        });
        return ret;
    };
    $.enableMediaQuery = (function(){
        var styles = [], styleLinks, date = new Date().getTime();
        function parseMedia(link){
            var medias = link.getAttribute('media'), 
				pMin = /\(\s*min-width\s*:\s*(\d+)px\s*\)/, 
				pMax = /\(\s*max-width\s*:\s*(\d+)px\s*\)/, 
				resMin, 
				resMax, 
				supportedMedia = ['handheld', 'all', 'screen', 'projection', 'tty', 'tv', 'print'], 
				curMedia, 
	            mediaString = [];
	            medias = (!medias) ? ['all'] : medias.split(',');
			
            for (var i = 0, len = medias.length; i < len; i++) {
				curMedia = $.arrayInString(medias[i], supportedMedia);
				
                if (curMedia != -1) {
					
                    curMedia = supportedMedia[curMedia];
                    if (!resMin) {
                        resMin = pMin.exec(medias[i]);
                        if (resMin) {
                            resMin = parseInt(resMin[1], 10);
                        }
                    }
                    if (!resMax) {
                        resMax = pMax.exec(medias[i]);
                        if (resMax) {
                            resMax = parseInt(resMax[1], 10);
                        }
                    }
                    mediaString.push(curMedia);
                }
            }
			if (resMin || resMax) {
				styles.push({
					obj: link,
					min: resMin,
					max: resMax,
					medium: mediaString.join(','),
					used: false
				});
			}
        }
        return {
            init: function(){
                if (!styleLinks) {
					var resizeTimer;
                    styleLinks = $('link[rel*=style]').each(function(){
                        parseMedia(this);
                    });
                    $.enableMediaQuery.adjust();
                    $(window).bind('resize.mediaQueries', function(){
						clearTimeout(resizeTimer);
						resizeTimer = setTimeout( $.enableMediaQuery.adjust , 29);
					});
                }
            },
            adjust: function(){
                var width 		= $(window).width(),
					addStyles	= [],
					changeQuery,
					shouldUse,
					i, len
				;
				
                for (i = 0, len = styles.length; i < len; i++) {
					shouldUse = !styles[i].obj.disabled && ((!(styles[i].min && styles[i].min > width) && !(styles[i].max && styles[i].max < width)) || (!styles[i].max && !styles[i].min));
                    if ( shouldUse ) {
                        var n = styles[i].obj.cloneNode(true);
                        n.setAttribute('media', styles[i].medium);
                        n.className = 'insertStyleforMedia' + date;
						addStyles.push(n);
						if( !styles[i].used ){
							styles[i].used = true;
							changeQuery = true;
						}
                    } else if( styles[i].used !== shouldUse ){
						styles[i].used = false;
						changeQuery = true;
					}
                }
				
				if(changeQuery){
					$('link.insertStyleforMedia' + date).remove();
					var head = document.getElementsByTagName('head');
					for(i = 0, len = addStyles.length; i < len; i++){
						head[0].appendChild(addStyles[i]);
					}
					//repaint
					$('body').css('zoom', '1').css('zoom', '');
				}
            }
        };
    })();
	//make some odd assumption before dom-ready
	$.support.mediaQueries = !( $.browser.msie && parseFloat($.browser.version, 10) < 9) || ($.browser.mozilla && parseFloat($.browser.version, 10) < 1.9 );
    setTimeout(function(){
		if (!$.isReady && document.body && !$.support.mediaQueries) {
	        try {
				$.enableMediaQuery.init();
	        } catch (e) {}
	    } 
	}, 1);
    $(function(){
		//test media query compatibility
		$.support.mediaQueries = $.testMedia('only all');
		if (!$.support.mediaQueries) {
            $.enableMediaQuery.init();
        }
    });
})(jQuery);
