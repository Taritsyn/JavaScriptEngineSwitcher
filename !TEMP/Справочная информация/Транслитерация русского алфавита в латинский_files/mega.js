/**
 * ------------------------------------------------------------------------
 * JA Elastica Template for J25 & J32
 * ------------------------------------------------------------------------
 * Copyright (C) 2004-2011 J.O.O.M Solutions Co., Ltd. All Rights Reserved.
 * @license – GNU/GPL, http://www.gnu.org/licenses/gpl.html
 * Author: J.O.O.M Solutions Co., Ltd
 * Websites: http://www.joomlart.com – http://www.joomlancers.com
 * ------------------------------------------------------------------------
 */

(function($){
  
  jaMegaMenuMoo = new Class({

    Implements: Options,

    options: {
      action: 'mouseover',  // Action to show menu, mouseover or click
      delayBeforeShow: 300, // Delay in ms before menu is shown
      slide: 0,             // Enable slide
      direction: 'down',
      duration: 300,        // Slide speed, smaller number for faster speed, bigger – for slower
      fading: 0,            // Enable fading
      bgopacity: 0.9,       // Set the transparent background, 0 to disable, 0 < bgopacity < 1: the opacity of the background
      hidestyle: 'normal',
      delayHide: 500,
      fixArrow: false,
      offset: 5
    },

    toElement: function () {
      return this.menu;
    },

    initialize: function (menu, options) {
      this.menu = $(menu);
      if (!this.menu) {
        return;
      }
      this.setOptions(options);

      //ignore delayHide if no animation
      if (!this.options.slide && !this.options.fading) {
        this.options.delayHide = 10;
      }

      this.childopen = [];
      this.imgloaded = false;
      this.loaded = false;
      this.prepare();
    },

    prepare: function () {
      //preload images
      var imgElms = this.menu.getElements('img');
      if (imgElms.length && !this.imgloaded) {
        var imgSrcs = [];
        imgElms.each(function (image) {
          imgSrcs.push(image.src)
        });

        new Asset.images(imgSrcs, {
          onComplete: function () {
            this.start();
          }.bind(this)
        });
        
        this.imgloaded = true;
        //call this start if cannot load image after sometime
        this.start.delay(3000, this);
      } else {
        this.start();
      }
    },

    start: function () {
      //init once
      if (this.loaded) {
        return;
      }
      this.loaded = true;
      this.zindex = 1000;

      //get wrapper
      var pw = this.menu;
      while (pw = pw.getParent()) {
        if (pw.hasClass('main') || pw.hasClass('wrap')) {
          this.wrapper = pw;
          break;
        }
      }

      this.items = this.menu.getElements('li.mega');
      this.items.each(function (li) {
        //link item
        var link = li.getChildren('a.mega')[0],
          child = li.getChildren('.childcontent')[0],
          level0 = li.getParent().hasClass('level0'),
          parent = this.getParent(li),
          item = {
            stimer: null,
            direction: ((level0 && this.options.direction == 'up') ? 0 : 1)
          };

        //child content
        if (child) {
          var childwrap = child.getElement('.childcontent-inner-wrap'),
            childinner = child.getElement('.childcontent-inner'),
            width = childinner.getWidth() + 1,
            height = childinner.getHeight(),
            padding = childwrap.getStyle('padding-left').toInt() + childwrap.getStyle('padding-right').toInt(),
            overflow = false;

          child.setStyles({
            width: width,
            height: height + 20
          });

          childwrap.setStyle('width', width);

          if (['auto', 'scroll'].contains(childinner.getStyle('overflow'))) {
            overflow = true;

            //fix for ie6/7
            if (Browser.ie) {
              if (Browser.version <= 7) {
                childinner.setStyle('position', 'relative');
              }

              if (Browser.version == 6) {
                childinner.setStyle('height', childinner.getStyle('max-height') || 400);
              }
            }
          }

          //show direction
          if (this.options.direction == 'up') {
            if (level0) {
              child.setStyle('top', -child.getHeight()); //ajust top position
            } else {
              child.setStyle('bottom', 0);
            }
          }
        }

        if (child && this.options.bgopacity) {
          //Make transparent background
          new Element('div', {
            'class': 'childcontent-bg',
            styles: {
              width: '100%',
              height: height,
              opacity: this.options.bgopacity,
              position: 'absolute',
              top: 0,
              left: 0,
              zIndex: 1,
              background: child.getStyle('background'),
              backgroundImage: child.getStyle('background-image'),
              backgroundRepeat: child.getStyle('background-repeat'),
              backgroundColor: child.getStyle('background-color')
            }
          }).inject(childwrap, 'top');

          child.setStyle('background', 'none');
          childwrap.setStyles({
            position: 'relative',
            zIndex: 2
          });
        }

        if (child && (this.options.slide || this.options.fading)) {

          if (child.hasClass('right')) {
            child.setStyle('right', 0);
          }

          //init Fx.Styles for childcontent
          var fx = new Fx.Morph(childwrap, {
            duration: this.options.duration,
            transition: Fx.Transitions.linear,
            onComplete: this.itemAnimDone.bind(this, item),
            link: 'cancel'
          }),
            stylesOn = {};

          if (this.options.slide) {
            if (level0) {
              stylesOn[item.direction == 1 ? 'margin-top' : 'bottom'] = 0;
            } else {
              stylesOn[window.isRTL ? 'margin-right' : 'margin-left'] = 0;
            }
          }
          if (this.options.fading) {
            stylesOn['opacity'] = 1;
          }
        }

        if (child && this.options.action == 'click') {
          li.addEvent('click', function (e) {
            e.stopPropagation();

            if (li.hasClass('group')) {
              return;
            }

            if (item.status == 'open') {
              if (this.cursorIn(li, e)) {
                this.itemHide(item);
              } else {
                this.hideOthers(li);
              }
            } else {
              this.itemShow(item);
            }
          }.bind(this));
        }

        if (this.options.action == 'mouseover' || this.options.action == 'mouseenter') {
          li.addEvent('mouseover', function (e) {
            if (li.hasClass('group')) {
              return;
            }

            e.stop();
            
            clearTimeout(item.stimer);
            clearTimeout(this.atimer);

            this.intent(item, 'open');
            // this.itemShow(item);
            item.stimer = this.itemShow.delay(this.options.delayBeforeShow, this, [item]);

          }.bind(this))

          .addEvent('mouseleave', function (e) {
            if (li.hasClass('group')) {
              return;
            }

            clearTimeout(item.stimer);

            this.intent(item, 'close');
            if (child) {
              item.stimer = this.itemHide.delay(this.options.delayHide, this, [item]);
            } else {
              this.itemHide(item);
            }
          }.bind(this));

          //if has childcontent, don't goto link before open childcontent – fix for touch screen
          if (link && child) {
            link.addEvent('click', function (e) {
              if (!item.clickable) {
                e.stop();
              }
            });
          }

          //stop if click on menu item – prevent raise event to container => hide all open submenu
          li.addEvent('click', function (e) {
            e.stopPropagation()
          });

          if (child) {
            child.addEvent('mouseover', function () {
              clearTimeout(item.stimer);
              clearTimeout(this.atimer);

              this.intent(item, 'open');
              this.itemShow(item);
            }.bind(this)).addEvent('mouseleave', function (e) {
              e.stop();

              this.intent(item, 'close');
              clearTimeout(item.stimer);

              if (!this.cursorIn(item.el, e)) {
                this.atimer = this.hideAlls.delay(this.options.delayHide, this);
              }
            }.bind(this))
          }
        }

        //when click on a link – close all open childcontent
        if (link && !child) {
          link.addEvent('click', function (e) {
            e.stopPropagation(); //prevent to raise event up
            this.hideOthers(null);
            //Remove current class
            this.menu.getElements('.active').removeClass('active');

            //Add current class
            var p = li;
            while (p) {
              var idata = p.retrieve('item');

              p.addClass('active');
              idata.link.addClass('active');
              p = idata.parent;
            }
          }.bind(this));
        }

        Object.append(item, {
          el: li,
          parent: parent,
          link: link,
          child: child,
          childwrap: childwrap,
          childinner: childinner,
          width: width,
          height: height,
          padding: padding,
          level0: level0,
          fx: fx,
          stylesOn: stylesOn,
          overflow: overflow,
          clickable: !(link && child)
        });

        li.store('item', item);
      }, this);

      //click on windows will close all submenus
      var container = $('ja-wrapper');
      if (!container) {
        container = document.body;
      }

      container.addEvent('click', function (e) {
        this.hideAlls();
      }.bind(this));

      this.menu.getElements('.childcontent').setStyle('display', 'none');
    },

    getParent: function (el) {
      var p = el;
      while ((p = p.getParent())) {
        if (this.items.contains(p) && !p.hasClass('group')) {
          return p;
        }

        if (!p || p == this.menu) {
          return null;
        }
      }
    },

    intent: function (item, action) {
      item.intent = action;

      while (item.parent && (item = item.parent.retrieve('item'))) {
        item.intent = action;
      }
    },

    cursorIn: function (el, event) {
      if (!el || !event) {
        return false;
      }

      var pos = el.getPosition(),
        cursor = event.page;

      return (cursor.x > pos.x && cursor.x < pos.x + el.getWidth() && cursor.y > pos.y && cursor.y < pos.y + el.getHeight());
    },

    itemOver: function (item) {
      item.el.addClass('over');

      if (item.el.hasClass('haschild')) {
        item.el.removeClass('haschild').addClass('haschild-over');
      }

      if (item.link) {
        item.link.addClass('over');
      }
    },

    itemOut: function (item) {
      item.el.removeClass('over');

      if (item.el.hasClass('haschild-over')) {
        item.el.removeClass('haschild-over').addClass('haschild');
      }

      if (item.link) {
        item.link.removeClass('over');
      }
    },

    itemShow: function (item) {
      
      if(this.childopen.indexOf(item) < this.childopen.length -1){
        this.hideOthers(item.el);
      }
      
      if (item.status == 'open') {
        return; //don't need do anything
      }

      //Setup the class
      this.itemOver(item);

      //push to show queue
      if (item.level0) {
        this.childopen.length = 0;
      }

      if (item.child) {
        this.childopen.push(item);
      }

      item.intent = 'open';
      item.status = 'open';

      this.enableclick.delay(100, this, item);

      if (item.child) {
        //reposition the submenu
        this.positionSubmenu(item);

        if (item.fx && !item.stylesOff) {
          item.stylesOff = {};
          if (this.options.slide) {
            if (item.level0) {
              item.stylesOff[item.direction == 1 ? 'margin-top' : 'bottom'] = -item.height;
            } else {
              item.stylesOff[window.isRTL ? 'margin-right' : 'margin-left'] = (item.direction == 1 ? -item.width : item.width);
            }
          }
          if (this.options.fading) {
            item.stylesOff['opacity'] = 0;
          }
          item.fx.set(item.stylesOff);
        }

        clearTimeout(item.sid);
        item.child.setStyles({
          display: 'block',
          zIndex: this.zindex++
        });
      }

      if (!item.fx || !item.child) {
        return;
      }

      item.child.setStyle('overflow', 'hidden');
      if (item.overflow) {
        item.childinner.setStyle('overflow', 'hidden');
      }

      item.fx.start(item.stylesOn);
    },

    itemHide: function (item) {
      clearTimeout(item.stimer);

      item.status = 'close';
      item.intent = 'close';

      this.itemOut(item);
      this.childopen.erase(item);

      if (!item.fx && item.child) {
        clearTimeout(item.sid);
        item.sid = setTimeout(function(){ item.child.setStyle('display', 'none'); }, this.options.delayHide);
      }

      if (!item.fx || !item.child || item.child.getStyle('opacity') == '0') {
        return;
      }

      item.child.setStyle('overflow', 'hidden');
      if (item.overflow) {
        item.childinner.setStyle('overflow', 'hidden');
      }

      switch (this.options.hidestyle) {
      case 'fast':
        item.fx.options.duration = 100;
        item.fx.start(item.stylesOff);
        break;
      case 'fastwhenshow':
        item.fx.start(Object.merge(item.stylesOff, {
          'opacity': 0
        }));
        break;
      case 'normal':
      default:
        item.fx.start(item.stylesOff);
        break;
      }
    },

    itemAnimDone: function (item) {
      //hide done
      if (item.status == 'close') {
        //reset duration and enable opacity if not fading
        if (this.options.hidestyle.test(/fast/)) {
          item.fx.options.duration = this.options.duration;
          if (!this.options.fading) {
            item.childwrap.setStyle('opacity', 1);
          }
        }
        //hide
        item.child.setStyle('display', 'none');
        this.disableclick.delay(100, this, item);

        var pitem = item.parent ? item.parent.retrieve('item') : null;
        if (pitem && pitem.intent == 'close') {
          this.itemHide(pitem);
        }
      }

      //show done
      if (item.status == 'open') {
        item.child.setStyle('overflow', '');
        if (item.overflow) {
          item.childinner.setStyle('overflow-y', 'auto');
        }

        item.childwrap.setStyle('opacity', 1);
        item.child.setStyle('display', 'block');
      }
    },

    hideOthers: function (el) {
      this.childopen.each(function (item) {
        if (!el || (item.el != el && !item.el.contains(el))) {
          item.intent = 'close';
        }
      });

      if (this.options.slide || this.options.fading) {
        var last = this.childopen.getLast();
        if (last && last.intent == 'close') {
          this.itemHide(last);
        }
      } else {
        this.childopen.each(function (item) {
          if(item.intent == 'close'){
            this.itemHide(item);  
          }
        }, this);
      }
    },

    hideAlls: function (el) {
      this.childopen.flatten().each(function (item) {
        if (!item.fx) {
          this.itemHide(item);
        } else {
          item.intent = 'close';
        }
      }, this);

      if (this.options.slide || this.options.fading) {
        var last = this.childopen.getLast();
        if (last && last.intent == 'close') {
          this.itemHide(last);
        }
      }
    },

    enableclick: function (item) {
      if (item.link && item.child) {
        item.clickable = true;
      }
    },

    disableclick: function (item) {
      item.clickable = false;
    },

    positionSubmenu: function (item) {
      var options = this.options, offsleft, offstop, left, top, stylesOff = {},
        icoord = item.el.getCoordinates(),
        bodySize = $(document.body).getScrollSize(),
        winRect = {
          top: window.getScrollTop(),
          left: window.getScrollLeft(),
          width: window.getWidth(),
          height: window.getHeight()
        },
        wrapRect = this.wrapper ? this.wrapper.getCoordinates() : {
          top: 0,
          left: 0,
          width: winRect.width,
          height: winRect.height
        };

      winRect.top = Math.max(winRect.top, wrapRect.top);
      winRect.left = Math.max(winRect.left, wrapRect.left);
      winRect.width = Math.min(winRect.width, wrapRect.width);
      winRect.height = Math.min(winRect.height, $(document.body).getScrollHeight());
      winRect.right = winRect.left + winRect.width;
      winRect.bottom = winRect.top + winRect.height;

      if (!item.level0) {
        var pitem = item.parent.retrieve('item'),
          offsety = parseFloat(pitem.child.getFirst().getStyle('margin-top')),
          offsetx = parseFloat(pitem.child.getFirst().getStyle(window.isRTL ? 'margin-right' : 'margin-left'));

        item.direction = pitem.direction;

        window.isRTL && (offsetx = 0 - offsetx);
        icoord.top -= offsety;
        icoord.bottom -= offsety;
        icoord.left -= offsetx;
        icoord.right -= offsetx;
      }

      if (item.level0) {
        if (window.isRTL) {
          offsleft = Math.max(winRect.left, icoord.right - item.width - 20);
          left = Math.max(0, offsleft - winRect.left);
        } else {
          offsleft = Math.max(winRect.left, Math.min(winRect.right - item.width, icoord.left));
          left = Math.max(0, Math.min(winRect.right - item.width, icoord.left) - winRect.left);
        }
      } else {
        if (window.isRTL) {
          if (item.direction == 1) {
            offsleft = icoord.left - item.width - 20 + options.offset;
            left = -icoord.width - 20;

            if (offsleft < winRect.left) {
              item.direction = 0;
              offsleft = Math.min(winRect.right, Math.max(winRect.left, icoord.right + item.padding - 20 - options.offset));
              left = icoord.width - 20;
              stylesOff['margin-right'] = item.width;
            }
          } else {
            offsleft = icoord.right + item.padding - 20;
            left = icoord.width - 20;

            if (offsleft + item.width > winRect.right) {
              item.direction = 1;
              offsleft = Math.max(winRect.left, icoord.left - item.width - 20);
              left = -icoord.width - 20;
              stylesOff['margin-right'] = -item.width;
            }
          }
        } else {

          if (item.direction == 1) {
            offsleft = icoord.right - options.offset;
            left = icoord.width;

            if (offsleft + item.width > winRect.right) {
              item.direction = 0;
              offsleft = Math.max(winRect.left, icoord.left - item.width - item.padding + options.offset);
              left = -icoord.width;
              stylesOff['margin-left'] = item.width;
            }
          } else {
            offsleft = icoord.left - item.width - item.padding + options.offset;
            left = -icoord.width;

            if (offsleft < winRect.left) {
              item.direction = 1;
              offsleft = Math.max(winRect.left, Math.min(winRect.right - item.width, icoord.right - options.offset));
              left = icoord.width;
              stylesOff['margin-left'] = -item.width;
            }
          }
        }
      }

      if (options.slide && item.fx && Object.getLength(stylesOff)) {
        item.fx.set(stylesOff);
      }

      // Teline IV
      if (options.fixArrow && item.childinner) {
        item.childinner.setStyle('background-position', (icoord.left - offsleft + (icoord.width / 2 - 4.5)) + 'px top');
      }

      var oldp = item.child.getStyle('display');
      item.child.setStyle('display', 'block');
      if (item.child.getOffsetParent()) {
        left = offsleft - item.child.getOffsetParent().getCoordinates().left;
      }
      item.child.setStyles({
        'margin-left': 0,
        'left': left,
        'display': oldp
      });
    }
  });

})(document.id);
