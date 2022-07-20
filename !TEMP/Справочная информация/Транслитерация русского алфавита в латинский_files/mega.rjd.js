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
 
/**
 * Extended js to make mega menu responsive
 * wide: normal menu (apply for desktop)
 * narrow: show 2 level menu only, use click event to show/hide submenu (apply for tabless)
 * tiny: only the first level menu, as submenu of a menu button (apply for mobile)
 */
 
//add menu button to use in small and/or tiny screen
document.addEvent('domready', function () {
 if ($('ja-megamenu')) {
   var menuName = '&#8801;';
   // if (location.href.indexOf('/eng/') > -1) menuName = 'Menu';
   var menubutton = new Element('div', {id: 'ja-menu-button', html: menuName}).inject($('ja-megamenu'), 'before');
   var ul0 = $('ja-megamenu').getElement('ul.level0');
   if (!ul0) return;
   var lis = ul0.getChildren();
   // bind event for this button – show/hide main menu
   menubutton.addEvent('click', function () {
     //action only when the menu button is shown
     if (this.getStyle('display') == 'block') {
       //add/remove class active for main menu
       if (this.getParent().hasClass('rjd-active')) {
         this.getParent().removeClass('rjd-active');
         lis.removeClass('rjd-active');
       } else {
         this.getParent().addClass('rjd-active');
         // Appling scroll to mobile menu
         jQuery('#ja-megamenu').height('auto');
         jQuery('#ja-megamenu').css('overflowX', 'hidden'); 
         var maxMenuHeight = jQuery(window).height() - jQuery('#ja-header').height();
         if (jQuery('#ja-megamenu').height() <= maxMenuHeight)
          jQuery('#ja-megamenu').css('overflowY', 'hidden');
         else
          {
           jQuery('#ja-megamenu').height(maxMenuHeight);
           jQuery('#ja-megamenu').css('overflowY', 'visible'); 
          }  
       };
     }
   });
   
   //bind event for first level menu items
   lis.each (function(li) {
     //add event for link in menu item. the event action only when menubutton is not hide (eg: wide screen)
     if (!li.getElement('a')) return;
     li.getElement('a').addEvent('click', function () {
       if (menubutton.getStyle('z-index') >= 3) {
         //check z-index of this li item: 4 – goto the link
         if (menubutton.getStyle('z-index') == 4) {
           location.href = this.href;
           return;
         }
         //check if has submenu – leave menu process – jump to link
         if (!li.getElement('.childcontent')) return true;
         //add/remove class active for li
         if (li.hasClass('rjd-active')) {
           li.removeClass('rjd-active');
         } else {
           //remove current active
           lis.removeClass('rjd-active');
           //add active for this item
           li.addClass('rjd-active');
         }
         return false;
       }
     });
   });
 }
});
