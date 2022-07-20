var quoteText = '';

function addBookmark ()
 {
 };

function addBookmarkOnClick (a, hRef, title)
 {
  if (window.opera && window.print) // Opera
   {
    var mbm = a;
    mbm.setAttribute('rel', 'sidebar');
    mbm.setAttribute('href', hRef);
    mbm.setAttribute('title', title);
    mbm.click();
   }
  else if (window.sidebar) // Firefox
   window.sidebar.addPanel(title, hRef, '')
  else if (document.all) // Internet Explorer
   window.external.addFavorite(hRef, title)
  else
   alert('Ctrl + D');
 };

function addSmile (smile)
 {
  var memo = document.form.posttext;
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    var range = document.selection.createRange();
    range.text = smile;
    range.collapse();
    range.select();
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    memo.value = value.substr(0, start) + smile + value.substr(end, value.length);
    memo.setSelectionRange(start + smile.length, start + smile.length);
   }
  else // Unknown
   memo.value += smile;
 };

function blockQuote ()
 {
  var qText = '' + quoteText + '';
  var memo = document.form.posttext;
  if (document.selection) // Internet Explorer || Opera 8.0+
   {
    memo.focus();
    if (document.selection.createRange().text == qText)
     formatText('blockquote')
    else
     memo.value += '<blockquote>' + qText + '</blockquote>';
   }
  else if (window.getSelection) // Firefox || Mozilla
   {
    memo.focus();
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    if ((value.substr(start, end - start) != '') || (qText == ''))
     formatText('blockquote')
    else
     memo.value += '<blockquote>' + qText + '</blockquote>';
   }
  else
   formatText('blockquote');
  quoteText = '';
 };

function checkAllItems ()
 {
  var checked = document.getElementById('checkAll').checked;
  var itemCheckboxes = document.getElementsByClassName('item-checkbox');
  for (var i = 0; i < itemCheckboxes.length; i++) itemCheckboxes[i].checked = checked;
  document.getElementById('subscriptionsToEditDiv').style.display = 'none';
  document.getElementById('editDiv').className = '';
  document.getElementById('editButton').disabled = ((itemCheckboxes.length != 1) || (!checked));
  document.getElementById('deleteButton').disabled = ((itemCheckboxes.length == 0) || (!checked));
  return true;
 };

function checkSingleItem ()
 {
  var itemsChecked = 0;
  var itemCheckboxes = document.getElementsByClassName('item-checkbox');
  for (var i = 0; i < itemCheckboxes.length; i++) if (itemCheckboxes[i].checked) itemsChecked++;
  var allCheckbox = document.getElementById('checkAll');
  if ((itemsChecked > 0) && (itemsChecked < itemCheckboxes.length))
   allCheckbox.indeterminate = true
  else
   {
    allCheckbox.indeterminate = false;
    allCheckbox.checked = (itemsChecked == itemCheckboxes.length);
   };
  document.getElementById('subscriptionsToEditDiv').style.display = 'none';
  document.getElementById('editDiv').className = '';
  document.getElementById('editButton').disabled = (itemsChecked != 1);
  document.getElementById('deleteButton').disabled = (itemsChecked == 0);
 };

function copyQuote (event)
 {
  var qText = '';
  if (document.selection) // Internet Explorer || Opera 8.0+
   qText = document.selection.createRange().text
  else if (window.getSelection) // Firefox || Mozilla
   qText = window.getSelection()
  else
   qText = '';
  if ((event != 'mousedown') || (qText != '') || (quoteText == '')) quoteText = qText;
 };

function enableUserName (enabled)
 {
  var userName = document.getElementById('userName');
  userName.readOnly = !enabled;
  if (enabled)
   userName.style.backgroundColor = ''
  else
   {
    userName.style.backgroundColor = '#eaeaea';
    userName.value = userName.attributes.getNamedItem('externalvalue').value;
   };
  if (enabled) userName.focus();
 };

function enableVote (state)
 {
  if (state && document.voteform.postmark.options[0].selected) state = false;
  document.voteform.votebutton.disabled = !state;
 };

