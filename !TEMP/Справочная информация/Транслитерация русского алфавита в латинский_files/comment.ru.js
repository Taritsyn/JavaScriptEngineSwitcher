function checkData ()
 {
  if (document.form.authorname.value == '')
   {
    alert('Не заполнено обязательное поле «Имя».');
    document.form.authorname.focus();
    return false;
   };
  if (document.form.captcha.value == '')
   {
    alert('Не заполнено обязательное поле «Результат операции».');
    document.form.captcha.focus();
    return false;
   };
  if ((document.form.posttext.style.color != '') || (document.form.posttext.value == ''))
   {
    alert('Не заполнено обязательное поле «Текст».');
    document.form.posttext.focus();
    return false;
   };
  window.onbeforeunload = null;
  document.form.addbutton.disabled = true;
  return true;
 };

function showPreview ()
 {
  if (checkData())
   {
    document.form.addbutton.disabled = false;
    var postText = removeExtraCRLF(document.form.posttext.value);
    postText = stringReplace(postText, '\r\n', '<br/>');
    postText = stringReplace(postText, '\n', '<br/>');
    if (document.form.postmark)
     {
      var mark = document.form.postmark.value;
      if (mark.substr(0, 7) != 'Оценка ')
       {
        if (mark.substr(0, 1) == '0') mark = mark.substr(1, mark.length);
        postText = postText + '<br/><br/>' + 'Моя оценка: ' + mark.substr(0, mark.indexOf(' – ')) + '/10 (' + mark.substr(mark.indexOf(' – ') + 3, mark.length) + ').';
       };
     };
    top.PreviewWindow = window.open('', 'PreviewWindow', 'width=640, height=480, resizable=yes, menubar=no, toolbar=no, location=no, directories=no, scrollbars=yes, status=no');
    top.PreviewWindow.document.write('<html><head><title>Предпросмотр сообщения</title><link rel="stylesheet" type="text/css" href="/sys/styles/ja_elastica/template.css"/><link rel="stylesheet" type="text/css" href="/sys/styles/stingray.css"/></head><body onload="self.focus();"><div id="post-block"><div class="post-header"><span class="post-time">' + getDateTime() + '</span> <span class="author-name">' + document.form.authorname.value + '</span></div><div class="post-text">' + postText + '</div></div></body></html>');
    top.PreviewWindow.document.close();
   };
 };

window.onbeforeunload = function ()
 {
  postTextArea = document.getElementById('post-text')
  if (postTextArea) postTextArea = postTextArea.childNodes[0];
  if (postTextArea) if (postTextArea.value != '') return postTextArea.value;
 };