function fontSize (language)
 {
  var sizeCaption = '';
  if (language == 'eng')
   sizeCaption = 'Font size (%):'
  else
   sizeCaption = 'Размер шрифта (%):';
  var selectedText = '';
  var memo = document.form.posttext;
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    var range = document.selection.createRange();
    selectedText = range.text;
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    selectedText = value.substr(start, end - start);
   };
  var sizeValue = 100;
  if (selectedText.substr(0, 23) == '<span style="font-size: ')
   {
    sizeValue = selectedText.substr(23, selectedText.length);
    if (sizeValue.indexOf('%;">') > -1) sizeValue = sizeValue.substr(0, sizeValue.indexOf('%;">'));
    if (selectedText.indexOf('>') > -1)
     {
      selectedText = selectedText.substr(selectedText.indexOf('>') + 1, selectedText.length);
      if (selectedText.indexOf('</span>') > -1) selectedText = selectedText.substr(0, selectedText.indexOf('</span>'));
     };
   };
  var size = prompt(sizeCaption, sizeValue);
  if ((size == null) || (size == '')) return;
  if (size.substr(size.length - 1, 1) == '%') size = size.substr(0, size.length - 1);
  if (isNaN(size) || isNaN(parseInt(size))) return;
  if (parseInt(size) <= 0) return;
  if (parseInt(size) != 100) selectedText = '<span style="font-size: ' + size + '%;">' + selectedText + '</span>';
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    range = document.selection.createRange();
    var rangeLen = range.text.length;
    range.text = selectedText;
    if (rangeLen > 0)
     {
      range.moveStart("character", 0 - selectedText.length);
      range.select();
     };
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    start = memo.selectionStart;
    end = memo.selectionEnd;
    value = memo.value;
    memo.value = value.substr(0, start) + selectedText + value.substr(end, value.length);
    if (start != end) memo.setSelectionRange(start, end + memo.value.length - value.length);
   }
  else // Unknown
   memo.value += selectedText;
 };

function formatQuote (dTimeAuthor)
 {
  var memo = document.form.posttext;
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    var range = document.selection.createRange();
    var rangeLen = range.text.length;
    var tagAdded = true;
    if ((range.text.substr(0, 12) != '<blockquote>') || (range.text.substr(range.text.length - 13, 13) != '</blockquote>'))
     range.text = '<blockquote><b>' + dTimeAuthor + '</b><br/>' + range.text + '</blockquote>'
    else
     {
      range.text = range.text.substr(12, range.text.length - 25);
      tagAdded = false;
     };
    if (rangeLen == 0)
     {
      range.move("character", 0 - 13);
      range.collapse();
     }
    else if (tagAdded)
     range.moveStart("character", 0 - rangeLen - 25)
    else
     range.moveStart("character", 0 - rangeLen + 25);
    range.select();
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    if ((value.substr(start, 12) == '<blockquote>') && (value.substr(end - 13, 13) == '</blockquote>'))
     memo.value = value.substr(0, start) + value.substr(start + 12, end - start - 25) + value.substr(end, value.length)
    else // if ((start != 0) || (start != end))
     memo.value = value.substr(0, start) + '<blockquote><b>' + dTimeAuthor + '</b><br/>' + value.substr(start, end - start) + '</blockquote>' + value.substr(end, value.length)
    if (start == end)
     memo.setSelectionRange(start + dTimeAuthor.length + 23, start + dTimeAuthor.length + 23)
    else if (value.substr(start, 12) == '<blockquote>')
     memo.setSelectionRange(start, end - 25)
    else
     memo.setSelectionRange(start, 25 + end);
   }
  else // Unknown
   memo.value += '<blockquote><b>' + dTimeAuthor + '</b><br/></blockquote>';
 };

function formatText (style)
 {
  var memo = document.form.posttext;
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    var range = document.selection.createRange();
    var rangeLen = range.text.length;
    var tagAdded = true;
    if ((range.text.substr(0, style.length + 2) != '<' + style + '>') || (range.text.substr(range.text.length - style.length - 3, style.length + 3) != '</' + style + '>'))
     range.text = '<' + style + '>' + range.text + '</' + style + '>'
    else
     {
      range.text = range.text.substr(style.length + 2, range.text.length - 2 * style.length - 5);
      tagAdded = false;
     };
    if (rangeLen == 0)
     {
      range.move("character", 0 - style.length - 3);
      range.collapse();
     }
    else if (tagAdded)
     range.moveStart("character", 0 - rangeLen - 2 * style.length - 5)
    else
     range.moveStart("character", 0 - rangeLen + 2 * style.length + 5);
    range.select();
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    if ((value.substr(start, style.length + 2) == '<' + style + '>') && (value.substr(end - style.length - 3, style.length + 3) == '</' + style + '>'))
     memo.value = value.substr(0, start) + value.substr(start + style.length + 2, end - start - 2 * style.length - 5) + value.substr(end, value.length)
    else // if ((start != 0) || (start != end))
     memo.value = value.substr(0, start) + '<' + style + '>' + value.substr(start, end - start) + '</' + style + '>' + value.substr(end, value.length)
    if (start == end)
     memo.setSelectionRange(start + style.length + 2, start + style.length + 2)
    else if (value.substr(start, style.length + 2) == '<' + style + '>')
     memo.setSelectionRange(start, end - 2 * style.length - 5)
    else
     memo.setSelectionRange(start, 2 * style.length + 5 + end);
   }
  else // Unknown
   memo.value += '<' + style + '></' + style + '>';
 };

function getDateTime ()
 {
  var dateTime = '';
  var now = new Date();
  var temp = now.getDate();
  var tempStr = '' + temp;
  if (tempStr.length < 2) tempStr = '0' + tempStr;
  dateTime = tempStr + '.';
  temp = now.getMonth() + 1;
  tempStr = '' + temp;
  if (tempStr.length < 2) tempStr = '0' + tempStr;
  dateTime = dateTime + tempStr + '.' + now.getFullYear() + ' ';
  temp = now.getHours();
  tempStr = '' + temp;
  if (tempStr.length < 2) tempStr = '0' + tempStr;
  dateTime = dateTime + tempStr + ':';
  temp = now.getMinutes();
  tempStr = '' + temp;
  if (tempStr.length < 2) tempStr = '0' + tempStr;
  dateTime = dateTime + tempStr + ':';
  temp = now.getSeconds();
  tempStr = '' + temp;
  if (tempStr.length < 2) tempStr = '0' + tempStr;
  dateTime = dateTime + tempStr;
  return dateTime;
 };

function getComputedStyleProperty(element, property, asNumber)
 {
  // Get the DOM node if you pass in a string
  element = (typeof element === 'string') ? document.querySelector(element) : element;
  var computedStyle = window.getComputedStyle(element);
  var propertyValue = computedStyle[property];
  if (!propertyValue) propertyValue = element[property];
  if (asNumber === true) propertyValue = parseFloat(propertyValue);
  return propertyValue;
}

// https://stackoverflow.com/a/23749355
function getElementsFullHeight(element)
 {
  // Get the DOM node if you pass in a string
  element = (typeof element === 'string') ? document.querySelector(element) : element;
  var computedStyle = window.getComputedStyle(element);
  var margin = parseFloat(computedStyle['marginTop']) + parseFloat(computedStyle['marginBottom']);
  return Math.ceil(element.offsetHeight + margin);
}

function insertHyperlink (language)
 {
  var hRefCaption = '';
  var titleCaption = '';
  if (language == 'eng')
   {
    hRefCaption = 'Hyperlink\'s URL:';
    titleCaption = 'Hyperlink\'s Title:';
   }
  else
   {
    hRefCaption = 'Адрес ссылки:';
    titleCaption = 'Текст ссылки:';
   };
  var selectedText = '';
  var memo = document.form.posttext;
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    var range = document.selection.createRange();
    selectedText = range.text;
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    selectedText = value.substr(start, end - start);
   };
  var hRefValue = 'http://';
  var titleValue = '';
  if (selectedText.substr(0, 9) == '<a href="')
   {
    hRefValue = selectedText.substr(9, selectedText.length);
    if (hRefValue.indexOf('"') > -1) hRefValue = hRefValue.substr(0, hRefValue.indexOf('"'));
    if (selectedText.indexOf('>') > -1)
     {
      titleValue = selectedText.substr(selectedText.indexOf('>') + 1, selectedText.length);
      if (titleValue.indexOf('</a>') > -1) titleValue = titleValue.substr(0, titleValue.indexOf('</a>'));
     };
   }
  else
   titleValue = selectedText;
  var hRef = prompt(hRefCaption, hRefValue);
  if ((hRef == null) || (hRef == '') || (hRef == hRefValue)) return;
  var target = '';
  if ((hRef.substr(0, 6) == 'ftp://') || (hRef.substr(0, 7) == 'http://') || (hRef.substr(0, 8) == 'https://') || (hRef.substr(0, 4) == 'www.')) target = ' target="_blank"';
  var title = prompt(titleCaption, titleValue);
  if (title == null) return;
  if (title == '') title = hRef;
  selectedText = '<a href="' + hRef + '"' + target + '>' + title + '</a>';
  memo.focus();
  if (memo.createTextRange) // Internet Explorer || Opera 8.0+
   {
    range = document.selection.createRange();
    var rangeLen = range.text.length;
    range.text = selectedText;
    if (rangeLen > 0)
     {
      range.moveStart("character", 0 - selectedText.length);
      range.select();
     };
   }
  else if (document.form.posttext.selectionStart >= 0) // Firefox || Mozilla
   {
    start = memo.selectionStart;
    end = memo.selectionEnd;
    value = memo.value;
    memo.value = value.substr(0, start) + selectedText + value.substr(end, value.length);
    if (start != end) memo.setSelectionRange(start, end + memo.value.length - value.length);
   }
  else // Unknown
   memo.value += selectedText;
 };

function jsPrintF()
 {
  len = arguments.length;
  if (len == 0) return;
  if (len == 1) return arguments[0];
  var result = '';
  var regExStr = '';
  var replStr = '';
  var formatStr = arguments[0];
  var rE;
  for (var i = 1; i < arguments.length; i++)
   {
    replStr += String(i + 100) + arguments[i] + String(i + 100);
    regExStr += String(i + 100) + '(.*)' + String(i + 100);
   };
  rE = new RegExp(regExStr);
  result = replStr.replace(rE, formatStr);
  return result;
 };

function quoteAuthor (number)
 {
  var postHeader = document.getElementsByName('' + number + '')[0].parentNode;
  var postTime = postHeader.getElementsByClassName('post-time')[0].innerText;
  var authorName = postHeader.getElementsByClassName('author-name')[0].innerText;
  var postTimeAuthor = postTime + ' ' + authorName;
  var qText = '';
  if (quoteText != '')
   qText = '' + quoteText + ''
  else
   {
    var text = postHeader.parentNode.getElementsByClassName('post-text')[0].innerHTML;
    qText = replaceSmiles(text);
   }
  var memo = document.form.posttext;
  if (document.selection) // Internet Explorer || Opera 8.0+
   {
    memo.focus();
    if (document.selection.createRange().text == qText)
     formatQuote(postTimeAuthor)
    else
     memo.value += '<blockquote><b>' + postTimeAuthor + '</b><br/>' + qText + '</blockquote>';
   }
  else if (window.getSelection) // Firefox || Mozilla
   {
    memo.focus();
    var start = memo.selectionStart;
    var end = memo.selectionEnd;
    var value = memo.value;
    if ((value.substr(start, end - start) != '') || (qText == ''))
     formatQuote(postTimeAuthor)
    else
     memo.value += '<blockquote><b>' + postTimeAuthor + '</b><br/>' + qText + '</blockquote>';
   }
  else
   formatQuote(postTimeAuthor);
  quoteText = '';
 };

function removeExtraCRLF (s)
 {
  var newS = stringReplace(s, '\r\n\r\n\r\n', '\r\n\r\n');
  newS = stringReplace(newS, '\n\n\n', '\n\n');
  var c = newS.charAt(newS.length - 1);
  while ((c == '\r')||(c == '\n'))
   {
    newS = newS.substring(0, newS.length - 1);
    c = newS.charAt(newS.length - 1);
   };
  return newS;
 };

function replaceSmiles (imgText)
 {
  var text = '';
  var tempText = imgText;
  var lowerText = tempText.toLowerCase();
  var i = lowerText.indexOf('<img ');
  while (i > -1)
   {
    text = text + tempText.substring(0, i);
    tempText = tempText.substring(i, tempText.length);
    lowerText = tempText.toLowerCase();
    i = lowerText.indexOf(' alt=');
    Alt = tempText.substring(i + 5, tempText.length);
    if ((Alt.substring(0, 1) == '"') || (Alt.substring(0, 1) == '\'')) Alt = Alt.substring(1, Alt.length);
    text = text + Alt.substring(0, 3);
    tempText = tempText.substring(i, tempText.length);
    i = tempText.indexOf('>');
    tempText = tempText.substring(i + 1, tempText.length);
    lowerText = tempText.toLowerCase();
    i = lowerText.indexOf('<img ');
   };
  text = text + tempText;
  return text;
 };

function resizeIFrame (iFrame)
 {
  // iFrame.style.height = iFrame.contentWindow.document.body.scrollHeight + 'px';
  if (iFrame.clientWidth >= 330)
   iFrame.style.height = '164px';
  else if (iFrame.clientWidth >= 220)
   iFrame.style.height = '254px';
  else
   iFrame.style.height = '434px';
 };

function resizeMemo (memo)
 {
  var cols = Math.floor((memo.clientWidth - 2) / 8);
  var lines = memo.value.split('\n');
  var linesCount = 0;
  for (var i=0; i < lines.length; i++)
   if (lines[i].length == 0)
    linesCount = linesCount + 1
   else
    linesCount = linesCount + Math.ceil(lines[i].length / cols);
  if (linesCount < 4) linesCount = 4;
  memo.rows = linesCount;
 };

function setHomepage ()
 {
 };

function setHomepageOnClick (a, hRef)
 {
  var currentURL = location.href;
  currentURL = currentURL.substr(currentURL.indexOf('//') + 2, currentURL.length);
  currentURL = currentURL.substr(currentURL.indexOf('/'), currentURL.length);
  if (window.opera && window.print) // Opera
   {
    if (currentURL.indexOf('/eng/') == 0)
     alert('Unfortunately, you Internet browser does not allow to do this automatically AT ALL so you have to do it manually.')
    else
     alert('К сожалению, Ваш Интернет-обозреватель ВООБЩЕ не позволяет сделать это автоматически, поэтому Вам придётся сделать это вручную.');
   }
  else if (window.sidebar) // Firefox
   {
    try
     {
      var prefs = Components.classes['@mozilla.org/preferences-service;1'].getService(Components.interfaces.nsIPrefBranch);
      prefs.setCharPref('browser.startup.homepage', hRef);
     }
    catch(e)
     {
      if (currentURL.indexOf('/eng/') == 0)
       alert('Unfortunately, you Internet browser does not allow to do this automatically DUE TO SECURITY REASONS (this constrain can be removed) so you have to do it manually.')
      else
       alert('К сожалению, Ваш Интернет-обозреватель не позволяет сделать это автоматически ПО СООБРАЖЕНИЯМ БЕЗОПАСНОСТИ (это ограничение можно снять), поэтому Вам придётся сделать это вручную.');
     };
   }
  else if (window.external) // Internet Explorer
   {
    a.style.behavior = 'url(#default#homepage)';
    a.setHomePage(hRef);
   };
 };

function stringReplace (s, oldPattern, newPattern)
 {
  var newS = '';
  var tempS = s;
  var i = tempS.indexOf(oldPattern);
  while (i > -1)
   {
    newS = newS + tempS.substring(0, i) + newPattern;
    tempS = tempS.substring(i + oldPattern.length, tempS.length);
    i = tempS.indexOf(oldPattern);
   };
  newS = newS + tempS;
  return newS;
 };
