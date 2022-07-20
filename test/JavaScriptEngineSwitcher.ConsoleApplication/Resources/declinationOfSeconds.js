/*!
 *  Copyright 2011 Twitter, Inc.
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */



var Hogan = {};

(function (Hogan) {
  Hogan.Template = function (codeObj, text, compiler, options) {
    codeObj = codeObj || {};
    this.r = codeObj.code || this.r;
    this.c = compiler;
    this.options = options || {};
    this.text = text || '';
    this.partials = codeObj.partials || {};
    this.subs = codeObj.subs || {};
    this.buf = '';
  }

  Hogan.Template.prototype = {
    // render: replaced by generated code.
    r: function (context, partials, indent) { return ''; },

    // variable escaping
    v: hoganEscape,

    // triple stache
    t: coerceToString,

    render: function render(context, partials, indent) {
      return this.ri([context], partials || {}, indent);
    },

    // render internal -- a hook for overrides that catches partials too
    ri: function (context, partials, indent) {
      return this.r(context, partials, indent);
    },

    // ensurePartial
    ep: function(symbol, partials) {
      var partial = this.partials[symbol];

      // check to see that if we've instantiated this partial before
      var template = partials[partial.name];
      if (partial.instance && partial.base == template) {
        return partial.instance;
      }

      if (typeof template == 'string') {
        if (!this.c) {
          throw new Error("No compiler available.");
        }
        template = this.c.compile(template, this.options);
      }

      if (!template) {
        return null;
      }

      // We use this to check whether the partials dictionary has changed
      this.partials[symbol].base = template;

      if (partial.subs) {
        // Make sure we consider parent template now
        if (!partials.stackText) partials.stackText = {};
        for (key in partial.subs) {
          if (!partials.stackText[key]) {
            partials.stackText[key] = (this.activeSub !== undefined && partials.stackText[this.activeSub]) ? partials.stackText[this.activeSub] : this.text;
          }
        }
        template = createSpecializedPartial(template, partial.subs, partial.partials,
          this.stackSubs, this.stackPartials, partials.stackText);
      }
      this.partials[symbol].instance = template;

      return template;
    },

    // tries to find a partial in the current scope and render it
    rp: function(symbol, context, partials, indent) {
      var partial = this.ep(symbol, partials);
      if (!partial) {
        return '';
      }

      return partial.ri(context, partials, indent);
    },

    // render a section
    rs: function(context, partials, section) {
      var tail = context[context.length - 1];

      if (!isArray(tail)) {
        section(context, partials, this);
        return;
      }

      for (var i = 0; i < tail.length; i++) {
        context.push(tail[i]);
        section(context, partials, this);
        context.pop();
      }
    },

    // maybe start a section
    s: function(val, ctx, partials, inverted, start, end, tags) {
      var pass;

      if (isArray(val) && val.length === 0) {
        return false;
      }

      if (typeof val == 'function') {
        val = this.ms(val, ctx, partials, inverted, start, end, tags);
      }

      pass = !!val;

      if (!inverted && pass && ctx) {
        ctx.push((typeof val == 'object') ? val : ctx[ctx.length - 1]);
      }

      return pass;
    },

    // find values with dotted names
    d: function(key, ctx, partials, returnFound) {
      var found,
          names = key.split('.'),
          val = this.f(names[0], ctx, partials, returnFound),
          doModelGet = this.options.modelGet,
          cx = null;

      if (key === '.' && isArray(ctx[ctx.length - 2])) {
        val = ctx[ctx.length - 1];
      } else {
        for (var i = 1; i < names.length; i++) {
          found = findInScope(names[i], val, doModelGet);
          if (found !== undefined) {
            cx = val;
            val = found;
          } else {
            val = '';
          }
        }
      }

      if (returnFound && !val) {
        return false;
      }

      if (!returnFound && typeof val == 'function') {
        ctx.push(cx);
        val = this.mv(val, ctx, partials);
        ctx.pop();
      }

      return val;
    },

    // find values with normal names
    f: function(key, ctx, partials, returnFound) {
      var val = false,
          v = null,
          found = false,
          doModelGet = this.options.modelGet;

      for (var i = ctx.length - 1; i >= 0; i--) {
        v = ctx[i];
        val = findInScope(key, v, doModelGet);
        if (val !== undefined) {
          found = true;
          break;
        }
      }

      if (!found) {
        return (returnFound) ? false : "";
      }

      if (!returnFound && typeof val == 'function') {
        val = this.mv(val, ctx, partials);
      }

      return val;
    },

    // higher order templates
    ls: function(func, cx, partials, text, tags) {
      var oldTags = this.options.delimiters;

      this.options.delimiters = tags;
      this.b(this.ct(coerceToString(func.call(cx, text)), cx, partials));
      this.options.delimiters = oldTags;

      return false;
    },

    // compile text
    ct: function(text, cx, partials) {
      if (this.options.disableLambda) {
        throw new Error('Lambda features disabled.');
      }
      return this.c.compile(text, this.options).render(cx, partials);
    },

    // template result buffering
    b: function(s) { this.buf += s; },

    fl: function() { var r = this.buf; this.buf = ''; return r; },

    // method replace section
    ms: function(func, ctx, partials, inverted, start, end, tags) {
      var textSource,
          cx = ctx[ctx.length - 1],
          result = func.call(cx);

      if (typeof result == 'function') {
        if (inverted) {
          return true;
        } else {
          textSource = (this.activeSub && this.subsText && this.subsText[this.activeSub]) ? this.subsText[this.activeSub] : this.text;
          return this.ls(result, cx, partials, textSource.substring(start, end), tags);
        }
      }

      return result;
    },

    // method replace variable
    mv: function(func, ctx, partials) {
      var cx = ctx[ctx.length - 1];
      var result = func.call(cx);

      if (typeof result == 'function') {
        return this.ct(coerceToString(result.call(cx)), cx, partials);
      }

      return result;
    },

    sub: function(name, context, partials, indent) {
      var f = this.subs[name];
      if (f) {
        this.activeSub = name;
        f(context, partials, this, indent);
        this.activeSub = false;
      }
    }

  };

  //Find a key in an object
  function findInScope(key, scope, doModelGet) {
    var val;

    if (scope && typeof scope == 'object') {

      if (scope[key] !== undefined) {
        val = scope[key];

      // try lookup with get for backbone or similar model data
      } else if (doModelGet && scope.get && typeof scope.get == 'function') {
        val = scope.get(key);
      }
    }

    return val;
  }

  function createSpecializedPartial(instance, subs, partials, stackSubs, stackPartials, stackText) {
    function PartialTemplate() {};
    PartialTemplate.prototype = instance;
    function Substitutions() {};
    Substitutions.prototype = instance.subs;
    var key;
    var partial = new PartialTemplate();
    partial.subs = new Substitutions();
    partial.subsText = {};  //hehe. substext.
    partial.buf = '';

    stackSubs = stackSubs || {};
    partial.stackSubs = stackSubs;
    partial.subsText = stackText;
    for (key in subs) {
      if (!stackSubs[key]) stackSubs[key] = subs[key];
    }
    for (key in stackSubs) {
      partial.subs[key] = stackSubs[key];
    }

    stackPartials = stackPartials || {};
    partial.stackPartials = stackPartials;
    for (key in partials) {
      if (!stackPartials[key]) stackPartials[key] = partials[key];
    }
    for (key in stackPartials) {
      partial.partials[key] = stackPartials[key];
    }

    return partial;
  }

  var rAmp = /&/g,
      rLt = /</g,
      rGt = />/g,
      rApos = /\'/g,
      rQuot = /\"/g,
      hChars = /[&<>\"\']/;

  function coerceToString(val) {
    return String((val === null || val === undefined) ? '' : val);
  }

  function hoganEscape(str) {
    str = coerceToString(str);
    return hChars.test(str) ?
      str
        .replace(rAmp, '&amp;')
        .replace(rLt, '&lt;')
        .replace(rGt, '&gt;')
        .replace(rApos, '&#39;')
        .replace(rQuot, '&quot;') :
      str;
  }

  var isArray = Array.isArray || function(a) {
    return Object.prototype.toString.call(a) === '[object Array]';
  };

})(typeof exports !== 'undefined' ? exports : Hogan);



(function (Hogan) {
  // Setup regex  assignments
  // remove whitespace according to Mustache spec
  var rIsWhitespace = /\S/,
      rQuot = /\"/g,
      rNewline =  /\n/g,
      rCr = /\r/g,
      rSlash = /\\/g,
      rLineSep = /\u2028/,
      rParagraphSep = /\u2029/;

  Hogan.tags = {
    '#': 1, '^': 2, '<': 3, '$': 4,
    '/': 5, '!': 6, '>': 7, '=': 8, '_v': 9,
    '{': 10, '&': 11, '_t': 12
  };

  Hogan.scan = function scan(text, delimiters) {
    var len = text.length,
        IN_TEXT = 0,
        IN_TAG_TYPE = 1,
        IN_TAG = 2,
        state = IN_TEXT,
        tagType = null,
        tag = null,
        buf = '',
        tokens = [],
        seenTag = false,
        i = 0,
        lineStart = 0,
        otag = '{{',
        ctag = '}}';

    function addBuf() {
      if (buf.length > 0) {
        tokens.push({tag: '_t', text: new String(buf)});
        buf = '';
      }
    }

    function lineIsWhitespace() {
      var isAllWhitespace = true;
      for (var j = lineStart; j < tokens.length; j++) {
        isAllWhitespace =
          (Hogan.tags[tokens[j].tag] < Hogan.tags['_v']) ||
          (tokens[j].tag == '_t' && tokens[j].text.match(rIsWhitespace) === null);
        if (!isAllWhitespace) {
          return false;
        }
      }

      return isAllWhitespace;
    }

    function filterLine(haveSeenTag, noNewLine) {
      addBuf();

      if (haveSeenTag && lineIsWhitespace()) {
        for (var j = lineStart, next; j < tokens.length; j++) {
          if (tokens[j].text) {
            if ((next = tokens[j+1]) && next.tag == '>') {
              // set indent to token value
              next.indent = tokens[j].text.toString()
            }
            tokens.splice(j, 1);
          }
        }
      } else if (!noNewLine) {
        tokens.push({tag:'\n'});
      }

      seenTag = false;
      lineStart = tokens.length;
    }

    function changeDelimiters(text, index) {
      var close = '=' + ctag,
          closeIndex = text.indexOf(close, index),
          delimiters = trim(
            text.substring(text.indexOf('=', index) + 1, closeIndex)
          ).split(' ');

      otag = delimiters[0];
      ctag = delimiters[delimiters.length - 1];

      return closeIndex + close.length - 1;
    }

    if (delimiters) {
      delimiters = delimiters.split(' ');
      otag = delimiters[0];
      ctag = delimiters[1];
    }

    for (i = 0; i < len; i++) {
      if (state == IN_TEXT) {
        if (tagChange(otag, text, i)) {
          --i;
          addBuf();
          state = IN_TAG_TYPE;
        } else {
          if (text.charAt(i) == '\n') {
            filterLine(seenTag);
          } else {
            buf += text.charAt(i);
          }
        }
      } else if (state == IN_TAG_TYPE) {
        i += otag.length - 1;
        tag = Hogan.tags[text.charAt(i + 1)];
        tagType = tag ? text.charAt(i + 1) : '_v';
        if (tagType == '=') {
          i = changeDelimiters(text, i);
          state = IN_TEXT;
        } else {
          if (tag) {
            i++;
          }
          state = IN_TAG;
        }
        seenTag = i;
      } else {
        if (tagChange(ctag, text, i)) {
          tokens.push({tag: tagType, n: trim(buf), otag: otag, ctag: ctag,
                       i: (tagType == '/') ? seenTag - otag.length : i + ctag.length});
          buf = '';
          i += ctag.length - 1;
          state = IN_TEXT;
          if (tagType == '{') {
            if (ctag == '}}') {
              i++;
            } else {
              cleanTripleStache(tokens[tokens.length - 1]);
            }
          }
        } else {
          buf += text.charAt(i);
        }
      }
    }

    filterLine(seenTag, true);

    return tokens;
  }

  function cleanTripleStache(token) {
    if (token.n.substr(token.n.length - 1) === '}') {
      token.n = token.n.substring(0, token.n.length - 1);
    }
  }

  function trim(s) {
    if (s.trim) {
      return s.trim();
    }

    return s.replace(/^\s*|\s*$/g, '');
  }

  function tagChange(tag, text, index) {
    if (text.charAt(index) != tag.charAt(0)) {
      return false;
    }

    for (var i = 1, l = tag.length; i < l; i++) {
      if (text.charAt(index + i) != tag.charAt(i)) {
        return false;
      }
    }

    return true;
  }

  // the tags allowed inside super templates
  var allowedInSuper = {'_t': true, '\n': true, '$': true, '/': true};

  function buildTree(tokens, kind, stack, customTags) {
    var instructions = [],
        opener = null,
        tail = null,
        token = null;

    tail = stack[stack.length - 1];

    while (tokens.length > 0) {
      token = tokens.shift();

      if (tail && tail.tag == '<' && !(token.tag in allowedInSuper)) {
        throw new Error('Illegal content in < super tag.');
      }

      if (Hogan.tags[token.tag] <= Hogan.tags['$'] || isOpener(token, customTags)) {
        stack.push(token);
        token.nodes = buildTree(tokens, token.tag, stack, customTags);
      } else if (token.tag == '/') {
        if (stack.length === 0) {
          throw new Error('Closing tag without opener: /' + token.n);
        }
        opener = stack.pop();
        if (token.n != opener.n && !isCloser(token.n, opener.n, customTags)) {
          throw new Error('Nesting error: ' + opener.n + ' vs. ' + token.n);
        }
        opener.end = token.i;
        return instructions;
      } else if (token.tag == '\n') {
        token.last = (tokens.length == 0) || (tokens[0].tag == '\n');
      }

      instructions.push(token);
    }

    if (stack.length > 0) {
      throw new Error('missing closing tag: ' + stack.pop().n);
    }

    return instructions;
  }

  function isOpener(token, tags) {
    for (var i = 0, l = tags.length; i < l; i++) {
      if (tags[i].o == token.n) {
        token.tag = '#';
        return true;
      }
    }
  }

  function isCloser(close, open, tags) {
    for (var i = 0, l = tags.length; i < l; i++) {
      if (tags[i].c == close && tags[i].o == open) {
        return true;
      }
    }
  }

  function stringifySubstitutions(obj) {
    var items = [];
    for (var key in obj) {
      items.push('"' + esc(key) + '": function(c,p,t,i) {' + obj[key] + '}');
    }
    return "{ " + items.join(",") + " }";
  }

  function stringifyPartials(codeObj) {
    var partials = [];
    for (var key in codeObj.partials) {
      partials.push('"' + esc(key) + '":{name:"' + esc(codeObj.partials[key].name) + '", ' + stringifyPartials(codeObj.partials[key]) + "}");
    }
    return "partials: {" + partials.join(",") + "}, subs: " + stringifySubstitutions(codeObj.subs);
  }

  Hogan.stringify = function(codeObj, text, options) {
    return "{code: function (c,p,i) { " + Hogan.wrapMain(codeObj.code) + " }," + stringifyPartials(codeObj) +  "}";
  }

  var serialNo = 0;
  Hogan.generate = function(tree, text, options) {
    serialNo = 0;
    var context = { code: '', subs: {}, partials: {} };
    Hogan.walk(tree, context);

    if (options.asString) {
      return this.stringify(context, text, options);
    }

    return this.makeTemplate(context, text, options);
  }

  Hogan.wrapMain = function(code) {
    return 'var t=this;t.b(i=i||"");' + code + 'return t.fl();';
  }

  Hogan.template = Hogan.Template;

  Hogan.makeTemplate = function(codeObj, text, options) {
    var template = this.makePartials(codeObj);
    template.code = new Function('c', 'p', 'i', this.wrapMain(codeObj.code));
    return new this.template(template, text, this, options);
  }

  Hogan.makePartials = function(codeObj) {
    var key, template = {subs: {}, partials: codeObj.partials, name: codeObj.name};
    for (key in template.partials) {
      template.partials[key] = this.makePartials(template.partials[key]);
    }
    for (key in codeObj.subs) {
      template.subs[key] = new Function('c', 'p', 't', 'i', codeObj.subs[key]);
    }
    return template;
  }

  function esc(s) {
    return s.replace(rSlash, '\\\\')
            .replace(rQuot, '\\\"')
            .replace(rNewline, '\\n')
            .replace(rCr, '\\r')
            .replace(rLineSep, '\\u2028')
            .replace(rParagraphSep, '\\u2029');
  }

  function chooseMethod(s) {
    return (~s.indexOf('.')) ? 'd' : 'f';
  }

  function createPartial(node, context) {
    var prefix = "<" + (context.prefix || "");
    var sym = prefix + node.n + serialNo++;
    context.partials[sym] = {name: node.n, partials: {}};
    context.code += 't.b(t.rp("' +  esc(sym) + '",c,p,"' + (node.indent || '') + '"));';
    return sym;
  }

  Hogan.codegen = {
    '#': function(node, context) {
      context.code += 'if(t.s(t.' + chooseMethod(node.n) + '("' + esc(node.n) + '",c,p,1),' +
                      'c,p,0,' + node.i + ',' + node.end + ',"' + node.otag + " " + node.ctag + '")){' +
                      't.rs(c,p,' + 'function(c,p,t){';
      Hogan.walk(node.nodes, context);
      context.code += '});c.pop();}';
    },

    '^': function(node, context) {
      context.code += 'if(!t.s(t.' + chooseMethod(node.n) + '("' + esc(node.n) + '",c,p,1),c,p,1,0,0,"")){';
      Hogan.walk(node.nodes, context);
      context.code += '};';
    },

    '>': createPartial,
    '<': function(node, context) {
      var ctx = {partials: {}, code: '', subs: {}, inPartial: true};
      Hogan.walk(node.nodes, ctx);
      var template = context.partials[createPartial(node, context)];
      template.subs = ctx.subs;
      template.partials = ctx.partials;
    },

    '$': function(node, context) {
      var ctx = {subs: {}, code: '', partials: context.partials, prefix: node.n};
      Hogan.walk(node.nodes, ctx);
      context.subs[node.n] = ctx.code;
      if (!context.inPartial) {
        context.code += 't.sub("' + esc(node.n) + '",c,p,i);';
      }
    },

    '\n': function(node, context) {
      context.code += write('"\\n"' + (node.last ? '' : ' + i'));
    },

    '_v': function(node, context) {
      context.code += 't.b(t.v(t.' + chooseMethod(node.n) + '("' + esc(node.n) + '",c,p,0)));';
    },

    '_t': function(node, context) {
      context.code += write('"' + esc(node.text) + '"');
    },

    '{': tripleStache,

    '&': tripleStache
  }

  function tripleStache(node, context) {
    context.code += 't.b(t.t(t.' + chooseMethod(node.n) + '("' + esc(node.n) + '",c,p,0)));';
  }

  function write(s) {
    return 't.b(' + s + ');';
  }

  Hogan.walk = function(nodelist, context) {
    var func;
    for (var i = 0, l = nodelist.length; i < l; i++) {
      func = Hogan.codegen[nodelist[i].tag];
      func && func(nodelist[i], context);
    }
    return context;
  }

  Hogan.parse = function(tokens, text, options) {
    options = options || {};
    return buildTree(tokens, '', [], options.sectionTags || []);
  }

  Hogan.cache = {};

  Hogan.cacheKey = function(text, options) {
    return [text, !!options.asString, !!options.disableLambda, options.delimiters, !!options.modelGet].join('||');
  }

  Hogan.compile = function(text, options) {
    options = options || {};
    var key = Hogan.cacheKey(text, options);
    var template = this.cache[key];

    if (template) {
      var partials = template.partials;
      for (var name in partials) {
        delete partials[name].instance;
      }
      return template;
    }

    template = this.generate(this.parse(this.scan(text, options.delimiters), text, options), text, options);
    return this.cache[key] = template;
  }
})(typeof exports !== 'undefined' ? exports : Hogan);

'use strict';var _get=function get(object,property,receiver){if(object===null)object=Function.prototype;var desc=Object.getOwnPropertyDescriptor(object,property);if(desc===undefined){var parent=Object.getPrototypeOf(object);if(parent===null){return undefined;}else{return get(parent,property,receiver);}}else if("value"in desc){return desc.value;}else{var getter=desc.get;if(getter===undefined){return undefined;}return getter.call(receiver);}};var _slicedToArray=function(){function sliceIterator(arr,i){var _arr=[];var _n=true;var _d=false;var _e=undefined;try{for(var _i=arr[Symbol.iterator](),_s;!(_n=(_s=_i.next()).done);_n=true){_arr.push(_s.value);if(i&&_arr.length===i)break;}}catch(err){_d=true;_e=err;}finally{try{if(!_n&&_i["return"])_i["return"]();}finally{if(_d)throw _e;}}return _arr;}return function(arr,i){if(Array.isArray(arr)){return arr;}else if(Symbol.iterator in Object(arr)){return sliceIterator(arr,i);}else{throw new TypeError("Invalid attempt to destructure non-iterable instance");}};}();var _createClass=function(){function defineProperties(target,props){for(var i=0;i<props.length;i++){var descriptor=props[i];descriptor.enumerable=descriptor.enumerable||false;descriptor.configurable=true;if("value"in descriptor)descriptor.writable=true;Object.defineProperty(target,descriptor.key,descriptor);}}return function(Constructor,protoProps,staticProps){if(protoProps)defineProperties(Constructor.prototype,protoProps);if(staticProps)defineProperties(Constructor,staticProps);return Constructor;};}();function _toArray(arr){return Array.isArray(arr)?arr:Array.from(arr);}function _possibleConstructorReturn(self,call){if(!self){throw new ReferenceError("this hasn't been initialised - super() hasn't been called");}return call&&(typeof call==="object"||typeof call==="function")?call:self;}function _inherits(subClass,superClass){if(typeof superClass!=="function"&&superClass!==null){throw new TypeError("Super expression must either be null or a function, not "+typeof superClass);}subClass.prototype=Object.create(superClass&&superClass.prototype,{constructor:{value:subClass,enumerable:false,writable:true,configurable:true}});if(superClass)Object.setPrototypeOf?Object.setPrototypeOf(subClass,superClass):subClass.__proto__=superClass;}function _classCallCheck(instance,Constructor){if(!(instance instanceof Constructor)){throw new TypeError("Cannot call a class as a function");}}function _toConsumableArray(arr){if(Array.isArray(arr)){for(var i=0,arr2=Array(arr.length);i<arr.length;i++){arr2[i]=arr[i];}return arr2;}else{return Array.from(arr);}}/*!
 * Object.assign polyfill
 * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/assign
 */if(!Object.hasOwnProperty('assign')){Object.assign=function(target){var result,argIndex,argCount,nextSource,propName,methodName='Object.assign';if(typeof target==='undefined'||target===null){throw new TypeError(methodName+': argument is not an Object.');}result=Object(target);argCount=arguments.length;for(argIndex=1;argIndex<argCount;argIndex++){nextSource=arguments[argIndex];if(typeof nextSource!=='undefined'&&nextSource!==null){for(propName in nextSource){if(Object.prototype.hasOwnProperty.call(nextSource,propName)){result[propName]=nextSource[propName];}}}}return result;};}/*!
 * CoffeeScript Compiler v2.3.0
 * http://coffeescript.org
 *
 * Copyright 2009-2018 Jeremy Ashkenas
 * Released under the MIT License
 */var CoffeeScript=function(){var modules={},loadedModules={},require=function require(name){var result;if(typeof loadedModules[name]!=='undefined'){result=loadedModules[name];}else{if(typeof modules[name]!=='undefined'){result=modules[name].call(this);loadedModules[name]=typeof result!=='undefined'?result:null;modules[name]=undefined;}else{throw new Error("Can't load '"+name+"' module.");}}return result;};//#region URL: /helpers
modules['/helpers']=function(){var exports={};// This file contains the common helper functions that we'd like to share among
// the **Lexer**, **Rewriter**, and the **Nodes**. Merge objects, flatten
// arrays, count characters, that sort of thing.
// Peek at the beginning of a given string to see if it matches a sequence.
var attachCommentsToNode,buildLocationData,buildLocationHash,extend,_flatten,ref,repeat,syntaxErrorToString;exports.starts=function(string,literal,start){return literal===string.substr(start,literal.length);};// Peek at the end of a given string to see if it matches a sequence.
exports.ends=function(string,literal,back){var len;len=literal.length;return literal===string.substr(string.length-len-(back||0),len);};// Repeat a string `n` times.
exports.repeat=repeat=function repeat(str,n){var res;// Use clever algorithm to have O(log(n)) string concatenation operations.
res='';while(n>0){if(n&1){res+=str;}n>>>=1;str+=str;}return res;};// Trim out all falsy values from an array.
exports.compact=function(array){var i,item,len1,results;results=[];for(i=0,len1=array.length;i<len1;i++){item=array[i];if(item){results.push(item);}}return results;};// Count the number of occurrences of a string in a string.
exports.count=function(string,substr){var num,pos;num=pos=0;if(!substr.length){return 1/0;}while(pos=1+string.indexOf(substr,pos)){num++;}return num;};// Merge objects, returning a fresh copy with attributes from both sides.
// Used every time `Base#compile` is called, to allow properties in the
// options hash to propagate down the tree without polluting other branches.
exports.merge=function(options,overrides){return extend(extend({},options),overrides);};// Extend a source object with the properties of another object (shallow copy).
extend=exports.extend=function(object,properties){var key,val;for(key in properties){val=properties[key];object[key]=val;}return object;};// Return a flattened version of an array.
// Handy for getting a list of `children` from the nodes.
exports.flatten=_flatten=function flatten(array){var element,flattened,i,len1;flattened=[];for(i=0,len1=array.length;i<len1;i++){element=array[i];if('[object Array]'===Object.prototype.toString.call(element)){flattened=flattened.concat(_flatten(element));}else{flattened.push(element);}}return flattened;};// Delete a key from an object, returning the value. Useful when a node is
// looking for a particular method in an options hash.
exports.del=function(obj,key){var val;val=obj[key];delete obj[key];return val;};// Typical Array::some
exports.some=(ref=Array.prototype.some)!=null?ref:function(fn){var e,i,len1,ref1;ref1=this;for(i=0,len1=ref1.length;i<len1;i++){e=ref1[i];if(fn(e)){return true;}}return false;};// Helper function for extracting code from Literate CoffeeScript by stripping
// out all non-code blocks, producing a string of CoffeeScript code that can
// be compiled “normally.”
exports.invertLiterate=function(code){var blankLine,i,indented,insideComment,len1,line,listItemStart,out,ref1;out=[];blankLine=/^\s*$/;indented=/^[\t ]/;listItemStart=/^(?:\t?| {0,3})(?:[\*\-\+]|[0-9]{1,9}\.)[ \t]/;// Up to one tab, or up to three spaces, or neither;
// followed by `*`, `-` or `+`;
// or by an integer up to 9 digits long, followed by a period;
// followed by a space or a tab.
insideComment=false;ref1=code.split('\n');for(i=0,len1=ref1.length;i<len1;i++){line=ref1[i];if(blankLine.test(line)){insideComment=false;out.push(line);}else if(insideComment||listItemStart.test(line)){insideComment=true;out.push('# '+line);}else if(!insideComment&&indented.test(line)){out.push(line);}else{insideComment=true;out.push('# '+line);}}return out.join('\n');};// Merge two jison-style location data objects together.
// If `last` is not provided, this will simply return `first`.
buildLocationData=function buildLocationData(first,last){if(!last){return first;}else{return{first_line:first.first_line,first_column:first.first_column,last_line:last.last_line,last_column:last.last_column};}};buildLocationHash=function buildLocationHash(loc){return loc.first_line+'x'+loc.first_column+'-'+loc.last_line+'x'+loc.last_column;};// This returns a function which takes an object as a parameter, and if that
// object is an AST node, updates that object's locationData.
// The object is returned either way.
exports.addDataToNode=function(parserState,first,last){return function(obj){var i,len1,objHash,ref1,token,tokenHash;// Add location data
if((obj!=null?obj.updateLocationDataIfMissing:void 0)!=null&&first!=null){obj.updateLocationDataIfMissing(buildLocationData(first,last));}// Add comments data
if(!parserState.tokenComments){parserState.tokenComments={};ref1=parserState.parser.tokens;for(i=0,len1=ref1.length;i<len1;i++){token=ref1[i];if(!token.comments){continue;}tokenHash=buildLocationHash(token[2]);if(parserState.tokenComments[tokenHash]==null){parserState.tokenComments[tokenHash]=token.comments;}else{var _parserState$tokenCom;(_parserState$tokenCom=parserState.tokenComments[tokenHash]).push.apply(_parserState$tokenCom,_toConsumableArray(token.comments));}}}if(obj.locationData!=null){objHash=buildLocationHash(obj.locationData);if(parserState.tokenComments[objHash]!=null){attachCommentsToNode(parserState.tokenComments[objHash],obj);}}return obj;};};exports.attachCommentsToNode=attachCommentsToNode=function attachCommentsToNode(comments,node){var _node$comments;if(comments==null||comments.length===0){return;}if(node.comments==null){node.comments=[];}return(_node$comments=node.comments).push.apply(_node$comments,_toConsumableArray(comments));};// Convert jison location data to a string.
// `obj` can be a token, or a locationData.
exports.locationDataToString=function(obj){var locationData;if("2"in obj&&"first_line"in obj[2]){locationData=obj[2];}else if("first_line"in obj){locationData=obj;}if(locationData){return locationData.first_line+1+':'+(locationData.first_column+1)+'-'+(locationData.last_line+1+':'+(locationData.last_column+1));}else{return"No location data";}};// A `.coffee.md` compatible version of `basename`, that returns the file sans-extension.
exports.baseFileName=function(file){var stripExt=arguments.length>1&&arguments[1]!==undefined?arguments[1]:false;var useWinPathSep=arguments.length>2&&arguments[2]!==undefined?arguments[2]:false;var parts,pathSep;pathSep=useWinPathSep?/\\|\//:/\//;parts=file.split(pathSep);file=parts[parts.length-1];if(!(stripExt&&file.indexOf('.')>=0)){return file;}parts=file.split('.');parts.pop();if(parts[parts.length-1]==='coffee'&&parts.length>1){parts.pop();}return parts.join('.');};// Determine if a filename represents a CoffeeScript file.
exports.isCoffee=function(file){return /\.((lit)?coffee|coffee\.md)$/.test(file);};// Determine if a filename represents a Literate CoffeeScript file.
exports.isLiterate=function(file){return /\.(litcoffee|coffee\.md)$/.test(file);};// Throws a SyntaxError from a given location.
// The error's `toString` will return an error message following the "standard"
// format `<filename>:<line>:<col>: <message>` plus the line with the error and a
// marker showing where the error is.
exports.throwSyntaxError=function(message,location){var error;error=new SyntaxError(message);error.location=location;error.toString=syntaxErrorToString;// Instead of showing the compiler's stacktrace, show our custom error message
// (this is useful when the error bubbles up in Node.js applications that
// compile CoffeeScript for example).
error.stack=error.toString();throw error;};// Update a compiler SyntaxError with source code information if it didn't have
// it already.
exports.updateSyntaxError=function(error,code,filename){// Avoid screwing up the `stack` property of other errors (i.e. possible bugs).
if(error.toString===syntaxErrorToString){error.code||(error.code=code);error.filename||(error.filename=filename);error.stack=error.toString();}return error;};syntaxErrorToString=function syntaxErrorToString(){var codeLine,colorize,colorsEnabled,end,filename,first_column,first_line,last_column,last_line,marker,ref1,ref2,ref3,start;if(!(this.code&&this.location)){return Error.prototype.toString.call(this);}var _location=this.location;first_line=_location.first_line;first_column=_location.first_column;last_line=_location.last_line;last_column=_location.last_column;if(last_line==null){last_line=first_line;}if(last_column==null){last_column=first_column;}filename=this.filename||'[stdin]';codeLine=this.code.split('\n')[first_line];start=first_column;// Show only the first line on multi-line errors.
end=first_line===last_line?last_column+1:codeLine.length;marker=codeLine.slice(0,start).replace(/[^\s]/g,' ')+repeat('^',end-start);// Check to see if we're running on a color-enabled TTY.
if(typeof process!=="undefined"&&process!==null){colorsEnabled=((ref1=process.stdout)!=null?ref1.isTTY:void 0)&&!((ref2=process.env)!=null?ref2.NODE_DISABLE_COLORS:void 0);}if((ref3=this.colorful)!=null?ref3:colorsEnabled){colorize=function colorize(str){return'\x1B[1;31m'+str+'\x1B[0m';};codeLine=codeLine.slice(0,start)+colorize(codeLine.slice(start,end))+codeLine.slice(end);marker=colorize(marker);}return filename+':'+(first_line+1)+':'+(first_column+1)+': error: '+this.message+'\n'+codeLine+'\n'+marker;};exports.nameWhitespaceCharacter=function(string){switch(string){case' ':return'space';case'\n':return'newline';case'\r':return'carriage return';case'\t':return'tab';default:return string;}};return exports;};//#endregion
//#region URL: /rewriter
modules['/rewriter']=function(){var exports={};// The CoffeeScript language has a good deal of optional syntax, implicit syntax,
// and shorthand syntax. This can greatly complicate a grammar and bloat
// the resulting parse table. Instead of making the parser handle it all, we take
// a series of passes over the token stream, using this **Rewriter** to convert
// shorthand into the unambiguous long form, add implicit indentation and
// parentheses, and generally clean things up.
var BALANCED_PAIRS,CALL_CLOSERS,CONTROL_IN_IMPLICIT,DISCARDED,EXPRESSION_CLOSE,EXPRESSION_END,EXPRESSION_START,IMPLICIT_CALL,IMPLICIT_END,IMPLICIT_FUNC,IMPLICIT_UNSPACED_CALL,INVERSES,LINEBREAKS,Rewriter,SINGLE_CLOSERS,SINGLE_LINERS,generate,k,left,len,moveComments,right,throwSyntaxError,indexOf=[].indexOf;// Move attached comments from one token to another.
var _require=require('/helpers');throwSyntaxError=_require.throwSyntaxError;moveComments=function moveComments(fromToken,toToken){var comment,k,len,ref,unshiftedComments;if(!fromToken.comments){return;}if(toToken.comments&&toToken.comments.length!==0){unshiftedComments=[];ref=fromToken.comments;for(k=0,len=ref.length;k<len;k++){comment=ref[k];if(comment.unshift){unshiftedComments.push(comment);}else{toToken.comments.push(comment);}}toToken.comments=unshiftedComments.concat(toToken.comments);}else{toToken.comments=fromToken.comments;}return delete fromToken.comments;};// Create a generated token: one that exists due to a use of implicit syntax.
// Optionally have this new token take the attached comments from another token.
generate=function generate(tag,value,origin,commentsToken){var token;token=[tag,value];token.generated=true;if(origin){token.origin=origin;}if(commentsToken){moveComments(commentsToken,token);}return token;};// The **Rewriter** class is used by the [Lexer](lexer.html), directly against
// its internal array of tokens.
exports.Rewriter=Rewriter=function(){var Rewriter=function(){function Rewriter(){_classCallCheck(this,Rewriter);}_createClass(Rewriter,[{key:'rewrite',// Rewrite the token stream in multiple passes, one logical filter at
// a time. This could certainly be changed into a single pass through the
// stream, with a big ol’ efficient switch, but it’s much nicer to work with
// like this. The order of these passes matters—indentation must be
// corrected before implicit parentheses can be wrapped around blocks of code.
value:function rewrite(tokens1){var ref,ref1,t;this.tokens=tokens1;// Set environment variable `DEBUG_TOKEN_STREAM` to `true` to output token
// debugging info. Also set `DEBUG_REWRITTEN_TOKEN_STREAM` to `true` to
// output the token stream after it has been rewritten by this file.
if(typeof process!=="undefined"&&process!==null?(ref=process.env)!=null?ref.DEBUG_TOKEN_STREAM:void 0:void 0){if(process.env.DEBUG_REWRITTEN_TOKEN_STREAM){console.log('Initial token stream:');}console.log(function(){var k,len,ref1,results;ref1=this.tokens;results=[];for(k=0,len=ref1.length;k<len;k++){t=ref1[k];results.push(t[0]+'/'+t[1]+(t.comments?'*':''));}return results;}.call(this).join(' '));}this.removeLeadingNewlines();this.closeOpenCalls();this.closeOpenIndexes();this.normalizeLines();this.tagPostfixConditionals();this.addImplicitBracesAndParens();this.addParensToChainedDoIife();this.rescueStowawayComments();this.addLocationDataToGeneratedTokens();this.enforceValidCSXAttributes();this.fixOutdentLocationData();if(typeof process!=="undefined"&&process!==null?(ref1=process.env)!=null?ref1.DEBUG_REWRITTEN_TOKEN_STREAM:void 0:void 0){if(process.env.DEBUG_TOKEN_STREAM){console.log('Rewritten token stream:');}console.log(function(){var k,len,ref2,results;ref2=this.tokens;results=[];for(k=0,len=ref2.length;k<len;k++){t=ref2[k];results.push(t[0]+'/'+t[1]+(t.comments?'*':''));}return results;}.call(this).join(' '));}return this.tokens;}// Rewrite the token stream, looking one token ahead and behind.
// Allow the return value of the block to tell us how many tokens to move
// forwards (or backwards) in the stream, to make sure we don’t miss anything
// as tokens are inserted and removed, and the stream changes length under
// our feet.
},{key:'scanTokens',value:function scanTokens(block){var i,token,tokens;tokens=this.tokens;i=0;while(token=tokens[i]){i+=block.call(this,token,i,tokens);}return true;}},{key:'detectEnd',value:function detectEnd(i,condition,action){var opts=arguments.length>3&&arguments[3]!==undefined?arguments[3]:{};var levels,ref,ref1,token,tokens;tokens=this.tokens;levels=0;while(token=tokens[i]){if(levels===0&&condition.call(this,token,i)){return action.call(this,token,i);}if(ref=token[0],indexOf.call(EXPRESSION_START,ref)>=0){levels+=1;}else if(ref1=token[0],indexOf.call(EXPRESSION_END,ref1)>=0){levels-=1;}if(levels<0){if(opts.returnOnNegativeLevel){return;}return action.call(this,token,i);}i+=1;}return i-1;}// Leading newlines would introduce an ambiguity in the grammar, so we
// dispatch them here.
},{key:'removeLeadingNewlines',value:function removeLeadingNewlines(){var i,k,l,leadingNewlineToken,len,len1,ref,ref1,tag;ref=this.tokens;for(i=k=0,len=ref.length;k<len;i=++k){var _ref$i=_slicedToArray(ref[i],1);tag=_ref$i[0];if(tag!=='TERMINATOR'){// Find the index of the first non-`TERMINATOR` token.
break;}}if(i===0){return;}ref1=this.tokens.slice(0,i);// If there are any comments attached to the tokens we’re about to discard,
// shift them forward to what will become the new first token.
for(l=0,len1=ref1.length;l<len1;l++){leadingNewlineToken=ref1[l];moveComments(leadingNewlineToken,this.tokens[i]);}// Discard all the leading newline tokens.
return this.tokens.splice(0,i);}// The lexer has tagged the opening parenthesis of a method call. Match it with
// its paired close.
},{key:'closeOpenCalls',value:function closeOpenCalls(){var action,condition;condition=function condition(token,i){var ref;return(ref=token[0])===')'||ref==='CALL_END';};action=function action(token,i){return token[0]='CALL_END';};return this.scanTokens(function(token,i){if(token[0]==='CALL_START'){this.detectEnd(i+1,condition,action);}return 1;});}// The lexer has tagged the opening bracket of an indexing operation call.
// Match it with its paired close.
},{key:'closeOpenIndexes',value:function closeOpenIndexes(){var action,condition;condition=function condition(token,i){var ref;return(ref=token[0])===']'||ref==='INDEX_END';};action=function action(token,i){return token[0]='INDEX_END';};return this.scanTokens(function(token,i){if(token[0]==='INDEX_START'){this.detectEnd(i+1,condition,action);}return 1;});}// Match tags in token stream starting at `i` with `pattern`.
// `pattern` may consist of strings (equality), an array of strings (one of)
// or null (wildcard). Returns the index of the match or -1 if no match.
},{key:'indexOfTag',value:function indexOfTag(i){var fuzz,j,k,ref,ref1;fuzz=0;for(var _len=arguments.length,pattern=Array(_len>1?_len-1:0),_key=1;_key<_len;_key++){pattern[_key-1]=arguments[_key];}for(j=k=0,ref=pattern.length;0<=ref?k<ref:k>ref;j=0<=ref?++k:--k){if(pattern[j]==null){continue;}if(typeof pattern[j]==='string'){pattern[j]=[pattern[j]];}if(ref1=this.tag(i+j+fuzz),indexOf.call(pattern[j],ref1)<0){return-1;}}return i+j+fuzz-1;}// Returns `yes` if standing in front of something looking like
// `@<x>:`, `<x>:` or `<EXPRESSION_START><x>...<EXPRESSION_END>:`.
},{key:'looksObjectish',value:function looksObjectish(j){var end,index;if(this.indexOfTag(j,'@',null,':')!==-1||this.indexOfTag(j,null,':')!==-1){return true;}index=this.indexOfTag(j,EXPRESSION_START);if(index!==-1){end=null;this.detectEnd(index+1,function(token){var ref;return ref=token[0],indexOf.call(EXPRESSION_END,ref)>=0;},function(token,i){return end=i;});if(this.tag(end+1)===':'){return true;}}return false;}// Returns `yes` if current line of tokens contain an element of tags on same
// expression level. Stop searching at `LINEBREAKS` or explicit start of
// containing balanced expression.
},{key:'findTagsBackwards',value:function findTagsBackwards(i,tags){var backStack,ref,ref1,ref2,ref3,ref4,ref5;backStack=[];while(i>=0&&(backStack.length||(ref2=this.tag(i),indexOf.call(tags,ref2)<0)&&((ref3=this.tag(i),indexOf.call(EXPRESSION_START,ref3)<0)||this.tokens[i].generated)&&(ref4=this.tag(i),indexOf.call(LINEBREAKS,ref4)<0))){if(ref=this.tag(i),indexOf.call(EXPRESSION_END,ref)>=0){backStack.push(this.tag(i));}if((ref1=this.tag(i),indexOf.call(EXPRESSION_START,ref1)>=0)&&backStack.length){backStack.pop();}i-=1;}return ref5=this.tag(i),indexOf.call(tags,ref5)>=0;}// Look for signs of implicit calls and objects in the token stream and
// add them.
},{key:'addImplicitBracesAndParens',value:function addImplicitBracesAndParens(){var stack,start;// Track current balancing depth (both implicit and explicit) on stack.
stack=[];start=null;return this.scanTokens(function(token,i,tokens){var _this=this;var endImplicitCall,endImplicitObject,forward,implicitObjectContinues,inControlFlow,inImplicit,inImplicitCall,inImplicitControl,inImplicitObject,isImplicit,isImplicitCall,isImplicitObject,k,newLine,nextTag,nextToken,offset,prevTag,prevToken,ref,ref1,ref2,s,sameLine,stackIdx,stackItem,stackTag,stackTop,startIdx,startImplicitCall,startImplicitObject,startsLine,tag;var _token=_slicedToArray(token,1);tag=_token[0];var _prevToken=prevToken=i>0?tokens[i-1]:[];var _prevToken2=_slicedToArray(_prevToken,1);prevTag=_prevToken2[0];var _nextToken=nextToken=i<tokens.length-1?tokens[i+1]:[];var _nextToken2=_slicedToArray(_nextToken,1);nextTag=_nextToken2[0];stackTop=function stackTop(){return stack[stack.length-1];};startIdx=i;// Helper function, used for keeping track of the number of tokens consumed
// and spliced, when returning for getting a new token.
forward=function forward(n){return i-startIdx+n;};// Helper functions
isImplicit=function isImplicit(stackItem){var ref;return stackItem!=null?(ref=stackItem[2])!=null?ref.ours:void 0:void 0;};isImplicitObject=function isImplicitObject(stackItem){return isImplicit(stackItem)&&(stackItem!=null?stackItem[0]:void 0)==='{';};isImplicitCall=function isImplicitCall(stackItem){return isImplicit(stackItem)&&(stackItem!=null?stackItem[0]:void 0)==='(';};inImplicit=function inImplicit(){return isImplicit(stackTop());};inImplicitCall=function inImplicitCall(){return isImplicitCall(stackTop());};inImplicitObject=function inImplicitObject(){return isImplicitObject(stackTop());};// Unclosed control statement inside implicit parens (like
// class declaration or if-conditionals).
inImplicitControl=function inImplicitControl(){var ref;return inImplicit()&&((ref=stackTop())!=null?ref[0]:void 0)==='CONTROL';};startImplicitCall=function startImplicitCall(idx){stack.push(['(',idx,{ours:true}]);return tokens.splice(idx,0,generate('CALL_START','(',['','implicit function call',token[2]],prevToken));};endImplicitCall=function endImplicitCall(){stack.pop();tokens.splice(i,0,generate('CALL_END',')',['','end of input',token[2]],prevToken));return i+=1;};startImplicitObject=function startImplicitObject(idx){var startsLine=arguments.length>1&&arguments[1]!==undefined?arguments[1]:true;var val;stack.push(['{',idx,{sameLine:true,startsLine:startsLine,ours:true}]);val=new String('{');val.generated=true;return tokens.splice(idx,0,generate('{',val,token,prevToken));};endImplicitObject=function endImplicitObject(j){j=j!=null?j:i;stack.pop();tokens.splice(j,0,generate('}','}',token,prevToken));return i+=1;};implicitObjectContinues=function implicitObjectContinues(j){var nextTerminatorIdx;nextTerminatorIdx=null;_this.detectEnd(j,function(token){return token[0]==='TERMINATOR';},function(token,i){return nextTerminatorIdx=i;},{returnOnNegativeLevel:true});if(nextTerminatorIdx==null){return false;}return _this.looksObjectish(nextTerminatorIdx+1);};// Don’t end an implicit call/object on next indent if any of these are in an argument/value.
if((inImplicitCall()||inImplicitObject())&&indexOf.call(CONTROL_IN_IMPLICIT,tag)>=0||inImplicitObject()&&prevTag===':'&&tag==='FOR'){stack.push(['CONTROL',i,{ours:true}]);return forward(1);}if(tag==='INDENT'&&inImplicit()){// An `INDENT` closes an implicit call unless
//  1. We have seen a `CONTROL` argument on the line.
//  2. The last token before the indent is part of the list below.
if(prevTag!=='=>'&&prevTag!=='->'&&prevTag!=='['&&prevTag!=='('&&prevTag!==','&&prevTag!=='{'&&prevTag!=='ELSE'&&prevTag!=='='){while(inImplicitCall()||inImplicitObject()&&prevTag!==':'){if(inImplicitCall()){endImplicitCall();}else{endImplicitObject();}}}if(inImplicitControl()){stack.pop();}stack.push([tag,i]);return forward(1);}// Straightforward start of explicit expression.
if(indexOf.call(EXPRESSION_START,tag)>=0){stack.push([tag,i]);return forward(1);}// Close all implicit expressions inside of explicitly closed expressions.
if(indexOf.call(EXPRESSION_END,tag)>=0){while(inImplicit()){if(inImplicitCall()){endImplicitCall();}else if(inImplicitObject()){endImplicitObject();}else{stack.pop();}}start=stack.pop();}inControlFlow=function inControlFlow(){var controlFlow,isFunc,seenFor,tagCurrentLine;seenFor=_this.findTagsBackwards(i,['FOR'])&&_this.findTagsBackwards(i,['FORIN','FOROF','FORFROM']);controlFlow=seenFor||_this.findTagsBackwards(i,['WHILE','UNTIL','LOOP','LEADING_WHEN']);if(!controlFlow){return false;}isFunc=false;tagCurrentLine=token[2].first_line;_this.detectEnd(i,function(token,i){var ref;return ref=token[0],indexOf.call(LINEBREAKS,ref)>=0;},function(token,i){var first_line;var _ref=tokens[i-1]||[];var _ref2=_slicedToArray(_ref,3);prevTag=_ref2[0];first_line=_ref2[2].first_line;return isFunc=tagCurrentLine===first_line&&(prevTag==='->'||prevTag==='=>');},{returnOnNegativeLevel:true});return isFunc;};// Recognize standard implicit calls like
// f a, f() b, f? c, h[0] d etc.
// Added support for spread dots on the left side: f ...a
if((indexOf.call(IMPLICIT_FUNC,tag)>=0&&token.spaced||tag==='?'&&i>0&&!tokens[i-1].spaced)&&(indexOf.call(IMPLICIT_CALL,nextTag)>=0||nextTag==='...'&&(ref=this.tag(i+2),indexOf.call(IMPLICIT_CALL,ref)>=0)&&!this.findTagsBackwards(i,['INDEX_START','['])||indexOf.call(IMPLICIT_UNSPACED_CALL,nextTag)>=0&&!nextToken.spaced&&!nextToken.newLine)&&!inControlFlow()){if(tag==='?'){tag=token[0]='FUNC_EXIST';}startImplicitCall(i+1);return forward(2);}// Implicit call taking an implicit indented object as first argument.
//     f
//       a: b
//       c: d
// Don’t accept implicit calls of this type, when on the same line
// as the control structures below as that may misinterpret constructs like:
//     if f
//        a: 1
// as
//     if f(a: 1)
// which is probably always unintended.
// Furthermore don’t allow this in literal arrays, as
// that creates grammatical ambiguities.
if(indexOf.call(IMPLICIT_FUNC,tag)>=0&&this.indexOfTag(i+1,'INDENT')>-1&&this.looksObjectish(i+2)&&!this.findTagsBackwards(i,['CLASS','EXTENDS','IF','CATCH','SWITCH','LEADING_WHEN','FOR','WHILE','UNTIL'])){startImplicitCall(i+1);stack.push(['INDENT',i+2]);return forward(3);}// Implicit objects start here.
if(tag===':'){// Go back to the (implicit) start of the object.
s=function(){var ref1;switch(false){case(ref1=this.tag(i-1),indexOf.call(EXPRESSION_END,ref1)<0):return start[1];case this.tag(i-2)!=='@':return i-2;default:return i-1;}}.call(this);startsLine=s<=0||(ref1=this.tag(s-1),indexOf.call(LINEBREAKS,ref1)>=0)||tokens[s-1].newLine;// Are we just continuing an already declared object?
if(stackTop()){var _stackTop=stackTop();var _stackTop2=_slicedToArray(_stackTop,2);stackTag=_stackTop2[0];stackIdx=_stackTop2[1];if((stackTag==='{'||stackTag==='INDENT'&&this.tag(stackIdx-1)==='{')&&(startsLine||this.tag(s-1)===','||this.tag(s-1)==='{')){return forward(1);}}startImplicitObject(s,!!startsLine);return forward(2);}// End implicit calls when chaining method calls
// like e.g.:
//     f ->
//       a
//     .g b, ->
//       c
//     .h a
// and also
//     f a
//     .g b
//     .h a
// Mark all enclosing objects as not sameLine
if(indexOf.call(LINEBREAKS,tag)>=0){for(k=stack.length-1;k>=0;k+=-1){stackItem=stack[k];if(!isImplicit(stackItem)){break;}if(isImplicitObject(stackItem)){stackItem[2].sameLine=false;}}}newLine=prevTag==='OUTDENT'||prevToken.newLine;if(indexOf.call(IMPLICIT_END,tag)>=0||indexOf.call(CALL_CLOSERS,tag)>=0&&newLine||(tag==='..'||tag==='...')&&this.findTagsBackwards(i,["INDEX_START"])){while(inImplicit()){// Close implicit calls when reached end of argument list
var _stackTop3=stackTop();var _stackTop4=_slicedToArray(_stackTop3,3);stackTag=_stackTop4[0];stackIdx=_stackTop4[1];var _stackTop4$=_stackTop4[2];sameLine=_stackTop4$.sameLine;startsLine=_stackTop4$.startsLine;if(inImplicitCall()&&prevTag!==','||prevTag===','&&tag==='TERMINATOR'&&nextTag==null){endImplicitCall();// Close implicit objects such as:
// return a: 1, b: 2 unless true
}else if(inImplicitObject()&&sameLine&&tag!=='TERMINATOR'&&prevTag!==':'&&!((tag==='POST_IF'||tag==='FOR'||tag==='WHILE'||tag==='UNTIL')&&startsLine&&implicitObjectContinues(i+1))){endImplicitObject();// Close implicit objects when at end of line, line didn't end with a comma
// and the implicit object didn't start the line or the next line doesn’t look like
// the continuation of an object.
}else if(inImplicitObject()&&tag==='TERMINATOR'&&prevTag!==','&&!(startsLine&&this.looksObjectish(i+1))){endImplicitObject();}else{break;}}}// Close implicit object if comma is the last character
// and what comes after doesn’t look like it belongs.
// This is used for trailing commas and calls, like:
//     x =
//         a: b,
//         c: d,
//     e = 2
// and
//     f a, b: c, d: e, f, g: h: i, j
if(tag===','&&!this.looksObjectish(i+1)&&inImplicitObject()&&!((ref2=this.tag(i+2))==='FOROF'||ref2==='FORIN')&&(nextTag!=='TERMINATOR'||!this.looksObjectish(i+2))){// When nextTag is OUTDENT the comma is insignificant and
// should just be ignored so embed it in the implicit object.
// When it isn’t the comma go on to play a role in a call or
// array further up the stack, so give it a chance.
offset=nextTag==='OUTDENT'?1:0;while(inImplicitObject()){endImplicitObject(i+offset);}}return forward(1);});}// Make sure only strings and wrapped expressions are used in CSX attributes.
},{key:'enforceValidCSXAttributes',value:function enforceValidCSXAttributes(){return this.scanTokens(function(token,i,tokens){var next,ref;if(token.csxColon){next=tokens[i+1];if((ref=next[0])!=='STRING_START'&&ref!=='STRING'&&ref!=='('){throwSyntaxError('expected wrapped or quoted JSX attribute',next[2]);}}return 1;});}// Not all tokens survive processing by the parser. To avoid comments getting
// lost into the ether, find comments attached to doomed tokens and move them
// to a token that will make it to the other side.
},{key:'rescueStowawayComments',value:function rescueStowawayComments(){var insertPlaceholder,shiftCommentsBackward,shiftCommentsForward;insertPlaceholder=function insertPlaceholder(token,j,tokens,method){if(tokens[j][0]!=='TERMINATOR'){tokens[method](generate('TERMINATOR','\n',tokens[j]));}return tokens[method](generate('JS','',tokens[j],token));};shiftCommentsForward=function shiftCommentsForward(token,i,tokens){var comment,j,k,len,ref,ref1,ref2;// Find the next surviving token and attach this token’s comments to it,
// with a flag that we know to output such comments *before* that
// token’s own compilation. (Otherwise comments are output following
// the token they’re attached to.)
j=i;while(j!==tokens.length&&(ref=tokens[j][0],indexOf.call(DISCARDED,ref)>=0)){j++;}if(!(j===tokens.length||(ref1=tokens[j][0],indexOf.call(DISCARDED,ref1)>=0))){ref2=token.comments;for(k=0,len=ref2.length;k<len;k++){comment=ref2[k];comment.unshift=true;}moveComments(token,tokens[j]);return 1;// All following tokens are doomed!
}else{j=tokens.length-1;insertPlaceholder(token,j,tokens,'push');// The generated tokens were added to the end, not inline, so we don’t skip.
return 1;}};shiftCommentsBackward=function shiftCommentsBackward(token,i,tokens){var j,ref,ref1;// Find the last surviving token and attach this token’s comments to it.
j=i;while(j!==-1&&(ref=tokens[j][0],indexOf.call(DISCARDED,ref)>=0)){j--;}if(!(j===-1||(ref1=tokens[j][0],indexOf.call(DISCARDED,ref1)>=0))){moveComments(token,tokens[j]);return 1;// All previous tokens are doomed!
}else{insertPlaceholder(token,0,tokens,'unshift');// We added two tokens, so shift forward to account for the insertion.
return 3;}};return this.scanTokens(function(token,i,tokens){var dummyToken,j,ref,ref1,ret;if(!token.comments){return 1;}ret=1;if(ref=token[0],indexOf.call(DISCARDED,ref)>=0){// This token won’t survive passage through the parser, so we need to
// rescue its attached tokens and redistribute them to nearby tokens.
// Comments that don’t start a new line can shift backwards to the last
// safe token, while other tokens should shift forward.
dummyToken={comments:[]};j=token.comments.length-1;while(j!==-1){if(token.comments[j].newLine===false&&token.comments[j].here===false){dummyToken.comments.unshift(token.comments[j]);token.comments.splice(j,1);}j--;}if(dummyToken.comments.length!==0){ret=shiftCommentsBackward(dummyToken,i-1,tokens);}if(token.comments.length!==0){shiftCommentsForward(token,i,tokens);}}else{// If any of this token’s comments start a line—there’s only
// whitespace between the preceding newline and the start of the
// comment—and this isn’t one of the special `JS` tokens, then
// shift this comment forward to precede the next valid token.
// `Block.compileComments` also has logic to make sure that
// “starting new line” comments follow or precede the nearest
// newline relative to the token that the comment is attached to,
// but that newline might be inside a `}` or `)` or other generated
// token that we really want this comment to output after. Therefore
// we need to shift the comments here, avoiding such generated and
// discarded tokens.
dummyToken={comments:[]};j=token.comments.length-1;while(j!==-1){if(token.comments[j].newLine&&!token.comments[j].unshift&&!(token[0]==='JS'&&token.generated)){dummyToken.comments.unshift(token.comments[j]);token.comments.splice(j,1);}j--;}if(dummyToken.comments.length!==0){ret=shiftCommentsForward(dummyToken,i+1,tokens);}}if(((ref1=token.comments)!=null?ref1.length:void 0)===0){delete token.comments;}return ret;});}// Add location data to all tokens generated by the rewriter.
},{key:'addLocationDataToGeneratedTokens',value:function addLocationDataToGeneratedTokens(){return this.scanTokens(function(token,i,tokens){var column,line,nextLocation,prevLocation,ref,ref1;if(token[2]){return 1;}if(!(token.generated||token.explicit)){return 1;}if(token[0]==='{'&&(nextLocation=(ref=tokens[i+1])!=null?ref[2]:void 0)){var _nextLocation=nextLocation;line=_nextLocation.first_line;column=_nextLocation.first_column;}else if(prevLocation=(ref1=tokens[i-1])!=null?ref1[2]:void 0){var _prevLocation=prevLocation;line=_prevLocation.last_line;column=_prevLocation.last_column;}else{line=column=0;}token[2]={first_line:line,first_column:column,last_line:line,last_column:column};return 1;});}// `OUTDENT` tokens should always be positioned at the last character of the
// previous token, so that AST nodes ending in an `OUTDENT` token end up with a
// location corresponding to the last “real” token under the node.
},{key:'fixOutdentLocationData',value:function fixOutdentLocationData(){return this.scanTokens(function(token,i,tokens){var prevLocationData;if(!(token[0]==='OUTDENT'||token.generated&&token[0]==='CALL_END'||token.generated&&token[0]==='}')){return 1;}prevLocationData=tokens[i-1][2];token[2]={first_line:prevLocationData.last_line,first_column:prevLocationData.last_column,last_line:prevLocationData.last_line,last_column:prevLocationData.last_column};return 1;});}// Add parens around a `do` IIFE followed by a chained `.` so that the
// chaining applies to the executed function rather than the function
// object (see #3736)
},{key:'addParensToChainedDoIife',value:function addParensToChainedDoIife(){var action,condition,doIndex;condition=function condition(token,i){return this.tag(i-1)==='OUTDENT';};action=function action(token,i){var ref;if(ref=token[0],indexOf.call(CALL_CLOSERS,ref)<0){return;}this.tokens.splice(doIndex,0,generate('(','(',this.tokens[doIndex]));return this.tokens.splice(i+1,0,generate(')',')',this.tokens[i]));};doIndex=null;return this.scanTokens(function(token,i,tokens){var glyphIndex,ref;if(token[1]!=='do'){return 1;}doIndex=i;glyphIndex=i+1;if(this.tag(i+1)==='PARAM_START'){glyphIndex=null;this.detectEnd(i+1,function(token,i){return this.tag(i-1)==='PARAM_END';},function(token,i){return glyphIndex=i;});}if(!(glyphIndex!=null&&((ref=this.tag(glyphIndex))==='->'||ref==='=>')&&this.tag(glyphIndex+1)==='INDENT')){return 1;}this.detectEnd(glyphIndex+1,condition,action);return 2;});}// Because our grammar is LALR(1), it can’t handle some single-line
// expressions that lack ending delimiters. The **Rewriter** adds the implicit
// blocks, so it doesn’t need to. To keep the grammar clean and tidy, trailing
// newlines within expressions are removed and the indentation tokens of empty
// blocks are added.
},{key:'normalizeLines',value:function normalizeLines(){var _this2=this;var action,closeElseTag,condition,ifThens,indent,leading_if_then,leading_switch_when,outdent,starter;starter=indent=outdent=null;leading_switch_when=null;leading_if_then=null;// Count `THEN` tags
ifThens=[];condition=function condition(token,i){var ref,ref1,ref2,ref3;return token[1]!==';'&&(ref=token[0],indexOf.call(SINGLE_CLOSERS,ref)>=0)&&!(token[0]==='TERMINATOR'&&(ref1=this.tag(i+1),indexOf.call(EXPRESSION_CLOSE,ref1)>=0))&&!(token[0]==='ELSE'&&(starter!=='THEN'||leading_if_then||leading_switch_when))&&!(((ref2=token[0])==='CATCH'||ref2==='FINALLY')&&(starter==='->'||starter==='=>'))||(ref3=token[0],indexOf.call(CALL_CLOSERS,ref3)>=0)&&(this.tokens[i-1].newLine||this.tokens[i-1][0]==='OUTDENT');};action=function action(token,i){if(token[0]==='ELSE'&&starter==='THEN'){ifThens.pop();}return this.tokens.splice(this.tag(i-1)===','?i-1:i,0,outdent);};closeElseTag=function closeElseTag(tokens,i){var lastThen,outdentElse,tlen;tlen=ifThens.length;if(!(tlen>0)){return i;}lastThen=ifThens.pop();// Insert `OUTDENT` to close inner `IF`.
var _indentation=_this2.indentation(tokens[lastThen]);var _indentation2=_slicedToArray(_indentation,2);outdentElse=_indentation2[1];outdentElse[1]=tlen*2;tokens.splice(i,0,outdentElse);// Insert `OUTDENT` to close outer `IF`.
outdentElse[1]=2;tokens.splice(i+1,0,outdentElse);// Remove outdents from the end.
_this2.detectEnd(i+2,function(token,i){var ref;return(ref=token[0])==='OUTDENT'||ref==='TERMINATOR';},function(token,i){if(this.tag(i)==='OUTDENT'&&this.tag(i+1)==='OUTDENT'){return tokens.splice(i,2);}});return i+2;};return this.scanTokens(function(token,i,tokens){var conditionTag,j,k,ref,ref1,tag;var _token2=_slicedToArray(token,1);tag=_token2[0];conditionTag=(tag==='->'||tag==='=>')&&this.findTagsBackwards(i,['IF','WHILE','FOR','UNTIL','SWITCH','WHEN','LEADING_WHEN','[','INDEX_START'])&&!this.findTagsBackwards(i,['THEN','..','...']);if(tag==='TERMINATOR'){if(this.tag(i+1)==='ELSE'&&this.tag(i-1)!=='OUTDENT'){tokens.splice.apply(tokens,[i,1].concat(_toConsumableArray(this.indentation())));return 1;}if(ref=this.tag(i+1),indexOf.call(EXPRESSION_CLOSE,ref)>=0){tokens.splice(i,1);return 0;}}if(tag==='CATCH'){for(j=k=1;k<=2;j=++k){if(!((ref1=this.tag(i+j))==='OUTDENT'||ref1==='TERMINATOR'||ref1==='FINALLY')){continue;}tokens.splice.apply(tokens,[i+j,0].concat(_toConsumableArray(this.indentation())));return 2+j;}}if((tag==='->'||tag==='=>')&&(this.tag(i+1)===','||this.tag(i+1)==='.'&&token.newLine)){var _indentation3=this.indentation(tokens[i]);var _indentation4=_slicedToArray(_indentation3,2);indent=_indentation4[0];outdent=_indentation4[1];tokens.splice(i+1,0,indent,outdent);return 1;}if(indexOf.call(SINGLE_LINERS,tag)>=0&&this.tag(i+1)!=='INDENT'&&!(tag==='ELSE'&&this.tag(i+1)==='IF')&&!conditionTag){starter=tag;var _indentation5=this.indentation(tokens[i]);var _indentation6=_slicedToArray(_indentation5,2);indent=_indentation6[0];outdent=_indentation6[1];if(starter==='THEN'){indent.fromThen=true;}if(tag==='THEN'){leading_switch_when=this.findTagsBackwards(i,['LEADING_WHEN'])&&this.tag(i+1)==='IF';leading_if_then=this.findTagsBackwards(i,['IF'])&&this.tag(i+1)==='IF';}if(tag==='THEN'&&this.findTagsBackwards(i,['IF'])){ifThens.push(i);}// `ELSE` tag is not closed.
if(tag==='ELSE'&&this.tag(i-1)!=='OUTDENT'){i=closeElseTag(tokens,i);}tokens.splice(i+1,0,indent);this.detectEnd(i+2,condition,action);if(tag==='THEN'){tokens.splice(i,1);}return 1;}return 1;});}// Tag postfix conditionals as such, so that we can parse them with a
// different precedence.
},{key:'tagPostfixConditionals',value:function tagPostfixConditionals(){var action,condition,original;original=null;condition=function condition(token,i){var prevTag,tag;var _token3=_slicedToArray(token,1);tag=_token3[0];var _tokens=_slicedToArray(this.tokens[i-1],1);prevTag=_tokens[0];return tag==='TERMINATOR'||tag==='INDENT'&&indexOf.call(SINGLE_LINERS,prevTag)<0;};action=function action(token,i){if(token[0]!=='INDENT'||token.generated&&!token.fromThen){return original[0]='POST_'+original[0];}};return this.scanTokens(function(token,i){if(token[0]!=='IF'){return 1;}original=token;this.detectEnd(i+1,condition,action);return 1;});}// Generate the indentation tokens, based on another token on the same line.
},{key:'indentation',value:function indentation(origin){var indent,outdent;indent=['INDENT',2];outdent=['OUTDENT',2];if(origin){indent.generated=outdent.generated=true;indent.origin=outdent.origin=origin;}else{indent.explicit=outdent.explicit=true;}return[indent,outdent];}// Look up a tag by token index.
},{key:'tag',value:function tag(i){var ref;return(ref=this.tokens[i])!=null?ref[0]:void 0;}}]);return Rewriter;}();;Rewriter.prototype.generate=generate;return Rewriter;}.call(this);// Constants
// ---------
// List of the token pairs that must be balanced.
BALANCED_PAIRS=[['(',')'],['[',']'],['{','}'],['INDENT','OUTDENT'],['CALL_START','CALL_END'],['PARAM_START','PARAM_END'],['INDEX_START','INDEX_END'],['STRING_START','STRING_END'],['REGEX_START','REGEX_END']];// The inverse mappings of `BALANCED_PAIRS` we’re trying to fix up, so we can
// look things up from either end.
exports.INVERSES=INVERSES={};// The tokens that signal the start/end of a balanced pair.
EXPRESSION_START=[];EXPRESSION_END=[];for(k=0,len=BALANCED_PAIRS.length;k<len;k++){var _BALANCED_PAIRS$k=_slicedToArray(BALANCED_PAIRS[k],2);left=_BALANCED_PAIRS$k[0];right=_BALANCED_PAIRS$k[1];EXPRESSION_START.push(INVERSES[right]=left);EXPRESSION_END.push(INVERSES[left]=right);}// Tokens that indicate the close of a clause of an expression.
EXPRESSION_CLOSE=['CATCH','THEN','ELSE','FINALLY'].concat(EXPRESSION_END);// Tokens that, if followed by an `IMPLICIT_CALL`, indicate a function invocation.
IMPLICIT_FUNC=['IDENTIFIER','PROPERTY','SUPER',')','CALL_END',']','INDEX_END','@','THIS'];// If preceded by an `IMPLICIT_FUNC`, indicates a function invocation.
IMPLICIT_CALL=['IDENTIFIER','CSX_TAG','PROPERTY','NUMBER','INFINITY','NAN','STRING','STRING_START','REGEX','REGEX_START','JS','NEW','PARAM_START','CLASS','IF','TRY','SWITCH','THIS','UNDEFINED','NULL','BOOL','UNARY','YIELD','AWAIT','UNARY_MATH','SUPER','THROW','@','->','=>','[','(','{','--','++'];IMPLICIT_UNSPACED_CALL=['+','-'];// Tokens that always mark the end of an implicit call for single-liners.
IMPLICIT_END=['POST_IF','FOR','WHILE','UNTIL','WHEN','BY','LOOP','TERMINATOR'];// Single-line flavors of block expressions that have unclosed endings.
// The grammar can’t disambiguate them, so we insert the implicit indentation.
SINGLE_LINERS=['ELSE','->','=>','TRY','FINALLY','THEN'];SINGLE_CLOSERS=['TERMINATOR','CATCH','FINALLY','ELSE','OUTDENT','LEADING_WHEN'];// Tokens that end a line.
LINEBREAKS=['TERMINATOR','INDENT','OUTDENT'];// Tokens that close open calls when they follow a newline.
CALL_CLOSERS=['.','?.','::','?::'];// Tokens that prevent a subsequent indent from ending implicit calls/objects
CONTROL_IN_IMPLICIT=['IF','TRY','FINALLY','CATCH','CLASS','SWITCH'];// Tokens that are swallowed up by the parser, never leading to code generation.
// You can spot these in `grammar.coffee` because the `o` function second
// argument doesn’t contain a `new` call for these tokens.
// `STRING_START` isn’t on this list because its `locationData` matches that of
// the node that becomes `StringWithInterpolations`, and therefore
// `addDataToNode` attaches `STRING_START`’s tokens to that node.
DISCARDED=['(',')','[',']','{','}','.','..','...',',','=','++','--','?','AS','AWAIT','CALL_START','CALL_END','DEFAULT','ELSE','EXTENDS','EXPORT','FORIN','FOROF','FORFROM','IMPORT','INDENT','INDEX_SOAK','LEADING_WHEN','OUTDENT','PARAM_END','REGEX_START','REGEX_END','RETURN','STRING_END','THROW','UNARY','YIELD'].concat(IMPLICIT_UNSPACED_CALL.concat(IMPLICIT_END.concat(CALL_CLOSERS.concat(CONTROL_IN_IMPLICIT))));return exports;};//#endregion
//#region URL: /lexer
modules['/lexer']=function(){var exports={};// The CoffeeScript Lexer. Uses a series of token-matching regexes to attempt
// matches against the beginning of the source code. When a match is found,
// a token is produced, we consume the match, and start again. Tokens are in the
// form:
//     [tag, value, locationData]
// where locationData is {first_line, first_column, last_line, last_column}, which is a
// format that can be fed directly into [Jison](https://github.com/zaach/jison).  These
// are read by jison in the `parser.lexer` function defined in coffeescript.coffee.
var BOM,BOOL,CALLABLE,CODE,COFFEE_ALIASES,COFFEE_ALIAS_MAP,COFFEE_KEYWORDS,COMMENT,COMPARABLE_LEFT_SIDE,COMPARE,COMPOUND_ASSIGN,CSX_ATTRIBUTE,CSX_FRAGMENT_IDENTIFIER,CSX_IDENTIFIER,CSX_INTERPOLATION,HERECOMMENT_ILLEGAL,HEREDOC_DOUBLE,HEREDOC_INDENT,HEREDOC_SINGLE,HEREGEX,HEREGEX_OMIT,HERE_JSTOKEN,IDENTIFIER,INDENTABLE_CLOSERS,INDEXABLE,INSIDE_CSX,INVERSES,JSTOKEN,JS_KEYWORDS,LEADING_BLANK_LINE,LINE_BREAK,LINE_CONTINUER,Lexer,MATH,MULTI_DENT,NOT_REGEX,NUMBER,OPERATOR,POSSIBLY_DIVISION,REGEX,REGEX_FLAGS,REGEX_ILLEGAL,REGEX_INVALID_ESCAPE,RELATION,RESERVED,Rewriter,SHIFT,SIMPLE_STRING_OMIT,STRICT_PROSCRIBED,STRING_DOUBLE,STRING_INVALID_ESCAPE,STRING_OMIT,STRING_SINGLE,STRING_START,TRAILING_BLANK_LINE,TRAILING_SPACES,UNARY,UNARY_MATH,UNFINISHED,UNICODE_CODE_POINT_ESCAPE,VALID_FLAGS,WHITESPACE,attachCommentsToNode,compact,count,invertLiterate,isForFrom,isUnassignable,key,locationDataToString,merge,repeat,starts,throwSyntaxError,indexOf=[].indexOf,slice=[].slice;// Import the helpers we need.
var _require2=require('/rewriter');Rewriter=_require2.Rewriter;INVERSES=_require2.INVERSES;// The Lexer Class
// ---------------
// The Lexer class reads a stream of CoffeeScript and divvies it up into tagged
// tokens. Some potential ambiguity in the grammar has been avoided by
// pushing some extra smarts into the Lexer.
var _require3=require('/helpers');count=_require3.count;starts=_require3.starts;compact=_require3.compact;repeat=_require3.repeat;invertLiterate=_require3.invertLiterate;merge=_require3.merge;attachCommentsToNode=_require3.attachCommentsToNode;locationDataToString=_require3.locationDataToString;throwSyntaxError=_require3.throwSyntaxError;exports.Lexer=Lexer=function(){function Lexer(){_classCallCheck(this,Lexer);}_createClass(Lexer,[{key:'tokenize',// **tokenize** is the Lexer's main method. Scan by attempting to match tokens
// one at a time, using a regular expression anchored at the start of the
// remaining code, or a custom recursive token-matching method
// (for interpolations). When the next token has been recorded, we move forward
// within the code past the token, and begin again.
// Each tokenizing method is responsible for returning the number of characters
// it has consumed.
// Before returning the token stream, run it through the [Rewriter](rewriter.html).
value:function tokenize(code){var opts=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var consumed,end,i,ref;this.literate=opts.literate;// Are we lexing literate CoffeeScript?
this.indent=0;// The current indentation level.
this.baseIndent=0;// The overall minimum indentation level.
this.indebt=0;// The over-indentation at the current level.
this.outdebt=0;// The under-outdentation at the current level.
this.indents=[];// The stack of all current indentation levels.
this.indentLiteral='';// The indentation.
this.ends=[];// The stack for pairing up tokens.
this.tokens=[];// Stream of parsed tokens in the form `['TYPE', value, location data]`.
this.seenFor=false;// Used to recognize `FORIN`, `FOROF` and `FORFROM` tokens.
this.seenImport=false;// Used to recognize `IMPORT FROM? AS?` tokens.
this.seenExport=false;// Used to recognize `EXPORT FROM? AS?` tokens.
this.importSpecifierList=false;// Used to identify when in an `IMPORT {...} FROM? ...`.
this.exportSpecifierList=false;// Used to identify when in an `EXPORT {...} FROM? ...`.
this.csxDepth=0;// Used to optimize CSX checks, how deep in CSX we are.
this.csxObjAttribute={};// Used to detect if CSX attributes is wrapped in {} (<div {props...} />).
this.chunkLine=opts.line||0;// The start line for the current @chunk.
this.chunkColumn=opts.column||0;// The start column of the current @chunk.
code=this.clean(code);// The stripped, cleaned original source code.
// At every position, run through this list of attempted matches,
// short-circuiting if any of them succeed. Their order determines precedence:
// `@literalToken` is the fallback catch-all.
i=0;while(this.chunk=code.slice(i)){consumed=this.identifierToken()||this.commentToken()||this.whitespaceToken()||this.lineToken()||this.stringToken()||this.numberToken()||this.csxToken()||this.regexToken()||this.jsToken()||this.literalToken();// Update position.
var _getLineAndColumnFrom=this.getLineAndColumnFromChunk(consumed);var _getLineAndColumnFrom2=_slicedToArray(_getLineAndColumnFrom,2);this.chunkLine=_getLineAndColumnFrom2[0];this.chunkColumn=_getLineAndColumnFrom2[1];i+=consumed;if(opts.untilBalanced&&this.ends.length===0){return{tokens:this.tokens,index:i};}}this.closeIndentation();if(end=this.ends.pop()){this.error('missing '+end.tag,((ref=end.origin)!=null?ref:end)[2]);}if(opts.rewrite===false){return this.tokens;}return new Rewriter().rewrite(this.tokens);}// Preprocess the code to remove leading and trailing whitespace, carriage
// returns, etc. If we’re lexing literate CoffeeScript, strip external Markdown
// by removing all lines that aren’t indented by at least four spaces or a tab.
},{key:'clean',value:function clean(code){if(code.charCodeAt(0)===BOM){code=code.slice(1);}code=code.replace(/\r/g,'').replace(TRAILING_SPACES,'');if(WHITESPACE.test(code)){code='\n'+code;this.chunkLine--;}if(this.literate){code=invertLiterate(code);}return code;}// Tokenizers
// ----------
// Matches identifying literals: variables, keywords, method names, etc.
// Check to ensure that JavaScript reserved words aren’t being used as
// identifiers. Because CoffeeScript reserves a handful of keywords that are
// allowed in JavaScript, we’re careful not to tag them as keywords when
// referenced as property names here, so you can still do `jQuery.is()` even
// though `is` means `===` otherwise.
},{key:'identifierToken',value:function identifierToken(){var alias,colon,colonOffset,colonToken,id,idLength,inCSXTag,input,match,poppedToken,prev,prevprev,ref,ref1,ref10,ref2,ref3,ref4,ref5,ref6,ref7,ref8,ref9,regExSuper,regex,sup,tag,tagToken;inCSXTag=this.atCSXTag();regex=inCSXTag?CSX_ATTRIBUTE:IDENTIFIER;if(!(match=regex.exec(this.chunk))){return 0;}// Preserve length of id for location data
var _match=match;var _match2=_slicedToArray(_match,3);input=_match2[0];id=_match2[1];colon=_match2[2];idLength=id.length;poppedToken=void 0;if(id==='own'&&this.tag()==='FOR'){this.token('OWN',id);return id.length;}if(id==='from'&&this.tag()==='YIELD'){this.token('FROM',id);return id.length;}if(id==='as'&&this.seenImport){if(this.value()==='*'){this.tokens[this.tokens.length-1][0]='IMPORT_ALL';}else if(ref=this.value(true),indexOf.call(COFFEE_KEYWORDS,ref)>=0){prev=this.prev();var _ref3=['IDENTIFIER',this.value(true)];prev[0]=_ref3[0];prev[1]=_ref3[1];}if((ref1=this.tag())==='DEFAULT'||ref1==='IMPORT_ALL'||ref1==='IDENTIFIER'){this.token('AS',id);return id.length;}}if(id==='as'&&this.seenExport){if((ref2=this.tag())==='IDENTIFIER'||ref2==='DEFAULT'){this.token('AS',id);return id.length;}if(ref3=this.value(true),indexOf.call(COFFEE_KEYWORDS,ref3)>=0){prev=this.prev();var _ref4=['IDENTIFIER',this.value(true)];prev[0]=_ref4[0];prev[1]=_ref4[1];this.token('AS',id);return id.length;}}if(id==='default'&&this.seenExport&&((ref4=this.tag())==='EXPORT'||ref4==='AS')){this.token('DEFAULT',id);return id.length;}if(id==='do'&&(regExSuper=/^(\s*super)(?!\(\))/.exec(this.chunk.slice(3)))){this.token('SUPER','super');this.token('CALL_START','(');this.token('CALL_END',')');var _regExSuper=regExSuper;var _regExSuper2=_slicedToArray(_regExSuper,2);input=_regExSuper2[0];sup=_regExSuper2[1];return sup.length+3;}prev=this.prev();tag=colon||prev!=null&&((ref5=prev[0])==='.'||ref5==='?.'||ref5==='::'||ref5==='?::'||!prev.spaced&&prev[0]==='@')?'PROPERTY':'IDENTIFIER';if(tag==='IDENTIFIER'&&(indexOf.call(JS_KEYWORDS,id)>=0||indexOf.call(COFFEE_KEYWORDS,id)>=0)&&!(this.exportSpecifierList&&indexOf.call(COFFEE_KEYWORDS,id)>=0)){tag=id.toUpperCase();if(tag==='WHEN'&&(ref6=this.tag(),indexOf.call(LINE_BREAK,ref6)>=0)){tag='LEADING_WHEN';}else if(tag==='FOR'){this.seenFor=true;}else if(tag==='UNLESS'){tag='IF';}else if(tag==='IMPORT'){this.seenImport=true;}else if(tag==='EXPORT'){this.seenExport=true;}else if(indexOf.call(UNARY,tag)>=0){tag='UNARY';}else if(indexOf.call(RELATION,tag)>=0){if(tag!=='INSTANCEOF'&&this.seenFor){tag='FOR'+tag;this.seenFor=false;}else{tag='RELATION';if(this.value()==='!'){poppedToken=this.tokens.pop();id='!'+id;}}}}else if(tag==='IDENTIFIER'&&this.seenFor&&id==='from'&&isForFrom(prev)){tag='FORFROM';this.seenFor=false;// Throw an error on attempts to use `get` or `set` as keywords, or
// what CoffeeScript would normally interpret as calls to functions named
// `get` or `set`, i.e. `get({foo: function () {}})`.
}else if(tag==='PROPERTY'&&prev){if(prev.spaced&&(ref7=prev[0],indexOf.call(CALLABLE,ref7)>=0)&&/^[gs]et$/.test(prev[1])&&this.tokens.length>1&&(ref8=this.tokens[this.tokens.length-2][0])!=='.'&&ref8!=='?.'&&ref8!=='@'){this.error('\''+prev[1]+'\' cannot be used as a keyword, or as a function call without parentheses',prev[2]);}else if(this.tokens.length>2){prevprev=this.tokens[this.tokens.length-2];if(((ref9=prev[0])==='@'||ref9==='THIS')&&prevprev&&prevprev.spaced&&/^[gs]et$/.test(prevprev[1])&&(ref10=this.tokens[this.tokens.length-3][0])!=='.'&&ref10!=='?.'&&ref10!=='@'){this.error('\''+prevprev[1]+'\' cannot be used as a keyword, or as a function call without parentheses',prevprev[2]);}}}if(tag==='IDENTIFIER'&&indexOf.call(RESERVED,id)>=0){this.error('reserved word \''+id+'\'',{length:id.length});}if(!(tag==='PROPERTY'||this.exportSpecifierList)){if(indexOf.call(COFFEE_ALIASES,id)>=0){alias=id;id=COFFEE_ALIAS_MAP[id];}tag=function(){switch(id){case'!':return'UNARY';case'==':case'!=':return'COMPARE';case'true':case'false':return'BOOL';case'break':case'continue':case'debugger':return'STATEMENT';case'&&':case'||':return id;default:return tag;}}();}tagToken=this.token(tag,id,0,idLength);if(alias){tagToken.origin=[tag,alias,tagToken[2]];}if(poppedToken){var _ref5=[poppedToken[2].first_line,poppedToken[2].first_column];tagToken[2].first_line=_ref5[0];tagToken[2].first_column=_ref5[1];}if(colon){colonOffset=input.lastIndexOf(inCSXTag?'=':':');colonToken=this.token(':',':',colonOffset,colon.length);if(inCSXTag){// used by rewriter
colonToken.csxColon=true;}}if(inCSXTag&&tag==='IDENTIFIER'&&prev[0]!==':'){this.token(',',',',0,0,tagToken);}return input.length;}// Matches numbers, including decimals, hex, and exponential notation.
// Be careful not to interfere with ranges in progress.
},{key:'numberToken',value:function numberToken(){var base,lexedLength,match,number,numberValue,tag;if(!(match=NUMBER.exec(this.chunk))){return 0;}number=match[0];lexedLength=number.length;switch(false){case!/^0[BOX]/.test(number):this.error('radix prefix in \''+number+'\' must be lowercase',{offset:1});break;case!/^(?!0x).*E/.test(number):this.error('exponential notation in \''+number+'\' must be indicated with a lowercase \'e\'',{offset:number.indexOf('E')});break;case!/^0\d*[89]/.test(number):this.error('decimal literal \''+number+'\' must not be prefixed with \'0\'',{length:lexedLength});break;case!/^0\d+/.test(number):this.error('octal literal \''+number+'\' must be prefixed with \'0o\'',{length:lexedLength});}base=function(){switch(number.charAt(1)){case'b':return 2;case'o':return 8;case'x':return 16;default:return null;}}();numberValue=base!=null?parseInt(number.slice(2),base):parseFloat(number);tag=numberValue===2e308?'INFINITY':'NUMBER';this.token(tag,number,0,lexedLength);return lexedLength;}// Matches strings, including multiline strings, as well as heredocs, with or without
// interpolation.
},{key:'stringToken',value:function stringToken(){var _this3=this;var $,attempt,delimiter,doc,end,heredoc,i,indent,indentRegex,match,prev,quote,ref,regex,token,tokens;var _ref6=STRING_START.exec(this.chunk)||[];var _ref7=_slicedToArray(_ref6,1);quote=_ref7[0];if(!quote){return 0;}// If the preceding token is `from` and this is an import or export statement,
// properly tag the `from`.
prev=this.prev();if(prev&&this.value()==='from'&&(this.seenImport||this.seenExport)){prev[0]='FROM';}regex=function(){switch(quote){case"'":return STRING_SINGLE;case'"':return STRING_DOUBLE;case"'''":return HEREDOC_SINGLE;case'"""':return HEREDOC_DOUBLE;}}();heredoc=quote.length===3;var _matchWithInterpolati=this.matchWithInterpolations(regex,quote);tokens=_matchWithInterpolati.tokens;end=_matchWithInterpolati.index;$=tokens.length-1;delimiter=quote.charAt(0);if(heredoc){// Find the smallest indentation. It will be removed from all lines later.
indent=null;doc=function(){var j,len,results;results=[];for(i=j=0,len=tokens.length;j<len;i=++j){token=tokens[i];if(token[0]==='NEOSTRING'){results.push(token[1]);}}return results;}().join('#{}');while(match=HEREDOC_INDENT.exec(doc)){attempt=match[1];if(indent===null||0<(ref=attempt.length)&&ref<indent.length){indent=attempt;}}if(indent){indentRegex=RegExp('\\n'+indent,"g");}this.mergeInterpolationTokens(tokens,{delimiter:delimiter},function(value,i){value=_this3.formatString(value,{delimiter:quote});if(indentRegex){value=value.replace(indentRegex,'\n');}if(i===0){value=value.replace(LEADING_BLANK_LINE,'');}if(i===$){value=value.replace(TRAILING_BLANK_LINE,'');}return value;});}else{this.mergeInterpolationTokens(tokens,{delimiter:delimiter},function(value,i){value=_this3.formatString(value,{delimiter:quote});value=value.replace(SIMPLE_STRING_OMIT,function(match,offset){if(i===0&&offset===0||i===$&&offset+match.length===value.length){return'';}else{return' ';}});return value;});}if(this.atCSXTag()){this.token(',',',',0,0,this.prev);}return end;}// Matches and consumes comments. The comments are taken out of the token
// stream and saved for later, to be reinserted into the output after
// everything has been parsed and the JavaScript code generated.
},{key:'commentToken',value:function commentToken(){var chunk=arguments.length>0&&arguments[0]!==undefined?arguments[0]:this.chunk;var comment,commentAttachments,content,contents,here,i,match,matchIllegal,newLine,placeholderToken,prev;if(!(match=chunk.match(COMMENT))){return 0;}var _match3=match;var _match4=_slicedToArray(_match3,2);comment=_match4[0];here=_match4[1];contents=null;// Does this comment follow code on the same line?
newLine=/^\s*\n+\s*#/.test(comment);if(here){matchIllegal=HERECOMMENT_ILLEGAL.exec(comment);if(matchIllegal){this.error('block comments cannot contain '+matchIllegal[0],{offset:matchIllegal.index,length:matchIllegal[0].length});}// Parse indentation or outdentation as if this block comment didn’t exist.
chunk=chunk.replace('###'+here+'###','');// Remove leading newlines, like `Rewriter::removeLeadingNewlines`, to
// avoid the creation of unwanted `TERMINATOR` tokens.
chunk=chunk.replace(/^\n+/,'');this.lineToken(chunk);// Pull out the ###-style comment’s content, and format it.
content=here;if(indexOf.call(content,'\n')>=0){content=content.replace(RegExp('\\n'+repeat(' ',this.indent),"g"),'\n');}contents=[content];}else{// The `COMMENT` regex captures successive line comments as one token.
// Remove any leading newlines before the first comment, but preserve
// blank lines between line comments.
content=comment.replace(/^(\n*)/,'');content=content.replace(/^([ |\t]*)#/gm,'');contents=content.split('\n');}commentAttachments=function(){var j,len,results;results=[];for(i=j=0,len=contents.length;j<len;i=++j){content=contents[i];results.push({content:content,here:here!=null,newLine:newLine||i!==0// Line comments after the first one start new lines, by definition.
});}return results;}();prev=this.prev();if(!prev){// If there’s no previous token, create a placeholder token to attach
// this comment to; and follow with a newline.
commentAttachments[0].newLine=true;this.lineToken(this.chunk.slice(comment.length));placeholderToken=this.makeToken('JS','');placeholderToken.generated=true;placeholderToken.comments=commentAttachments;this.tokens.push(placeholderToken);this.newlineToken(0);}else{attachCommentsToNode(commentAttachments,prev);}return comment.length;}// Matches JavaScript interpolated directly into the source via backticks.
},{key:'jsToken',value:function jsToken(){var match,script;if(!(this.chunk.charAt(0)==='`'&&(match=HERE_JSTOKEN.exec(this.chunk)||JSTOKEN.exec(this.chunk)))){return 0;}// Convert escaped backticks to backticks, and escaped backslashes
// just before escaped backticks to backslashes
script=match[1].replace(/\\+(`|$)/g,function(string){// `string` is always a value like '\`', '\\\`', '\\\\\`', etc.
// By reducing it to its latter half, we turn '\`' to '`', '\\\`' to '\`', etc.
return string.slice(-Math.ceil(string.length/2));});this.token('JS',script,0,match[0].length);return match[0].length;}// Matches regular expression literals, as well as multiline extended ones.
// Lexing regular expressions is difficult to distinguish from division, so we
// borrow some basic heuristics from JavaScript and Ruby.
},{key:'regexToken',value:function regexToken(){var _this4=this;var body,closed,comment,comments,end,flags,index,j,len,match,origin,prev,ref,ref1,regex,tokens;switch(false){case!(match=REGEX_ILLEGAL.exec(this.chunk)):this.error('regular expressions cannot begin with '+match[2],{offset:match.index+match[1].length});break;case!(match=this.matchWithInterpolations(HEREGEX,'///')):var _match5=match;tokens=_match5.tokens;index=_match5.index;comments=this.chunk.slice(0,index).match(/\s+(#(?!{).*)/g);if(comments){for(j=0,len=comments.length;j<len;j++){comment=comments[j];this.commentToken(comment);}}break;case!(match=REGEX.exec(this.chunk)):var _match6=match;var _match7=_slicedToArray(_match6,3);regex=_match7[0];body=_match7[1];closed=_match7[2];this.validateEscapes(body,{isRegex:true,offsetInChunk:1});index=regex.length;prev=this.prev();if(prev){if(prev.spaced&&(ref=prev[0],indexOf.call(CALLABLE,ref)>=0)){if(!closed||POSSIBLY_DIVISION.test(regex)){return 0;}}else if(ref1=prev[0],indexOf.call(NOT_REGEX,ref1)>=0){return 0;}}if(!closed){this.error('missing / (unclosed regex)');}break;default:return 0;}var _REGEX_FLAGS$exec=REGEX_FLAGS.exec(this.chunk.slice(index));var _REGEX_FLAGS$exec2=_slicedToArray(_REGEX_FLAGS$exec,1);flags=_REGEX_FLAGS$exec2[0];end=index+flags.length;origin=this.makeToken('REGEX',null,0,end);switch(false){case!!VALID_FLAGS.test(flags):this.error('invalid regular expression flags '+flags,{offset:index,length:flags.length});break;case!(regex||tokens.length===1):if(body){body=this.formatRegex(body,{flags:flags,delimiter:'/'});}else{body=this.formatHeregex(tokens[0][1],{flags:flags});}this.token('REGEX',''+this.makeDelimitedLiteral(body,{delimiter:'/'})+flags,0,end,origin);break;default:this.token('REGEX_START','(',0,0,origin);this.token('IDENTIFIER','RegExp',0,0);this.token('CALL_START','(',0,0);this.mergeInterpolationTokens(tokens,{delimiter:'"',double:true},function(str){return _this4.formatHeregex(str,{flags:flags});});if(flags){this.token(',',',',index-1,0);this.token('STRING','"'+flags+'"',index-1,flags.length);}this.token(')',')',end-1,0);this.token('REGEX_END',')',end-1,0);}return end;}// Matches newlines, indents, and outdents, and determines which is which.
// If we can detect that the current line is continued onto the next line,
// then the newline is suppressed:
//     elements
//       .each( ... )
//       .map( ... )
// Keeps track of the level of indentation, because a single outdent token
// can close multiple indents, so we need to know how far in we happen to be.
},{key:'lineToken',value:function lineToken(){var chunk=arguments.length>0&&arguments[0]!==undefined?arguments[0]:this.chunk;var backslash,diff,indent,match,minLiteralLength,newIndentLiteral,noNewlines,prev,size;if(!(match=MULTI_DENT.exec(chunk))){return 0;}indent=match[0];prev=this.prev();backslash=(prev!=null?prev[0]:void 0)==='\\';if(!(backslash&&this.seenFor)){this.seenFor=false;}if(!(backslash&&this.seenImport||this.importSpecifierList)){this.seenImport=false;}if(!(backslash&&this.seenExport||this.exportSpecifierList)){this.seenExport=false;}size=indent.length-1-indent.lastIndexOf('\n');noNewlines=this.unfinished();newIndentLiteral=size>0?indent.slice(-size):'';if(!/^(.?)\1*$/.exec(newIndentLiteral)){this.error('mixed indentation',{offset:indent.length});return indent.length;}minLiteralLength=Math.min(newIndentLiteral.length,this.indentLiteral.length);if(newIndentLiteral.slice(0,minLiteralLength)!==this.indentLiteral.slice(0,minLiteralLength)){this.error('indentation mismatch',{offset:indent.length});return indent.length;}if(size-this.indebt===this.indent){if(noNewlines){this.suppressNewlines();}else{this.newlineToken(0);}return indent.length;}if(size>this.indent){if(noNewlines){if(!backslash){this.indebt=size-this.indent;}this.suppressNewlines();return indent.length;}if(!this.tokens.length){this.baseIndent=this.indent=size;this.indentLiteral=newIndentLiteral;return indent.length;}diff=size-this.indent+this.outdebt;this.token('INDENT',diff,indent.length-size,size);this.indents.push(diff);this.ends.push({tag:'OUTDENT'});this.outdebt=this.indebt=0;this.indent=size;this.indentLiteral=newIndentLiteral;}else if(size<this.baseIndent){this.error('missing indentation',{offset:indent.length});}else{this.indebt=0;this.outdentToken(this.indent-size,noNewlines,indent.length);}return indent.length;}// Record an outdent token or multiple tokens, if we happen to be moving back
// inwards past several recorded indents. Sets new @indent value.
},{key:'outdentToken',value:function outdentToken(moveOut,noNewlines,outdentLength){var decreasedIndent,dent,lastIndent,ref;decreasedIndent=this.indent-moveOut;while(moveOut>0){lastIndent=this.indents[this.indents.length-1];if(!lastIndent){this.outdebt=moveOut=0;}else if(this.outdebt&&moveOut<=this.outdebt){this.outdebt-=moveOut;moveOut=0;}else{dent=this.indents.pop()+this.outdebt;if(outdentLength&&(ref=this.chunk[outdentLength],indexOf.call(INDENTABLE_CLOSERS,ref)>=0)){decreasedIndent-=dent-moveOut;moveOut=dent;}this.outdebt=0;// pair might call outdentToken, so preserve decreasedIndent
this.pair('OUTDENT');this.token('OUTDENT',moveOut,0,outdentLength);moveOut-=dent;}}if(dent){this.outdebt-=moveOut;}this.suppressSemicolons();if(!(this.tag()==='TERMINATOR'||noNewlines)){this.token('TERMINATOR','\n',outdentLength,0);}this.indent=decreasedIndent;this.indentLiteral=this.indentLiteral.slice(0,decreasedIndent);return this;}// Matches and consumes non-meaningful whitespace. Tag the previous token
// as being “spaced”, because there are some cases where it makes a difference.
},{key:'whitespaceToken',value:function whitespaceToken(){var match,nline,prev;if(!((match=WHITESPACE.exec(this.chunk))||(nline=this.chunk.charAt(0)==='\n'))){return 0;}prev=this.prev();if(prev){prev[match?'spaced':'newLine']=true;}if(match){return match[0].length;}else{return 0;}}// Generate a newline token. Consecutive newlines get merged together.
},{key:'newlineToken',value:function newlineToken(offset){this.suppressSemicolons();if(this.tag()!=='TERMINATOR'){this.token('TERMINATOR','\n',offset,0);}return this;}// Use a `\` at a line-ending to suppress the newline.
// The slash is removed here once its job is done.
},{key:'suppressNewlines',value:function suppressNewlines(){var prev;prev=this.prev();if(prev[1]==='\\'){if(prev.comments&&this.tokens.length>1){// `@tokens.length` should be at least 2 (some code, then `\`).
// If something puts a `\` after nothing, they deserve to lose any
// comments that trail it.
attachCommentsToNode(prev.comments,this.tokens[this.tokens.length-2]);}this.tokens.pop();}return this;}// CSX is like JSX but for CoffeeScript.
},{key:'csxToken',value:function csxToken(){var _this5=this;var afterTag,colon,csxTag,end,firstChar,id,input,match,origin,prev,prevChar,ref,token,tokens;firstChar=this.chunk[0];// Check the previous token to detect if attribute is spread.
prevChar=this.tokens.length>0?this.tokens[this.tokens.length-1][0]:'';if(firstChar==='<'){match=CSX_IDENTIFIER.exec(this.chunk.slice(1))||CSX_FRAGMENT_IDENTIFIER.exec(this.chunk.slice(1));// Not the right hand side of an unspaced comparison (i.e. `a<b`).
if(!(match&&(this.csxDepth>0||!(prev=this.prev())||prev.spaced||(ref=prev[0],indexOf.call(COMPARABLE_LEFT_SIDE,ref)<0)))){return 0;}var _match8=match;var _match9=_slicedToArray(_match8,3);input=_match9[0];id=_match9[1];colon=_match9[2];origin=this.token('CSX_TAG',id,1,id.length);this.token('CALL_START','(');this.token('[','[');this.ends.push({tag:'/>',origin:origin,name:id});this.csxDepth++;return id.length+1;}else if(csxTag=this.atCSXTag()){if(this.chunk.slice(0,2)==='/>'){this.pair('/>');this.token(']',']',0,2);this.token('CALL_END',')',0,2);this.csxDepth--;return 2;}else if(firstChar==='{'){if(prevChar===':'){token=this.token('(','(');this.csxObjAttribute[this.csxDepth]=false;}else{token=this.token('{','{');this.csxObjAttribute[this.csxDepth]=true;}this.ends.push({tag:'}',origin:token});return 1;}else if(firstChar==='>'){// Ignore terminators inside a tag.
this.pair('/>');// As if the current tag was self-closing.
origin=this.token(']',']');this.token(',',',');var _matchWithInterpolati2=this.matchWithInterpolations(INSIDE_CSX,'>','</',CSX_INTERPOLATION);tokens=_matchWithInterpolati2.tokens;end=_matchWithInterpolati2.index;this.mergeInterpolationTokens(tokens,{delimiter:'"'},function(value,i){return _this5.formatString(value,{delimiter:'>'});});match=CSX_IDENTIFIER.exec(this.chunk.slice(end))||CSX_FRAGMENT_IDENTIFIER.exec(this.chunk.slice(end));if(!match||match[1]!==csxTag.name){this.error('expected corresponding CSX closing tag for '+csxTag.name,csxTag.origin[2]);}afterTag=end+csxTag.name.length;if(this.chunk[afterTag]!=='>'){this.error("missing closing > after tag name",{offset:afterTag,length:1});}// +1 for the closing `>`.
this.token('CALL_END',')',end,csxTag.name.length+1);this.csxDepth--;return afterTag+1;}else{return 0;}}else if(this.atCSXTag(1)){if(firstChar==='}'){this.pair(firstChar);if(this.csxObjAttribute[this.csxDepth]){this.token('}','}');this.csxObjAttribute[this.csxDepth]=false;}else{this.token(')',')');}this.token(',',',');return 1;}else{return 0;}}else{return 0;}}},{key:'atCSXTag',value:function atCSXTag(){var depth=arguments.length>0&&arguments[0]!==undefined?arguments[0]:0;var i,last,ref;if(this.csxDepth===0){return false;}i=this.ends.length-1;while(((ref=this.ends[i])!=null?ref.tag:void 0)==='OUTDENT'||depth-->0){// Ignore indents.
i--;}last=this.ends[i];return(last!=null?last.tag:void 0)==='/>'&&last;}// We treat all other single characters as a token. E.g.: `( ) , . !`
// Multi-character operators are also literal tokens, so that Jison can assign
// the proper order of operations. There are some symbols that we tag specially
// here. `;` and newlines are both treated as a `TERMINATOR`, we distinguish
// parentheses that indicate a method call from regular parentheses, and so on.
},{key:'literalToken',value:function literalToken(){var match,message,origin,prev,ref,ref1,ref2,ref3,ref4,skipToken,tag,token,value;if(match=OPERATOR.exec(this.chunk)){var _match10=match;var _match11=_slicedToArray(_match10,1);value=_match11[0];if(CODE.test(value)){this.tagParameters();}}else{value=this.chunk.charAt(0);}tag=value;prev=this.prev();if(prev&&indexOf.call(['='].concat(_toConsumableArray(COMPOUND_ASSIGN)),value)>=0){skipToken=false;if(value==='='&&((ref=prev[1])==='||'||ref==='&&')&&!prev.spaced){prev[0]='COMPOUND_ASSIGN';prev[1]+='=';prev=this.tokens[this.tokens.length-2];skipToken=true;}if(prev&&prev[0]!=='PROPERTY'){origin=(ref1=prev.origin)!=null?ref1:prev;message=isUnassignable(prev[1],origin[1]);if(message){this.error(message,origin[2]);}}if(skipToken){return value.length;}}if(value==='{'&&this.seenImport){this.importSpecifierList=true;}else if(this.importSpecifierList&&value==='}'){this.importSpecifierList=false;}else if(value==='{'&&(prev!=null?prev[0]:void 0)==='EXPORT'){this.exportSpecifierList=true;}else if(this.exportSpecifierList&&value==='}'){this.exportSpecifierList=false;}if(value===';'){if(ref2=prev!=null?prev[0]:void 0,indexOf.call(['='].concat(_toConsumableArray(UNFINISHED)),ref2)>=0){this.error('unexpected ;');}this.seenFor=this.seenImport=this.seenExport=false;tag='TERMINATOR';}else if(value==='*'&&(prev!=null?prev[0]:void 0)==='EXPORT'){tag='EXPORT_ALL';}else if(indexOf.call(MATH,value)>=0){tag='MATH';}else if(indexOf.call(COMPARE,value)>=0){tag='COMPARE';}else if(indexOf.call(COMPOUND_ASSIGN,value)>=0){tag='COMPOUND_ASSIGN';}else if(indexOf.call(UNARY,value)>=0){tag='UNARY';}else if(indexOf.call(UNARY_MATH,value)>=0){tag='UNARY_MATH';}else if(indexOf.call(SHIFT,value)>=0){tag='SHIFT';}else if(value==='?'&&(prev!=null?prev.spaced:void 0)){tag='BIN?';}else if(prev){if(value==='('&&!prev.spaced&&(ref3=prev[0],indexOf.call(CALLABLE,ref3)>=0)){if(prev[0]==='?'){prev[0]='FUNC_EXIST';}tag='CALL_START';}else if(value==='['&&((ref4=prev[0],indexOf.call(INDEXABLE,ref4)>=0)&&!prev.spaced||prev[0]==='::')){// `.prototype` can’t be a method you can call.
tag='INDEX_START';switch(prev[0]){case'?':prev[0]='INDEX_SOAK';}}}token=this.makeToken(tag,value);switch(value){case'(':case'{':case'[':this.ends.push({tag:INVERSES[value],origin:token});break;case')':case'}':case']':this.pair(value);}this.tokens.push(this.makeToken(tag,value));return value.length;}// Token Manipulators
// ------------------
// A source of ambiguity in our grammar used to be parameter lists in function
// definitions versus argument lists in function calls. Walk backwards, tagging
// parameters specially in order to make things easier for the parser.
},{key:'tagParameters',value:function tagParameters(){var i,paramEndToken,stack,tok,tokens;if(this.tag()!==')'){return this;}stack=[];tokens=this.tokens;i=tokens.length;paramEndToken=tokens[--i];paramEndToken[0]='PARAM_END';while(tok=tokens[--i]){switch(tok[0]){case')':stack.push(tok);break;case'(':case'CALL_START':if(stack.length){stack.pop();}else if(tok[0]==='('){tok[0]='PARAM_START';return this;}else{paramEndToken[0]='CALL_END';return this;}}}return this;}// Close up all remaining open blocks at the end of the file.
},{key:'closeIndentation',value:function closeIndentation(){return this.outdentToken(this.indent);}// Match the contents of a delimited token and expand variables and expressions
// inside it using Ruby-like notation for substitution of arbitrary
// expressions.
//     "Hello #{name.capitalize()}."
// If it encounters an interpolation, this method will recursively create a new
// Lexer and tokenize until the `{` of `#{` is balanced with a `}`.
//  - `regex` matches the contents of a token (but not `delimiter`, and not
//    `#{` if interpolations are desired).
//  - `delimiter` is the delimiter of the token. Examples are `'`, `"`, `'''`,
//    `"""` and `///`.
//  - `closingDelimiter` is different from `delimiter` only in CSX
//  - `interpolators` matches the start of an interpolation, for CSX it's both
//    `{` and `<` (i.e. nested CSX tag)
// This method allows us to have strings within interpolations within strings,
// ad infinitum.
},{key:'matchWithInterpolations',value:function matchWithInterpolations(regex,delimiter,closingDelimiter,interpolators){var _tokens2,_tokens3,_slice$call3,_slice$call4;var braceInterpolator,close,column,firstToken,index,interpolationOffset,interpolator,lastToken,line,match,nested,offsetInChunk,open,ref,rest,str,strPart,tokens;if(closingDelimiter==null){closingDelimiter=delimiter;}if(interpolators==null){interpolators=/^#\{/;}tokens=[];offsetInChunk=delimiter.length;if(this.chunk.slice(0,offsetInChunk)!==delimiter){return null;}str=this.chunk.slice(offsetInChunk);while(true){var _regex$exec=regex.exec(str);var _regex$exec2=_slicedToArray(_regex$exec,1);strPart=_regex$exec2[0];this.validateEscapes(strPart,{isRegex:delimiter.charAt(0)==='/',offsetInChunk:offsetInChunk});// Push a fake `'NEOSTRING'` token, which will get turned into a real string later.
tokens.push(this.makeToken('NEOSTRING',strPart,offsetInChunk));str=str.slice(strPart.length);offsetInChunk+=strPart.length;if(!(match=interpolators.exec(str))){break;}// To remove the `#` in `#{`.
var _match12=match;var _match13=_slicedToArray(_match12,1);interpolator=_match13[0];interpolationOffset=interpolator.length-1;var _getLineAndColumnFrom3=this.getLineAndColumnFromChunk(offsetInChunk+interpolationOffset);var _getLineAndColumnFrom4=_slicedToArray(_getLineAndColumnFrom3,2);line=_getLineAndColumnFrom4[0];column=_getLineAndColumnFrom4[1];rest=str.slice(interpolationOffset);// Account for the `#` in `#{`
var _tokenize=new Lexer().tokenize(rest,{line:line,column:column,untilBalanced:true});nested=_tokenize.tokens;index=_tokenize.index;index+=interpolationOffset;braceInterpolator=str[index-1]==='}';if(braceInterpolator){var _nested,_nested2,_slice$call,_slice$call2;// Turn the leading and trailing `{` and `}` into parentheses. Unnecessary
// parentheses will be removed later.
(_nested=nested,_nested2=_slicedToArray(_nested,1),open=_nested2[0],_nested),(_slice$call=slice.call(nested,-1),_slice$call2=_slicedToArray(_slice$call,1),close=_slice$call2[0],_slice$call);open[0]=open[1]='(';close[0]=close[1]=')';close.origin=['','end of interpolation',close[2]];}if(((ref=nested[1])!=null?ref[0]:void 0)==='TERMINATOR'){// Remove leading `'TERMINATOR'` (if any).
nested.splice(1,1);}if(!braceInterpolator){// We are not using `{` and `}`, so wrap the interpolated tokens instead.
open=this.makeToken('(','(',offsetInChunk,0);close=this.makeToken(')',')',offsetInChunk+index,0);nested=[open].concat(_toConsumableArray(nested),[close]);}// Push a fake `'TOKENS'` token, which will get turned into real tokens later.
tokens.push(['TOKENS',nested]);str=str.slice(index);offsetInChunk+=index;}if(str.slice(0,closingDelimiter.length)!==closingDelimiter){this.error('missing '+closingDelimiter,{length:delimiter.length});}(_tokens2=tokens,_tokens3=_slicedToArray(_tokens2,1),firstToken=_tokens3[0],_tokens2),(_slice$call3=slice.call(tokens,-1),_slice$call4=_slicedToArray(_slice$call3,1),lastToken=_slice$call4[0],_slice$call3);firstToken[2].first_column-=delimiter.length;if(lastToken[1].substr(-1)==='\n'){lastToken[2].last_line+=1;lastToken[2].last_column=closingDelimiter.length-1;}else{lastToken[2].last_column+=closingDelimiter.length;}if(lastToken[1].length===0){lastToken[2].last_column-=1;}return{tokens:tokens,index:offsetInChunk+closingDelimiter.length};}// Merge the array `tokens` of the fake token types `'TOKENS'` and `'NEOSTRING'`
// (as returned by `matchWithInterpolations`) into the token stream. The value
// of `'NEOSTRING'`s are converted using `fn` and turned into strings using
// `options` first.
},{key:'mergeInterpolationTokens',value:function mergeInterpolationTokens(tokens,options,fn){var converted,firstEmptyStringIndex,firstIndex,i,j,k,lastToken,len,len1,locationToken,lparen,placeholderToken,plusToken,rparen,tag,token,tokensToPush,val,value;if(tokens.length>1){lparen=this.token('STRING_START','(',0,0);}firstIndex=this.tokens.length;for(i=j=0,len=tokens.length;j<len;i=++j){var _tokens4;token=tokens[i];var _token4=token;var _token5=_slicedToArray(_token4,2);tag=_token5[0];value=_token5[1];switch(tag){case'TOKENS':if(value.length===2){if(!(value[0].comments||value[1].comments)){// Optimize out empty interpolations (an empty pair of parentheses).
continue;}// There are comments (and nothing else) in this interpolation.
if(this.csxDepth===0){// This is an interpolated string, not a CSX tag; and for whatever
// reason `` `a${/*test*/}b` `` is invalid JS. So compile to
// `` `a${/*test*/''}b` `` instead.
placeholderToken=this.makeToken('STRING',"''");}else{placeholderToken=this.makeToken('JS','');}// Use the same location data as the first parenthesis.
placeholderToken[2]=value[0][2];for(k=0,len1=value.length;k<len1;k++){var _placeholderToken$com;val=value[k];if(!val.comments){continue;}if(placeholderToken.comments==null){placeholderToken.comments=[];}(_placeholderToken$com=placeholderToken.comments).push.apply(_placeholderToken$com,_toConsumableArray(val.comments));}value.splice(1,0,placeholderToken);}// Push all the tokens in the fake `'TOKENS'` token. These already have
// sane location data.
locationToken=value[0];tokensToPush=value;break;case'NEOSTRING':// Convert `'NEOSTRING'` into `'STRING'`.
converted=fn.call(this,token[1],i);// Optimize out empty strings. We ensure that the tokens stream always
// starts with a string token, though, to make sure that the result
// really is a string.
if(converted.length===0){if(i===0){firstEmptyStringIndex=this.tokens.length;}else{continue;}}// However, there is one case where we can optimize away a starting
// empty string.
if(i===2&&firstEmptyStringIndex!=null){this.tokens.splice(firstEmptyStringIndex,2);// Remove empty string and the plus.
}token[0]='STRING';token[1]=this.makeDelimitedLiteral(converted,options);locationToken=token;tokensToPush=[token];}if(this.tokens.length>firstIndex){// Create a 0-length "+" token.
plusToken=this.token('+','+');plusToken[2]={first_line:locationToken[2].first_line,first_column:locationToken[2].first_column,last_line:locationToken[2].first_line,last_column:locationToken[2].first_column};}(_tokens4=this.tokens).push.apply(_tokens4,_toConsumableArray(tokensToPush));}if(lparen){var _slice$call5=slice.call(tokens,-1);var _slice$call6=_slicedToArray(_slice$call5,1);lastToken=_slice$call6[0];lparen.origin=['STRING',null,{first_line:lparen[2].first_line,first_column:lparen[2].first_column,last_line:lastToken[2].last_line,last_column:lastToken[2].last_column}];lparen[2]=lparen.origin[2];rparen=this.token('STRING_END',')');return rparen[2]={first_line:lastToken[2].last_line,first_column:lastToken[2].last_column,last_line:lastToken[2].last_line,last_column:lastToken[2].last_column};}}// Pairs up a closing token, ensuring that all listed pairs of tokens are
// correctly balanced throughout the course of the token stream.
},{key:'pair',value:function pair(tag){var _slice$call7,_slice$call8;var lastIndent,prev,ref,ref1,wanted;ref=this.ends,(_slice$call7=slice.call(ref,-1),_slice$call8=_slicedToArray(_slice$call7,1),prev=_slice$call8[0],_slice$call7);if(tag!==(wanted=prev!=null?prev.tag:void 0)){var _slice$call9,_slice$call10;if('OUTDENT'!==wanted){this.error('unmatched '+tag);}// Auto-close `INDENT` to support syntax like this:
//     el.click((event) ->
//       el.hide())
ref1=this.indents,(_slice$call9=slice.call(ref1,-1),_slice$call10=_slicedToArray(_slice$call9,1),lastIndent=_slice$call10[0],_slice$call9);this.outdentToken(lastIndent,true);return this.pair(tag);}return this.ends.pop();}// Helpers
// -------
// Returns the line and column number from an offset into the current chunk.
// `offset` is a number of characters into `@chunk`.
},{key:'getLineAndColumnFromChunk',value:function getLineAndColumnFromChunk(offset){var column,lastLine,lineCount,ref,string;if(offset===0){return[this.chunkLine,this.chunkColumn];}if(offset>=this.chunk.length){string=this.chunk;}else{string=this.chunk.slice(0,+(offset-1)+1||9e9);}lineCount=count(string,'\n');column=this.chunkColumn;if(lineCount>0){var _slice$call11,_slice$call12;ref=string.split('\n'),(_slice$call11=slice.call(ref,-1),_slice$call12=_slicedToArray(_slice$call11,1),lastLine=_slice$call12[0],_slice$call11);column=lastLine.length;}else{column+=string.length;}return[this.chunkLine+lineCount,column];}// Same as `token`, except this just returns the token without adding it
// to the results.
},{key:'makeToken',value:function makeToken(tag,value){var offsetInChunk=arguments.length>2&&arguments[2]!==undefined?arguments[2]:0;var length=arguments.length>3&&arguments[3]!==undefined?arguments[3]:value.length;var lastCharacter,locationData,token;locationData={};// Use length - 1 for the final offset - we're supplying the last_line and the last_column,
// so if last_column == first_column, then we're looking at a character of length 1.
var _getLineAndColumnFrom5=this.getLineAndColumnFromChunk(offsetInChunk);var _getLineAndColumnFrom6=_slicedToArray(_getLineAndColumnFrom5,2);locationData.first_line=_getLineAndColumnFrom6[0];locationData.first_column=_getLineAndColumnFrom6[1];lastCharacter=length>0?length-1:0;var _getLineAndColumnFrom7=this.getLineAndColumnFromChunk(offsetInChunk+lastCharacter);var _getLineAndColumnFrom8=_slicedToArray(_getLineAndColumnFrom7,2);locationData.last_line=_getLineAndColumnFrom8[0];locationData.last_column=_getLineAndColumnFrom8[1];token=[tag,value,locationData];return token;}// Add a token to the results.
// `offset` is the offset into the current `@chunk` where the token starts.
// `length` is the length of the token in the `@chunk`, after the offset.  If
// not specified, the length of `value` will be used.
// Returns the new token.
},{key:'token',value:function token(tag,value,offsetInChunk,length,origin){var token;token=this.makeToken(tag,value,offsetInChunk,length);if(origin){token.origin=origin;}this.tokens.push(token);return token;}// Peek at the last tag in the token stream.
},{key:'tag',value:function tag(){var _slice$call13,_slice$call14;var ref,token;ref=this.tokens,(_slice$call13=slice.call(ref,-1),_slice$call14=_slicedToArray(_slice$call13,1),token=_slice$call14[0],_slice$call13);return token!=null?token[0]:void 0;}// Peek at the last value in the token stream.
},{key:'value',value:function value(){var _slice$call15,_slice$call16;var useOrigin=arguments.length>0&&arguments[0]!==undefined?arguments[0]:false;var ref,ref1,token;ref=this.tokens,(_slice$call15=slice.call(ref,-1),_slice$call16=_slicedToArray(_slice$call15,1),token=_slice$call16[0],_slice$call15);if(useOrigin&&(token!=null?token.origin:void 0)!=null){return(ref1=token.origin)!=null?ref1[1]:void 0;}else{return token!=null?token[1]:void 0;}}// Get the previous token in the token stream.
},{key:'prev',value:function prev(){return this.tokens[this.tokens.length-1];}// Are we in the midst of an unfinished expression?
},{key:'unfinished',value:function unfinished(){var ref;return LINE_CONTINUER.test(this.chunk)||(ref=this.tag(),indexOf.call(UNFINISHED,ref)>=0);}},{key:'formatString',value:function formatString(str,options){return this.replaceUnicodeCodePointEscapes(str.replace(STRING_OMIT,'$1'),options);}},{key:'formatHeregex',value:function formatHeregex(str,options){return this.formatRegex(str.replace(HEREGEX_OMIT,'$1$2'),merge(options,{delimiter:'///'}));}},{key:'formatRegex',value:function formatRegex(str,options){return this.replaceUnicodeCodePointEscapes(str,options);}},{key:'unicodeCodePointToUnicodeEscapes',value:function unicodeCodePointToUnicodeEscapes(codePoint){var high,low,toUnicodeEscape;toUnicodeEscape=function toUnicodeEscape(val){var str;str=val.toString(16);return'\\u'+repeat('0',4-str.length)+str;};if(codePoint<0x10000){return toUnicodeEscape(codePoint);}// surrogate pair
high=Math.floor((codePoint-0x10000)/0x400)+0xD800;low=(codePoint-0x10000)%0x400+0xDC00;return''+toUnicodeEscape(high)+toUnicodeEscape(low);}// Replace `\u{...}` with `\uxxxx[\uxxxx]` in regexes without `u` flag
},{key:'replaceUnicodeCodePointEscapes',value:function replaceUnicodeCodePointEscapes(str,options){var _this6=this;var shouldReplace;shouldReplace=options.flags!=null&&indexOf.call(options.flags,'u')<0;return str.replace(UNICODE_CODE_POINT_ESCAPE,function(match,escapedBackslash,codePointHex,offset){var codePointDecimal;if(escapedBackslash){return escapedBackslash;}codePointDecimal=parseInt(codePointHex,16);if(codePointDecimal>0x10ffff){_this6.error('unicode code point escapes greater than \\u{10ffff} are not allowed',{offset:offset+options.delimiter.length,length:codePointHex.length+4});}if(!shouldReplace){return match;}return _this6.unicodeCodePointToUnicodeEscapes(codePointDecimal);});}// Validates escapes in strings and regexes.
},{key:'validateEscapes',value:function validateEscapes(str){var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var before,hex,invalidEscape,invalidEscapeRegex,match,message,octal,ref,unicode,unicodeCodePoint;invalidEscapeRegex=options.isRegex?REGEX_INVALID_ESCAPE:STRING_INVALID_ESCAPE;match=invalidEscapeRegex.exec(str);if(!match){return;}match[0],before=match[1],octal=match[2],hex=match[3],unicodeCodePoint=match[4],unicode=match[5];message=octal?"octal escape sequences are not allowed":"invalid escape sequence";invalidEscape='\\'+(octal||hex||unicodeCodePoint||unicode);return this.error(message+' '+invalidEscape,{offset:((ref=options.offsetInChunk)!=null?ref:0)+match.index+before.length,length:invalidEscape.length});}// Constructs a string or regex by escaping certain characters.
},{key:'makeDelimitedLiteral',value:function makeDelimitedLiteral(body){var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var regex;if(body===''&&options.delimiter==='/'){body='(?:)';}regex=RegExp('(\\\\\\\\)|(\\\\0(?=[1-7]))|\\\\?('+options.delimiter// Escaped backslash.
// Null character mistaken as octal escape.
// (Possibly escaped) delimiter.
// (Possibly escaped) newlines.
// Other escapes.
+')|\\\\?(?:(\\n)|(\\r)|(\\u2028)|(\\u2029))|(\\\\.)',"g");body=body.replace(regex,function(match,backslash,nul,delimiter,lf,cr,ls,ps,other){switch(false){// Ignore escaped backslashes.
case!backslash:if(options.double){return backslash+backslash;}else{return backslash;}case!nul:return'\\x00';case!delimiter:return'\\'+delimiter;case!lf:return'\\n';case!cr:return'\\r';case!ls:return'\\u2028';case!ps:return'\\u2029';case!other:if(options.double){return'\\'+other;}else{return other;}}});return''+options.delimiter+body+options.delimiter;}},{key:'suppressSemicolons',value:function suppressSemicolons(){var ref,ref1,results;results=[];while(this.value()===';'){this.tokens.pop();if(ref=(ref1=this.prev())!=null?ref1[0]:void 0,indexOf.call(['='].concat(_toConsumableArray(UNFINISHED)),ref)>=0){results.push(this.error('unexpected ;'));}else{results.push(void 0);}}return results;}// Throws an error at either a given offset from the current chunk or at the
// location of a token (`token[2]`).
},{key:'error',value:function error(message){var _getLineAndColumnFrom9,_getLineAndColumnFrom10;var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var first_column,first_line,location,ref,ref1;location='first_line'in options?options:((_getLineAndColumnFrom9=this.getLineAndColumnFromChunk((ref=options.offset)!=null?ref:0),_getLineAndColumnFrom10=_slicedToArray(_getLineAndColumnFrom9,2),first_line=_getLineAndColumnFrom10[0],first_column=_getLineAndColumnFrom10[1],_getLineAndColumnFrom9),{first_line:first_line,first_column:first_column,last_column:first_column+((ref1=options.length)!=null?ref1:1)-1});return throwSyntaxError(message,location);}}]);return Lexer;}();// Helper functions
// ----------------
isUnassignable=function isUnassignable(name){var displayName=arguments.length>1&&arguments[1]!==undefined?arguments[1]:name;switch(false){case indexOf.call([].concat(_toConsumableArray(JS_KEYWORDS),_toConsumableArray(COFFEE_KEYWORDS)),name)<0:return'keyword \''+displayName+'\' can\'t be assigned';case indexOf.call(STRICT_PROSCRIBED,name)<0:return'\''+displayName+'\' can\'t be assigned';case indexOf.call(RESERVED,name)<0:return'reserved word \''+displayName+'\' can\'t be assigned';default:return false;}};exports.isUnassignable=isUnassignable;// `from` isn’t a CoffeeScript keyword, but it behaves like one in `import` and
// `export` statements (handled above) and in the declaration line of a `for`
// loop. Try to detect when `from` is a variable identifier and when it is this
// “sometimes” keyword.
isForFrom=function isForFrom(prev){var ref;if(prev[0]==='IDENTIFIER'){// `for i from from`, `for from from iterable`
if(prev[1]==='from'){prev[1][0]='IDENTIFIER';true;}// `for i from iterable`
return true;// `for from…`
}else if(prev[0]==='FOR'){return false;// `for {from}…`, `for [from]…`, `for {a, from}…`, `for {a: from}…`
}else if((ref=prev[1])==='{'||ref==='['||ref===','||ref===':'){return false;}else{return true;}};// Constants
// ---------
// Keywords that CoffeeScript shares in common with JavaScript.
JS_KEYWORDS=['true','false','null','this','new','delete','typeof','in','instanceof','return','throw','break','continue','debugger','yield','await','if','else','switch','for','while','do','try','catch','finally','class','extends','super','import','export','default'];// CoffeeScript-only keywords.
COFFEE_KEYWORDS=['undefined','Infinity','NaN','then','unless','until','loop','of','by','when'];COFFEE_ALIAS_MAP={and:'&&',or:'||',is:'==',isnt:'!=',not:'!',yes:'true',no:'false',on:'true',off:'false'};COFFEE_ALIASES=function(){var results;results=[];for(key in COFFEE_ALIAS_MAP){results.push(key);}return results;}();COFFEE_KEYWORDS=COFFEE_KEYWORDS.concat(COFFEE_ALIASES);// The list of keywords that are reserved by JavaScript, but not used, or are
// used by CoffeeScript internally. We throw an error when these are encountered,
// to avoid having a JavaScript error at runtime.
RESERVED=['case','function','var','void','with','const','let','enum','native','implements','interface','package','private','protected','public','static'];STRICT_PROSCRIBED=['arguments','eval'];// The superset of both JavaScript keywords and reserved words, none of which may
// be used as identifiers or properties.
exports.JS_FORBIDDEN=JS_KEYWORDS.concat(RESERVED).concat(STRICT_PROSCRIBED);// The character code of the nasty Microsoft madness otherwise known as the BOM.
BOM=65279;// Token matching regexes.
IDENTIFIER=/^(?!\d)((?:(?!\s)[$\w\x7f-\uffff])+)([^\n\S]*:(?!:))?/;// Is this a property name?
CSX_IDENTIFIER=/^(?![\d<])((?:(?!\s)[\.\-$\w\x7f-\uffff])+)/;// Must not start with `<`.
// Like `IDENTIFIER`, but includes `-`s and `.`s.
// Fragment: <></>
CSX_FRAGMENT_IDENTIFIER=/^()>/;// Ends immediately with `>`.
CSX_ATTRIBUTE=/^(?!\d)((?:(?!\s)[\-$\w\x7f-\uffff])+)([^\S]*=(?!=))?/;// Like `IDENTIFIER`, but includes `-`s.
// Is this an attribute with a value?
NUMBER=/^0b[01]+|^0o[0-7]+|^0x[\da-f]+|^\d*\.?\d+(?:e[+-]?\d+)?/i;// binary
// octal
// hex
// decimal
OPERATOR=/^(?:[-=]>|[-+*\/%<>&|^!?=]=|>>>=?|([-+:])\1|([&|<>*\/%])\2=?|\?(\.|::)|\.{2,3})/;// function
// compound assign / compare
// zero-fill right shift
// doubles
// logic / shift / power / floor division / modulo
// soak access
// range or splat
WHITESPACE=/^[^\n\S]+/;COMMENT=/^\s*###([^#][\s\S]*?)(?:###[^\n\S]*|###$)|^(?:\s*#(?!##[^#]).*)+/;CODE=/^[-=]>/;MULTI_DENT=/^(?:\n[^\n\S]*)+/;JSTOKEN=/^`(?!``)((?:[^`\\]|\\[\s\S])*)`/;HERE_JSTOKEN=/^```((?:[^`\\]|\\[\s\S]|`(?!``))*)```/;// String-matching-regexes.
STRING_START=/^(?:'''|"""|'|")/;STRING_SINGLE=/^(?:[^\\']|\\[\s\S])*/;STRING_DOUBLE=/^(?:[^\\"#]|\\[\s\S]|\#(?!\{))*/;HEREDOC_SINGLE=/^(?:[^\\']|\\[\s\S]|'(?!''))*/;HEREDOC_DOUBLE=/^(?:[^\\"#]|\\[\s\S]|"(?!"")|\#(?!\{))*/;INSIDE_CSX=/^(?:[^\{<])*/;// Start of CoffeeScript interpolation. // Similar to `HEREDOC_DOUBLE` but there is no escaping.
// Maybe CSX tag (`<` not allowed even if bare).
CSX_INTERPOLATION=/^(?:\{|<(?!\/))/;// CoffeeScript interpolation.
// CSX opening tag.
STRING_OMIT=/((?:\\\\)+)|\\[^\S\n]*\n\s*/g;// Consume (and preserve) an even number of backslashes.
// Remove escaped newlines.
SIMPLE_STRING_OMIT=/\s*\n\s*/g;HEREDOC_INDENT=/\n+([^\n\S]*)(?=\S)/g;// Regex-matching-regexes.
REGEX=/^\/(?!\/)((?:[^[\/\n\\]|\\[^\n]|\[(?:\\[^\n]|[^\]\n\\])*\])*)(\/)?/;// Every other thing.
// Anything but newlines escaped.
// Character class.
REGEX_FLAGS=/^\w*/;VALID_FLAGS=/^(?!.*(.).*\1)[gimsuy]*$/;// Match any character, except those that need special handling below.
// Match `\` followed by any character.
// Match any `/` except `///`.
// Match `#` which is not part of interpolation, e.g. `#{}`.
// Comments consume everything until the end of the line, including `///`.
HEREGEX=/^(?:[^\\\/#\s]|\\[\s\S]|\/(?!\/\/)|\#(?!\{)|\s+(?:#(?!\{).*)?)*/;HEREGEX_OMIT=/((?:\\\\)+)|\\(\s)|\s+(?:#.*)?/g;// Consume (and preserve) an even number of backslashes.
// Preserve escaped whitespace.
// Remove whitespace and comments.
REGEX_ILLEGAL=/^(\/|\/{3}\s*)(\*)/;POSSIBLY_DIVISION=/^\/=?\s/;// Other regexes.
HERECOMMENT_ILLEGAL=/\*\//;LINE_CONTINUER=/^\s*(?:,|\??\.(?![.\d])|::)/;STRING_INVALID_ESCAPE=/((?:^|[^\\])(?:\\\\)*)\\(?:(0[0-7]|[1-7])|(x(?![\da-fA-F]{2}).{0,2})|(u\{(?![\da-fA-F]{1,}\})[^}]*\}?)|(u(?!\{|[\da-fA-F]{4}).{0,4}))/;// Make sure the escape isn’t escaped.
// octal escape
// hex escape
// unicode code point escape
// unicode escape
REGEX_INVALID_ESCAPE=/((?:^|[^\\])(?:\\\\)*)\\(?:(0[0-7])|(x(?![\da-fA-F]{2}).{0,2})|(u\{(?![\da-fA-F]{1,}\})[^}]*\}?)|(u(?!\{|[\da-fA-F]{4}).{0,4}))/;// Make sure the escape isn’t escaped.
// octal escape
// hex escape
// unicode code point escape
// unicode escape
UNICODE_CODE_POINT_ESCAPE=/(\\\\)|\\u\{([\da-fA-F]+)\}/g;// Make sure the escape isn’t escaped.
LEADING_BLANK_LINE=/^[^\n\S]*\n/;TRAILING_BLANK_LINE=/\n[^\n\S]*$/;TRAILING_SPACES=/\s+$/;// Compound assignment tokens.
COMPOUND_ASSIGN=['-=','+=','/=','*=','%=','||=','&&=','?=','<<=','>>=','>>>=','&=','^=','|=','**=','//=','%%='];// Unary tokens.
UNARY=['NEW','TYPEOF','DELETE','DO'];UNARY_MATH=['!','~'];// Bit-shifting tokens.
SHIFT=['<<','>>','>>>'];// Comparison tokens.
COMPARE=['==','!=','<','>','<=','>='];// Mathematical tokens.
MATH=['*','/','%','//','%%'];// Relational tokens that are negatable with `not` prefix.
RELATION=['IN','OF','INSTANCEOF'];// Boolean tokens.
BOOL=['TRUE','FALSE'];// Tokens which could legitimately be invoked or indexed. An opening
// parentheses or bracket following these tokens will be recorded as the start
// of a function invocation or indexing operation.
CALLABLE=['IDENTIFIER','PROPERTY',')',']','?','@','THIS','SUPER'];INDEXABLE=CALLABLE.concat(['NUMBER','INFINITY','NAN','STRING','STRING_END','REGEX','REGEX_END','BOOL','NULL','UNDEFINED','}','::']);// Tokens which can be the left-hand side of a less-than comparison, i.e. `a<b`.
COMPARABLE_LEFT_SIDE=['IDENTIFIER',')',']','NUMBER'];// Tokens which a regular expression will never immediately follow (except spaced
// CALLABLEs in some cases), but which a division operator can.
// See: http://www-archive.mozilla.org/js/language/js20-2002-04/rationale/syntax.html#regular-expressions
NOT_REGEX=INDEXABLE.concat(['++','--']);// Tokens that, when immediately preceding a `WHEN`, indicate that the `WHEN`
// occurs at the start of a line. We disambiguate these from trailing whens to
// avoid an ambiguity in the grammar.
LINE_BREAK=['INDENT','OUTDENT','TERMINATOR'];// Additional indent in front of these is ignored.
INDENTABLE_CLOSERS=[')','}',']'];// Tokens that, when appearing at the end of a line, suppress a following TERMINATOR/INDENT token
UNFINISHED=['\\','.','?.','?::','UNARY','MATH','UNARY_MATH','+','-','**','SHIFT','RELATION','COMPARE','&','^','|','&&','||','BIN?','EXTENDS'];return exports;};//#endregion
//#region URL: /parser
modules['/parser']=function(){/* parser generated by jison 0.4.18 *//*
			Returns a Parser object of the following structure:

			Parser: {
				yy: {}
			}

			Parser.prototype: {
				yy: {},
				trace: function(),
				symbols_: {associative list: name ==> number},
				terminals_: {associative list: number ==> name},
				productions_: [...],
				performAction: function anonymous(yytext, yyleng, yylineno, yy, yystate, $$, _$),
				table: [...],
				defaultActions: {...},
				parseError: function(str, hash),
				parse: function(input),

				lexer: {
						EOF: 1,
						parseError: function(str, hash),
						setInput: function(input),
						input: function(),
						unput: function(str),
						more: function(),
						less: function(n),
						pastInput: function(),
						upcomingInput: function(),
						showPosition: function(),
						test_match: function(regex_match_array, rule_index),
						next: function(),
						lex: function(),
						begin: function(condition),
						popState: function(),
						_currentRules: function(),
						topState: function(),
						pushState: function(condition),

						options: {
								ranges: boolean           (optional: true ==> token location info will include a .range[] member)
								flex: boolean             (optional: true ==> flex-like lexing behaviour where the rules are tested exhaustively to find the longest match)
								backtrack_lexer: boolean  (optional: true ==> lexer regexes are tested in order and for each matching regex the action code is invoked; the lexer terminates the scan when a token is returned by the action code)
						},

						performAction: function(yy, yy_, $avoiding_name_collisions, YY_START),
						rules: [...],
						conditions: {associative list: name ==> set},
				}
			}


			token location info (@$, _$, etc.): {
				first_line: n,
				last_line: n,
				first_column: n,
				last_column: n,
				range: [start_number, end_number]       (where the numbers are indexes into the input string, regular zero-based)
			}


			the parseError function receives a 'hash' object with these members for lexer and parser errors: {
				text:        (matched text)
				token:       (the produced terminal token, if any)
				line:        (yylineno)
			}
			while parser (grammar) errors will also provide these members, i.e. parser errors deliver a superset of attributes: {
				loc:         (yylloc)
				expected:    (string describing the set of expected tokens)
				recoverable: (boolean: TRUE when the parser has a error recovery rule available for this particular error)
			}
		*/var exports={};var parser=function(){var o=function o(k,v,_o,l){for(_o=_o||{},l=k.length;l--;_o[k[l]]=v){}return _o;},$V0=[1,24],$V1=[1,56],$V2=[1,91],$V3=[1,92],$V4=[1,87],$V5=[1,93],$V6=[1,94],$V7=[1,89],$V8=[1,90],$V9=[1,64],$Va=[1,66],$Vb=[1,67],$Vc=[1,68],$Vd=[1,69],$Ve=[1,70],$Vf=[1,72],$Vg=[1,73],$Vh=[1,58],$Vi=[1,42],$Vj=[1,36],$Vk=[1,76],$Vl=[1,77],$Vm=[1,86],$Vn=[1,54],$Vo=[1,59],$Vp=[1,60],$Vq=[1,74],$Vr=[1,75],$Vs=[1,47],$Vt=[1,55],$Vu=[1,71],$Vv=[1,81],$Vw=[1,82],$Vx=[1,83],$Vy=[1,84],$Vz=[1,53],$VA=[1,80],$VB=[1,38],$VC=[1,39],$VD=[1,40],$VE=[1,41],$VF=[1,43],$VG=[1,44],$VH=[1,95],$VI=[1,6,36,47,146],$VJ=[1,6,35,36,47,69,70,93,127,135,146,149,157],$VK=[1,113],$VL=[1,114],$VM=[1,115],$VN=[1,110],$VO=[1,98],$VP=[1,97],$VQ=[1,96],$VR=[1,99],$VS=[1,100],$VT=[1,101],$VU=[1,102],$VV=[1,103],$VW=[1,104],$VX=[1,105],$VY=[1,106],$VZ=[1,107],$V_=[1,108],$V$=[1,109],$V01=[1,117],$V11=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$V21=[2,196],$V31=[1,123],$V41=[1,128],$V51=[1,124],$V61=[1,125],$V71=[1,126],$V81=[1,129],$V91=[1,122],$Va1=[1,6,35,36,47,69,70,93,127,135,146,148,149,150,156,157,174],$Vb1=[1,6,35,36,45,46,47,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vc1=[2,122],$Vd1=[2,126],$Ve1=[6,35,88,93],$Vf1=[2,99],$Vg1=[1,141],$Vh1=[1,135],$Vi1=[1,140],$Vj1=[1,144],$Vk1=[1,149],$Vl1=[1,147],$Vm1=[1,151],$Vn1=[1,155],$Vo1=[1,153],$Vp1=[1,6,35,36,45,46,47,61,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vq1=[2,119],$Vr1=[1,6,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vs1=[2,31],$Vt1=[1,183],$Vu1=[2,86],$Vv1=[1,187],$Vw1=[1,193],$Vx1=[1,208],$Vy1=[1,203],$Vz1=[1,212],$VA1=[1,209],$VB1=[1,214],$VC1=[1,215],$VD1=[1,217],$VE1=[14,32,35,38,39,43,45,46,49,50,54,55,56,57,58,59,68,77,84,85,86,90,91,107,110,112,120,129,130,140,144,145,148,150,153,156,167,173,176,177,178,179,180,181],$VF1=[1,6,35,36,45,46,47,61,69,70,80,81,83,88,93,101,102,103,105,109,111,125,126,127,135,146,148,149,150,156,157,174,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194],$VG1=[1,228],$VH1=[1,229],$VI1=[2,142],$VJ1=[1,245],$VK1=[1,247],$VL1=[1,257],$VM1=[1,6,35,36,45,46,47,65,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$VN1=[1,6,33,35,36,45,46,47,61,65,69,70,80,81,83,88,93,101,102,103,105,109,111,117,125,126,127,135,146,148,149,150,156,157,164,165,166,174,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194],$VO1=[1,6,35,36,45,46,47,52,65,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$VP1=[1,287],$VQ1=[45,46,126],$VR1=[1,298],$VS1=[1,297],$VT1=[6,35],$VU1=[2,97],$VV1=[1,304],$VW1=[6,35,36,88,93],$VX1=[6,35,36,61,70,88,93],$VY1=[1,6,35,36,47,69,70,80,81,83,88,93,101,102,103,105,109,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$VZ1=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,178,179,183,184,185,186,187,188,189,190,191,192,193],$V_1=[2,348],$V$1=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,178,179,183,185,186,187,188,189,190,191,192,193],$V02=[45,46,80,81,101,102,103,105,125,126],$V12=[1,331],$V22=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174],$V32=[2,84],$V42=[1,347],$V52=[1,349],$V62=[1,354],$V72=[1,356],$V82=[6,35,69,93],$V92=[2,221],$Va2=[2,222],$Vb2=[1,6,35,36,45,46,47,61,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,164,165,166,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vc2=[1,370],$Vd2=[6,14,32,35,36,38,39,43,45,46,49,50,54,55,56,57,58,59,68,69,70,77,84,85,86,90,91,93,107,110,112,120,129,130,140,144,145,148,150,153,156,167,173,176,177,178,179,180,181],$Ve2=[6,35,36,69,93],$Vf2=[6,35,36,69,93,127],$Vg2=[1,6,35,36,45,46,47,61,65,69,70,80,81,83,88,93,101,102,103,105,109,111,125,126,127,135,146,148,149,150,156,157,164,165,166,174,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194],$Vh2=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,157,174],$Vi2=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,149,157,174],$Vj2=[2,273],$Vk2=[164,165,166],$Vl2=[93,164,165,166],$Vm2=[6,35,109],$Vn2=[1,395],$Vo2=[6,35,36,93,109],$Vp2=[6,35,36,65,93,109],$Vq2=[1,401],$Vr2=[1,402],$Vs2=[6,35,36,61,65,70,80,81,93,109,126],$Vt2=[6,35,36,70,80,81,93,109,126],$Vu2=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,178,179,185,186,187,188,189,190,191,192,193],$Vv2=[2,340],$Vw2=[2,339],$Vx2=[1,6,35,36,45,46,47,52,69,70,80,81,83,88,93,101,102,103,105,109,125,126,127,135,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vy2=[1,424],$Vz2=[14,32,38,39,43,45,46,49,50,54,55,56,57,58,59,68,77,83,84,85,86,90,91,107,110,112,120,129,130,140,144,145,148,150,153,156,167,173,176,177,178,179,180,181],$VA2=[2,207],$VB2=[6,35,36],$VC2=[2,98],$VD2=[1,433],$VE2=[1,434],$VF2=[1,6,35,36,47,69,70,80,81,83,88,93,101,102,103,105,109,127,135,142,143,146,148,149,150,156,157,169,171,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$VG2=[1,313],$VH2=[36,169,171],$VI2=[1,6,36,47,69,70,83,88,93,109,127,135,146,149,157,174],$VJ2=[1,469],$VK2=[1,475],$VL2=[1,6,35,36,47,69,70,93,127,135,146,149,157,174],$VM2=[2,113],$VN2=[1,488],$VO2=[1,489],$VP2=[6,35,36,69],$VQ2=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,169,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$VR2=[1,6,35,36,47,69,70,93,127,135,146,149,157,169],$VS2=[2,287],$VT2=[2,288],$VU2=[2,303],$VV2=[1,512],$VW2=[1,513],$VX2=[6,35,36,109],$VY2=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,150,156,157,174],$VZ2=[1,534],$V_2=[6,35,36,93,127],$V$2=[6,35,36,93],$V03=[1,6,35,36,47,69,70,83,88,93,109,127,135,142,146,148,149,150,156,157,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$V13=[35,93],$V23=[1,562],$V33=[1,563],$V43=[1,569],$V53=[1,570],$V63=[2,258],$V73=[2,261],$V83=[2,274],$V93=[1,619],$Va3=[1,620],$Vb3=[2,289],$Vc3=[2,293],$Vd3=[2,290],$Ve3=[2,294],$Vf3=[2,291],$Vg3=[2,292],$Vh3=[2,304],$Vi3=[2,305],$Vj3=[1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,174],$Vk3=[2,295],$Vl3=[2,297],$Vm3=[2,299],$Vn3=[2,301],$Vo3=[2,296],$Vp3=[2,298],$Vq3=[2,300],$Vr3=[2,302];var parser={trace:function trace(){},yy:{},symbols_:{"error":2,"Root":3,"Body":4,"Line":5,"TERMINATOR":6,"Expression":7,"ExpressionLine":8,"Statement":9,"FuncDirective":10,"YieldReturn":11,"AwaitReturn":12,"Return":13,"STATEMENT":14,"Import":15,"Export":16,"Value":17,"Code":18,"Operation":19,"Assign":20,"If":21,"Try":22,"While":23,"For":24,"Switch":25,"Class":26,"Throw":27,"Yield":28,"CodeLine":29,"IfLine":30,"OperationLine":31,"YIELD":32,"FROM":33,"Block":34,"INDENT":35,"OUTDENT":36,"Identifier":37,"IDENTIFIER":38,"CSX_TAG":39,"Property":40,"PROPERTY":41,"AlphaNumeric":42,"NUMBER":43,"String":44,"STRING":45,"STRING_START":46,"STRING_END":47,"Regex":48,"REGEX":49,"REGEX_START":50,"Invocation":51,"REGEX_END":52,"Literal":53,"JS":54,"UNDEFINED":55,"NULL":56,"BOOL":57,"INFINITY":58,"NAN":59,"Assignable":60,"=":61,"AssignObj":62,"ObjAssignable":63,"ObjRestValue":64,":":65,"SimpleObjAssignable":66,"ThisProperty":67,"[":68,"]":69,"...":70,"ObjSpreadExpr":71,"ObjSpreadIdentifier":72,"Object":73,"Parenthetical":74,"Super":75,"This":76,"SUPER":77,"Arguments":78,"ObjSpreadAccessor":79,".":80,"INDEX_START":81,"IndexValue":82,"INDEX_END":83,"RETURN":84,"AWAIT":85,"PARAM_START":86,"ParamList":87,"PARAM_END":88,"FuncGlyph":89,"->":90,"=>":91,"OptComma":92,",":93,"Param":94,"ParamVar":95,"Array":96,"Splat":97,"SimpleAssignable":98,"Accessor":99,"Range":100,"?.":101,"::":102,"?::":103,"Index":104,"INDEX_SOAK":105,"Slice":106,"{":107,"AssignList":108,"}":109,"CLASS":110,"EXTENDS":111,"IMPORT":112,"ImportDefaultSpecifier":113,"ImportNamespaceSpecifier":114,"ImportSpecifierList":115,"ImportSpecifier":116,"AS":117,"DEFAULT":118,"IMPORT_ALL":119,"EXPORT":120,"ExportSpecifierList":121,"EXPORT_ALL":122,"ExportSpecifier":123,"OptFuncExist":124,"FUNC_EXIST":125,"CALL_START":126,"CALL_END":127,"ArgList":128,"THIS":129,"@":130,"Elisions":131,"ArgElisionList":132,"OptElisions":133,"RangeDots":134,"..":135,"Arg":136,"ArgElision":137,"Elision":138,"SimpleArgs":139,"TRY":140,"Catch":141,"FINALLY":142,"CATCH":143,"THROW":144,"(":145,")":146,"WhileLineSource":147,"WHILE":148,"WHEN":149,"UNTIL":150,"WhileSource":151,"Loop":152,"LOOP":153,"ForBody":154,"ForLineBody":155,"FOR":156,"BY":157,"ForStart":158,"ForSource":159,"ForLineSource":160,"ForVariables":161,"OWN":162,"ForValue":163,"FORIN":164,"FOROF":165,"FORFROM":166,"SWITCH":167,"Whens":168,"ELSE":169,"When":170,"LEADING_WHEN":171,"IfBlock":172,"IF":173,"POST_IF":174,"IfBlockLine":175,"UNARY":176,"UNARY_MATH":177,"-":178,"+":179,"--":180,"++":181,"?":182,"MATH":183,"**":184,"SHIFT":185,"COMPARE":186,"&":187,"^":188,"|":189,"&&":190,"||":191,"BIN?":192,"RELATION":193,"COMPOUND_ASSIGN":194,"$accept":0,"$end":1},terminals_:{2:"error",6:"TERMINATOR",14:"STATEMENT",32:"YIELD",33:"FROM",35:"INDENT",36:"OUTDENT",38:"IDENTIFIER",39:"CSX_TAG",41:"PROPERTY",43:"NUMBER",45:"STRING",46:"STRING_START",47:"STRING_END",49:"REGEX",50:"REGEX_START",52:"REGEX_END",54:"JS",55:"UNDEFINED",56:"NULL",57:"BOOL",58:"INFINITY",59:"NAN",61:"=",65:":",68:"[",69:"]",70:"...",77:"SUPER",80:".",81:"INDEX_START",83:"INDEX_END",84:"RETURN",85:"AWAIT",86:"PARAM_START",88:"PARAM_END",90:"->",91:"=>",93:",",101:"?.",102:"::",103:"?::",105:"INDEX_SOAK",107:"{",109:"}",110:"CLASS",111:"EXTENDS",112:"IMPORT",117:"AS",118:"DEFAULT",119:"IMPORT_ALL",120:"EXPORT",122:"EXPORT_ALL",125:"FUNC_EXIST",126:"CALL_START",127:"CALL_END",129:"THIS",130:"@",135:"..",140:"TRY",142:"FINALLY",143:"CATCH",144:"THROW",145:"(",146:")",148:"WHILE",149:"WHEN",150:"UNTIL",153:"LOOP",156:"FOR",157:"BY",162:"OWN",164:"FORIN",165:"FOROF",166:"FORFROM",167:"SWITCH",169:"ELSE",171:"LEADING_WHEN",173:"IF",174:"POST_IF",176:"UNARY",177:"UNARY_MATH",178:"-",179:"+",180:"--",181:"++",182:"?",183:"MATH",184:"**",185:"SHIFT",186:"COMPARE",187:"&",188:"^",189:"|",190:"&&",191:"||",192:"BIN?",193:"RELATION",194:"COMPOUND_ASSIGN"},productions_:[0,[3,0],[3,1],[4,1],[4,3],[4,2],[5,1],[5,1],[5,1],[5,1],[10,1],[10,1],[9,1],[9,1],[9,1],[9,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[7,1],[8,1],[8,1],[8,1],[28,1],[28,2],[28,3],[34,2],[34,3],[37,1],[37,1],[40,1],[42,1],[42,1],[44,1],[44,3],[48,1],[48,3],[53,1],[53,1],[53,1],[53,1],[53,1],[53,1],[53,1],[53,1],[20,3],[20,4],[20,5],[62,1],[62,1],[62,3],[62,5],[62,3],[62,5],[66,1],[66,1],[66,1],[63,1],[63,3],[63,1],[64,2],[64,2],[64,2],[64,2],[71,1],[71,1],[71,1],[71,1],[71,1],[71,2],[71,2],[71,2],[72,2],[72,2],[79,2],[79,3],[13,2],[13,4],[13,1],[11,3],[11,2],[12,3],[12,2],[18,5],[18,2],[29,5],[29,2],[89,1],[89,1],[92,0],[92,1],[87,0],[87,1],[87,3],[87,4],[87,6],[94,1],[94,2],[94,2],[94,3],[94,1],[95,1],[95,1],[95,1],[95,1],[97,2],[97,2],[98,1],[98,2],[98,2],[98,1],[60,1],[60,1],[60,1],[17,1],[17,1],[17,1],[17,1],[17,1],[17,1],[17,1],[75,3],[75,4],[99,2],[99,2],[99,2],[99,2],[99,1],[99,1],[104,3],[104,2],[82,1],[82,1],[73,4],[108,0],[108,1],[108,3],[108,4],[108,6],[26,1],[26,2],[26,3],[26,4],[26,2],[26,3],[26,4],[26,5],[15,2],[15,4],[15,4],[15,5],[15,7],[15,6],[15,9],[115,1],[115,3],[115,4],[115,4],[115,6],[116,1],[116,3],[116,1],[116,3],[113,1],[114,3],[16,3],[16,5],[16,2],[16,4],[16,5],[16,6],[16,3],[16,5],[16,4],[16,7],[121,1],[121,3],[121,4],[121,4],[121,6],[123,1],[123,3],[123,3],[123,1],[123,3],[51,3],[51,3],[51,3],[124,0],[124,1],[78,2],[78,4],[76,1],[76,1],[67,2],[96,2],[96,3],[96,4],[134,1],[134,1],[100,5],[100,5],[106,3],[106,2],[106,3],[106,2],[106,2],[106,1],[128,1],[128,3],[128,4],[128,4],[128,6],[136,1],[136,1],[136,1],[136,1],[132,1],[132,3],[132,4],[132,4],[132,6],[137,1],[137,2],[133,1],[133,2],[131,1],[131,2],[138,1],[139,1],[139,1],[139,3],[139,3],[22,2],[22,3],[22,4],[22,5],[141,3],[141,3],[141,2],[27,2],[27,4],[74,3],[74,5],[147,2],[147,4],[147,2],[147,4],[151,2],[151,4],[151,4],[151,2],[151,4],[151,4],[23,2],[23,2],[23,2],[23,2],[23,1],[152,2],[152,2],[24,2],[24,2],[24,2],[24,2],[154,2],[154,4],[154,2],[155,4],[155,2],[158,2],[158,3],[158,3],[163,1],[163,1],[163,1],[163,1],[161,1],[161,3],[159,2],[159,2],[159,4],[159,4],[159,4],[159,4],[159,4],[159,4],[159,6],[159,6],[159,6],[159,6],[159,6],[159,6],[159,6],[159,6],[159,2],[159,4],[159,4],[160,2],[160,2],[160,4],[160,4],[160,4],[160,4],[160,4],[160,4],[160,6],[160,6],[160,6],[160,6],[160,6],[160,6],[160,6],[160,6],[160,2],[160,4],[160,4],[25,5],[25,5],[25,7],[25,7],[25,4],[25,6],[168,1],[168,2],[170,3],[170,4],[172,3],[172,5],[21,1],[21,3],[21,3],[21,3],[175,3],[175,5],[30,1],[30,3],[30,3],[30,3],[31,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,2],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,3],[19,5],[19,4]],performAction:function anonymous(yytext,yyleng,yylineno,yy,yystate/* action[1] */,$$/* vstack */,_$/* lstack */){/* this == yyval */var $0=$$.length-1;switch(yystate){case 1:return this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Block());break;case 2:return this.$=$$[$0];break;case 3:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(yy.Block.wrap([$$[$0]]));break;case 4:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])($$[$0-2].push($$[$0]));break;case 5:this.$=$$[$0-1];break;case 6:case 7:case 8:case 9:case 10:case 11:case 12:case 14:case 15:case 16:case 17:case 18:case 19:case 20:case 21:case 22:case 23:case 24:case 25:case 26:case 27:case 28:case 29:case 30:case 40:case 45:case 47:case 57:case 62:case 63:case 64:case 65:case 67:case 72:case 73:case 74:case 75:case 76:case 97:case 98:case 109:case 110:case 111:case 112:case 118:case 119:case 122:case 127:case 136:case 221:case 222:case 223:case 225:case 237:case 238:case 281:case 282:case 331:case 337:case 343:this.$=$$[$0];break;case 13:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.StatementLiteral($$[$0]));break;case 31:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Op($$[$0],new yy.Value(new yy.Literal(''))));break;case 32:case 347:case 348:case 349:case 352:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op($$[$0-1],$$[$0]));break;case 33:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Op($$[$0-2].concat($$[$0-1]),$$[$0]));break;case 34:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Block());break;case 35:case 83:case 137:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])($$[$0-1]);break;case 36:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.IdentifierLiteral($$[$0]));break;case 37:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.CSXTag($$[$0]));break;case 38:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.PropertyName($$[$0]));break;case 39:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.NumberLiteral($$[$0]));break;case 41:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.StringLiteral($$[$0]));break;case 42:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.StringWithInterpolations($$[$0-1]));break;case 43:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.RegexLiteral($$[$0]));break;case 44:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.RegexWithInterpolations($$[$0-1].args));break;case 46:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.PassthroughLiteral($$[$0]));break;case 48:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.UndefinedLiteral($$[$0]));break;case 49:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.NullLiteral($$[$0]));break;case 50:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.BooleanLiteral($$[$0]));break;case 51:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.InfinityLiteral($$[$0]));break;case 52:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.NaNLiteral($$[$0]));break;case 53:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Assign($$[$0-2],$$[$0]));break;case 54:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Assign($$[$0-3],$$[$0]));break;case 55:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Assign($$[$0-4],$$[$0-1]));break;case 56:case 115:case 120:case 121:case 123:case 124:case 125:case 126:case 128:case 283:case 284:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Value($$[$0]));break;case 58:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Assign(yy.addDataToNode(yy,_$[$0-2])(new yy.Value($$[$0-2])),$$[$0],'object',{operatorToken:yy.addDataToNode(yy,_$[$0-1])(new yy.Literal($$[$0-1]))}));break;case 59:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Assign(yy.addDataToNode(yy,_$[$0-4])(new yy.Value($$[$0-4])),$$[$0-1],'object',{operatorToken:yy.addDataToNode(yy,_$[$0-3])(new yy.Literal($$[$0-3]))}));break;case 60:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Assign(yy.addDataToNode(yy,_$[$0-2])(new yy.Value($$[$0-2])),$$[$0],null,{operatorToken:yy.addDataToNode(yy,_$[$0-1])(new yy.Literal($$[$0-1]))}));break;case 61:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Assign(yy.addDataToNode(yy,_$[$0-4])(new yy.Value($$[$0-4])),$$[$0-1],null,{operatorToken:yy.addDataToNode(yy,_$[$0-3])(new yy.Literal($$[$0-3]))}));break;case 66:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Value(new yy.ComputedPropertyName($$[$0-1])));break;case 68:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Splat(new yy.Value($$[$0-1])));break;case 69:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Splat(new yy.Value($$[$0])));break;case 70:case 113:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Splat($$[$0-1]));break;case 71:case 114:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Splat($$[$0]));break;case 77:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.SuperCall(yy.addDataToNode(yy,_$[$0-1])(new yy.Super()),$$[$0],false,$$[$0-1]));break;case 78:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Call(new yy.Value($$[$0-1]),$$[$0]));break;case 79:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Call($$[$0-1],$$[$0]));break;case 80:case 81:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Value($$[$0-1]).add($$[$0]));break;case 82:case 131:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Access($$[$0]));break;case 84:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Return($$[$0]));break;case 85:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Return(new yy.Value($$[$0-1])));break;case 86:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Return());break;case 87:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.YieldReturn($$[$0]));break;case 88:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.YieldReturn());break;case 89:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.AwaitReturn($$[$0]));break;case 90:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.AwaitReturn());break;case 91:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Code($$[$0-3],$$[$0],$$[$0-1],yy.addDataToNode(yy,_$[$0-4])(new yy.Literal($$[$0-4]))));break;case 92:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Code([],$$[$0],$$[$0-1]));break;case 93:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Code($$[$0-3],yy.addDataToNode(yy,_$[$0])(yy.Block.wrap([$$[$0]])),$$[$0-1],yy.addDataToNode(yy,_$[$0-4])(new yy.Literal($$[$0-4]))));break;case 94:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Code([],yy.addDataToNode(yy,_$[$0])(yy.Block.wrap([$$[$0]])),$$[$0-1]));break;case 95:case 96:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.FuncGlyph($$[$0]));break;case 99:case 142:case 232:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])([]);break;case 100:case 143:case 162:case 183:case 216:case 230:case 234:case 285:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])([$$[$0]]);break;case 101:case 144:case 163:case 184:case 217:case 226:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])($$[$0-2].concat($$[$0]));break;case 102:case 145:case 164:case 185:case 218:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])($$[$0-3].concat($$[$0]));break;case 103:case 146:case 166:case 187:case 220:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])($$[$0-5].concat($$[$0-2]));break;case 104:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Param($$[$0]));break;case 105:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Param($$[$0-1],null,true));break;case 106:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Param($$[$0],null,true));break;case 107:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Param($$[$0-2],$$[$0]));break;case 108:case 224:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Expansion());break;case 116:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0-1].add($$[$0]));break;case 117:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Value($$[$0-1]).add($$[$0]));break;case 129:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Super(yy.addDataToNode(yy,_$[$0])(new yy.Access($$[$0])),[],false,$$[$0-2]));break;case 130:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Super(yy.addDataToNode(yy,_$[$0-1])(new yy.Index($$[$0-1])),[],false,$$[$0-3]));break;case 132:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Access($$[$0],'soak'));break;case 133:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])([yy.addDataToNode(yy,_$[$0-1])(new yy.Access(new yy.PropertyName('prototype'))),yy.addDataToNode(yy,_$[$0])(new yy.Access($$[$0]))]);break;case 134:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])([yy.addDataToNode(yy,_$[$0-1])(new yy.Access(new yy.PropertyName('prototype'),'soak')),yy.addDataToNode(yy,_$[$0])(new yy.Access($$[$0]))]);break;case 135:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Access(new yy.PropertyName('prototype')));break;case 138:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(yy.extend($$[$0],{soak:true}));break;case 139:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Index($$[$0]));break;case 140:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Slice($$[$0]));break;case 141:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Obj($$[$0-2],$$[$0-3].generated));break;case 147:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Class());break;case 148:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Class(null,null,$$[$0]));break;case 149:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Class(null,$$[$0]));break;case 150:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Class(null,$$[$0-1],$$[$0]));break;case 151:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Class($$[$0]));break;case 152:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Class($$[$0-1],null,$$[$0]));break;case 153:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Class($$[$0-2],$$[$0]));break;case 154:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Class($$[$0-3],$$[$0-1],$$[$0]));break;case 155:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.ImportDeclaration(null,$$[$0]));break;case 156:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause($$[$0-2],null),$$[$0]));break;case 157:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause(null,$$[$0-2]),$$[$0]));break;case 158:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause(null,new yy.ImportSpecifierList([])),$$[$0]));break;case 159:this.$=yy.addDataToNode(yy,_$[$0-6],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause(null,new yy.ImportSpecifierList($$[$0-4])),$$[$0]));break;case 160:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause($$[$0-4],$$[$0-2]),$$[$0]));break;case 161:this.$=yy.addDataToNode(yy,_$[$0-8],_$[$0])(new yy.ImportDeclaration(new yy.ImportClause($$[$0-7],new yy.ImportSpecifierList($$[$0-4])),$$[$0]));break;case 165:case 186:case 199:case 219:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])($$[$0-2]);break;case 167:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.ImportSpecifier($$[$0]));break;case 168:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ImportSpecifier($$[$0-2],$$[$0]));break;case 169:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.ImportSpecifier(new yy.Literal($$[$0])));break;case 170:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ImportSpecifier(new yy.Literal($$[$0-2]),$$[$0]));break;case 171:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.ImportDefaultSpecifier($$[$0]));break;case 172:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ImportNamespaceSpecifier(new yy.Literal($$[$0-2]),$$[$0]));break;case 173:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ExportNamedDeclaration(new yy.ExportSpecifierList([])));break;case 174:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.ExportNamedDeclaration(new yy.ExportSpecifierList($$[$0-2])));break;case 175:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.ExportNamedDeclaration($$[$0]));break;case 176:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.ExportNamedDeclaration(new yy.Assign($$[$0-2],$$[$0],null,{moduleDeclaration:'export'})));break;case 177:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.ExportNamedDeclaration(new yy.Assign($$[$0-3],$$[$0],null,{moduleDeclaration:'export'})));break;case 178:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])(new yy.ExportNamedDeclaration(new yy.Assign($$[$0-4],$$[$0-1],null,{moduleDeclaration:'export'})));break;case 179:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ExportDefaultDeclaration($$[$0]));break;case 180:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.ExportDefaultDeclaration(new yy.Value($$[$0-1])));break;case 181:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.ExportAllDeclaration(new yy.Literal($$[$0-2]),$$[$0]));break;case 182:this.$=yy.addDataToNode(yy,_$[$0-6],_$[$0])(new yy.ExportNamedDeclaration(new yy.ExportSpecifierList($$[$0-4]),$$[$0]));break;case 188:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.ExportSpecifier($$[$0]));break;case 189:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ExportSpecifier($$[$0-2],$$[$0]));break;case 190:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ExportSpecifier($$[$0-2],new yy.Literal($$[$0])));break;case 191:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.ExportSpecifier(new yy.Literal($$[$0])));break;case 192:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.ExportSpecifier(new yy.Literal($$[$0-2]),$$[$0]));break;case 193:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.TaggedTemplateCall($$[$0-2],$$[$0],$$[$0-1]));break;case 194:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Call($$[$0-2],$$[$0],$$[$0-1]));break;case 195:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.SuperCall(yy.addDataToNode(yy,_$[$0-2])(new yy.Super()),$$[$0],$$[$0-1],$$[$0-2]));break;case 196:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(false);break;case 197:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(true);break;case 198:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])([]);break;case 200:case 201:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Value(new yy.ThisLiteral($$[$0])));break;case 202:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Value(yy.addDataToNode(yy,_$[$0-1])(new yy.ThisLiteral($$[$0-1])),[yy.addDataToNode(yy,_$[$0])(new yy.Access($$[$0]))],'this'));break;case 203:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Arr([]));break;case 204:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Arr($$[$0-1]));break;case 205:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Arr([].concat($$[$0-2],$$[$0-1])));break;case 206:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])('inclusive');break;case 207:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])('exclusive');break;case 208:case 209:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Range($$[$0-3],$$[$0-1],$$[$0-2]));break;case 210:case 212:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Range($$[$0-2],$$[$0],$$[$0-1]));break;case 211:case 213:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Range($$[$0-1],null,$$[$0]));break;case 214:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Range(null,$$[$0],$$[$0-1]));break;case 215:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Range(null,null,$$[$0]));break;case 227:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])($$[$0-3].concat($$[$0-2],$$[$0]));break;case 228:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])($$[$0-2].concat($$[$0-1]));break;case 229:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])($$[$0-5].concat($$[$0-4],$$[$0-2],$$[$0-1]));break;case 231:case 235:case 332:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0-1].concat($$[$0]));break;case 233:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])([].concat($$[$0]));break;case 236:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])(new yy.Elision());break;case 239:case 240:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])([].concat($$[$0-2],$$[$0]));break;case 241:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Try($$[$0]));break;case 242:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Try($$[$0-1],$$[$0][0],$$[$0][1]));break;case 243:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Try($$[$0-2],null,null,$$[$0]));break;case 244:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Try($$[$0-3],$$[$0-2][0],$$[$0-2][1],$$[$0]));break;case 245:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])([$$[$0-1],$$[$0]]);break;case 246:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])([yy.addDataToNode(yy,_$[$0-1])(new yy.Value($$[$0-1])),$$[$0]]);break;case 247:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])([null,$$[$0]]);break;case 248:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Throw($$[$0]));break;case 249:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Throw(new yy.Value($$[$0-1])));break;case 250:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Parens($$[$0-1]));break;case 251:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Parens($$[$0-2]));break;case 252:case 256:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.While($$[$0]));break;case 253:case 257:case 258:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.While($$[$0-2],{guard:$$[$0]}));break;case 254:case 259:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.While($$[$0],{invert:true}));break;case 255:case 260:case 261:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.While($$[$0-2],{invert:true,guard:$$[$0]}));break;case 262:case 263:case 271:case 272:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0-1].addBody($$[$0]));break;case 264:case 265:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0].addBody(yy.addDataToNode(yy,_$[$0-1])(yy.Block.wrap([$$[$0-1]]))));break;case 266:this.$=yy.addDataToNode(yy,_$[$0],_$[$0])($$[$0]);break;case 267:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.While(yy.addDataToNode(yy,_$[$0-1])(new yy.BooleanLiteral('true'))).addBody($$[$0]));break;case 268:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.While(yy.addDataToNode(yy,_$[$0-1])(new yy.BooleanLiteral('true'))).addBody(yy.addDataToNode(yy,_$[$0])(yy.Block.wrap([$$[$0]]))));break;case 269:case 270:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0].addBody($$[$0-1]));break;case 273:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.For([],{source:yy.addDataToNode(yy,_$[$0])(new yy.Value($$[$0]))}));break;case 274:case 276:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.For([],{source:yy.addDataToNode(yy,_$[$0-2])(new yy.Value($$[$0-2])),step:$$[$0]}));break;case 275:case 277:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])($$[$0-1].addSource($$[$0]));break;case 278:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.For([],{name:$$[$0][0],index:$$[$0][1]}));break;case 279:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(function(){var index,name;var _$$$$=_slicedToArray($$[$0],2);name=_$$$$[0];index=_$$$$[1];return new yy.For([],{name:name,index:index,await:true,awaitTag:yy.addDataToNode(yy,_$[$0-1])(new yy.Literal($$[$0-1]))});}());break;case 280:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(function(){var index,name;var _$$$$2=_slicedToArray($$[$0],2);name=_$$$$2[0];index=_$$$$2[1];return new yy.For([],{name:name,index:index,own:true,ownTag:yy.addDataToNode(yy,_$[$0-1])(new yy.Literal($$[$0-1]))});}());break;case 286:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])([$$[$0-2],$$[$0]]);break;case 287:case 306:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])({source:$$[$0]});break;case 288:case 307:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])({source:$$[$0],object:true});break;case 289:case 290:case 308:case 309:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])({source:$$[$0-2],guard:$$[$0]});break;case 291:case 292:case 310:case 311:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])({source:$$[$0-2],guard:$$[$0],object:true});break;case 293:case 294:case 312:case 313:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])({source:$$[$0-2],step:$$[$0]});break;case 295:case 296:case 297:case 298:case 314:case 315:case 316:case 317:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])({source:$$[$0-4],guard:$$[$0-2],step:$$[$0]});break;case 299:case 300:case 301:case 302:case 318:case 319:case 320:case 321:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])({source:$$[$0-4],step:$$[$0-2],guard:$$[$0]});break;case 303:case 322:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])({source:$$[$0],from:true});break;case 304:case 305:case 323:case 324:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])({source:$$[$0-2],guard:$$[$0],from:true});break;case 325:case 326:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Switch($$[$0-3],$$[$0-1]));break;case 327:case 328:this.$=yy.addDataToNode(yy,_$[$0-6],_$[$0])(new yy.Switch($$[$0-5],$$[$0-3],$$[$0-1]));break;case 329:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Switch(null,$$[$0-1]));break;case 330:this.$=yy.addDataToNode(yy,_$[$0-5],_$[$0])(new yy.Switch(null,$$[$0-3],$$[$0-1]));break;case 333:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])([[$$[$0-1],$$[$0]]]);break;case 334:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])([[$$[$0-2],$$[$0-1]]]);break;case 335:case 341:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.If($$[$0-1],$$[$0],{type:$$[$0-2]}));break;case 336:case 342:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])($$[$0-4].addElse(yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.If($$[$0-1],$$[$0],{type:$$[$0-2]}))));break;case 338:case 344:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])($$[$0-2].addElse($$[$0]));break;case 339:case 340:case 345:case 346:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.If($$[$0],yy.addDataToNode(yy,_$[$0-2])(yy.Block.wrap([$$[$0-2]])),{type:$$[$0-1],statement:true}));break;case 350:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('-',$$[$0]));break;case 351:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('+',$$[$0]));break;case 353:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('--',$$[$0]));break;case 354:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('++',$$[$0]));break;case 355:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('--',$$[$0-1],null,true));break;case 356:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Op('++',$$[$0-1],null,true));break;case 357:this.$=yy.addDataToNode(yy,_$[$0-1],_$[$0])(new yy.Existence($$[$0-1]));break;case 358:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Op('+',$$[$0-2],$$[$0]));break;case 359:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Op('-',$$[$0-2],$$[$0]));break;case 360:case 361:case 362:case 363:case 364:case 365:case 366:case 367:case 368:case 369:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Op($$[$0-1],$$[$0-2],$$[$0]));break;case 370:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(function(){if($$[$0-1].charAt(0)==='!'){return new yy.Op($$[$0-1].slice(1),$$[$0-2],$$[$0]).invert();}else{return new yy.Op($$[$0-1],$$[$0-2],$$[$0]);}}());break;case 371:this.$=yy.addDataToNode(yy,_$[$0-2],_$[$0])(new yy.Assign($$[$0-2],$$[$0],$$[$0-1]));break;case 372:this.$=yy.addDataToNode(yy,_$[$0-4],_$[$0])(new yy.Assign($$[$0-4],$$[$0-1],$$[$0-3]));break;case 373:this.$=yy.addDataToNode(yy,_$[$0-3],_$[$0])(new yy.Assign($$[$0-3],$$[$0],$$[$0-2]));break;}},table:[{1:[2,1],3:1,4:2,5:3,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{1:[3]},{1:[2,2],6:$VH},o($VI,[2,3]),o($VJ,[2,6],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($VJ,[2,7]),o($VJ,[2,8],{158:116,151:118,154:119,148:$VK,150:$VL,156:$VM,174:$V01}),o($VJ,[2,9]),o($V11,[2,16],{124:120,99:121,104:127,45:$V21,46:$V21,126:$V21,80:$V31,81:$V41,101:$V51,102:$V61,103:$V71,105:$V81,125:$V91}),o($V11,[2,17],{104:127,99:130,80:$V31,81:$V41,101:$V51,102:$V61,103:$V71,105:$V81}),o($V11,[2,18]),o($V11,[2,19]),o($V11,[2,20]),o($V11,[2,21]),o($V11,[2,22]),o($V11,[2,23]),o($V11,[2,24]),o($V11,[2,25]),o($V11,[2,26]),o($V11,[2,27]),o($VJ,[2,28]),o($VJ,[2,29]),o($VJ,[2,30]),o($Va1,[2,12]),o($Va1,[2,13]),o($Va1,[2,14]),o($Va1,[2,15]),o($VJ,[2,10]),o($VJ,[2,11]),o($Vb1,$Vc1,{61:[1,131]}),o($Vb1,[2,123]),o($Vb1,[2,124]),o($Vb1,[2,125]),o($Vb1,$Vd1),o($Vb1,[2,127]),o($Vb1,[2,128]),o($Ve1,$Vf1,{87:132,94:133,95:134,37:136,67:137,96:138,73:139,38:$V2,39:$V3,68:$Vg1,70:$Vh1,107:$Vm,130:$Vi1}),{5:143,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,34:142,35:$Vj1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:145,8:146,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:150,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:156,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:157,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:158,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:[1,159],85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{17:161,18:162,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:163,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:160,100:32,107:$Vm,129:$Vq,130:$Vr,145:$Vu},{17:161,18:162,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:163,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:164,100:32,107:$Vm,129:$Vq,130:$Vr,145:$Vu},o($Vp1,$Vq1,{180:[1,165],181:[1,166],194:[1,167]}),o($V11,[2,337],{169:[1,168]}),{34:169,35:$Vj1},{34:170,35:$Vj1},{34:171,35:$Vj1},o($V11,[2,266]),{34:172,35:$Vj1},{34:173,35:$Vj1},{7:174,8:175,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:[1,176],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vr1,[2,147],{53:30,74:31,100:32,51:33,76:34,75:35,96:61,73:62,42:63,48:65,37:78,67:79,44:88,89:152,17:161,18:162,60:163,34:177,98:179,35:$Vj1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,111:[1,178],129:$Vq,130:$Vr,145:$Vu}),{7:180,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,181],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o([1,6,35,36,47,69,70,93,127,135,146,148,149,150,156,157,174,182,183,184,185,186,187,188,189,190,191,192,193],$Vs1,{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:182,14:$V0,32:$Vk1,33:$Vt1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:[1,184],85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,153:$Vx,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($VJ,[2,343],{169:[1,185]}),o([1,6,36,47,69,70,93,127,135,146,148,149,150,156,157,174],$Vu1,{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:186,14:$V0,32:$Vk1,35:$Vv1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,153:$Vx,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),{37:192,38:$V2,39:$V3,44:188,45:$V5,46:$V6,107:[1,191],113:189,114:190,119:$Vw1},{26:195,37:196,38:$V2,39:$V3,107:[1,194],110:$Vn,118:[1,197],122:[1,198]},o($Vp1,[2,120]),o($Vp1,[2,121]),o($Vb1,[2,45]),o($Vb1,[2,46]),o($Vb1,[2,47]),o($Vb1,[2,48]),o($Vb1,[2,49]),o($Vb1,[2,50]),o($Vb1,[2,51]),o($Vb1,[2,52]),{4:199,5:3,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,35:[1,200],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:201,8:202,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vx1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,69:$Vy1,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,131:204,132:205,136:210,137:207,138:206,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{80:$VB1,81:$VC1,124:213,125:$V91,126:$V21},o($Vb1,[2,200]),o($Vb1,[2,201],{40:216,41:$VD1}),o($VE1,[2,95]),o($VE1,[2,96]),o($VF1,[2,115]),o($VF1,[2,118]),{7:218,8:219,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:220,8:221,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:222,8:223,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:225,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,34:224,35:$Vj1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{37:231,38:$V2,39:$V3,67:232,68:$Vf,73:234,85:$VG1,96:233,100:226,107:$Vm,130:$Vi1,161:227,162:$VH1,163:230},{159:235,160:236,164:[1,237],165:[1,238],166:[1,239]},o([6,35,93,109],$VI1,{44:88,108:240,62:241,63:242,64:243,66:244,42:246,71:248,37:249,40:250,67:251,72:252,73:253,74:254,75:255,76:256,38:$V2,39:$V3,41:$VD1,43:$V4,45:$V5,46:$V6,68:$VJ1,70:$VK1,77:$VL1,107:$Vm,129:$Vq,130:$Vr,145:$Vu}),o($VM1,[2,39]),o($VM1,[2,40]),o($Vb1,[2,43]),{17:161,18:162,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:258,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:163,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:259,100:32,107:$Vm,129:$Vq,130:$Vr,145:$Vu},o($VN1,[2,36]),o($VN1,[2,37]),o($VO1,[2,41]),{4:260,5:3,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VI,[2,5],{7:4,8:5,9:6,10:7,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,13:23,15:25,16:26,11:27,12:28,60:29,53:30,74:31,100:32,51:33,76:34,75:35,89:37,98:45,172:46,151:48,147:49,152:50,154:51,155:52,175:57,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,5:261,14:$V0,32:$V1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vi,86:$Vj,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vv,150:$Vw,153:$Vx,156:$Vy,167:$Vz,173:$VA,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($V11,[2,357]),{7:262,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:263,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:264,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:265,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:266,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:267,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:268,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:269,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:270,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:271,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:272,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:273,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:274,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:275,8:276,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V11,[2,265]),o($V11,[2,270]),{7:220,8:277,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:222,8:278,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{37:231,38:$V2,39:$V3,67:232,68:$Vf,73:234,85:$VG1,96:233,100:279,107:$Vm,130:$Vi1,161:227,162:$VH1,163:230},{159:235,164:[1,280],165:[1,281],166:[1,282]},{7:283,8:284,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V11,[2,264]),o($V11,[2,269]),{44:285,45:$V5,46:$V6,78:286,126:$VP1},o($VF1,[2,116]),o($VQ1,[2,197]),{40:288,41:$VD1},{40:289,41:$VD1},o($VF1,[2,135],{40:290,41:$VD1}),{40:291,41:$VD1},o($VF1,[2,136]),{7:293,8:295,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$VR1,73:62,74:31,75:35,76:34,77:$Vg,82:292,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,106:294,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,134:296,135:$VS1,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{81:$V41,104:299,105:$V81},o($VF1,[2,117]),{6:[1,301],7:300,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,302],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VT1,$VU1,{92:305,88:[1,303],93:$VV1}),o($VW1,[2,100]),o($VW1,[2,104],{61:[1,307],70:[1,306]}),o($VW1,[2,108],{37:136,67:137,96:138,73:139,95:308,38:$V2,39:$V3,68:$Vg1,107:$Vm,130:$Vi1}),o($VX1,[2,109]),o($VX1,[2,110]),o($VX1,[2,111]),o($VX1,[2,112]),{40:216,41:$VD1},{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vx1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,69:$Vy1,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,131:204,132:205,136:210,137:207,138:206,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VY1,[2,92]),o($VJ,[2,94]),{4:312,5:3,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,36:[1,311],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VZ1,$V_1,{151:111,154:112,158:116,182:$VQ}),o($VJ,[2,347]),{7:158,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{148:$VK,150:$VL,151:118,154:119,156:$VM,158:116,174:$V01},o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,182,183,184,185,186,187,188,189,190,191,192,193],$Vs1,{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:182,14:$V0,32:$Vk1,33:$Vt1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,153:$Vx,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($V$1,[2,349],{151:111,154:112,158:116,182:$VQ,184:$VS}),o($Ve1,$Vf1,{94:133,95:134,37:136,67:137,96:138,73:139,87:314,38:$V2,39:$V3,68:$Vg1,70:$Vh1,107:$Vm,130:$Vi1}),{34:142,35:$Vj1},{7:315,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{148:$VK,150:$VL,151:118,154:119,156:$VM,158:116,174:[1,316]},{7:317,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V$1,[2,350],{151:111,154:112,158:116,182:$VQ,184:$VS}),o($V$1,[2,351],{151:111,154:112,158:116,182:$VQ,184:$VS}),o($VZ1,[2,352],{151:111,154:112,158:116,182:$VQ}),o($VJ,[2,90],{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:318,14:$V0,32:$Vk1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vu1,150:$Vu1,156:$Vu1,174:$Vu1,153:$Vx,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($V11,[2,353],{45:$Vq1,46:$Vq1,80:$Vq1,81:$Vq1,101:$Vq1,102:$Vq1,103:$Vq1,105:$Vq1,125:$Vq1,126:$Vq1}),o($VQ1,$V21,{124:120,99:121,104:127,80:$V31,81:$V41,101:$V51,102:$V61,103:$V71,105:$V81,125:$V91}),{80:$V31,81:$V41,99:130,101:$V51,102:$V61,103:$V71,104:127,105:$V81},o($V02,$Vc1),o($V11,[2,354],{45:$Vq1,46:$Vq1,80:$Vq1,81:$Vq1,101:$Vq1,102:$Vq1,103:$Vq1,105:$Vq1,125:$Vq1,126:$Vq1}),o($V11,[2,355]),o($V11,[2,356]),{6:[1,321],7:319,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,320],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{34:322,35:$Vj1,173:[1,323]},o($V11,[2,241],{141:324,142:[1,325],143:[1,326]}),o($V11,[2,262]),o($V11,[2,263]),o($V11,[2,271]),o($V11,[2,272]),{35:[1,327],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[1,328]},{168:329,170:330,171:$V12},o($V11,[2,148]),{7:332,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vr1,[2,151],{34:333,35:$Vj1,45:$Vq1,46:$Vq1,80:$Vq1,81:$Vq1,101:$Vq1,102:$Vq1,103:$Vq1,105:$Vq1,125:$Vq1,126:$Vq1,111:[1,334]}),o($V22,[2,248],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{73:335,107:$Vm},o($V22,[2,32],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:336,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o([1,6,36,47,69,70,93,127,135,146,149,157],[2,88],{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:337,14:$V0,32:$Vk1,35:$Vv1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vu1,150:$Vu1,156:$Vu1,174:$Vu1,153:$Vx,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),{34:338,35:$Vj1,173:[1,339]},o($Va1,$V32,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{73:340,107:$Vm},o($Va1,[2,155]),{33:[1,341],93:[1,342]},{33:[1,343]},{35:$V42,37:348,38:$V2,39:$V3,109:[1,344],115:345,116:346,118:$V52},o([33,93],[2,171]),{117:[1,350]},{35:$V62,37:355,38:$V2,39:$V3,109:[1,351],118:$V72,121:352,123:353},o($Va1,[2,175]),{61:[1,357]},{7:358,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,359],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{33:[1,360]},{6:$VH,146:[1,361]},{4:362,5:3,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V82,$V92,{151:111,154:112,158:116,134:363,70:[1,364],135:$VS1,148:$VK,150:$VL,156:$VM,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V82,$Va2,{134:365,70:$VR1,135:$VS1}),o($Vb2,[2,203]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,69:[1,366],70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,136:368,138:367,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o([6,35,69],$VU1,{133:369,92:371,93:$Vc2}),o($Vd2,[2,234]),o($Ve2,[2,225]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vx1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,131:373,132:372,136:210,137:207,138:206,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vd2,[2,236]),o($Ve2,[2,230]),o($Vf2,[2,223]),o($Vf2,[2,224],{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,98:45,172:46,151:48,147:49,152:50,154:51,155:52,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,89:152,9:154,7:374,14:$V0,32:$Vk1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vv,150:$Vw,153:$Vx,156:$Vy,167:$Vz,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),{78:375,126:$VP1},{40:376,41:$VD1},{7:377,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vg2,[2,202]),o($Vg2,[2,38]),{34:378,35:$Vj1,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{34:379,35:$Vj1},o($Vh2,[2,256],{151:111,154:112,158:116,148:$VK,149:[1,380],150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{35:[2,252],149:[1,381]},o($Vh2,[2,259],{151:111,154:112,158:116,148:$VK,149:[1,382],150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{35:[2,254],149:[1,383]},o($V11,[2,267]),o($Vi2,[2,268],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{35:$Vj2,157:[1,384]},o($Vk2,[2,278]),{37:231,38:$V2,39:$V3,67:232,68:$Vg1,73:234,96:233,107:$Vm,130:$Vi1,161:385,163:230},{37:231,38:$V2,39:$V3,67:232,68:$Vg1,73:234,96:233,107:$Vm,130:$Vi1,161:386,163:230},o($Vk2,[2,285],{93:[1,387]}),o($Vl2,[2,281]),o($Vl2,[2,282]),o($Vl2,[2,283]),o($Vl2,[2,284]),o($V11,[2,275]),{35:[2,277]},{7:388,8:389,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:390,8:391,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:392,8:393,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vm2,$VU1,{92:394,93:$Vn2}),o($Vo2,[2,143]),o($Vo2,[2,56],{65:[1,396]}),o($Vo2,[2,57]),o($Vp2,[2,65],{78:399,79:400,61:[1,397],70:[1,398],80:$Vq2,81:$Vr2,126:$VP1}),{7:403,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vp2,[2,67]),{37:249,38:$V2,39:$V3,40:250,41:$VD1,66:404,67:251,71:405,72:252,73:253,74:254,75:255,76:256,77:$VL1,107:$Vm,129:$Vq,130:$Vr,145:$Vu},{70:[1,406],78:407,79:408,80:$Vq2,81:$Vr2,126:$VP1},o($Vs2,[2,62]),o($Vs2,[2,63]),o($Vs2,[2,64]),o($Vt2,[2,72]),o($Vt2,[2,73]),o($Vt2,[2,74]),o($Vt2,[2,75]),o($Vt2,[2,76]),{78:409,80:$VB1,81:$VC1,126:$VP1},o($V02,$Vd1,{52:[1,410]}),o($V02,$Vq1),{6:$VH,47:[1,411]},o($VI,[2,4]),o($Vu2,[2,358],{151:111,154:112,158:116,182:$VQ,183:$VR,184:$VS}),o($Vu2,[2,359],{151:111,154:112,158:116,182:$VQ,183:$VR,184:$VS}),o($V$1,[2,360],{151:111,154:112,158:116,182:$VQ,184:$VS}),o($V$1,[2,361],{151:111,154:112,158:116,182:$VQ,184:$VS}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,185,186,187,188,189,190,191,192,193],[2,362],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,186,187,188,189,190,191,192],[2,363],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,187,188,189,190,191,192],[2,364],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,188,189,190,191,192],[2,365],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,189,190,191,192],[2,366],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,190,191,192],[2,367],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,191,192],[2,368],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,192],[2,369],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,193:$V$}),o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,157,174,186,187,188,189,190,191,192,193],[2,370],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT}),o($Vi2,$Vv2,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($VJ,[2,346]),{149:[1,412]},{149:[1,413]},o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,149,150,156,174,178,179,182,183,184,185,186,187,188,189,190,191,192,193],$Vj2,{157:[1,414]}),{7:415,8:416,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:417,8:418,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:419,8:420,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vi2,$Vw2,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($VJ,[2,345]),o($Vx2,[2,193]),o($Vx2,[2,194]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vy2,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,127:[1,421],128:422,129:$Vq,130:$Vr,136:423,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VF1,[2,131]),o($VF1,[2,132]),o($VF1,[2,133]),o($VF1,[2,134]),{83:[1,425]},{70:$VR1,83:[2,139],134:426,135:$VS1,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{83:[2,140]},{70:$VR1,134:427,135:$VS1},{7:428,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,83:[2,215],84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vz2,[2,206]),o($Vz2,$VA2),o($VF1,[2,138]),o($V22,[2,53],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:429,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:430,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{89:431,90:$Vk,91:$Vl},o($VB2,$VC2,{95:134,37:136,67:137,96:138,73:139,94:432,38:$V2,39:$V3,68:$Vg1,70:$Vh1,107:$Vm,130:$Vi1}),{6:$VD2,35:$VE2},o($VW1,[2,105]),{7:435,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VW1,[2,106]),o($Vf2,$V92,{151:111,154:112,158:116,70:[1,436],148:$VK,150:$VL,156:$VM,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vf2,$Va2),o($VF2,[2,34]),{6:$VH,36:[1,437]},{7:438,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VT1,$VU1,{92:305,88:[1,439],93:$VV1}),o($VZ1,$V_1,{151:111,154:112,158:116,182:$VQ}),{7:440,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{34:378,35:$Vj1,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($VJ,[2,89],{151:111,154:112,158:116,148:$V32,150:$V32,156:$V32,174:$V32,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,[2,371],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:441,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:442,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V11,[2,338]),{7:443,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V11,[2,242],{142:[1,444]}),{34:445,35:$Vj1},{34:448,35:$Vj1,37:446,38:$V2,39:$V3,73:447,107:$Vm},{168:449,170:330,171:$V12},{168:450,170:330,171:$V12},{36:[1,451],169:[1,452],170:453,171:$V12},o($VH2,[2,331]),{7:455,8:456,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,139:454,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VI2,[2,149],{151:111,154:112,158:116,34:457,35:$Vj1,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V11,[2,152]),{7:458,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{36:[1,459]},o($V22,[2,33],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($VJ,[2,87],{151:111,154:112,158:116,148:$V32,150:$V32,156:$V32,174:$V32,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($VJ,[2,344]),{7:461,8:460,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{36:[1,462]},{44:463,45:$V5,46:$V6},{107:[1,465],114:464,119:$Vw1},{44:466,45:$V5,46:$V6},{33:[1,467]},o($Vm2,$VU1,{92:468,93:$VJ2}),o($Vo2,[2,162]),{35:$V42,37:348,38:$V2,39:$V3,115:470,116:346,118:$V52},o($Vo2,[2,167],{117:[1,471]}),o($Vo2,[2,169],{117:[1,472]}),{37:473,38:$V2,39:$V3},o($Va1,[2,173]),o($Vm2,$VU1,{92:474,93:$VK2}),o($Vo2,[2,183]),{35:$V62,37:355,38:$V2,39:$V3,118:$V72,121:476,123:353},o($Vo2,[2,188],{117:[1,477]}),o($Vo2,[2,191],{117:[1,478]}),{6:[1,480],7:479,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,481],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VL2,[2,179],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{73:482,107:$Vm},{44:483,45:$V5,46:$V6},o($Vb1,[2,250]),{6:$VH,36:[1,484]},{7:485,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o([14,32,38,39,43,45,46,49,50,54,55,56,57,58,59,68,77,84,85,86,90,91,107,110,112,120,129,130,140,144,145,148,150,153,156,167,173,176,177,178,179,180,181],$VA2,{6:$VM2,35:$VM2,69:$VM2,93:$VM2}),{7:486,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vb2,[2,204]),o($Vd2,[2,235]),o($Ve2,[2,231]),{6:$VN2,35:$VO2,69:[1,487]},o($VP2,$VC2,{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,89:37,98:45,172:46,151:48,147:49,152:50,154:51,155:52,175:57,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,9:148,138:206,136:210,97:211,7:309,8:310,137:490,131:491,14:$V0,32:$Vk1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,70:$Vz1,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,90:$Vk,91:$Vl,93:$VA1,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vv,150:$Vw,153:$Vx,156:$Vy,167:$Vz,173:$VA,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($VP2,[2,232]),o($VB2,$VU1,{92:371,133:492,93:$Vc2}),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,136:368,138:367,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vf2,[2,114],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vx2,[2,195]),o($Vb1,[2,129]),{83:[1,493],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($VQ2,[2,335]),o($VR2,[2,341]),{7:494,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:495,8:496,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:497,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:498,8:499,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:500,8:501,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vk2,[2,279]),o($Vk2,[2,280]),{37:231,38:$V2,39:$V3,67:232,68:$Vg1,73:234,96:233,107:$Vm,130:$Vi1,163:502},{35:$VS2,148:$VK,149:[1,503],150:$VL,151:111,154:112,156:$VM,157:[1,504],158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,306],149:[1,505],157:[1,506]},{35:$VT2,148:$VK,149:[1,507],150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,307],149:[1,508]},{35:$VU2,148:$VK,149:[1,509],150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,322],149:[1,510]},{6:$VV2,35:$VW2,109:[1,511]},o($VX2,$VC2,{44:88,63:242,64:243,66:244,42:246,71:248,37:249,40:250,67:251,72:252,73:253,74:254,75:255,76:256,62:514,38:$V2,39:$V3,41:$VD1,43:$V4,45:$V5,46:$V6,68:$VJ1,70:$VK1,77:$VL1,107:$Vm,129:$Vq,130:$Vr,145:$Vu}),{7:515,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,516],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:517,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,35:[1,518],37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vo2,[2,68]),o($Vt2,[2,78]),o($Vt2,[2,80]),{40:519,41:$VD1},{7:293,8:295,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$VR1,73:62,74:31,75:35,76:34,77:$Vg,82:520,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,106:294,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,134:296,135:$VS1,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{69:[1,521],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($Vo2,[2,69],{78:399,79:400,80:$Vq2,81:$Vr2,126:$VP1}),o($Vo2,[2,71],{78:407,79:408,80:$Vq2,81:$Vr2,126:$VP1}),o($Vo2,[2,70]),o($Vt2,[2,79]),o($Vt2,[2,81]),o($Vt2,[2,77]),o($Vb1,[2,44]),o($VO1,[2,42]),{7:522,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:523,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:524,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o([1,6,35,36,47,69,70,83,88,93,109,127,135,146,148,150,156,174],$VS2,{151:111,154:112,158:116,149:[1,525],157:[1,526],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{149:[1,527],157:[1,528]},o($VY2,$VT2,{151:111,154:112,158:116,149:[1,529],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{149:[1,530]},o($VY2,$VU2,{151:111,154:112,158:116,149:[1,531],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{149:[1,532]},o($Vx2,[2,198]),o([6,35,127],$VU1,{92:533,93:$VZ2}),o($V_2,[2,216]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vy2,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,128:535,129:$Vq,130:$Vr,136:423,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VF1,[2,137]),{7:536,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,83:[2,211],84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:537,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,83:[2,213],84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{83:[2,214],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($V22,[2,54],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{36:[1,538],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{5:540,7:4,8:5,9:6,10:7,11:27,12:28,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$V1,34:539,35:$Vj1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vi,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($VW1,[2,101]),{37:136,38:$V2,39:$V3,67:137,68:$Vg1,70:$Vh1,73:139,94:541,95:134,96:138,107:$Vm,130:$Vi1},o($V$2,$Vf1,{94:133,95:134,37:136,67:137,96:138,73:139,87:542,38:$V2,39:$V3,68:$Vg1,70:$Vh1,107:$Vm,130:$Vi1}),o($VW1,[2,107],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vf2,$VM2),o($VF2,[2,35]),o($Vi2,$Vv2,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{89:543,90:$Vk,91:$Vl},o($Vi2,$Vw2,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{36:[1,544],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($V22,[2,373],{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{34:545,35:$Vj1,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{34:546,35:$Vj1},o($V11,[2,243]),{34:547,35:$Vj1},{34:548,35:$Vj1},o($V03,[2,247]),{36:[1,549],169:[1,550],170:453,171:$V12},{36:[1,551],169:[1,552],170:453,171:$V12},o($V11,[2,329]),{34:553,35:$Vj1},o($VH2,[2,332]),{34:554,35:$Vj1,93:[1,555]},o($V13,[2,237],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V13,[2,238]),o($V11,[2,150]),o($VI2,[2,153],{151:111,154:112,158:116,34:556,35:$Vj1,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V11,[2,249]),{34:557,35:$Vj1},{148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($Va1,[2,85]),o($Va1,[2,156]),{33:[1,558]},{35:$V42,37:348,38:$V2,39:$V3,115:559,116:346,118:$V52},o($Va1,[2,157]),{44:560,45:$V5,46:$V6},{6:$V23,35:$V33,109:[1,561]},o($VX2,$VC2,{37:348,116:564,38:$V2,39:$V3,118:$V52}),o($VB2,$VU1,{92:565,93:$VJ2}),{37:566,38:$V2,39:$V3},{37:567,38:$V2,39:$V3},{33:[2,172]},{6:$V43,35:$V53,109:[1,568]},o($VX2,$VC2,{37:355,123:571,38:$V2,39:$V3,118:$V72}),o($VB2,$VU1,{92:572,93:$VK2}),{37:573,38:$V2,39:$V3,118:[1,574]},{37:575,38:$V2,39:$V3},o($VL2,[2,176],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:576,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:577,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{36:[1,578]},o($Va1,[2,181]),{146:[1,579]},{69:[1,580],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{69:[1,581],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($Vb2,[2,205]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,131:373,136:210,137:582,138:206,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vx1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,93:$VA1,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,131:373,132:583,136:210,137:207,138:206,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Ve2,[2,226]),o($VP2,[2,233],{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,89:37,98:45,172:46,151:48,147:49,152:50,154:51,155:52,175:57,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,9:148,97:211,7:309,8:310,138:367,136:368,14:$V0,32:$Vk1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,70:$Vz1,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,90:$Vk,91:$Vl,93:$VA1,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vv,150:$Vw,153:$Vx,156:$Vy,167:$Vz,173:$VA,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),{6:$VN2,35:$VO2,36:[1,584]},o($Vb1,[2,130]),o($Vi2,[2,257],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{35:$V63,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,253]},o($Vi2,[2,260],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{35:$V73,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,255]},{35:$V83,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,276]},o($Vk2,[2,286]),{7:585,8:586,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:587,8:588,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:589,8:590,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:591,8:592,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:593,8:594,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:595,8:596,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:597,8:598,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:599,8:600,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vb2,[2,141]),{37:249,38:$V2,39:$V3,40:250,41:$VD1,42:246,43:$V4,44:88,45:$V5,46:$V6,62:601,63:242,64:243,66:244,67:251,68:$VJ1,70:$VK1,71:248,72:252,73:253,74:254,75:255,76:256,77:$VL1,107:$Vm,129:$Vq,130:$Vr,145:$Vu},o($V$2,$VI1,{44:88,62:241,63:242,64:243,66:244,42:246,71:248,37:249,40:250,67:251,72:252,73:253,74:254,75:255,76:256,108:602,38:$V2,39:$V3,41:$VD1,43:$V4,45:$V5,46:$V6,68:$VJ1,70:$VK1,77:$VL1,107:$Vm,129:$Vq,130:$Vr,145:$Vu}),o($Vo2,[2,144]),o($Vo2,[2,58],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:603,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vo2,[2,60],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:604,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($Vt2,[2,82]),{83:[1,605]},o($Vp2,[2,66]),o($Vi2,$V63,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vi2,$V73,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vi2,$V83,{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{7:606,8:607,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:608,8:609,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:610,8:611,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:612,8:613,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:614,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:615,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:616,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:617,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{6:$V93,35:$Va3,127:[1,618]},o([6,35,36,127],$VC2,{17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,13:23,15:25,16:26,60:29,53:30,74:31,100:32,51:33,76:34,75:35,89:37,98:45,172:46,151:48,147:49,152:50,154:51,155:52,175:57,96:61,73:62,42:63,48:65,37:78,67:79,158:85,44:88,9:148,97:211,7:309,8:310,136:621,14:$V0,32:$Vk1,38:$V2,39:$V3,43:$V4,45:$V5,46:$V6,49:$V7,50:$V8,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,68:$Vf,70:$Vz1,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,90:$Vk,91:$Vl,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,148:$Vv,150:$Vw,153:$Vx,156:$Vy,167:$Vz,173:$VA,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG}),o($VB2,$VU1,{92:622,93:$VZ2}),{83:[2,210],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{83:[2,212],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($V11,[2,55]),o($VY1,[2,91]),o($VJ,[2,93]),o($VW1,[2,102]),o($VB2,$VU1,{92:623,93:$VV1}),{34:539,35:$Vj1},o($V11,[2,372]),o($VQ2,[2,336]),o($V11,[2,244]),o($V03,[2,245]),o($V03,[2,246]),o($V11,[2,325]),{34:624,35:$Vj1},o($V11,[2,326]),{34:625,35:$Vj1},{36:[1,626]},o($VH2,[2,333],{6:[1,627]}),{7:628,8:629,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V11,[2,154]),o($VR2,[2,342]),{44:630,45:$V5,46:$V6},o($Vm2,$VU1,{92:631,93:$VJ2}),o($Va1,[2,158]),{33:[1,632]},{37:348,38:$V2,39:$V3,116:633,118:$V52},{35:$V42,37:348,38:$V2,39:$V3,115:634,116:346,118:$V52},o($Vo2,[2,163]),{6:$V23,35:$V33,36:[1,635]},o($Vo2,[2,168]),o($Vo2,[2,170]),o($Va1,[2,174],{33:[1,636]}),{37:355,38:$V2,39:$V3,118:$V72,123:637},{35:$V62,37:355,38:$V2,39:$V3,118:$V72,121:638,123:353},o($Vo2,[2,184]),{6:$V43,35:$V53,36:[1,639]},o($Vo2,[2,189]),o($Vo2,[2,190]),o($Vo2,[2,192]),o($VL2,[2,177],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{36:[1,640],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($Va1,[2,180]),o($Vb1,[2,251]),o($Vb1,[2,208]),o($Vb1,[2,209]),o($Ve2,[2,227]),o($VB2,$VU1,{92:371,133:641,93:$Vc2}),o($Ve2,[2,228]),{35:$Vb3,148:$VK,150:$VL,151:111,154:112,156:$VM,157:[1,642],158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,308],157:[1,643]},{35:$Vc3,148:$VK,149:[1,644],150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,312],149:[1,645]},{35:$Vd3,148:$VK,150:$VL,151:111,154:112,156:$VM,157:[1,646],158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,309],157:[1,647]},{35:$Ve3,148:$VK,149:[1,648],150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,313],149:[1,649]},{35:$Vf3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,310]},{35:$Vg3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,311]},{35:$Vh3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,323]},{35:$Vi3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,324]},o($Vo2,[2,145]),o($VB2,$VU1,{92:650,93:$Vn2}),{36:[1,651],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{36:[1,652],148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VG2,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},o($Vt2,[2,83]),o($Vj3,$Vb3,{151:111,154:112,158:116,157:[1,653],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{157:[1,654]},o($VY2,$Vc3,{151:111,154:112,158:116,149:[1,655],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{149:[1,656]},o($Vj3,$Vd3,{151:111,154:112,158:116,157:[1,657],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{157:[1,658]},o($VY2,$Ve3,{151:111,154:112,158:116,149:[1,659],178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{149:[1,660]},o($V22,$Vf3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vg3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vh3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vi3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($Vx2,[2,199]),{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,136:661,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:309,8:310,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,35:$Vy2,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,70:$Vz1,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,97:211,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,128:662,129:$Vq,130:$Vr,136:423,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V_2,[2,217]),{6:$V93,35:$Va3,36:[1,663]},{6:$VD2,35:$VE2,36:[1,664]},{36:[1,665]},{36:[1,666]},o($V11,[2,330]),o($VH2,[2,334]),o($V13,[2,239],{151:111,154:112,158:116,148:$VK,150:$VL,156:$VM,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V13,[2,240]),o($Va1,[2,160]),{6:$V23,35:$V33,109:[1,667]},{44:668,45:$V5,46:$V6},o($Vo2,[2,164]),o($VB2,$VU1,{92:669,93:$VJ2}),o($Vo2,[2,165]),{44:670,45:$V5,46:$V6},o($Vo2,[2,185]),o($VB2,$VU1,{92:671,93:$VK2}),o($Vo2,[2,186]),o($Va1,[2,178]),{6:$VN2,35:$VO2,36:[1,672]},{7:673,8:674,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:675,8:676,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:677,8:678,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:679,8:680,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:681,8:682,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:683,8:684,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:685,8:686,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:687,8:688,9:148,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,29:20,30:21,31:22,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vj,89:37,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$VA,175:57,176:$VB,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{6:$VV2,35:$VW2,36:[1,689]},o($Vo2,[2,59]),o($Vo2,[2,61]),{7:690,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:691,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:692,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:693,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:694,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:695,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:696,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},{7:697,9:154,13:23,14:$V0,15:25,16:26,17:8,18:9,19:10,20:11,21:12,22:13,23:14,24:15,25:16,26:17,27:18,28:19,32:$Vk1,37:78,38:$V2,39:$V3,42:63,43:$V4,44:88,45:$V5,46:$V6,48:65,49:$V7,50:$V8,51:33,53:30,54:$V9,55:$Va,56:$Vb,57:$Vc,58:$Vd,59:$Ve,60:29,67:79,68:$Vf,73:62,74:31,75:35,76:34,77:$Vg,84:$Vh,85:$Vl1,86:$Vm1,89:152,90:$Vk,91:$Vl,96:61,98:45,100:32,107:$Vm,110:$Vn,112:$Vo,120:$Vp,129:$Vq,130:$Vr,140:$Vs,144:$Vt,145:$Vu,147:49,148:$Vv,150:$Vw,151:48,152:50,153:$Vx,154:51,155:52,156:$Vy,158:85,167:$Vz,172:46,173:$Vn1,176:$Vo1,177:$VC,178:$VD,179:$VE,180:$VF,181:$VG},o($V_2,[2,218]),o($VB2,$VU1,{92:698,93:$VZ2}),o($V_2,[2,219]),o($VW1,[2,103]),o($V11,[2,327]),o($V11,[2,328]),{33:[1,699]},o($Va1,[2,159]),{6:$V23,35:$V33,36:[1,700]},o($Va1,[2,182]),{6:$V43,35:$V53,36:[1,701]},o($Ve2,[2,229]),{35:$Vk3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,314]},{35:$Vl3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,316]},{35:$Vm3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,318]},{35:$Vn3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,320]},{35:$Vo3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,315]},{35:$Vp3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,317]},{35:$Vq3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,319]},{35:$Vr3,148:$VK,150:$VL,151:111,154:112,156:$VM,158:116,174:$VN,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$},{35:[2,321]},o($Vo2,[2,146]),o($V22,$Vk3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vl3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vm3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vn3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vo3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vp3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vq3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),o($V22,$Vr3,{151:111,154:112,158:116,178:$VO,179:$VP,182:$VQ,183:$VR,184:$VS,185:$VT,186:$VU,187:$VV,188:$VW,189:$VX,190:$VY,191:$VZ,192:$V_,193:$V$}),{6:$V93,35:$Va3,36:[1,702]},{44:703,45:$V5,46:$V6},o($Vo2,[2,166]),o($Vo2,[2,187]),o($V_2,[2,220]),o($Va1,[2,161])],defaultActions:{236:[2,277],294:[2,140],473:[2,172],496:[2,253],499:[2,255],501:[2,276],594:[2,310],596:[2,311],598:[2,323],600:[2,324],674:[2,314],676:[2,316],678:[2,318],680:[2,320],682:[2,315],684:[2,317],686:[2,319],688:[2,321]},parseError:function parseError(str,hash){if(hash.recoverable){this.trace(str);}else{var error=new Error(str);error.hash=hash;throw error;}},parse:function parse(input){var self=this,stack=[0],tstack=[],vstack=[null],lstack=[],table=this.table,yytext='',yylineno=0,yyleng=0,recovering=0,TERROR=2,EOF=1;var args=lstack.slice.call(arguments,1);var lexer=Object.create(this.lexer);var sharedState={yy:{}};for(var k in this.yy){if(Object.prototype.hasOwnProperty.call(this.yy,k)){sharedState.yy[k]=this.yy[k];}}lexer.setInput(input,sharedState.yy);sharedState.yy.lexer=lexer;sharedState.yy.parser=this;if(typeof lexer.yylloc=='undefined'){lexer.yylloc={};}var yyloc=lexer.yylloc;lstack.push(yyloc);var ranges=lexer.options&&lexer.options.ranges;if(typeof sharedState.yy.parseError==='function'){this.parseError=sharedState.yy.parseError;}else{this.parseError=Object.getPrototypeOf(this).parseError;}function popStack(n){stack.length=stack.length-2*n;vstack.length=vstack.length-n;lstack.length=lstack.length-n;}_token_stack:var lex=function lex(){var token;token=lexer.lex()||EOF;if(typeof token!=='number'){token=self.symbols_[token]||token;}return token;};var symbol,preErrorSymbol,state,action,a,r,yyval={},p,len,newState,expected;while(true){state=stack[stack.length-1];if(this.defaultActions[state]){action=this.defaultActions[state];}else{if(symbol===null||typeof symbol=='undefined'){symbol=lex();}action=table[state]&&table[state][symbol];}if(typeof action==='undefined'||!action.length||!action[0]){var errStr='';expected=[];for(p in table[state]){if(this.terminals_[p]&&p>TERROR){expected.push('\''+this.terminals_[p]+'\'');}}if(lexer.showPosition){errStr='Parse error on line '+(yylineno+1)+':\n'+lexer.showPosition()+'\nExpecting '+expected.join(', ')+', got \''+(this.terminals_[symbol]||symbol)+'\'';}else{errStr='Parse error on line '+(yylineno+1)+': Unexpected '+(symbol==EOF?'end of input':'\''+(this.terminals_[symbol]||symbol)+'\'');}this.parseError(errStr,{text:lexer.match,token:this.terminals_[symbol]||symbol,line:lexer.yylineno,loc:yyloc,expected:expected});}if(action[0]instanceof Array&&action.length>1){throw new Error('Parse Error: multiple actions possible at state: '+state+', token: '+symbol);}switch(action[0]){case 1:stack.push(symbol);vstack.push(lexer.yytext);lstack.push(lexer.yylloc);stack.push(action[1]);symbol=null;if(!preErrorSymbol){yyleng=lexer.yyleng;yytext=lexer.yytext;yylineno=lexer.yylineno;yyloc=lexer.yylloc;if(recovering>0){recovering--;}}else{symbol=preErrorSymbol;preErrorSymbol=null;}break;case 2:len=this.productions_[action[1]][1];yyval.$=vstack[vstack.length-len];yyval._$={first_line:lstack[lstack.length-(len||1)].first_line,last_line:lstack[lstack.length-1].last_line,first_column:lstack[lstack.length-(len||1)].first_column,last_column:lstack[lstack.length-1].last_column};if(ranges){yyval._$.range=[lstack[lstack.length-(len||1)].range[0],lstack[lstack.length-1].range[1]];}r=this.performAction.apply(yyval,[yytext,yyleng,yylineno,sharedState.yy,action[1],vstack,lstack].concat(args));if(typeof r!=='undefined'){return r;}if(len){stack=stack.slice(0,-1*len*2);vstack=vstack.slice(0,-1*len);lstack=lstack.slice(0,-1*len);}stack.push(this.productions_[action[1]][0]);vstack.push(yyval.$);lstack.push(yyval._$);newState=table[stack[stack.length-2]][stack[stack.length-1]];stack.push(newState);break;case 3:return true;}}return true;}};function Parser(){this.yy={};}Parser.prototype=parser;parser.Parser=Parser;return new Parser();}();/*BT-
		if (typeof require !== 'undefined' && typeof exports !== 'undefined') {
		*/exports.parser=parser;exports.Parser=parser.Parser;exports.parse=function(){return parser.parse.apply(parser,arguments);};/*BT-
		exports.main = function() {};
		if (typeof module !== 'undefined' && require.main === module) {
			exports.main(process.argv.slice(1));
		}
		}
		*/return exports;};//#endregion
//#region URL: /scope
modules['/scope']=function(){var exports={};// The **Scope** class regulates lexical scoping within CoffeeScript. As you
// generate code, you create a tree of scopes in the same shape as the nested
// function bodies. Each scope knows about the variables declared within it,
// and has a reference to its parent enclosing scope. In this way, we know which
// variables are new and need to be declared with `var`, and which are shared
// with external scopes.
var Scope,indexOf=[].indexOf;exports.Scope=Scope=function(){// Initialize a scope with its parent, for lookups up the chain,
// as well as a reference to the **Block** node it belongs to, which is
// where it should declare its variables, a reference to the function that
// it belongs to, and a list of variables referenced in the source code
// and therefore should be avoided when generating variables. Also track comments
// that should be output as part of variable declarations.
function Scope(parent,expressions,method,referencedVars){_classCallCheck(this,Scope);var ref,ref1;this.parent=parent;this.expressions=expressions;this.method=method;this.referencedVars=referencedVars;this.variables=[{name:'arguments',type:'arguments'}];this.comments={};this.positions={};if(!this.parent){this.utilities={};}// The `@root` is the top-level **Scope** object for a given file.
this.root=(ref=(ref1=this.parent)!=null?ref1.root:void 0)!=null?ref:this;}// Adds a new variable or overrides an existing one.
_createClass(Scope,[{key:'add',value:function add(name,type,immediate){if(this.shared&&!immediate){return this.parent.add(name,type,immediate);}if(Object.prototype.hasOwnProperty.call(this.positions,name)){return this.variables[this.positions[name]].type=type;}else{return this.positions[name]=this.variables.push({name:name,type:type})-1;}}// When `super` is called, we need to find the name of the current method we're
// in, so that we know how to invoke the same method of the parent class. This
// can get complicated if super is being called from an inner function.
// `namedMethod` will walk up the scope tree until it either finds the first
// function object that has a name filled in, or bottoms out.
},{key:'namedMethod',value:function namedMethod(){var ref;if(((ref=this.method)!=null?ref.name:void 0)||!this.parent){return this.method;}return this.parent.namedMethod();}// Look up a variable name in lexical scope, and declare it if it does not
// already exist.
},{key:'find',value:function find(name){var type=arguments.length>1&&arguments[1]!==undefined?arguments[1]:'var';if(this.check(name)){return true;}this.add(name,type);return false;}// Reserve a variable name as originating from a function parameter for this
// scope. No `var` required for internal references.
},{key:'parameter',value:function parameter(name){if(this.shared&&this.parent.check(name,true)){return;}return this.add(name,'param');}// Just check to see if a variable has already been declared, without reserving,
// walks up to the root scope.
},{key:'check',value:function check(name){var ref;return!!(this.type(name)||((ref=this.parent)!=null?ref.check(name):void 0));}// Generate a temporary variable name at the given index.
},{key:'temporary',value:function temporary(name,index){var single=arguments.length>2&&arguments[2]!==undefined?arguments[2]:false;var diff,endCode,letter,newCode,num,startCode;if(single){startCode=name.charCodeAt(0);endCode='z'.charCodeAt(0);diff=endCode-startCode;newCode=startCode+index%(diff+1);letter=String.fromCharCode(newCode);num=Math.floor(index/(diff+1));return''+letter+(num||'');}else{return''+name+(index||'');}}// Gets the type of a variable.
},{key:'type',value:function type(name){var i,len,ref,v;ref=this.variables;for(i=0,len=ref.length;i<len;i++){v=ref[i];if(v.name===name){return v.type;}}return null;}// If we need to store an intermediate result, find an available name for a
// compiler-generated variable. `_var`, `_var2`, and so on...
},{key:'freeVariable',value:function freeVariable(name){var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var index,ref,temp;index=0;while(true){temp=this.temporary(name,index,options.single);if(!(this.check(temp)||indexOf.call(this.root.referencedVars,temp)>=0)){break;}index++;}if((ref=options.reserve)!=null?ref:true){this.add(temp,'var',true);}return temp;}// Ensure that an assignment is made at the top of this scope
// (or at the top-level scope, if requested).
},{key:'assign',value:function assign(name,value){this.add(name,{value:value,assigned:true},true);return this.hasAssignments=true;}// Does this scope have any declared variables?
},{key:'hasDeclarations',value:function hasDeclarations(){return!!this.declaredVariables().length;}// Return the list of variables first declared in this scope.
},{key:'declaredVariables',value:function declaredVariables(){var v;return function(){var i,len,ref,results;ref=this.variables;results=[];for(i=0,len=ref.length;i<len;i++){v=ref[i];if(v.type==='var'){results.push(v.name);}}return results;}.call(this).sort();}// Return the list of assignments that are supposed to be made at the top
// of this scope.
},{key:'assignedVariables',value:function assignedVariables(){var i,len,ref,results,v;ref=this.variables;results=[];for(i=0,len=ref.length;i<len;i++){v=ref[i];if(v.type.assigned){results.push(v.name+' = '+v.type.value);}}return results;}}]);return Scope;}();return exports;};//#endregion
//#region URL: /nodes
modules['/nodes']=function(){var exports={};// `nodes.coffee` contains all of the node classes for the syntax tree. Most
// nodes are created as the result of actions in the [grammar](grammar.html),
// but some are created by other nodes as a method of code generation. To convert
// the syntax tree into a string of JavaScript code, call `compile()` on the root.
var Access,Arr,Assign,AwaitReturn,Base,Block,BooleanLiteral,CSXTag,Call,Class,Code,CodeFragment,ComputedPropertyName,Elision,ExecutableClassBody,Existence,Expansion,ExportAllDeclaration,ExportDeclaration,ExportDefaultDeclaration,ExportNamedDeclaration,ExportSpecifier,ExportSpecifierList,Extends,For,FuncGlyph,HereComment,HoistTarget,IdentifierLiteral,If,ImportClause,ImportDeclaration,ImportDefaultSpecifier,ImportNamespaceSpecifier,ImportSpecifier,ImportSpecifierList,In,Index,InfinityLiteral,JS_FORBIDDEN,LEVEL_ACCESS,LEVEL_COND,LEVEL_LIST,LEVEL_OP,LEVEL_PAREN,LEVEL_TOP,LineComment,Literal,ModuleDeclaration,ModuleSpecifier,ModuleSpecifierList,NEGATE,NO,NaNLiteral,NullLiteral,NumberLiteral,Obj,Op,Param,Parens,PassthroughLiteral,PropertyName,Range,RegexLiteral,RegexWithInterpolations,Return,SIMPLENUM,Scope,Slice,Splat,StatementLiteral,StringLiteral,StringWithInterpolations,Super,SuperCall,Switch,TAB,THIS,TaggedTemplateCall,ThisLiteral,Throw,Try,UTILITIES,UndefinedLiteral,Value,While,YES,YieldReturn,addDataToNode,attachCommentsToNode,compact,del,ends,extend,flatten,fragmentsToText,hasLineComments,indentInitial,isLiteralArguments,isLiteralThis,isUnassignable,locationDataToString,merge,moveComments,multident,shouldCacheOrIsAssignable,some,starts,throwSyntaxError,_unfoldSoak,unshiftAfterComments,utility,indexOf=[].indexOf,splice=[].splice,slice1=[].slice;Error.stackTraceLimit=2e308;var _require4=require('/scope');Scope=_require4.Scope;// Import the helpers we plan to use.
var _require5=require('/lexer');isUnassignable=_require5.isUnassignable;JS_FORBIDDEN=_require5.JS_FORBIDDEN;// Functions required by parser.
var _require6=require('/helpers');compact=_require6.compact;flatten=_require6.flatten;extend=_require6.extend;merge=_require6.merge;del=_require6.del;starts=_require6.starts;ends=_require6.ends;some=_require6.some;addDataToNode=_require6.addDataToNode;attachCommentsToNode=_require6.attachCommentsToNode;locationDataToString=_require6.locationDataToString;throwSyntaxError=_require6.throwSyntaxError;exports.extend=extend;exports.addDataToNode=addDataToNode;// Constant functions for nodes that don’t need customization.
YES=function YES(){return true;};NO=function NO(){return false;};THIS=function THIS(){return this;};NEGATE=function NEGATE(){this.negated=!this.negated;return this;};//### CodeFragment
// The various nodes defined below all compile to a collection of **CodeFragment** objects.
// A CodeFragments is a block of generated code, and the location in the source file where the code
// came from. CodeFragments can be assembled together into working code just by catting together
// all the CodeFragments' `code` snippets, in order.
exports.CodeFragment=CodeFragment=function(){function CodeFragment(parent,code){_classCallCheck(this,CodeFragment);var ref1;this.code=''+code;this.type=(parent!=null?(ref1=parent.constructor)!=null?ref1.name:void 0:void 0)||'unknown';this.locationData=parent!=null?parent.locationData:void 0;this.comments=parent!=null?parent.comments:void 0;}_createClass(CodeFragment,[{key:'toString',value:function toString(){// This is only intended for debugging.
return''+this.code+(this.locationData?": "+locationDataToString(this.locationData):'');}}]);return CodeFragment;}();// Convert an array of CodeFragments into a string.
fragmentsToText=function fragmentsToText(fragments){var fragment;return function(){var j,len1,results;results=[];for(j=0,len1=fragments.length;j<len1;j++){fragment=fragments[j];results.push(fragment.code);}return results;}().join('');};//### Base
// The **Base** is the abstract base class for all nodes in the syntax tree.
// Each subclass implements the `compileNode` method, which performs the
// code generation for that node. To compile a node to JavaScript,
// call `compile` on it, which wraps `compileNode` in some generic extra smarts,
// to know when the generated code needs to be wrapped up in a closure.
// An options hash is passed and cloned throughout, containing information about
// the environment from higher in the tree (such as if a returned value is
// being requested by the surrounding function), information about the current
// scope, and indentation level.
exports.Base=Base=function(){var Base=function(){function Base(){_classCallCheck(this,Base);}_createClass(Base,[{key:'compile',value:function compile(o,lvl){return fragmentsToText(this.compileToFragments(o,lvl));}// Occasionally a node is compiled multiple times, for example to get the name
// of a variable to add to scope tracking. When we know that a “premature”
// compilation won’t result in comments being output, set those comments aside
// so that they’re preserved for a later `compile` call that will result in
// the comments being included in the output.
},{key:'compileWithoutComments',value:function compileWithoutComments(o,lvl){var method=arguments.length>2&&arguments[2]!==undefined?arguments[2]:'compile';var fragments,unwrapped;if(this.comments){this.ignoreTheseCommentsTemporarily=this.comments;delete this.comments;}unwrapped=this.unwrapAll();if(unwrapped.comments){unwrapped.ignoreTheseCommentsTemporarily=unwrapped.comments;delete unwrapped.comments;}fragments=this[method](o,lvl);if(this.ignoreTheseCommentsTemporarily){this.comments=this.ignoreTheseCommentsTemporarily;delete this.ignoreTheseCommentsTemporarily;}if(unwrapped.ignoreTheseCommentsTemporarily){unwrapped.comments=unwrapped.ignoreTheseCommentsTemporarily;delete unwrapped.ignoreTheseCommentsTemporarily;}return fragments;}},{key:'compileNodeWithoutComments',value:function compileNodeWithoutComments(o,lvl){return this.compileWithoutComments(o,lvl,'compileNode');}// Common logic for determining whether to wrap this node in a closure before
// compiling it, or to compile directly. We need to wrap if this node is a
// *statement*, and it's not a *pureStatement*, and we're not at
// the top level of a block (which would be unnecessary), and we haven't
// already been asked to return the result (because statements know how to
// return results).
},{key:'compileToFragments',value:function compileToFragments(o,lvl){var fragments,node;o=extend({},o);if(lvl){o.level=lvl;}node=this.unfoldSoak(o)||this;node.tab=o.indent;fragments=o.level===LEVEL_TOP||!node.isStatement(o)?node.compileNode(o):node.compileClosure(o);this.compileCommentFragments(o,node,fragments);return fragments;}},{key:'compileToFragmentsWithoutComments',value:function compileToFragmentsWithoutComments(o,lvl){return this.compileWithoutComments(o,lvl,'compileToFragments');}// Statements converted into expressions via closure-wrapping share a scope
// object with their parent closure, to preserve the expected lexical scope.
},{key:'compileClosure',value:function compileClosure(o){var args,argumentsNode,func,jumpNode,meth,parts,ref1,ref2;if(jumpNode=this.jumps()){jumpNode.error('cannot use a pure statement in an expression');}o.sharedScope=true;func=new Code([],Block.wrap([this]));args=[];if(this.contains(function(node){return node instanceof SuperCall;})){func.bound=true;}else if((argumentsNode=this.contains(isLiteralArguments))||this.contains(isLiteralThis)){args=[new ThisLiteral()];if(argumentsNode){meth='apply';args.push(new IdentifierLiteral('arguments'));}else{meth='call';}func=new Value(func,[new Access(new PropertyName(meth))]);}parts=new Call(func,args).compileNode(o);switch(false){case!(func.isGenerator||((ref1=func.base)!=null?ref1.isGenerator:void 0)):parts.unshift(this.makeCode("(yield* "));parts.push(this.makeCode(")"));break;case!(func.isAsync||((ref2=func.base)!=null?ref2.isAsync:void 0)):parts.unshift(this.makeCode("(await "));parts.push(this.makeCode(")"));}return parts;}},{key:'compileCommentFragments',value:function compileCommentFragments(o,node,fragments){var base1,base2,comment,commentFragment,j,len1,ref1,unshiftCommentFragment;if(!node.comments){return fragments;}// This is where comments, that are attached to nodes as a `comments`
// property, become `CodeFragment`s. “Inline block comments,” e.g.
// `/* */`-delimited comments that are interspersed within code on a line,
// are added to the current `fragments` stream. All other fragments are
// attached as properties to the nearest preceding or following fragment,
// to remain stowaways until they get properly output in `compileComments`
// later on.
unshiftCommentFragment=function unshiftCommentFragment(commentFragment){var precedingFragment;if(commentFragment.unshift){// Find the first non-comment fragment and insert `commentFragment`
// before it.
return unshiftAfterComments(fragments,commentFragment);}else{if(fragments.length!==0){precedingFragment=fragments[fragments.length-1];if(commentFragment.newLine&&precedingFragment.code!==''&&!/\n\s*$/.test(precedingFragment.code)){commentFragment.code='\n'+commentFragment.code;}}return fragments.push(commentFragment);}};ref1=node.comments;for(j=0,len1=ref1.length;j<len1;j++){comment=ref1[j];if(!(indexOf.call(this.compiledComments,comment)<0)){continue;}this.compiledComments.push(comment);// Don’t output this comment twice.
// For block/here comments, denoted by `###`, that are inline comments
// like `1 + ### comment ### 2`, create fragments and insert them into
// the fragments array.
// Otherwise attach comment fragments to their closest fragment for now,
// so they can be inserted into the output later after all the newlines
// have been added.
if(comment.here){// Block comment, delimited by `###`.
commentFragment=new HereComment(comment).compileNode(o);// Line comment, delimited by `#`.
}else{commentFragment=new LineComment(comment).compileNode(o);}if(commentFragment.isHereComment&&!commentFragment.newLine||node.includeCommentFragments()){// Inline block comments, like `1 + /* comment */ 2`, or a node whose
// `compileToFragments` method has logic for outputting comments.
unshiftCommentFragment(commentFragment);}else{if(fragments.length===0){fragments.push(this.makeCode(''));}if(commentFragment.unshift){if((base1=fragments[0]).precedingComments==null){base1.precedingComments=[];}fragments[0].precedingComments.push(commentFragment);}else{if((base2=fragments[fragments.length-1]).followingComments==null){base2.followingComments=[];}fragments[fragments.length-1].followingComments.push(commentFragment);}}}return fragments;}// If the code generation wishes to use the result of a complex expression
// in multiple places, ensure that the expression is only ever evaluated once,
// by assigning it to a temporary variable. Pass a level to precompile.
// If `level` is passed, then returns `[val, ref]`, where `val` is the compiled value, and `ref`
// is the compiled reference. If `level` is not passed, this returns `[val, ref]` where
// the two values are raw nodes which have not been compiled.
},{key:'cache',value:function cache(o,level,shouldCache){var complex,ref,sub;complex=shouldCache!=null?shouldCache(this):this.shouldCache();if(complex){ref=new IdentifierLiteral(o.scope.freeVariable('ref'));sub=new Assign(ref,this);if(level){return[sub.compileToFragments(o,level),[this.makeCode(ref.value)]];}else{return[sub,ref];}}else{ref=level?this.compileToFragments(o,level):this;return[ref,ref];}}// Occasionally it may be useful to make an expression behave as if it was 'hoisted', whereby the
// result of the expression is available before its location in the source, but the expression's
// variable scope corresponds the source position. This is used extensively to deal with executable
// class bodies in classes.
// Calling this method mutates the node, proxying the `compileNode` and `compileToFragments`
// methods to store their result for later replacing the `target` node, which is returned by the
// call.
},{key:'hoist',value:function hoist(){var compileNode,compileToFragments,target;this.hoisted=true;target=new HoistTarget(this);compileNode=this.compileNode;compileToFragments=this.compileToFragments;this.compileNode=function(o){return target.update(compileNode,o);};this.compileToFragments=function(o){return target.update(compileToFragments,o);};return target;}},{key:'cacheToCodeFragments',value:function cacheToCodeFragments(cacheValues){return[fragmentsToText(cacheValues[0]),fragmentsToText(cacheValues[1])];}// Construct a node that returns the current node's result.
// Note that this is overridden for smarter behavior for
// many statement nodes (e.g. If, For)...
},{key:'makeReturn',value:function makeReturn(res){var me;me=this.unwrapAll();if(res){return new Call(new Literal(res+'.push'),[me]);}else{return new Return(me);}}// Does this node, or any of its children, contain a node of a certain kind?
// Recursively traverses down the *children* nodes and returns the first one
// that verifies `pred`. Otherwise return undefined. `contains` does not cross
// scope boundaries.
},{key:'contains',value:function contains(pred){var node;node=void 0;this.traverseChildren(false,function(n){if(pred(n)){node=n;return false;}});return node;}// Pull out the last node of a node list.
},{key:'lastNode',value:function lastNode(list){if(list.length===0){return null;}else{return list[list.length-1];}}// `toString` representation of the node, for inspecting the parse tree.
// This is what `coffee --nodes` prints out.
},{key:'toString',value:function toString(){var idt=arguments.length>0&&arguments[0]!==undefined?arguments[0]:'';var name=arguments.length>1&&arguments[1]!==undefined?arguments[1]:this.constructor.name;var tree;tree='\n'+idt+name;if(this.soak){tree+='?';}this.eachChild(function(node){return tree+=node.toString(idt+TAB);});return tree;}// Passes each child to a function, breaking when the function returns `false`.
},{key:'eachChild',value:function eachChild(func){var attr,child,j,k,len1,len2,ref1,ref2;if(!this.children){return this;}ref1=this.children;for(j=0,len1=ref1.length;j<len1;j++){attr=ref1[j];if(this[attr]){ref2=flatten([this[attr]]);for(k=0,len2=ref2.length;k<len2;k++){child=ref2[k];if(func(child)===false){return this;}}}}return this;}},{key:'traverseChildren',value:function traverseChildren(crossScope,func){return this.eachChild(function(child){var recur;recur=func(child);if(recur!==false){return child.traverseChildren(crossScope,func);}});}// `replaceInContext` will traverse children looking for a node for which `match` returns
// true. Once found, the matching node will be replaced by the result of calling `replacement`.
},{key:'replaceInContext',value:function replaceInContext(match,replacement){var attr,child,children,i,j,k,len1,len2,ref1,ref2;if(!this.children){return false;}ref1=this.children;for(j=0,len1=ref1.length;j<len1;j++){attr=ref1[j];if(children=this[attr]){if(Array.isArray(children)){for(i=k=0,len2=children.length;k<len2;i=++k){child=children[i];if(match(child)){splice.apply(children,[i,i-i+1].concat(ref2=replacement(child,this))),ref2;return true;}else{if(child.replaceInContext(match,replacement)){return true;}}}}else if(match(children)){this[attr]=replacement(children,this);return true;}else{if(children.replaceInContext(match,replacement)){return true;}}}}}},{key:'invert',value:function invert(){return new Op('!',this);}},{key:'unwrapAll',value:function unwrapAll(){var node;node=this;while(node!==(node=node.unwrap())){continue;}return node;}// For this node and all descendents, set the location data to `locationData`
// if the location data is not already set.
},{key:'updateLocationDataIfMissing',value:function updateLocationDataIfMissing(locationData){if(this.locationData&&!this.forceUpdateLocation){return this;}delete this.forceUpdateLocation;this.locationData=locationData;return this.eachChild(function(child){return child.updateLocationDataIfMissing(locationData);});}// Throw a SyntaxError associated with this node’s location.
},{key:'error',value:function error(message){return throwSyntaxError(message,this.locationData);}},{key:'makeCode',value:function makeCode(code){return new CodeFragment(this,code);}},{key:'wrapInParentheses',value:function wrapInParentheses(fragments){return[this.makeCode('(')].concat(_toConsumableArray(fragments),[this.makeCode(')')]);}},{key:'wrapInBraces',value:function wrapInBraces(fragments){return[this.makeCode('{')].concat(_toConsumableArray(fragments),[this.makeCode('}')]);}// `fragmentsList` is an array of arrays of fragments. Each array in fragmentsList will be
// concatenated together, with `joinStr` added in between each, to produce a final flat array
// of fragments.
},{key:'joinFragmentArrays',value:function joinFragmentArrays(fragmentsList,joinStr){var answer,fragments,i,j,len1;answer=[];for(i=j=0,len1=fragmentsList.length;j<len1;i=++j){fragments=fragmentsList[i];if(i){answer.push(this.makeCode(joinStr));}answer=answer.concat(fragments);}return answer;}}]);return Base;}();;// Default implementations of the common node properties and methods. Nodes
// will override these with custom logic, if needed.
// `children` are the properties to recurse into when tree walking. The
// `children` list *is* the structure of the AST. The `parent` pointer, and
// the pointer to the `children` are how you can traverse the tree.
Base.prototype.children=[];// `isStatement` has to do with “everything is an expression”. A few things
// can’t be expressions, such as `break`. Things that `isStatement` returns
// `true` for are things that can’t be used as expressions. There are some
// error messages that come from `nodes.coffee` due to statements ending up
// in expression position.
Base.prototype.isStatement=NO;// Track comments that have been compiled into fragments, to avoid outputting
// them twice.
Base.prototype.compiledComments=[];// `includeCommentFragments` lets `compileCommentFragments` know whether this node
// has special awareness of how to handle comments within its output.
Base.prototype.includeCommentFragments=NO;// `jumps` tells you if an expression, or an internal part of an expression
// has a flow control construct (like `break`, or `continue`, or `return`,
// or `throw`) that jumps out of the normal flow of control and can’t be
// used as a value. This is important because things like this make no sense;
// we have to disallow them.
Base.prototype.jumps=NO;// If `node.shouldCache() is false`, it is safe to use `node` more than once.
// Otherwise you need to store the value of `node` in a variable and output
// that variable several times instead. Kind of like this: `5` need not be
// cached. `returnFive()`, however, could have side effects as a result of
// evaluating it more than once, and therefore we need to cache it. The
// parameter is named `shouldCache` rather than `mustCache` because there are
// also cases where we might not need to cache but where we want to, for
// example a long expression that may well be idempotent but we want to cache
// for brevity.
Base.prototype.shouldCache=YES;Base.prototype.isChainable=NO;Base.prototype.isAssignable=NO;Base.prototype.isNumber=NO;Base.prototype.unwrap=THIS;Base.prototype.unfoldSoak=NO;// Is this node used to assign a certain variable?
Base.prototype.assigns=NO;return Base;}.call(this);//### HoistTarget
// A **HoistTargetNode** represents the output location in the node tree for a hoisted node.
// See Base#hoist.
exports.HoistTarget=HoistTarget=function(_Base){_inherits(HoistTarget,_Base);_createClass(HoistTarget,null,[{key:'expand',// Expands hoisted fragments in the given array
value:function expand(fragments){var fragment,i,j,ref1;for(i=j=fragments.length-1;j>=0;i=j+=-1){fragment=fragments[i];if(fragment.fragments){splice.apply(fragments,[i,i-i+1].concat(ref1=this.expand(fragment.fragments))),ref1;}}return fragments;}}]);function HoistTarget(source1){_classCallCheck(this,HoistTarget);var _this7=_possibleConstructorReturn(this,(HoistTarget.__proto__||Object.getPrototypeOf(HoistTarget)).call(this));_this7.source=source1;// Holds presentational options to apply when the source node is compiled.
_this7.options={};// Placeholder fragments to be replaced by the source node’s compilation.
_this7.targetFragments={fragments:[]};return _this7;}_createClass(HoistTarget,[{key:'isStatement',value:function isStatement(o){return this.source.isStatement(o);}// Update the target fragments with the result of compiling the source.
// Calls the given compile function with the node and options (overriden with the target
// presentational options).
},{key:'update',value:function update(compile,o){return this.targetFragments.fragments=compile.call(this.source,merge(o,this.options));}// Copies the target indent and level, and returns the placeholder fragments
},{key:'compileToFragments',value:function compileToFragments(o,level){this.options.indent=o.indent;this.options.level=level!=null?level:o.level;return[this.targetFragments];}},{key:'compileNode',value:function compileNode(o){return this.compileToFragments(o);}},{key:'compileClosure',value:function compileClosure(o){return this.compileToFragments(o);}}]);return HoistTarget;}(Base);//### Block
// The block is the list of expressions that forms the body of an
// indented block of code -- the implementation of a function, a clause in an
// `if`, `switch`, or `try`, and so on...
exports.Block=Block=function(){var Block=function(_Base2){_inherits(Block,_Base2);function Block(nodes){_classCallCheck(this,Block);var _this8=_possibleConstructorReturn(this,(Block.__proto__||Object.getPrototypeOf(Block)).call(this));_this8.expressions=compact(flatten(nodes||[]));return _this8;}// Tack an expression on to the end of this expression list.
_createClass(Block,[{key:'push',value:function push(node){this.expressions.push(node);return this;}// Remove and return the last expression of this expression list.
},{key:'pop',value:function pop(){return this.expressions.pop();}// Add an expression at the beginning of this expression list.
},{key:'unshift',value:function unshift(node){this.expressions.unshift(node);return this;}// If this Block consists of just a single node, unwrap it by pulling
// it back out.
},{key:'unwrap',value:function unwrap(){if(this.expressions.length===1){return this.expressions[0];}else{return this;}}// Is this an empty block of code?
},{key:'isEmpty',value:function isEmpty(){return!this.expressions.length;}},{key:'isStatement',value:function isStatement(o){var exp,j,len1,ref1;ref1=this.expressions;for(j=0,len1=ref1.length;j<len1;j++){exp=ref1[j];if(exp.isStatement(o)){return true;}}return false;}},{key:'jumps',value:function jumps(o){var exp,j,jumpNode,len1,ref1;ref1=this.expressions;for(j=0,len1=ref1.length;j<len1;j++){exp=ref1[j];if(jumpNode=exp.jumps(o)){return jumpNode;}}}// A Block node does not return its entire body, rather it
// ensures that the final expression is returned.
},{key:'makeReturn',value:function makeReturn(res){var expr,len;len=this.expressions.length;while(len--){expr=this.expressions[len];this.expressions[len]=expr.makeReturn(res);if(expr instanceof Return&&!expr.expression){this.expressions.splice(len,1);}break;}return this;}// A **Block** is the only node that can serve as the root.
},{key:'compileToFragments',value:function compileToFragments(){var o=arguments.length>0&&arguments[0]!==undefined?arguments[0]:{};var level=arguments[1];if(o.scope){return _get(Block.prototype.__proto__||Object.getPrototypeOf(Block.prototype),'compileToFragments',this).call(this,o,level);}else{return this.compileRoot(o);}}// Compile all expressions within the **Block** body. If we need to return
// the result, and it’s an expression, simply return it. If it’s a statement,
// ask the statement to do so.
},{key:'compileNode',value:function compileNode(o){var answer,compiledNodes,fragments,index,j,lastFragment,len1,node,ref1,top;this.tab=o.indent;top=o.level===LEVEL_TOP;compiledNodes=[];ref1=this.expressions;for(index=j=0,len1=ref1.length;j<len1;index=++j){node=ref1[index];if(node.hoisted){// This is a hoisted expression.
// We want to compile this and ignore the result.
node.compileToFragments(o);continue;}node=node.unfoldSoak(o)||node;if(node instanceof Block){// This is a nested block. We don’t do anything special here like
// enclose it in a new scope; we just compile the statements in this
// block along with our own.
compiledNodes.push(node.compileNode(o));}else if(top){node.front=true;fragments=node.compileToFragments(o);if(!node.isStatement(o)){fragments=indentInitial(fragments,this);var _slice1$call=slice1.call(fragments,-1);var _slice1$call2=_slicedToArray(_slice1$call,1);lastFragment=_slice1$call2[0];if(!(lastFragment.code===''||lastFragment.isComment)){fragments.push(this.makeCode(';'));}}compiledNodes.push(fragments);}else{compiledNodes.push(node.compileToFragments(o,LEVEL_LIST));}}if(top){if(this.spaced){return[].concat(this.joinFragmentArrays(compiledNodes,'\n\n'),this.makeCode('\n'));}else{return this.joinFragmentArrays(compiledNodes,'\n');}}if(compiledNodes.length){answer=this.joinFragmentArrays(compiledNodes,', ');}else{answer=[this.makeCode('void 0')];}if(compiledNodes.length>1&&o.level>=LEVEL_LIST){return this.wrapInParentheses(answer);}else{return answer;}}// If we happen to be the top-level **Block**, wrap everything in a safety
// closure, unless requested not to. It would be better not to generate them
// in the first place, but for now, clean up obvious double-parentheses.
},{key:'compileRoot',value:function compileRoot(o){var fragments,j,len1,name,ref1,ref2;o.indent=o.bare?'':TAB;o.level=LEVEL_TOP;this.spaced=true;o.scope=new Scope(null,this,null,(ref1=o.referencedVars)!=null?ref1:[]);ref2=o.locals||[];for(j=0,len1=ref2.length;j<len1;j++){name=ref2[j];// Mark given local variables in the root scope as parameters so they don’t
// end up being declared on this block.
o.scope.parameter(name);}fragments=this.compileWithDeclarations(o);HoistTarget.expand(fragments);fragments=this.compileComments(fragments);if(o.bare){return fragments;}return[].concat(this.makeCode("(function() {\n"),fragments,this.makeCode("\n}).call(this);\n"));}// Compile the expressions body for the contents of a function, with
// declarations of all inner variables pushed up to the top.
},{key:'compileWithDeclarations',value:function compileWithDeclarations(o){var assigns,declaredVariable,declaredVariables,declaredVariablesIndex,declars,exp,fragments,i,j,k,len1,len2,post,ref1,rest,scope,spaced;fragments=[];post=[];ref1=this.expressions;for(i=j=0,len1=ref1.length;j<len1;i=++j){exp=ref1[i];exp=exp.unwrap();if(!(exp instanceof Literal)){break;}}o=merge(o,{level:LEVEL_TOP});if(i){rest=this.expressions.splice(i,9e9);var _ref8=[this.spaced,false];spaced=_ref8[0];this.spaced=_ref8[1];var _ref9=[this.compileNode(o),spaced];fragments=_ref9[0];this.spaced=_ref9[1];this.expressions=rest;}post=this.compileNode(o);var _o2=o;scope=_o2.scope;if(scope.expressions===this){declars=o.scope.hasDeclarations();assigns=scope.hasAssignments;if(declars||assigns){if(i){fragments.push(this.makeCode('\n'));}fragments.push(this.makeCode(this.tab+'var '));if(declars){declaredVariables=scope.declaredVariables();for(declaredVariablesIndex=k=0,len2=declaredVariables.length;k<len2;declaredVariablesIndex=++k){declaredVariable=declaredVariables[declaredVariablesIndex];fragments.push(this.makeCode(declaredVariable));if(Object.prototype.hasOwnProperty.call(o.scope.comments,declaredVariable)){var _fragments;(_fragments=fragments).push.apply(_fragments,_toConsumableArray(o.scope.comments[declaredVariable]));}if(declaredVariablesIndex!==declaredVariables.length-1){fragments.push(this.makeCode(', '));}}}if(assigns){if(declars){fragments.push(this.makeCode(',\n'+(this.tab+TAB)));}fragments.push(this.makeCode(scope.assignedVariables().join(',\n'+(this.tab+TAB))));}fragments.push(this.makeCode(';\n'+(this.spaced?'\n':'')));}else if(fragments.length&&post.length){fragments.push(this.makeCode("\n"));}}return fragments.concat(post);}},{key:'compileComments',value:function compileComments(fragments){var code,commentFragment,fragment,fragmentIndent,fragmentIndex,indent,j,k,l,len1,len2,len3,newLineIndex,onNextLine,p,pastFragment,pastFragmentIndex,q,ref1,ref2,ref3,ref4,trail,upcomingFragment,upcomingFragmentIndex;for(fragmentIndex=j=0,len1=fragments.length;j<len1;fragmentIndex=++j){fragment=fragments[fragmentIndex];// Insert comments into the output at the next or previous newline.
// If there are no newlines at which to place comments, create them.
if(fragment.precedingComments){// Determine the indentation level of the fragment that we are about
// to insert comments before, and use that indentation level for our
// inserted comments. At this point, the fragments’ `code` property
// is the generated output JavaScript, and CoffeeScript always
// generates output indented by two spaces; so all we need to do is
// search for a `code` property that begins with at least two spaces.
fragmentIndent='';ref1=fragments.slice(0,fragmentIndex+1);for(k=ref1.length-1;k>=0;k+=-1){pastFragment=ref1[k];indent=/^ {2,}/m.exec(pastFragment.code);if(indent){fragmentIndent=indent[0];break;}else if(indexOf.call(pastFragment.code,'\n')>=0){break;}}code='\n'+fragmentIndent+function(){var l,len2,ref2,results;ref2=fragment.precedingComments;results=[];for(l=0,len2=ref2.length;l<len2;l++){commentFragment=ref2[l];if(commentFragment.isHereComment&&commentFragment.multiline){results.push(multident(commentFragment.code,fragmentIndent,false));}else{results.push(commentFragment.code);}}return results;}().join('\n'+fragmentIndent).replace(/^(\s*)$/gm,'');ref2=fragments.slice(0,fragmentIndex+1);for(pastFragmentIndex=l=ref2.length-1;l>=0;pastFragmentIndex=l+=-1){pastFragment=ref2[pastFragmentIndex];newLineIndex=pastFragment.code.lastIndexOf('\n');if(newLineIndex===-1){// Keep searching previous fragments until we can’t go back any
// further, either because there are no fragments left or we’ve
// discovered that we’re in a code block that is interpolated
// inside a string.
if(pastFragmentIndex===0){pastFragment.code='\n'+pastFragment.code;newLineIndex=0;}else if(pastFragment.isStringWithInterpolations&&pastFragment.code==='{'){code=code.slice(1)+'\n';// Move newline to end.
newLineIndex=1;}else{continue;}}delete fragment.precedingComments;pastFragment.code=pastFragment.code.slice(0,newLineIndex)+code+pastFragment.code.slice(newLineIndex);break;}}// Yes, this is awfully similar to the previous `if` block, but if you
// look closely you’ll find lots of tiny differences that make this
// confusing if it were abstracted into a function that both blocks share.
if(fragment.followingComments){// Does the first trailing comment follow at the end of a line of code,
// like `; // Comment`, or does it start a new line after a line of code?
trail=fragment.followingComments[0].trail;fragmentIndent='';// Find the indent of the next line of code, if we have any non-trailing
// comments to output. We need to first find the next newline, as these
// comments will be output after that; and then the indent of the line
// that follows the next newline.
if(!(trail&&fragment.followingComments.length===1)){onNextLine=false;ref3=fragments.slice(fragmentIndex);for(p=0,len2=ref3.length;p<len2;p++){upcomingFragment=ref3[p];if(!onNextLine){if(indexOf.call(upcomingFragment.code,'\n')>=0){onNextLine=true;}else{continue;}}else{indent=/^ {2,}/m.exec(upcomingFragment.code);if(indent){fragmentIndent=indent[0];break;}else if(indexOf.call(upcomingFragment.code,'\n')>=0){break;}}}}// Is this comment following the indent inserted by bare mode?
// If so, there’s no need to indent this further.
code=fragmentIndex===1&&/^\s+$/.test(fragments[0].code)?'':trail?' ':'\n'+fragmentIndent;// Assemble properly indented comments.
code+=function(){var len3,q,ref4,results;ref4=fragment.followingComments;results=[];for(q=0,len3=ref4.length;q<len3;q++){commentFragment=ref4[q];if(commentFragment.isHereComment&&commentFragment.multiline){results.push(multident(commentFragment.code,fragmentIndent,false));}else{results.push(commentFragment.code);}}return results;}().join('\n'+fragmentIndent).replace(/^(\s*)$/gm,'');ref4=fragments.slice(fragmentIndex);for(upcomingFragmentIndex=q=0,len3=ref4.length;q<len3;upcomingFragmentIndex=++q){upcomingFragment=ref4[upcomingFragmentIndex];newLineIndex=upcomingFragment.code.indexOf('\n');if(newLineIndex===-1){// Keep searching upcoming fragments until we can’t go any
// further, either because there are no fragments left or we’ve
// discovered that we’re in a code block that is interpolated
// inside a string.
if(upcomingFragmentIndex===fragments.length-1){upcomingFragment.code=upcomingFragment.code+'\n';newLineIndex=upcomingFragment.code.length;}else if(upcomingFragment.isStringWithInterpolations&&upcomingFragment.code==='}'){code=code+'\n';newLineIndex=0;}else{continue;}}delete fragment.followingComments;if(upcomingFragment.code==='\n'){// Avoid inserting extra blank lines.
code=code.replace(/^\n/,'');}upcomingFragment.code=upcomingFragment.code.slice(0,newLineIndex)+code+upcomingFragment.code.slice(newLineIndex);break;}}}return fragments;}// Wrap up the given nodes as a **Block**, unless it already happens
// to be one.
}],[{key:'wrap',value:function wrap(nodes){if(nodes.length===1&&nodes[0]instanceof Block){return nodes[0];}return new Block(nodes);}}]);return Block;}(Base);;Block.prototype.children=['expressions'];return Block;}.call(this);//### Literal
// `Literal` is a base class for static values that can be passed through
// directly into JavaScript without translation, such as: strings, numbers,
// `true`, `false`, `null`...
exports.Literal=Literal=function(){var Literal=function(_Base3){_inherits(Literal,_Base3);function Literal(value1){_classCallCheck(this,Literal);var _this9=_possibleConstructorReturn(this,(Literal.__proto__||Object.getPrototypeOf(Literal)).call(this));_this9.value=value1;return _this9;}_createClass(Literal,[{key:'assigns',value:function assigns(name){return name===this.value;}},{key:'compileNode',value:function compileNode(o){return[this.makeCode(this.value)];}},{key:'toString',value:function toString(){// This is only intended for debugging.
return' '+(this.isStatement()?_get(Literal.prototype.__proto__||Object.getPrototypeOf(Literal.prototype),'toString',this).call(this):this.constructor.name)+': '+this.value;}}]);return Literal;}(Base);;Literal.prototype.shouldCache=NO;return Literal;}.call(this);exports.NumberLiteral=NumberLiteral=function(_Literal){_inherits(NumberLiteral,_Literal);function NumberLiteral(){_classCallCheck(this,NumberLiteral);return _possibleConstructorReturn(this,(NumberLiteral.__proto__||Object.getPrototypeOf(NumberLiteral)).apply(this,arguments));}return NumberLiteral;}(Literal);exports.InfinityLiteral=InfinityLiteral=function(_NumberLiteral){_inherits(InfinityLiteral,_NumberLiteral);function InfinityLiteral(){_classCallCheck(this,InfinityLiteral);return _possibleConstructorReturn(this,(InfinityLiteral.__proto__||Object.getPrototypeOf(InfinityLiteral)).apply(this,arguments));}_createClass(InfinityLiteral,[{key:'compileNode',value:function compileNode(){return[this.makeCode('2e308')];}}]);return InfinityLiteral;}(NumberLiteral);exports.NaNLiteral=NaNLiteral=function(_NumberLiteral2){_inherits(NaNLiteral,_NumberLiteral2);function NaNLiteral(){_classCallCheck(this,NaNLiteral);return _possibleConstructorReturn(this,(NaNLiteral.__proto__||Object.getPrototypeOf(NaNLiteral)).call(this,'NaN'));}_createClass(NaNLiteral,[{key:'compileNode',value:function compileNode(o){var code;code=[this.makeCode('0/0')];if(o.level>=LEVEL_OP){return this.wrapInParentheses(code);}else{return code;}}}]);return NaNLiteral;}(NumberLiteral);exports.StringLiteral=StringLiteral=function(_Literal2){_inherits(StringLiteral,_Literal2);function StringLiteral(){_classCallCheck(this,StringLiteral);return _possibleConstructorReturn(this,(StringLiteral.__proto__||Object.getPrototypeOf(StringLiteral)).apply(this,arguments));}_createClass(StringLiteral,[{key:'compileNode',value:function compileNode(o){var res;return res=this.csx?[this.makeCode(this.unquote(true,true))]:_get(StringLiteral.prototype.__proto__||Object.getPrototypeOf(StringLiteral.prototype),'compileNode',this).call(this);}},{key:'unquote',value:function unquote(){var doubleQuote=arguments.length>0&&arguments[0]!==undefined?arguments[0]:false;var newLine=arguments.length>1&&arguments[1]!==undefined?arguments[1]:false;var unquoted;unquoted=this.value.slice(1,-1);if(doubleQuote){unquoted=unquoted.replace(/\\"/g,'"');}if(newLine){unquoted=unquoted.replace(/\\n/g,'\n');}return unquoted;}}]);return StringLiteral;}(Literal);exports.RegexLiteral=RegexLiteral=function(_Literal3){_inherits(RegexLiteral,_Literal3);function RegexLiteral(){_classCallCheck(this,RegexLiteral);return _possibleConstructorReturn(this,(RegexLiteral.__proto__||Object.getPrototypeOf(RegexLiteral)).apply(this,arguments));}return RegexLiteral;}(Literal);exports.PassthroughLiteral=PassthroughLiteral=function(_Literal4){_inherits(PassthroughLiteral,_Literal4);function PassthroughLiteral(){_classCallCheck(this,PassthroughLiteral);return _possibleConstructorReturn(this,(PassthroughLiteral.__proto__||Object.getPrototypeOf(PassthroughLiteral)).apply(this,arguments));}return PassthroughLiteral;}(Literal);exports.IdentifierLiteral=IdentifierLiteral=function(){var IdentifierLiteral=function(_Literal5){_inherits(IdentifierLiteral,_Literal5);function IdentifierLiteral(){_classCallCheck(this,IdentifierLiteral);return _possibleConstructorReturn(this,(IdentifierLiteral.__proto__||Object.getPrototypeOf(IdentifierLiteral)).apply(this,arguments));}_createClass(IdentifierLiteral,[{key:'eachName',value:function eachName(iterator){return iterator(this);}}]);return IdentifierLiteral;}(Literal);;IdentifierLiteral.prototype.isAssignable=YES;return IdentifierLiteral;}.call(this);exports.CSXTag=CSXTag=function(_IdentifierLiteral){_inherits(CSXTag,_IdentifierLiteral);function CSXTag(){_classCallCheck(this,CSXTag);return _possibleConstructorReturn(this,(CSXTag.__proto__||Object.getPrototypeOf(CSXTag)).apply(this,arguments));}return CSXTag;}(IdentifierLiteral);exports.PropertyName=PropertyName=function(){var PropertyName=function(_Literal6){_inherits(PropertyName,_Literal6);function PropertyName(){_classCallCheck(this,PropertyName);return _possibleConstructorReturn(this,(PropertyName.__proto__||Object.getPrototypeOf(PropertyName)).apply(this,arguments));}return PropertyName;}(Literal);;PropertyName.prototype.isAssignable=YES;return PropertyName;}.call(this);exports.ComputedPropertyName=ComputedPropertyName=function(_PropertyName){_inherits(ComputedPropertyName,_PropertyName);function ComputedPropertyName(){_classCallCheck(this,ComputedPropertyName);return _possibleConstructorReturn(this,(ComputedPropertyName.__proto__||Object.getPrototypeOf(ComputedPropertyName)).apply(this,arguments));}_createClass(ComputedPropertyName,[{key:'compileNode',value:function compileNode(o){return[this.makeCode('[')].concat(_toConsumableArray(this.value.compileToFragments(o,LEVEL_LIST)),[this.makeCode(']')]);}}]);return ComputedPropertyName;}(PropertyName);exports.StatementLiteral=StatementLiteral=function(){var StatementLiteral=function(_Literal7){_inherits(StatementLiteral,_Literal7);function StatementLiteral(){_classCallCheck(this,StatementLiteral);return _possibleConstructorReturn(this,(StatementLiteral.__proto__||Object.getPrototypeOf(StatementLiteral)).apply(this,arguments));}_createClass(StatementLiteral,[{key:'jumps',value:function jumps(o){if(this.value==='break'&&!((o!=null?o.loop:void 0)||(o!=null?o.block:void 0))){return this;}if(this.value==='continue'&&!(o!=null?o.loop:void 0)){return this;}}},{key:'compileNode',value:function compileNode(o){return[this.makeCode(''+this.tab+this.value+';')];}}]);return StatementLiteral;}(Literal);;StatementLiteral.prototype.isStatement=YES;StatementLiteral.prototype.makeReturn=THIS;return StatementLiteral;}.call(this);exports.ThisLiteral=ThisLiteral=function(_Literal8){_inherits(ThisLiteral,_Literal8);function ThisLiteral(){_classCallCheck(this,ThisLiteral);return _possibleConstructorReturn(this,(ThisLiteral.__proto__||Object.getPrototypeOf(ThisLiteral)).call(this,'this'));}_createClass(ThisLiteral,[{key:'compileNode',value:function compileNode(o){var code,ref1;code=((ref1=o.scope.method)!=null?ref1.bound:void 0)?o.scope.method.context:this.value;return[this.makeCode(code)];}}]);return ThisLiteral;}(Literal);exports.UndefinedLiteral=UndefinedLiteral=function(_Literal9){_inherits(UndefinedLiteral,_Literal9);function UndefinedLiteral(){_classCallCheck(this,UndefinedLiteral);return _possibleConstructorReturn(this,(UndefinedLiteral.__proto__||Object.getPrototypeOf(UndefinedLiteral)).call(this,'undefined'));}_createClass(UndefinedLiteral,[{key:'compileNode',value:function compileNode(o){return[this.makeCode(o.level>=LEVEL_ACCESS?'(void 0)':'void 0')];}}]);return UndefinedLiteral;}(Literal);exports.NullLiteral=NullLiteral=function(_Literal10){_inherits(NullLiteral,_Literal10);function NullLiteral(){_classCallCheck(this,NullLiteral);return _possibleConstructorReturn(this,(NullLiteral.__proto__||Object.getPrototypeOf(NullLiteral)).call(this,'null'));}return NullLiteral;}(Literal);exports.BooleanLiteral=BooleanLiteral=function(_Literal11){_inherits(BooleanLiteral,_Literal11);function BooleanLiteral(){_classCallCheck(this,BooleanLiteral);return _possibleConstructorReturn(this,(BooleanLiteral.__proto__||Object.getPrototypeOf(BooleanLiteral)).apply(this,arguments));}return BooleanLiteral;}(Literal);//### Return
// A `return` is a *pureStatement*—wrapping it in a closure wouldn’t make sense.
exports.Return=Return=function(){var Return=function(_Base4){_inherits(Return,_Base4);function Return(expression1){_classCallCheck(this,Return);var _this25=_possibleConstructorReturn(this,(Return.__proto__||Object.getPrototypeOf(Return)).call(this));_this25.expression=expression1;return _this25;}_createClass(Return,[{key:'compileToFragments',value:function compileToFragments(o,level){var expr,ref1;expr=(ref1=this.expression)!=null?ref1.makeReturn():void 0;if(expr&&!(expr instanceof Return)){return expr.compileToFragments(o,level);}else{return _get(Return.prototype.__proto__||Object.getPrototypeOf(Return.prototype),'compileToFragments',this).call(this,o,level);}}},{key:'compileNode',value:function compileNode(o){var answer,fragment,j,len1;answer=[];// TODO: If we call `expression.compile()` here twice, we’ll sometimes
// get back different results!
if(this.expression){answer=this.expression.compileToFragments(o,LEVEL_PAREN);unshiftAfterComments(answer,this.makeCode(this.tab+'return '));// Since the `return` got indented by `@tab`, preceding comments that are
// multiline need to be indented.
for(j=0,len1=answer.length;j<len1;j++){fragment=answer[j];if(fragment.isHereComment&&indexOf.call(fragment.code,'\n')>=0){fragment.code=multident(fragment.code,this.tab);}else if(fragment.isLineComment){fragment.code=''+this.tab+fragment.code;}else{break;}}}else{answer.push(this.makeCode(this.tab+'return'));}answer.push(this.makeCode(';'));return answer;}}]);return Return;}(Base);;Return.prototype.children=['expression'];Return.prototype.isStatement=YES;Return.prototype.makeReturn=THIS;Return.prototype.jumps=THIS;return Return;}.call(this);// `yield return` works exactly like `return`, except that it turns the function
// into a generator.
exports.YieldReturn=YieldReturn=function(_Return){_inherits(YieldReturn,_Return);function YieldReturn(){_classCallCheck(this,YieldReturn);return _possibleConstructorReturn(this,(YieldReturn.__proto__||Object.getPrototypeOf(YieldReturn)).apply(this,arguments));}_createClass(YieldReturn,[{key:'compileNode',value:function compileNode(o){if(o.scope.parent==null){this.error('yield can only occur inside functions');}return _get(YieldReturn.prototype.__proto__||Object.getPrototypeOf(YieldReturn.prototype),'compileNode',this).call(this,o);}}]);return YieldReturn;}(Return);exports.AwaitReturn=AwaitReturn=function(_Return2){_inherits(AwaitReturn,_Return2);function AwaitReturn(){_classCallCheck(this,AwaitReturn);return _possibleConstructorReturn(this,(AwaitReturn.__proto__||Object.getPrototypeOf(AwaitReturn)).apply(this,arguments));}_createClass(AwaitReturn,[{key:'compileNode',value:function compileNode(o){if(o.scope.parent==null){this.error('await can only occur inside functions');}return _get(AwaitReturn.prototype.__proto__||Object.getPrototypeOf(AwaitReturn.prototype),'compileNode',this).call(this,o);}}]);return AwaitReturn;}(Return);//### Value
// A value, variable or literal or parenthesized, indexed or dotted into,
// or vanilla.
exports.Value=Value=function(){var Value=function(_Base5){_inherits(Value,_Base5);function Value(base,props,tag){var isDefaultValue=arguments.length>3&&arguments[3]!==undefined?arguments[3]:false;_classCallCheck(this,Value);var ref1,ref2;var _this28=_possibleConstructorReturn(this,(Value.__proto__||Object.getPrototypeOf(Value)).call(this));if(!props&&base instanceof Value){var _ret;return _ret=base,_possibleConstructorReturn(_this28,_ret);}_this28.base=base;_this28.properties=props||[];if(tag){_this28[tag]=true;}_this28.isDefaultValue=isDefaultValue;// If this is a `@foo =` assignment, if there are comments on `@` move them
// to be on `foo`.
if(((ref1=_this28.base)!=null?ref1.comments:void 0)&&_this28.base instanceof ThisLiteral&&((ref2=_this28.properties[0])!=null?ref2.name:void 0)!=null){moveComments(_this28.base,_this28.properties[0].name);}return _this28;}// Add a property (or *properties* ) `Access` to the list.
_createClass(Value,[{key:'add',value:function add(props){this.properties=this.properties.concat(props);this.forceUpdateLocation=true;return this;}},{key:'hasProperties',value:function hasProperties(){return this.properties.length!==0;}},{key:'bareLiteral',value:function bareLiteral(type){return!this.properties.length&&this.base instanceof type;}// Some boolean checks for the benefit of other nodes.
},{key:'isArray',value:function isArray(){return this.bareLiteral(Arr);}},{key:'isRange',value:function isRange(){return this.bareLiteral(Range);}},{key:'shouldCache',value:function shouldCache(){return this.hasProperties()||this.base.shouldCache();}},{key:'isAssignable',value:function isAssignable(){return this.hasProperties()||this.base.isAssignable();}},{key:'isNumber',value:function isNumber(){return this.bareLiteral(NumberLiteral);}},{key:'isString',value:function isString(){return this.bareLiteral(StringLiteral);}},{key:'isRegex',value:function isRegex(){return this.bareLiteral(RegexLiteral);}},{key:'isUndefined',value:function isUndefined(){return this.bareLiteral(UndefinedLiteral);}},{key:'isNull',value:function isNull(){return this.bareLiteral(NullLiteral);}},{key:'isBoolean',value:function isBoolean(){return this.bareLiteral(BooleanLiteral);}},{key:'isAtomic',value:function isAtomic(){var j,len1,node,ref1;ref1=this.properties.concat(this.base);for(j=0,len1=ref1.length;j<len1;j++){node=ref1[j];if(node.soak||node instanceof Call){return false;}}return true;}},{key:'isNotCallable',value:function isNotCallable(){return this.isNumber()||this.isString()||this.isRegex()||this.isArray()||this.isRange()||this.isSplice()||this.isObject()||this.isUndefined()||this.isNull()||this.isBoolean();}},{key:'isStatement',value:function isStatement(o){return!this.properties.length&&this.base.isStatement(o);}},{key:'assigns',value:function assigns(name){return!this.properties.length&&this.base.assigns(name);}},{key:'jumps',value:function jumps(o){return!this.properties.length&&this.base.jumps(o);}},{key:'isObject',value:function isObject(onlyGenerated){if(this.properties.length){return false;}return this.base instanceof Obj&&(!onlyGenerated||this.base.generated);}},{key:'isElision',value:function isElision(){if(!(this.base instanceof Arr)){return false;}return this.base.hasElision();}},{key:'isSplice',value:function isSplice(){var _slice1$call3,_slice1$call4;var lastProp,ref1;ref1=this.properties,(_slice1$call3=slice1.call(ref1,-1),_slice1$call4=_slicedToArray(_slice1$call3,1),lastProp=_slice1$call4[0],_slice1$call3);return lastProp instanceof Slice;}},{key:'looksStatic',value:function looksStatic(className){var ref1;return(this.this||this.base instanceof ThisLiteral||this.base.value===className)&&this.properties.length===1&&((ref1=this.properties[0].name)!=null?ref1.value:void 0)!=='prototype';}// The value can be unwrapped as its inner node, if there are no attached
// properties.
},{key:'unwrap',value:function unwrap(){if(this.properties.length){return this;}else{return this.base;}}// A reference has base part (`this` value) and name part.
// We cache them separately for compiling complex expressions.
// `a()[b()] ?= c` -> `(_base = a())[_name = b()] ? _base[_name] = c`
},{key:'cacheReference',value:function cacheReference(o){var _slice1$call5,_slice1$call6;var base,bref,name,nref,ref1;ref1=this.properties,(_slice1$call5=slice1.call(ref1,-1),_slice1$call6=_slicedToArray(_slice1$call5,1),name=_slice1$call6[0],_slice1$call5);if(this.properties.length<2&&!this.base.shouldCache()&&!(name!=null?name.shouldCache():void 0)){return[this,this];// `a` `a.b`
}base=new Value(this.base,this.properties.slice(0,-1));if(base.shouldCache()){// `a().b`
bref=new IdentifierLiteral(o.scope.freeVariable('base'));base=new Value(new Parens(new Assign(bref,base)));}if(!name){// `a()`
return[base,bref];}if(name.shouldCache()){// `a[b()]`
nref=new IdentifierLiteral(o.scope.freeVariable('name'));name=new Index(new Assign(nref,name.index));nref=new Index(nref);}return[base.add(name),new Value(bref||base.base,[nref||name])];}// We compile a value to JavaScript by compiling and joining each property.
// Things get much more interesting if the chain of properties has *soak*
// operators `?.` interspersed. Then we have to take care not to accidentally
// evaluate anything twice when building the soak chain.
},{key:'compileNode',value:function compileNode(o){var fragments,j,len1,prop,props;this.base.front=this.front;props=this.properties;if(props.length&&this.base.cached!=null){// Cached fragments enable correct order of the compilation,
// and reuse of variables in the scope.
// Example:
// `a(x = 5).b(-> x = 6)` should compile in the same order as
// `a(x = 5); b(-> x = 6)`
// (see issue #4437, https://github.com/jashkenas/coffeescript/issues/4437)
fragments=this.base.cached;}else{fragments=this.base.compileToFragments(o,props.length?LEVEL_ACCESS:null);}if(props.length&&SIMPLENUM.test(fragmentsToText(fragments))){fragments.push(this.makeCode('.'));}for(j=0,len1=props.length;j<len1;j++){var _fragments2;prop=props[j];(_fragments2=fragments).push.apply(_fragments2,_toConsumableArray(prop.compileToFragments(o)));}return fragments;}// Unfold a soak into an `If`: `a?.b` -> `a.b if a?`
},{key:'unfoldSoak',value:function unfoldSoak(o){var _this29=this;return this.unfoldedSoak!=null?this.unfoldedSoak:this.unfoldedSoak=function(){var fst,i,ifn,j,len1,prop,ref,ref1,snd;ifn=_this29.base.unfoldSoak(o);if(ifn){var _ifn$body$properties;(_ifn$body$properties=ifn.body.properties).push.apply(_ifn$body$properties,_toConsumableArray(_this29.properties));return ifn;}ref1=_this29.properties;for(i=j=0,len1=ref1.length;j<len1;i=++j){prop=ref1[i];if(!prop.soak){continue;}prop.soak=false;fst=new Value(_this29.base,_this29.properties.slice(0,i));snd=new Value(_this29.base,_this29.properties.slice(i));if(fst.shouldCache()){ref=new IdentifierLiteral(o.scope.freeVariable('ref'));fst=new Parens(new Assign(ref,fst));snd.base=ref;}return new If(new Existence(fst),snd,{soak:true});}return false;}();}},{key:'eachName',value:function eachName(iterator){if(this.hasProperties()){return iterator(this);}else if(this.base.isAssignable()){return this.base.eachName(iterator);}else{return this.error('tried to assign to unassignable value');}}}]);return Value;}(Base);;Value.prototype.children=['base','properties'];return Value;}.call(this);//### HereComment
// Comment delimited by `###` (becoming `/* */`).
exports.HereComment=HereComment=function(_Base6){_inherits(HereComment,_Base6);function HereComment(_ref10){var content1=_ref10.content,newLine1=_ref10.newLine,unshift=_ref10.unshift;_classCallCheck(this,HereComment);var _this30=_possibleConstructorReturn(this,(HereComment.__proto__||Object.getPrototypeOf(HereComment)).call(this));_this30.content=content1;_this30.newLine=newLine1;_this30.unshift=unshift;return _this30;}_createClass(HereComment,[{key:'compileNode',value:function compileNode(o){var fragment,hasLeadingMarks,j,largestIndent,leadingWhitespace,len1,line,multiline,ref1;multiline=indexOf.call(this.content,'\n')>=0;hasLeadingMarks=/\n\s*[#|\*]/.test(this.content);if(hasLeadingMarks){this.content=this.content.replace(/^([ \t]*)#(?=\s)/gm,' *');}// Unindent multiline comments. They will be reindented later.
if(multiline){largestIndent='';ref1=this.content.split('\n');for(j=0,len1=ref1.length;j<len1;j++){line=ref1[j];leadingWhitespace=/^\s*/.exec(line)[0];if(leadingWhitespace.length>largestIndent.length){largestIndent=leadingWhitespace;}}this.content=this.content.replace(RegExp('^('+leadingWhitespace+')',"gm"),'');}this.content='/*'+this.content+(hasLeadingMarks?' ':'')+'*/';fragment=this.makeCode(this.content);fragment.newLine=this.newLine;fragment.unshift=this.unshift;fragment.multiline=multiline;// Don’t rely on `fragment.type`, which can break when the compiler is minified.
fragment.isComment=fragment.isHereComment=true;return fragment;}}]);return HereComment;}(Base);//### LineComment
// Comment running from `#` to the end of a line (becoming `//`).
exports.LineComment=LineComment=function(_Base7){_inherits(LineComment,_Base7);function LineComment(_ref11){var content1=_ref11.content,newLine1=_ref11.newLine,unshift=_ref11.unshift;_classCallCheck(this,LineComment);var _this31=_possibleConstructorReturn(this,(LineComment.__proto__||Object.getPrototypeOf(LineComment)).call(this));_this31.content=content1;_this31.newLine=newLine1;_this31.unshift=unshift;return _this31;}_createClass(LineComment,[{key:'compileNode',value:function compileNode(o){var fragment;fragment=this.makeCode(/^\s*$/.test(this.content)?'':'//'+this.content);fragment.newLine=this.newLine;fragment.unshift=this.unshift;fragment.trail=!this.newLine&&!this.unshift;// Don’t rely on `fragment.type`, which can break when the compiler is minified.
fragment.isComment=fragment.isLineComment=true;return fragment;}}]);return LineComment;}(Base);//### Call
// Node for a function invocation.
exports.Call=Call=function(){var Call=function(_Base8){_inherits(Call,_Base8);function Call(variable1){var args1=arguments.length>1&&arguments[1]!==undefined?arguments[1]:[];var soak1=arguments[2];var token1=arguments[3];_classCallCheck(this,Call);var ref1;var _this32=_possibleConstructorReturn(this,(Call.__proto__||Object.getPrototypeOf(Call)).call(this));_this32.variable=variable1;_this32.args=args1;_this32.soak=soak1;_this32.token=token1;_this32.isNew=false;if(_this32.variable instanceof Value&&_this32.variable.isNotCallable()){_this32.variable.error("literal is not a function");}_this32.csx=_this32.variable.base instanceof CSXTag;// `@variable` never gets output as a result of this node getting created as
// part of `RegexWithInterpolations`, so for that case move any comments to
// the `args` property that gets passed into `RegexWithInterpolations` via
// the grammar.
if(((ref1=_this32.variable.base)!=null?ref1.value:void 0)==='RegExp'&&_this32.args.length!==0){moveComments(_this32.variable,_this32.args[0]);}return _this32;}// When setting the location, we sometimes need to update the start location to
// account for a newly-discovered `new` operator to the left of us. This
// expands the range on the left, but not the right.
_createClass(Call,[{key:'updateLocationDataIfMissing',value:function updateLocationDataIfMissing(locationData){var base,ref1;if(this.locationData&&this.needsUpdatedStartLocation){this.locationData.first_line=locationData.first_line;this.locationData.first_column=locationData.first_column;base=((ref1=this.variable)!=null?ref1.base:void 0)||this.variable;if(base.needsUpdatedStartLocation){this.variable.locationData.first_line=locationData.first_line;this.variable.locationData.first_column=locationData.first_column;base.updateLocationDataIfMissing(locationData);}delete this.needsUpdatedStartLocation;}return _get(Call.prototype.__proto__||Object.getPrototypeOf(Call.prototype),'updateLocationDataIfMissing',this).call(this,locationData);}// Tag this invocation as creating a new instance.
},{key:'newInstance',value:function newInstance(){var base,ref1;base=((ref1=this.variable)!=null?ref1.base:void 0)||this.variable;if(base instanceof Call&&!base.isNew){base.newInstance();}else{this.isNew=true;}this.needsUpdatedStartLocation=true;return this;}// Soaked chained invocations unfold into if/else ternary structures.
},{key:'unfoldSoak',value:function unfoldSoak(o){var call,ifn,j,left,len1,list,ref1,rite;if(this.soak){if(this.variable instanceof Super){left=new Literal(this.variable.compile(o));rite=new Value(left);if(this.variable.accessor==null){this.variable.error("Unsupported reference to 'super'");}}else{if(ifn=_unfoldSoak(o,this,'variable')){return ifn;}var _cacheReference=new Value(this.variable).cacheReference(o);var _cacheReference2=_slicedToArray(_cacheReference,2);left=_cacheReference2[0];rite=_cacheReference2[1];}rite=new Call(rite,this.args);rite.isNew=this.isNew;left=new Literal('typeof '+left.compile(o)+' === "function"');return new If(left,new Value(rite),{soak:true});}call=this;list=[];while(true){if(call.variable instanceof Call){list.push(call);call=call.variable;continue;}if(!(call.variable instanceof Value)){break;}list.push(call);if(!((call=call.variable.base)instanceof Call)){break;}}ref1=list.reverse();for(j=0,len1=ref1.length;j<len1;j++){call=ref1[j];if(ifn){if(call.variable instanceof Call){call.variable=ifn;}else{call.variable.base=ifn;}}ifn=_unfoldSoak(o,call,'variable');}return ifn;}// Compile a vanilla function call.
},{key:'compileNode',value:function compileNode(o){var _fragments3,_fragments4;var arg,argCode,argIndex,cache,compiledArgs,fragments,j,len1,ref1,ref2,ref3,ref4,varAccess;if(this.csx){return this.compileCSX(o);}if((ref1=this.variable)!=null){ref1.front=this.front;}compiledArgs=[];// If variable is `Accessor` fragments are cached and used later
// in `Value::compileNode` to ensure correct order of the compilation,
// and reuse of variables in the scope.
// Example:
// `a(x = 5).b(-> x = 6)` should compile in the same order as
// `a(x = 5); b(-> x = 6)`
// (see issue #4437, https://github.com/jashkenas/coffeescript/issues/4437)
varAccess=((ref2=this.variable)!=null?(ref3=ref2.properties)!=null?ref3[0]:void 0:void 0)instanceof Access;argCode=function(){var j,len1,ref4,results;ref4=this.args||[];results=[];for(j=0,len1=ref4.length;j<len1;j++){arg=ref4[j];if(arg instanceof Code){results.push(arg);}}return results;}.call(this);if(argCode.length>0&&varAccess&&!this.variable.base.cached){var _variable$base$cache=this.variable.base.cache(o,LEVEL_ACCESS,function(){return false;});var _variable$base$cache2=_slicedToArray(_variable$base$cache,1);cache=_variable$base$cache2[0];this.variable.base.cached=cache;}ref4=this.args;for(argIndex=j=0,len1=ref4.length;j<len1;argIndex=++j){var _compiledArgs;arg=ref4[argIndex];if(argIndex){compiledArgs.push(this.makeCode(", "));}(_compiledArgs=compiledArgs).push.apply(_compiledArgs,_toConsumableArray(arg.compileToFragments(o,LEVEL_LIST)));}fragments=[];if(this.isNew){if(this.variable instanceof Super){this.variable.error("Unsupported reference to 'super'");}fragments.push(this.makeCode('new '));}(_fragments3=fragments).push.apply(_fragments3,_toConsumableArray(this.variable.compileToFragments(o,LEVEL_ACCESS)));(_fragments4=fragments).push.apply(_fragments4,[this.makeCode('(')].concat(_toConsumableArray(compiledArgs),[this.makeCode(')')]));return fragments;}},{key:'compileCSX',value:function compileCSX(o){var _fragments5;var attr,attrProps,attributes,content,fragments,j,len1,obj,ref1,tag;var _args=_slicedToArray(this.args,2);attributes=_args[0];content=_args[1];attributes.base.csx=true;if(content!=null){content.base.csx=true;}fragments=[this.makeCode('<')];(_fragments5=fragments).push.apply(_fragments5,_toConsumableArray(tag=this.variable.compileToFragments(o,LEVEL_ACCESS)));if(attributes.base instanceof Arr){ref1=attributes.base.objects;for(j=0,len1=ref1.length;j<len1;j++){var _fragments6;obj=ref1[j];attr=obj.base;attrProps=(attr!=null?attr.properties:void 0)||[];// Catch invalid CSX attributes: <div {a:"b", props} {props} "value" />
if(!(attr instanceof Obj||attr instanceof IdentifierLiteral)||attr instanceof Obj&&!attr.generated&&(attrProps.length>1||!(attrProps[0]instanceof Splat))){obj.error("Unexpected token. Allowed CSX attributes are: id=\"val\", src={source}, {props...} or attribute.");}if(obj.base instanceof Obj){obj.base.csx=true;}fragments.push(this.makeCode(' '));(_fragments6=fragments).push.apply(_fragments6,_toConsumableArray(obj.compileToFragments(o,LEVEL_PAREN)));}}if(content){var _fragments7,_fragments8;fragments.push(this.makeCode('>'));(_fragments7=fragments).push.apply(_fragments7,_toConsumableArray(content.compileNode(o,LEVEL_LIST)));(_fragments8=fragments).push.apply(_fragments8,[this.makeCode('</')].concat(_toConsumableArray(tag),[this.makeCode('>')]));}else{fragments.push(this.makeCode(' />'));}return fragments;}}]);return Call;}(Base);;Call.prototype.children=['variable','args'];return Call;}.call(this);//### Super
// Takes care of converting `super()` calls into calls against the prototype's
// function of the same name.
// When `expressions` are set the call will be compiled in such a way that the
// expressions are evaluated without altering the return value of the `SuperCall`
// expression.
exports.SuperCall=SuperCall=function(){var SuperCall=function(_Call){_inherits(SuperCall,_Call);function SuperCall(){_classCallCheck(this,SuperCall);return _possibleConstructorReturn(this,(SuperCall.__proto__||Object.getPrototypeOf(SuperCall)).apply(this,arguments));}_createClass(SuperCall,[{key:'isStatement',value:function isStatement(o){var ref1;return((ref1=this.expressions)!=null?ref1.length:void 0)&&o.level===LEVEL_TOP;}},{key:'compileNode',value:function compileNode(o){var ref,ref1,replacement,superCall;if(!((ref1=this.expressions)!=null?ref1.length:void 0)){return _get(SuperCall.prototype.__proto__||Object.getPrototypeOf(SuperCall.prototype),'compileNode',this).call(this,o);}superCall=new Literal(fragmentsToText(_get(SuperCall.prototype.__proto__||Object.getPrototypeOf(SuperCall.prototype),'compileNode',this).call(this,o)));replacement=new Block(this.expressions.slice());if(o.level>LEVEL_TOP){var _superCall$cache=superCall.cache(o,null,YES);// If we might be in an expression we need to cache and return the result
var _superCall$cache2=_slicedToArray(_superCall$cache,2);superCall=_superCall$cache2[0];ref=_superCall$cache2[1];replacement.push(ref);}replacement.unshift(superCall);return replacement.compileToFragments(o,o.level===LEVEL_TOP?o.level:LEVEL_LIST);}}]);return SuperCall;}(Call);;SuperCall.prototype.children=Call.prototype.children.concat(['expressions']);return SuperCall;}.call(this);exports.Super=Super=function(){var Super=function(_Base9){_inherits(Super,_Base9);function Super(accessor){_classCallCheck(this,Super);var _this34=_possibleConstructorReturn(this,(Super.__proto__||Object.getPrototypeOf(Super)).call(this));_this34.accessor=accessor;return _this34;}_createClass(Super,[{key:'compileNode',value:function compileNode(o){var fragments,method,name,nref,ref1,ref2,salvagedComments,variable;method=o.scope.namedMethod();if(!(method!=null?method.isMethod:void 0)){this.error('cannot use super outside of an instance method');}if(!(method.ctor!=null||this.accessor!=null)){var _method=method;name=_method.name;variable=_method.variable;if(name.shouldCache()||name instanceof Index&&name.index.isAssignable()){nref=new IdentifierLiteral(o.scope.parent.freeVariable('name'));name.index=new Assign(nref,name.index);}this.accessor=nref!=null?new Index(nref):name;}if((ref1=this.accessor)!=null?(ref2=ref1.name)!=null?ref2.comments:void 0:void 0){// A `super()` call gets compiled to e.g. `super.method()`, which means
// the `method` property name gets compiled for the first time here, and
// again when the `method:` property of the class gets compiled. Since
// this compilation happens first, comments attached to `method:` would
// get incorrectly output near `super.method()`, when we want them to
// get output on the second pass when `method:` is output. So set them
// aside during this compilation pass, and put them back on the object so
// that they’re there for the later compilation.
salvagedComments=this.accessor.name.comments;delete this.accessor.name.comments;}fragments=new Value(new Literal('super'),this.accessor?[this.accessor]:[]).compileToFragments(o);if(salvagedComments){attachCommentsToNode(salvagedComments,this.accessor.name);}return fragments;}}]);return Super;}(Base);;Super.prototype.children=['accessor'];return Super;}.call(this);//### RegexWithInterpolations
// Regexes with interpolations are in fact just a variation of a `Call` (a
// `RegExp()` call to be precise) with a `StringWithInterpolations` inside.
exports.RegexWithInterpolations=RegexWithInterpolations=function(_Call2){_inherits(RegexWithInterpolations,_Call2);function RegexWithInterpolations(){var args=arguments.length>0&&arguments[0]!==undefined?arguments[0]:[];_classCallCheck(this,RegexWithInterpolations);return _possibleConstructorReturn(this,(RegexWithInterpolations.__proto__||Object.getPrototypeOf(RegexWithInterpolations)).call(this,new Value(new IdentifierLiteral('RegExp')),args,false));}return RegexWithInterpolations;}(Call);//### TaggedTemplateCall
exports.TaggedTemplateCall=TaggedTemplateCall=function(_Call3){_inherits(TaggedTemplateCall,_Call3);function TaggedTemplateCall(variable,arg,soak){_classCallCheck(this,TaggedTemplateCall);if(arg instanceof StringLiteral){arg=new StringWithInterpolations(Block.wrap([new Value(arg)]));}return _possibleConstructorReturn(this,(TaggedTemplateCall.__proto__||Object.getPrototypeOf(TaggedTemplateCall)).call(this,variable,[arg],soak));}_createClass(TaggedTemplateCall,[{key:'compileNode',value:function compileNode(o){return this.variable.compileToFragments(o,LEVEL_ACCESS).concat(this.args[0].compileToFragments(o,LEVEL_LIST));}}]);return TaggedTemplateCall;}(Call);//### Extends
// Node to extend an object's prototype with an ancestor object.
// After `goog.inherits` from the
// [Closure Library](https://github.com/google/closure-library/blob/master/closure/goog/base.js).
exports.Extends=Extends=function(){var Extends=function(_Base10){_inherits(Extends,_Base10);function Extends(child1,parent1){_classCallCheck(this,Extends);var _this37=_possibleConstructorReturn(this,(Extends.__proto__||Object.getPrototypeOf(Extends)).call(this));_this37.child=child1;_this37.parent=parent1;return _this37;}// Hooks one constructor into another's prototype chain.
_createClass(Extends,[{key:'compileToFragments',value:function compileToFragments(o){return new Call(new Value(new Literal(utility('extend',o))),[this.child,this.parent]).compileToFragments(o);}}]);return Extends;}(Base);;Extends.prototype.children=['child','parent'];return Extends;}.call(this);//### Access
// A `.` access into a property of a value, or the `::` shorthand for
// an access into the object's prototype.
exports.Access=Access=function(){var Access=function(_Base11){_inherits(Access,_Base11);function Access(name1,tag){_classCallCheck(this,Access);var _this38=_possibleConstructorReturn(this,(Access.__proto__||Object.getPrototypeOf(Access)).call(this));_this38.name=name1;_this38.soak=tag==='soak';return _this38;}_createClass(Access,[{key:'compileToFragments',value:function compileToFragments(o){var name,node;name=this.name.compileToFragments(o);node=this.name.unwrap();if(node instanceof PropertyName){return[this.makeCode('.')].concat(_toConsumableArray(name));}else{return[this.makeCode('[')].concat(_toConsumableArray(name),[this.makeCode(']')]);}}}]);return Access;}(Base);;Access.prototype.children=['name'];Access.prototype.shouldCache=NO;return Access;}.call(this);//### Index
// A `[ ... ]` indexed access into an array or object.
exports.Index=Index=function(){var Index=function(_Base12){_inherits(Index,_Base12);function Index(index1){_classCallCheck(this,Index);var _this39=_possibleConstructorReturn(this,(Index.__proto__||Object.getPrototypeOf(Index)).call(this));_this39.index=index1;return _this39;}_createClass(Index,[{key:'compileToFragments',value:function compileToFragments(o){return[].concat(this.makeCode("["),this.index.compileToFragments(o,LEVEL_PAREN),this.makeCode("]"));}},{key:'shouldCache',value:function shouldCache(){return this.index.shouldCache();}}]);return Index;}(Base);;Index.prototype.children=['index'];return Index;}.call(this);//### Range
// A range literal. Ranges can be used to extract portions (slices) of arrays,
// to specify a range for comprehensions, or as a value, to be expanded into the
// corresponding array of integers at runtime.
exports.Range=Range=function(){var Range=function(_Base13){_inherits(Range,_Base13);function Range(from1,to1,tag){_classCallCheck(this,Range);var _this40=_possibleConstructorReturn(this,(Range.__proto__||Object.getPrototypeOf(Range)).call(this));_this40.from=from1;_this40.to=to1;_this40.exclusive=tag==='exclusive';_this40.equals=_this40.exclusive?'':'=';return _this40;}// Compiles the range's source variables -- where it starts and where it ends.
// But only if they need to be cached to avoid double evaluation.
_createClass(Range,[{key:'compileVariables',value:function compileVariables(o){var shouldCache,step;o=merge(o,{top:true});shouldCache=del(o,'shouldCache');var _cacheToCodeFragments=this.cacheToCodeFragments(this.from.cache(o,LEVEL_LIST,shouldCache));var _cacheToCodeFragments2=_slicedToArray(_cacheToCodeFragments,2);this.fromC=_cacheToCodeFragments2[0];this.fromVar=_cacheToCodeFragments2[1];var _cacheToCodeFragments3=this.cacheToCodeFragments(this.to.cache(o,LEVEL_LIST,shouldCache));var _cacheToCodeFragments4=_slicedToArray(_cacheToCodeFragments3,2);this.toC=_cacheToCodeFragments4[0];this.toVar=_cacheToCodeFragments4[1];if(step=del(o,'step')){var _cacheToCodeFragments5=this.cacheToCodeFragments(step.cache(o,LEVEL_LIST,shouldCache));var _cacheToCodeFragments6=_slicedToArray(_cacheToCodeFragments5,2);this.step=_cacheToCodeFragments6[0];this.stepVar=_cacheToCodeFragments6[1];}this.fromNum=this.from.isNumber()?Number(this.fromVar):null;this.toNum=this.to.isNumber()?Number(this.toVar):null;return this.stepNum=(step!=null?step.isNumber():void 0)?Number(this.stepVar):null;}// When compiled normally, the range returns the contents of the *for loop*
// needed to iterate over the values in the range. Used by comprehensions.
},{key:'compileNode',value:function compileNode(o){var cond,condPart,from,gt,idx,idxName,known,lowerBound,lt,namedIndex,ref1,ref2,stepCond,stepNotZero,stepPart,to,upperBound,varPart;if(!this.fromVar){this.compileVariables(o);}if(!o.index){return this.compileArray(o);}// Set up endpoints.
known=this.fromNum!=null&&this.toNum!=null;idx=del(o,'index');idxName=del(o,'name');namedIndex=idxName&&idxName!==idx;varPart=known&&!namedIndex?'var '+idx+' = '+this.fromC:idx+' = '+this.fromC;if(this.toC!==this.toVar){varPart+=', '+this.toC;}if(this.step!==this.stepVar){varPart+=', '+this.step;}// Generate the condition.
lt=idx+' <'+this.equals;gt=idx+' >'+this.equals;// Always check if the `step` isn't zero to avoid the infinite loop.
var _ref12=[this.fromNum,this.toNum];from=_ref12[0];to=_ref12[1];stepNotZero=((ref1=this.stepNum)!=null?ref1:this.stepVar)+' !== 0';stepCond=((ref2=this.stepNum)!=null?ref2:this.stepVar)+' > 0';lowerBound=lt+' '+(known?to:this.toVar);upperBound=gt+' '+(known?to:this.toVar);condPart=this.step!=null?this.stepNum!=null&&this.stepNum!==0?this.stepNum>0?''+lowerBound:''+upperBound:stepNotZero+' && ('+stepCond+' ? '+lowerBound+' : '+upperBound+')':known?(from<=to?lt:gt)+' '+to:'('+this.fromVar+' <= '+this.toVar+' ? '+lowerBound+' : '+upperBound+')';cond=this.stepVar?this.stepVar+' > 0':this.fromVar+' <= '+this.toVar;// Generate the step.
stepPart=this.stepVar?idx+' += '+this.stepVar:known?namedIndex?from<=to?'++'+idx:'--'+idx:from<=to?idx+'++':idx+'--':namedIndex?cond+' ? ++'+idx+' : --'+idx:cond+' ? '+idx+'++ : '+idx+'--';if(namedIndex){varPart=idxName+' = '+varPart;}if(namedIndex){stepPart=idxName+' = '+stepPart;}// The final loop body.
return[this.makeCode(varPart+'; '+condPart+'; '+stepPart)];}// When used as a value, expand the range into the equivalent array.
},{key:'compileArray',value:function compileArray(o){var args,body,cond,hasArgs,i,idt,known,post,pre,range,ref1,ref2,result,vars;known=this.fromNum!=null&&this.toNum!=null;if(known&&Math.abs(this.fromNum-this.toNum)<=20){range=function(){var results=[];for(var j=ref1=this.fromNum,ref2=this.toNum;ref1<=ref2?j<=ref2:j>=ref2;ref1<=ref2?j++:j--){results.push(j);}return results;}.apply(this);if(this.exclusive){range.pop();}return[this.makeCode('['+range.join(', ')+']')];}idt=this.tab+TAB;i=o.scope.freeVariable('i',{single:true,reserve:false});result=o.scope.freeVariable('results',{reserve:false});pre='\n'+idt+'var '+result+' = [];';if(known){o.index=i;body=fragmentsToText(this.compileNode(o));}else{vars=i+' = '+this.fromC+(this.toC!==this.toVar?', '+this.toC:'');cond=this.fromVar+' <= '+this.toVar;body='var '+vars+'; '+cond+' ? '+i+' <'+this.equals+' '+this.toVar+' : '+i+' >'+this.equals+' '+this.toVar+'; '+cond+' ? '+i+'++ : '+i+'--';}post='{ '+result+'.push('+i+'); }\n'+idt+'return '+result+';\n'+o.indent;hasArgs=function hasArgs(node){return node!=null?node.contains(isLiteralArguments):void 0;};if(hasArgs(this.from)||hasArgs(this.to)){args=', arguments';}return[this.makeCode('(function() {'+pre+'\n'+idt+'for ('+body+')'+post+'}).apply(this'+(args!=null?args:'')+')')];}}]);return Range;}(Base);;Range.prototype.children=['from','to'];return Range;}.call(this);//### Slice
// An array slice literal. Unlike JavaScript's `Array#slice`, the second parameter
// specifies the index of the end of the slice, just as the first parameter
// is the index of the beginning.
exports.Slice=Slice=function(){var Slice=function(_Base14){_inherits(Slice,_Base14);function Slice(range1){_classCallCheck(this,Slice);var _this41=_possibleConstructorReturn(this,(Slice.__proto__||Object.getPrototypeOf(Slice)).call(this));_this41.range=range1;return _this41;}// We have to be careful when trying to slice through the end of the array,
// `9e9` is used because not all implementations respect `undefined` or `1/0`.
// `9e9` should be safe because `9e9` > `2**32`, the max array length.
_createClass(Slice,[{key:'compileNode',value:function compileNode(o){var compiled,compiledText,from,fromCompiled,to,toStr;// Handle an expression in the property access, e.g. `a[!b in c..]`.
var _range=this.range;to=_range.to;from=_range.from;if(from!=null?from.shouldCache():void 0){from=new Value(new Parens(from));}if(to!=null?to.shouldCache():void 0){to=new Value(new Parens(to));}fromCompiled=(from!=null?from.compileToFragments(o,LEVEL_PAREN):void 0)||[this.makeCode('0')];if(to){compiled=to.compileToFragments(o,LEVEL_PAREN);compiledText=fragmentsToText(compiled);if(!(!this.range.exclusive&&+compiledText===-1)){toStr=', '+(this.range.exclusive?compiledText:to.isNumber()?''+(+compiledText+1):(compiled=to.compileToFragments(o,LEVEL_ACCESS),'+'+fragmentsToText(compiled)+' + 1 || 9e9'));}}return[this.makeCode('.slice('+fragmentsToText(fromCompiled)+(toStr||'')+')')];}}]);return Slice;}(Base);;Slice.prototype.children=['range'];return Slice;}.call(this);//### Obj
// An object literal, nothing fancy.
exports.Obj=Obj=function(){var Obj=function(_Base15){_inherits(Obj,_Base15);function Obj(props){var generated=arguments.length>1&&arguments[1]!==undefined?arguments[1]:false;var lhs1=arguments.length>2&&arguments[2]!==undefined?arguments[2]:false;_classCallCheck(this,Obj);var _this42=_possibleConstructorReturn(this,(Obj.__proto__||Object.getPrototypeOf(Obj)).call(this));_this42.generated=generated;_this42.lhs=lhs1;_this42.objects=_this42.properties=props||[];return _this42;}_createClass(Obj,[{key:'isAssignable',value:function isAssignable(){var j,len1,message,prop,ref1,ref2;ref1=this.properties;for(j=0,len1=ref1.length;j<len1;j++){prop=ref1[j];// Check for reserved words.
message=isUnassignable(prop.unwrapAll().value);if(message){prop.error(message);}if(prop instanceof Assign&&prop.context==='object'&&!(((ref2=prop.value)!=null?ref2.base:void 0)instanceof Arr)){prop=prop.value;}if(!prop.isAssignable()){return false;}}return true;}},{key:'shouldCache',value:function shouldCache(){return!this.isAssignable();}// Check if object contains splat.
},{key:'hasSplat',value:function hasSplat(){var j,len1,prop,ref1;ref1=this.properties;for(j=0,len1=ref1.length;j<len1;j++){prop=ref1[j];if(prop instanceof Splat){return true;}}return false;}// Move rest property to the end of the list.
// `{a, rest..., b} = obj` -> `{a, b, rest...} = obj`
// `foo = ({a, rest..., b}) ->` -> `foo = {a, b, rest...}) ->`
},{key:'reorderProperties',value:function reorderProperties(){var i,prop,props,splatProp,splatProps;props=this.properties;splatProps=function(){var j,len1,results;results=[];for(i=j=0,len1=props.length;j<len1;i=++j){prop=props[i];if(prop instanceof Splat){results.push(i);}}return results;}();if((splatProps!=null?splatProps.length:void 0)>1){props[splatProps[1]].error("multiple spread elements are disallowed");}splatProp=props.splice(splatProps[0],1);return this.objects=this.properties=[].concat(props,splatProp);}},{key:'compileNode',value:function compileNode(o){var answer,i,idt,indent,isCompact,j,join,k,key,l,lastNode,len1,len2,len3,len4,node,p,prop,props,ref1,unwrappedVal,value;if(this.hasSplat()&&this.lhs){this.reorderProperties();}props=this.properties;if(this.generated){for(j=0,len1=props.length;j<len1;j++){node=props[j];if(node instanceof Value){node.error('cannot have an implicit value in an implicit object');}}}idt=o.indent+=TAB;lastNode=this.lastNode(this.properties);if(this.csx){// CSX attributes <div id="val" attr={aaa} {props...} />
return this.compileCSXAttributes(o);}// If this object is the left-hand side of an assignment, all its children
// are too.
if(this.lhs){for(k=0,len2=props.length;k<len2;k++){prop=props[k];if(!(prop instanceof Assign)){continue;}var _prop=prop;value=_prop.value;unwrappedVal=value.unwrapAll();if(unwrappedVal instanceof Arr||unwrappedVal instanceof Obj){unwrappedVal.lhs=true;}else if(unwrappedVal instanceof Assign){unwrappedVal.nestedLhs=true;}}}isCompact=true;ref1=this.properties;for(l=0,len3=ref1.length;l<len3;l++){prop=ref1[l];if(prop instanceof Assign&&prop.context==='object'){isCompact=false;}}answer=[];answer.push(this.makeCode(isCompact?'':'\n'));for(i=p=0,len4=props.length;p<len4;i=++p){var _answer;prop=props[i];join=i===props.length-1?'':isCompact?', ':prop===lastNode?'\n':',\n';indent=isCompact?'':idt;key=prop instanceof Assign&&prop.context==='object'?prop.variable:prop instanceof Assign?(!this.lhs?prop.operatorToken.error('unexpected '+prop.operatorToken.value):void 0,prop.variable):prop;if(key instanceof Value&&key.hasProperties()){if(prop.context==='object'||!key.this){key.error('invalid object key');}key=key.properties[0].name;prop=new Assign(key,prop,'object');}if(key===prop){if(prop.shouldCache()){var _prop$base$cache=prop.base.cache(o);var _prop$base$cache2=_slicedToArray(_prop$base$cache,2);key=_prop$base$cache2[0];value=_prop$base$cache2[1];if(key instanceof IdentifierLiteral){key=new PropertyName(key.value);}prop=new Assign(key,value,'object');}else if(key instanceof Value&&key.base instanceof ComputedPropertyName){// `{ [foo()] }` output as `{ [ref = foo()]: ref }`.
if(prop.base.value.shouldCache()){var _prop$base$value$cach=prop.base.value.cache(o);var _prop$base$value$cach2=_slicedToArray(_prop$base$value$cach,2);key=_prop$base$value$cach2[0];value=_prop$base$value$cach2[1];if(key instanceof IdentifierLiteral){key=new ComputedPropertyName(key.value);}prop=new Assign(key,value,'object');}else{// `{ [expression] }` output as `{ [expression]: expression }`.
prop=new Assign(key,prop.base.value,'object');}}else if(!(typeof prop.bareLiteral==="function"?prop.bareLiteral(IdentifierLiteral):void 0)&&!(prop instanceof Splat)){prop=new Assign(prop,prop,'object');}}if(indent){answer.push(this.makeCode(indent));}(_answer=answer).push.apply(_answer,_toConsumableArray(prop.compileToFragments(o,LEVEL_TOP)));if(join){answer.push(this.makeCode(join));}}answer.push(this.makeCode(isCompact?'':'\n'+this.tab));answer=this.wrapInBraces(answer);if(this.front){return this.wrapInParentheses(answer);}else{return answer;}}},{key:'assigns',value:function assigns(name){var j,len1,prop,ref1;ref1=this.properties;for(j=0,len1=ref1.length;j<len1;j++){prop=ref1[j];if(prop.assigns(name)){return true;}}return false;}},{key:'eachName',value:function eachName(iterator){var j,len1,prop,ref1,results;ref1=this.properties;results=[];for(j=0,len1=ref1.length;j<len1;j++){prop=ref1[j];if(prop instanceof Assign&&prop.context==='object'){prop=prop.value;}prop=prop.unwrapAll();if(prop.eachName!=null){results.push(prop.eachName(iterator));}else{results.push(void 0);}}return results;}},{key:'compileCSXAttributes',value:function compileCSXAttributes(o){var answer,i,j,join,len1,prop,props;props=this.properties;answer=[];for(i=j=0,len1=props.length;j<len1;i=++j){var _answer2;prop=props[i];prop.csx=true;join=i===props.length-1?'':' ';if(prop instanceof Splat){prop=new Literal('{'+prop.compile(o)+'}');}(_answer2=answer).push.apply(_answer2,_toConsumableArray(prop.compileToFragments(o,LEVEL_TOP)));answer.push(this.makeCode(join));}if(this.front){return this.wrapInParentheses(answer);}else{return answer;}}}]);return Obj;}(Base);;Obj.prototype.children=['properties'];return Obj;}.call(this);//### Arr
// An array literal.
exports.Arr=Arr=function(){var Arr=function(_Base16){_inherits(Arr,_Base16);function Arr(objs){var lhs1=arguments.length>1&&arguments[1]!==undefined?arguments[1]:false;_classCallCheck(this,Arr);var _this43=_possibleConstructorReturn(this,(Arr.__proto__||Object.getPrototypeOf(Arr)).call(this));_this43.lhs=lhs1;_this43.objects=objs||[];return _this43;}_createClass(Arr,[{key:'hasElision',value:function hasElision(){var j,len1,obj,ref1;ref1=this.objects;for(j=0,len1=ref1.length;j<len1;j++){obj=ref1[j];if(obj instanceof Elision){return true;}}return false;}},{key:'isAssignable',value:function isAssignable(){var i,j,len1,obj,ref1;if(!this.objects.length){return false;}ref1=this.objects;for(i=j=0,len1=ref1.length;j<len1;i=++j){obj=ref1[i];if(obj instanceof Splat&&i+1!==this.objects.length){return false;}if(!(obj.isAssignable()&&(!obj.isAtomic||obj.isAtomic()))){return false;}}return true;}},{key:'shouldCache',value:function shouldCache(){return!this.isAssignable();}},{key:'compileNode',value:function compileNode(o){var answer,compiledObjs,fragment,fragmentIndex,fragmentIsElision,fragments,includesLineCommentsOnNonFirstElement,index,j,k,l,len1,len2,len3,len4,len5,obj,objIndex,olen,p,passedElision,q,ref1,unwrappedObj;if(!this.objects.length){return[this.makeCode('[]')];}o.indent+=TAB;fragmentIsElision=function fragmentIsElision(fragment){return fragmentsToText(fragment).trim()===',';};// Detect if `Elisions` at the beginning of the array are processed (e.g. [, , , a]).
passedElision=false;answer=[];ref1=this.objects;for(objIndex=j=0,len1=ref1.length;j<len1;objIndex=++j){obj=ref1[objIndex];unwrappedObj=obj.unwrapAll();// Let `compileCommentFragments` know to intersperse block comments
// into the fragments created when compiling this array.
if(unwrappedObj.comments&&unwrappedObj.comments.filter(function(comment){return!comment.here;}).length===0){unwrappedObj.includeCommentFragments=YES;}// If this array is the left-hand side of an assignment, all its children
// are too.
if(this.lhs){if(unwrappedObj instanceof Arr||unwrappedObj instanceof Obj){unwrappedObj.lhs=true;}}}compiledObjs=function(){var k,len2,ref2,results;ref2=this.objects;results=[];for(k=0,len2=ref2.length;k<len2;k++){obj=ref2[k];results.push(obj.compileToFragments(o,LEVEL_LIST));}return results;}.call(this);olen=compiledObjs.length;// If `compiledObjs` includes newlines, we will output this as a multiline
// array (i.e. with a newline and indentation after the `[`). If an element
// contains line comments, that should also trigger multiline output since
// by definition line comments will introduce newlines into our output.
// The exception is if only the first element has line comments; in that
// case, output as the compact form if we otherwise would have, so that the
// first element’s line comments get output before or after the array.
includesLineCommentsOnNonFirstElement=false;for(index=k=0,len2=compiledObjs.length;k<len2;index=++k){var _answer3;fragments=compiledObjs[index];for(l=0,len3=fragments.length;l<len3;l++){fragment=fragments[l];if(fragment.isHereComment){fragment.code=fragment.code.trim();}else if(index!==0&&includesLineCommentsOnNonFirstElement===false&&hasLineComments(fragment)){includesLineCommentsOnNonFirstElement=true;}}// Add ', ' if all `Elisions` from the beginning of the array are processed (e.g. [, , , a]) and
// element isn't `Elision` or last element is `Elision` (e.g. [a,,b,,])
if(index!==0&&passedElision&&(!fragmentIsElision(fragments)||index===olen-1)){answer.push(this.makeCode(', '));}passedElision=passedElision||!fragmentIsElision(fragments);(_answer3=answer).push.apply(_answer3,_toConsumableArray(fragments));}if(includesLineCommentsOnNonFirstElement||indexOf.call(fragmentsToText(answer),'\n')>=0){for(fragmentIndex=p=0,len4=answer.length;p<len4;fragmentIndex=++p){fragment=answer[fragmentIndex];if(fragment.isHereComment){fragment.code=multident(fragment.code,o.indent,false)+'\n'+o.indent;}else if(fragment.code===', '&&!(fragment!=null?fragment.isElision:void 0)){fragment.code=',\n'+o.indent;}}answer.unshift(this.makeCode('[\n'+o.indent));answer.push(this.makeCode('\n'+this.tab+']'));}else{for(q=0,len5=answer.length;q<len5;q++){fragment=answer[q];if(fragment.isHereComment){fragment.code=fragment.code+' ';}}answer.unshift(this.makeCode('['));answer.push(this.makeCode(']'));}return answer;}},{key:'assigns',value:function assigns(name){var j,len1,obj,ref1;ref1=this.objects;for(j=0,len1=ref1.length;j<len1;j++){obj=ref1[j];if(obj.assigns(name)){return true;}}return false;}},{key:'eachName',value:function eachName(iterator){var j,len1,obj,ref1,results;ref1=this.objects;results=[];for(j=0,len1=ref1.length;j<len1;j++){obj=ref1[j];obj=obj.unwrapAll();results.push(obj.eachName(iterator));}return results;}}]);return Arr;}(Base);;Arr.prototype.children=['objects'];return Arr;}.call(this);//### Class
// The CoffeeScript class definition.
// Initialize a **Class** with its name, an optional superclass, and a body.
exports.Class=Class=function(){var Class=function(_Base17){_inherits(Class,_Base17);function Class(variable1,parent1){var body1=arguments.length>2&&arguments[2]!==undefined?arguments[2]:new Block();_classCallCheck(this,Class);var _this44=_possibleConstructorReturn(this,(Class.__proto__||Object.getPrototypeOf(Class)).call(this));_this44.variable=variable1;_this44.parent=parent1;_this44.body=body1;return _this44;}_createClass(Class,[{key:'compileNode',value:function compileNode(o){var executableBody,node,parentName;this.name=this.determineName();executableBody=this.walkBody();if(this.parent instanceof Value&&!this.parent.hasProperties()){// Special handling to allow `class expr.A extends A` declarations
parentName=this.parent.base.value;}this.hasNameClash=this.name!=null&&this.name===parentName;node=this;if(executableBody||this.hasNameClash){node=new ExecutableClassBody(node,executableBody);}else if(this.name==null&&o.level===LEVEL_TOP){// Anonymous classes are only valid in expressions
node=new Parens(node);}if(this.boundMethods.length&&this.parent){if(this.variable==null){this.variable=new IdentifierLiteral(o.scope.freeVariable('_class'));}if(this.variableRef==null){var _variable$cache=this.variable.cache(o);var _variable$cache2=_slicedToArray(_variable$cache,2);this.variable=_variable$cache2[0];this.variableRef=_variable$cache2[1];}}if(this.variable){node=new Assign(this.variable,node,null,{moduleDeclaration:this.moduleDeclaration});}this.compileNode=this.compileClassDeclaration;try{return node.compileToFragments(o);}finally{delete this.compileNode;}}},{key:'compileClassDeclaration',value:function compileClassDeclaration(o){var ref1,ref2,result;if(this.externalCtor||this.boundMethods.length){if(this.ctor==null){this.ctor=this.makeDefaultConstructor();}}if((ref1=this.ctor)!=null){ref1.noReturn=true;}if(this.boundMethods.length){this.proxyBoundMethods();}o.indent+=TAB;result=[];result.push(this.makeCode("class "));if(this.name){result.push(this.makeCode(this.name));}if(((ref2=this.variable)!=null?ref2.comments:void 0)!=null){this.compileCommentFragments(o,this.variable,result);}if(this.name){result.push(this.makeCode(' '));}if(this.parent){var _result;(_result=result).push.apply(_result,[this.makeCode('extends ')].concat(_toConsumableArray(this.parent.compileToFragments(o)),[this.makeCode(' ')]));}result.push(this.makeCode('{'));if(!this.body.isEmpty()){var _result2;this.body.spaced=true;result.push(this.makeCode('\n'));(_result2=result).push.apply(_result2,_toConsumableArray(this.body.compileToFragments(o,LEVEL_TOP)));result.push(this.makeCode('\n'+this.tab));}result.push(this.makeCode('}'));return result;}// Figure out the appropriate name for this class
},{key:'determineName',value:function determineName(){var _slice1$call7,_slice1$call8;var message,name,node,ref1,tail;if(!this.variable){return null;}ref1=this.variable.properties,(_slice1$call7=slice1.call(ref1,-1),_slice1$call8=_slicedToArray(_slice1$call7,1),tail=_slice1$call8[0],_slice1$call7);node=tail?tail instanceof Access&&tail.name:this.variable.base;if(!(node instanceof IdentifierLiteral||node instanceof PropertyName)){return null;}name=node.value;if(!tail){message=isUnassignable(name);if(message){this.variable.error(message);}}if(indexOf.call(JS_FORBIDDEN,name)>=0){return'_'+name;}else{return name;}}},{key:'walkBody',value:function walkBody(){var assign,end,executableBody,expression,expressions,exprs,i,initializer,initializerExpression,j,k,len1,len2,method,properties,pushSlice,ref1,start;this.ctor=null;this.boundMethods=[];executableBody=null;initializer=[];expressions=this.body.expressions;i=0;ref1=expressions.slice();for(j=0,len1=ref1.length;j<len1;j++){expression=ref1[j];if(expression instanceof Value&&expression.isObject(true)){properties=expression.base.properties;exprs=[];end=0;start=0;pushSlice=function pushSlice(){if(end>start){return exprs.push(new Value(new Obj(properties.slice(start,end),true)));}};while(assign=properties[end]){if(initializerExpression=this.addInitializerExpression(assign)){pushSlice();exprs.push(initializerExpression);initializer.push(initializerExpression);start=end+1;}end++;}pushSlice();splice.apply(expressions,[i,i-i+1].concat(exprs)),exprs;i+=exprs.length;}else{if(initializerExpression=this.addInitializerExpression(expression)){initializer.push(initializerExpression);expressions[i]=initializerExpression;}i+=1;}}for(k=0,len2=initializer.length;k<len2;k++){method=initializer[k];if(method instanceof Code){if(method.ctor){if(this.ctor){method.error('Cannot define more than one constructor in a class');}this.ctor=method;}else if(method.isStatic&&method.bound){method.context=this.name;}else if(method.bound){this.boundMethods.push(method);}}}if(initializer.length!==expressions.length){this.body.expressions=function(){var l,len3,results;results=[];for(l=0,len3=initializer.length;l<len3;l++){expression=initializer[l];results.push(expression.hoist());}return results;}();return new Block(expressions);}}// Add an expression to the class initializer
// This is the key method for determining whether an expression in a class
// body should appear in the initializer or the executable body. If the given
// `node` is valid in a class body the method will return a (new, modified,
// or identical) node for inclusion in the class initializer, otherwise
// nothing will be returned and the node will appear in the executable body.
// At time of writing, only methods (instance and static) are valid in ES
// class initializers. As new ES class features (such as class fields) reach
// Stage 4, this method will need to be updated to support them. We
// additionally allow `PassthroughLiteral`s (backticked expressions) in the
// initializer as an escape hatch for ES features that are not implemented
// (e.g. getters and setters defined via the `get` and `set` keywords as
// opposed to the `Object.defineProperty` method).
},{key:'addInitializerExpression',value:function addInitializerExpression(node){if(node.unwrapAll()instanceof PassthroughLiteral){return node;}else if(this.validInitializerMethod(node)){return this.addInitializerMethod(node);}else{return null;}}// Checks if the given node is a valid ES class initializer method.
},{key:'validInitializerMethod',value:function validInitializerMethod(node){if(!(node instanceof Assign&&node.value instanceof Code)){return false;}if(node.context==='object'&&!node.variable.hasProperties()){return true;}return node.variable.looksStatic(this.name)&&(this.name||!node.value.bound);}// Returns a configured class initializer method
},{key:'addInitializerMethod',value:function addInitializerMethod(assign){var method,methodName,variable;variable=assign.variable;method=assign.value;method.isMethod=true;method.isStatic=variable.looksStatic(this.name);if(method.isStatic){method.name=variable.properties[0];}else{methodName=variable.base;method.name=new(methodName.shouldCache()?Index:Access)(methodName);method.name.updateLocationDataIfMissing(methodName.locationData);if(methodName.value==='constructor'){method.ctor=this.parent?'derived':'base';}if(method.bound&&method.ctor){method.error('Cannot define a constructor as a bound (fat arrow) function');}}return method;}},{key:'makeDefaultConstructor',value:function makeDefaultConstructor(){var applyArgs,applyCtor,ctor;ctor=this.addInitializerMethod(new Assign(new Value(new PropertyName('constructor')),new Code()));this.body.unshift(ctor);if(this.parent){ctor.body.push(new SuperCall(new Super(),[new Splat(new IdentifierLiteral('arguments'))]));}if(this.externalCtor){applyCtor=new Value(this.externalCtor,[new Access(new PropertyName('apply'))]);applyArgs=[new ThisLiteral(),new IdentifierLiteral('arguments')];ctor.body.push(new Call(applyCtor,applyArgs));ctor.body.makeReturn();}return ctor;}},{key:'proxyBoundMethods',value:function proxyBoundMethods(){var method,name;this.ctor.thisAssignments=function(){var j,len1,ref1,results;ref1=this.boundMethods;results=[];for(j=0,len1=ref1.length;j<len1;j++){method=ref1[j];if(this.parent){method.classVariable=this.variableRef;}name=new Value(new ThisLiteral(),[method.name]);results.push(new Assign(name,new Call(new Value(name,[new Access(new PropertyName('bind'))]),[new ThisLiteral()])));}return results;}.call(this);return null;}}]);return Class;}(Base);;Class.prototype.children=['variable','parent','body'];return Class;}.call(this);exports.ExecutableClassBody=ExecutableClassBody=function(){var ExecutableClassBody=function(_Base18){_inherits(ExecutableClassBody,_Base18);function ExecutableClassBody(_class){var body1=arguments.length>1&&arguments[1]!==undefined?arguments[1]:new Block();_classCallCheck(this,ExecutableClassBody);var _this45=_possibleConstructorReturn(this,(ExecutableClassBody.__proto__||Object.getPrototypeOf(ExecutableClassBody)).call(this));_this45.class=_class;_this45.body=body1;return _this45;}_createClass(ExecutableClassBody,[{key:'compileNode',value:function compileNode(o){var _body$expressions;var args,argumentsNode,directives,externalCtor,ident,jumpNode,klass,params,parent,ref1,wrapper;if(jumpNode=this.body.jumps()){jumpNode.error('Class bodies cannot contain pure statements');}if(argumentsNode=this.body.contains(isLiteralArguments)){argumentsNode.error("Class bodies shouldn't reference arguments");}params=[];args=[new ThisLiteral()];wrapper=new Code(params,this.body);klass=new Parens(new Call(new Value(wrapper,[new Access(new PropertyName('call'))]),args));this.body.spaced=true;o.classScope=wrapper.makeScope(o.scope);this.name=(ref1=this.class.name)!=null?ref1:o.classScope.freeVariable(this.defaultClassVariableName);ident=new IdentifierLiteral(this.name);directives=this.walkBody();this.setContext();if(this.class.hasNameClash){parent=new IdentifierLiteral(o.classScope.freeVariable('superClass'));wrapper.params.push(new Param(parent));args.push(this.class.parent);this.class.parent=parent;}if(this.externalCtor){externalCtor=new IdentifierLiteral(o.classScope.freeVariable('ctor',{reserve:false}));this.class.externalCtor=externalCtor;this.externalCtor.variable.base=externalCtor;}if(this.name!==this.class.name){this.body.expressions.unshift(new Assign(new IdentifierLiteral(this.name),this.class));}else{this.body.expressions.unshift(this.class);}(_body$expressions=this.body.expressions).unshift.apply(_body$expressions,_toConsumableArray(directives));this.body.push(ident);return klass.compileToFragments(o);}// Traverse the class's children and:
// - Hoist valid ES properties into `@properties`
// - Hoist static assignments into `@properties`
// - Convert invalid ES properties into class or prototype assignments
},{key:'walkBody',value:function walkBody(){var _this46=this;var directives,expr,index;directives=[];index=0;while(expr=this.body.expressions[index]){if(!(expr instanceof Value&&expr.isString())){break;}if(expr.hoisted){index++;}else{var _directives;(_directives=directives).push.apply(_directives,_toConsumableArray(this.body.expressions.splice(index,1)));}}this.traverseChildren(false,function(child){var cont,i,j,len1,node,ref1;if(child instanceof Class||child instanceof HoistTarget){return false;}cont=true;if(child instanceof Block){ref1=child.expressions;for(i=j=0,len1=ref1.length;j<len1;i=++j){node=ref1[i];if(node instanceof Value&&node.isObject(true)){cont=false;child.expressions[i]=_this46.addProperties(node.base.properties);}else if(node instanceof Assign&&node.variable.looksStatic(_this46.name)){node.value.isStatic=true;}}child.expressions=flatten(child.expressions);}return cont;});return directives;}},{key:'setContext',value:function setContext(){var _this47=this;return this.body.traverseChildren(false,function(node){if(node instanceof ThisLiteral){return node.value=_this47.name;}else if(node instanceof Code&&node.bound&&node.isStatic){return node.context=_this47.name;}});}// Make class/prototype assignments for invalid ES properties
},{key:'addProperties',value:function addProperties(assigns){var assign,base,name,prototype,result,value,variable;result=function(){var j,len1,results;results=[];for(j=0,len1=assigns.length;j<len1;j++){assign=assigns[j];variable=assign.variable;base=variable!=null?variable.base:void 0;value=assign.value;delete assign.context;if(base.value==='constructor'){if(value instanceof Code){base.error('constructors must be defined at the top level of a class body');}// The class scope is not available yet, so return the assignment to update later
assign=this.externalCtor=new Assign(new Value(),value);}else if(!assign.variable.this){name=new(base.shouldCache()?Index:Access)(base);prototype=new Access(new PropertyName('prototype'));variable=new Value(new ThisLiteral(),[prototype,name]);assign.variable=variable;}else if(assign.value instanceof Code){assign.value.isStatic=true;}results.push(assign);}return results;}.call(this);return compact(result);}}]);return ExecutableClassBody;}(Base);;ExecutableClassBody.prototype.children=['class','body'];ExecutableClassBody.prototype.defaultClassVariableName='_Class';return ExecutableClassBody;}.call(this);//### Import and Export
exports.ModuleDeclaration=ModuleDeclaration=function(){var ModuleDeclaration=function(_Base19){_inherits(ModuleDeclaration,_Base19);function ModuleDeclaration(clause,source1){_classCallCheck(this,ModuleDeclaration);var _this48=_possibleConstructorReturn(this,(ModuleDeclaration.__proto__||Object.getPrototypeOf(ModuleDeclaration)).call(this));_this48.clause=clause;_this48.source=source1;_this48.checkSource();return _this48;}_createClass(ModuleDeclaration,[{key:'checkSource',value:function checkSource(){if(this.source!=null&&this.source instanceof StringWithInterpolations){return this.source.error('the name of the module to be imported from must be an uninterpolated string');}}},{key:'checkScope',value:function checkScope(o,moduleDeclarationType){if(o.indent.length!==0){return this.error(moduleDeclarationType+' statements must be at top-level scope');}}}]);return ModuleDeclaration;}(Base);;ModuleDeclaration.prototype.children=['clause','source'];ModuleDeclaration.prototype.isStatement=YES;ModuleDeclaration.prototype.jumps=THIS;ModuleDeclaration.prototype.makeReturn=THIS;return ModuleDeclaration;}.call(this);exports.ImportDeclaration=ImportDeclaration=function(_ModuleDeclaration){_inherits(ImportDeclaration,_ModuleDeclaration);function ImportDeclaration(){_classCallCheck(this,ImportDeclaration);return _possibleConstructorReturn(this,(ImportDeclaration.__proto__||Object.getPrototypeOf(ImportDeclaration)).apply(this,arguments));}_createClass(ImportDeclaration,[{key:'compileNode',value:function compileNode(o){var code,ref1;this.checkScope(o,'import');o.importedSymbols=[];code=[];code.push(this.makeCode(this.tab+'import '));if(this.clause!=null){var _code;(_code=code).push.apply(_code,_toConsumableArray(this.clause.compileNode(o)));}if(((ref1=this.source)!=null?ref1.value:void 0)!=null){if(this.clause!==null){code.push(this.makeCode(' from '));}code.push(this.makeCode(this.source.value));}code.push(this.makeCode(';'));return code;}}]);return ImportDeclaration;}(ModuleDeclaration);exports.ImportClause=ImportClause=function(){var ImportClause=function(_Base20){_inherits(ImportClause,_Base20);function ImportClause(defaultBinding,namedImports){_classCallCheck(this,ImportClause);var _this50=_possibleConstructorReturn(this,(ImportClause.__proto__||Object.getPrototypeOf(ImportClause)).call(this));_this50.defaultBinding=defaultBinding;_this50.namedImports=namedImports;return _this50;}_createClass(ImportClause,[{key:'compileNode',value:function compileNode(o){var code;code=[];if(this.defaultBinding!=null){var _code2;(_code2=code).push.apply(_code2,_toConsumableArray(this.defaultBinding.compileNode(o)));if(this.namedImports!=null){code.push(this.makeCode(', '));}}if(this.namedImports!=null){var _code3;(_code3=code).push.apply(_code3,_toConsumableArray(this.namedImports.compileNode(o)));}return code;}}]);return ImportClause;}(Base);;ImportClause.prototype.children=['defaultBinding','namedImports'];return ImportClause;}.call(this);exports.ExportDeclaration=ExportDeclaration=function(_ModuleDeclaration2){_inherits(ExportDeclaration,_ModuleDeclaration2);function ExportDeclaration(){_classCallCheck(this,ExportDeclaration);return _possibleConstructorReturn(this,(ExportDeclaration.__proto__||Object.getPrototypeOf(ExportDeclaration)).apply(this,arguments));}_createClass(ExportDeclaration,[{key:'compileNode',value:function compileNode(o){var code,ref1;this.checkScope(o,'export');code=[];code.push(this.makeCode(this.tab+'export '));if(this instanceof ExportDefaultDeclaration){code.push(this.makeCode('default '));}if(!(this instanceof ExportDefaultDeclaration)&&(this.clause instanceof Assign||this.clause instanceof Class)){// Prevent exporting an anonymous class; all exported members must be named
if(this.clause instanceof Class&&!this.clause.variable){this.clause.error('anonymous classes cannot be exported');}code.push(this.makeCode('var '));this.clause.moduleDeclaration='export';}if(this.clause.body!=null&&this.clause.body instanceof Block){code=code.concat(this.clause.compileToFragments(o,LEVEL_TOP));}else{code=code.concat(this.clause.compileNode(o));}if(((ref1=this.source)!=null?ref1.value:void 0)!=null){code.push(this.makeCode(' from '+this.source.value));}code.push(this.makeCode(';'));return code;}}]);return ExportDeclaration;}(ModuleDeclaration);exports.ExportNamedDeclaration=ExportNamedDeclaration=function(_ExportDeclaration){_inherits(ExportNamedDeclaration,_ExportDeclaration);function ExportNamedDeclaration(){_classCallCheck(this,ExportNamedDeclaration);return _possibleConstructorReturn(this,(ExportNamedDeclaration.__proto__||Object.getPrototypeOf(ExportNamedDeclaration)).apply(this,arguments));}return ExportNamedDeclaration;}(ExportDeclaration);exports.ExportDefaultDeclaration=ExportDefaultDeclaration=function(_ExportDeclaration2){_inherits(ExportDefaultDeclaration,_ExportDeclaration2);function ExportDefaultDeclaration(){_classCallCheck(this,ExportDefaultDeclaration);return _possibleConstructorReturn(this,(ExportDefaultDeclaration.__proto__||Object.getPrototypeOf(ExportDefaultDeclaration)).apply(this,arguments));}return ExportDefaultDeclaration;}(ExportDeclaration);exports.ExportAllDeclaration=ExportAllDeclaration=function(_ExportDeclaration3){_inherits(ExportAllDeclaration,_ExportDeclaration3);function ExportAllDeclaration(){_classCallCheck(this,ExportAllDeclaration);return _possibleConstructorReturn(this,(ExportAllDeclaration.__proto__||Object.getPrototypeOf(ExportAllDeclaration)).apply(this,arguments));}return ExportAllDeclaration;}(ExportDeclaration);exports.ModuleSpecifierList=ModuleSpecifierList=function(){var ModuleSpecifierList=function(_Base21){_inherits(ModuleSpecifierList,_Base21);function ModuleSpecifierList(specifiers){_classCallCheck(this,ModuleSpecifierList);var _this55=_possibleConstructorReturn(this,(ModuleSpecifierList.__proto__||Object.getPrototypeOf(ModuleSpecifierList)).call(this));_this55.specifiers=specifiers;return _this55;}_createClass(ModuleSpecifierList,[{key:'compileNode',value:function compileNode(o){var code,compiledList,fragments,index,j,len1,specifier;code=[];o.indent+=TAB;compiledList=function(){var j,len1,ref1,results;ref1=this.specifiers;results=[];for(j=0,len1=ref1.length;j<len1;j++){specifier=ref1[j];results.push(specifier.compileToFragments(o,LEVEL_LIST));}return results;}.call(this);if(this.specifiers.length!==0){code.push(this.makeCode('{\n'+o.indent));for(index=j=0,len1=compiledList.length;j<len1;index=++j){var _code4;fragments=compiledList[index];if(index){code.push(this.makeCode(',\n'+o.indent));}(_code4=code).push.apply(_code4,_toConsumableArray(fragments));}code.push(this.makeCode("\n}"));}else{code.push(this.makeCode('{}'));}return code;}}]);return ModuleSpecifierList;}(Base);;ModuleSpecifierList.prototype.children=['specifiers'];return ModuleSpecifierList;}.call(this);exports.ImportSpecifierList=ImportSpecifierList=function(_ModuleSpecifierList){_inherits(ImportSpecifierList,_ModuleSpecifierList);function ImportSpecifierList(){_classCallCheck(this,ImportSpecifierList);return _possibleConstructorReturn(this,(ImportSpecifierList.__proto__||Object.getPrototypeOf(ImportSpecifierList)).apply(this,arguments));}return ImportSpecifierList;}(ModuleSpecifierList);exports.ExportSpecifierList=ExportSpecifierList=function(_ModuleSpecifierList2){_inherits(ExportSpecifierList,_ModuleSpecifierList2);function ExportSpecifierList(){_classCallCheck(this,ExportSpecifierList);return _possibleConstructorReturn(this,(ExportSpecifierList.__proto__||Object.getPrototypeOf(ExportSpecifierList)).apply(this,arguments));}return ExportSpecifierList;}(ModuleSpecifierList);exports.ModuleSpecifier=ModuleSpecifier=function(){var ModuleSpecifier=function(_Base22){_inherits(ModuleSpecifier,_Base22);function ModuleSpecifier(original,alias,moduleDeclarationType1){_classCallCheck(this,ModuleSpecifier);var ref1,ref2;var _this58=_possibleConstructorReturn(this,(ModuleSpecifier.__proto__||Object.getPrototypeOf(ModuleSpecifier)).call(this));_this58.original=original;_this58.alias=alias;_this58.moduleDeclarationType=moduleDeclarationType1;if(_this58.original.comments||((ref1=_this58.alias)!=null?ref1.comments:void 0)){_this58.comments=[];if(_this58.original.comments){var _this58$comments;(_this58$comments=_this58.comments).push.apply(_this58$comments,_toConsumableArray(_this58.original.comments));}if((ref2=_this58.alias)!=null?ref2.comments:void 0){var _this58$comments2;(_this58$comments2=_this58.comments).push.apply(_this58$comments2,_toConsumableArray(_this58.alias.comments));}}// The name of the variable entering the local scope
_this58.identifier=_this58.alias!=null?_this58.alias.value:_this58.original.value;return _this58;}_createClass(ModuleSpecifier,[{key:'compileNode',value:function compileNode(o){var code;o.scope.find(this.identifier,this.moduleDeclarationType);code=[];code.push(this.makeCode(this.original.value));if(this.alias!=null){code.push(this.makeCode(' as '+this.alias.value));}return code;}}]);return ModuleSpecifier;}(Base);;ModuleSpecifier.prototype.children=['original','alias'];return ModuleSpecifier;}.call(this);exports.ImportSpecifier=ImportSpecifier=function(_ModuleSpecifier){_inherits(ImportSpecifier,_ModuleSpecifier);function ImportSpecifier(imported,local){_classCallCheck(this,ImportSpecifier);return _possibleConstructorReturn(this,(ImportSpecifier.__proto__||Object.getPrototypeOf(ImportSpecifier)).call(this,imported,local,'import'));}_createClass(ImportSpecifier,[{key:'compileNode',value:function compileNode(o){var ref1;// Per the spec, symbols can’t be imported multiple times
// (e.g. `import { foo, foo } from 'lib'` is invalid)
if((ref1=this.identifier,indexOf.call(o.importedSymbols,ref1)>=0)||o.scope.check(this.identifier)){this.error('\''+this.identifier+'\' has already been declared');}else{o.importedSymbols.push(this.identifier);}return _get(ImportSpecifier.prototype.__proto__||Object.getPrototypeOf(ImportSpecifier.prototype),'compileNode',this).call(this,o);}}]);return ImportSpecifier;}(ModuleSpecifier);exports.ImportDefaultSpecifier=ImportDefaultSpecifier=function(_ImportSpecifier){_inherits(ImportDefaultSpecifier,_ImportSpecifier);function ImportDefaultSpecifier(){_classCallCheck(this,ImportDefaultSpecifier);return _possibleConstructorReturn(this,(ImportDefaultSpecifier.__proto__||Object.getPrototypeOf(ImportDefaultSpecifier)).apply(this,arguments));}return ImportDefaultSpecifier;}(ImportSpecifier);exports.ImportNamespaceSpecifier=ImportNamespaceSpecifier=function(_ImportSpecifier2){_inherits(ImportNamespaceSpecifier,_ImportSpecifier2);function ImportNamespaceSpecifier(){_classCallCheck(this,ImportNamespaceSpecifier);return _possibleConstructorReturn(this,(ImportNamespaceSpecifier.__proto__||Object.getPrototypeOf(ImportNamespaceSpecifier)).apply(this,arguments));}return ImportNamespaceSpecifier;}(ImportSpecifier);exports.ExportSpecifier=ExportSpecifier=function(_ModuleSpecifier2){_inherits(ExportSpecifier,_ModuleSpecifier2);function ExportSpecifier(local,exported){_classCallCheck(this,ExportSpecifier);return _possibleConstructorReturn(this,(ExportSpecifier.__proto__||Object.getPrototypeOf(ExportSpecifier)).call(this,local,exported,'export'));}return ExportSpecifier;}(ModuleSpecifier);//### Assign
// The **Assign** is used to assign a local variable to value, or to set the
// property of an object -- including within object literals.
exports.Assign=Assign=function(){var Assign=function(_Base23){_inherits(Assign,_Base23);function Assign(variable1,value1,context1){var options=arguments.length>3&&arguments[3]!==undefined?arguments[3]:{};_classCallCheck(this,Assign);var _this63=_possibleConstructorReturn(this,(Assign.__proto__||Object.getPrototypeOf(Assign)).call(this));_this63.variable=variable1;_this63.value=value1;_this63.context=context1;_this63.param=options.param;_this63.subpattern=options.subpattern;_this63.operatorToken=options.operatorToken;_this63.moduleDeclaration=options.moduleDeclaration;return _this63;}_createClass(Assign,[{key:'isStatement',value:function isStatement(o){return(o!=null?o.level:void 0)===LEVEL_TOP&&this.context!=null&&(this.moduleDeclaration||indexOf.call(this.context,"?")>=0);}},{key:'checkAssignability',value:function checkAssignability(o,varBase){if(Object.prototype.hasOwnProperty.call(o.scope.positions,varBase.value)&&o.scope.variables[o.scope.positions[varBase.value]].type==='import'){return varBase.error('\''+varBase.value+'\' is read-only');}}},{key:'assigns',value:function assigns(name){return this[this.context==='object'?'value':'variable'].assigns(name);}},{key:'unfoldSoak',value:function unfoldSoak(o){return _unfoldSoak(o,this,'variable');}// Compile an assignment, delegating to `compileDestructuring` or
// `compileSplice` if appropriate. Keep track of the name of the base object
// we've been assigned to, for correct internal references. If the variable
// has not been seen yet within the current scope, declare it.
},{key:'compileNode',value:function compileNode(o){var _this64=this;var answer,compiledName,isValue,name,properties,prototype,ref1,ref2,ref3,ref4,ref5,val,varBase;isValue=this.variable instanceof Value;if(isValue){// When compiling `@variable`, remember if it is part of a function parameter.
this.variable.param=this.param;// If `@variable` is an array or an object, we’re destructuring;
// if it’s also `isAssignable()`, the destructuring syntax is supported
// in ES and we can output it as is; otherwise we `@compileDestructuring`
// and convert this ES-unsupported destructuring into acceptable output.
if(this.variable.isArray()||this.variable.isObject()){// This is the left-hand side of an assignment; let `Arr` and `Obj`
// know that, so that those nodes know that they’re assignable as
// destructured variables.
this.variable.base.lhs=true;if(!this.variable.isAssignable()){if(this.variable.isObject()&&this.variable.base.hasSplat()){return this.compileObjectDestruct(o);}else{return this.compileDestructuring(o);}}}if(this.variable.isSplice()){return this.compileSplice(o);}if((ref1=this.context)==='||='||ref1==='&&='||ref1==='?='){return this.compileConditional(o);}if((ref2=this.context)==='//='||ref2==='%%='){return this.compileSpecialMath(o);}}if(!this.context||this.context==='**='){varBase=this.variable.unwrapAll();if(!varBase.isAssignable()){this.variable.error('\''+this.variable.compile(o)+'\' can\'t be assigned');}varBase.eachName(function(name){var commentFragments,commentsNode,message;if(typeof name.hasProperties==="function"?name.hasProperties():void 0){return;}message=isUnassignable(name.value);if(message){name.error(message);}// `moduleDeclaration` can be `'import'` or `'export'`.
_this64.checkAssignability(o,name);if(_this64.moduleDeclaration){return o.scope.add(name.value,_this64.moduleDeclaration);}else if(_this64.param){return o.scope.add(name.value,_this64.param==='alwaysDeclare'?'var':'param');}else{o.scope.find(name.value);// If this assignment identifier has one or more herecomments
// attached, output them as part of the declarations line (unless
// other herecomments are already staged there) for compatibility
// with Flow typing. Don’t do this if this assignment is for a
// class, e.g. `ClassName = class ClassName {`, as Flow requires
// the comment to be between the class name and the `{`.
if(name.comments&&!o.scope.comments[name.value]&&!(_this64.value instanceof Class)&&name.comments.every(function(comment){return comment.here&&!comment.multiline;})){commentsNode=new IdentifierLiteral(name.value);commentsNode.comments=name.comments;commentFragments=[];_this64.compileCommentFragments(o,commentsNode,commentFragments);return o.scope.comments[name.value]=commentFragments;}}});}if(this.value instanceof Code){if(this.value.isStatic){this.value.name=this.variable.properties[0];}else if(((ref3=this.variable.properties)!=null?ref3.length:void 0)>=2){var _ref13,_ref14,_splice$call,_splice$call2;ref4=this.variable.properties,(_ref13=ref4,_ref14=_toArray(_ref13),properties=_ref14.slice(0),_ref13),(_splice$call=splice.call(properties,-2),_splice$call2=_slicedToArray(_splice$call,2),prototype=_splice$call2[0],name=_splice$call2[1],_splice$call);if(((ref5=prototype.name)!=null?ref5.value:void 0)==='prototype'){this.value.name=name;}}}if(this.csx){this.value.base.csxAttribute=true;}val=this.value.compileToFragments(o,LEVEL_LIST);compiledName=this.variable.compileToFragments(o,LEVEL_LIST);if(this.context==='object'){if(this.variable.shouldCache()){compiledName.unshift(this.makeCode('['));compiledName.push(this.makeCode(']'));}return compiledName.concat(this.makeCode(this.csx?'=':': '),val);}answer=compiledName.concat(this.makeCode(' '+(this.context||'=')+' '),val);// Per https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Destructuring_assignment#Assignment_without_declaration,
// if we’re destructuring without declaring, the destructuring assignment must be wrapped in parentheses.
// The assignment is wrapped in parentheses if 'o.level' has lower precedence than LEVEL_LIST (3)
// (i.e. LEVEL_COND (4), LEVEL_OP (5) or LEVEL_ACCESS (6)), or if we're destructuring object, e.g. {a,b} = obj.
if(o.level>LEVEL_LIST||isValue&&this.variable.base instanceof Obj&&!this.nestedLhs&&!(this.param===true)){return this.wrapInParentheses(answer);}else{return answer;}}// Object rest property is not assignable: `{{a}...}`
},{key:'compileObjectDestruct',value:function compileObjectDestruct(o){var assigns,props,refVal,splat,splatProp;this.variable.base.reorderProperties();props=this.variable.base.properties;var _slice1$call9=slice1.call(props,-1);var _slice1$call10=_slicedToArray(_slice1$call9,1);splat=_slice1$call10[0];splatProp=splat.name;assigns=[];refVal=new Value(new IdentifierLiteral(o.scope.freeVariable('ref')));props.splice(-1,1,new Splat(refVal));assigns.push(new Assign(new Value(new Obj(props)),this.value).compileToFragments(o,LEVEL_LIST));assigns.push(new Assign(new Value(splatProp),refVal).compileToFragments(o,LEVEL_LIST));return this.joinFragmentArrays(assigns,', ');}// Brief implementation of recursive pattern matching, when assigning array or
// object literals to a value. Peeks at their properties to assign inner names.
},{key:'compileDestructuring',value:function compileDestructuring(o){var _this65=this;var assignObjects,assigns,code,compSlice,compSplice,complexObjects,expIdx,expans,fragments,hasObjAssigns,i,isExpans,isSplat,leftObjs,loopObjects,obj,objIsUnassignable,objects,olen,processObjects,pushAssign,ref,refExp,restVar,rightObjs,slicer,splatVar,splatVarAssign,splatVarRef,splats,splatsAndExpans,top,value,vvar,vvarText;top=o.level===LEVEL_TOP;value=this.value;objects=this.variable.base.objects;olen=objects.length;// Special-case for `{} = a` and `[] = a` (empty patterns).
// Compile to simply `a`.
if(olen===0){code=value.compileToFragments(o);if(o.level>=LEVEL_OP){return this.wrapInParentheses(code);}else{return code;}}// Disallow `[...] = a` for some reason. (Could be equivalent to `[] = a`?)
var _objects=objects;var _objects2=_slicedToArray(_objects,1);obj=_objects2[0];if(olen===1&&obj instanceof Expansion){obj.error('Destructuring assignment has no target');}// Count all `Splats`: [a, b, c..., d, e]
splats=function(){var j,len1,results;results=[];for(i=j=0,len1=objects.length;j<len1;i=++j){obj=objects[i];if(obj instanceof Splat){results.push(i);}}return results;}();// Count all `Expansions`: [a, b, ..., c, d]
expans=function(){var j,len1,results;results=[];for(i=j=0,len1=objects.length;j<len1;i=++j){obj=objects[i];if(obj instanceof Expansion){results.push(i);}}return results;}();// Combine splats and expansions.
splatsAndExpans=[].concat(_toConsumableArray(splats),_toConsumableArray(expans));// Show error if there is more than one `Splat`, or `Expansion`.
// Examples: [a, b, c..., d, e, f...], [a, b, ..., c, d, ...], [a, b, ..., c, d, e...]
if(splatsAndExpans.length>1){// Sort 'splatsAndExpans' so we can show error at first disallowed token.
objects[splatsAndExpans.sort()[1]].error("multiple splats/expansions are disallowed in an assignment");}isSplat=(splats!=null?splats.length:void 0)>0;isExpans=(expans!=null?expans.length:void 0)>0;vvar=value.compileToFragments(o,LEVEL_LIST);vvarText=fragmentsToText(vvar);assigns=[];pushAssign=function pushAssign(variable,val){return assigns.push(new Assign(variable,val,null,{param:_this65.param,subpattern:true}).compileToFragments(o,LEVEL_LIST));};if(isSplat){splatVar=objects[splats[0]].name.unwrap();if(splatVar instanceof Arr||splatVar instanceof Obj){splatVarRef=new IdentifierLiteral(o.scope.freeVariable('ref'));objects[splats[0]].name=splatVarRef;splatVarAssign=function splatVarAssign(){return pushAssign(new Value(splatVar),splatVarRef);};}}// At this point, there are several things to destructure. So the `fn()` in
// `{a, b} = fn()` must be cached, for example. Make vvar into a simple
// variable if it isn’t already.
if(!(value.unwrap()instanceof IdentifierLiteral)||this.variable.assigns(vvarText)){ref=o.scope.freeVariable('ref');assigns.push([this.makeCode(ref+' = ')].concat(_toConsumableArray(vvar)));vvar=[this.makeCode(ref)];vvarText=ref;}slicer=function slicer(type){return function(vvar,start){var end=arguments.length>2&&arguments[2]!==undefined?arguments[2]:false;var args,slice;if(!(vvar instanceof Value)){vvar=new IdentifierLiteral(vvar);}args=[vvar,new NumberLiteral(start)];if(end){args.push(new NumberLiteral(end));}slice=new Value(new IdentifierLiteral(utility(type,o)),[new Access(new PropertyName('call'))]);return new Value(new Call(slice,args));};};// Helper which outputs `[].slice` code.
compSlice=slicer("slice");// Helper which outputs `[].splice` code.
compSplice=slicer("splice");// Check if `objects` array contains any instance of `Assign`, e.g. {a:1}.
hasObjAssigns=function hasObjAssigns(objs){var j,len1,results;results=[];for(i=j=0,len1=objs.length;j<len1;i=++j){obj=objs[i];if(obj instanceof Assign&&obj.context==='object'){results.push(i);}}return results;};// Check if `objects` array contains any unassignable object.
objIsUnassignable=function objIsUnassignable(objs){var j,len1;for(j=0,len1=objs.length;j<len1;j++){obj=objs[j];if(!obj.isAssignable()){return true;}}return false;};// `objects` are complex when there is object assign ({a:1}),
// unassignable object, or just a single node.
complexObjects=function complexObjects(objs){return hasObjAssigns(objs).length||objIsUnassignable(objs)||olen===1;};// "Complex" `objects` are processed in a loop.
// Examples: [a, b, {c, r...}, d], [a, ..., {b, r...}, c, d]
loopObjects=function loopObjects(objs,vvar,vvarTxt){var acc,idx,j,len1,message,results,vval;results=[];for(i=j=0,len1=objs.length;j<len1;i=++j){obj=objs[i];if(obj instanceof Elision){// `Elision` can be skipped.
continue;}// If `obj` is {a: 1}
if(obj instanceof Assign&&obj.context==='object'){var _obj=obj;idx=_obj.variable.base;vvar=_obj.value;if(vvar instanceof Assign){var _vvar=vvar;vvar=_vvar.variable;}idx=vvar.this?vvar.properties[0].name:new PropertyName(vvar.unwrap().value);acc=idx.unwrap()instanceof PropertyName;vval=new Value(value,[new(acc?Access:Index)(idx)]);}else{// `obj` is [a...], {a...} or a
vvar=function(){switch(false){case!(obj instanceof Splat):return new Value(obj.name);default:return obj;}}();vval=function(){switch(false){case!(obj instanceof Splat):return compSlice(vvarTxt,i);default:return new Value(new Literal(vvarTxt),[new Index(new NumberLiteral(i))]);}}();}message=isUnassignable(vvar.unwrap().value);if(message){vvar.error(message);}results.push(pushAssign(vvar,vval));}return results;};// "Simple" `objects` can be split and compiled to arrays, [a, b, c] = arr, [a, b, c...] = arr
assignObjects=function assignObjects(objs,vvar,vvarTxt){var vval;vvar=new Value(new Arr(objs,true));vval=vvarTxt instanceof Value?vvarTxt:new Value(new Literal(vvarTxt));return pushAssign(vvar,vval);};processObjects=function processObjects(objs,vvar,vvarTxt){if(complexObjects(objs)){return loopObjects(objs,vvar,vvarTxt);}else{return assignObjects(objs,vvar,vvarTxt);}};// In case there is `Splat` or `Expansion` in `objects`,
// we can split array in two simple subarrays.
// `Splat` [a, b, c..., d, e] can be split into  [a, b, c...] and [d, e].
// `Expansion` [a, b, ..., c, d] can be split into [a, b] and [c, d].
// Examples:
// a) `Splat`
//   CS: [a, b, c..., d, e] = arr
//   JS: [a, b, ...c] = arr, [d, e] = splice.call(c, -2)
// b) `Expansion`
//   CS: [a, b, ..., d, e] = arr
//   JS: [a, b] = arr, [d, e] = slice.call(arr, -2)
if(splatsAndExpans.length){expIdx=splatsAndExpans[0];leftObjs=objects.slice(0,expIdx+(isSplat?1:0));rightObjs=objects.slice(expIdx+1);if(leftObjs.length!==0){processObjects(leftObjs,vvar,vvarText);}if(rightObjs.length!==0){// Slice or splice `objects`.
refExp=function(){switch(false){case!isSplat:return compSplice(new Value(objects[expIdx].name),rightObjs.length*-1);case!isExpans:return compSlice(vvarText,rightObjs.length*-1);}}();if(complexObjects(rightObjs)){restVar=refExp;refExp=o.scope.freeVariable('ref');assigns.push([this.makeCode(refExp+' = ')].concat(_toConsumableArray(restVar.compileToFragments(o,LEVEL_LIST))));}processObjects(rightObjs,vvar,refExp);}}else{// There is no `Splat` or `Expansion` in `objects`.
processObjects(objects,vvar,vvarText);}if(typeof splatVarAssign==="function"){splatVarAssign();}if(!(top||this.subpattern)){assigns.push(vvar);}fragments=this.joinFragmentArrays(assigns,', ');if(o.level<LEVEL_LIST){return fragments;}else{return this.wrapInParentheses(fragments);}}// When compiling a conditional assignment, take care to ensure that the
// operands are only evaluated once, even though we have to reference them
// more than once.
},{key:'compileConditional',value:function compileConditional(o){var fragments,left,right;// Disallow conditional assignment of undefined variables.
var _variable$cacheRefere=this.variable.cacheReference(o);var _variable$cacheRefere2=_slicedToArray(_variable$cacheRefere,2);left=_variable$cacheRefere2[0];right=_variable$cacheRefere2[1];if(!left.properties.length&&left.base instanceof Literal&&!(left.base instanceof ThisLiteral)&&!o.scope.check(left.base.value)){this.variable.error('the variable "'+left.base.value+'" can\'t be assigned with '+this.context+' because it has not been declared before');}if(indexOf.call(this.context,"?")>=0){o.isExistentialEquals=true;return new If(new Existence(left),right,{type:'if'}).addElse(new Assign(right,this.value,'=')).compileToFragments(o);}else{fragments=new Op(this.context.slice(0,-1),left,new Assign(right,this.value,'=')).compileToFragments(o);if(o.level<=LEVEL_LIST){return fragments;}else{return this.wrapInParentheses(fragments);}}}// Convert special math assignment operators like `a //= b` to the equivalent
// extended form `a = a ** b` and then compiles that.
},{key:'compileSpecialMath',value:function compileSpecialMath(o){var left,right;var _variable$cacheRefere3=this.variable.cacheReference(o);var _variable$cacheRefere4=_slicedToArray(_variable$cacheRefere3,2);left=_variable$cacheRefere4[0];right=_variable$cacheRefere4[1];return new Assign(left,new Op(this.context.slice(0,-1),right,this.value)).compileToFragments(o);}// Compile the assignment from an array splice literal, using JavaScript's
// `Array#splice` method.
},{key:'compileSplice',value:function compileSplice(o){var answer,exclusive,from,fromDecl,fromRef,name,to,unwrappedVar,valDef,valRef;var _variable$properties$=this.variable.properties.pop();var _variable$properties$2=_variable$properties$.range;from=_variable$properties$2.from;to=_variable$properties$2.to;exclusive=_variable$properties$2.exclusive;unwrappedVar=this.variable.unwrapAll();if(unwrappedVar.comments){moveComments(unwrappedVar,this);delete this.variable.comments;}name=this.variable.compile(o);if(from){var _cacheToCodeFragments7=this.cacheToCodeFragments(from.cache(o,LEVEL_OP));var _cacheToCodeFragments8=_slicedToArray(_cacheToCodeFragments7,2);fromDecl=_cacheToCodeFragments8[0];fromRef=_cacheToCodeFragments8[1];}else{fromDecl=fromRef='0';}if(to){if((from!=null?from.isNumber():void 0)&&to.isNumber()){to=to.compile(o)-fromRef;if(!exclusive){to+=1;}}else{to=to.compile(o,LEVEL_ACCESS)+' - '+fromRef;if(!exclusive){to+=' + 1';}}}else{to="9e9";}var _value$cache=this.value.cache(o,LEVEL_LIST);var _value$cache2=_slicedToArray(_value$cache,2);valDef=_value$cache2[0];valRef=_value$cache2[1];answer=[].concat(this.makeCode(utility('splice',o)+'.apply('+name+', ['+fromDecl+', '+to+'].concat('),valDef,this.makeCode(")), "),valRef);if(o.level>LEVEL_TOP){return this.wrapInParentheses(answer);}else{return answer;}}},{key:'eachName',value:function eachName(iterator){return this.variable.unwrapAll().eachName(iterator);}}]);return Assign;}(Base);;Assign.prototype.children=['variable','value'];Assign.prototype.isAssignable=YES;return Assign;}.call(this);//### FuncGlyph
exports.FuncGlyph=FuncGlyph=function(_Base24){_inherits(FuncGlyph,_Base24);function FuncGlyph(glyph){_classCallCheck(this,FuncGlyph);var _this66=_possibleConstructorReturn(this,(FuncGlyph.__proto__||Object.getPrototypeOf(FuncGlyph)).call(this));_this66.glyph=glyph;return _this66;}return FuncGlyph;}(Base);//### Code
// A function definition. This is the only node that creates a new Scope.
// When for the purposes of walking the contents of a function body, the Code
// has no *children* -- they're within the inner scope.
exports.Code=Code=function(){var Code=function(_Base25){_inherits(Code,_Base25);function Code(params,body,funcGlyph,paramStart){_classCallCheck(this,Code);var ref1;var _this67=_possibleConstructorReturn(this,(Code.__proto__||Object.getPrototypeOf(Code)).call(this));_this67.funcGlyph=funcGlyph;_this67.paramStart=paramStart;_this67.params=params||[];_this67.body=body||new Block();_this67.bound=((ref1=_this67.funcGlyph)!=null?ref1.glyph:void 0)==='=>';_this67.isGenerator=false;_this67.isAsync=false;_this67.isMethod=false;_this67.body.traverseChildren(false,function(node){if(node instanceof Op&&node.isYield()||node instanceof YieldReturn){_this67.isGenerator=true;}if(node instanceof Op&&node.isAwait()||node instanceof AwaitReturn){_this67.isAsync=true;}if(node instanceof For&&node.isAwait()){return _this67.isAsync=true;}});return _this67;}_createClass(Code,[{key:'isStatement',value:function isStatement(){return this.isMethod;}},{key:'makeScope',value:function makeScope(parentScope){return new Scope(parentScope,this.body,this);}// Compilation creates a new scope unless explicitly asked to share with the
// outer scope. Handles splat parameters in the parameter list by setting
// such parameters to be the final parameter in the function definition, as
// required per the ES2015 spec. If the CoffeeScript function definition had
// parameters after the splat, they are declared via expressions in the
// function body.
},{key:'compileNode',value:function compileNode(o){var _body$expressions3,_answer5;var answer,body,boundMethodCheck,comment,condition,exprs,generatedVariables,haveBodyParam,haveSplatParam,i,ifTrue,j,k,l,len1,len2,len3,m,methodScope,modifiers,name,param,paramNames,paramToAddToScope,params,paramsAfterSplat,ref,ref1,ref2,ref3,ref4,ref5,ref6,ref7,ref8,scopeVariablesCount,signature,splatParamName,thisAssignments,wasEmpty,yieldNode;if(this.ctor){if(this.isAsync){this.name.error('Class constructor may not be async');}if(this.isGenerator){this.name.error('Class constructor may not be a generator');}}if(this.bound){if((ref1=o.scope.method)!=null?ref1.bound:void 0){this.context=o.scope.method.context;}if(!this.context){this.context='this';}}o.scope=del(o,'classScope')||this.makeScope(o.scope);o.scope.shared=del(o,'sharedScope');o.indent+=TAB;delete o.bare;delete o.isExistentialEquals;params=[];exprs=[];thisAssignments=(ref2=(ref3=this.thisAssignments)!=null?ref3.slice():void 0)!=null?ref2:[];paramsAfterSplat=[];haveSplatParam=false;haveBodyParam=false;// Check for duplicate parameters and separate `this` assignments.
paramNames=[];this.eachParamName(function(name,node,param,obj){var replacement,target;if(indexOf.call(paramNames,name)>=0){node.error('multiple parameters named \''+name+'\'');}paramNames.push(name);if(node.this){name=node.properties[0].name.value;if(indexOf.call(JS_FORBIDDEN,name)>=0){name='_'+name;}target=new IdentifierLiteral(o.scope.freeVariable(name,{reserve:false}));// `Param` is object destructuring with a default value: ({@prop = 1}) ->
// In a case when the variable name is already reserved, we have to assign
// a new variable name to the destructured variable: ({prop:prop1 = 1}) ->
replacement=param.name instanceof Obj&&obj instanceof Assign&&obj.operatorToken.value==='='?new Assign(new IdentifierLiteral(name),target,'object'):target;//, operatorToken: new Literal ':'
param.renameParam(node,replacement);return thisAssignments.push(new Assign(node,target));}});ref4=this.params;// Parse the parameters, adding them to the list of parameters to put in the
// function definition; and dealing with splats or expansions, including
// adding expressions to the function body to declare all parameter
// variables that would have been after the splat/expansion parameter.
// If we encounter a parameter that needs to be declared in the function
// body for any reason, for example it’s destructured with `this`, also
// declare and assign all subsequent parameters in the function body so that
// any non-idempotent parameters are evaluated in the correct order.
for(i=j=0,len1=ref4.length;j<len1;i=++j){param=ref4[i];// Was `...` used with this parameter? (Only one such parameter is allowed
// per function.) Splat/expansion parameters cannot have default values,
// so we need not worry about that.
if(param.splat||param instanceof Expansion){if(haveSplatParam){param.error('only one splat or expansion parameter is allowed per function definition');}else if(param instanceof Expansion&&this.params.length===1){param.error('an expansion parameter cannot be the only parameter in a function definition');}haveSplatParam=true;if(param.splat){if(param.name instanceof Arr||param.name instanceof Obj){// Splat arrays are treated oddly by ES; deal with them the legacy
// way in the function body. TODO: Should this be handled in the
// function parameter list, and if so, how?
splatParamName=o.scope.freeVariable('arg');params.push(ref=new Value(new IdentifierLiteral(splatParamName)));exprs.push(new Assign(new Value(param.name),ref));}else{params.push(ref=param.asReference(o));splatParamName=fragmentsToText(ref.compileNodeWithoutComments(o));}if(param.shouldCache()){exprs.push(new Assign(new Value(param.name),ref));// `param` is an Expansion
}}else{splatParamName=o.scope.freeVariable('args');params.push(new Value(new IdentifierLiteral(splatParamName)));}o.scope.parameter(splatParamName);}else{// Parse all other parameters; if a splat paramater has not yet been
// encountered, add these other parameters to the list to be output in
// the function definition.
if(param.shouldCache()||haveBodyParam){param.assignedInBody=true;haveBodyParam=true;// This parameter cannot be declared or assigned in the parameter
// list. So put a reference in the parameter list and add a statement
// to the function body assigning it, e.g.
// `(arg) => { var a = arg.a; }`, with a default value if it has one.
if(param.value!=null){condition=new Op('===',param,new UndefinedLiteral());ifTrue=new Assign(new Value(param.name),param.value);exprs.push(new If(condition,ifTrue));}else{exprs.push(new Assign(new Value(param.name),param.asReference(o),null,{param:'alwaysDeclare'}));}}// If this parameter comes before the splat or expansion, it will go
// in the function definition parameter list.
if(!haveSplatParam){// If this parameter has a default value, and it hasn’t already been
// set by the `shouldCache()` block above, define it as a statement in
// the function body. This parameter comes after the splat parameter,
// so we can’t define its default value in the parameter list.
if(param.shouldCache()){ref=param.asReference(o);}else{if(param.value!=null&&!param.assignedInBody){ref=new Assign(new Value(param.name),param.value,null,{param:true});}else{ref=param;}}// Add this parameter’s reference(s) to the function scope.
if(param.name instanceof Arr||param.name instanceof Obj){// This parameter is destructured.
param.name.lhs=true;if(!param.shouldCache()){param.name.eachName(function(prop){return o.scope.parameter(prop.value);});}}else{// This compilation of the parameter is only to get its name to add
// to the scope name tracking; since the compilation output here
// isn’t kept for eventual output, don’t include comments in this
// compilation, so that they get output the “real” time this param
// is compiled.
paramToAddToScope=param.value!=null?param:ref;o.scope.parameter(fragmentsToText(paramToAddToScope.compileToFragmentsWithoutComments(o)));}params.push(ref);}else{paramsAfterSplat.push(param);// If this parameter had a default value, since it’s no longer in the
// function parameter list we need to assign its default value
// (if necessary) as an expression in the body.
if(param.value!=null&&!param.shouldCache()){condition=new Op('===',param,new UndefinedLiteral());ifTrue=new Assign(new Value(param.name),param.value);exprs.push(new If(condition,ifTrue));}if(((ref5=param.name)!=null?ref5.value:void 0)!=null){// Add this parameter to the scope, since it wouldn’t have been added
// yet since it was skipped earlier.
o.scope.add(param.name.value,'var',true);}}}}// If there were parameters after the splat or expansion parameter, those
// parameters need to be assigned in the body of the function.
if(paramsAfterSplat.length!==0){// Create a destructured assignment, e.g. `[a, b, c] = [args..., b, c]`
exprs.unshift(new Assign(new Value(new Arr([new Splat(new IdentifierLiteral(splatParamName))].concat(_toConsumableArray(function(){var k,len2,results;results=[];for(k=0,len2=paramsAfterSplat.length;k<len2;k++){param=paramsAfterSplat[k];results.push(param.asReference(o));}return results;}())))),new Value(new IdentifierLiteral(splatParamName))));}// Add new expressions to the function body
wasEmpty=this.body.isEmpty();if(!this.expandCtorSuper(thisAssignments)){var _body$expressions2;(_body$expressions2=this.body.expressions).unshift.apply(_body$expressions2,_toConsumableArray(thisAssignments));}(_body$expressions3=this.body.expressions).unshift.apply(_body$expressions3,_toConsumableArray(exprs));if(this.isMethod&&this.bound&&!this.isStatic&&this.classVariable){boundMethodCheck=new Value(new Literal(utility('boundMethodCheck',o)));this.body.expressions.unshift(new Call(boundMethodCheck,[new Value(new ThisLiteral()),this.classVariable]));}if(!(wasEmpty||this.noReturn)){this.body.makeReturn();}// JavaScript doesn’t allow bound (`=>`) functions to also be generators.
// This is usually caught via `Op::compileContinuation`, but double-check:
if(this.bound&&this.isGenerator){yieldNode=this.body.contains(function(node){return node instanceof Op&&node.operator==='yield';});(yieldNode||this).error('yield cannot occur inside bound (fat arrow) functions');}// Assemble the output
modifiers=[];if(this.isMethod&&this.isStatic){modifiers.push('static');}if(this.isAsync){modifiers.push('async');}if(!(this.isMethod||this.bound)){modifiers.push('function'+(this.isGenerator?'*':''));}else if(this.isGenerator){modifiers.push('*');}signature=[this.makeCode('(')];// Block comments between a function name and `(` get output between
// `function` and `(`.
if(((ref6=this.paramStart)!=null?ref6.comments:void 0)!=null){this.compileCommentFragments(o,this.paramStart,signature);}for(i=k=0,len2=params.length;k<len2;i=++k){var _signature;param=params[i];if(i!==0){signature.push(this.makeCode(', '));}if(haveSplatParam&&i===params.length-1){signature.push(this.makeCode('...'));}// Compile this parameter, but if any generated variables get created
// (e.g. `ref`), shift those into the parent scope since we can’t put a
// `var` line inside a function parameter list.
scopeVariablesCount=o.scope.variables.length;(_signature=signature).push.apply(_signature,_toConsumableArray(param.compileToFragments(o)));if(scopeVariablesCount!==o.scope.variables.length){var _o$scope$parent$varia;generatedVariables=o.scope.variables.splice(scopeVariablesCount);(_o$scope$parent$varia=o.scope.parent.variables).push.apply(_o$scope$parent$varia,_toConsumableArray(generatedVariables));}}signature.push(this.makeCode(')'));// Block comments between `)` and `->`/`=>` get output between `)` and `{`.
if(((ref7=this.funcGlyph)!=null?ref7.comments:void 0)!=null){ref8=this.funcGlyph.comments;for(l=0,len3=ref8.length;l<len3;l++){comment=ref8[l];comment.unshift=false;}this.compileCommentFragments(o,this.funcGlyph,signature);}if(!this.body.isEmpty()){body=this.body.compileWithDeclarations(o);}// We need to compile the body before method names to ensure `super`
// references are handled.
if(this.isMethod){var _ref15=[o.scope,o.scope.parent];methodScope=_ref15[0];o.scope=_ref15[1];name=this.name.compileToFragments(o);if(name[0].code==='.'){name.shift();}o.scope=methodScope;}answer=this.joinFragmentArrays(function(){var len4,p,results;results=[];for(p=0,len4=modifiers.length;p<len4;p++){m=modifiers[p];results.push(this.makeCode(m));}return results;}.call(this),' ');if(modifiers.length&&name){answer.push(this.makeCode(' '));}if(name){var _answer4;(_answer4=answer).push.apply(_answer4,_toConsumableArray(name));}(_answer5=answer).push.apply(_answer5,_toConsumableArray(signature));if(this.bound&&!this.isMethod){answer.push(this.makeCode(' =>'));}answer.push(this.makeCode(' {'));if(body!=null?body.length:void 0){var _answer6;(_answer6=answer).push.apply(_answer6,[this.makeCode('\n')].concat(_toConsumableArray(body),[this.makeCode('\n'+this.tab)]));}answer.push(this.makeCode('}'));if(this.isMethod){return indentInitial(answer,this);}if(this.front||o.level>=LEVEL_ACCESS){return this.wrapInParentheses(answer);}else{return answer;}}},{key:'eachParamName',value:function eachParamName(iterator){var j,len1,param,ref1,results;ref1=this.params;results=[];for(j=0,len1=ref1.length;j<len1;j++){param=ref1[j];results.push(param.eachName(iterator));}return results;}// Short-circuit `traverseChildren` method to prevent it from crossing scope
// boundaries unless `crossScope` is `true`.
},{key:'traverseChildren',value:function traverseChildren(crossScope,func){if(crossScope){return _get(Code.prototype.__proto__||Object.getPrototypeOf(Code.prototype),'traverseChildren',this).call(this,crossScope,func);}}// Short-circuit `replaceInContext` method to prevent it from crossing context boundaries. Bound
// functions have the same context.
},{key:'replaceInContext',value:function replaceInContext(child,replacement){if(this.bound){return _get(Code.prototype.__proto__||Object.getPrototypeOf(Code.prototype),'replaceInContext',this).call(this,child,replacement);}else{return false;}}},{key:'expandCtorSuper',value:function expandCtorSuper(thisAssignments){var _this68=this;var haveThisParam,param,ref1,seenSuper;if(!this.ctor){return false;}this.eachSuperCall(Block.wrap(this.params),function(superCall){return superCall.error("'super' is not allowed in constructor parameter defaults");});seenSuper=this.eachSuperCall(this.body,function(superCall){if(_this68.ctor==='base'){superCall.error("'super' is only allowed in derived class constructors");}return superCall.expressions=thisAssignments;});haveThisParam=thisAssignments.length&&thisAssignments.length!==((ref1=this.thisAssignments)!=null?ref1.length:void 0);if(this.ctor==='derived'&&!seenSuper&&haveThisParam){param=thisAssignments[0].variable;param.error("Can't use @params in derived class constructors without calling super");}return seenSuper;}// Find all super calls in the given context node;
// returns `true` if `iterator` is called.
},{key:'eachSuperCall',value:function eachSuperCall(context,iterator){var _this69=this;var seenSuper;seenSuper=false;context.traverseChildren(true,function(child){var childArgs;if(child instanceof SuperCall){// `super` in a constructor (the only `super` without an accessor)
// cannot be given an argument with a reference to `this`, as that would
// be referencing `this` before calling `super`.
if(!child.variable.accessor){childArgs=child.args.filter(function(arg){return!(arg instanceof Class)&&(!(arg instanceof Code)||arg.bound);});Block.wrap(childArgs).traverseChildren(true,function(node){if(node.this){return node.error("Can't call super with @params in derived class constructors");}});}seenSuper=true;iterator(child);}else if(child instanceof ThisLiteral&&_this69.ctor==='derived'&&!seenSuper){child.error("Can't reference 'this' before calling super in derived class constructors");}// `super` has the same target in bound (arrow) functions, so check them too
return!(child instanceof SuperCall)&&(!(child instanceof Code)||child.bound);});return seenSuper;}}]);return Code;}(Base);;Code.prototype.children=['params','body'];Code.prototype.jumps=NO;return Code;}.call(this);//### Param
// A parameter in a function definition. Beyond a typical JavaScript parameter,
// these parameters can also attach themselves to the context of the function,
// as well as be a splat, gathering up a group of parameters into an array.
exports.Param=Param=function(){var Param=function(_Base26){_inherits(Param,_Base26);function Param(name1,value1,splat1){_classCallCheck(this,Param);var message,token;var _this70=_possibleConstructorReturn(this,(Param.__proto__||Object.getPrototypeOf(Param)).call(this));_this70.name=name1;_this70.value=value1;_this70.splat=splat1;message=isUnassignable(_this70.name.unwrapAll().value);if(message){_this70.name.error(message);}if(_this70.name instanceof Obj&&_this70.name.generated){token=_this70.name.objects[0].operatorToken;token.error('unexpected '+token.value);}return _this70;}_createClass(Param,[{key:'compileToFragments',value:function compileToFragments(o){return this.name.compileToFragments(o,LEVEL_LIST);}},{key:'compileToFragmentsWithoutComments',value:function compileToFragmentsWithoutComments(o){return this.name.compileToFragmentsWithoutComments(o,LEVEL_LIST);}},{key:'asReference',value:function asReference(o){var name,node;if(this.reference){return this.reference;}node=this.name;if(node.this){name=node.properties[0].name.value;if(indexOf.call(JS_FORBIDDEN,name)>=0){name='_'+name;}node=new IdentifierLiteral(o.scope.freeVariable(name));}else if(node.shouldCache()){node=new IdentifierLiteral(o.scope.freeVariable('arg'));}node=new Value(node);node.updateLocationDataIfMissing(this.locationData);return this.reference=node;}},{key:'shouldCache',value:function shouldCache(){return this.name.shouldCache();}// Iterates the name or names of a `Param`.
// In a sense, a destructured parameter represents multiple JS parameters. This
// method allows to iterate them all.
// The `iterator` function will be called as `iterator(name, node)` where
// `name` is the name of the parameter and `node` is the AST node corresponding
// to that name.
},{key:'eachName',value:function eachName(iterator){var _this71=this;var name=arguments.length>1&&arguments[1]!==undefined?arguments[1]:this.name;var atParam,j,len1,nObj,node,obj,ref1,ref2;atParam=function atParam(obj){var originalObj=arguments.length>1&&arguments[1]!==undefined?arguments[1]:null;return iterator('@'+obj.properties[0].name.value,obj,_this71,originalObj);};if(name instanceof Literal){// * simple literals `foo`
return iterator(name.value,name,this);}if(name instanceof Value){// * at-params `@foo`
return atParam(name);}ref2=(ref1=name.objects)!=null?ref1:[];for(j=0,len1=ref2.length;j<len1;j++){obj=ref2[j];// Save original obj.
nObj=obj;// * destructured parameter with default value
if(obj instanceof Assign&&obj.context==null){obj=obj.variable;}// * assignments within destructured parameters `{foo:bar}`
if(obj instanceof Assign){// ... possibly with a default value
if(obj.value instanceof Assign){obj=obj.value.variable;}else{obj=obj.value;}this.eachName(iterator,obj.unwrap());// * splats within destructured parameters `[xs...]`
}else if(obj instanceof Splat){node=obj.name.unwrap();iterator(node.value,node,this);}else if(obj instanceof Value){// * destructured parameters within destructured parameters `[{a}]`
if(obj.isArray()||obj.isObject()){this.eachName(iterator,obj.base);// * at-params within destructured parameters `{@foo}`
}else if(obj.this){atParam(obj,nObj);}else{// * simple destructured parameters {foo}
iterator(obj.base.value,obj.base,this);}}else if(obj instanceof Elision){obj;}else if(!(obj instanceof Expansion)){obj.error('illegal parameter '+obj.compile());}}}// Rename a param by replacing the given AST node for a name with a new node.
// This needs to ensure that the the source for object destructuring does not change.
},{key:'renameParam',value:function renameParam(node,newNode){var isNode,replacement;isNode=function isNode(candidate){return candidate===node;};replacement=function replacement(node,parent){var key;if(parent instanceof Obj){key=node;if(node.this){key=node.properties[0].name;}// No need to assign a new variable for the destructured variable if the variable isn't reserved.
// Examples:
// `({@foo}) ->`  should compile to `({foo}) { this.foo = foo}`
// `foo = 1; ({@foo}) ->` should compile to `foo = 1; ({foo:foo1}) { this.foo = foo1 }`
if(node.this&&key.value===newNode.value){return new Value(newNode);}else{return new Assign(new Value(key),newNode,'object');}}else{return newNode;}};return this.replaceInContext(isNode,replacement);}}]);return Param;}(Base);;Param.prototype.children=['name','value'];return Param;}.call(this);//### Splat
// A splat, either as a parameter to a function, an argument to a call,
// or as part of a destructuring assignment.
exports.Splat=Splat=function(){var Splat=function(_Base27){_inherits(Splat,_Base27);function Splat(name){_classCallCheck(this,Splat);var _this72=_possibleConstructorReturn(this,(Splat.__proto__||Object.getPrototypeOf(Splat)).call(this));_this72.name=name.compile?name:new Literal(name);return _this72;}_createClass(Splat,[{key:'shouldCache',value:function shouldCache(){return false;}},{key:'isAssignable',value:function isAssignable(){if(this.name instanceof Obj||this.name instanceof Parens){return false;}return this.name.isAssignable()&&(!this.name.isAtomic||this.name.isAtomic());}},{key:'assigns',value:function assigns(name){return this.name.assigns(name);}},{key:'compileNode',value:function compileNode(o){return[this.makeCode('...')].concat(_toConsumableArray(this.name.compileToFragments(o,LEVEL_OP)));}},{key:'unwrap',value:function unwrap(){return this.name;}}]);return Splat;}(Base);;Splat.prototype.children=['name'];return Splat;}.call(this);//### Expansion
// Used to skip values inside an array destructuring (pattern matching) or
// parameter list.
exports.Expansion=Expansion=function(){var Expansion=function(_Base28){_inherits(Expansion,_Base28);function Expansion(){_classCallCheck(this,Expansion);return _possibleConstructorReturn(this,(Expansion.__proto__||Object.getPrototypeOf(Expansion)).apply(this,arguments));}_createClass(Expansion,[{key:'compileNode',value:function compileNode(o){return this.error('Expansion must be used inside a destructuring assignment or parameter list');}},{key:'asReference',value:function asReference(o){return this;}},{key:'eachName',value:function eachName(iterator){}}]);return Expansion;}(Base);;Expansion.prototype.shouldCache=NO;return Expansion;}.call(this);//### Elision
// Array elision element (for example, [,a, , , b, , c, ,]).
exports.Elision=Elision=function(){var Elision=function(_Base29){_inherits(Elision,_Base29);function Elision(){_classCallCheck(this,Elision);return _possibleConstructorReturn(this,(Elision.__proto__||Object.getPrototypeOf(Elision)).apply(this,arguments));}_createClass(Elision,[{key:'compileToFragments',value:function compileToFragments(o,level){var fragment;fragment=_get(Elision.prototype.__proto__||Object.getPrototypeOf(Elision.prototype),'compileToFragments',this).call(this,o,level);fragment.isElision=true;return fragment;}},{key:'compileNode',value:function compileNode(o){return[this.makeCode(', ')];}},{key:'asReference',value:function asReference(o){return this;}},{key:'eachName',value:function eachName(iterator){}}]);return Elision;}(Base);;Elision.prototype.isAssignable=YES;Elision.prototype.shouldCache=NO;return Elision;}.call(this);//### While
// A while loop, the only sort of low-level loop exposed by CoffeeScript. From
// it, all other loops can be manufactured. Useful in cases where you need more
// flexibility or more speed than a comprehension can provide.
exports.While=While=function(){var While=function(_Base30){_inherits(While,_Base30);function While(condition,options){_classCallCheck(this,While);var _this75=_possibleConstructorReturn(this,(While.__proto__||Object.getPrototypeOf(While)).call(this));_this75.condition=(options!=null?options.invert:void 0)?condition.invert():condition;_this75.guard=options!=null?options.guard:void 0;return _this75;}_createClass(While,[{key:'makeReturn',value:function makeReturn(res){if(res){return _get(While.prototype.__proto__||Object.getPrototypeOf(While.prototype),'makeReturn',this).call(this,res);}else{this.returns=!this.jumps();return this;}}},{key:'addBody',value:function addBody(body1){this.body=body1;return this;}},{key:'jumps',value:function jumps(){var expressions,j,jumpNode,len1,node;expressions=this.body.expressions;if(!expressions.length){return false;}for(j=0,len1=expressions.length;j<len1;j++){node=expressions[j];if(jumpNode=node.jumps({loop:true})){return jumpNode;}}return false;}// The main difference from a JavaScript *while* is that the CoffeeScript
// *while* can be used as a part of a larger expression -- while loops may
// return an array containing the computed result of each iteration.
},{key:'compileNode',value:function compileNode(o){var answer,body,rvar,set;o.indent+=TAB;set='';body=this.body;if(body.isEmpty()){body=this.makeCode('');}else{if(this.returns){body.makeReturn(rvar=o.scope.freeVariable('results'));set=''+this.tab+rvar+' = [];\n';}if(this.guard){if(body.expressions.length>1){body.expressions.unshift(new If(new Parens(this.guard).invert(),new StatementLiteral("continue")));}else{if(this.guard){body=Block.wrap([new If(this.guard,body)]);}}}body=[].concat(this.makeCode("\n"),body.compileToFragments(o,LEVEL_TOP),this.makeCode('\n'+this.tab));}answer=[].concat(this.makeCode(set+this.tab+"while ("),this.condition.compileToFragments(o,LEVEL_PAREN),this.makeCode(") {"),body,this.makeCode("}"));if(this.returns){answer.push(this.makeCode('\n'+this.tab+'return '+rvar+';'));}return answer;}}]);return While;}(Base);;While.prototype.children=['condition','guard','body'];While.prototype.isStatement=YES;return While;}.call(this);//### Op
// Simple Arithmetic and logical operations. Performs some conversion from
// CoffeeScript operations into their JavaScript equivalents.
exports.Op=Op=function(){var CONVERSIONS,INVERSIONS;var Op=function(_Base31){_inherits(Op,_Base31);function Op(op,first,second,flip){var _ret5;_classCallCheck(this,Op);var firstCall;var _this76=_possibleConstructorReturn(this,(Op.__proto__||Object.getPrototypeOf(Op)).call(this));if(op==='in'){var _ret2;return _ret2=new In(first,second),_possibleConstructorReturn(_this76,_ret2);}if(op==='do'){var _ret3;return _ret3=Op.prototype.generateDo(first),_possibleConstructorReturn(_this76,_ret3);}if(op==='new'){if((firstCall=first.unwrap())instanceof Call&&!firstCall.do&&!firstCall.isNew){var _ret4;return _ret4=firstCall.newInstance(),_possibleConstructorReturn(_this76,_ret4);}if(first instanceof Code&&first.bound||first.do){first=new Parens(first);}}_this76.operator=CONVERSIONS[op]||op;_this76.first=first;_this76.second=second;_this76.flip=!!flip;return _ret5=_this76,_possibleConstructorReturn(_this76,_ret5);}_createClass(Op,[{key:'isNumber',value:function isNumber(){var ref1;return this.isUnary()&&((ref1=this.operator)==='+'||ref1==='-')&&this.first instanceof Value&&this.first.isNumber();}},{key:'isAwait',value:function isAwait(){return this.operator==='await';}},{key:'isYield',value:function isYield(){var ref1;return(ref1=this.operator)==='yield'||ref1==='yield*';}},{key:'isUnary',value:function isUnary(){return!this.second;}},{key:'shouldCache',value:function shouldCache(){return!this.isNumber();}// Am I capable of
// [Python-style comparison chaining](https://docs.python.org/3/reference/expressions.html#not-in)?
},{key:'isChainable',value:function isChainable(){var ref1;return(ref1=this.operator)==='<'||ref1==='>'||ref1==='>='||ref1==='<='||ref1==='==='||ref1==='!==';}},{key:'invert',value:function invert(){var allInvertable,curr,fst,op,ref1;if(this.isChainable()&&this.first.isChainable()){allInvertable=true;curr=this;while(curr&&curr.operator){allInvertable&&(allInvertable=curr.operator in INVERSIONS);curr=curr.first;}if(!allInvertable){return new Parens(this).invert();}curr=this;while(curr&&curr.operator){curr.invert=!curr.invert;curr.operator=INVERSIONS[curr.operator];curr=curr.first;}return this;}else if(op=INVERSIONS[this.operator]){this.operator=op;if(this.first.unwrap()instanceof Op){this.first.invert();}return this;}else if(this.second){return new Parens(this).invert();}else if(this.operator==='!'&&(fst=this.first.unwrap())instanceof Op&&((ref1=fst.operator)==='!'||ref1==='in'||ref1==='instanceof')){return fst;}else{return new Op('!',this);}}},{key:'unfoldSoak',value:function unfoldSoak(o){var ref1;return((ref1=this.operator)==='++'||ref1==='--'||ref1==='delete')&&_unfoldSoak(o,this,'first');}},{key:'generateDo',value:function generateDo(exp){var call,func,j,len1,param,passedParams,ref,ref1;passedParams=[];func=exp instanceof Assign&&(ref=exp.value.unwrap())instanceof Code?ref:exp;ref1=func.params||[];for(j=0,len1=ref1.length;j<len1;j++){param=ref1[j];if(param.value){passedParams.push(param.value);delete param.value;}else{passedParams.push(param);}}call=new Call(exp,passedParams);call.do=true;return call;}},{key:'compileNode',value:function compileNode(o){var answer,isChain,lhs,message,ref1,rhs;isChain=this.isChainable()&&this.first.isChainable();if(!isChain){// In chains, there's no need to wrap bare obj literals in parens,
// as the chained expression is wrapped.
this.first.front=this.front;}if(this.operator==='delete'&&o.scope.check(this.first.unwrapAll().value)){this.error('delete operand may not be argument or var');}if((ref1=this.operator)==='--'||ref1==='++'){message=isUnassignable(this.first.unwrapAll().value);if(message){this.first.error(message);}}if(this.isYield()||this.isAwait()){return this.compileContinuation(o);}if(this.isUnary()){return this.compileUnary(o);}if(isChain){return this.compileChain(o);}switch(this.operator){case'?':return this.compileExistence(o,this.second.isDefaultValue);case'//':return this.compileFloorDivision(o);case'%%':return this.compileModulo(o);default:lhs=this.first.compileToFragments(o,LEVEL_OP);rhs=this.second.compileToFragments(o,LEVEL_OP);answer=[].concat(lhs,this.makeCode(' '+this.operator+' '),rhs);if(o.level<=LEVEL_OP){return answer;}else{return this.wrapInParentheses(answer);}}}// Mimic Python's chained comparisons when multiple comparison operators are
// used sequentially. For example:
//     bin/coffee -e 'console.log 50 < 65 > 10'
//     true
},{key:'compileChain',value:function compileChain(o){var fragments,fst,shared;var _first$second$cache=this.first.second.cache(o);var _first$second$cache2=_slicedToArray(_first$second$cache,2);this.first.second=_first$second$cache2[0];shared=_first$second$cache2[1];fst=this.first.compileToFragments(o,LEVEL_OP);fragments=fst.concat(this.makeCode(' '+(this.invert?'&&':'||')+' '),shared.compileToFragments(o),this.makeCode(' '+this.operator+' '),this.second.compileToFragments(o,LEVEL_OP));return this.wrapInParentheses(fragments);}// Keep reference to the left expression, unless this an existential assignment
},{key:'compileExistence',value:function compileExistence(o,checkOnlyUndefined){var fst,ref;if(this.first.shouldCache()){ref=new IdentifierLiteral(o.scope.freeVariable('ref'));fst=new Parens(new Assign(ref,this.first));}else{fst=this.first;ref=fst;}return new If(new Existence(fst,checkOnlyUndefined),ref,{type:'if'}).addElse(this.second).compileToFragments(o);}// Compile a unary **Op**.
},{key:'compileUnary',value:function compileUnary(o){var op,parts,plusMinus;parts=[];op=this.operator;parts.push([this.makeCode(op)]);if(op==='!'&&this.first instanceof Existence){this.first.negated=!this.first.negated;return this.first.compileToFragments(o);}if(o.level>=LEVEL_ACCESS){return new Parens(this).compileToFragments(o);}plusMinus=op==='+'||op==='-';if(op==='new'||op==='typeof'||op==='delete'||plusMinus&&this.first instanceof Op&&this.first.operator===op){parts.push([this.makeCode(' ')]);}if(plusMinus&&this.first instanceof Op||op==='new'&&this.first.isStatement(o)){this.first=new Parens(this.first);}parts.push(this.first.compileToFragments(o,LEVEL_OP));if(this.flip){parts.reverse();}return this.joinFragmentArrays(parts,'');}},{key:'compileContinuation',value:function compileContinuation(o){var op,parts,ref1,ref2;parts=[];op=this.operator;if(o.scope.parent==null){this.error(this.operator+' can only occur inside functions');}if(((ref1=o.scope.method)!=null?ref1.bound:void 0)&&o.scope.method.isGenerator){this.error('yield cannot occur inside bound (fat arrow) functions');}if(indexOf.call(Object.keys(this.first),'expression')>=0&&!(this.first instanceof Throw)){if(this.first.expression!=null){parts.push(this.first.expression.compileToFragments(o,LEVEL_OP));}}else{if(o.level>=LEVEL_PAREN){parts.push([this.makeCode("(")]);}parts.push([this.makeCode(op)]);if(((ref2=this.first.base)!=null?ref2.value:void 0)!==''){parts.push([this.makeCode(" ")]);}parts.push(this.first.compileToFragments(o,LEVEL_OP));if(o.level>=LEVEL_PAREN){parts.push([this.makeCode(")")]);}}return this.joinFragmentArrays(parts,'');}},{key:'compileFloorDivision',value:function compileFloorDivision(o){var div,floor,second;floor=new Value(new IdentifierLiteral('Math'),[new Access(new PropertyName('floor'))]);second=this.second.shouldCache()?new Parens(this.second):this.second;div=new Op('/',this.first,second);return new Call(floor,[div]).compileToFragments(o);}},{key:'compileModulo',value:function compileModulo(o){var mod;mod=new Value(new Literal(utility('modulo',o)));return new Call(mod,[this.first,this.second]).compileToFragments(o);}},{key:'toString',value:function toString(idt){return _get(Op.prototype.__proto__||Object.getPrototypeOf(Op.prototype),'toString',this).call(this,idt,this.constructor.name+' '+this.operator);}}]);return Op;}(Base);;// The map of conversions from CoffeeScript to JavaScript symbols.
CONVERSIONS={'==':'===','!=':'!==','of':'in','yieldfrom':'yield*'};// The map of invertible operators.
INVERSIONS={'!==':'===','===':'!=='};Op.prototype.children=['first','second'];return Op;}.call(this);//### In
exports.In=In=function(){var In=function(_Base32){_inherits(In,_Base32);function In(object,array){_classCallCheck(this,In);var _this77=_possibleConstructorReturn(this,(In.__proto__||Object.getPrototypeOf(In)).call(this));_this77.object=object;_this77.array=array;return _this77;}_createClass(In,[{key:'compileNode',value:function compileNode(o){var hasSplat,j,len1,obj,ref1;if(this.array instanceof Value&&this.array.isArray()&&this.array.base.objects.length){ref1=this.array.base.objects;for(j=0,len1=ref1.length;j<len1;j++){obj=ref1[j];if(!(obj instanceof Splat)){continue;}hasSplat=true;break;}if(!hasSplat){// `compileOrTest` only if we have an array literal with no splats
return this.compileOrTest(o);}}return this.compileLoopTest(o);}},{key:'compileOrTest',value:function compileOrTest(o){var cmp,cnj,i,item,j,len1,ref,ref1,sub,tests;var _object$cache=this.object.cache(o,LEVEL_OP);var _object$cache2=_slicedToArray(_object$cache,2);sub=_object$cache2[0];ref=_object$cache2[1];var _ref16=this.negated?[' !== ',' && ']:[' === ',' || '];var _ref17=_slicedToArray(_ref16,2);cmp=_ref17[0];cnj=_ref17[1];tests=[];ref1=this.array.base.objects;for(i=j=0,len1=ref1.length;j<len1;i=++j){item=ref1[i];if(i){tests.push(this.makeCode(cnj));}tests=tests.concat(i?ref:sub,this.makeCode(cmp),item.compileToFragments(o,LEVEL_ACCESS));}if(o.level<LEVEL_OP){return tests;}else{return this.wrapInParentheses(tests);}}},{key:'compileLoopTest',value:function compileLoopTest(o){var fragments,ref,sub;var _object$cache3=this.object.cache(o,LEVEL_LIST);var _object$cache4=_slicedToArray(_object$cache3,2);sub=_object$cache4[0];ref=_object$cache4[1];fragments=[].concat(this.makeCode(utility('indexOf',o)+".call("),this.array.compileToFragments(o,LEVEL_LIST),this.makeCode(", "),ref,this.makeCode(") "+(this.negated?'< 0':'>= 0')));if(fragmentsToText(sub)===fragmentsToText(ref)){return fragments;}fragments=sub.concat(this.makeCode(', '),fragments);if(o.level<LEVEL_LIST){return fragments;}else{return this.wrapInParentheses(fragments);}}},{key:'toString',value:function toString(idt){return _get(In.prototype.__proto__||Object.getPrototypeOf(In.prototype),'toString',this).call(this,idt,this.constructor.name+(this.negated?'!':''));}}]);return In;}(Base);;In.prototype.children=['object','array'];In.prototype.invert=NEGATE;return In;}.call(this);//### Try
// A classic *try/catch/finally* block.
exports.Try=Try=function(){var Try=function(_Base33){_inherits(Try,_Base33);function Try(attempt,errorVariable,recovery,ensure){_classCallCheck(this,Try);var _this78=_possibleConstructorReturn(this,(Try.__proto__||Object.getPrototypeOf(Try)).call(this));_this78.attempt=attempt;_this78.errorVariable=errorVariable;_this78.recovery=recovery;_this78.ensure=ensure;return _this78;}_createClass(Try,[{key:'jumps',value:function jumps(o){var ref1;return this.attempt.jumps(o)||((ref1=this.recovery)!=null?ref1.jumps(o):void 0);}},{key:'makeReturn',value:function makeReturn(res){if(this.attempt){this.attempt=this.attempt.makeReturn(res);}if(this.recovery){this.recovery=this.recovery.makeReturn(res);}return this;}// Compilation is more or less as you would expect -- the *finally* clause
// is optional, the *catch* is not.
},{key:'compileNode',value:function compileNode(o){var catchPart,ensurePart,generatedErrorVariableName,message,placeholder,tryPart;o.indent+=TAB;tryPart=this.attempt.compileToFragments(o,LEVEL_TOP);catchPart=this.recovery?(generatedErrorVariableName=o.scope.freeVariable('error',{reserve:false}),placeholder=new IdentifierLiteral(generatedErrorVariableName),this.errorVariable?(message=isUnassignable(this.errorVariable.unwrapAll().value),message?this.errorVariable.error(message):void 0,this.recovery.unshift(new Assign(this.errorVariable,placeholder))):void 0,[].concat(this.makeCode(" catch ("),placeholder.compileToFragments(o),this.makeCode(") {\n"),this.recovery.compileToFragments(o,LEVEL_TOP),this.makeCode('\n'+this.tab+'}'))):!(this.ensure||this.recovery)?(generatedErrorVariableName=o.scope.freeVariable('error',{reserve:false}),[this.makeCode(' catch ('+generatedErrorVariableName+') {}')]):[];ensurePart=this.ensure?[].concat(this.makeCode(" finally {\n"),this.ensure.compileToFragments(o,LEVEL_TOP),this.makeCode('\n'+this.tab+'}')):[];return[].concat(this.makeCode(this.tab+'try {\n'),tryPart,this.makeCode('\n'+this.tab+'}'),catchPart,ensurePart);}}]);return Try;}(Base);;Try.prototype.children=['attempt','recovery','ensure'];Try.prototype.isStatement=YES;return Try;}.call(this);//### Throw
// Simple node to throw an exception.
exports.Throw=Throw=function(){var Throw=function(_Base34){_inherits(Throw,_Base34);function Throw(expression1){_classCallCheck(this,Throw);var _this79=_possibleConstructorReturn(this,(Throw.__proto__||Object.getPrototypeOf(Throw)).call(this));_this79.expression=expression1;return _this79;}_createClass(Throw,[{key:'compileNode',value:function compileNode(o){var fragments;fragments=this.expression.compileToFragments(o,LEVEL_LIST);unshiftAfterComments(fragments,this.makeCode('throw '));fragments.unshift(this.makeCode(this.tab));fragments.push(this.makeCode(';'));return fragments;}}]);return Throw;}(Base);;Throw.prototype.children=['expression'];Throw.prototype.isStatement=YES;Throw.prototype.jumps=NO;// A **Throw** is already a return, of sorts...
Throw.prototype.makeReturn=THIS;return Throw;}.call(this);//### Existence
// Checks a variable for existence -- not `null` and not `undefined`. This is
// similar to `.nil?` in Ruby, and avoids having to consult a JavaScript truth
// table. Optionally only check if a variable is not `undefined`.
exports.Existence=Existence=function(){var Existence=function(_Base35){_inherits(Existence,_Base35);function Existence(expression1){var onlyNotUndefined=arguments.length>1&&arguments[1]!==undefined?arguments[1]:false;_classCallCheck(this,Existence);var salvagedComments;var _this80=_possibleConstructorReturn(this,(Existence.__proto__||Object.getPrototypeOf(Existence)).call(this));_this80.expression=expression1;_this80.comparisonTarget=onlyNotUndefined?'undefined':'null';salvagedComments=[];_this80.expression.traverseChildren(true,function(child){var comment,j,len1,ref1;if(child.comments){ref1=child.comments;for(j=0,len1=ref1.length;j<len1;j++){comment=ref1[j];if(indexOf.call(salvagedComments,comment)<0){salvagedComments.push(comment);}}return delete child.comments;}});attachCommentsToNode(salvagedComments,_this80);moveComments(_this80.expression,_this80);return _this80;}_createClass(Existence,[{key:'compileNode',value:function compileNode(o){var cmp,cnj,code;this.expression.front=this.front;code=this.expression.compile(o,LEVEL_OP);if(this.expression.unwrap()instanceof IdentifierLiteral&&!o.scope.check(code)){var _ref18=this.negated?['===','||']:['!==','&&'];var _ref19=_slicedToArray(_ref18,2);cmp=_ref19[0];cnj=_ref19[1];code='typeof '+code+' '+cmp+' "undefined"'+(this.comparisonTarget!=='undefined'?' '+cnj+' '+code+' '+cmp+' '+this.comparisonTarget:'');}else{// We explicity want to use loose equality (`==`) when comparing against `null`,
// so that an existence check roughly corresponds to a check for truthiness.
// Do *not* change this to `===` for `null`, as this will break mountains of
// existing code. When comparing only against `undefined`, however, we want to
// use `===` because this use case is for parity with ES2015+ default values,
// which only get assigned when the variable is `undefined` (but not `null`).
cmp=this.comparisonTarget==='null'?this.negated?'==':'!=':this.negated?'===':'!==';// `undefined`
code=code+' '+cmp+' '+this.comparisonTarget;}return[this.makeCode(o.level<=LEVEL_COND?code:'('+code+')')];}}]);return Existence;}(Base);;Existence.prototype.children=['expression'];Existence.prototype.invert=NEGATE;return Existence;}.call(this);//### Parens
// An extra set of parentheses, specified explicitly in the source. At one time
// we tried to clean up the results by detecting and removing redundant
// parentheses, but no longer -- you can put in as many as you please.
// Parentheses are a good way to force any statement to become an expression.
exports.Parens=Parens=function(){var Parens=function(_Base36){_inherits(Parens,_Base36);function Parens(body1){_classCallCheck(this,Parens);var _this81=_possibleConstructorReturn(this,(Parens.__proto__||Object.getPrototypeOf(Parens)).call(this));_this81.body=body1;return _this81;}_createClass(Parens,[{key:'unwrap',value:function unwrap(){return this.body;}},{key:'shouldCache',value:function shouldCache(){return this.body.shouldCache();}},{key:'compileNode',value:function compileNode(o){var bare,expr,fragments,ref1,shouldWrapComment;expr=this.body.unwrap();// If these parentheses are wrapping an `IdentifierLiteral` followed by a
// block comment, output the parentheses (or put another way, don’t optimize
// away these redundant parentheses). This is because Flow requires
// parentheses in certain circumstances to distinguish identifiers followed
// by comment-based type annotations from JavaScript labels.
shouldWrapComment=(ref1=expr.comments)!=null?ref1.some(function(comment){return comment.here&&!comment.unshift&&!comment.newLine;}):void 0;if(expr instanceof Value&&expr.isAtomic()&&!this.csxAttribute&&!shouldWrapComment){expr.front=this.front;return expr.compileToFragments(o);}fragments=expr.compileToFragments(o,LEVEL_PAREN);bare=o.level<LEVEL_OP&&!shouldWrapComment&&(expr instanceof Op||expr.unwrap()instanceof Call||expr instanceof For&&expr.returns)&&(o.level<LEVEL_COND||fragments.length<=3);if(this.csxAttribute){return this.wrapInBraces(fragments);}if(bare){return fragments;}else{return this.wrapInParentheses(fragments);}}}]);return Parens;}(Base);;Parens.prototype.children=['body'];return Parens;}.call(this);//### StringWithInterpolations
exports.StringWithInterpolations=StringWithInterpolations=function(){var StringWithInterpolations=function(_Base37){_inherits(StringWithInterpolations,_Base37);function StringWithInterpolations(body1){_classCallCheck(this,StringWithInterpolations);var _this82=_possibleConstructorReturn(this,(StringWithInterpolations.__proto__||Object.getPrototypeOf(StringWithInterpolations)).call(this));_this82.body=body1;return _this82;}// `unwrap` returns `this` to stop ancestor nodes reaching in to grab @body,
// and using @body.compileNode. `StringWithInterpolations.compileNode` is
// _the_ custom logic to output interpolated strings as code.
_createClass(StringWithInterpolations,[{key:'unwrap',value:function unwrap(){return this;}},{key:'shouldCache',value:function shouldCache(){return this.body.shouldCache();}},{key:'compileNode',value:function compileNode(o){var code,element,elements,expr,fragments,j,len1,salvagedComments,wrapped;if(this.csxAttribute){wrapped=new Parens(new StringWithInterpolations(this.body));wrapped.csxAttribute=true;return wrapped.compileNode(o);}// Assumes that `expr` is `Value` » `StringLiteral` or `Op`
expr=this.body.unwrap();elements=[];salvagedComments=[];expr.traverseChildren(false,function(node){var comment,j,k,len1,len2,ref1;if(node instanceof StringLiteral){if(node.comments){var _salvagedComments;(_salvagedComments=salvagedComments).push.apply(_salvagedComments,_toConsumableArray(node.comments));delete node.comments;}elements.push(node);return true;}else if(node instanceof Parens){if(salvagedComments.length!==0){for(j=0,len1=salvagedComments.length;j<len1;j++){comment=salvagedComments[j];comment.unshift=true;comment.newLine=true;}attachCommentsToNode(salvagedComments,node);}elements.push(node);return false;}else if(node.comments){// This node is getting discarded, but salvage its comments.
if(elements.length!==0&&!(elements[elements.length-1]instanceof StringLiteral)){ref1=node.comments;for(k=0,len2=ref1.length;k<len2;k++){comment=ref1[k];comment.unshift=false;comment.newLine=true;}attachCommentsToNode(node.comments,elements[elements.length-1]);}else{var _salvagedComments2;(_salvagedComments2=salvagedComments).push.apply(_salvagedComments2,_toConsumableArray(node.comments));}delete node.comments;}return true;});fragments=[];if(!this.csx){fragments.push(this.makeCode('`'));}for(j=0,len1=elements.length;j<len1;j++){element=elements[j];if(element instanceof StringLiteral){var _fragments9;element.value=element.unquote(true,this.csx);if(!this.csx){// Backticks and `${` inside template literals must be escaped.
element.value=element.value.replace(/(\\*)(`|\$\{)/g,function(match,backslashes,toBeEscaped){if(backslashes.length%2===0){return backslashes+'\\'+toBeEscaped;}else{return match;}});}(_fragments9=fragments).push.apply(_fragments9,_toConsumableArray(element.compileToFragments(o)));}else{var _fragments10;if(!this.csx){fragments.push(this.makeCode('$'));}code=element.compileToFragments(o,LEVEL_PAREN);if(!this.isNestedTag(element)||code.some(function(fragment){return fragment.comments!=null;})){code=this.wrapInBraces(code);// Flag the `{` and `}` fragments as having been generated by this
// `StringWithInterpolations` node, so that `compileComments` knows
// to treat them as bounds. Don’t trust `fragment.type`, which can
// report minified variable names when this compiler is minified.
code[0].isStringWithInterpolations=true;code[code.length-1].isStringWithInterpolations=true;}(_fragments10=fragments).push.apply(_fragments10,_toConsumableArray(code));}}if(!this.csx){fragments.push(this.makeCode('`'));}return fragments;}},{key:'isNestedTag',value:function isNestedTag(element){var call,exprs,ref1;exprs=(ref1=element.body)!=null?ref1.expressions:void 0;call=exprs!=null?exprs[0].unwrap():void 0;return this.csx&&exprs&&exprs.length===1&&call instanceof Call&&call.csx;}}]);return StringWithInterpolations;}(Base);;StringWithInterpolations.prototype.children=['body'];return StringWithInterpolations;}.call(this);//### For
// CoffeeScript's replacement for the *for* loop is our array and object
// comprehensions, that compile into *for* loops here. They also act as an
// expression, able to return the result of each filtered iteration.
// Unlike Python array comprehensions, they can be multi-line, and you can pass
// the current index of the loop as a second parameter. Unlike Ruby blocks,
// you can map and filter in a single pass.
exports.For=For=function(){var For=function(_While){_inherits(For,_While);function For(body,source){_classCallCheck(this,For);var _this83=_possibleConstructorReturn(this,(For.__proto__||Object.getPrototypeOf(For)).call(this));_this83.addBody(body);_this83.addSource(source);return _this83;}_createClass(For,[{key:'isAwait',value:function isAwait(){var ref1;return(ref1=this.await)!=null?ref1:false;}},{key:'addBody',value:function addBody(body){this.body=Block.wrap([body]);return this;}},{key:'addSource',value:function addSource(source){var _this84=this;var attr,attribs,attribute,j,k,len1,len2,ref1,ref2,ref3,ref4;var _source$source=source.source;this.source=_source$source===undefined?false:_source$source;attribs=["name","index","guard","step","own","ownTag","await","awaitTag","object","from"];for(j=0,len1=attribs.length;j<len1;j++){attr=attribs[j];this[attr]=(ref1=source[attr])!=null?ref1:this[attr];}if(!this.source){return this;}if(this.from&&this.index){this.index.error('cannot use index with for-from');}if(this.own&&!this.object){this.ownTag.error('cannot use own with for-'+(this.from?'from':'in'));}if(this.object){var _ref20=[this.index,this.name];this.name=_ref20[0];this.index=_ref20[1];}if(((ref2=this.index)!=null?typeof ref2.isArray==="function"?ref2.isArray():void 0:void 0)||((ref3=this.index)!=null?typeof ref3.isObject==="function"?ref3.isObject():void 0:void 0)){this.index.error('index cannot be a pattern matching expression');}if(this.await&&!this.from){this.awaitTag.error('await must be used with for-from');}this.range=this.source instanceof Value&&this.source.base instanceof Range&&!this.source.properties.length&&!this.from;this.pattern=this.name instanceof Value;if(this.range&&this.index){this.index.error('indexes do not apply to range loops');}if(this.range&&this.pattern){this.name.error('cannot pattern match over range loops');}this.returns=false;ref4=['source','guard','step','name','index'];// Move up any comments in the “`for` line”, i.e. the line of code with `for`,
// from any child nodes of that line up to the `for` node itself so that these
// comments get output, and get output above the `for` loop.
for(k=0,len2=ref4.length;k<len2;k++){attribute=ref4[k];if(!this[attribute]){continue;}this[attribute].traverseChildren(true,function(node){var comment,l,len3,ref5;if(node.comments){ref5=node.comments;for(l=0,len3=ref5.length;l<len3;l++){comment=ref5[l];// These comments are buried pretty deeply, so if they happen to be
// trailing comments the line they trail will be unrecognizable when
// we’re done compiling this `for` loop; so just shift them up to
// output above the `for` line.
comment.newLine=comment.unshift=true;}return moveComments(node,_this84[attribute]);}});moveComments(this[attribute],this);}return this;}// Welcome to the hairiest method in all of CoffeeScript. Handles the inner
// loop, filtering, stepping, and result saving for array, object, and range
// comprehensions. Some of the generated code can be shared in common, and
// some cannot.
},{key:'compileNode',value:function compileNode(o){var _slice1$call11,_slice1$call12;var body,bodyFragments,compare,compareDown,declare,declareDown,defPart,down,forClose,forCode,forPartFragments,fragments,guardPart,idt1,increment,index,ivar,kvar,kvarAssign,last,lvar,name,namePart,ref,ref1,resultPart,returnResult,rvar,scope,source,step,stepNum,stepVar,svar,varPart;body=Block.wrap([this.body]);ref1=body.expressions,(_slice1$call11=slice1.call(ref1,-1),_slice1$call12=_slicedToArray(_slice1$call11,1),last=_slice1$call12[0],_slice1$call11);if((last!=null?last.jumps():void 0)instanceof Return){this.returns=false;}source=this.range?this.source.base:this.source;scope=o.scope;if(!this.pattern){name=this.name&&this.name.compile(o,LEVEL_LIST);}index=this.index&&this.index.compile(o,LEVEL_LIST);if(name&&!this.pattern){scope.find(name);}if(index&&!(this.index instanceof Value)){scope.find(index);}if(this.returns){rvar=scope.freeVariable('results');}if(this.from){if(this.pattern){ivar=scope.freeVariable('x',{single:true});}}else{ivar=this.object&&index||scope.freeVariable('i',{single:true});}kvar=(this.range||this.from)&&name||index||ivar;kvarAssign=kvar!==ivar?kvar+' = ':"";if(this.step&&!this.range){var _cacheToCodeFragments9=this.cacheToCodeFragments(this.step.cache(o,LEVEL_LIST,shouldCacheOrIsAssignable));var _cacheToCodeFragments10=_slicedToArray(_cacheToCodeFragments9,2);step=_cacheToCodeFragments10[0];stepVar=_cacheToCodeFragments10[1];if(this.step.isNumber()){stepNum=Number(stepVar);}}if(this.pattern){name=ivar;}varPart='';guardPart='';defPart='';idt1=this.tab+TAB;if(this.range){forPartFragments=source.compileToFragments(merge(o,{index:ivar,name:name,step:this.step,shouldCache:shouldCacheOrIsAssignable}));}else{svar=this.source.compile(o,LEVEL_LIST);if((name||this.own)&&!(this.source.unwrap()instanceof IdentifierLiteral)){defPart+=''+this.tab+(ref=scope.freeVariable('ref'))+' = '+svar+';\n';svar=ref;}if(name&&!this.pattern&&!this.from){namePart=name+' = '+svar+'['+kvar+']';}if(!this.object&&!this.from){if(step!==stepVar){defPart+=''+this.tab+step+';\n';}down=stepNum<0;if(!(this.step&&stepNum!=null&&down)){lvar=scope.freeVariable('len');}declare=''+kvarAssign+ivar+' = 0, '+lvar+' = '+svar+'.length';declareDown=''+kvarAssign+ivar+' = '+svar+'.length - 1';compare=ivar+' < '+lvar;compareDown=ivar+' >= 0';if(this.step){if(stepNum!=null){if(down){compare=compareDown;declare=declareDown;}}else{compare=stepVar+' > 0 ? '+compare+' : '+compareDown;declare='('+stepVar+' > 0 ? ('+declare+') : '+declareDown+')';}increment=ivar+' += '+stepVar;}else{increment=''+(kvar!==ivar?'++'+ivar:ivar+'++');}forPartFragments=[this.makeCode(declare+'; '+compare+'; '+kvarAssign+increment)];}}if(this.returns){resultPart=''+this.tab+rvar+' = [];\n';returnResult='\n'+this.tab+'return '+rvar+';';body.makeReturn(rvar);}if(this.guard){if(body.expressions.length>1){body.expressions.unshift(new If(new Parens(this.guard).invert(),new StatementLiteral("continue")));}else{if(this.guard){body=Block.wrap([new If(this.guard,body)]);}}}if(this.pattern){body.expressions.unshift(new Assign(this.name,this.from?new IdentifierLiteral(kvar):new Literal(svar+'['+kvar+']')));}if(namePart){varPart='\n'+idt1+namePart+';';}if(this.object){forPartFragments=[this.makeCode(kvar+' in '+svar)];if(this.own){guardPart='\n'+idt1+'if (!'+utility('hasProp',o)+'.call('+svar+', '+kvar+')) continue;';}}else if(this.from){if(this.await){forPartFragments=new Op('await',new Parens(new Literal(kvar+' of '+svar)));forPartFragments=forPartFragments.compileToFragments(o,LEVEL_TOP);}else{forPartFragments=[this.makeCode(kvar+' of '+svar)];}}bodyFragments=body.compileToFragments(merge(o,{indent:idt1}),LEVEL_TOP);if(bodyFragments&&bodyFragments.length>0){bodyFragments=[].concat(this.makeCode('\n'),bodyFragments,this.makeCode('\n'));}fragments=[this.makeCode(defPart)];if(resultPart){fragments.push(this.makeCode(resultPart));}forCode=this.await?'for ':'for (';forClose=this.await?'':')';fragments=fragments.concat(this.makeCode(this.tab),this.makeCode(forCode),forPartFragments,this.makeCode(forClose+' {'+guardPart+varPart),bodyFragments,this.makeCode(this.tab),this.makeCode('}'));if(returnResult){fragments.push(this.makeCode(returnResult));}return fragments;}}]);return For;}(While);;For.prototype.children=['body','source','guard','step'];return For;}.call(this);//### Switch
// A JavaScript *switch* statement. Converts into a returnable expression on-demand.
exports.Switch=Switch=function(){var Switch=function(_Base38){_inherits(Switch,_Base38);function Switch(subject,cases,otherwise){_classCallCheck(this,Switch);var _this85=_possibleConstructorReturn(this,(Switch.__proto__||Object.getPrototypeOf(Switch)).call(this));_this85.subject=subject;_this85.cases=cases;_this85.otherwise=otherwise;return _this85;}_createClass(Switch,[{key:'jumps',value:function jumps(){var o=arguments.length>0&&arguments[0]!==undefined?arguments[0]:{block:true};var block,conds,j,jumpNode,len1,ref1,ref2;ref1=this.cases;for(j=0,len1=ref1.length;j<len1;j++){var _ref1$j=_slicedToArray(ref1[j],2);conds=_ref1$j[0];block=_ref1$j[1];if(jumpNode=block.jumps(o)){return jumpNode;}}return(ref2=this.otherwise)!=null?ref2.jumps(o):void 0;}},{key:'makeReturn',value:function makeReturn(res){var j,len1,pair,ref1,ref2;ref1=this.cases;for(j=0,len1=ref1.length;j<len1;j++){pair=ref1[j];pair[1].makeReturn(res);}if(res){this.otherwise||(this.otherwise=new Block([new Literal('void 0')]));}if((ref2=this.otherwise)!=null){ref2.makeReturn(res);}return this;}},{key:'compileNode',value:function compileNode(o){var block,body,cond,conditions,expr,fragments,i,idt1,idt2,j,k,len1,len2,ref1,ref2;idt1=o.indent+TAB;idt2=o.indent=idt1+TAB;fragments=[].concat(this.makeCode(this.tab+"switch ("),this.subject?this.subject.compileToFragments(o,LEVEL_PAREN):this.makeCode("false"),this.makeCode(") {\n"));ref1=this.cases;for(i=j=0,len1=ref1.length;j<len1;i=++j){var _ref1$i=_slicedToArray(ref1[i],2);conditions=_ref1$i[0];block=_ref1$i[1];ref2=flatten([conditions]);for(k=0,len2=ref2.length;k<len2;k++){cond=ref2[k];if(!this.subject){cond=cond.invert();}fragments=fragments.concat(this.makeCode(idt1+"case "),cond.compileToFragments(o,LEVEL_PAREN),this.makeCode(":\n"));}if((body=block.compileToFragments(o,LEVEL_TOP)).length>0){fragments=fragments.concat(body,this.makeCode('\n'));}if(i===this.cases.length-1&&!this.otherwise){break;}expr=this.lastNode(block.expressions);if(expr instanceof Return||expr instanceof Throw||expr instanceof Literal&&expr.jumps()&&expr.value!=='debugger'){continue;}fragments.push(cond.makeCode(idt2+'break;\n'));}if(this.otherwise&&this.otherwise.expressions.length){var _fragments11;(_fragments11=fragments).push.apply(_fragments11,[this.makeCode(idt1+"default:\n")].concat(_toConsumableArray(this.otherwise.compileToFragments(o,LEVEL_TOP)),[this.makeCode("\n")]));}fragments.push(this.makeCode(this.tab+'}'));return fragments;}}]);return Switch;}(Base);;Switch.prototype.children=['subject','cases','otherwise'];Switch.prototype.isStatement=YES;return Switch;}.call(this);//### If
// *If/else* statements. Acts as an expression by pushing down requested returns
// to the last line of each clause.
// Single-expression **Ifs** are compiled into conditional operators if possible,
// because ternaries are already proper expressions, and don’t need conversion.
exports.If=If=function(){var If=function(_Base39){_inherits(If,_Base39);function If(condition,body1){var options=arguments.length>2&&arguments[2]!==undefined?arguments[2]:{};_classCallCheck(this,If);var _this86=_possibleConstructorReturn(this,(If.__proto__||Object.getPrototypeOf(If)).call(this));_this86.body=body1;_this86.condition=options.type==='unless'?condition.invert():condition;_this86.elseBody=null;_this86.isChain=false;_this86.soak=options.soak;if(_this86.condition.comments){moveComments(_this86.condition,_this86);}return _this86;}_createClass(If,[{key:'bodyNode',value:function bodyNode(){var ref1;return(ref1=this.body)!=null?ref1.unwrap():void 0;}},{key:'elseBodyNode',value:function elseBodyNode(){var ref1;return(ref1=this.elseBody)!=null?ref1.unwrap():void 0;}// Rewrite a chain of **Ifs** to add a default case as the final *else*.
},{key:'addElse',value:function addElse(elseBody){if(this.isChain){this.elseBodyNode().addElse(elseBody);}else{this.isChain=elseBody instanceof If;this.elseBody=this.ensureBlock(elseBody);this.elseBody.updateLocationDataIfMissing(elseBody.locationData);}return this;}// The **If** only compiles into a statement if either of its bodies needs
// to be a statement. Otherwise a conditional operator is safe.
},{key:'isStatement',value:function isStatement(o){var ref1;return(o!=null?o.level:void 0)===LEVEL_TOP||this.bodyNode().isStatement(o)||((ref1=this.elseBodyNode())!=null?ref1.isStatement(o):void 0);}},{key:'jumps',value:function jumps(o){var ref1;return this.body.jumps(o)||((ref1=this.elseBody)!=null?ref1.jumps(o):void 0);}},{key:'compileNode',value:function compileNode(o){if(this.isStatement(o)){return this.compileStatement(o);}else{return this.compileExpression(o);}}},{key:'makeReturn',value:function makeReturn(res){if(res){this.elseBody||(this.elseBody=new Block([new Literal('void 0')]));}this.body&&(this.body=new Block([this.body.makeReturn(res)]));this.elseBody&&(this.elseBody=new Block([this.elseBody.makeReturn(res)]));return this;}},{key:'ensureBlock',value:function ensureBlock(node){if(node instanceof Block){return node;}else{return new Block([node]);}}// Compile the `If` as a regular *if-else* statement. Flattened chains
// force inner *else* bodies into statement form.
},{key:'compileStatement',value:function compileStatement(o){var answer,body,child,cond,exeq,ifPart,indent;child=del(o,'chainChild');exeq=del(o,'isExistentialEquals');if(exeq){return new If(this.condition.invert(),this.elseBodyNode(),{type:'if'}).compileToFragments(o);}indent=o.indent+TAB;cond=this.condition.compileToFragments(o,LEVEL_PAREN);body=this.ensureBlock(this.body).compileToFragments(merge(o,{indent:indent}));ifPart=[].concat(this.makeCode("if ("),cond,this.makeCode(") {\n"),body,this.makeCode('\n'+this.tab+'}'));if(!child){ifPart.unshift(this.makeCode(this.tab));}if(!this.elseBody){return ifPart;}answer=ifPart.concat(this.makeCode(' else '));if(this.isChain){o.chainChild=true;answer=answer.concat(this.elseBody.unwrap().compileToFragments(o,LEVEL_TOP));}else{answer=answer.concat(this.makeCode("{\n"),this.elseBody.compileToFragments(merge(o,{indent:indent}),LEVEL_TOP),this.makeCode('\n'+this.tab+'}'));}return answer;}// Compile the `If` as a conditional operator.
},{key:'compileExpression',value:function compileExpression(o){var alt,body,cond,fragments;cond=this.condition.compileToFragments(o,LEVEL_COND);body=this.bodyNode().compileToFragments(o,LEVEL_LIST);alt=this.elseBodyNode()?this.elseBodyNode().compileToFragments(o,LEVEL_LIST):[this.makeCode('void 0')];fragments=cond.concat(this.makeCode(" ? "),body,this.makeCode(" : "),alt);if(o.level>=LEVEL_COND){return this.wrapInParentheses(fragments);}else{return fragments;}}},{key:'unfoldSoak',value:function unfoldSoak(){return this.soak&&this;}}]);return If;}(Base);;If.prototype.children=['condition','body','elseBody'];return If;}.call(this);// Constants
// ---------
UTILITIES={modulo:function modulo(){return'function(a, b) { return (+a % (b = +b) + b) % b; }';},boundMethodCheck:function boundMethodCheck(){return"function(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new Error('Bound instance method accessed before binding'); } }";},// Shortcuts to speed up the lookup time for native functions.
hasProp:function hasProp(){return'{}.hasOwnProperty';},indexOf:function indexOf(){return'[].indexOf';},slice:function slice(){return'[].slice';},splice:function splice(){return'[].splice';}};// Levels indicate a node's position in the AST. Useful for knowing if
// parens are necessary or superfluous.
LEVEL_TOP=1;// ...;
LEVEL_PAREN=2;// (...)
LEVEL_LIST=3;// [...]
LEVEL_COND=4;// ... ? x : y
LEVEL_OP=5;// !...
LEVEL_ACCESS=6;// ...[0]
// Tabs are two spaces for pretty printing.
TAB='  ';SIMPLENUM=/^[+-]?\d+$/;// Helper Functions
// ----------------
// Helper for ensuring that utility functions are assigned at the top level.
utility=function utility(name,o){var ref,root;root=o.scope.root;if(name in root.utilities){return root.utilities[name];}else{ref=root.freeVariable(name);root.assign(ref,UTILITIES[name](o));return root.utilities[name]=ref;}};multident=function multident(code,tab){var includingFirstLine=arguments.length>2&&arguments[2]!==undefined?arguments[2]:true;var endsWithNewLine;endsWithNewLine=code[code.length-1]==='\n';code=(includingFirstLine?tab:'')+code.replace(/\n/g,'$&'+tab);code=code.replace(/\s+$/,'');if(endsWithNewLine){code=code+'\n';}return code;};// Wherever in CoffeeScript 1 we might’ve inserted a `makeCode "#{@tab}"` to
// indent a line of code, now we must account for the possibility of comments
// preceding that line of code. If there are such comments, indent each line of
// such comments, and _then_ indent the first following line of code.
indentInitial=function indentInitial(fragments,node){var fragment,fragmentIndex,j,len1;for(fragmentIndex=j=0,len1=fragments.length;j<len1;fragmentIndex=++j){fragment=fragments[fragmentIndex];if(fragment.isHereComment){fragment.code=multident(fragment.code,node.tab);}else{fragments.splice(fragmentIndex,0,node.makeCode(''+node.tab));break;}}return fragments;};hasLineComments=function hasLineComments(node){var comment,j,len1,ref1;if(!node.comments){return false;}ref1=node.comments;for(j=0,len1=ref1.length;j<len1;j++){comment=ref1[j];if(comment.here===false){return true;}}return false;};// Move the `comments` property from one object to another, deleting it from
// the first object.
moveComments=function moveComments(from,to){if(!(from!=null?from.comments:void 0)){return;}attachCommentsToNode(from.comments,to);return delete from.comments;};// Sometimes when compiling a node, we want to insert a fragment at the start
// of an array of fragments; but if the start has one or more comment fragments,
// we want to insert this fragment after those but before any non-comments.
unshiftAfterComments=function unshiftAfterComments(fragments,fragmentToInsert){var fragment,fragmentIndex,inserted,j,len1;inserted=false;for(fragmentIndex=j=0,len1=fragments.length;j<len1;fragmentIndex=++j){fragment=fragments[fragmentIndex];if(!!fragment.isComment){continue;}fragments.splice(fragmentIndex,0,fragmentToInsert);inserted=true;break;}if(!inserted){fragments.push(fragmentToInsert);}return fragments;};isLiteralArguments=function isLiteralArguments(node){return node instanceof IdentifierLiteral&&node.value==='arguments';};isLiteralThis=function isLiteralThis(node){return node instanceof ThisLiteral||node instanceof Code&&node.bound;};shouldCacheOrIsAssignable=function shouldCacheOrIsAssignable(node){return node.shouldCache()||(typeof node.isAssignable==="function"?node.isAssignable():void 0);};// Unfold a node's child if soak, then tuck the node under created `If`
_unfoldSoak=function _unfoldSoak(o,parent,name){var ifn;if(!(ifn=parent[name].unfoldSoak(o))){return;}parent[name]=ifn.body;ifn.body=new Value(parent);return ifn;};return exports;};//#endregion
//#region URL: /coffeescript
modules['/coffeescript']=function(){var exports={};// CoffeeScript can be used both on the server, as a command-line compiler based
// on Node.js/V8, or to run CoffeeScript directly in the browser. This module
// contains the main entry functions for tokenizing, parsing, and compiling
// source CoffeeScript into JavaScript.
var FILE_EXTENSIONS,Lexer,SourceMap,base64encode,checkShebangLine,compile,formatSourcePosition,getSourceMap,helpers,lexer,packageJson,parser,sourceMaps,sources,withPrettyErrors,indexOf=[].indexOf;var _require7=require('/lexer');Lexer=_require7.Lexer;var _require8=require('/parser');parser=_require8.parser;helpers=require('/helpers');/*BT-
		SourceMap = require('/sourcemap');

		// Require `package.json`, which is two levels above this file, as this file is
		// evaluated from `lib/coffeescript`.
		packageJson = require('../../package.json');
		*/// The current CoffeeScript version number.
exports.VERSION=/*BT- packageJson.version*/'2.3.0';/*BT-
		exports.FILE_EXTENSIONS = FILE_EXTENSIONS = ['.coffee', '.litcoffee', '.coffee.md'];
		*/// Expose helpers for testing.
exports.helpers=helpers;/*BT-
		// Function that allows for btoa in both nodejs and the browser.
		base64encode = function(src) {
			switch (false) {
				case typeof Buffer !== 'function':
					return Buffer.from(src).toString('base64');
				case typeof btoa !== 'function':
					// The contents of a `<script>` block are encoded via UTF-16, so if any extended
					// characters are used in the block, btoa will fail as it maxes out at UTF-8.
					// See https://developer.mozilla.org/en-US/docs/Web/API/WindowBase64/Base64_encoding_and_decoding#The_Unicode_Problem
					// for the gory details, and for the solution implemented here.
					return btoa(encodeURIComponent(src).replace(/%([0-9A-F]{2})/g, function(match, p1) {
						return String.fromCharCode('0x' + p1);
					}));
				default:
					throw new Error('Unable to base64 encode inline sourcemap.');
			}
		};
		*/// Function wrapper to add source file information to SyntaxErrors thrown by the
// lexer/parser/compiler.
withPrettyErrors=function withPrettyErrors(fn){return function(code){var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var err;try{return fn.call(this,code,options);}catch(error){err=error;if(typeof code!=='string'){// Support `CoffeeScript.nodes(tokens)`.
throw err;}throw helpers.updateSyntaxError(err,code,options.filename);}};};/*BT-
		// For each compiled file, save its source in memory in case we need to
		// recompile it later. We might need to recompile if the first compilation
		// didn’t create a source map (faster) but something went wrong and we need
		// a stack trace. Assuming that most of the time, code isn’t throwing
		// exceptions, it’s probably more efficient to compile twice only when we
		// need a stack trace, rather than always generating a source map even when
		// it’s not likely to be used. Save in form of `filename`: [`(source)`]
		sources = {};

		// Also save source maps if generated, in form of `(source)`: [`(source map)`].
		sourceMaps = {};
		*/// Compile CoffeeScript code to JavaScript, using the Coffee/Jison compiler.
// If `options.sourceMap` is specified, then `options.filename` must also be
// specified. All options that can be passed to `SourceMap#generate` may also
// be passed here.
// This returns a javascript string, unless `options.sourceMap` is passed,
// in which case this returns a `{js, v3SourceMap, sourceMap}`
// object, where sourceMap is a sourcemap.coffee#SourceMap object, handy for
// doing programmatic lookups.
exports.compile=compile=withPrettyErrors(function(code){var options=arguments.length>1&&arguments[1]!==undefined?arguments[1]:{};var currentColumn,currentLine,encoded,filename,fragment,fragments,generateSourceMap,header,i,j,js,len,len1,map,newLines,ref,ref1,sourceMapDataURI,sourceURL,token,tokens,transpiler,transpilerOptions,transpilerOutput,v3SourceMap;// Clone `options`, to avoid mutating the `options` object passed in.
options=Object.assign({},options);/*BT-
			// Always generate a source map if no filename is passed in, since without a
			// a filename we have no way to retrieve this source later in the event that
			// we need to recompile it to get a source map for `prepareStackTrace`.
			generateSourceMap = options.sourceMap || options.inlineMap || (options.filename == null);
			filename = options.filename || '<anonymous>';
			checkShebangLine(filename, code);
			if (sources[filename] == null) {
				sources[filename] = [];
			}
			sources[filename].push(code);
			if (generateSourceMap) {
				map = new SourceMap;
			}
			*/tokens=lexer.tokenize(code,options);// Pass a list of referenced variables, so that generated variables won’t get
// the same name.
options.referencedVars=function(){var i,len,results;results=[];for(i=0,len=tokens.length;i<len;i++){token=tokens[i];if(token[0]==='IDENTIFIER'){results.push(token[1]);}}return results;}();// Check for import or export; if found, force bare mode.
if(!(options.bare!=null&&options.bare===true)){for(i=0,len=tokens.length;i<len;i++){token=tokens[i];if((ref=token[0])==='IMPORT'||ref==='EXPORT'){options.bare=true;break;}}}fragments=parser.parse(tokens).compileToFragments(options);currentLine=0;/*BT-
			if (options.header) {
				currentLine += 1;
			}
			if (options.shiftLine) {
				currentLine += 1;
			}
			*/currentColumn=0;js="";for(j=0,len1=fragments.length;j<len1;j++){fragment=fragments[j];/*BT-
				// Update the sourcemap with data from each fragment.
				if (generateSourceMap) {
					// Do not include empty, whitespace, or semicolon-only fragments.
					if (fragment.locationData && !/^[;\s]*$/.test(fragment.code)) {
						map.add([fragment.locationData.first_line, fragment.locationData.first_column], [currentLine, currentColumn], {
							noReplace: true
						});
					}
					newLines = helpers.count(fragment.code, "\n");
					currentLine += newLines;
					if (newLines) {
						currentColumn = fragment.code.length - (fragment.code.lastIndexOf("\n") + 1);
					} else {
						currentColumn += fragment.code.length;
					}
				}
				*/// Copy the code from each fragment into the final JavaScript.
js+=fragment.code;}/*BT-
			if (options.header) {
				header = `Generated by CoffeeScript ${this.VERSION}`;
				js = `// ${header}\n${js}`;
			}
			if (generateSourceMap) {
				v3SourceMap = map.generate(options, code);
				if (sourceMaps[filename] == null) {
					sourceMaps[filename] = [];
				}
				sourceMaps[filename].push(map);
			}
			if (options.transpile) {
				if (typeof options.transpile !== 'object') {
					// This only happens if run via the Node API and `transpile` is set to
					// something other than an object.
					throw new Error('The transpile option must be given an object with options to pass to Babel');
				}
				// Get the reference to Babel that we have been passed if this compiler
				// is run via the CLI or Node API.
				transpiler = options.transpile.transpile;
				delete options.transpile.transpile;
				transpilerOptions = Object.assign({}, options.transpile);
				// See https://github.com/babel/babel/issues/827#issuecomment-77573107:
				// Babel can take a v3 source map object as input in `inputSourceMap`
				// and it will return an *updated* v3 source map object in its output.
				if (v3SourceMap && (transpilerOptions.inputSourceMap == null)) {
					transpilerOptions.inputSourceMap = v3SourceMap;
				}
				transpilerOutput = transpiler(js, transpilerOptions);
				js = transpilerOutput.code;
				if (v3SourceMap && transpilerOutput.map) {
					v3SourceMap = transpilerOutput.map;
				}
			}
			if (options.inlineMap) {
				encoded = base64encode(JSON.stringify(v3SourceMap));
				sourceMapDataURI = `//# sourceMappingURL=data:application/json;base64,${encoded}`;
				sourceURL = `//# sourceURL=${(ref1 = options.filename) != null ? ref1 : 'coffeescript'}`;
				js = `${js}\n${sourceMapDataURI}\n${sourceURL}`;
			}
			if (options.sourceMap) {
				return {
					js,
					sourceMap: map,
					v3SourceMap: JSON.stringify(v3SourceMap, null, 2)
				};
			} else {
			*/return js;/*BT-
			}
			*/});/*BT-
		// Tokenize a string of CoffeeScript code, and return the array of tokens.
		exports.tokens = withPrettyErrors(function(code, options) {
			return lexer.tokenize(code, options);
		});

		// Parse a string of CoffeeScript code or an array of lexed tokens, and
		// return the AST. You can then compile it by calling `.compile()` on the root,
		// or traverse it by using `.traverseChildren()` with a callback.
		exports.nodes = withPrettyErrors(function(source, options) {
			if (typeof source === 'string') {
				return parser.parse(lexer.tokenize(source, options));
			} else {
				return parser.parse(source);
			}
		});

		// This file used to export these methods; leave stubs that throw warnings
		// instead. These methods have been moved into `index.coffee` to provide
		// separate entrypoints for Node and non-Node environments, so that static
		// analysis tools don’t choke on Node packages when compiling for a non-Node
		// environment.
		exports.run = exports.eval = exports.register = function() {
			throw new Error('require index.coffee, not this file');
		};
		*/// Instantiate a Lexer for our use here.
lexer=new Lexer();// The real Lexer produces a generic stream of tokens. This object provides a
// thin wrapper around it, compatible with the Jison API. We can then pass it
// directly as a "Jison lexer".
parser.lexer={lex:function lex(){var tag,token;token=parser.tokens[this.pos++];if(token){var _token6=token;var _token7=_slicedToArray(_token6,3);tag=_token7[0];this.yytext=_token7[1];this.yylloc=_token7[2];parser.errorToken=token.origin||token;this.yylineno=this.yylloc.first_line;}else{tag='';}return tag;},setInput:function setInput(tokens){parser.tokens=tokens;return this.pos=0;},upcomingInput:function upcomingInput(){return'';}};// Make all the AST nodes visible to the parser.
parser.yy=require('/nodes');// Override Jison's default error handling function.
parser.yy.parseError=function(message,_ref21){var token=_ref21.token;var errorLoc,errorTag,errorText,errorToken,tokens;// Disregard Jison's message, it contains redundant line number information.
// Disregard the token, we take its value directly from the lexer in case
// the error is caused by a generated token which might refer to its origin.
var _parser=parser;errorToken=_parser.errorToken;tokens=_parser.tokens;var _errorToken=errorToken;var _errorToken2=_slicedToArray(_errorToken,3);errorTag=_errorToken2[0];errorText=_errorToken2[1];errorLoc=_errorToken2[2];errorText=function(){switch(false){case errorToken!==tokens[tokens.length-1]:return'end of input';case errorTag!=='INDENT'&&errorTag!=='OUTDENT':return'indentation';case errorTag!=='IDENTIFIER'&&errorTag!=='NUMBER'&&errorTag!=='INFINITY'&&errorTag!=='STRING'&&errorTag!=='STRING_START'&&errorTag!=='REGEX'&&errorTag!=='REGEX_START':return errorTag.replace(/_START$/,'').toLowerCase();default:return helpers.nameWhitespaceCharacter(errorText);}}();// The second argument has a `loc` property, which should have the location
// data for this token. Unfortunately, Jison seems to send an outdated `loc`
// (from the previous token), so we take the location information directly
// from the lexer.
return helpers.throwSyntaxError('unexpected '+errorText,errorLoc);};/*BT-
		// Based on http://v8.googlecode.com/svn/branches/bleeding_edge/src/messages.js
		// Modified to handle sourceMap
		formatSourcePosition = function(frame, getSourceMapping) {
			var as, column, fileLocation, filename, functionName, isConstructor, isMethodCall, line, methodName, source, tp, typeName;
			filename = void 0;
			fileLocation = '';
			if (frame.isNative()) {
				fileLocation = "native";
			} else {
				if (frame.isEval()) {
					filename = frame.getScriptNameOrSourceURL();
					if (!filename) {
						fileLocation = `${frame.getEvalOrigin()}, `;
					}
				} else {
					filename = frame.getFileName();
				}
				filename || (filename = "<anonymous>");
				line = frame.getLineNumber();
				column = frame.getColumnNumber();
				// Check for a sourceMap position
				source = getSourceMapping(filename, line, column);
				fileLocation = source ? `${filename}:${source[0]}:${source[1]}` : `${filename}:${line}:${column}`;
			}
			functionName = frame.getFunctionName();
			isConstructor = frame.isConstructor();
			isMethodCall = !(frame.isToplevel() || isConstructor);
			if (isMethodCall) {
				methodName = frame.getMethodName();
				typeName = frame.getTypeName();
				if (functionName) {
					tp = as = '';
					if (typeName && functionName.indexOf(typeName)) {
						tp = `${typeName}.`;
					}
					if (methodName && functionName.indexOf(`.${methodName}`) !== functionName.length - methodName.length - 1) {
						as = ` [as ${methodName}]`;
					}
					return `${tp}${functionName}${as} (${fileLocation})`;
				} else {
					return `${typeName}.${methodName || '<anonymous>'} (${fileLocation})`;
				}
			} else if (isConstructor) {
				return `new ${functionName || '<anonymous>'} (${fileLocation})`;
			} else if (functionName) {
				return `${functionName} (${fileLocation})`;
			} else {
				return fileLocation;
			}
		};

		getSourceMap = function(filename, line, column) {
			var answer, i, map, ref, ref1, sourceLocation;
			if (!(filename === '<anonymous>' || (ref = filename.slice(filename.lastIndexOf('.')), indexOf.call(FILE_EXTENSIONS, ref) >= 0))) {
				// Skip files that we didn’t compile, like Node system files that appear in
				// the stack trace, as they never have source maps.
				return null;
			}
			if (filename !== '<anonymous>' && (sourceMaps[filename] != null)) {
				return sourceMaps[filename][sourceMaps[filename].length - 1];
			// CoffeeScript compiled in a browser or via `CoffeeScript.compile` or `.run`
			// may get compiled with `options.filename` that’s missing, which becomes
			// `<anonymous>`; but the runtime might request the stack trace with the
			// filename of the script file. See if we have a source map cached under
			// `<anonymous>` that matches the error.
			} else if (sourceMaps['<anonymous>'] != null) {
				ref1 = sourceMaps['<anonymous>'];
				// Work backwards from the most recent anonymous source maps, until we find
				// one that works. This isn’t foolproof; there is a chance that multiple
				// source maps will have line/column pairs that match. But we have no other
				// way to match them. `frame.getFunction().toString()` doesn’t always work,
				// and it’s not foolproof either.
				for (i = ref1.length - 1; i >= 0; i += -1) {
					map = ref1[i];
					sourceLocation = map.sourceLocation([line - 1, column - 1]);
					if (((sourceLocation != null ? sourceLocation[0] : void 0) != null) && (sourceLocation[1] != null)) {
						return map;
					}
				}
			}
			// If all else fails, recompile this source to get a source map. We need the
			// previous section (for `<anonymous>`) despite this option, because after it
			// gets compiled we will still need to look it up from
			// `sourceMaps['<anonymous>']` in order to find and return it. That’s why we
			// start searching from the end in the previous block, because most of the
			// time the source map we want is the last one.
			if (sources[filename] != null) {
				answer = compile(sources[filename][sources[filename].length - 1], {
					filename: filename,
					sourceMap: true,
					literate: helpers.isLiterate(filename)
				});
				return answer.sourceMap;
			} else {
				return null;
			}
		};

		// Based on [michaelficarra/CoffeeScriptRedux](http://goo.gl/ZTx1p)
		// NodeJS / V8 have no support for transforming positions in stack traces using
		// sourceMap, so we must monkey-patch Error to display CoffeeScript source
		// positions.
		Error.prepareStackTrace = function(err, stack) {
			var frame, frames, getSourceMapping;
			getSourceMapping = function(filename, line, column) {
				var answer, sourceMap;
				sourceMap = getSourceMap(filename, line, column);
				if (sourceMap != null) {
					answer = sourceMap.sourceLocation([line - 1, column - 1]);
				}
				if (answer != null) {
					return [answer[0] + 1, answer[1] + 1];
				} else {
					return null;
				}
			};
			frames = (function() {
				var i, len, results;
				results = [];
				for (i = 0, len = stack.length; i < len; i++) {
					frame = stack[i];
					if (frame.getFunction() === exports.run) {
						break;
					}
					results.push(`    at ${formatSourcePosition(frame, getSourceMapping)}`);
				}
				return results;
			})();
			return `${err.toString()}\n${frames.join('\n')}\n`;
		};

		checkShebangLine = function(file, input) {
			var args, firstLine, ref, rest;
			firstLine = input.split(/$/m)[0];
			rest = firstLine != null ? firstLine.match(/^#!\s*([^\s]+\s*)(.*)/) : void 0;
			args = rest != null ? (ref = rest[2]) != null ? ref.split(/\s/).filter(function(s) {
				return s !== '';
			}) : void 0 : void 0;
			if ((args != null ? args.length : void 0) > 1) {
				console.error('The script to be run begins with a shebang line with more than one\nargument. This script will fail on platforms such as Linux which only\nallow a single argument.');
				console.error(`The shebang line was: '${firstLine}' in file '${file}'`);
				return console.error(`The arguments were: ${JSON.stringify(args)}`);
			}
		};
		*/return exports;};//#endregion
return require('/coffeescript');}();

/*!
 * String.prototype.repeat polyfill
 * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/repeat
 */
if (!String.prototype.hasOwnProperty('repeat')) {
	String.prototype.repeat = function (count) {
		var result,
			value,
			processedCount,
			methodName = 'String.prototype.repeat'
			;

		processedCount = +count;
		if (processedCount !== count) {
			processedCount = 0;
		}

		if (processedCount < 0 || processedCount == Infinity) {
			throw new RangeError(methodName + ': argument out of range.');
		}

		value = '' + this;
		processedCount = Math.floor(processedCount);

		if (value.length === 0 || processedCount === 0) {
			return '';
		}

		if (value.length * processedCount >= 1 << 28) {
			throw new RangeError(methodName + ': Repeat count must not overflow maximum string size.');
		}

		result = '';
		for (; ;) {
			if ((processedCount & 1) === 1) {
				result += value;
			}
			processedCount >>>= 1;
			if (processedCount === 0) {
				break;
			}
			value += value;
		}

		return result;
	};
}

/*!
 * Clean-css v4.1.11
 * https://github.com/jakubpawlowicz/clean-css
 *
 * Copyright (C) 2017 JakubPawlowicz.com
 * Released under the terms of MIT license
 */
var CleanCss = (function(){
	var modules = {},
		loadedModules = {},
		require = function(name) {
			var result;
		
			if (typeof loadedModules[name] !== 'undefined') {
				result = loadedModules[name];
			}
			else {
				if (typeof modules[name] !== 'undefined') {
					result = modules[name].call(this);
					
					loadedModules[name] = (typeof result !== 'undefined') ? result : null;
					modules[name] = undefined;
				}
				else {
					throw new Error("Can't load '" + name + "' module.");
				}
			}
		
			return result;
		}
		;

	//#region URL: os
	modules['os'] = function () {
		var exports = {},
			isWindows = true
			;

		exports.EOL = isWindows ? '\r\n' : '\n';

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-0/optimize
	modules['/optimizer/level-0/optimize'] = function () {
		function level0Optimize(tokens) {
		  // noop as level 0 means no optimizations!
		  return tokens;
		}

		return level0Optimize;
	};
	//#endregion

	//#region URL: /optimizer/level-1/optimize
	modules['/optimizer/level-1/optimize'] = function () {
		var shortenHex = require('/optimizer/level-1/shorten-hex');
		var shortenHsl = require('/optimizer/level-1/shorten-hsl');
		var shortenRgb = require('/optimizer/level-1/shorten-rgb');
		var sortSelectors = require('/optimizer/level-1/sort-selectors');
		var tidyRules = require('/optimizer/level-1/tidy-rules');
		var tidyBlock = require('/optimizer/level-1/tidy-block');
		var tidyAtRule = require('/optimizer/level-1/tidy-at-rule');

		var Hack = require('/optimizer/hack');
		var removeUnused = require('/optimizer/remove-unused');
		var restoreFromOptimizing = require('/optimizer/restore-from-optimizing');
		var wrapForOptimizing = require('/optimizer/wrap-for-optimizing').all;

		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;

		var Token = require('/tokenizer/token');
		var Marker = require('/tokenizer/marker');

		var formatPosition = require('/utils/format-position');
		var split = require('/utils/split');

		var IgnoreProperty = 'ignore-property';

		var CHARSET_TOKEN = '@charset';
		var CHARSET_REGEXP = new RegExp('^' + CHARSET_TOKEN, 'i');

		var DEFAULT_ROUNDING_PRECISION = require('/options/rounding-precision').DEFAULT;

		var WHOLE_PIXEL_VALUE = /(?:^|\s|\()(-?\d+)px/;
		var TIME_VALUE = /^(\-?[\d\.]+)(m?s)$/;

		var HEX_VALUE_PATTERN = /[0-9a-f]/i;
		var PROPERTY_NAME_PATTERN = /^(?:\-chrome\-|\-[\w\-]+\w|\w[\w\-]+\w|\-\-\S+)$/;
		var IMPORT_PREFIX_PATTERN = /^@import/i;
		var QUOTED_PATTERN = /^('.*'|".*")$/;
		var QUOTED_BUT_SAFE_PATTERN = /^['"][a-zA-Z][a-zA-Z\d\-_]+['"]$/;
		var URL_PREFIX_PATTERN = /^url\(/i;
		var VARIABLE_NAME_PATTERN = /^--\S+$/;

		function isNegative(value) {
		  return value && value[1][0] == '-' && parseFloat(value[1]) < 0;
		}

		function isQuoted(value) {
		  return QUOTED_PATTERN.test(value);
		}

		function isUrl(value) {
		  return URL_PREFIX_PATTERN.test(value);
		}

		function normalizeUrl(value) {
		  return value
			.replace(URL_PREFIX_PATTERN, 'url(')
			.replace(/\\?\n|\\?\r\n/g, '');
		}

		function optimizeBackground(property) {
		  var values = property.value;

		  if (values.length == 1 && values[0][1] == 'none') {
			values[0][1] = '0 0';
		  }

		  if (values.length == 1 && values[0][1] == 'transparent') {
			values[0][1] = '0 0';
		  }
		}

		function optimizeBorderRadius(property) {
		  var values = property.value;
		  var spliceAt;

		  if (values.length == 3 && values[1][1] == '/' && values[0][1] == values[2][1]) {
			spliceAt = 1;
		  } else if (values.length == 5 && values[2][1] == '/' && values[0][1] == values[3][1] && values[1][1] == values[4][1]) {
			spliceAt = 2;
		  } else if (values.length == 7 && values[3][1] == '/' && values[0][1] == values[4][1] && values[1][1] == values[5][1] && values[2][1] == values[6][1]) {
			spliceAt = 3;
		  } else if (values.length == 9 && values[4][1] == '/' && values[0][1] == values[5][1] && values[1][1] == values[6][1] && values[2][1] == values[7][1] && values[3][1] == values[8][1]) {
			spliceAt = 4;
		  }

		  if (spliceAt) {
			property.value.splice(spliceAt);
			property.dirty = true;
		  }
		}

		function optimizeColors(name, value, compatibility) {
		  if (value.indexOf('#') === -1 && value.indexOf('rgb') == -1 && value.indexOf('hsl') == -1) {
			return shortenHex(value);
		  }

		  value = value
			.replace(/rgb\((\-?\d+),(\-?\d+),(\-?\d+)\)/g, function (match, red, green, blue) {
			  return shortenRgb(red, green, blue);
			})
			.replace(/hsl\((-?\d+),(-?\d+)%?,(-?\d+)%?\)/g, function (match, hue, saturation, lightness) {
			  return shortenHsl(hue, saturation, lightness);
			})
			.replace(/(^|[^='"])#([0-9a-f]{6})/gi, function (match, prefix, color, at, inputValue) {
			  var suffix = inputValue[at + match.length];

			  if (suffix && HEX_VALUE_PATTERN.test(suffix)) {
				return match;
			  } else if (color[0] == color[1] && color[2] == color[3] && color[4] == color[5]) {
				return (prefix + '#' + color[0] + color[2] + color[4]).toLowerCase();
			  } else {
				return (prefix + '#' + color).toLowerCase();
			  }
			})
			.replace(/(^|[^='"])#([0-9a-f]{3})/gi, function (match, prefix, color) {
			  return prefix + '#' + color.toLowerCase();
			})
			.replace(/(rgb|rgba|hsl|hsla)\(([^\)]+)\)/g, function (match, colorFunction, colorDef) {
			  var tokens = colorDef.split(',');
			  var applies = (colorFunction == 'hsl' && tokens.length == 3) ||
				(colorFunction == 'hsla' && tokens.length == 4) ||
				(colorFunction == 'rgb' && tokens.length == 3 && colorDef.indexOf('%') > 0) ||
				(colorFunction == 'rgba' && tokens.length == 4 && colorDef.indexOf('%') > 0);

			  if (!applies) {
				return match;
			  }

			  if (tokens[1].indexOf('%') == -1) {
				tokens[1] += '%';
			  }

			  if (tokens[2].indexOf('%') == -1) {
				tokens[2] += '%';
			  }

			  return colorFunction + '(' + tokens.join(',') + ')';
			});

		  if (compatibility.colors.opacity && name.indexOf('background') == -1) {
			value = value.replace(/(?:rgba|hsla)\(0,0%?,0%?,0\)/g, function (match) {
			  if (split(value, ',').pop().indexOf('gradient(') > -1) {
				return match;
			  }

			  return 'transparent';
			});
		  }

		  return shortenHex(value);
		}

		function optimizeFilter(property) {
		  if (property.value.length == 1) {
			property.value[0][1] = property.value[0][1].replace(/progid:DXImageTransform\.Microsoft\.(Alpha|Chroma)(\W)/, function (match, filter, suffix) {
			  return filter.toLowerCase() + suffix;
			});
		  }

		  property.value[0][1] = property.value[0][1]
			.replace(/,(\S)/g, ', $1')
			.replace(/ ?= ?/g, '=');
		}

		function optimizeFontWeight(property, atIndex) {
		  var value = property.value[atIndex][1];

		  if (value == 'normal') {
			value = '400';
		  } else if (value == 'bold') {
			value = '700';
		  }

		  property.value[atIndex][1] = value;
		}

		function optimizeMultipleZeros(property) {
		  var values = property.value;
		  var spliceAt;

		  if (values.length == 4 && values[0][1] === '0' && values[1][1] === '0' && values[2][1] === '0' && values[3][1] === '0') {
			if (property.name.indexOf('box-shadow') > -1) {
			  spliceAt = 2;
			} else {
			  spliceAt = 1;
			}
		  }

		  if (spliceAt) {
			property.value.splice(spliceAt);
			property.dirty = true;
		  }
		}

		function optimizeOutline(property) {
		  var values = property.value;

		  if (values.length == 1 && values[0][1] == 'none') {
			values[0][1] = '0';
		  }
		}

		function optimizePixelLengths(_, value, compatibility) {
		  if (!WHOLE_PIXEL_VALUE.test(value)) {
			return value;
		  }

		  return value.replace(WHOLE_PIXEL_VALUE, function (match, val) {
			var newValue;
			var intVal = parseInt(val);

			if (intVal === 0) {
			  return match;
			}

			if (compatibility.properties.shorterLengthUnits && compatibility.units.pt && intVal * 3 % 4 === 0) {
			  newValue = intVal * 3 / 4 + 'pt';
			}

			if (compatibility.properties.shorterLengthUnits && compatibility.units.pc && intVal % 16 === 0) {
			  newValue = intVal / 16 + 'pc';
			}

			if (compatibility.properties.shorterLengthUnits && compatibility.units.in && intVal % 96 === 0) {
			  newValue = intVal / 96 + 'in';
			}

			if (newValue) {
			  newValue = match.substring(0, match.indexOf(val)) + newValue;
			}

			return newValue && newValue.length < match.length ? newValue : match;
		  });
		}

		function optimizePrecision(_, value, precisionOptions) {
		  if (!precisionOptions.enabled || value.indexOf('.') === -1) {
			return value;
		  }

		  return value
			.replace(precisionOptions.decimalPointMatcher, '$1$2$3')
			.replace(precisionOptions.zeroMatcher, function (match, integerPart, fractionPart, unit) {
			  var multiplier = precisionOptions.units[unit].multiplier;
			  var parsedInteger = parseInt(integerPart);
			  var integer = isNaN(parsedInteger) ? 0 : parsedInteger;
			  var fraction = parseFloat(fractionPart);

			  return Math.round((integer + fraction) * multiplier) / multiplier + unit;
			});
		}

		function optimizeTimeUnits(_, value) {
		  if (!TIME_VALUE.test(value))
			return value;

		  return value.replace(TIME_VALUE, function (match, val, unit) {
			var newValue;

			if (unit == 'ms') {
			  newValue = parseInt(val) / 1000 + 's';
			} else if (unit == 's') {
			  newValue = parseFloat(val) * 1000 + 'ms';
			}

			return newValue.length < match.length ? newValue : match;
		  });
		}

		function optimizeUnits(name, value, unitsRegexp) {
		  if (/^(?:\-moz\-calc|\-webkit\-calc|calc|rgb|hsl|rgba|hsla)\(/.test(value)) {
			return value;
		  }

		  if (name == 'flex' || name == '-ms-flex' || name == '-webkit-flex' || name == 'flex-basis' || name == '-webkit-flex-basis') {
			return value;
		  }

		  if (value.indexOf('%') > 0 && (name == 'height' || name == 'max-height' || name == 'width' || name == 'max-width')) {
			return value;
		  }

		  return value
			.replace(unitsRegexp, '$1' + '0' + '$2')
			.replace(unitsRegexp, '$1' + '0' + '$2');
		}

		function optimizeWhitespace(name, value) {
		  if (name.indexOf('filter') > -1 || value.indexOf(' ') == -1 || value.indexOf('expression') === 0) {
			return value;
		  }

		  if (value.indexOf(Marker.SINGLE_QUOTE) > -1 || value.indexOf(Marker.DOUBLE_QUOTE) > -1) {
			return value;
		  }

		  value = value.replace(/\s+/g, ' ');

		  if (value.indexOf('calc') > -1) {
			value = value.replace(/\) ?\/ ?/g, ')/ ');
		  }

		  return value
			.replace(/(\(;?)\s+/g, '$1')
			.replace(/\s+(;?\))/g, '$1')
			.replace(/, /g, ',');
		}

		function optimizeZeroDegUnit(_, value) {
		  if (value.indexOf('0deg') == -1) {
			return value;
		  }

		  return value.replace(/\(0deg\)/g, '(0)');
		}

		function optimizeZeroUnits(name, value) {
		  if (value.indexOf('0') == -1) {
			return value;
		  }

		  if (value.indexOf('-') > -1) {
			value = value
			  .replace(/([^\w\d\-]|^)\-0([^\.]|$)/g, '$10$2')
			  .replace(/([^\w\d\-]|^)\-0([^\.]|$)/g, '$10$2');
		  }

		  return value
			.replace(/(^|\s)0+([1-9])/g, '$1$2')
			.replace(/(^|\D)\.0+(\D|$)/g, '$10$2')
			.replace(/(^|\D)\.0+(\D|$)/g, '$10$2')
			.replace(/\.([1-9]*)0+(\D|$)/g, function (match, nonZeroPart, suffix) {
			  return (nonZeroPart.length > 0 ? '.' : '') + nonZeroPart + suffix;
			})
			.replace(/(^|\D)0\.(\d)/g, '$1.$2');
		}

		function removeQuotes(name, value) {
		  if (name == 'content' || name.indexOf('font-feature-settings') > -1 || name.indexOf('grid-') > -1) {
			return value;
		  }

		  return QUOTED_BUT_SAFE_PATTERN.test(value) ?
			value.substring(1, value.length - 1) :
			value;
		}

		function removeUrlQuotes(value) {
		  return /^url\(['"].+['"]\)$/.test(value) && !/^url\(['"].*[\*\s\(\)'"].*['"]\)$/.test(value) && !/^url\(['"]data:[^;]+;charset/.test(value) ?
			value.replace(/["']/g, '') :
			value;
		}

		function transformValue(propertyName, propertyValue, transformCallback) {
		  var transformedValue = transformCallback(propertyName, propertyValue);

		  if (transformedValue === undefined) {
			return propertyValue;
		  } else if (transformedValue === false) {
			return IgnoreProperty;
		  } else {
			return transformedValue;
		  }
		}

		//

		function optimizeBody(properties, context) {
		  var options = context.options;
		  var levelOptions = options.level[OptimizationLevel.One];
		  var property, name, type, value;
		  var valueIsUrl;
		  var propertyToken;
		  var _properties = wrapForOptimizing(properties, true);

		  propertyLoop:
		  for (var i = 0, l = _properties.length; i < l; i++) {
			property = _properties[i];
			name = property.name;

			if (!PROPERTY_NAME_PATTERN.test(name)) {
			  propertyToken = property.all[property.position];
			  context.warnings.push('Invalid property name \'' + name + '\' at ' + formatPosition(propertyToken[1][2][0]) + '. Ignoring.');
			  property.unused = true;
			}

			if (property.value.length === 0) {
			  propertyToken = property.all[property.position];
			  context.warnings.push('Empty property \'' + name + '\' at ' + formatPosition(propertyToken[1][2][0]) + '. Ignoring.');
			  property.unused = true;
			}

			if (property.hack && (
				(property.hack[0] == Hack.ASTERISK || property.hack[0] == Hack.UNDERSCORE) && !options.compatibility.properties.iePrefixHack ||
				property.hack[0] == Hack.BACKSLASH && !options.compatibility.properties.ieSuffixHack ||
				property.hack[0] == Hack.BANG && !options.compatibility.properties.ieBangHack)) {
			  property.unused = true;
			}

			if (levelOptions.removeNegativePaddings && name.indexOf('padding') === 0 && (isNegative(property.value[0]) || isNegative(property.value[1]) || isNegative(property.value[2]) || isNegative(property.value[3]))) {
			  property.unused = true;
			}

			if (!options.compatibility.properties.ieFilters && isLegacyFilter(property)) {
			  property.unused = true;
			}

			if (property.unused) {
			  continue;
			}

			if (property.block) {
			  optimizeBody(property.value[0][1], context);
			  continue;
			}

			if (VARIABLE_NAME_PATTERN.test(name)) {
			  continue;
			}

			for (var j = 0, m = property.value.length; j < m; j++) {
			  type = property.value[j][0];
			  value = property.value[j][1];
			  valueIsUrl = isUrl(value);

			  if (type == Token.PROPERTY_BLOCK) {
				property.unused = true;
				context.warnings.push('Invalid value token at ' + formatPosition(value[0][1][2][0]) + '. Ignoring.');
				break;
			  }

			  if (valueIsUrl && !context.validator.isUrl(value)) {
				property.unused = true;
				context.warnings.push('Broken URL \'' + value + '\' at ' + formatPosition(property.value[j][2][0]) + '. Ignoring.');
				break;
			  }

			  if (valueIsUrl) {
				value = levelOptions.normalizeUrls ?
				  normalizeUrl(value) :
				  value;
				value = !options.compatibility.properties.urlQuotes ?
				  removeUrlQuotes(value) :
				  value;
			  } else if (isQuoted(value)) {
				value = levelOptions.removeQuotes ?
				  removeQuotes(name, value) :
				  value;
			  } else {
				value = levelOptions.removeWhitespace ?
				  optimizeWhitespace(name, value) :
				  value;
				value = optimizePrecision(name, value, options.precision);
				value = optimizePixelLengths(name, value, options.compatibility);
				value = levelOptions.replaceTimeUnits ?
				  optimizeTimeUnits(name, value) :
				  value;
				value = levelOptions.replaceZeroUnits ?
				  optimizeZeroUnits(name, value) :
				  value;

				if (options.compatibility.properties.zeroUnits) {
				  value = optimizeZeroDegUnit(name, value);
				  value = optimizeUnits(name, value, options.unitsRegexp);
				}

				if (options.compatibility.properties.colors) {
				  value = optimizeColors(name, value, options.compatibility);
				}
			  }

			  value = transformValue(name, value, levelOptions.transform);

			  if (value === IgnoreProperty) {
				property.unused = true;
				continue propertyLoop;
			  }

			  property.value[j][1] = value;
			}

			if (levelOptions.replaceMultipleZeros) {
			  optimizeMultipleZeros(property);
			}

			if (name == 'background' && levelOptions.optimizeBackground) {
			  optimizeBackground(property);
			} else if (name.indexOf('border') === 0 && name.indexOf('radius') > 0 && levelOptions.optimizeBorderRadius) {
			  optimizeBorderRadius(property);
			} else if (name == 'filter'&& levelOptions.optimizeFilter && options.compatibility.properties.ieFilters) {
			  optimizeFilter(property);
			} else if (name == 'font-weight' && levelOptions.optimizeFontWeight) {
			  optimizeFontWeight(property, 0);
			} else if (name == 'outline' && levelOptions.optimizeOutline) {
			  optimizeOutline(property);
			}
		  }

		  restoreFromOptimizing(_properties);
		  removeUnused(_properties);
		  removeComments(properties, options);
		}

		function removeComments(tokens, options) {
		  var token;
		  var i;

		  for (i = 0; i < tokens.length; i++) {
			token = tokens[i];

			if (token[0] != Token.COMMENT) {
			  continue;
			}

			optimizeComment(token, options);

			if (token[1].length === 0) {
			  tokens.splice(i, 1);
			  i--;
			}
		  }
		}

		function optimizeComment(token, options) {
		  if (token[1][2] == Marker.EXCLAMATION && (options.level[OptimizationLevel.One].specialComments == 'all' || options.commentsKept < options.level[OptimizationLevel.One].specialComments)) {
			options.commentsKept++;
			return;
		  }

		  token[1] = [];
		}

		function cleanupCharsets(tokens) {
		  var hasCharset = false;

		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];

			if (token[0] != Token.AT_RULE)
			  continue;

			if (!CHARSET_REGEXP.test(token[1]))
			  continue;

			if (hasCharset || token[1].indexOf(CHARSET_TOKEN) == -1) {
			  tokens.splice(i, 1);
			  i--;
			  l--;
			} else {
			  hasCharset = true;
			  tokens.splice(i, 1);
			  tokens.unshift([Token.AT_RULE, token[1].replace(CHARSET_REGEXP, CHARSET_TOKEN)]);
			}
		  }
		}

		function buildUnitRegexp(options) {
		  var units = ['px', 'em', 'ex', 'cm', 'mm', 'in', 'pt', 'pc', '%'];
		  var otherUnits = ['ch', 'rem', 'vh', 'vm', 'vmax', 'vmin', 'vw'];

		  otherUnits.forEach(function (unit) {
			if (options.compatibility.units[unit]) {
			  units.push(unit);
			}
		  });

		  return new RegExp('(^|\\s|\\(|,)0(?:' + units.join('|') + ')(\\W|$)', 'g');
		}

		function buildPrecisionOptions(roundingPrecision) {
		  var precisionOptions = {
			matcher: null,
			units: {},
		  };
		  var optimizable = [];
		  var unit;
		  var value;

		  for (unit in roundingPrecision) {
			value = roundingPrecision[unit];

			if (value != DEFAULT_ROUNDING_PRECISION) {
			  precisionOptions.units[unit] = {};
			  precisionOptions.units[unit].value = value;
			  precisionOptions.units[unit].multiplier = Math.pow(10, value);

			  optimizable.push(unit);
			}
		  }

		  if (optimizable.length > 0) {
			precisionOptions.enabled = true;
			precisionOptions.decimalPointMatcher = new RegExp('(\\d)\\.($|' + optimizable.join('|') + ')($|\W)', 'g');
			precisionOptions.zeroMatcher = new RegExp('(\\d*)(\\.\\d+)(' + optimizable.join('|') + ')', 'g');
		  }

		  return precisionOptions;
		}

		function isImport(token) {
		  return IMPORT_PREFIX_PATTERN.test(token[1]);
		}

		function isLegacyFilter(property) {
		  var value;

		  if (property.name == 'filter' || property.name == '-ms-filter') {
			value = property.value[0][1];

			return value.indexOf('progid') > -1 ||
			  value.indexOf('alpha') === 0 ||
			  value.indexOf('chroma') === 0;
		  } else {
			return false;
		  }
		}

		function level1Optimize(tokens, context) {
		  var options = context.options;
		  var levelOptions = options.level[OptimizationLevel.One];
		  var ie7Hack = options.compatibility.selectors.ie7Hack;
		  var adjacentSpace = options.compatibility.selectors.adjacentSpace;
		  var spaceAfterClosingBrace = options.compatibility.properties.spaceAfterClosingBrace;
		  var format = options.format;
		  var mayHaveCharset = false;
		  var afterRules = false;

		  options.unitsRegexp = options.unitsRegexp || buildUnitRegexp(options);
		  options.precision = options.precision || buildPrecisionOptions(levelOptions.roundingPrecision);
		  options.commentsKept = options.commentsKept || 0;

		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];

			switch (token[0]) {
			  case Token.AT_RULE:
				token[1] = isImport(token) && afterRules ? '' : token[1];
				token[1] = levelOptions.tidyAtRules ? tidyAtRule(token[1]) : token[1];
				mayHaveCharset = true;
				break;
			  case Token.AT_RULE_BLOCK:
				optimizeBody(token[2], context);
				afterRules = true;
				break;
			  case Token.NESTED_BLOCK:
				token[1] = levelOptions.tidyBlockScopes ? tidyBlock(token[1], spaceAfterClosingBrace) : token[1];
				level1Optimize(token[2], context);
				afterRules = true;
				break;
			  case Token.COMMENT:
				optimizeComment(token, options);
				break;
			  case Token.RULE:
				token[1] = levelOptions.tidySelectors ? tidyRules(token[1], !ie7Hack, adjacentSpace, format, context.warnings) : token[1];
				token[1] = token[1].length > 1 ? sortSelectors(token[1], levelOptions.selectorsSortingMethod) : token[1];
				optimizeBody(token[2], context);
				afterRules = true;
				break;
			}

			if (token[0] == Token.COMMENT && token[1].length === 0 || levelOptions.removeEmpty && (token[1].length === 0 || (token[2] && token[2].length === 0))) {
			  tokens.splice(i, 1);
			  i--;
			  l--;
			}
		  }

		  if (levelOptions.cleanupCharsets && mayHaveCharset) {
			cleanupCharsets(tokens);
		  }

		  return tokens;
		}

		return level1Optimize;
	};
	//#endregion

	//#region URL: /optimizer/level-1/shorten-hex
	modules['/optimizer/level-1/shorten-hex'] = function () {
		var COLORS = {
		  aliceblue: '#f0f8ff',
		  antiquewhite: '#faebd7',
		  aqua: '#0ff',
		  aquamarine: '#7fffd4',
		  azure: '#f0ffff',
		  beige: '#f5f5dc',
		  bisque: '#ffe4c4',
		  black: '#000',
		  blanchedalmond: '#ffebcd',
		  blue: '#00f',
		  blueviolet: '#8a2be2',
		  brown: '#a52a2a',
		  burlywood: '#deb887',
		  cadetblue: '#5f9ea0',
		  chartreuse: '#7fff00',
		  chocolate: '#d2691e',
		  coral: '#ff7f50',
		  cornflowerblue: '#6495ed',
		  cornsilk: '#fff8dc',
		  crimson: '#dc143c',
		  cyan: '#0ff',
		  darkblue: '#00008b',
		  darkcyan: '#008b8b',
		  darkgoldenrod: '#b8860b',
		  darkgray: '#a9a9a9',
		  darkgreen: '#006400',
		  darkgrey: '#a9a9a9',
		  darkkhaki: '#bdb76b',
		  darkmagenta: '#8b008b',
		  darkolivegreen: '#556b2f',
		  darkorange: '#ff8c00',
		  darkorchid: '#9932cc',
		  darkred: '#8b0000',
		  darksalmon: '#e9967a',
		  darkseagreen: '#8fbc8f',
		  darkslateblue: '#483d8b',
		  darkslategray: '#2f4f4f',
		  darkslategrey: '#2f4f4f',
		  darkturquoise: '#00ced1',
		  darkviolet: '#9400d3',
		  deeppink: '#ff1493',
		  deepskyblue: '#00bfff',
		  dimgray: '#696969',
		  dimgrey: '#696969',
		  dodgerblue: '#1e90ff',
		  firebrick: '#b22222',
		  floralwhite: '#fffaf0',
		  forestgreen: '#228b22',
		  fuchsia: '#f0f',
		  gainsboro: '#dcdcdc',
		  ghostwhite: '#f8f8ff',
		  gold: '#ffd700',
		  goldenrod: '#daa520',
		  gray: '#808080',
		  green: '#008000',
		  greenyellow: '#adff2f',
		  grey: '#808080',
		  honeydew: '#f0fff0',
		  hotpink: '#ff69b4',
		  indianred: '#cd5c5c',
		  indigo: '#4b0082',
		  ivory: '#fffff0',
		  khaki: '#f0e68c',
		  lavender: '#e6e6fa',
		  lavenderblush: '#fff0f5',
		  lawngreen: '#7cfc00',
		  lemonchiffon: '#fffacd',
		  lightblue: '#add8e6',
		  lightcoral: '#f08080',
		  lightcyan: '#e0ffff',
		  lightgoldenrodyellow: '#fafad2',
		  lightgray: '#d3d3d3',
		  lightgreen: '#90ee90',
		  lightgrey: '#d3d3d3',
		  lightpink: '#ffb6c1',
		  lightsalmon: '#ffa07a',
		  lightseagreen: '#20b2aa',
		  lightskyblue: '#87cefa',
		  lightslategray: '#778899',
		  lightslategrey: '#778899',
		  lightsteelblue: '#b0c4de',
		  lightyellow: '#ffffe0',
		  lime: '#0f0',
		  limegreen: '#32cd32',
		  linen: '#faf0e6',
		  magenta: '#ff00ff',
		  maroon: '#800000',
		  mediumaquamarine: '#66cdaa',
		  mediumblue: '#0000cd',
		  mediumorchid: '#ba55d3',
		  mediumpurple: '#9370db',
		  mediumseagreen: '#3cb371',
		  mediumslateblue: '#7b68ee',
		  mediumspringgreen: '#00fa9a',
		  mediumturquoise: '#48d1cc',
		  mediumvioletred: '#c71585',
		  midnightblue: '#191970',
		  mintcream: '#f5fffa',
		  mistyrose: '#ffe4e1',
		  moccasin: '#ffe4b5',
		  navajowhite: '#ffdead',
		  navy: '#000080',
		  oldlace: '#fdf5e6',
		  olive: '#808000',
		  olivedrab: '#6b8e23',
		  orange: '#ffa500',
		  orangered: '#ff4500',
		  orchid: '#da70d6',
		  palegoldenrod: '#eee8aa',
		  palegreen: '#98fb98',
		  paleturquoise: '#afeeee',
		  palevioletred: '#db7093',
		  papayawhip: '#ffefd5',
		  peachpuff: '#ffdab9',
		  peru: '#cd853f',
		  pink: '#ffc0cb',
		  plum: '#dda0dd',
		  powderblue: '#b0e0e6',
		  purple: '#800080',
		  rebeccapurple: '#663399',
		  red: '#f00',
		  rosybrown: '#bc8f8f',
		  royalblue: '#4169e1',
		  saddlebrown: '#8b4513',
		  salmon: '#fa8072',
		  sandybrown: '#f4a460',
		  seagreen: '#2e8b57',
		  seashell: '#fff5ee',
		  sienna: '#a0522d',
		  silver: '#c0c0c0',
		  skyblue: '#87ceeb',
		  slateblue: '#6a5acd',
		  slategray: '#708090',
		  slategrey: '#708090',
		  snow: '#fffafa',
		  springgreen: '#00ff7f',
		  steelblue: '#4682b4',
		  tan: '#d2b48c',
		  teal: '#008080',
		  thistle: '#d8bfd8',
		  tomato: '#ff6347',
		  turquoise: '#40e0d0',
		  violet: '#ee82ee',
		  wheat: '#f5deb3',
		  white: '#fff',
		  whitesmoke: '#f5f5f5',
		  yellow: '#ff0',
		  yellowgreen: '#9acd32'
		};

		var toHex = {};
		var toName = {};

		for (var name in COLORS) {
		  var hex = COLORS[name];

		  if (name.length < hex.length) {
			toName[hex] = name;
		  } else {
			toHex[name] = hex;
		  }
		}

		var toHexPattern = new RegExp('(^| |,|\\))(' + Object.keys(toHex).join('|') + ')( |,|\\)|$)', 'ig');
		var toNamePattern = new RegExp('(' + Object.keys(toName).join('|') + ')([^a-f0-9]|$)', 'ig');

		function hexConverter(match, prefix, colorValue, suffix) {
		  return prefix + toHex[colorValue.toLowerCase()] + suffix;
		}

		function nameConverter(match, colorValue, suffix) {
		  return toName[colorValue.toLowerCase()] + suffix;
		}

		function shortenHex(value) {
		  var hasHex = value.indexOf('#') > -1;
		  var shortened = value.replace(toHexPattern, hexConverter);

		  if (shortened != value) {
			shortened = shortened.replace(toHexPattern, hexConverter);
		  }

		  return hasHex ?
			shortened.replace(toNamePattern, nameConverter) :
			shortened;
		}

		return shortenHex;
	};
	//#endregion

	//#region URL: /optimizer/level-1/shorten-hsl
	modules['/optimizer/level-1/shorten-hsl'] = function () {
		// HSL to RGB converter. Both methods adapted from:
		// http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv-color-model-conversion-algorithms-in-javascript

		function hslToRgb(h, s, l) {
		  var r, g, b;

		  // normalize hue orientation b/w 0 and 360 degrees
		  h = h % 360;
		  if (h < 0)
			h += 360;
		  h = ~~h / 360;

		  if (s < 0)
			s = 0;
		  else if (s > 100)
			s = 100;
		  s = ~~s / 100;

		  if (l < 0)
			l = 0;
		  else if (l > 100)
			l = 100;
		  l = ~~l / 100;

		  if (s === 0) {
			r = g = b = l; // achromatic
		  } else {
			var q = l < 0.5 ?
			  l * (1 + s) :
			  l + s - l * s;
			var p = 2 * l - q;
			r = hueToRgb(p, q, h + 1/3);
			g = hueToRgb(p, q, h);
			b = hueToRgb(p, q, h - 1/3);
		  }

		  return [~~(r * 255), ~~(g * 255), ~~(b * 255)];
		}

		function hueToRgb(p, q, t) {
		  if (t < 0) t += 1;
		  if (t > 1) t -= 1;
		  if (t < 1/6) return p + (q - p) * 6 * t;
		  if (t < 1/2) return q;
		  if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
		  return p;
		}

		function shortenHsl(hue, saturation, lightness) {
		  var asRgb = hslToRgb(hue, saturation, lightness);
		  var redAsHex = asRgb[0].toString(16);
		  var greenAsHex = asRgb[1].toString(16);
		  var blueAsHex = asRgb[2].toString(16);

		  return '#' +
			((redAsHex.length == 1 ? '0' : '') + redAsHex) +
			((greenAsHex.length == 1 ? '0' : '') + greenAsHex) +
			((blueAsHex.length == 1 ? '0' : '') + blueAsHex);
		}

		return shortenHsl;
	};
	//#endregion

	//#region URL: /optimizer/level-1/shorten-rgb
	modules['/optimizer/level-1/shorten-rgb'] = function () {
		function shortenRgb(red, green, blue) {
		  var normalizedRed = Math.max(0, Math.min(parseInt(red), 255));
		  var normalizedGreen = Math.max(0, Math.min(parseInt(green), 255));
		  var normalizedBlue = Math.max(0, Math.min(parseInt(blue), 255));

		  // Credit: Asen  http://jsbin.com/UPUmaGOc/2/edit?js,console
		  return '#' + ('00000' + (normalizedRed << 16 | normalizedGreen << 8 | normalizedBlue).toString(16)).slice(-6);
		}

		return shortenRgb;
	};
	//#endregion

	//#region URL: /optimizer/level-1/sort-selectors
	modules['/optimizer/level-1/sort-selectors'] = function () {
		var naturalCompare = require('/utils/natural-compare');

		function naturalSorter(scope1, scope2) {
		  return naturalCompare(scope1[1], scope2[1]);
		}

		function standardSorter(scope1, scope2) {
		  return scope1[1] > scope2[1] ? 1 : -1;
		}

		function sortSelectors(selectors, method) {
		  switch (method) {
			case 'natural':
			  return selectors.sort(naturalSorter);
			case 'standard':
			  return selectors.sort(standardSorter);
			case 'none':
			case false:
			  return selectors;
		  }
		}

		return sortSelectors;
	};
	//#endregion

	//#region URL: /optimizer/level-1/tidy-at-rule
	modules['/optimizer/level-1/tidy-at-rule'] = function () {
		function tidyAtRule(value) {
		  return value
			.replace(/\s+/g, ' ')
			.replace(/url\(\s+/g, 'url(')
			.replace(/\s+\)/g, ')')
			.trim();
		}

		return tidyAtRule;
	};
	//#endregion

	//#region URL: /optimizer/level-1/tidy-block
	modules['/optimizer/level-1/tidy-block'] = function () {
		var SUPPORTED_COMPACT_BLOCK_MATCHER = /^@media\W/;

		function tidyBlock(values, spaceAfterClosingBrace) {
		  var withoutSpaceAfterClosingBrace;
		  var i;

		  for (i = values.length - 1; i >= 0; i--) {
			withoutSpaceAfterClosingBrace = !spaceAfterClosingBrace && SUPPORTED_COMPACT_BLOCK_MATCHER.test(values[i][1]);

			values[i][1] = values[i][1]
			  .replace(/\n|\r\n/g, ' ')
			  .replace(/\s+/g, ' ')
			  .replace(/(,|:|\() /g, '$1')
			  .replace(/ \)/g, ')')
			  .replace(/'([a-zA-Z][a-zA-Z\d\-_]+)'/, '$1')
			  .replace(/"([a-zA-Z][a-zA-Z\d\-_]+)"/, '$1')
			  .replace(withoutSpaceAfterClosingBrace ? /\) /g : null, ')');
		  }

		  return values;
		}

		return tidyBlock;
	};
	//#endregion

	//#region URL: /optimizer/level-1/tidy-rules
	modules['/optimizer/level-1/tidy-rules'] = function () {
		var Spaces = require('/options/format').Spaces;
		var Marker = require('/tokenizer/marker');
		var formatPosition = require('/utils/format-position');

		var CASE_ATTRIBUTE_PATTERN = /[\s"'][iI]\s*\]/;
		var CASE_RESTORE_PATTERN = /([\d\w])([iI])\]/g;
		var DOUBLE_QUOTE_CASE_PATTERN = /="([a-zA-Z][a-zA-Z\d\-_]+)"([iI])/g;
		var DOUBLE_QUOTE_PATTERN = /="([a-zA-Z][a-zA-Z\d\-_]+)"(\s|\])/g;
		var HTML_COMMENT_PATTERN = /^(?:(?:<!--|-->)\s*)+/;
		var SINGLE_QUOTE_CASE_PATTERN = /='([a-zA-Z][a-zA-Z\d\-_]+)'([iI])/g;
		var SINGLE_QUOTE_PATTERN = /='([a-zA-Z][a-zA-Z\d\-_]+)'(\s|\])/g;
		var RELATION_PATTERN = /[>\+~]/;
		var WHITESPACE_PATTERN = /\s/;

		var ASTERISK_PLUS_HTML_HACK = '*+html ';
		var ASTERISK_FIRST_CHILD_PLUS_HTML_HACK = '*:first-child+html ';
		var LESS_THAN = '<';

		function hasInvalidCharacters(value) {
		  var isEscaped;
		  var isInvalid = false;
		  var character;
		  var isQuote = false;
		  var i, l;

		  for (i = 0, l = value.length; i < l; i++) {
			character = value[i];

			if (isEscaped) {
			  // continue as always
			} else if (character == Marker.SINGLE_QUOTE || character == Marker.DOUBLE_QUOTE) {
			  isQuote = !isQuote;
			} else if (!isQuote && (character == Marker.CLOSE_CURLY_BRACKET || character == Marker.EXCLAMATION || character == LESS_THAN || character == Marker.SEMICOLON)) {
			  isInvalid = true;
			  break;
			} else if (!isQuote && i === 0 && RELATION_PATTERN.test(character)) {
			  isInvalid = true;
			  break;
			}

			isEscaped = character == Marker.BACK_SLASH;
		  }

		  return isInvalid;
		}

		function removeWhitespace(value, format) {
		  var stripped = [];
		  var character;
		  var isNewLineNix;
		  var isNewLineWin;
		  var isEscaped;
		  var wasEscaped;
		  var isQuoted;
		  var isSingleQuoted;
		  var isDoubleQuoted;
		  var isAttribute;
		  var isRelation;
		  var isWhitespace;
		  var roundBracketLevel = 0;
		  var wasRelation = false;
		  var wasWhitespace = false;
		  var withCaseAttribute = CASE_ATTRIBUTE_PATTERN.test(value);
		  var spaceAroundRelation = format && format.spaces[Spaces.AroundSelectorRelation];
		  var i, l;

		  for (i = 0, l = value.length; i < l; i++) {
			character = value[i];

			isNewLineNix = character == Marker.NEW_LINE_NIX;
			isNewLineWin = character == Marker.NEW_LINE_NIX && value[i - 1] == Marker.NEW_LINE_WIN;
			isQuoted = isSingleQuoted || isDoubleQuoted;
			isRelation = !isAttribute && !isEscaped && roundBracketLevel === 0 && RELATION_PATTERN.test(character);
			isWhitespace = WHITESPACE_PATTERN.test(character);

			if (wasEscaped && isQuoted && isNewLineWin) {
			  // swallow escaped new windows lines in comments
			  stripped.pop();
			  stripped.pop();
			} else if (isEscaped && isQuoted && isNewLineNix) {
			  // swallow escaped new *nix lines in comments
			  stripped.pop();
			} else if (isEscaped) {
			  stripped.push(character);
			} else if (character == Marker.OPEN_SQUARE_BRACKET && !isQuoted) {
			  stripped.push(character);
			  isAttribute = true;
			} else if (character == Marker.CLOSE_SQUARE_BRACKET && !isQuoted) {
			  stripped.push(character);
			  isAttribute = false;
			} else if (character == Marker.OPEN_ROUND_BRACKET && !isQuoted) {
			  stripped.push(character);
			  roundBracketLevel++;
			} else if (character == Marker.CLOSE_ROUND_BRACKET && !isQuoted) {
			  stripped.push(character);
			  roundBracketLevel--;
			} else if (character == Marker.SINGLE_QUOTE && !isQuoted) {
			  stripped.push(character);
			  isSingleQuoted = true;
			} else if (character == Marker.DOUBLE_QUOTE && !isQuoted) {
			  stripped.push(character);
			  isDoubleQuoted = true;
			} else if (character == Marker.SINGLE_QUOTE && isQuoted) {
			  stripped.push(character);
			  isSingleQuoted = false;
			} else if (character == Marker.DOUBLE_QUOTE && isQuoted) {
			  stripped.push(character);
			  isDoubleQuoted = false;
			} else if (isWhitespace && wasRelation && !spaceAroundRelation) {
			  continue;
			} else if (!isWhitespace && wasRelation && spaceAroundRelation) {
			  stripped.push(Marker.SPACE);
			  stripped.push(character);
			} else if (isWhitespace && (isAttribute || roundBracketLevel > 0) && !isQuoted) {
			  // skip space
			} else if (isWhitespace && wasWhitespace && !isQuoted) {
			  // skip extra space
			} else if ((isNewLineWin || isNewLineNix) && (isAttribute || roundBracketLevel > 0) && isQuoted) {
			  // skip newline
			} else if (isRelation && wasWhitespace && !spaceAroundRelation) {
			  stripped.pop();
			  stripped.push(character);
			} else if (isRelation && !wasWhitespace && spaceAroundRelation) {
			  stripped.push(Marker.SPACE);
			  stripped.push(character);
			} else if (isWhitespace) {
			  stripped.push(Marker.SPACE);
			} else {
			  stripped.push(character);
			}

			wasEscaped = isEscaped;
			isEscaped = character == Marker.BACK_SLASH;
			wasRelation = isRelation;
			wasWhitespace = isWhitespace;
		  }

		  return withCaseAttribute ?
			stripped.join('').replace(CASE_RESTORE_PATTERN, '$1 $2]') :
			stripped.join('');
		}

		function removeQuotes(value) {
		  if (value.indexOf('\'') == -1 && value.indexOf('"') == -1) {
			return value;
		  }

		  return value
			.replace(SINGLE_QUOTE_CASE_PATTERN, '=$1 $2')
			.replace(SINGLE_QUOTE_PATTERN, '=$1$2')
			.replace(DOUBLE_QUOTE_CASE_PATTERN, '=$1 $2')
			.replace(DOUBLE_QUOTE_PATTERN, '=$1$2');
		}

		function tidyRules(rules, removeUnsupported, adjacentSpace, format, warnings) {
		  var list = [];
		  var repeated = [];

		  function removeHTMLComment(rule, match) {
			warnings.push('HTML comment \'' + match + '\' at ' + formatPosition(rule[2][0]) + '. Removing.');
			return '';
		  }

		  for (var i = 0, l = rules.length; i < l; i++) {
			var rule = rules[i];
			var reduced = rule[1];

			reduced = reduced.replace(HTML_COMMENT_PATTERN, removeHTMLComment.bind(null, rule));

			if (hasInvalidCharacters(reduced)) {
			  warnings.push('Invalid selector \'' + rule[1] + '\' at ' + formatPosition(rule[2][0]) + '. Ignoring.');
			  continue;
			}

			reduced = removeWhitespace(reduced, format);
			reduced = removeQuotes(reduced);

			if (adjacentSpace && reduced.indexOf('nav') > 0) {
			  reduced = reduced.replace(/\+nav(\S|$)/, '+ nav$1');
			}

			if (removeUnsupported && reduced.indexOf(ASTERISK_PLUS_HTML_HACK) > -1) {
			  continue;
			}

			if (removeUnsupported && reduced.indexOf(ASTERISK_FIRST_CHILD_PLUS_HTML_HACK) > -1) {
			  continue;
			}

			if (reduced.indexOf('*') > -1) {
			  reduced = reduced
				.replace(/\*([:#\.\[])/g, '$1')
				.replace(/^(\:first\-child)?\+html/, '*$1+html');
			}

			if (repeated.indexOf(reduced) > -1) {
			  continue;
			}

			rule[1] = reduced;
			repeated.push(reduced);
			list.push(rule);
		  }

		  if (list.length == 1 && list[0][1].length === 0) {
			warnings.push('Empty selector \'' + list[0][1] + '\' at ' + formatPosition(list[0][2][0]) + '. Ignoring.');
			list = [];
		  }

		  return list;
		}

		return tidyRules;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/every-values-pair
	modules['/optimizer/level-2/properties/every-values-pair'] = function () {
		var Marker = require('/tokenizer/marker');

		function everyValuesPair(fn, left, right) {
		  var leftSize = left.value.length;
		  var rightSize = right.value.length;
		  var total = Math.max(leftSize, rightSize);
		  var lowerBound = Math.min(leftSize, rightSize) - 1;
		  var leftValue;
		  var rightValue;
		  var position;

		  for (position = 0; position < total; position++) {
			leftValue = left.value[position] && left.value[position][1] || leftValue;
			rightValue = right.value[position] && right.value[position][1] || rightValue;

			if (leftValue == Marker.COMMA || rightValue == Marker.COMMA) {
			  continue;
			}

			if (!fn(leftValue, rightValue, position, position <= lowerBound)) {
			  return false;
			}
		  }

		  return true;
		}

		return everyValuesPair;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/find-component-in
	modules['/optimizer/level-2/properties/find-component-in'] = function () {
		var compactable = require('/optimizer/level-2/compactable');

		function findComponentIn(shorthand, longhand) {
		  var comparator = nameComparator(longhand);

		  return findInDirectComponents(shorthand, comparator) || findInSubComponents(shorthand, comparator);
		}

		function nameComparator(to) {
		  return function (property) {
			return to.name === property.name;
		  };
		}

		function findInDirectComponents(shorthand, comparator) {
		  return shorthand.components.filter(comparator)[0];
		}

		function findInSubComponents(shorthand, comparator) {
		  var shorthandComponent;
		  var longhandMatch;
		  var i, l;

		  if (!compactable[shorthand.name].shorthandComponents) {
			return;
		  }

		  for (i = 0, l = shorthand.components.length; i < l; i++) {
			shorthandComponent = shorthand.components[i];
			longhandMatch = findInDirectComponents(shorthandComponent, comparator);

			if (longhandMatch) {
			  return longhandMatch;
			}
		  }

		  return;
		}

		return findComponentIn;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/has-inherit
	modules['/optimizer/level-2/properties/has-inherit'] = function () {
		function hasInherit(property) {
		  for (var i = property.value.length - 1; i >= 0; i--) {
			if (property.value[i][1] == 'inherit')
			  return true;
		  }

		  return false;
		}

		return hasInherit;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/is-component-of
	modules['/optimizer/level-2/properties/is-component-of'] = function () {
		var compactable = require('/optimizer/level-2/compactable');

		function isComponentOf(property1, property2, shallow) {
		  return isDirectComponentOf(property1, property2) ||
			!shallow && !!compactable[property1.name].shorthandComponents && isSubComponentOf(property1, property2);
		}

		function isDirectComponentOf(property1, property2) {
		  var descriptor = compactable[property1.name];

		  return 'components' in descriptor && descriptor.components.indexOf(property2.name) > -1;
		}

		function isSubComponentOf(property1, property2) {
		  return property1
			.components
			.some(function (component) {
			  return isDirectComponentOf(component, property2);
			});
		}

		return isComponentOf;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/is-mergeable-shorthand
	modules['/optimizer/level-2/properties/is-mergeable-shorthand'] = function () {
		var Marker = require('/tokenizer/marker');

		function isMergeableShorthand(shorthand) {
		  if (shorthand.name != 'font') {
			return true;
		  }

		  return shorthand.value[0][1].indexOf(Marker.INTERNAL) == -1;
		}

		return isMergeableShorthand;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/merge-into-shorthands
	modules['/optimizer/level-2/properties/merge-into-shorthands'] = function () {
		var everyValuesPair = require('/optimizer/level-2/properties/every-values-pair');
		var hasInherit = require('/optimizer/level-2/properties/has-inherit');
		var populateComponents = require('/optimizer/level-2/properties/populate-components');

		var compactable = require('/optimizer/level-2/compactable');
		var deepClone = require('/optimizer/level-2/clone').deep;
		var restoreWithComponents = require('/optimizer/level-2/restore-with-components');

		var restoreFromOptimizing = require('/optimizer/restore-from-optimizing');
		var wrapSingle = require('/optimizer/wrap-for-optimizing').single;

		var serializeBody = require('/writer/one-time').body;
		var Token = require('/tokenizer/token');

		function mergeIntoShorthands(properties, validator) {
		  var candidates = {};
		  var descriptor;
		  var componentOf;
		  var property;
		  var i, l;
		  var j, m;

		  // there is no shorthand property made up of less than 3 longhands
		  if (properties.length < 3) {
			return;
		  }

		  for (i = 0, l = properties.length; i < l; i++) {
			property = properties[i];
			descriptor = compactable[property.name];

			if (property.unused) {
			  continue;
			}

			if (property.hack) {
			  continue;
			}

			if (property.block) {
			  continue;
			}

			invalidateOrCompact(properties, i, candidates, validator);

			if (descriptor && descriptor.componentOf) {
			  for (j = 0, m = descriptor.componentOf.length; j < m; j++) {
				componentOf = descriptor.componentOf[j];

				candidates[componentOf] = candidates[componentOf] || {};
				candidates[componentOf][property.name] = property;
			  }
			}
		  }

		  invalidateOrCompact(properties, i, candidates, validator);
		}

		function invalidateOrCompact(properties, position, candidates, validator) {
		  var invalidatedBy = properties[position];
		  var shorthandName;
		  var shorthandDescriptor;
		  var candidateComponents;

		  for (shorthandName in candidates) {
			if (undefined !== invalidatedBy && shorthandName == invalidatedBy.name) {
			  continue;
			}

			shorthandDescriptor = compactable[shorthandName];
			candidateComponents = candidates[shorthandName];
			if (invalidatedBy && invalidates(candidates, shorthandName, invalidatedBy)) {
			  delete candidates[shorthandName];
			  continue;
			}

			if (shorthandDescriptor.components.length > Object.keys(candidateComponents).length) {
			  continue;
			}

			if (mixedImportance(candidateComponents)) {
			  continue;
			}

			if (!overridable(candidateComponents, shorthandName, validator)) {
			  continue;
			}

			if (!mergeable(candidateComponents)) {
			  continue;
			}

			if (mixedInherit(candidateComponents)) {
			  replaceWithInheritBestFit(properties, candidateComponents, shorthandName, validator);
			} else {
			  replaceWithShorthand(properties, candidateComponents, shorthandName, validator);
			}
		  }
		}

		function invalidates(candidates, shorthandName, invalidatedBy) {
		  var shorthandDescriptor = compactable[shorthandName];
		  var invalidatedByDescriptor = compactable[invalidatedBy.name];
		  var componentName;

		  if ('overridesShorthands' in shorthandDescriptor && shorthandDescriptor.overridesShorthands.indexOf(invalidatedBy.name) > -1) {
			return true;
		  }

		  if (invalidatedByDescriptor && 'componentOf' in invalidatedByDescriptor) {
			for (componentName in candidates[shorthandName]) {
			  if (invalidatedByDescriptor.componentOf.indexOf(componentName) > -1) {
				return true;
			  }
			}
		  }

		  return false;
		}

		function mixedImportance(components) {
		  var important;
		  var componentName;

		  for (componentName in components) {
			if (undefined !== important && components[componentName].important != important) {
			  return true;
			}

			important = components[componentName].important;
		  }

		  return false;
		}

		function overridable(components, shorthandName, validator) {
		  var descriptor = compactable[shorthandName];
		  var newValuePlaceholder = [
			Token.PROPERTY,
			[Token.PROPERTY_NAME, shorthandName],
			[Token.PROPERTY_VALUE, descriptor.defaultValue]
		  ];
		  var newProperty = wrapSingle(newValuePlaceholder);
		  var component;
		  var mayOverride;
		  var i, l;

		  populateComponents([newProperty], validator, []);

		  for (i = 0, l = descriptor.components.length; i < l; i++) {
			component = components[descriptor.components[i]];
			mayOverride = compactable[component.name].canOverride;

			if (!everyValuesPair(mayOverride.bind(null, validator), newProperty.components[i], component)) {
			  return false;
			}
		  }

		  return true;
		}

		function mergeable(components) {
		  var lastCount = null;
		  var currentCount;
		  var componentName;
		  var component;
		  var descriptor;
		  var values;

		  for (componentName in components) {
			component = components[componentName];
			descriptor = compactable[componentName];

			if (!('restore' in descriptor)) {
			  continue;
			}

			restoreFromOptimizing([component.all[component.position]], restoreWithComponents);
			values = descriptor.restore(component, compactable);

			currentCount = values.length;

			if (lastCount !== null && currentCount !== lastCount) {
			  return false;
			}

			lastCount = currentCount;
		  }

		  return true;
		}

		function mixedInherit(components) {
		  var componentName;
		  var lastValue = null;
		  var currentValue;

		  for (componentName in components) {
			currentValue = hasInherit(components[componentName]);

			if (lastValue !== null && lastValue !== currentValue) {
			  return true;
			}

			lastValue = currentValue;
		  }

		  return false;
		}

		function replaceWithInheritBestFit(properties, candidateComponents, shorthandName, validator) {
		  var viaLonghands = buildSequenceWithInheritLonghands(candidateComponents, shorthandName, validator);
		  var viaShorthand = buildSequenceWithInheritShorthand(candidateComponents, shorthandName, validator);
		  var longhandTokensSequence = viaLonghands[0];
		  var shorthandTokensSequence = viaShorthand[0];
		  var isLonghandsShorter = serializeBody(longhandTokensSequence).length < serializeBody(shorthandTokensSequence).length;
		  var newTokensSequence = isLonghandsShorter ? longhandTokensSequence : shorthandTokensSequence;
		  var newProperty = isLonghandsShorter ? viaLonghands[1] : viaShorthand[1];
		  var newComponents = isLonghandsShorter ? viaLonghands[2] : viaShorthand[2];
		  var all = candidateComponents[Object.keys(candidateComponents)[0]].all;
		  var componentName;
		  var oldComponent;
		  var newComponent;
		  var newToken;

		  newProperty.position = all.length;
		  newProperty.shorthand = true;
		  newProperty.dirty = true;
		  newProperty.all = all;
		  newProperty.all.push(newTokensSequence[0]);

		  properties.push(newProperty);

		  for (componentName in candidateComponents) {
			oldComponent = candidateComponents[componentName];
			oldComponent.unused = true;

			if (oldComponent.name in newComponents) {
			  newComponent = newComponents[oldComponent.name];
			  newToken = findTokenIn(newTokensSequence, componentName);

			  newComponent.position = all.length;
			  newComponent.all = all;
			  newComponent.all.push(newToken);

			  properties.push(newComponent);
			}
		  }
		}

		function buildSequenceWithInheritLonghands(components, shorthandName, validator) {
		  var tokensSequence = [];
		  var inheritComponents = {};
		  var nonInheritComponents = {};
		  var descriptor = compactable[shorthandName];
		  var shorthandToken = [
			Token.PROPERTY,
			[Token.PROPERTY_NAME, shorthandName],
			[Token.PROPERTY_VALUE, descriptor.defaultValue]
		  ];
		  var newProperty = wrapSingle(shorthandToken);
		  var component;
		  var longhandToken;
		  var newComponent;
		  var nameMetadata;
		  var i, l;

		  populateComponents([newProperty], validator, []);

		  for (i = 0, l = descriptor.components.length; i < l; i++) {
			component = components[descriptor.components[i]];

			if (hasInherit(component)) {
			  longhandToken = component.all[component.position].slice(0, 2);
			  Array.prototype.push.apply(longhandToken, component.value);
			  tokensSequence.push(longhandToken);

			  newComponent = deepClone(component);
			  newComponent.value = inferComponentValue(components, newComponent.name);

			  newProperty.components[i] = newComponent;
			  inheritComponents[component.name] = deepClone(component);
			} else {
			  newComponent = deepClone(component);
			  newComponent.all = component.all;
			  newProperty.components[i] = newComponent;

			  nonInheritComponents[component.name] = component;
			}
		  }

		  nameMetadata = joinMetadata(nonInheritComponents, 1);
		  shorthandToken[1].push(nameMetadata);

		  restoreFromOptimizing([newProperty], restoreWithComponents);

		  shorthandToken = shorthandToken.slice(0, 2);
		  Array.prototype.push.apply(shorthandToken, newProperty.value);

		  tokensSequence.unshift(shorthandToken);

		  return [tokensSequence, newProperty, inheritComponents];
		}

		function inferComponentValue(components, propertyName) {
		  var descriptor = compactable[propertyName];

		  if ('oppositeTo' in descriptor) {
			return components[descriptor.oppositeTo].value;
		  } else {
			return [[Token.PROPERTY_VALUE, descriptor.defaultValue]];
		  }
		}

		function joinMetadata(components, at) {
		  var metadata = [];
		  var component;
		  var originalValue;
		  var componentMetadata;
		  var componentName;

		  for (componentName in components) {
			component = components[componentName];
			originalValue = component.all[component.position];
			componentMetadata = originalValue[at][originalValue[at].length - 1];

			Array.prototype.push.apply(metadata, componentMetadata);
		  }

		  return metadata.sort(metadataSorter);
		}

		function metadataSorter(metadata1, metadata2) {
		  var line1 = metadata1[0];
		  var line2 = metadata2[0];
		  var column1 = metadata1[1];
		  var column2 = metadata2[1];

		  if (line1 < line2) {
			return -1;
		  } else if (line1 === line2) {
			return column1 < column2 ? -1 : 1;
		  } else {
			return 1;
		  }
		}

		function buildSequenceWithInheritShorthand(components, shorthandName, validator) {
		  var tokensSequence = [];
		  var inheritComponents = {};
		  var nonInheritComponents = {};
		  var descriptor = compactable[shorthandName];
		  var shorthandToken = [
			Token.PROPERTY,
			[Token.PROPERTY_NAME, shorthandName],
			[Token.PROPERTY_VALUE, 'inherit']
		  ];
		  var newProperty = wrapSingle(shorthandToken);
		  var component;
		  var longhandToken;
		  var nameMetadata;
		  var valueMetadata;
		  var i, l;

		  populateComponents([newProperty], validator, []);

		  for (i = 0, l = descriptor.components.length; i < l; i++) {
			component = components[descriptor.components[i]];

			if (hasInherit(component)) {
			  inheritComponents[component.name] = component;
			} else {
			  longhandToken = component.all[component.position].slice(0, 2);
			  Array.prototype.push.apply(longhandToken, component.value);
			  tokensSequence.push(longhandToken);

			  nonInheritComponents[component.name] = deepClone(component);
			}
		  }

		  nameMetadata = joinMetadata(inheritComponents, 1);
		  shorthandToken[1].push(nameMetadata);

		  valueMetadata = joinMetadata(inheritComponents, 2);
		  shorthandToken[2].push(valueMetadata);

		  tokensSequence.unshift(shorthandToken);

		  return [tokensSequence, newProperty, nonInheritComponents];
		}

		function findTokenIn(tokens, componentName) {
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			if (tokens[i][1][1] == componentName) {
			  return tokens[i];
			}
		  }
		}

		function replaceWithShorthand(properties, candidateComponents, shorthandName, validator) {
		  var descriptor = compactable[shorthandName];
		  var nameMetadata;
		  var valueMetadata;
		  var newValuePlaceholder = [
			Token.PROPERTY,
			[Token.PROPERTY_NAME, shorthandName],
			[Token.PROPERTY_VALUE, descriptor.defaultValue]
		  ];
		  var all;

		  var newProperty = wrapSingle(newValuePlaceholder);
		  newProperty.shorthand = true;
		  newProperty.dirty = true;

		  populateComponents([newProperty], validator, []);

		  for (var i = 0, l = descriptor.components.length; i < l; i++) {
			var component = candidateComponents[descriptor.components[i]];

			newProperty.components[i] = deepClone(component);
			newProperty.important = component.important;

			all = component.all;
		  }

		  for (var componentName in candidateComponents) {
			candidateComponents[componentName].unused = true;
		  }

		  nameMetadata = joinMetadata(candidateComponents, 1);
		  newValuePlaceholder[1].push(nameMetadata);

		  valueMetadata = joinMetadata(candidateComponents, 2);
		  newValuePlaceholder[2].push(valueMetadata);

		  newProperty.position = all.length;
		  newProperty.all = all;
		  newProperty.all.push(newValuePlaceholder);

		  properties.push(newProperty);
		}

		return mergeIntoShorthands;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/optimize
	modules['/optimizer/level-2/properties/optimize'] = function () {
		var mergeIntoShorthands = require('/optimizer/level-2/properties/merge-into-shorthands');
		var overrideProperties = require('/optimizer/level-2/properties/override-properties');
		var populateComponents = require('/optimizer/level-2/properties/populate-components');

		var restoreWithComponents = require('/optimizer/level-2/restore-with-components');

		var wrapForOptimizing = require('/optimizer/wrap-for-optimizing').all;
		var removeUnused = require('/optimizer/remove-unused');
		var restoreFromOptimizing = require('/optimizer/restore-from-optimizing');

		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;

		function optimizeProperties(properties, withOverriding, withMerging, context) {
		  var levelOptions = context.options.level[OptimizationLevel.Two];
		  var _properties = wrapForOptimizing(properties, false, levelOptions.skipProperties);
		  var _property;
		  var i, l;

		  populateComponents(_properties, context.validator, context.warnings);

		  for (i = 0, l = _properties.length; i < l; i++) {
			_property = _properties[i];
			if (_property.block) {
			  optimizeProperties(_property.value[0][1], withOverriding, withMerging, context);
			}
		  }

		  if (withMerging && levelOptions.mergeIntoShorthands) {
			mergeIntoShorthands(_properties, context.validator);
		  }

		  if (withOverriding && levelOptions.overrideProperties) {
			overrideProperties(_properties, withMerging, context.options.compatibility, context.validator);
		  }

		  restoreFromOptimizing(_properties, restoreWithComponents);
		  removeUnused(_properties);
		}

		return optimizeProperties;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/override-properties
	modules['/optimizer/level-2/properties/override-properties'] = function () {
		var hasInherit = require('/optimizer/level-2/properties/has-inherit');
		var everyValuesPair = require('/optimizer/level-2/properties/every-values-pair');
		var findComponentIn = require('/optimizer/level-2/properties/find-component-in');
		var isComponentOf = require('/optimizer/level-2/properties/is-component-of');
		var isMergeableShorthand = require('/optimizer/level-2/properties/is-mergeable-shorthand');
		var overridesNonComponentShorthand = require('/optimizer/level-2/properties/overrides-non-component-shorthand');
		var sameVendorPrefixesIn = require('/optimizer/level-2/properties/vendor-prefixes').same;

		var compactable = require('/optimizer/level-2/compactable');
		var deepClone = require('/optimizer/level-2/clone').deep;
		var deepClone = require('/optimizer/level-2/clone').deep;
		var restoreWithComponents = require('/optimizer/level-2/restore-with-components');
		var shallowClone = require('/optimizer/level-2/clone').shallow;

		var restoreFromOptimizing = require('/optimizer/restore-from-optimizing');

		var Token = require('/tokenizer/token');
		var Marker = require('/tokenizer/marker');

		var serializeProperty = require('/writer/one-time').property;

		function wouldBreakCompatibility(property, validator) {
		  for (var i = 0; i < property.components.length; i++) {
			var component = property.components[i];
			var descriptor = compactable[component.name];
			var canOverride = descriptor && descriptor.canOverride || canOverride.sameValue;

			var _component = shallowClone(component);
			_component.value = [[Token.PROPERTY_VALUE, descriptor.defaultValue]];

			if (!everyValuesPair(canOverride.bind(null, validator), _component, component)) {
			  return true;
			}
		  }

		  return false;
		}

		function overrideIntoMultiplex(property, by) {
		  by.unused = true;

		  turnIntoMultiplex(by, multiplexSize(property));
		  property.value = by.value;
		}

		function overrideByMultiplex(property, by) {
		  by.unused = true;
		  property.multiplex = true;
		  property.value = by.value;
		}

		function overrideSimple(property, by) {
		  by.unused = true;
		  property.value = by.value;
		}

		function override(property, by) {
		  if (by.multiplex)
			overrideByMultiplex(property, by);
		  else if (property.multiplex)
			overrideIntoMultiplex(property, by);
		  else
			overrideSimple(property, by);
		}

		function overrideShorthand(property, by) {
		  by.unused = true;

		  for (var i = 0, l = property.components.length; i < l; i++) {
			override(property.components[i], by.components[i], property.multiplex);
		  }
		}

		function turnIntoMultiplex(property, size) {
		  property.multiplex = true;

		  if (compactable[property.name].shorthand) {
			turnShorthandValueIntoMultiplex(property, size);
		  } else {
			turnLonghandValueIntoMultiplex(property, size);
		  }
		}

		function turnShorthandValueIntoMultiplex(property, size) {
		  var component;
		  var i, l;

		  for (i = 0, l = property.components.length; i < l; i++) {
			component = property.components[i];

			if (!component.multiplex) {
			  turnLonghandValueIntoMultiplex(component, size);
			}
		  }
		}

		function turnLonghandValueIntoMultiplex(property, size) {
		  var withRealValue = compactable[property.name].intoMultiplexMode == 'real';
		  var withValue = withRealValue ?
			property.value.slice(0) :
			compactable[property.name].defaultValue;
		  var i = multiplexSize(property);
		  var j;
		  var m = withValue.length;

		  for (; i < size; i++) {
			property.value.push([Token.PROPERTY_VALUE, Marker.COMMA]);

			if (Array.isArray(withValue)) {
			  for (j = 0; j < m; j++) {
				property.value.push(withRealValue ? withValue[j] : [Token.PROPERTY_VALUE, withValue[j]]);
			  }
			} else {
			  property.value.push(withRealValue ? withValue : [Token.PROPERTY_VALUE, withValue]);
			}
		  }
		}

		function multiplexSize(component) {
		  var size = 0;

		  for (var i = 0, l = component.value.length; i < l; i++) {
			if (component.value[i][1] == Marker.COMMA)
			  size++;
		  }

		  return size + 1;
		}

		function lengthOf(property) {
		  var fakeAsArray = [
			Token.PROPERTY,
			[Token.PROPERTY_NAME, property.name]
		  ].concat(property.value);
		  return serializeProperty([fakeAsArray], 0).length;
		}

		function moreSameShorthands(properties, startAt, name) {
		  // Since we run the main loop in `compactOverrides` backwards, at this point some
		  // properties may not be marked as unused.
		  // We should consider reverting the order if possible
		  var count = 0;

		  for (var i = startAt; i >= 0; i--) {
			if (properties[i].name == name && !properties[i].unused)
			  count++;
			if (count > 1)
			  break;
		  }

		  return count > 1;
		}

		function overridingFunction(shorthand, validator) {
		  for (var i = 0, l = shorthand.components.length; i < l; i++) {
			if (!anyValue(validator.isUrl, shorthand.components[i]) && anyValue(validator.isFunction, shorthand.components[i])) {
			  return true;
			}
		  }

		  return false;
		}

		function anyValue(fn, property) {
		  for (var i = 0, l = property.value.length; i < l; i++) {
			if (property.value[i][1] == Marker.COMMA)
			  continue;

			if (fn(property.value[i][1]))
			  return true;
		  }

		  return false;
		}

		function wouldResultInLongerValue(left, right) {
		  if (!left.multiplex && !right.multiplex || left.multiplex && right.multiplex)
			return false;

		  var multiplex = left.multiplex ? left : right;
		  var simple = left.multiplex ? right : left;
		  var component;

		  var multiplexClone = deepClone(multiplex);
		  restoreFromOptimizing([multiplexClone], restoreWithComponents);

		  var simpleClone = deepClone(simple);
		  restoreFromOptimizing([simpleClone], restoreWithComponents);

		  var lengthBefore = lengthOf(multiplexClone) + 1 + lengthOf(simpleClone);

		  if (left.multiplex) {
			component = findComponentIn(multiplexClone, simpleClone);
			overrideIntoMultiplex(component, simpleClone);
		  } else {
			component = findComponentIn(simpleClone, multiplexClone);
			turnIntoMultiplex(simpleClone, multiplexSize(multiplexClone));
			overrideByMultiplex(component, multiplexClone);
		  }

		  restoreFromOptimizing([simpleClone], restoreWithComponents);

		  var lengthAfter = lengthOf(simpleClone);

		  return lengthBefore <= lengthAfter;
		}

		function isCompactable(property) {
		  return property.name in compactable;
		}

		function noneOverrideHack(left, right) {
		  return !left.multiplex &&
			(left.name == 'background' || left.name == 'background-image') &&
			right.multiplex &&
			(right.name == 'background' || right.name == 'background-image') &&
			anyLayerIsNone(right.value);
		}

		function anyLayerIsNone(values) {
		  var layers = intoLayers(values);

		  for (var i = 0, l = layers.length; i < l; i++) {
			if (layers[i].length == 1 && layers[i][0][1] == 'none')
			  return true;
		  }

		  return false;
		}

		function intoLayers(values) {
		  var layers = [];

		  for (var i = 0, layer = [], l = values.length; i < l; i++) {
			var value = values[i];
			if (value[1] == Marker.COMMA) {
			  layers.push(layer);
			  layer = [];
			} else {
			  layer.push(value);
			}
		  }

		  layers.push(layer);
		  return layers;
		}

		function overrideProperties(properties, withMerging, compatibility, validator) {
		  var mayOverride, right, left, component;
		  var overriddenComponents;
		  var overriddenComponent;
		  var overridingComponent;
		  var overridable;
		  var i, j, k;

		  propertyLoop:
		  for (i = properties.length - 1; i >= 0; i--) {
			right = properties[i];

			if (!isCompactable(right))
			  continue;

			if (right.block)
			  continue;

			mayOverride = compactable[right.name].canOverride;

			traverseLoop:
			for (j = i - 1; j >= 0; j--) {
			  left = properties[j];

			  if (!isCompactable(left))
				continue;

			  if (left.block)
				continue;

			  if (left.unused || right.unused)
				continue;

			  if (left.hack && !right.hack && !right.important || !left.hack && !left.important && right.hack)
				continue;

			  if (left.important == right.important && left.hack[0] != right.hack[0])
				continue;

			  if (left.important == right.important && (left.hack[0] != right.hack[0] || (left.hack[1] && left.hack[1] != right.hack[1])))
				continue;

			  if (hasInherit(right))
				continue;

			  if (noneOverrideHack(left, right))
				continue;

			  if (right.shorthand && isComponentOf(right, left)) {
				// maybe `left` can be overridden by `right` which is a shorthand?
				if (!right.important && left.important)
				  continue;

				if (!sameVendorPrefixesIn([left], right.components))
				  continue;

				if (!anyValue(validator.isFunction, left) && overridingFunction(right, validator))
				  continue;

				if (!isMergeableShorthand(right)) {
				  left.unused = true;
				  continue;
				}

				component = findComponentIn(right, left);
				mayOverride = compactable[left.name].canOverride;
				if (everyValuesPair(mayOverride.bind(null, validator), left, component)) {
				  left.unused = true;
				}
			  } else if (right.shorthand && overridesNonComponentShorthand(right, left)) {
				// `right` is a shorthand while `left` can be overriden by it, think `border` and `border-top`
				if (!right.important && left.important) {
				  continue;
				}

				if (!sameVendorPrefixesIn([left], right.components)) {
				  continue;
				}

				if (!anyValue(validator.isFunction, left) && overridingFunction(right, validator)) {
				  continue;
				}

				overriddenComponents = left.shorthand ?
				  left.components:
				  [left];

				for (k = overriddenComponents.length - 1; k >= 0; k--) {
				  overriddenComponent = overriddenComponents[k];
				  overridingComponent = findComponentIn(right, overriddenComponent);
				  mayOverride = compactable[overriddenComponent.name].canOverride;

				  if (!everyValuesPair(mayOverride.bind(null, validator), left, overridingComponent)) {
					continue traverseLoop;
				  }
				}

				left.unused = true;
			  } else if (withMerging && left.shorthand && !right.shorthand && isComponentOf(left, right, true)) {
				// maybe `right` can be pulled into `left` which is a shorthand?
				if (right.important && !left.important)
				  continue;

				if (!right.important && left.important) {
				  right.unused = true;
				  continue;
				}

				// Pending more clever algorithm in #527
				if (moreSameShorthands(properties, i - 1, left.name))
				  continue;

				if (overridingFunction(left, validator))
				  continue;

				if (!isMergeableShorthand(left))
				  continue;

				component = findComponentIn(left, right);
				if (everyValuesPair(mayOverride.bind(null, validator), component, right)) {
				  var disabledBackgroundMerging =
					!compatibility.properties.backgroundClipMerging && component.name.indexOf('background-clip') > -1 ||
					!compatibility.properties.backgroundOriginMerging && component.name.indexOf('background-origin') > -1 ||
					!compatibility.properties.backgroundSizeMerging && component.name.indexOf('background-size') > -1;
				  var nonMergeableValue = compactable[right.name].nonMergeableValue === right.value[0][1];

				  if (disabledBackgroundMerging || nonMergeableValue)
					continue;

				  if (!compatibility.properties.merging && wouldBreakCompatibility(left, validator))
					continue;

				  if (component.value[0][1] != right.value[0][1] && (hasInherit(left) || hasInherit(right)))
					continue;

				  if (wouldResultInLongerValue(left, right))
					continue;

				  if (!left.multiplex && right.multiplex)
					turnIntoMultiplex(left, multiplexSize(right));

				  override(component, right);
				  left.dirty = true;
				}
			  } else if (withMerging && left.shorthand && right.shorthand && left.name == right.name) {
				// merge if all components can be merged

				if (!left.multiplex && right.multiplex)
				  continue;

				if (!right.important && left.important) {
				  right.unused = true;
				  continue propertyLoop;
				}

				if (right.important && !left.important) {
				  left.unused = true;
				  continue;
				}

				if (!isMergeableShorthand(right)) {
				  left.unused = true;
				  continue;
				}

				for (k = left.components.length - 1; k >= 0; k--) {
				  var leftComponent = left.components[k];
				  var rightComponent = right.components[k];

				  mayOverride = compactable[leftComponent.name].canOverride;
				  if (!everyValuesPair(mayOverride.bind(null, validator), leftComponent, rightComponent))
					continue propertyLoop;
				}

				overrideShorthand(left, right);
				left.dirty = true;
			  } else if (withMerging && left.shorthand && right.shorthand && isComponentOf(left, right)) {
				// border is a shorthand but any of its components is a shorthand too

				if (!left.important && right.important)
				  continue;

				component = findComponentIn(left, right);
				mayOverride = compactable[right.name].canOverride;
				if (!everyValuesPair(mayOverride.bind(null, validator), component, right))
				  continue;

				if (left.important && !right.important) {
				  right.unused = true;
				  continue;
				}

				var rightRestored = compactable[right.name].restore(right, compactable);
				if (rightRestored.length > 1)
				  continue;

				component = findComponentIn(left, right);
				override(component, right);
				right.dirty = true;
			  } else if (left.name == right.name) {
				// two non-shorthands should be merged based on understandability
				overridable = true;

				if (right.shorthand) {
				  for (k = right.components.length - 1; k >= 0 && overridable; k--) {
					overriddenComponent = left.components[k];
					overridingComponent = right.components[k];
					mayOverride = compactable[overridingComponent.name].canOverride;

					overridable = overridable && everyValuesPair(mayOverride.bind(null, validator), overriddenComponent, overridingComponent);
				  }
				} else {
				  mayOverride = compactable[right.name].canOverride;
				  overridable = everyValuesPair(mayOverride.bind(null, validator), left, right);
				}

				if (left.important && !right.important && overridable) {
				  right.unused = true;
				  continue;
				}

				if (!left.important && right.important && overridable) {
				  left.unused = true;
				  continue;
				}

				if (!overridable) {
				  continue;
				}

				left.unused = true;
			  }
			}
		  }
		}

		return overrideProperties;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/overrides-non-component-shorthand
	modules['/optimizer/level-2/properties/overrides-non-component-shorthand'] = function () {
		var compactable = require('/optimizer/level-2/compactable');

		function overridesNonComponentShorthand(property1, property2) {
		  return property1.name in compactable &&
			'overridesShorthands' in compactable[property1.name] &&
			compactable[property1.name].overridesShorthands.indexOf(property2.name) > -1;
		}

		return overridesNonComponentShorthand;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/populate-components
	modules['/optimizer/level-2/properties/populate-components'] = function () {
		var compactable = require('/optimizer/level-2/compactable');
		var InvalidPropertyError = require('/optimizer/level-2/invalid-property-error');

		function populateComponents(properties, validator, warnings) {
		  var component;
		  var j, m;

		  for (var i = properties.length - 1; i >= 0; i--) {
			var property = properties[i];
			var descriptor = compactable[property.name];

			if (descriptor && descriptor.shorthand) {
			  property.shorthand = true;
			  property.dirty = true;

			  try {
				property.components = descriptor.breakUp(property, compactable, validator);

				if (descriptor.shorthandComponents) {
				  for (j = 0, m = property.components.length; j < m; j++) {
					component = property.components[j];
					component.components = compactable[component.name].breakUp(component, compactable, validator);
				  }
				}
			  } catch (e) {
				if (e instanceof InvalidPropertyError) {
				  property.components = []; // this will set property.unused to true below
				  warnings.push(e.message);
				} else {
				  throw e;
				}
			  }

			  if (property.components.length > 0)
				property.multiplex = property.components[0].multiplex;
			  else
				property.unused = true;
			}
		  }
		}

		return populateComponents;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/understandable
	modules['/optimizer/level-2/properties/understandable'] = function () {
		var sameVendorPrefixes = require('/optimizer/level-2/properties/vendor-prefixes').same;

		function understandable(validator, value1, value2, _position, isPaired) {
		  if (!sameVendorPrefixes(value1, value2)) {
			return false;
		  }

		  if (isPaired && validator.isVariable(value1) !== validator.isVariable(value2)) {
			return false;
		  }

		  return true;
		}

		return understandable;
	};
	//#endregion

	//#region URL: /optimizer/level-2/properties/vendor-prefixes
	modules['/optimizer/level-2/properties/vendor-prefixes'] = function () {
		var VENDOR_PREFIX_PATTERN = /(?:^|\W)(\-\w+\-)/g;

		function unique(value) {
		  var prefixes = [];
		  var match;

		  while ((match = VENDOR_PREFIX_PATTERN.exec(value)) !== null) {
			if (prefixes.indexOf(match[0]) == -1) {
			  prefixes.push(match[0]);
			}
		  }

		  return prefixes;
		}

		function same(value1, value2) {
		  return unique(value1).sort().join(',') == unique(value2).sort().join(',');
		}

		var exports = {
		  unique: unique,
		  same: same
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/break-up
	modules['/optimizer/level-2/break-up'] = function () {
		var InvalidPropertyError = require('/optimizer/level-2/invalid-property-error');

		var wrapSingle = require('/optimizer/wrap-for-optimizing').single;

		var Token = require('/tokenizer/token');
		var Marker = require('/tokenizer/marker');

		var formatPosition = require('/utils/format-position');

		function _anyIsInherit(values) {
		  var i, l;

		  for (i = 0, l = values.length; i < l; i++) {
			if (values[i][1] == 'inherit') {
			  return true;
			}
		  }

		  return false;
		}

		function _colorFilter(validator) {
		  return function (value) {
			return value[1] == 'invert' || validator.isColor(value[1]) || validator.isPrefixed(value[1]);
		  };
		}

		function _styleFilter(validator) {
		  return function (value) {
			return value[1] != 'inherit' && validator.isStyleKeyword(value[1]) && !validator.isColorFunction(value[1]);
		  };
		}

		function _wrapDefault(name, property, compactable) {
		  var descriptor = compactable[name];
		  if (descriptor.doubleValues && descriptor.defaultValue.length == 2) {
			return wrapSingle([
			  Token.PROPERTY,
			  [Token.PROPERTY_NAME, name],
			  [Token.PROPERTY_VALUE, descriptor.defaultValue[0]],
			  [Token.PROPERTY_VALUE, descriptor.defaultValue[1]]
			]);
		  } else if (descriptor.doubleValues && descriptor.defaultValue.length == 1) {
			return wrapSingle([
			  Token.PROPERTY,
			  [Token.PROPERTY_NAME, name],
			  [Token.PROPERTY_VALUE, descriptor.defaultValue[0]]
			]);
		  } else {
			return wrapSingle([
			  Token.PROPERTY,
			  [Token.PROPERTY_NAME, name],
			  [Token.PROPERTY_VALUE, descriptor.defaultValue]
			]);
		  }
		}

		function _widthFilter(validator) {
		  return function (value) {
			return value[1] != 'inherit' &&
			  (validator.isWidth(value[1]) || validator.isUnit(value[1]) && !validator.isDynamicUnit(value[1])) &&
			  !validator.isStyleKeyword(value[1]) &&
			  !validator.isColorFunction(value[1]);
		  };
		}

		function animation(property, compactable, validator) {
		  var duration = _wrapDefault(property.name + '-duration', property, compactable);
		  var timing = _wrapDefault(property.name + '-timing-function', property, compactable);
		  var delay = _wrapDefault(property.name + '-delay', property, compactable);
		  var iteration = _wrapDefault(property.name + '-iteration-count', property, compactable);
		  var direction = _wrapDefault(property.name + '-direction', property, compactable);
		  var fill = _wrapDefault(property.name + '-fill-mode', property, compactable);
		  var play = _wrapDefault(property.name + '-play-state', property, compactable);
		  var name = _wrapDefault(property.name + '-name', property, compactable);
		  var components = [duration, timing, delay, iteration, direction, fill, play, name];
		  var values = property.value;
		  var value;
		  var durationSet = false;
		  var timingSet = false;
		  var delaySet = false;
		  var iterationSet = false;
		  var directionSet = false;
		  var fillSet = false;
		  var playSet = false;
		  var nameSet = false;
		  var i;
		  var l;

		  if (property.value.length == 1 && property.value[0][1] == 'inherit') {
			duration.value = timing.value = delay.value = iteration.value = direction.value = fill.value = play.value = name.value = property.value;
			return components;
		  }

		  if (values.length > 1 && _anyIsInherit(values)) {
			throw new InvalidPropertyError('Invalid animation values at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  for (i = 0, l = values.length; i < l; i++) {
			value = values[i];

			if (validator.isTime(value[1]) && !durationSet) {
			  duration.value = [value];
			  durationSet = true;
			} else if (validator.isTime(value[1]) && !delaySet) {
			  delay.value = [value];
			  delaySet = true;
			} else if ((validator.isGlobal(value[1]) || validator.isAnimationTimingFunction(value[1])) && !timingSet) {
			  timing.value = [value];
			  timingSet = true;
			} else if ((validator.isAnimationIterationCountKeyword(value[1]) || validator.isPositiveNumber(value[1])) && !iterationSet) {
			  iteration.value = [value];
			  iterationSet = true;
			} else if (validator.isAnimationDirectionKeyword(value[1]) && !directionSet) {
			  direction.value = [value];
			  directionSet = true;
			} else if (validator.isAnimationFillModeKeyword(value[1]) && !fillSet) {
			  fill.value = [value];
			  fillSet = true;
			} else if (validator.isAnimationPlayStateKeyword(value[1]) && !playSet) {
			  play.value = [value];
			  playSet = true;
			} else if ((validator.isAnimationNameKeyword(value[1]) || validator.isIdentifier(value[1])) && !nameSet) {
			  name.value = [value];
			  nameSet = true;
			} else {
			  throw new InvalidPropertyError('Invalid animation value at ' + formatPosition(value[2][0]) + '. Ignoring.');
			}
		  }

		  return components;
		}

		function background(property, compactable, validator) {
		  var image = _wrapDefault('background-image', property, compactable);
		  var position = _wrapDefault('background-position', property, compactable);
		  var size = _wrapDefault('background-size', property, compactable);
		  var repeat = _wrapDefault('background-repeat', property, compactable);
		  var attachment = _wrapDefault('background-attachment', property, compactable);
		  var origin = _wrapDefault('background-origin', property, compactable);
		  var clip = _wrapDefault('background-clip', property, compactable);
		  var color = _wrapDefault('background-color', property, compactable);
		  var components = [image, position, size, repeat, attachment, origin, clip, color];
		  var values = property.value;

		  var positionSet = false;
		  var clipSet = false;
		  var originSet = false;
		  var repeatSet = false;

		  var anyValueSet = false;

		  if (property.value.length == 1 && property.value[0][1] == 'inherit') {
			// NOTE: 'inherit' is not a valid value for background-attachment
			color.value = image.value =  repeat.value = position.value = size.value = origin.value = clip.value = property.value;
			return components;
		  }

		  if (property.value.length == 1 && property.value[0][1] == '0 0') {
			return components;
		  }

		  for (var i = values.length - 1; i >= 0; i--) {
			var value = values[i];

			if (validator.isBackgroundAttachmentKeyword(value[1])) {
			  attachment.value = [value];
			  anyValueSet = true;
			} else if (validator.isBackgroundClipKeyword(value[1]) || validator.isBackgroundOriginKeyword(value[1])) {
			  if (clipSet) {
				origin.value = [value];
				originSet = true;
			  } else {
				clip.value = [value];
				clipSet = true;
			  }
			  anyValueSet = true;
			} else if (validator.isBackgroundRepeatKeyword(value[1])) {
			  if (repeatSet) {
				repeat.value.unshift(value);
			  } else {
				repeat.value = [value];
				repeatSet = true;
			  }
			  anyValueSet = true;
			} else if (validator.isBackgroundPositionKeyword(value[1]) || validator.isBackgroundSizeKeyword(value[1]) || validator.isUnit(value[1]) || validator.isDynamicUnit(value[1])) {
			  if (i > 0) {
				var previousValue = values[i - 1];

				if (previousValue[1] == Marker.FORWARD_SLASH) {
				  size.value = [value];
				} else if (i > 1 && values[i - 2][1] == Marker.FORWARD_SLASH) {
				  size.value = [previousValue, value];
				  i -= 2;
				} else {
				  if (!positionSet)
					position.value = [];

				  position.value.unshift(value);
				  positionSet = true;
				}
			  } else {
				if (!positionSet)
				  position.value = [];

				position.value.unshift(value);
				positionSet = true;
			  }
			  anyValueSet = true;
			} else if ((color.value[0][1] == compactable[color.name].defaultValue || color.value[0][1] == 'none') && (validator.isColor(value[1]) || validator.isPrefixed(value[1]))) {
			  color.value = [value];
			  anyValueSet = true;
			} else if (validator.isUrl(value[1]) || validator.isFunction(value[1])) {
			  image.value = [value];
			  anyValueSet = true;
			}
		  }

		  if (clipSet && !originSet)
			origin.value = clip.value.slice(0);

		  if (!anyValueSet) {
			throw new InvalidPropertyError('Invalid background value at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  return components;
		}

		function borderRadius(property, compactable) {
		  var values = property.value;
		  var splitAt = -1;

		  for (var i = 0, l = values.length; i < l; i++) {
			if (values[i][1] == Marker.FORWARD_SLASH) {
			  splitAt = i;
			  break;
			}
		  }

		  if (splitAt === 0 || splitAt === values.length - 1) {
			throw new InvalidPropertyError('Invalid border-radius value at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  var target = _wrapDefault(property.name, property, compactable);
		  target.value = splitAt > -1 ?
			values.slice(0, splitAt) :
			values.slice(0);
		  target.components = fourValues(target, compactable);

		  var remainder = _wrapDefault(property.name, property, compactable);
		  remainder.value = splitAt > -1 ?
			values.slice(splitAt + 1) :
			values.slice(0);
		  remainder.components = fourValues(remainder, compactable);

		  for (var j = 0; j < 4; j++) {
			target.components[j].multiplex = true;
			target.components[j].value = target.components[j].value.concat(remainder.components[j].value);
		  }

		  return target.components;
		}

		function font(property, compactable, validator) {
		  var style = _wrapDefault('font-style', property, compactable);
		  var variant = _wrapDefault('font-variant', property, compactable);
		  var weight = _wrapDefault('font-weight', property, compactable);
		  var stretch = _wrapDefault('font-stretch', property, compactable);
		  var size = _wrapDefault('font-size', property, compactable);
		  var height = _wrapDefault('line-height', property, compactable);
		  var family = _wrapDefault('font-family', property, compactable);
		  var components = [style, variant, weight, stretch, size, height, family];
		  var values = property.value;
		  var fuzzyMatched = 4; // style, variant, weight, and stretch
		  var index = 0;
		  var isStretchSet = false;
		  var isStretchValid;
		  var isStyleSet = false;
		  var isStyleValid;
		  var isVariantSet = false;
		  var isVariantValid;
		  var isWeightSet = false;
		  var isWeightValid;
		  var isSizeSet = false;
		  var appendableFamilyName = false;

		  if (!values[index]) {
			throw new InvalidPropertyError('Missing font values at ' + formatPosition(property.all[property.position][1][2][0]) + '. Ignoring.');
		  }

		  if (values.length == 1 && values[0][1] == 'inherit') {
			style.value = variant.value = weight.value = stretch.value = size.value = height.value = family.value = values;
			return components;
		  }

		  if (values.length == 1 && (validator.isFontKeyword(values[0][1]) || validator.isGlobal(values[0][1]) || validator.isPrefixed(values[0][1]))) {
			values[0][1] = Marker.INTERNAL + values[0][1];
			style.value = variant.value = weight.value = stretch.value = size.value = height.value = family.value = values;
			return components;
		  }

		  if (values.length < 2 || !_anyIsFontSize(values, validator) || !_anyIsFontFamily(values, validator)) {
			throw new InvalidPropertyError('Invalid font values at ' + formatPosition(property.all[property.position][1][2][0]) + '. Ignoring.');
		  }

		  if (values.length > 1 && _anyIsInherit(values)) {
			throw new InvalidPropertyError('Invalid font values at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  // fuzzy match style, variant, weight, and stretch on first elements
		  while (index < fuzzyMatched) {
			isStretchValid = validator.isFontStretchKeyword(values[index][1]) || validator.isGlobal(values[index][1]);
			isStyleValid = validator.isFontStyleKeyword(values[index][1]) || validator.isGlobal(values[index][1]);
			isVariantValid = validator.isFontVariantKeyword(values[index][1]) || validator.isGlobal(values[index][1]);
			isWeightValid = validator.isFontWeightKeyword(values[index][1]) || validator.isGlobal(values[index][1]);

			if (isStyleValid && !isStyleSet) {
			  style.value = [values[index]];
			  isStyleSet = true;
			} else if (isVariantValid && !isVariantSet) {
			  variant.value = [values[index]];
			  isVariantSet = true;
			} else if (isWeightValid && !isWeightSet) {
			  weight.value = [values[index]];
			  isWeightSet = true;
			} else if (isStretchValid && !isStretchSet) {
			  stretch.value = [values[index]];
			  isStretchSet = true;
			} else if (isStyleValid && isStyleSet || isVariantValid && isVariantSet || isWeightValid && isWeightSet || isStretchValid && isStretchSet) {
			  throw new InvalidPropertyError('Invalid font style / variant / weight / stretch value at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
			} else {
			  break;
			}

			index++;
		  }

		  // now comes font-size ...
		  if (validator.isFontSizeKeyword(values[index][1]) || validator.isUnit(values[index][1]) && !validator.isDynamicUnit(values[index][1])) {
			size.value = [values[index]];
			isSizeSet = true;
			index++;
		  } else {
			throw new InvalidPropertyError('Missing font size at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  if (!values[index]) {
			throw new InvalidPropertyError('Missing font family at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  // ... and perhaps line-height
		  if (isSizeSet && values[index] && values[index][1] == Marker.FORWARD_SLASH && values[index + 1] && (validator.isLineHeightKeyword(values[index + 1][1]) || validator.isUnit(values[index + 1][1]) || validator.isNumber(values[index + 1][1]))) {
			height.value = [values[index + 1]];
			index++;
			index++;
		  }

		  // ... and whatever comes next is font-family
		  family.value = [];

		  while (values[index]) {
			if (values[index][1] == Marker.COMMA) {
			  appendableFamilyName = false;
			} else {
			  if (appendableFamilyName) {
				family.value[family.value.length - 1][1] += Marker.SPACE + values[index][1];
			  } else {
				family.value.push(values[index]);
			  }

			  appendableFamilyName = true;
			}

			index++;
		  }

		  if (family.value.length === 0) {
			throw new InvalidPropertyError('Missing font family at ' + formatPosition(values[0][2][0]) + '. Ignoring.');
		  }

		  return components;
		}

		function _anyIsFontSize(values, validator) {
		  var value;
		  var i, l;

		  for (i = 0, l = values.length; i < l; i++) {
			value = values[i];

			if (validator.isFontSizeKeyword(value[1]) || validator.isUnit(value[1]) && !validator.isDynamicUnit(value[1]) || validator.isFunction(value[1])) {
			  return true;
			}
		  }

		  return false;
		}

		function _anyIsFontFamily(values, validator) {
		  var value;
		  var i, l;

		  for (i = 0, l = values.length; i < l; i++) {
			value = values[i];

			if (validator.isIdentifier(value[1])) {
			  return true;
			}
		  }

		  return false;
		}

		function fourValues(property, compactable) {
		  var componentNames = compactable[property.name].components;
		  var components = [];
		  var value = property.value;

		  if (value.length < 1)
			return [];

		  if (value.length < 2)
			value[1] = value[0].slice(0);
		  if (value.length < 3)
			value[2] = value[0].slice(0);
		  if (value.length < 4)
			value[3] = value[1].slice(0);

		  for (var i = componentNames.length - 1; i >= 0; i--) {
			var component = wrapSingle([
			  Token.PROPERTY,
			  [Token.PROPERTY_NAME, componentNames[i]]
			]);
			component.value = [value[i]];
			components.unshift(component);
		  }

		  return components;
		}

		function multiplex(splitWith) {
		  return function (property, compactable, validator) {
			var splitsAt = [];
			var values = property.value;
			var i, j, l, m;

			// find split commas
			for (i = 0, l = values.length; i < l; i++) {
			  if (values[i][1] == ',')
				splitsAt.push(i);
			}

			if (splitsAt.length === 0)
			  return splitWith(property, compactable, validator);

			var splitComponents = [];

			// split over commas, and into components
			for (i = 0, l = splitsAt.length; i <= l; i++) {
			  var from = i === 0 ? 0 : splitsAt[i - 1] + 1;
			  var to = i < l ? splitsAt[i] : values.length;

			  var _property = _wrapDefault(property.name, property, compactable);
			  _property.value = values.slice(from, to);

			  splitComponents.push(splitWith(_property, compactable, validator));
			}

			var components = splitComponents[0];

			// group component values from each split
			for (i = 0, l = components.length; i < l; i++) {
			  components[i].multiplex = true;

			  for (j = 1, m = splitComponents.length; j < m; j++) {
				components[i].value.push([Token.PROPERTY_VALUE, Marker.COMMA]);
				Array.prototype.push.apply(components[i].value, splitComponents[j][i].value);
			  }
			}

			return components;
		  };
		}

		function listStyle(property, compactable, validator) {
		  var type = _wrapDefault('list-style-type', property, compactable);
		  var position = _wrapDefault('list-style-position', property, compactable);
		  var image = _wrapDefault('list-style-image', property, compactable);
		  var components = [type, position, image];

		  if (property.value.length == 1 && property.value[0][1] == 'inherit') {
			type.value = position.value = image.value = [property.value[0]];
			return components;
		  }

		  var values = property.value.slice(0);
		  var total = values.length;
		  var index = 0;

		  // `image` first...
		  for (index = 0, total = values.length; index < total; index++) {
			if (validator.isUrl(values[index][1]) || values[index][1] == '0') {
			  image.value = [values[index]];
			  values.splice(index, 1);
			  break;
			}
		  }

		  // ... then `position`
		  for (index = 0, total = values.length; index < total; index++) {
			if (validator.isListStylePositionKeyword(values[index][1])) {
			  position.value = [values[index]];
			  values.splice(index, 1);
			  break;
			}
		  }

		  // ... and what's left is a `type`
		  if (values.length > 0 && (validator.isListStyleTypeKeyword(values[0][1]) || validator.isIdentifier(values[0][1]))) {
			type.value = [values[0]];
		  }

		  return components;
		}

		function widthStyleColor(property, compactable, validator) {
		  var descriptor = compactable[property.name];
		  var components = [
			_wrapDefault(descriptor.components[0], property, compactable),
			_wrapDefault(descriptor.components[1], property, compactable),
			_wrapDefault(descriptor.components[2], property, compactable)
		  ];
		  var color, style, width;

		  for (var i = 0; i < 3; i++) {
			var component = components[i];

			if (component.name.indexOf('color') > 0)
			  color = component;
			else if (component.name.indexOf('style') > 0)
			  style = component;
			else
			  width = component;
		  }

		  if ((property.value.length == 1 && property.value[0][1] == 'inherit') ||
			  (property.value.length == 3 && property.value[0][1] == 'inherit' && property.value[1][1] == 'inherit' && property.value[2][1] == 'inherit')) {
			color.value = style.value = width.value = [property.value[0]];
			return components;
		  }

		  var values = property.value.slice(0);
		  var match, matches;

		  // NOTE: usually users don't follow the required order of parts in this shorthand,
		  // so we'll try to parse it caring as little about order as possible

		  if (values.length > 0) {
			matches = values.filter(_widthFilter(validator));
			match = matches.length > 1 && (matches[0][1] == 'none' || matches[0][1] == 'auto') ? matches[1] : matches[0];
			if (match) {
			  width.value = [match];
			  values.splice(values.indexOf(match), 1);
			}
		  }

		  if (values.length > 0) {
			match = values.filter(_styleFilter(validator))[0];
			if (match) {
			  style.value = [match];
			  values.splice(values.indexOf(match), 1);
			}
		  }

		  if (values.length > 0) {
			match = values.filter(_colorFilter(validator))[0];
			if (match) {
			  color.value = [match];
			  values.splice(values.indexOf(match), 1);
			}
		  }

		  return components;
		}

		var exports = {
		  animation: animation,
		  background: background,
		  border: widthStyleColor,
		  borderRadius: borderRadius,
		  font: font,
		  fourValues: fourValues,
		  listStyle: listStyle,
		  multiplex: multiplex,
		  outline: widthStyleColor
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/can-override
	modules['/optimizer/level-2/can-override'] = function () {
		var understandable = require('/optimizer/level-2/properties/understandable');

		function animationIterationCount(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isAnimationIterationCountKeyword(value2) || validator.isPositiveNumber(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  }

		  return validator.isAnimationIterationCountKeyword(value2) || validator.isPositiveNumber(value2);
		}

		function animationName(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isAnimationNameKeyword(value2) || validator.isIdentifier(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  }

		  return validator.isAnimationNameKeyword(value2) || validator.isIdentifier(value2);
		}

		function animationTimingFunction(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isAnimationTimingFunction(value2) || validator.isGlobal(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  }

		  return validator.isAnimationTimingFunction(value2) || validator.isGlobal(value2);
		}

		function areSameFunction(validator, value1, value2) {
		  if (!validator.isFunction(value1) || !validator.isFunction(value2)) {
			return false;
		  }

		  var function1Name = value1.substring(0, value1.indexOf('('));
		  var function2Name = value2.substring(0, value2.indexOf('('));

		  return function1Name === function2Name;
		}

		function backgroundPosition(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isBackgroundPositionKeyword(value2) || validator.isGlobal(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (validator.isBackgroundPositionKeyword(value2) || validator.isGlobal(value2)) {
			return true;
		  }

		  return unit(validator, value1, value2);
		}

		function backgroundSize(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isBackgroundSizeKeyword(value2) || validator.isGlobal(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (validator.isBackgroundSizeKeyword(value2) || validator.isGlobal(value2)) {
			return true;
		  }

		  return unit(validator, value1, value2);
		}

		function color(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !validator.isColor(value2)) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (!validator.colorOpacity && (validator.isRgbColor(value1) || validator.isHslColor(value1))) {
			return false;
		  } else if (!validator.colorOpacity && (validator.isRgbColor(value2) || validator.isHslColor(value2))) {
			return false;
		  } else if (validator.isColor(value1) && validator.isColor(value2)) {
			return true;
		  }

		  return sameFunctionOrValue(validator, value1, value2);
		}

		function components(overrideCheckers) {
		  return function (validator, value1, value2, position) {
			return overrideCheckers[position](validator, value1, value2);
		  };
		}

		function fontFamily(validator, value1, value2) {
		  return understandable(validator, value1, value2, 0, true);
		}

		function image(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !validator.isImage(value2)) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (validator.isImage(value2)) {
			return true;
		  } else if (validator.isImage(value1)) {
			return false;
		  }

		  return sameFunctionOrValue(validator, value1, value2);
		}

		function keyword(propertyName) {
		  return function(validator, value1, value2) {
			if (!understandable(validator, value1, value2, 0, true) && !validator.isKeyword(propertyName)(value2)) {
			  return false;
			} else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			  return true;
			}

			return validator.isKeyword(propertyName)(value2);
		  };
		}

		function keywordWithGlobal(propertyName) {
		  return function(validator, value1, value2) {
			if (!understandable(validator, value1, value2, 0, true) && !(validator.isKeyword(propertyName)(value2) || validator.isGlobal(value2))) {
			  return false;
			} else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			  return true;
			}

			return validator.isKeyword(propertyName)(value2) || validator.isGlobal(value2);
		  };
		}

		function sameFunctionOrValue(validator, value1, value2) {
		  return areSameFunction(validator, value1, value2) ?
			true :
			value1 === value2;
		}



		function textShadow(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isUnit(value2) || validator.isColor(value2) || validator.isGlobal(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  }

		  return validator.isUnit(value2) || validator.isColor(value2) || validator.isGlobal(value2);
		}

		function time(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !validator.isTime(value2)) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (validator.isTime(value1) && !validator.isTime(value2)) {
			return false;
		  } else if (validator.isTime(value2)) {
			return true;
		  } else if (validator.isTime(value1)) {
			return false;
		  } else if (validator.isFunction(value1) && !validator.isPrefixed(value1) && validator.isFunction(value2) && !validator.isPrefixed(value2)) {
			return true;
		  }

		  return sameFunctionOrValue(validator, value1, value2);
		}

		function unit(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !validator.isUnit(value2)) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if (validator.isUnit(value1) && !validator.isUnit(value2)) {
			return false;
		  } else if (validator.isUnit(value2)) {
			return true;
		  } else if (validator.isUnit(value1)) {
			return false;
		  } else if (validator.isFunction(value1) && !validator.isPrefixed(value1) && validator.isFunction(value2) && !validator.isPrefixed(value2)) {
			return true;
		  }

		  return sameFunctionOrValue(validator, value1, value2);
		}

		function unitOrKeywordWithGlobal(propertyName) {
		  var byKeyword = keywordWithGlobal(propertyName);

		  return function(validator, value1, value2) {
			return unit(validator, value1, value2) || byKeyword(validator, value1, value2);
		  };
		}

		function unitOrNumber(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !(validator.isUnit(value2) || validator.isNumber(value2))) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  } else if ((validator.isUnit(value1) || validator.isNumber(value1)) && !(validator.isUnit(value2) || validator.isNumber(value2))) {
			return false;
		  } else if (validator.isUnit(value2) || validator.isNumber(value2)) {
			return true;
		  } else if (validator.isUnit(value1) || validator.isNumber(value1)) {
			return false;
		  } else if (validator.isFunction(value1) && !validator.isPrefixed(value1) && validator.isFunction(value2) && !validator.isPrefixed(value2)) {
			return true;
		  }

		  return sameFunctionOrValue(validator, value1, value2);
		}

		function zIndex(validator, value1, value2) {
		  if (!understandable(validator, value1, value2, 0, true) && !validator.isZIndex(value2)) {
			return false;
		  } else if (validator.isVariable(value1) && validator.isVariable(value2)) {
			return true;
		  }

		  return validator.isZIndex(value2);
		}

		var exports = {
		  generic: {
			color: color,
			components: components,
			image: image,
			time: time,
			unit: unit,
			unitOrNumber: unitOrNumber
		  },
		  property: {
			animationDirection: keywordWithGlobal('animation-direction'),
			animationFillMode: keyword('animation-fill-mode'),
			animationIterationCount: animationIterationCount,
			animationName: animationName,
			animationPlayState: keywordWithGlobal('animation-play-state'),
			animationTimingFunction: animationTimingFunction,
			backgroundAttachment: keyword('background-attachment'),
			backgroundClip: keywordWithGlobal('background-clip'),
			backgroundOrigin: keyword('background-origin'),
			backgroundPosition: backgroundPosition,
			backgroundRepeat: keyword('background-repeat'),
			backgroundSize: backgroundSize,
			bottom: unitOrKeywordWithGlobal('bottom'),
			borderCollapse: keyword('border-collapse'),
			borderStyle: keywordWithGlobal('*-style'),
			clear: keywordWithGlobal('clear'),
			cursor: keywordWithGlobal('cursor'),
			display: keywordWithGlobal('display'),
			float: keywordWithGlobal('float'),
			left: unitOrKeywordWithGlobal('left'),
			fontFamily: fontFamily,
			fontStretch: keywordWithGlobal('font-stretch'),
			fontStyle: keywordWithGlobal('font-style'),
			fontVariant: keywordWithGlobal('font-variant'),
			fontWeight: keywordWithGlobal('font-weight'),
			listStyleType: keywordWithGlobal('list-style-type'),
			listStylePosition: keywordWithGlobal('list-style-position'),
			outlineStyle: keywordWithGlobal('*-style'),
			overflow: keywordWithGlobal('overflow'),
			position: keywordWithGlobal('position'),
			right: unitOrKeywordWithGlobal('right'),
			textAlign: keywordWithGlobal('text-align'),
			textDecoration: keywordWithGlobal('text-decoration'),
			textOverflow: keywordWithGlobal('text-overflow'),
			textShadow: textShadow,
			top: unitOrKeywordWithGlobal('top'),
			transform: sameFunctionOrValue,
			verticalAlign: unitOrKeywordWithGlobal('vertical-align'),
			visibility: keywordWithGlobal('visibility'),
			whiteSpace: keywordWithGlobal('white-space'),
			zIndex: zIndex
		  }
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/clone
	modules['/optimizer/level-2/clone'] = function () {
		var wrapSingle = require('/optimizer/wrap-for-optimizing').single;

		var Token = require('/tokenizer/token');

		function deep(property) {
		  var cloned = shallow(property);
		  for (var i = property.components.length - 1; i >= 0; i--) {
			var component = shallow(property.components[i]);
			component.value = property.components[i].value.slice(0);
			cloned.components.unshift(component);
		  }

		  cloned.dirty = true;
		  cloned.value = property.value.slice(0);

		  return cloned;
		}

		function shallow(property) {
		  var cloned = wrapSingle([
			Token.PROPERTY,
			[Token.PROPERTY_NAME, property.name]
		  ]);
		  cloned.important = property.important;
		  cloned.hack = property.hack;
		  cloned.unused = false;
		  return cloned;
		}

		var exports = {
		  deep: deep,
		  shallow: shallow
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/compactable
	modules['/optimizer/level-2/compactable'] = function () {
		// Contains the interpretation of CSS properties, as used by the property optimizer

		var breakUp = require('/optimizer/level-2/break-up');
		var canOverride = require('/optimizer/level-2/can-override');
		var restore = require('/optimizer/level-2/restore');

		var override = require('/utils/override');

		// Properties to process
		// Extend this object in order to add support for more properties in the optimizer.
		//
		// Each key in this object represents a CSS property and should be an object.
		// Such an object contains properties that describe how the represented CSS property should be handled.
		// Possible options:
		//
		// * components: array (Only specify for shorthand properties.)
		//   Contains the names of the granular properties this shorthand compacts.
		//
		// * canOverride: function
		//   Returns whether two tokens of this property can be merged with each other.
		//   This property has no meaning for shorthands.
		//
		// * defaultValue: string
		//   Specifies the default value of the property according to the CSS standard.
		//   For shorthand, this is used when every component is set to its default value, therefore it should be the shortest possible default value of all the components.
		//
		// * shortestValue: string
		//   Specifies the shortest possible value the property can possibly have.
		//   (Falls back to defaultValue if unspecified.)
		//
		// * breakUp: function (Only specify for shorthand properties.)
		//   Breaks the shorthand up to its components.
		//
		// * restore: function (Only specify for shorthand properties.)
		//   Puts the shorthand together from its components.
		//
		var compactable = {
		  'animation': {
			canOverride: canOverride.generic.components([
			  canOverride.generic.time,
			  canOverride.property.animationTimingFunction,
			  canOverride.generic.time,
			  canOverride.property.animationIterationCount,
			  canOverride.property.animationDirection,
			  canOverride.property.animationFillMode,
			  canOverride.property.animationPlayState,
			  canOverride.property.animationName
			]),
			components: [
			  'animation-duration',
			  'animation-timing-function',
			  'animation-delay',
			  'animation-iteration-count',
			  'animation-direction',
			  'animation-fill-mode',
			  'animation-play-state',
			  'animation-name'
			],
			breakUp: breakUp.multiplex(breakUp.animation),
			defaultValue: 'none',
			restore: restore.multiplex(restore.withoutDefaults),
			shorthand: true,
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-delay': {
			canOverride: canOverride.generic.time,
			componentOf: [
			  'animation'
			],
			defaultValue: '0s',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-direction': {
			canOverride: canOverride.property.animationDirection,
			componentOf: [
			  'animation'
			],
			defaultValue: 'normal',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-duration': {
			canOverride: canOverride.generic.time,
			componentOf: [
			  'animation'
			],
			defaultValue: '0s',
			intoMultiplexMode: 'real',
			keepUnlessDefault: 'animation-delay',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-fill-mode': {
			canOverride: canOverride.property.animationFillMode,
			componentOf: [
			  'animation'
			],
			defaultValue: 'none',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-iteration-count': {
			canOverride: canOverride.property.animationIterationCount,
			componentOf: [
			  'animation'
			],
			defaultValue: '1',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-name': {
			canOverride: canOverride.property.animationName,
			componentOf: [
			  'animation'
			],
			defaultValue: 'none',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-play-state': {
			canOverride: canOverride.property.animationPlayState,
			componentOf: [
			  'animation'
			],
			defaultValue: 'running',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'animation-timing-function': {
			canOverride: canOverride.property.animationTimingFunction,
			componentOf: [
			  'animation'
			],
			defaultValue: 'ease',
			intoMultiplexMode: 'real',
			vendorPrefixes: [
			  '-moz-',
			  '-o-',
			  '-webkit-'
			]
		  },
		  'background': {
			canOverride: canOverride.generic.components([
			  canOverride.generic.image,
			  canOverride.property.backgroundPosition,
			  canOverride.property.backgroundSize,
			  canOverride.property.backgroundRepeat,
			  canOverride.property.backgroundAttachment,
			  canOverride.property.backgroundOrigin,
			  canOverride.property.backgroundClip,
			  canOverride.generic.color
			]),
			components: [
			  'background-image',
			  'background-position',
			  'background-size',
			  'background-repeat',
			  'background-attachment',
			  'background-origin',
			  'background-clip',
			  'background-color'
			],
			breakUp: breakUp.multiplex(breakUp.background),
			defaultValue: '0 0',
			restore: restore.multiplex(restore.background),
			shortestValue: '0',
			shorthand: true
		  },
		  'background-attachment': {
			canOverride: canOverride.property.backgroundAttachment,
			componentOf: [
			  'background'
			],
			defaultValue: 'scroll',
			intoMultiplexMode: 'real'
		  },
		  'background-clip': {
			canOverride: canOverride.property.backgroundClip,
			componentOf: [
			  'background'
			],
			defaultValue: 'border-box',
			intoMultiplexMode: 'real',
			shortestValue: 'border-box'
		  },
		  'background-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'background'
			],
			defaultValue: 'transparent',
			intoMultiplexMode: 'real', // otherwise real color will turn into default since color appears in last multiplex only
			multiplexLastOnly: true,
			nonMergeableValue: 'none',
			shortestValue: 'red'
		  },
		  'background-image': {
			canOverride: canOverride.generic.image,
			componentOf: [
			  'background'
			],
			defaultValue: 'none',
			intoMultiplexMode: 'default'
		  },
		  'background-origin': {
			canOverride: canOverride.property.backgroundOrigin,
			componentOf: [
			  'background'
			],
			defaultValue: 'padding-box',
			intoMultiplexMode: 'real',
			shortestValue: 'border-box'
		  },
		  'background-position': {
			canOverride: canOverride.property.backgroundPosition,
			componentOf: [
			  'background'
			],
			defaultValue: ['0', '0'],
			doubleValues: true,
			intoMultiplexMode: 'real',
			shortestValue: '0'
		  },
		  'background-repeat': {
			canOverride: canOverride.property.backgroundRepeat,
			componentOf: [
			  'background'
			],
			defaultValue: ['repeat'],
			doubleValues: true,
			intoMultiplexMode: 'real'
		  },
		  'background-size': {
			canOverride: canOverride.property.backgroundSize,
			componentOf: [
			  'background'
			],
			defaultValue: ['auto'],
			doubleValues: true,
			intoMultiplexMode: 'real',
			shortestValue: '0 0'
		  },
		  'bottom': {
			canOverride: canOverride.property.bottom,
			defaultValue: 'auto'
		  },
		  'border': {
			breakUp: breakUp.border,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.property.borderStyle,
			  canOverride.generic.color
			]),
			components: [
			  'border-width',
			  'border-style',
			  'border-color'
			],
			defaultValue: 'none',
			overridesShorthands: [
			  'border-bottom',
			  'border-left',
			  'border-right',
			  'border-top'
			],
			restore: restore.withoutDefaults,
			shorthand: true,
			shorthandComponents: true
		  },
		  'border-bottom': {
			breakUp: breakUp.border,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.property.borderStyle,
			  canOverride.generic.color
			]),
			components: [
			  'border-bottom-width',
			  'border-bottom-style',
			  'border-bottom-color'
			],
			defaultValue: 'none',
			restore: restore.withoutDefaults,
			shorthand: true
		  },
		  'border-bottom-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'border-bottom',
			  'border-color'
			],
			defaultValue: 'none'
		  },
		  'border-bottom-left-radius': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-radius'
			],
			defaultValue: '0',
			vendorPrefixes: [
			  '-moz-',
			  '-o-'
			]
		  },
		  'border-bottom-right-radius': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-radius'
			],
			defaultValue: '0',
			vendorPrefixes: [
			  '-moz-',
			  '-o-'
			]
		  },
		  'border-bottom-style': {
			canOverride: canOverride.property.borderStyle,
			componentOf: [
			  'border-bottom',
			  'border-style'
			],
			defaultValue: 'none'
		  },
		  'border-bottom-width': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-bottom',
			  'border-width'
			],
			defaultValue: 'medium',
			oppositeTo: 'border-top-width',
			shortestValue: '0'
		  },
		  'border-collapse': {
			canOverride: canOverride.property.borderCollapse,
			defaultValue: 'separate'
		  },
		  'border-color': {
			breakUp: breakUp.fourValues,
			canOverride: canOverride.generic.components([
			  canOverride.generic.color,
			  canOverride.generic.color,
			  canOverride.generic.color,
			  canOverride.generic.color
			]),
			componentOf: [
			  'border'
			],
			components: [
			  'border-top-color',
			  'border-right-color',
			  'border-bottom-color',
			  'border-left-color'
			],
			defaultValue: 'none',
			restore: restore.fourValues,
			shortestValue: 'red',
			shorthand: true
		  },
		  'border-left': {
			breakUp: breakUp.border,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.property.borderStyle,
			  canOverride.generic.color
			]),
			components: [
			  'border-left-width',
			  'border-left-style',
			  'border-left-color'
			],
			defaultValue: 'none',
			restore: restore.withoutDefaults,
			shorthand: true
		  },
		  'border-left-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'border-color',
			  'border-left'
			],
			defaultValue: 'none'
		  },
		  'border-left-style': {
			canOverride: canOverride.property.borderStyle,
			componentOf: [
			  'border-left',
			  'border-style'
			],
			defaultValue: 'none'
		  },
		  'border-left-width': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-left',
			  'border-width'
			],
			defaultValue: 'medium',
			oppositeTo: 'border-right-width',
			shortestValue: '0'
		  },
		  'border-radius': {
			breakUp: breakUp.borderRadius,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit
			]),
			components: [
			  'border-top-left-radius',
			  'border-top-right-radius',
			  'border-bottom-right-radius',
			  'border-bottom-left-radius'
			],
			defaultValue: '0',
			restore: restore.borderRadius,
			shorthand: true,
			vendorPrefixes: [
			  '-moz-',
			  '-o-'
			]
		  },
		  'border-right': {
			breakUp: breakUp.border,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.property.borderStyle,
			  canOverride.generic.color
			]),
			components: [
			  'border-right-width',
			  'border-right-style',
			  'border-right-color'
			],
			defaultValue: 'none',
			restore: restore.withoutDefaults,
			shorthand: true
		  },
		  'border-right-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'border-color',
			  'border-right'
			],
			defaultValue: 'none'
		  },
		  'border-right-style': {
			canOverride: canOverride.property.borderStyle,
			componentOf: [
			  'border-right',
			  'border-style'
			],
			defaultValue: 'none'
		  },
		  'border-right-width': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-right',
			  'border-width'
			],
			defaultValue: 'medium',
			oppositeTo: 'border-left-width',
			shortestValue: '0'
		  },
		  'border-style': {
			breakUp: breakUp.fourValues,
			canOverride: canOverride.generic.components([
			  canOverride.property.borderStyle,
			  canOverride.property.borderStyle,
			  canOverride.property.borderStyle,
			  canOverride.property.borderStyle
			]),
			componentOf: [
			  'border'
			],
			components: [
			  'border-top-style',
			  'border-right-style',
			  'border-bottom-style',
			  'border-left-style'
			],
			defaultValue: 'none',
			restore: restore.fourValues,
			shorthand: true
		  },
		  'border-top': {
			breakUp: breakUp.border,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.property.borderStyle,
			  canOverride.generic.color
			]),
			components: [
			  'border-top-width',
			  'border-top-style',
			  'border-top-color'
			],
			defaultValue: 'none',
			restore: restore.withoutDefaults,
			shorthand: true
		  },
		  'border-top-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'border-color',
			  'border-top'
			],
			defaultValue: 'none'
		  },
		  'border-top-left-radius': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-radius'
			],
			defaultValue: '0',
			vendorPrefixes: [
			  '-moz-',
			  '-o-'
			]
		  },
		  'border-top-right-radius': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-radius'
			],
			defaultValue: '0',
			vendorPrefixes: [
			  '-moz-',
			  '-o-'
			]
		  },
		  'border-top-style': {
			canOverride: canOverride.property.borderStyle,
			componentOf: [
			  'border-style',
			  'border-top'
			],
			defaultValue: 'none'
		  },
		  'border-top-width': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'border-top',
			  'border-width'
			],
			defaultValue: 'medium',
			oppositeTo: 'border-bottom-width',
			shortestValue: '0'
		  },
		  'border-width': {
			breakUp: breakUp.fourValues,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit
			]),
			componentOf: [
			  'border'
			],
			components: [
			  'border-top-width',
			  'border-right-width',
			  'border-bottom-width',
			  'border-left-width'
			],
			defaultValue: 'medium',
			restore: restore.fourValues,
			shortestValue: '0',
			shorthand: true
		  },
		  'clear': {
			canOverride: canOverride.property.clear,
			defaultValue: 'none'
		  },
		  'color': {
			canOverride: canOverride.generic.color,
			defaultValue: 'transparent',
			shortestValue: 'red'
		  },
		  'cursor': {
			canOverride: canOverride.property.cursor,
			defaultValue: 'auto'
		  },
		  'display': {
			canOverride: canOverride.property.display,
		  },
		  'float': {
			canOverride: canOverride.property.float,
			defaultValue: 'none'
		  },
		  'font': {
			breakUp: breakUp.font,
			canOverride: canOverride.generic.components([
			  canOverride.property.fontStyle,
			  canOverride.property.fontVariant,
			  canOverride.property.fontWeight,
			  canOverride.property.fontStretch,
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.property.fontFamily
			]),
			components: [
			  'font-style',
			  'font-variant',
			  'font-weight',
			  'font-stretch',
			  'font-size',
			  'line-height',
			  'font-family'
			],
			restore: restore.font,
			shorthand: true
		  },
		  'font-family': {
			canOverride: canOverride.property.fontFamily,
			defaultValue: 'user|agent|specific'
		  },
		  'font-size': {
			canOverride: canOverride.generic.unit,
			defaultValue: 'medium',
			shortestValue: '0'
		  },
		  'font-stretch': {
			canOverride: canOverride.property.fontStretch,
			defaultValue: 'normal'
		  },
		  'font-style': {
			canOverride: canOverride.property.fontStyle,
			defaultValue: 'normal'
		  },
		  'font-variant': {
			canOverride: canOverride.property.fontVariant,
			defaultValue: 'normal'
		  },
		  'font-weight': {
			canOverride: canOverride.property.fontWeight,
			defaultValue: 'normal',
			shortestValue: '400'
		  },
		  'height': {
			canOverride: canOverride.generic.unit,
			defaultValue: 'auto',
			shortestValue: '0'
		  },
		  'left': {
			canOverride: canOverride.property.left,
			defaultValue: 'auto'
		  },
		  'line-height': {
			canOverride: canOverride.generic.unitOrNumber,
			defaultValue: 'normal',
			shortestValue: '0'
		  },
		  'list-style': {
			canOverride: canOverride.generic.components([
			  canOverride.property.listStyleType,
			  canOverride.property.listStylePosition,
			  canOverride.property.listStyleImage
			]),
			components: [
			  'list-style-type',
			  'list-style-position',
			  'list-style-image'
			],
			breakUp: breakUp.listStyle,
			restore: restore.withoutDefaults,
			defaultValue: 'outside', // can't use 'disc' because that'd override default 'decimal' for <ol>
			shortestValue: 'none',
			shorthand: true
		  },
		  'list-style-image' : {
			canOverride: canOverride.generic.image,
			componentOf: [
			  'list-style'
			],
			defaultValue: 'none'
		  },
		  'list-style-position' : {
			canOverride: canOverride.property.listStylePosition,
			componentOf: [
			  'list-style'
			],
			defaultValue: 'outside',
			shortestValue: 'inside'
		  },
		  'list-style-type' : {
			canOverride: canOverride.property.listStyleType,
			componentOf: [
			  'list-style'
			],
			// NOTE: we can't tell the real default value here, it's 'disc' for <ul> and 'decimal' for <ol>
			// this is a hack, but it doesn't matter because this value will be either overridden or
			// it will disappear at the final step anyway
			defaultValue: 'decimal|disc',
			shortestValue: 'none'
		  },
		  'margin': {
			breakUp: breakUp.fourValues,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit
			]),
			components: [
			  'margin-top',
			  'margin-right',
			  'margin-bottom',
			  'margin-left'
			],
			defaultValue: '0',
			restore: restore.fourValues,
			shorthand: true
		  },
		  'margin-bottom': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'margin'
			],
			defaultValue: '0',
			oppositeTo: 'margin-top'
		  },
		  'margin-left': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'margin'
			],
			defaultValue: '0',
			oppositeTo: 'margin-right'
		  },
		  'margin-right': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'margin'
			],
			defaultValue: '0',
			oppositeTo: 'margin-left'
		  },
		  'margin-top': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'margin'
			],
			defaultValue: '0',
			oppositeTo: 'margin-bottom'
		  },
		  'outline': {
			canOverride: canOverride.generic.components([
			  canOverride.generic.color,
			  canOverride.property.outlineStyle,
			  canOverride.generic.unit
			]),
			components: [
			  'outline-color',
			  'outline-style',
			  'outline-width'
			],
			breakUp: breakUp.outline,
			restore: restore.withoutDefaults,
			defaultValue: '0',
			shorthand: true
		  },
		  'outline-color': {
			canOverride: canOverride.generic.color,
			componentOf: [
			  'outline'
			],
			defaultValue: 'invert',
			shortestValue: 'red'
		  },
		  'outline-style': {
			canOverride: canOverride.property.outlineStyle,
			componentOf: [
			  'outline'
			],
			defaultValue: 'none'
		  },
		  'outline-width': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'outline'
			],
			defaultValue: 'medium',
			shortestValue: '0'
		  },
		  'overflow': {
			canOverride: canOverride.property.overflow,
			defaultValue: 'visible'
		  },
		  'overflow-x': {
			canOverride: canOverride.property.overflow,
			defaultValue: 'visible'
		  },
		  'overflow-y': {
			canOverride: canOverride.property.overflow,
			defaultValue: 'visible'
		  },
		  'padding': {
			breakUp: breakUp.fourValues,
			canOverride: canOverride.generic.components([
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit,
			  canOverride.generic.unit
			]),
			components: [
			  'padding-top',
			  'padding-right',
			  'padding-bottom',
			  'padding-left'
			],
			defaultValue: '0',
			restore: restore.fourValues,
			shorthand: true
		  },
		  'padding-bottom': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'padding'
			],
			defaultValue: '0',
			oppositeTo: 'padding-top'
		  },
		  'padding-left': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'padding'
			],
			defaultValue: '0',
			oppositeTo: 'padding-right'
		  },
		  'padding-right': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'padding'
			],
			defaultValue: '0',
			oppositeTo: 'padding-left'
		  },
		  'padding-top': {
			canOverride: canOverride.generic.unit,
			componentOf: [
			  'padding'
			],
			defaultValue: '0',
			oppositeTo: 'padding-bottom'
		  },
		  'position': {
			canOverride: canOverride.property.position,
			defaultValue: 'static'
		  },
		  'right': {
			canOverride: canOverride.property.right,
			defaultValue: 'auto'
		  },
		  'text-align': {
			canOverride: canOverride.property.textAlign,
			// NOTE: we can't tell the real default value here, as it depends on default text direction
			// this is a hack, but it doesn't matter because this value will be either overridden or
			// it will disappear anyway
			defaultValue: 'left|right'
		  },
		  'text-decoration': {
			canOverride: canOverride.property.textDecoration,
			defaultValue: 'none'
		  },
		  'text-overflow': {
			canOverride: canOverride.property.textOverflow,
			defaultValue: 'none'
		  },
		  'text-shadow': {
			canOverride: canOverride.property.textShadow,
			defaultValue: 'none'
		  },
		  'top': {
			canOverride: canOverride.property.top,
			defaultValue: 'auto'
		  },
		  'transform': {
			canOverride: canOverride.property.transform,
			vendorPrefixes: [
			  '-moz-',
			  '-ms-',
			  '-webkit-'
			]
		  },
		  'vertical-align': {
			canOverride: canOverride.property.verticalAlign,
			defaultValue: 'baseline'
		  },
		  'visibility': {
			canOverride: canOverride.property.visibility,
			defaultValue: 'visible'
		  },
		  'white-space': {
			canOverride: canOverride.property.whiteSpace,
			defaultValue: 'normal'
		  },
		  'width': {
			canOverride: canOverride.generic.unit,
			defaultValue: 'auto',
			shortestValue: '0'
		  },
		  'z-index': {
			canOverride: canOverride.property.zIndex,
			defaultValue: 'auto'
		  }
		};

		function cloneDescriptor(propertyName, prefix) {
		  var clonedDescriptor = override(compactable[propertyName], {});

		  if ('componentOf' in clonedDescriptor) {
			clonedDescriptor.componentOf = clonedDescriptor.componentOf.map(function (shorthandName) {
			  return prefix + shorthandName;
			});
		  }

		  if ('components' in clonedDescriptor) {
			clonedDescriptor.components = clonedDescriptor.components.map(function (longhandName) {
			  return prefix + longhandName;
			});
		  }

		  if ('keepUnlessDefault' in clonedDescriptor) {
			clonedDescriptor.keepUnlessDefault = prefix + clonedDescriptor.keepUnlessDefault;
		  }

		  return clonedDescriptor;
		}

		// generate vendor-prefixed properties
		var vendorPrefixedCompactable = {};

		for (var propertyName in compactable) {
		  var descriptor = compactable[propertyName];

		  if (!('vendorPrefixes' in descriptor)) {
			continue;
		  }

		  for (var i = 0; i < descriptor.vendorPrefixes.length; i++) {
			var prefix = descriptor.vendorPrefixes[i];
			var clonedDescriptor = cloneDescriptor(propertyName, prefix);
			delete clonedDescriptor.vendorPrefixes;

			vendorPrefixedCompactable[prefix + propertyName] = clonedDescriptor;
		  }

		  delete descriptor.vendorPrefixes;
		}

		return override(compactable, vendorPrefixedCompactable);
	};
	//#endregion

	//#region URL: /optimizer/level-2/extract-properties
	modules['/optimizer/level-2/extract-properties'] = function () {
		// This extractor is used in level 2 optimizations
		// IMPORTANT: Mind Token class and this code is not related!
		// Properties will be tokenized in one step, see #429

		var Token = require('/tokenizer/token');
		var serializeRules = require('/writer/one-time').rules;
		var serializeValue = require('/writer/one-time').value;

		function extractProperties(token) {
		  var properties = [];
		  var inSpecificSelector;
		  var property;
		  var name;
		  var value;
		  var i, l;

		  if (token[0] == Token.RULE) {
			inSpecificSelector = !/[\.\+>~]/.test(serializeRules(token[1]));

			for (i = 0, l = token[2].length; i < l; i++) {
			  property = token[2][i];

			  if (property[0] != Token.PROPERTY)
				continue;

			  name = property[1][1];
			  if (name.length === 0)
				continue;

			  if (name.indexOf('--') === 0)
				continue;

			  value = serializeValue(property, i);

			  properties.push([
				name,
				value,
				findNameRoot(name),
				token[2][i],
				name + ':' + value,
				token[1],
				inSpecificSelector
			  ]);
			}
		  } else if (token[0] == Token.NESTED_BLOCK) {
			for (i = 0, l = token[2].length; i < l; i++) {
			  properties = properties.concat(extractProperties(token[2][i]));
			}
		  }

		  return properties;
		}

		function findNameRoot(name) {
		  if (name == 'list-style')
			return name;
		  if (name.indexOf('-radius') > 0)
			return 'border-radius';
		  if (name == 'border-collapse' || name == 'border-spacing' || name == 'border-image')
			return name;
		  if (name.indexOf('border-') === 0 && /^border\-\w+\-\w+$/.test(name))
			return name.match(/border\-\w+/)[0];
		  if (name.indexOf('border-') === 0 && /^border\-\w+$/.test(name))
			return 'border';
		  if (name.indexOf('text-') === 0)
			return name;
		  if (name == '-chrome-')
			return name;

		  return name.replace(/^\-\w+\-/, '').match(/([a-zA-Z]+)/)[0].toLowerCase();
		}

		return extractProperties;
	};
	//#endregion

	//#region URL: /optimizer/level-2/invalid-property-error
	modules['/optimizer/level-2/invalid-property-error'] = function () {
		function InvalidPropertyError(message) {
		  this.name = 'InvalidPropertyError';
		  this.message = message;
		  this.stack = (new Error()).stack;
		}

		InvalidPropertyError.prototype = Object.create(Error.prototype);
		InvalidPropertyError.prototype.constructor = InvalidPropertyError;

		return InvalidPropertyError;
	};
	//#endregion

	//#region URL: /optimizer/level-2/is-mergeable
	modules['/optimizer/level-2/is-mergeable'] = function () {
		var Marker = require('/tokenizer/marker');
		var split = require('/utils/split');

		var DEEP_SELECTOR_PATTERN = /\/deep\//;
		var DOUBLE_COLON_PATTERN = /^::/;
		var NOT_PSEUDO = ':not';
		var PSEUDO_CLASSES_WITH_ARGUMENTS = [
		  ':dir',
		  ':lang',
		  ':not',
		  ':nth-child',
		  ':nth-last-child',
		  ':nth-last-of-type',
		  ':nth-of-type'
		];
		var RELATION_PATTERN = /[>\+~]/;
		var UNMIXABLE_PSEUDO_CLASSES = [
		  ':after',
		  ':before',
		  ':first-letter',
		  ':first-line',
		  ':lang'
		];
		var UNMIXABLE_PSEUDO_ELEMENTS = [
		  '::after',
		  '::before',
		  '::first-letter',
		  '::first-line'
		];

		var Level = {
		  DOUBLE_QUOTE: 'double-quote',
		  SINGLE_QUOTE: 'single-quote',
		  ROOT: 'root'
		};

		function isMergeable(selector, mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) {
		  var singleSelectors = split(selector, Marker.COMMA);
		  var singleSelector;
		  var i, l;

		  for (i = 0, l = singleSelectors.length; i < l; i++) {
			singleSelector = singleSelectors[i];

			if (singleSelector.length === 0 ||
				isDeepSelector(singleSelector) ||
				(singleSelector.indexOf(Marker.COLON) > -1 && !areMergeable(singleSelector, extractPseudoFrom(singleSelector), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging))) {
			  return false;
			}
		  }

		  return true;
		}

		function isDeepSelector(selector) {
		  return DEEP_SELECTOR_PATTERN.test(selector);
		}

		function extractPseudoFrom(selector) {
		  var list = [];
		  var character;
		  var buffer = [];
		  var level = Level.ROOT;
		  var roundBracketLevel = 0;
		  var isQuoted;
		  var isEscaped;
		  var isPseudo = false;
		  var isRelation;
		  var wasColon = false;
		  var index;
		  var len;

		  for (index = 0, len = selector.length; index < len; index++) {
			character = selector[index];

			isRelation = !isEscaped && RELATION_PATTERN.test(character);
			isQuoted = level == Level.DOUBLE_QUOTE || level == Level.SINGLE_QUOTE;

			if (isEscaped) {
			  buffer.push(character);
			} else if (character == Marker.DOUBLE_QUOTE && level == Level.ROOT) {
			  buffer.push(character);
			  level = Level.DOUBLE_QUOTE;
			} else if (character == Marker.DOUBLE_QUOTE && level == Level.DOUBLE_QUOTE) {
			  buffer.push(character);
			  level = Level.ROOT;
			} else if (character == Marker.SINGLE_QUOTE && level == Level.ROOT) {
			  buffer.push(character);
			  level = Level.SINGLE_QUOTE;
			} else if (character == Marker.SINGLE_QUOTE && level == Level.SINGLE_QUOTE) {
			  buffer.push(character);
			  level = Level.ROOT;
			} else if (isQuoted) {
			  buffer.push(character);
			} else if (character == Marker.OPEN_ROUND_BRACKET) {
			  buffer.push(character);
			  roundBracketLevel++;
			} else if (character == Marker.CLOSE_ROUND_BRACKET && roundBracketLevel == 1 && isPseudo) {
			  buffer.push(character);
			  list.push(buffer.join(''));
			  roundBracketLevel--;
			  buffer = [];
			  isPseudo = false;
			} else if (character == Marker.CLOSE_ROUND_BRACKET) {
			  buffer.push(character);
			  roundBracketLevel--;
			} else if (character == Marker.COLON && roundBracketLevel === 0 && isPseudo && !wasColon) {
			  list.push(buffer.join(''));
			  buffer = [];
			  buffer.push(character);
			} else if (character == Marker.COLON && roundBracketLevel === 0 && !wasColon) {
			  buffer = [];
			  buffer.push(character);
			  isPseudo = true;
			} else if (character == Marker.SPACE && roundBracketLevel === 0 && isPseudo) {
			  list.push(buffer.join(''));
			  buffer = [];
			  isPseudo = false;
			} else if (isRelation && roundBracketLevel === 0 && isPseudo) {
			  list.push(buffer.join(''));
			  buffer = [];
			  isPseudo = false;
			} else {
			  buffer.push(character);
			}

			isEscaped = character == Marker.BACK_SLASH;
			wasColon = character == Marker.COLON;
		  }

		  if (buffer.length > 0 && isPseudo) {
			list.push(buffer.join(''));
		  }

		  return list;
		}

		function areMergeable(selector, matches, mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) {
		  return areAllowed(matches, mergeablePseudoClasses, mergeablePseudoElements) &&
			needArguments(matches) &&
			(matches.length < 2 || !someIncorrectlyChained(selector, matches)) &&
			(matches.length < 2 || multiplePseudoMerging && allMixable(matches));
		}

		function areAllowed(matches, mergeablePseudoClasses, mergeablePseudoElements) {
		  var match;
		  var name;
		  var i, l;

		  for (i = 0, l = matches.length; i < l; i++) {
			match = matches[i];
			name = match.indexOf(Marker.OPEN_ROUND_BRACKET) > -1 ?
			  match.substring(0, match.indexOf(Marker.OPEN_ROUND_BRACKET)) :
			  match;

			if (mergeablePseudoClasses.indexOf(name) === -1 && mergeablePseudoElements.indexOf(name) === -1) {
			  return false;
			}
		  }

		  return true;
		}

		function needArguments(matches) {
		  var match;
		  var name;
		  var bracketOpensAt;
		  var hasArguments;
		  var i, l;

		  for (i = 0, l = matches.length; i < l; i++) {
			match = matches[i];

			bracketOpensAt = match.indexOf(Marker.OPEN_ROUND_BRACKET);
			hasArguments = bracketOpensAt > -1;
			name = hasArguments ?
			  match.substring(0, bracketOpensAt) :
			  match;

			if (hasArguments && PSEUDO_CLASSES_WITH_ARGUMENTS.indexOf(name) == -1) {
			  return false;
			}

			if (!hasArguments && PSEUDO_CLASSES_WITH_ARGUMENTS.indexOf(name) > -1) {
			  return false;
			}
		  }

		  return true;
		}

		function someIncorrectlyChained(selector, matches) {
		  var positionInSelector = 0;
		  var match;
		  var matchAt;
		  var nextMatch;
		  var nextMatchAt;
		  var name;
		  var nextName;
		  var areChained;
		  var i, l;

		  for (i = 0, l = matches.length; i < l; i++) {
			match = matches[i];
			nextMatch = matches[i + 1];

			if (!nextMatch) {
			  break;
			}

			matchAt = selector.indexOf(match, positionInSelector);
			nextMatchAt = selector.indexOf(match, matchAt + 1);
			positionInSelector = nextMatchAt;
			areChained = matchAt + match.length == nextMatchAt;

			if (areChained) {
			  name = match.indexOf(Marker.OPEN_ROUND_BRACKET) > -1 ?
				match.substring(0, match.indexOf(Marker.OPEN_ROUND_BRACKET)) :
				match;
			  nextName = nextMatch.indexOf(Marker.OPEN_ROUND_BRACKET) > -1 ?
				nextMatch.substring(0, nextMatch.indexOf(Marker.OPEN_ROUND_BRACKET)) :
				nextMatch;

			  if (name != NOT_PSEUDO || nextName != NOT_PSEUDO) {
				return true;
			  }
			}
		  }

		  return false;
		}

		function allMixable(matches) {
		  var unmixableMatches = 0;
		  var match;
		  var i, l;

		  for (i = 0, l = matches.length; i < l; i++) {
			match = matches[i];

			if (isPseudoElement(match)) {
			  unmixableMatches += UNMIXABLE_PSEUDO_ELEMENTS.indexOf(match) > -1 ? 1 : 0;
			} else {
			  unmixableMatches += UNMIXABLE_PSEUDO_CLASSES.indexOf(match) > -1 ? 1 : 0;
			}

			if (unmixableMatches > 1) {
			  return false;
			}
		  }

		  return true;
		}

		function isPseudoElement(pseudo) {
		  return DOUBLE_COLON_PATTERN.test(pseudo);
		}

		return isMergeable;
	};
	//#endregion

	//#region URL: /optimizer/level-2/merge-adjacent
	modules['/optimizer/level-2/merge-adjacent'] = function () {
		var isMergeable = require('/optimizer/level-2/is-mergeable');

		var optimizeProperties = require('/optimizer/level-2/properties/optimize');

		var sortSelectors = require('/optimizer/level-1/sort-selectors');
		var tidyRules = require('/optimizer/level-1/tidy-rules');

		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;

		var serializeBody = require('/writer/one-time').body;
		var serializeRules = require('/writer/one-time').rules;

		var Token = require('/tokenizer/token');

		function mergeAdjacent(tokens, context) {
		  var lastToken = [null, [], []];
		  var options = context.options;
		  var adjacentSpace = options.compatibility.selectors.adjacentSpace;
		  var selectorsSortingMethod = options.level[OptimizationLevel.One].selectorsSortingMethod;
		  var mergeablePseudoClasses = options.compatibility.selectors.mergeablePseudoClasses;
		  var mergeablePseudoElements = options.compatibility.selectors.mergeablePseudoElements;
		  var mergeLimit = options.compatibility.selectors.mergeLimit;
		  var multiplePseudoMerging = options.compatibility.selectors.multiplePseudoMerging;

		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];

			if (token[0] != Token.RULE) {
			  lastToken = [null, [], []];
			  continue;
			}

			if (lastToken[0] == Token.RULE && serializeRules(token[1]) == serializeRules(lastToken[1])) {
			  Array.prototype.push.apply(lastToken[2], token[2]);
			  optimizeProperties(lastToken[2], true, true, context);
			  token[2] = [];
			} else if (lastToken[0] == Token.RULE && serializeBody(token[2]) == serializeBody(lastToken[2]) &&
				isMergeable(serializeRules(token[1]), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) &&
				isMergeable(serializeRules(lastToken[1]), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) &&
				lastToken[1].length < mergeLimit) {
			  lastToken[1] = tidyRules(lastToken[1].concat(token[1]), false, adjacentSpace, false, context.warnings);
			  lastToken[1] = lastToken.length > 1 ? sortSelectors(lastToken[1], selectorsSortingMethod) : lastToken[1];
			  token[2] = [];
			} else {
			  lastToken = token;
			}
		  }
		}

		return mergeAdjacent;
	};
	//#endregion

	//#region URL: /optimizer/level-2/merge-media-queries
	modules['/optimizer/level-2/merge-media-queries'] = function () {
		var canReorder = require('/optimizer/level-2/reorderable').canReorder;
		var canReorderSingle = require('/optimizer/level-2/reorderable').canReorderSingle;
		var extractProperties = require('/optimizer/level-2/extract-properties');
		var rulesOverlap = require('/optimizer/level-2/rules-overlap');

		var serializeRules = require('/writer/one-time').rules;
		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;
		var Token = require('/tokenizer/token');

		function mergeMediaQueries(tokens, context) {
		  var mergeSemantically = context.options.level[OptimizationLevel.Two].mergeSemantically;
		  var specificityCache = context.cache.specificity;
		  var candidates = {};
		  var reduced = [];

		  for (var i = tokens.length - 1; i >= 0; i--) {
			var token = tokens[i];
			if (token[0] != Token.NESTED_BLOCK) {
			  continue;
			}

			var key = serializeRules(token[1]);
			var candidate = candidates[key];
			if (!candidate) {
			  candidate = [];
			  candidates[key] = candidate;
			}

			candidate.push(i);
		  }

		  for (var name in candidates) {
			var positions = candidates[name];

			positionLoop:
			for (var j = positions.length - 1; j > 0; j--) {
			  var positionOne = positions[j];
			  var tokenOne = tokens[positionOne];
			  var positionTwo = positions[j - 1];
			  var tokenTwo = tokens[positionTwo];

			  directionLoop:
			  for (var direction = 1; direction >= -1; direction -= 2) {
				var topToBottom = direction == 1;
				var from = topToBottom ? positionOne + 1 : positionTwo - 1;
				var to = topToBottom ? positionTwo : positionOne;
				var delta = topToBottom ? 1 : -1;
				var source = topToBottom ? tokenOne : tokenTwo;
				var target = topToBottom ? tokenTwo : tokenOne;
				var movedProperties = extractProperties(source);

				while (from != to) {
				  var traversedProperties = extractProperties(tokens[from]);
				  from += delta;

				  if (mergeSemantically && allSameRulePropertiesCanBeReordered(movedProperties, traversedProperties, specificityCache)) {
					continue;
				  }

				  if (!canReorder(movedProperties, traversedProperties, specificityCache))
					continue directionLoop;
				}

				target[2] = topToBottom ?
				  source[2].concat(target[2]) :
				  target[2].concat(source[2]);
				source[2] = [];

				reduced.push(target);
				continue positionLoop;
			  }
			}
		  }

		  return reduced;
		}

		function allSameRulePropertiesCanBeReordered(movedProperties, traversedProperties, specificityCache) {
		  var movedProperty;
		  var movedRule;
		  var traversedProperty;
		  var traversedRule;
		  var i, l;
		  var j, m;

		  for (i = 0, l = movedProperties.length; i < l; i++) {
			movedProperty = movedProperties[i];
			movedRule = movedProperty[5];

			for (j = 0, m = traversedProperties.length; j < m; j++) {
			  traversedProperty = traversedProperties[j];
			  traversedRule = traversedProperty[5];

			  if (rulesOverlap(movedRule, traversedRule, true) && !canReorderSingle(movedProperty, traversedProperty, specificityCache)) {
				return false;
			  }
			}
		  }

		  return true;
		}

		return mergeMediaQueries;
	};
	//#endregion

	//#region URL: /optimizer/level-2/merge-non-adjacent-by-body
	modules['/optimizer/level-2/merge-non-adjacent-by-body'] = function () {
		var isMergeable = require('/optimizer/level-2/is-mergeable');

		var sortSelectors = require('/optimizer/level-1/sort-selectors');
		var tidyRules = require('/optimizer/level-1/tidy-rules');

		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;

		var serializeBody = require('/writer/one-time').body;
		var serializeRules = require('/writer/one-time').rules;

		var Token = require('/tokenizer/token');

		function unsafeSelector(value) {
		  return /\.|\*| :/.test(value);
		}

		function isBemElement(token) {
		  var asString = serializeRules(token[1]);
		  return asString.indexOf('__') > -1 || asString.indexOf('--') > -1;
		}

		function withoutModifier(selector) {
		  return selector.replace(/--[^ ,>\+~:]+/g, '');
		}

		function removeAnyUnsafeElements(left, candidates) {
		  var leftSelector = withoutModifier(serializeRules(left[1]));

		  for (var body in candidates) {
			var right = candidates[body];
			var rightSelector = withoutModifier(serializeRules(right[1]));

			if (rightSelector.indexOf(leftSelector) > -1 || leftSelector.indexOf(rightSelector) > -1)
			  delete candidates[body];
		  }
		}

		function mergeNonAdjacentByBody(tokens, context) {
		  var options = context.options;
		  var mergeSemantically = options.level[OptimizationLevel.Two].mergeSemantically;
		  var adjacentSpace = options.compatibility.selectors.adjacentSpace;
		  var selectorsSortingMethod = options.level[OptimizationLevel.One].selectorsSortingMethod;
		  var mergeablePseudoClasses = options.compatibility.selectors.mergeablePseudoClasses;
		  var mergeablePseudoElements = options.compatibility.selectors.mergeablePseudoElements;
		  var multiplePseudoMerging = options.compatibility.selectors.multiplePseudoMerging;
		  var candidates = {};

		  for (var i = tokens.length - 1; i >= 0; i--) {
			var token = tokens[i];
			if (token[0] != Token.RULE)
			  continue;

			if (token[2].length > 0 && (!mergeSemantically && unsafeSelector(serializeRules(token[1]))))
			  candidates = {};

			if (token[2].length > 0 && mergeSemantically && isBemElement(token))
			  removeAnyUnsafeElements(token, candidates);

			var candidateBody = serializeBody(token[2]);
			var oldToken = candidates[candidateBody];
			if (oldToken &&
				isMergeable(serializeRules(token[1]), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) &&
				isMergeable(serializeRules(oldToken[1]), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging)) {

			  if (token[2].length > 0) {
				token[1] = tidyRules(oldToken[1].concat(token[1]), false, adjacentSpace, false, context.warnings);
				token[1] = token[1].length > 1 ? sortSelectors(token[1], selectorsSortingMethod) : token[1];
			  } else {
				token[1] = oldToken[1].concat(token[1]);
			  }

			  oldToken[2] = [];
			  candidates[candidateBody] = null;
			}

			candidates[serializeBody(token[2])] = token;
		  }
		}

		return mergeNonAdjacentByBody;
	};
	//#endregion

	//#region URL: /optimizer/level-2/merge-non-adjacent-by-selector
	modules['/optimizer/level-2/merge-non-adjacent-by-selector'] = function () {
		var canReorder = require('/optimizer/level-2/reorderable').canReorder;
		var extractProperties = require('/optimizer/level-2/extract-properties');

		var optimizeProperties = require('/optimizer/level-2/properties/optimize');

		var serializeRules = require('/writer/one-time').rules;

		var Token = require('/tokenizer/token');

		function mergeNonAdjacentBySelector(tokens, context) {
		  var specificityCache = context.cache.specificity;
		  var allSelectors = {};
		  var repeatedSelectors = [];
		  var i;

		  for (i = tokens.length - 1; i >= 0; i--) {
			if (tokens[i][0] != Token.RULE)
			  continue;
			if (tokens[i][2].length === 0)
			  continue;

			var selector = serializeRules(tokens[i][1]);
			allSelectors[selector] = [i].concat(allSelectors[selector] || []);

			if (allSelectors[selector].length == 2)
			  repeatedSelectors.push(selector);
		  }

		  for (i = repeatedSelectors.length - 1; i >= 0; i--) {
			var positions = allSelectors[repeatedSelectors[i]];

			selectorIterator:
			for (var j = positions.length - 1; j > 0; j--) {
			  var positionOne = positions[j - 1];
			  var tokenOne = tokens[positionOne];
			  var positionTwo = positions[j];
			  var tokenTwo = tokens[positionTwo];

			  directionIterator:
			  for (var direction = 1; direction >= -1; direction -= 2) {
				var topToBottom = direction == 1;
				var from = topToBottom ? positionOne + 1 : positionTwo - 1;
				var to = topToBottom ? positionTwo : positionOne;
				var delta = topToBottom ? 1 : -1;
				var moved = topToBottom ? tokenOne : tokenTwo;
				var target = topToBottom ? tokenTwo : tokenOne;
				var movedProperties = extractProperties(moved);

				while (from != to) {
				  var traversedProperties = extractProperties(tokens[from]);
				  from += delta;

				  // traversed then moved as we move selectors towards the start
				  var reorderable = topToBottom ?
					canReorder(movedProperties, traversedProperties, specificityCache) :
					canReorder(traversedProperties, movedProperties, specificityCache);

				  if (!reorderable && !topToBottom)
					continue selectorIterator;
				  if (!reorderable && topToBottom)
					continue directionIterator;
				}

				if (topToBottom) {
				  Array.prototype.push.apply(moved[2], target[2]);
				  target[2] = moved[2];
				} else {
				  Array.prototype.push.apply(target[2], moved[2]);
				}

				optimizeProperties(target[2], true, true, context);
				moved[2] = [];
			  }
			}
		  }
		}

		return mergeNonAdjacentBySelector;
	};
	//#endregion

	//#region URL: /optimizer/level-2/optimize
	modules['/optimizer/level-2/optimize'] = function () {
		var mergeAdjacent = require('/optimizer/level-2/merge-adjacent');
		var mergeMediaQueries = require('/optimizer/level-2/merge-media-queries');
		var mergeNonAdjacentByBody = require('/optimizer/level-2/merge-non-adjacent-by-body');
		var mergeNonAdjacentBySelector = require('/optimizer/level-2/merge-non-adjacent-by-selector');
		var reduceNonAdjacent = require('/optimizer/level-2/reduce-non-adjacent');
		var removeDuplicateFontAtRules = require('/optimizer/level-2/remove-duplicate-font-at-rules');
		var removeDuplicateMediaQueries = require('/optimizer/level-2/remove-duplicate-media-queries');
		var removeDuplicates = require('/optimizer/level-2/remove-duplicates');
		var removeUnusedAtRules = require('/optimizer/level-2/remove-unused-at-rules');
		var restructure = require('/optimizer/level-2/restructure');

		var optimizeProperties = require('/optimizer/level-2/properties/optimize');

		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;

		var Token = require('/tokenizer/token');

		function removeEmpty(tokens) {
		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];
			var isEmpty = false;

			switch (token[0]) {
			  case Token.RULE:
				isEmpty = token[1].length === 0 || token[2].length === 0;
				break;
			  case Token.NESTED_BLOCK:
				removeEmpty(token[2]);
				isEmpty = token[2].length === 0;
				break;
			  case Token.AT_RULE:
				isEmpty = token[1].length === 0;
				break;
			  case Token.AT_RULE_BLOCK:
				isEmpty = token[2].length === 0;
			}

			if (isEmpty) {
			  tokens.splice(i, 1);
			  i--;
			  l--;
			}
		  }
		}

		function recursivelyOptimizeBlocks(tokens, context) {
		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];

			if (token[0] == Token.NESTED_BLOCK) {
			  var isKeyframes = /@(-moz-|-o-|-webkit-)?keyframes/.test(token[1][0][1]);
			  level2Optimize(token[2], context, !isKeyframes);
			}
		  }
		}

		function recursivelyOptimizeProperties(tokens, context) {
		  for (var i = 0, l = tokens.length; i < l; i++) {
			var token = tokens[i];

			switch (token[0]) {
			  case Token.RULE:
				optimizeProperties(token[2], true, true, context);
				break;
			  case Token.NESTED_BLOCK:
				recursivelyOptimizeProperties(token[2], context);
			}
		  }
		}

		function level2Optimize(tokens, context, withRestructuring) {
		  var levelOptions = context.options.level[OptimizationLevel.Two];
		  var reduced;
		  var i;

		  recursivelyOptimizeBlocks(tokens, context);
		  recursivelyOptimizeProperties(tokens, context);

		  if (levelOptions.removeDuplicateRules) {
			removeDuplicates(tokens, context);
		  }

		  if (levelOptions.mergeAdjacentRules) {
			mergeAdjacent(tokens, context);
		  }

		  if (levelOptions.reduceNonAdjacentRules) {
			reduceNonAdjacent(tokens, context);
		  }

		  if (levelOptions.mergeNonAdjacentRules && levelOptions.mergeNonAdjacentRules != 'body') {
			mergeNonAdjacentBySelector(tokens, context);
		  }

		  if (levelOptions.mergeNonAdjacentRules && levelOptions.mergeNonAdjacentRules != 'selector') {
			mergeNonAdjacentByBody(tokens, context);
		  }

		  if (levelOptions.restructureRules && levelOptions.mergeAdjacentRules && withRestructuring) {
			restructure(tokens, context);
			mergeAdjacent(tokens, context);
		  }

		  if (levelOptions.restructureRules && !levelOptions.mergeAdjacentRules && withRestructuring) {
			restructure(tokens, context);
		  }

		  if (levelOptions.removeDuplicateFontRules) {
			removeDuplicateFontAtRules(tokens, context);
		  }

		  if (levelOptions.removeDuplicateMediaBlocks) {
			removeDuplicateMediaQueries(tokens, context);
		  }

		  if (levelOptions.removeUnusedAtRules) {
			removeUnusedAtRules(tokens, context);
		  }

		  if (levelOptions.mergeMedia) {
			reduced = mergeMediaQueries(tokens, context);
			for (i = reduced.length - 1; i >= 0; i--) {
			  level2Optimize(reduced[i][2], context, false);
			}
		  }

		  if (levelOptions.removeEmpty) {
			removeEmpty(tokens);
		  }

		  return tokens;
		}

		return level2Optimize;
	};
	//#endregion

	//#region URL: /optimizer/level-2/reduce-non-adjacent
	modules['/optimizer/level-2/reduce-non-adjacent'] = function () {
		var isMergeable = require('/optimizer/level-2/is-mergeable');

		var optimizeProperties = require('/optimizer/level-2/properties/optimize');

		var cloneArray = require('/utils/clone-array');

		var Token = require('/tokenizer/token');

		var serializeBody = require('/writer/one-time').body;
		var serializeRules = require('/writer/one-time').rules;

		function reduceNonAdjacent(tokens, context) {
		  var options = context.options;
		  var mergeablePseudoClasses = options.compatibility.selectors.mergeablePseudoClasses;
		  var mergeablePseudoElements = options.compatibility.selectors.mergeablePseudoElements;
		  var multiplePseudoMerging = options.compatibility.selectors.multiplePseudoMerging;
		  var candidates = {};
		  var repeated = [];

		  for (var i = tokens.length - 1; i >= 0; i--) {
			var token = tokens[i];

			if (token[0] != Token.RULE) {
			  continue;
			} else if (token[2].length === 0) {
			  continue;
			}

			var selectorAsString = serializeRules(token[1]);
			var isComplexAndNotSpecial = token[1].length > 1 &&
			  isMergeable(selectorAsString, mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging);
			var wrappedSelectors = wrappedSelectorsFrom(token[1]);
			var selectors = isComplexAndNotSpecial ?
			  [selectorAsString].concat(wrappedSelectors) :
			  [selectorAsString];

			for (var j = 0, m = selectors.length; j < m; j++) {
			  var selector = selectors[j];

			  if (!candidates[selector])
				candidates[selector] = [];
			  else
				repeated.push(selector);

			  candidates[selector].push({
				where: i,
				list: wrappedSelectors,
				isPartial: isComplexAndNotSpecial && j > 0,
				isComplex: isComplexAndNotSpecial && j === 0
			  });
			}
		  }

		  reduceSimpleNonAdjacentCases(tokens, repeated, candidates, options, context);
		  reduceComplexNonAdjacentCases(tokens, candidates, options, context);
		}

		function wrappedSelectorsFrom(list) {
		  var wrapped = [];

		  for (var i = 0; i < list.length; i++) {
			wrapped.push([list[i][1]]);
		  }

		  return wrapped;
		}

		function reduceSimpleNonAdjacentCases(tokens, repeated, candidates, options, context) {
		  function filterOut(idx, bodies) {
			return data[idx].isPartial && bodies.length === 0;
		  }

		  function reduceBody(token, newBody, processedCount, tokenIdx) {
			if (!data[processedCount - tokenIdx - 1].isPartial)
			  token[2] = newBody;
		  }

		  for (var i = 0, l = repeated.length; i < l; i++) {
			var selector = repeated[i];
			var data = candidates[selector];

			reduceSelector(tokens, data, {
			  filterOut: filterOut,
			  callback: reduceBody
			}, options, context);
		  }
		}

		function reduceComplexNonAdjacentCases(tokens, candidates, options, context) {
		  var mergeablePseudoClasses = options.compatibility.selectors.mergeablePseudoClasses;
		  var mergeablePseudoElements = options.compatibility.selectors.mergeablePseudoElements;
		  var multiplePseudoMerging = options.compatibility.selectors.multiplePseudoMerging;
		  var localContext = {};

		  function filterOut(idx) {
			return localContext.data[idx].where < localContext.intoPosition;
		  }

		  function collectReducedBodies(token, newBody, processedCount, tokenIdx) {
			if (tokenIdx === 0)
			  localContext.reducedBodies.push(newBody);
		  }

		  allSelectors:
		  for (var complexSelector in candidates) {
			var into = candidates[complexSelector];
			if (!into[0].isComplex)
			  continue;

			var intoPosition = into[into.length - 1].where;
			var intoToken = tokens[intoPosition];
			var reducedBodies = [];

			var selectors = isMergeable(complexSelector, mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging) ?
			  into[0].list :
			  [complexSelector];

			localContext.intoPosition = intoPosition;
			localContext.reducedBodies = reducedBodies;

			for (var j = 0, m = selectors.length; j < m; j++) {
			  var selector = selectors[j];
			  var data = candidates[selector];

			  if (data.length < 2)
				continue allSelectors;

			  localContext.data = data;

			  reduceSelector(tokens, data, {
				filterOut: filterOut,
				callback: collectReducedBodies
			  }, options, context);

			  if (serializeBody(reducedBodies[reducedBodies.length - 1]) != serializeBody(reducedBodies[0]))
				continue allSelectors;
			}

			intoToken[2] = reducedBodies[0];
		  }
		}

		function reduceSelector(tokens, data, context, options, outerContext) {
		  var bodies = [];
		  var bodiesAsList = [];
		  var processedTokens = [];

		  for (var j = data.length - 1; j >= 0; j--) {
			if (context.filterOut(j, bodies))
			  continue;

			var where = data[j].where;
			var token = tokens[where];
			var clonedBody = cloneArray(token[2]);

			bodies = bodies.concat(clonedBody);
			bodiesAsList.push(clonedBody);
			processedTokens.push(where);
		  }

		  optimizeProperties(bodies, true, false, outerContext);

		  var processedCount = processedTokens.length;
		  var propertyIdx = bodies.length - 1;
		  var tokenIdx = processedCount - 1;

		  while (tokenIdx >= 0) {
			 if ((tokenIdx === 0 || (bodies[propertyIdx] && bodiesAsList[tokenIdx].indexOf(bodies[propertyIdx]) > -1)) && propertyIdx > -1) {
			  propertyIdx--;
			  continue;
			}

			var newBody = bodies.splice(propertyIdx + 1);
			context.callback(tokens[processedTokens[tokenIdx]], newBody, processedCount, tokenIdx);

			tokenIdx--;
		  }
		}

		return reduceNonAdjacent;
	};
	//#endregion

	//#region URL: /optimizer/level-2/remove-duplicate-font-at-rules
	modules['/optimizer/level-2/remove-duplicate-font-at-rules'] = function () {
		var Token = require('/tokenizer/token');

		var serializeAll = require('/writer/one-time').all;

		var FONT_FACE_SCOPE = '@font-face';

		function removeDuplicateFontAtRules(tokens) {
		  var fontAtRules = [];
		  var token;
		  var key;
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			token = tokens[i];

			if (token[0] != Token.AT_RULE_BLOCK && token[1][0][1] != FONT_FACE_SCOPE) {
			  continue;
			}

			key = serializeAll([token]);

			if (fontAtRules.indexOf(key) > -1) {
			  token[2] = [];
			} else {
			  fontAtRules.push(key);
			}
		  }
		}

		return removeDuplicateFontAtRules;
	};
	//#endregion

	//#region URL: /optimizer/level-2/remove-duplicate-media-queries
	modules['/optimizer/level-2/remove-duplicate-media-queries'] = function () {
		var Token = require('/tokenizer/token');

		var serializeAll = require('/writer/one-time').all;
		var serializeRules = require('/writer/one-time').rules;

		function removeDuplicateMediaQueries(tokens) {
		  var candidates = {};
		  var candidate;
		  var token;
		  var key;
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			token = tokens[i];
			if (token[0] != Token.NESTED_BLOCK) {
			  continue;
			}

			key = serializeRules(token[1]) + '%' + serializeAll(token[2]);
			candidate = candidates[key];

			if (candidate) {
			  candidate[2] = [];
			}

			candidates[key] = token;
		  }
		}

		return removeDuplicateMediaQueries;
	};
	//#endregion

	//#region URL: /optimizer/level-2/remove-duplicates
	modules['/optimizer/level-2/remove-duplicates'] = function () {
		var Token = require('/tokenizer/token');

		var serializeBody = require('/writer/one-time').body;
		var serializeRules = require('/writer/one-time').rules;

		function removeDuplicates(tokens) {
		  var matched = {};
		  var moreThanOnce = [];
		  var id, token;
		  var body, bodies;

		  for (var i = 0, l = tokens.length; i < l; i++) {
			token = tokens[i];
			if (token[0] != Token.RULE)
			  continue;

			id = serializeRules(token[1]);

			if (matched[id] && matched[id].length == 1)
			  moreThanOnce.push(id);
			else
			  matched[id] = matched[id] || [];

			matched[id].push(i);
		  }

		  for (i = 0, l = moreThanOnce.length; i < l; i++) {
			id = moreThanOnce[i];
			bodies = [];

			for (var j = matched[id].length - 1; j >= 0; j--) {
			  token = tokens[matched[id][j]];
			  body = serializeBody(token[2]);

			  if (bodies.indexOf(body) > -1)
				token[2] = [];
			  else
				bodies.push(body);
			}
		  }
		}

		return removeDuplicates;
	};
	//#endregion

	//#region URL: /optimizer/level-2/remove-unused-at-rules
	modules['/optimizer/level-2/remove-unused-at-rules'] = function () {
		var populateComponents = require('/optimizer/level-2/properties/populate-components');

		var wrapForOptimizing = require('/optimizer/wrap-for-optimizing').single;
		var restoreFromOptimizing = require('/optimizer/restore-from-optimizing');

		var Token = require('/tokenizer/token');

		var animationNameRegex = /^(\-moz\-|\-o\-|\-webkit\-)?animation-name$/;
		var animationRegex = /^(\-moz\-|\-o\-|\-webkit\-)?animation$/;
		var keyframeRegex = /^@(\-moz\-|\-o\-|\-webkit\-)?keyframes /;
		var importantRegex = /\s{0,31}!important$/;
		var optionalMatchingQuotesRegex = /^(['"]?)(.*)\1$/;

		function normalize(value) {
		  return value
			.replace(optionalMatchingQuotesRegex, '$2')
			.replace(importantRegex, '');
		}

		function removeUnusedAtRules(tokens, context) {
		  removeUnusedAtRule(tokens, matchCounterStyle, markCounterStylesAsUsed, context);
		  removeUnusedAtRule(tokens, matchFontFace, markFontFacesAsUsed, context);
		  removeUnusedAtRule(tokens, matchKeyframe, markKeyframesAsUsed, context);
		  removeUnusedAtRule(tokens, matchNamespace, markNamespacesAsUsed, context);
		}

		function removeUnusedAtRule(tokens, matchCallback, markCallback, context) {
		  var atRules = {};
		  var atRule;
		  var atRuleTokens;
		  var atRuleToken;
		  var zeroAt;
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			matchCallback(tokens[i], atRules);
		  }

		  if (Object.keys(atRules).length === 0) {
			return;
		  }

		  markUsedAtRules(tokens, markCallback, atRules, context);

		  for (atRule in atRules) {
			atRuleTokens = atRules[atRule];

			for (i = 0, l = atRuleTokens.length; i < l; i++) {
			  atRuleToken = atRuleTokens[i];
			  zeroAt = atRuleToken[0] == Token.AT_RULE ? 1 : 2;
			  atRuleToken[zeroAt] = [];
			}
		  }
		}

		function markUsedAtRules(tokens, markCallback, atRules, context) {
		  var boundMarkCallback = markCallback(atRules);
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			switch (tokens[i][0]) {
			  case Token.RULE:
				boundMarkCallback(tokens[i], context);
				break;
			  case Token.NESTED_BLOCK:
				markUsedAtRules(tokens[i][2], markCallback, atRules, context);
			}
		  }
		}

		function matchCounterStyle(token, atRules) {
		  var match;

		  if (token[0] == Token.AT_RULE_BLOCK && token[1][0][1].indexOf('@counter-style') === 0) {
			match = token[1][0][1].split(' ')[1];
			atRules[match] = atRules[match] || [];
			atRules[match].push(token);
		  }
		}

		function markCounterStylesAsUsed(atRules) {
		  return function (token, context) {
			var property;
			var wrappedProperty;
			var i, l;

			for (i = 0, l = token[2].length; i < l; i++) {
			  property = token[2][i];

			  if (property[1][1] == 'list-style') {
				wrappedProperty = wrapForOptimizing(property);
				populateComponents([wrappedProperty], context.validator, context.warnings);

				if (wrappedProperty.components[0].value[0][1] in atRules) {
				  delete atRules[property[2][1]];
				}

				restoreFromOptimizing([wrappedProperty]);
			  }

			  if (property[1][1] == 'list-style-type' && property[2][1] in atRules) {
				delete atRules[property[2][1]];
			  }
			}
		  };
		}

		function matchFontFace(token, atRules) {
		  var property;
		  var match;
		  var i, l;

		  if (token[0] == Token.AT_RULE_BLOCK && token[1][0][1] == '@font-face') {
			for (i = 0, l = token[2].length; i < l; i++) {
			  property = token[2][i];

			  if (property[1][1] == 'font-family') {
				match = normalize(property[2][1].toLowerCase());
				atRules[match] = atRules[match] || [];
				atRules[match].push(token);
				break;
			  }
			}
		  }
		}

		function markFontFacesAsUsed(atRules) {
		  return function (token, context) {
			var property;
			var wrappedProperty;
			var component;
			var normalizedMatch;
			var i, l;
			var j, m;

			for (i = 0, l = token[2].length; i < l; i++) {
			  property = token[2][i];

			  if (property[1][1] == 'font') {
				wrappedProperty = wrapForOptimizing(property);
				populateComponents([wrappedProperty], context.validator, context.warnings);
				component = wrappedProperty.components[6];

				for (j = 0, m = component.value.length; j < m; j++) {
				  normalizedMatch = normalize(component.value[j][1].toLowerCase());

				  if (normalizedMatch in atRules) {
					delete atRules[normalizedMatch];
				  }
				}

				restoreFromOptimizing([wrappedProperty]);
			  }

			  if (property[1][1] == 'font-family') {
				for (j = 2, m = property.length; j < m; j++) {
				  normalizedMatch = normalize(property[j][1].toLowerCase());

				  if (normalizedMatch in atRules) {
					delete atRules[normalizedMatch];
				  }
				}
			  }
			}
		  };
		}

		function matchKeyframe(token, atRules) {
		  var match;

		  if (token[0] == Token.NESTED_BLOCK && keyframeRegex.test(token[1][0][1])) {
			match = token[1][0][1].split(' ')[1];
			atRules[match] = atRules[match] || [];
			atRules[match].push(token);
		  }
		}

		function markKeyframesAsUsed(atRules) {
		  return function (token, context) {
			var property;
			var wrappedProperty;
			var component;
			var i, l;
			var j, m;

			for (i = 0, l = token[2].length; i < l; i++) {
			  property = token[2][i];

			  if (animationRegex.test(property[1][1])) {
				wrappedProperty = wrapForOptimizing(property);
				populateComponents([wrappedProperty], context.validator, context.warnings);
				component = wrappedProperty.components[7];

				for (j = 0, m = component.value.length; j < m; j++) {
				  if (component.value[j][1] in atRules) {
					delete atRules[component.value[j][1]];
				  }
				}

				restoreFromOptimizing([wrappedProperty]);
			  }

			  if (animationNameRegex.test(property[1][1])) {
				for (j = 2, m = property.length; j < m; j++) {
				  if (property[j][1] in atRules) {
					delete atRules[property[j][1]];
				  }
				}
			  }
			}
		  };
		}

		function matchNamespace(token, atRules) {
		  var match;

		  if (token[0] == Token.AT_RULE && token[1].indexOf('@namespace') === 0) {
			match = token[1].split(' ')[1];
			atRules[match] = atRules[match] || [];
			atRules[match].push(token);
		  }
		}

		function markNamespacesAsUsed(atRules) {
		  var namespaceRegex = new RegExp(Object.keys(atRules).join('\\\||') + '\\\|', 'g');

		  return function (token) {
			var match;
			var scope;
			var normalizedMatch;
			var i, l;
			var j, m;

			for (i = 0, l = token[1].length; i < l; i++) {
			  scope = token[1][i];
			  match = scope[1].match(namespaceRegex);

			  for (j = 0, m = match.length; j < m; j++) {
				normalizedMatch = match[j].substring(0, match[j].length - 1);

				if (normalizedMatch in atRules) {
				  delete atRules[normalizedMatch];
				}
			  }
			}
		  };
		}

		return removeUnusedAtRules;
	};
	//#endregion

	//#region URL: /optimizer/level-2/reorderable
	modules['/optimizer/level-2/reorderable'] = function () {
		// TODO: it'd be great to merge it with the other canReorder functionality

		var rulesOverlap = require('/optimizer/level-2/rules-overlap');
		var specificitiesOverlap = require('/optimizer/level-2/specificities-overlap');

		var FLEX_PROPERTIES = /align\-items|box\-align|box\-pack|flex|justify/;
		var BORDER_PROPERTIES = /^border\-(top|right|bottom|left|color|style|width|radius)/;

		function canReorder(left, right, cache) {
		  for (var i = right.length - 1; i >= 0; i--) {
			for (var j = left.length - 1; j >= 0; j--) {
			  if (!canReorderSingle(left[j], right[i], cache))
				return false;
			}
		  }

		  return true;
		}

		function canReorderSingle(left, right, cache) {
		  var leftName = left[0];
		  var leftValue = left[1];
		  var leftNameRoot = left[2];
		  var leftSelector = left[5];
		  var leftInSpecificSelector = left[6];
		  var rightName = right[0];
		  var rightValue = right[1];
		  var rightNameRoot = right[2];
		  var rightSelector = right[5];
		  var rightInSpecificSelector = right[6];

		  if (leftName == 'font' && rightName == 'line-height' || rightName == 'font' && leftName == 'line-height')
			return false;
		  if (FLEX_PROPERTIES.test(leftName) && FLEX_PROPERTIES.test(rightName))
			return false;
		  if (leftNameRoot == rightNameRoot && unprefixed(leftName) == unprefixed(rightName) && (vendorPrefixed(leftName) ^ vendorPrefixed(rightName)))
			return false;
		  if (leftNameRoot == 'border' && BORDER_PROPERTIES.test(rightNameRoot) && (leftName == 'border' || leftName == rightNameRoot || (leftValue != rightValue && sameBorderComponent(leftName, rightName))))
			return false;
		  if (rightNameRoot == 'border' && BORDER_PROPERTIES.test(leftNameRoot) && (rightName == 'border' || rightName == leftNameRoot || (leftValue != rightValue && sameBorderComponent(leftName, rightName))))
			return false;
		  if (leftNameRoot == 'border' && rightNameRoot == 'border' && leftName != rightName && (isSideBorder(leftName) && isStyleBorder(rightName) || isStyleBorder(leftName) && isSideBorder(rightName)))
			return false;
		  if (leftNameRoot != rightNameRoot)
			return true;
		  if (leftName == rightName && leftNameRoot == rightNameRoot && (leftValue == rightValue || withDifferentVendorPrefix(leftValue, rightValue)))
			return true;
		  if (leftName != rightName && leftNameRoot == rightNameRoot && leftName != leftNameRoot && rightName != rightNameRoot)
			return true;
		  if (leftName != rightName && leftNameRoot == rightNameRoot && leftValue == rightValue)
			return true;
		  if (rightInSpecificSelector && leftInSpecificSelector && !inheritable(leftNameRoot) && !inheritable(rightNameRoot) && !rulesOverlap(rightSelector, leftSelector, false))
			return true;
		  if (!specificitiesOverlap(leftSelector, rightSelector, cache))
			return true;

		  return false;
		}

		function vendorPrefixed(name) {
		  return /^\-(?:moz|webkit|ms|o)\-/.test(name);
		}

		function unprefixed(name) {
		  return name.replace(/^\-(?:moz|webkit|ms|o)\-/, '');
		}

		function sameBorderComponent(name1, name2) {
		  return name1.split('-').pop() == name2.split('-').pop();
		}

		function isSideBorder(name) {
		  return name == 'border-top' || name == 'border-right' || name == 'border-bottom' || name == 'border-left';
		}

		function isStyleBorder(name) {
		  return name == 'border-color' || name == 'border-style' || name == 'border-width';
		}

		function withDifferentVendorPrefix(value1, value2) {
		  return vendorPrefixed(value1) && vendorPrefixed(value2) && value1.split('-')[1] != value2.split('-')[2];
		}

		function inheritable(name) {
		  // According to http://www.w3.org/TR/CSS21/propidx.html
		  // Others will be catched by other, preceeding rules
		  return name == 'font' || name == 'line-height' || name == 'list-style';
		}

		var exports = {
		  canReorder: canReorder,
		  canReorderSingle: canReorderSingle
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/restore
	modules['/optimizer/level-2/restore'] = function () {
		var shallowClone = require('/optimizer/level-2/clone').shallow;

		var Token = require('/tokenizer/token');
		var Marker = require('/tokenizer/marker');

		function isInheritOnly(values) {
		  for (var i = 0, l = values.length; i < l; i++) {
			var value = values[i][1];

			if (value != 'inherit' && value != Marker.COMMA && value != Marker.FORWARD_SLASH)
			  return false;
		  }

		  return true;
		}

		function background(property, compactable, lastInMultiplex) {
		  var components = property.components;
		  var restored = [];
		  var needsOne, needsBoth;

		  function restoreValue(component) {
			Array.prototype.unshift.apply(restored, component.value);
		  }

		  function isDefaultValue(component) {
			var descriptor = compactable[component.name];

			if (descriptor.doubleValues && descriptor.defaultValue.length == 1) {
			  return component.value[0][1] == descriptor.defaultValue[0] && (component.value[1] ? component.value[1][1] == descriptor.defaultValue[0] : true);
			} else if (descriptor.doubleValues && descriptor.defaultValue.length != 1) {
			  return component.value[0][1] == descriptor.defaultValue[0] && (component.value[1] ? component.value[1][1] : component.value[0][1]) == descriptor.defaultValue[1];
			} else {
			  return component.value[0][1] == descriptor.defaultValue;
			}
		  }

		  for (var i = components.length - 1; i >= 0; i--) {
			var component = components[i];
			var isDefault = isDefaultValue(component);

			if (component.name == 'background-clip') {
			  var originComponent = components[i - 1];
			  var isOriginDefault = isDefaultValue(originComponent);

			  needsOne = component.value[0][1] == originComponent.value[0][1];

			  needsBoth = !needsOne && (
				(isOriginDefault && !isDefault) ||
				(!isOriginDefault && !isDefault) ||
				(!isOriginDefault && isDefault && component.value[0][1] != originComponent.value[0][1]));

			  if (needsOne) {
				restoreValue(originComponent);
			  } else if (needsBoth) {
				restoreValue(component);
				restoreValue(originComponent);
			  }

			  i--;
			} else if (component.name == 'background-size') {
			  var positionComponent = components[i - 1];
			  var isPositionDefault = isDefaultValue(positionComponent);

			  needsOne = !isPositionDefault && isDefault;

			  needsBoth = !needsOne &&
				(isPositionDefault && !isDefault || !isPositionDefault && !isDefault);

			  if (needsOne) {
				restoreValue(positionComponent);
			  } else if (needsBoth) {
				restoreValue(component);
				restored.unshift([Token.PROPERTY_VALUE, Marker.FORWARD_SLASH]);
				restoreValue(positionComponent);
			  } else if (positionComponent.value.length == 1) {
				restoreValue(positionComponent);
			  }

			  i--;
			} else {
			  if (isDefault || compactable[component.name].multiplexLastOnly && !lastInMultiplex)
				continue;

			  restoreValue(component);
			}
		  }

		  if (restored.length === 0 && property.value.length == 1 && property.value[0][1] == '0')
			restored.push(property.value[0]);

		  if (restored.length === 0)
			restored.push([Token.PROPERTY_VALUE, compactable[property.name].defaultValue]);

		  if (isInheritOnly(restored))
			return [restored[0]];

		  return restored;
		}

		function borderRadius(property, compactable) {
		  if (property.multiplex) {
			var horizontal = shallowClone(property);
			var vertical = shallowClone(property);

			for (var i = 0; i < 4; i++) {
			  var component = property.components[i];

			  var horizontalComponent = shallowClone(property);
			  horizontalComponent.value = [component.value[0]];
			  horizontal.components.push(horizontalComponent);

			  var verticalComponent = shallowClone(property);
			  // FIXME: only shorthand compactor (see breakup#borderRadius) knows that border radius
			  // longhands have two values, whereas tokenizer does not care about populating 2nd value
			  // if it's missing, hence this fallback
			  verticalComponent.value = [component.value[1] || component.value[0]];
			  vertical.components.push(verticalComponent);
			}

			var horizontalValues = fourValues(horizontal, compactable);
			var verticalValues = fourValues(vertical, compactable);

			if (horizontalValues.length == verticalValues.length &&
				horizontalValues[0][1] == verticalValues[0][1] &&
				(horizontalValues.length > 1 ? horizontalValues[1][1] == verticalValues[1][1] : true) &&
				(horizontalValues.length > 2 ? horizontalValues[2][1] == verticalValues[2][1] : true) &&
				(horizontalValues.length > 3 ? horizontalValues[3][1] == verticalValues[3][1] : true)) {
			  return horizontalValues;
			} else {
			  return horizontalValues.concat([[Token.PROPERTY_VALUE, Marker.FORWARD_SLASH]]).concat(verticalValues);
			}
		  } else {
			return fourValues(property, compactable);
		  }
		}

		function font(property, compactable) {
		  var components = property.components;
		  var restored = [];
		  var component;
		  var componentIndex = 0;
		  var fontFamilyIndex = 0;

		  if (property.value[0][1].indexOf(Marker.INTERNAL) === 0) {
			property.value[0][1] = property.value[0][1].substring(Marker.INTERNAL.length);
			return property.value;
		  }

		  // first four components are optional
		  while (componentIndex < 4) {
			component = components[componentIndex];

			if (component.value[0][1] != compactable[component.name].defaultValue) {
			  Array.prototype.push.apply(restored, component.value);
			}

			componentIndex++;
		  }

		  // then comes font-size
		  Array.prototype.push.apply(restored, components[componentIndex].value);
		  componentIndex++;

		  // then may come line-height
		  if (components[componentIndex].value[0][1] != compactable[components[componentIndex].name].defaultValue) {
			Array.prototype.push.apply(restored, [[Token.PROPERTY_VALUE, Marker.FORWARD_SLASH]]);
			Array.prototype.push.apply(restored, components[componentIndex].value);
		  }

		  componentIndex++;

		  // then comes font-family
		  while (components[componentIndex].value[fontFamilyIndex]) {
			restored.push(components[componentIndex].value[fontFamilyIndex]);

			if (components[componentIndex].value[fontFamilyIndex + 1]) {
			  restored.push([Token.PROPERTY_VALUE, Marker.COMMA]);
			}

			fontFamilyIndex++;
		  }

		  if (isInheritOnly(restored)) {
			return [restored[0]];
		  }

		  return restored;
		}

		function fourValues(property) {
		  var components = property.components;
		  var value1 = components[0].value[0];
		  var value2 = components[1].value[0];
		  var value3 = components[2].value[0];
		  var value4 = components[3].value[0];

		  if (value1[1] == value2[1] && value1[1] == value3[1] && value1[1] == value4[1]) {
			return [value1];
		  } else if (value1[1] == value3[1] && value2[1] == value4[1]) {
			return [value1, value2];
		  } else if (value2[1] == value4[1]) {
			return [value1, value2, value3];
		  } else {
			return [value1, value2, value3, value4];
		  }
		}

		function multiplex(restoreWith) {
		  return function (property, compactable) {
			if (!property.multiplex)
			  return restoreWith(property, compactable, true);

			var multiplexSize = 0;
			var restored = [];
			var componentMultiplexSoFar = {};
			var i, l;

			// At this point we don't know what's the multiplex size, e.g. how many background layers are there
			for (i = 0, l = property.components[0].value.length; i < l; i++) {
			  if (property.components[0].value[i][1] == Marker.COMMA)
				multiplexSize++;
			}

			for (i = 0; i <= multiplexSize; i++) {
			  var _property = shallowClone(property);

			  // We split multiplex into parts and restore them one by one
			  for (var j = 0, m = property.components.length; j < m; j++) {
				var componentToClone = property.components[j];
				var _component = shallowClone(componentToClone);
				_property.components.push(_component);

				// The trick is some properties has more than one value, so we iterate over values looking for
				// a multiplex separator - a comma
				for (var k = componentMultiplexSoFar[_component.name] || 0, n = componentToClone.value.length; k < n; k++) {
				  if (componentToClone.value[k][1] == Marker.COMMA) {
					componentMultiplexSoFar[_component.name] = k + 1;
					break;
				  }

				  _component.value.push(componentToClone.value[k]);
				}
			  }

			  // No we can restore shorthand value
			  var lastInMultiplex = i == multiplexSize;
			  var _restored = restoreWith(_property, compactable, lastInMultiplex);
			  Array.prototype.push.apply(restored, _restored);

			  if (i < multiplexSize)
				restored.push([Token.PROPERTY_VALUE, Marker.COMMA]);
			}

			return restored;
		  };
		}

		function withoutDefaults(property, compactable) {
		  var components = property.components;
		  var restored = [];

		  for (var i = components.length - 1; i >= 0; i--) {
			var component = components[i];
			var descriptor = compactable[component.name];

			if (component.value[0][1] != descriptor.defaultValue || ('keepUnlessDefault' in descriptor) && !isDefault(components, compactable, descriptor.keepUnlessDefault)) {
			  restored.unshift(component.value[0]);
			}
		  }

		  if (restored.length === 0)
			restored.push([Token.PROPERTY_VALUE, compactable[property.name].defaultValue]);

		  if (isInheritOnly(restored))
			return [restored[0]];

		  return restored;
		}

		function isDefault(components, compactable, propertyName) {
		  var component;
		  var i, l;

		  for (i = 0, l = components.length; i < l; i++) {
			component = components[i];

			if (component.name == propertyName && component.value[0][1] == compactable[propertyName].defaultValue) {
			  return true;
			}
		  }

		  return false;
		}

		var exports = {
		  background: background,
		  borderRadius: borderRadius,
		  font: font,
		  fourValues: fourValues,
		  multiplex: multiplex,
		  withoutDefaults: withoutDefaults
		};

		return exports;
	};
	//#endregion

	//#region URL: /optimizer/level-2/restore-with-components
	modules['/optimizer/level-2/restore-with-components'] = function () {
		var compactable = require('/optimizer/level-2/compactable');

		function restoreWithComponents(property) {
		  var descriptor = compactable[property.name];

		  if (descriptor && descriptor.shorthand) {
			return descriptor.restore(property, compactable);
		  } else {
			return property.value;
		  }
		}

		return restoreWithComponents;
	};
	//#endregion

	//#region URL: /optimizer/level-2/restructure
	modules['/optimizer/level-2/restructure'] = function () {
		var canReorderSingle = require('/optimizer/level-2/reorderable').canReorderSingle;
		var extractProperties = require('/optimizer/level-2/extract-properties');
		var isMergeable = require('/optimizer/level-2/is-mergeable');
		var tidyRuleDuplicates = require('/optimizer/level-2/tidy-rule-duplicates');

		var Token = require('/tokenizer/token');

		var cloneArray = require('/utils/clone-array');

		var serializeBody = require('/writer/one-time').body;
		var serializeRules = require('/writer/one-time').rules;

		function naturalSorter(a, b) {
		  return a > b ? 1 : -1;
		}

		function cloneAndMergeSelectors(propertyA, propertyB) {
		  var cloned = cloneArray(propertyA);
		  cloned[5] = cloned[5].concat(propertyB[5]);

		  return cloned;
		}

		function restructure(tokens, context) {
		  var options = context.options;
		  var mergeablePseudoClasses = options.compatibility.selectors.mergeablePseudoClasses;
		  var mergeablePseudoElements = options.compatibility.selectors.mergeablePseudoElements;
		  var mergeLimit = options.compatibility.selectors.mergeLimit;
		  var multiplePseudoMerging = options.compatibility.selectors.multiplePseudoMerging;
		  var specificityCache = context.cache.specificity;
		  var movableTokens = {};
		  var movedProperties = [];
		  var multiPropertyMoveCache = {};
		  var movedToBeDropped = [];
		  var maxCombinationsLevel = 2;
		  var ID_JOIN_CHARACTER = '%';

		  function sendToMultiPropertyMoveCache(position, movedProperty, allFits) {
			for (var i = allFits.length - 1; i >= 0; i--) {
			  var fit = allFits[i][0];
			  var id = addToCache(movedProperty, fit);

			  if (multiPropertyMoveCache[id].length > 1 && processMultiPropertyMove(position, multiPropertyMoveCache[id])) {
				removeAllMatchingFromCache(id);
				break;
			  }
			}
		  }

		  function addToCache(movedProperty, fit) {
			var id = cacheId(fit);
			multiPropertyMoveCache[id] = multiPropertyMoveCache[id] || [];
			multiPropertyMoveCache[id].push([movedProperty, fit]);
			return id;
		  }

		  function removeAllMatchingFromCache(matchId) {
			var matchSelectors = matchId.split(ID_JOIN_CHARACTER);
			var forRemoval = [];
			var i;

			for (var id in multiPropertyMoveCache) {
			  var selectors = id.split(ID_JOIN_CHARACTER);
			  for (i = selectors.length - 1; i >= 0; i--) {
				if (matchSelectors.indexOf(selectors[i]) > -1) {
				  forRemoval.push(id);
				  break;
				}
			  }
			}

			for (i = forRemoval.length - 1; i >= 0; i--) {
			  delete multiPropertyMoveCache[forRemoval[i]];
			}
		  }

		  function cacheId(cachedTokens) {
			var id = [];
			for (var i = 0, l = cachedTokens.length; i < l; i++) {
			  id.push(serializeRules(cachedTokens[i][1]));
			}
			return id.join(ID_JOIN_CHARACTER);
		  }

		  function tokensToMerge(sourceTokens) {
			var uniqueTokensWithBody = [];
			var mergeableTokens = [];

			for (var i = sourceTokens.length - 1; i >= 0; i--) {
			  if (!isMergeable(serializeRules(sourceTokens[i][1]), mergeablePseudoClasses, mergeablePseudoElements, multiplePseudoMerging)) {
				continue;
			  }

			  mergeableTokens.unshift(sourceTokens[i]);
			  if (sourceTokens[i][2].length > 0 && uniqueTokensWithBody.indexOf(sourceTokens[i]) == -1)
				uniqueTokensWithBody.push(sourceTokens[i]);
			}

			return uniqueTokensWithBody.length > 1 ?
			  mergeableTokens :
			  [];
		  }

		  function shortenIfPossible(position, movedProperty) {
			var name = movedProperty[0];
			var value = movedProperty[1];
			var key = movedProperty[4];
			var valueSize = name.length + value.length + 1;
			var allSelectors = [];
			var qualifiedTokens = [];

			var mergeableTokens = tokensToMerge(movableTokens[key]);
			if (mergeableTokens.length < 2)
			  return;

			var allFits = findAllFits(mergeableTokens, valueSize, 1);
			var bestFit = allFits[0];
			if (bestFit[1] > 0)
			  return sendToMultiPropertyMoveCache(position, movedProperty, allFits);

			for (var i = bestFit[0].length - 1; i >=0; i--) {
			  allSelectors = bestFit[0][i][1].concat(allSelectors);
			  qualifiedTokens.unshift(bestFit[0][i]);
			}

			allSelectors = tidyRuleDuplicates(allSelectors);
			dropAsNewTokenAt(position, [movedProperty], allSelectors, qualifiedTokens);
		  }

		  function fitSorter(fit1, fit2) {
			return fit1[1] > fit2[1] ? 1 : (fit1[1] == fit2[1] ? 0 : -1);
		  }

		  function findAllFits(mergeableTokens, propertySize, propertiesCount) {
			var combinations = allCombinations(mergeableTokens, propertySize, propertiesCount, maxCombinationsLevel - 1);
			return combinations.sort(fitSorter);
		  }

		  function allCombinations(tokensVariant, propertySize, propertiesCount, level) {
			var differenceVariants = [[tokensVariant, sizeDifference(tokensVariant, propertySize, propertiesCount)]];
			if (tokensVariant.length > 2 && level > 0) {
			  for (var i = tokensVariant.length - 1; i >= 0; i--) {
				var subVariant = Array.prototype.slice.call(tokensVariant, 0);
				subVariant.splice(i, 1);
				differenceVariants = differenceVariants.concat(allCombinations(subVariant, propertySize, propertiesCount, level - 1));
			  }
			}

			return differenceVariants;
		  }

		  function sizeDifference(tokensVariant, propertySize, propertiesCount) {
			var allSelectorsSize = 0;
			for (var i = tokensVariant.length - 1; i >= 0; i--) {
			  allSelectorsSize += tokensVariant[i][2].length > propertiesCount ? serializeRules(tokensVariant[i][1]).length : -1;
			}
			return allSelectorsSize - (tokensVariant.length - 1) * propertySize + 1;
		  }

		  function dropAsNewTokenAt(position, properties, allSelectors, mergeableTokens) {
			var i, j, k, m;
			var allProperties = [];

			for (i = mergeableTokens.length - 1; i >= 0; i--) {
			  var mergeableToken = mergeableTokens[i];

			  for (j = mergeableToken[2].length - 1; j >= 0; j--) {
				var mergeableProperty = mergeableToken[2][j];

				for (k = 0, m = properties.length; k < m; k++) {
				  var property = properties[k];

				  var mergeablePropertyName = mergeableProperty[1][1];
				  var propertyName = property[0];
				  var propertyBody = property[4];
				  if (mergeablePropertyName == propertyName && serializeBody([mergeableProperty]) == propertyBody) {
					mergeableToken[2].splice(j, 1);
					break;
				  }
				}
			  }
			}

			for (i = properties.length - 1; i >= 0; i--) {
			  allProperties.unshift(properties[i][3]);
			}

			var newToken = [Token.RULE, allSelectors, allProperties];
			tokens.splice(position, 0, newToken);
		  }

		  function dropPropertiesAt(position, movedProperty) {
			var key = movedProperty[4];
			var toMove = movableTokens[key];

			if (toMove && toMove.length > 1) {
			  if (!shortenMultiMovesIfPossible(position, movedProperty))
				shortenIfPossible(position, movedProperty);
			}
		  }

		  function shortenMultiMovesIfPossible(position, movedProperty) {
			var candidates = [];
			var propertiesAndMergableTokens = [];
			var key = movedProperty[4];
			var j, k;

			var mergeableTokens = tokensToMerge(movableTokens[key]);
			if (mergeableTokens.length < 2)
			  return;

			movableLoop:
			for (var value in movableTokens) {
			  var tokensList = movableTokens[value];

			  for (j = mergeableTokens.length - 1; j >= 0; j--) {
				if (tokensList.indexOf(mergeableTokens[j]) == -1)
				  continue movableLoop;
			  }

			  candidates.push(value);
			}

			if (candidates.length < 2)
			  return false;

			for (j = candidates.length - 1; j >= 0; j--) {
			  for (k = movedProperties.length - 1; k >= 0; k--) {
				if (movedProperties[k][4] == candidates[j]) {
				  propertiesAndMergableTokens.unshift([movedProperties[k], mergeableTokens]);
				  break;
				}
			  }
			}

			return processMultiPropertyMove(position, propertiesAndMergableTokens);
		  }

		  function processMultiPropertyMove(position, propertiesAndMergableTokens) {
			var valueSize = 0;
			var properties = [];
			var property;

			for (var i = propertiesAndMergableTokens.length - 1; i >= 0; i--) {
			  property = propertiesAndMergableTokens[i][0];
			  var fullValue = property[4];
			  valueSize += fullValue.length + (i > 0 ? 1 : 0);

			  properties.push(property);
			}

			var mergeableTokens = propertiesAndMergableTokens[0][1];
			var bestFit = findAllFits(mergeableTokens, valueSize, properties.length)[0];
			if (bestFit[1] > 0)
			  return false;

			var allSelectors = [];
			var qualifiedTokens = [];
			for (i = bestFit[0].length - 1; i >= 0; i--) {
			  allSelectors = bestFit[0][i][1].concat(allSelectors);
			  qualifiedTokens.unshift(bestFit[0][i]);
			}

			allSelectors = tidyRuleDuplicates(allSelectors);
			dropAsNewTokenAt(position, properties, allSelectors, qualifiedTokens);

			for (i = properties.length - 1; i >= 0; i--) {
			  property = properties[i];
			  var index = movedProperties.indexOf(property);

			  delete movableTokens[property[4]];

			  if (index > -1 && movedToBeDropped.indexOf(index) == -1)
				movedToBeDropped.push(index);
			}

			return true;
		  }

		  function boundToAnotherPropertyInCurrrentToken(property, movedProperty, token) {
			var propertyName = property[0];
			var movedPropertyName = movedProperty[0];
			if (propertyName != movedPropertyName)
			  return false;

			var key = movedProperty[4];
			var toMove = movableTokens[key];
			return toMove && toMove.indexOf(token) > -1;
		  }

		  for (var i = tokens.length - 1; i >= 0; i--) {
			var token = tokens[i];
			var isRule;
			var j, k, m;
			var samePropertyAt;

			if (token[0] == Token.RULE) {
			  isRule = true;
			} else if (token[0] == Token.NESTED_BLOCK) {
			  isRule = false;
			} else {
			  continue;
			}

			// We cache movedProperties.length as it may change in the loop
			var movedCount = movedProperties.length;

			var properties = extractProperties(token);
			movedToBeDropped = [];

			var unmovableInCurrentToken = [];
			for (j = properties.length - 1; j >= 0; j--) {
			  for (k = j - 1; k >= 0; k--) {
				if (!canReorderSingle(properties[j], properties[k], specificityCache)) {
				  unmovableInCurrentToken.push(j);
				  break;
				}
			  }
			}

			for (j = properties.length - 1; j >= 0; j--) {
			  var property = properties[j];
			  var movedSameProperty = false;

			  for (k = 0; k < movedCount; k++) {
				var movedProperty = movedProperties[k];

				if (movedToBeDropped.indexOf(k) == -1 && (!canReorderSingle(property, movedProperty, specificityCache) && !boundToAnotherPropertyInCurrrentToken(property, movedProperty, token) ||
					movableTokens[movedProperty[4]] && movableTokens[movedProperty[4]].length === mergeLimit)) {
				  dropPropertiesAt(i + 1, movedProperty, token);

				  if (movedToBeDropped.indexOf(k) == -1) {
					movedToBeDropped.push(k);
					delete movableTokens[movedProperty[4]];
				  }
				}

				if (!movedSameProperty) {
				  movedSameProperty = property[0] == movedProperty[0] && property[1] == movedProperty[1];

				  if (movedSameProperty) {
					samePropertyAt = k;
				  }
				}
			  }

			  if (!isRule || unmovableInCurrentToken.indexOf(j) > -1)
				continue;

			  var key = property[4];

			  if (movedSameProperty && movedProperties[samePropertyAt][5].length + property[5].length > mergeLimit) {
				dropPropertiesAt(i + 1, movedProperties[samePropertyAt]);
				movedProperties.splice(samePropertyAt, 1);
				movableTokens[key] = [token];
				movedSameProperty = false;
			  } else {
				movableTokens[key] = movableTokens[key] || [];
				movableTokens[key].push(token);
			  }

			  if (movedSameProperty) {
				movedProperties[samePropertyAt] = cloneAndMergeSelectors(movedProperties[samePropertyAt], property);
			  } else {
				movedProperties.push(property);
			  }
			}

			movedToBeDropped = movedToBeDropped.sort(naturalSorter);
			for (j = 0, m = movedToBeDropped.length; j < m; j++) {
			  var dropAt = movedToBeDropped[j] - j;
			  movedProperties.splice(dropAt, 1);
			}
		  }

		  var position = tokens[0] && tokens[0][0] == Token.AT_RULE && tokens[0][1].indexOf('@charset') === 0 ? 1 : 0;
		  for (; position < tokens.length - 1; position++) {
			var isImportRule = tokens[position][0] === Token.AT_RULE && tokens[position][1].indexOf('@import') === 0;
			var isComment = tokens[position][0] === Token.COMMENT;
			if (!(isImportRule || isComment))
			  break;
		  }

		  for (i = 0; i < movedProperties.length; i++) {
			dropPropertiesAt(position, movedProperties[i]);
		  }
		}

		return restructure;
	};
	//#endregion

	//#region URL: /optimizer/level-2/rules-overlap
	modules['/optimizer/level-2/rules-overlap'] = function () {
		var MODIFIER_PATTERN = /\-\-.+$/;

		function rulesOverlap(rule1, rule2, bemMode) {
		  var scope1;
		  var scope2;
		  var i, l;
		  var j, m;

		  for (i = 0, l = rule1.length; i < l; i++) {
			scope1 = rule1[i][1];

			for (j = 0, m = rule2.length; j < m; j++) {
			  scope2 = rule2[j][1];

			  if (scope1 == scope2) {
				return true;
			  }

			  if (bemMode && withoutModifiers(scope1) == withoutModifiers(scope2)) {
				return true;
			  }
			}
		  }

		  return false;
		}

		function withoutModifiers(scope) {
		  return scope.replace(MODIFIER_PATTERN, '');
		}

		return rulesOverlap;
	};
	//#endregion

	//#region URL: /optimizer/level-2/specificities-overlap
	modules['/optimizer/level-2/specificities-overlap'] = function () {
		var specificity = require('/optimizer/level-2/specificity');

		function specificitiesOverlap(selector1, selector2, cache) {
		  var specificity1;
		  var specificity2;
		  var i, l;
		  var j, m;

		  for (i = 0, l = selector1.length; i < l; i++) {
			specificity1 = findSpecificity(selector1[i][1], cache);

			for (j = 0, m = selector2.length; j < m; j++) {
			  specificity2 = findSpecificity(selector2[j][1], cache);

			  if (specificity1[0] === specificity2[0] && specificity1[1] === specificity2[1] && specificity1[2] === specificity2[2]) {
				return true;
			  }
			}
		  }

		  return false;
		}

		function findSpecificity(selector, cache) {
		  var value;

		  if (!(selector in cache)) {
			cache[selector] = value = specificity(selector);
		  }

		  return value || cache[selector];
		}

		return specificitiesOverlap;
	};
	//#endregion

	//#region URL: /optimizer/level-2/specificity
	modules['/optimizer/level-2/specificity'] = function () {
		var Marker = require('/tokenizer/marker');

		var Selector = {
		  ADJACENT_SIBLING: '+',
		  DESCENDANT: '>',
		  DOT: '.',
		  HASH: '#',
		  NON_ADJACENT_SIBLING: '~',
		  PSEUDO: ':'
		};

		var LETTER_PATTERN = /[a-zA-Z]/;
		var NOT_PREFIX = ':not(';
		var SEPARATOR_PATTERN = /[\s,\(>~\+]/;

		function specificity(selector) {
		  var result = [0, 0, 0];
		  var character;
		  var isEscaped;
		  var isSingleQuoted;
		  var isDoubleQuoted;
		  var roundBracketLevel = 0;
		  var couldIntroduceNewTypeSelector;
		  var withinNotPseudoClass = false;
		  var wasPseudoClass = false;
		  var i, l;

		  for (i = 0, l = selector.length; i < l; i++) {
			character = selector[i];

			if (isEscaped) {
			  // noop
			} else if (character == Marker.SINGLE_QUOTE && !isDoubleQuoted && !isSingleQuoted) {
			  isSingleQuoted = true;
			} else if (character == Marker.SINGLE_QUOTE && !isDoubleQuoted && isSingleQuoted) {
			  isSingleQuoted = false;
			} else if (character == Marker.DOUBLE_QUOTE && !isDoubleQuoted && !isSingleQuoted) {
			  isDoubleQuoted = true;
			} else if (character == Marker.DOUBLE_QUOTE && isDoubleQuoted && !isSingleQuoted) {
			  isDoubleQuoted = false;
			} else if (isSingleQuoted || isDoubleQuoted) {
			  continue;
			} else if (roundBracketLevel > 0 && !withinNotPseudoClass) {
			  // noop
			} else if (character == Marker.OPEN_ROUND_BRACKET) {
			  roundBracketLevel++;
			} else if (character == Marker.CLOSE_ROUND_BRACKET && roundBracketLevel == 1) {
			  roundBracketLevel--;
			  withinNotPseudoClass = false;
			} else if (character == Marker.CLOSE_ROUND_BRACKET) {
			  roundBracketLevel--;
			} else if (character == Selector.HASH) {
			  result[0]++;
			} else if (character == Selector.DOT || character == Marker.OPEN_SQUARE_BRACKET) {
			  result[1]++;
			} else if (character == Selector.PSEUDO && !wasPseudoClass && !isNotPseudoClass(selector, i)) {
			  result[1]++;
			  withinNotPseudoClass = false;
			} else if (character == Selector.PSEUDO) {
			  withinNotPseudoClass = true;
			} else if ((i === 0 || couldIntroduceNewTypeSelector) && LETTER_PATTERN.test(character)) {
			  result[2]++;
			}

			isEscaped = character == Marker.BACK_SLASH;
			wasPseudoClass = character == Selector.PSEUDO;
			couldIntroduceNewTypeSelector = !isEscaped && SEPARATOR_PATTERN.test(character);
		  }

		  return result;
		}

		function isNotPseudoClass(selector, index) {
		  return selector.indexOf(NOT_PREFIX, index) === index;
		}

		return specificity;
	};
	//#endregion

	//#region URL: /optimizer/level-2/tidy-rule-duplicates
	modules['/optimizer/level-2/tidy-rule-duplicates'] = function () {
		function ruleSorter(s1, s2) {
		  return s1[1] > s2[1] ? 1 : -1;
		}

		function tidyRuleDuplicates(rules) {
		  var list = [];
		  var repeated = [];

		  for (var i = 0, l = rules.length; i < l; i++) {
			var rule = rules[i];

			if (repeated.indexOf(rule[1]) == -1) {
			  repeated.push(rule[1]);
			  list.push(rule);
			}
		  }

		  return list.sort(ruleSorter);
		}

		return tidyRuleDuplicates;
	};
	//#endregion

	//#region URL: /optimizer/hack
	modules['/optimizer/hack'] = function () {
		var Hack = {
		  ASTERISK: 'asterisk',
		  BANG: 'bang',
		  BACKSLASH: 'backslash',
		  UNDERSCORE: 'underscore'
		};

		return Hack;
	};
	//#endregion

	//#region URL: /optimizer/remove-unused
	modules['/optimizer/remove-unused'] = function () {
		function removeUnused(properties) {
		  for (var i = properties.length - 1; i >= 0; i--) {
			var property = properties[i];

			if (property.unused) {
			  property.all.splice(property.position, 1);
			}
		  }
		}

		return removeUnused;
	};
	//#endregion

	//#region URL: /optimizer/restore-from-optimizing
	modules['/optimizer/restore-from-optimizing'] = function () {
		var Hack = require('/optimizer/hack');

		var Marker = require('/tokenizer/marker');

		var ASTERISK_HACK = '*';
		var BACKSLASH_HACK = '\\';
		var IMPORTANT_TOKEN = '!important';
		var UNDERSCORE_HACK = '_';
		var BANG_HACK = '!ie';

		function restoreFromOptimizing(properties, restoreCallback) {
		  var property;
		  var restored;
		  var current;
		  var i;

		  for (i = properties.length - 1; i >= 0; i--) {
			property = properties[i];

			if (property.unused) {
			  continue;
			}

			if (!property.dirty && !property.important && !property.hack) {
			  continue;
			}

			if (restoreCallback) {
			  restored = restoreCallback(property);
			  property.value = restored;
			} else {
			  restored = property.value;
			}

			if (property.important) {
			  restoreImportant(property);
			}

			if (property.hack) {
			  restoreHack(property);
			}

			if ('all' in property) {
			  current = property.all[property.position];
			  current[1][1] = property.name;

			  current.splice(2, current.length - 1);
			  Array.prototype.push.apply(current, restored);
			}
		  }
		}

		function restoreImportant(property) {
		  property.value[property.value.length - 1][1] += IMPORTANT_TOKEN;
		}

		function restoreHack(property) {
		  if (property.hack[0] == Hack.UNDERSCORE) {
			property.name = UNDERSCORE_HACK + property.name;
		  } else if (property.hack[0] == Hack.ASTERISK) {
			property.name = ASTERISK_HACK + property.name;
		  } else if (property.hack[0] == Hack.BACKSLASH) {
			property.value[property.value.length - 1][1] += BACKSLASH_HACK + property.hack[1];
		  } else if (property.hack[0] == Hack.BANG) {
			property.value[property.value.length - 1][1] += Marker.SPACE + BANG_HACK;
		  }
		}

		return restoreFromOptimizing;
	};
	//#endregion

	//#region URL: /optimizer/validator
	modules['/optimizer/validator'] = function () {
		var functionNoVendorRegexStr = '[A-Z]+(\\-|[A-Z]|[0-9])+\\(.*?\\)';
		var functionVendorRegexStr = '\\-(\\-|[A-Z]|[0-9])+\\(.*?\\)';
		var variableRegexStr = 'var\\(\\-\\-[^\\)]+\\)';
		var functionAnyRegexStr = '(' + variableRegexStr + '|' + functionNoVendorRegexStr + '|' + functionVendorRegexStr + ')';

		var animationTimingFunctionRegex = /^(cubic\-bezier|steps)\([^\)]+\)$/;
		var calcRegex = new RegExp('^(\\-moz\\-|\\-webkit\\-)?calc\\([^\\)]+\\)$', 'i');
		var decimalRegex = /[0-9]/;
		var functionAnyRegex = new RegExp('^' + functionAnyRegexStr + '$', 'i');
		var hslColorRegex = /^hsl\(\s{0,31}[\-\.]?\d+\s{0,31},\s{0,31}\.?\d+%\s{0,31},\s{0,31}\.?\d+%\s{0,31}\)|hsla\(\s{0,31}[\-\.]?\d+\s{0,31},\s{0,31}\.?\d+%\s{0,31},\s{0,31}\.?\d+%\s{0,31},\s{0,31}\.?\d+\s{0,31}\)$/;
		var identifierRegex = /^(\-[a-z0-9_][a-z0-9\-_]*|[a-z][a-z0-9\-_]*)$/i;
		var longHexColorRegex = /^#[0-9a-f]{6}$/i;
		var namedEntityRegex = /^[a-z]+$/i;
		var prefixRegex = /^-([a-z0-9]|-)*$/i;
		var rgbColorRegex = /^rgb\(\s{0,31}[\d]{1,3}\s{0,31},\s{0,31}[\d]{1,3}\s{0,31},\s{0,31}[\d]{1,3}\s{0,31}\)|rgba\(\s{0,31}[\d]{1,3}\s{0,31},\s{0,31}[\d]{1,3}\s{0,31},\s{0,31}[\d]{1,3}\s{0,31},\s{0,31}[\.\d]+\s{0,31}\)$/;
		var shortHexColorRegex = /^#[0-9a-f]{3}$/i;
		var validTimeUnits = ['ms', 's'];
		var urlRegex = /^url\([\s\S]+\)$/i;
		var variableRegex = new RegExp('^' + variableRegexStr + '$', 'i');

		var DECIMAL_DOT = '.';
		var MINUS_SIGN = '-';
		var PLUS_SIGN = '+';

		var Keywords = {
		  '^': [
			'inherit',
			'initial',
			'unset'
		  ],
		  '*-style': [
			'auto',
			'dashed',
			'dotted',
			'double',
			'groove',
			'hidden',
			'inset',
			'none',
			'outset',
			'ridge',
			'solid'
		  ],
		  'animation-direction': [
			'alternate',
			'alternate-reverse',
			'normal',
			'reverse'
		  ],
		  'animation-fill-mode': [
			'backwards',
			'both',
			'forwards',
			'none'
		  ],
		  'animation-iteration-count': [
			'infinite'
		  ],
		  'animation-name': [
			'none'
		  ],
		  'animation-play-state': [
			'paused',
			'running'
		  ],
		  'animation-timing-function': [
			'ease',
			'ease-in',
			'ease-in-out',
			'ease-out',
			'linear',
			'step-end',
			'step-start'
		  ],
		  'background-attachment': [
			'fixed',
			'inherit',
			'local',
			'scroll'
		  ],
		  'background-clip': [
			'border-box',
			'content-box',
			'inherit',
			'padding-box',
			'text'
		  ],
		  'background-origin': [
			'border-box',
			'content-box',
			'inherit',
			'padding-box'
		  ],
		  'background-position': [
			'bottom',
			'center',
			'left',
			'right',
			'top'
		  ],
		  'background-repeat': [
			'no-repeat',
			'inherit',
			'repeat',
			'repeat-x',
			'repeat-y',
			'round',
			'space'
		  ],
		  'background-size': [
			'auto',
			'cover',
			'contain'
		  ],
		  'border-collapse': [
			'collapse',
			'inherit',
			'separate'
		  ],
		  'bottom': [
			'auto'
		  ],
		  'clear': [
			'both',
			'left',
			'none',
			'right'
		  ],
		  'color': [
			'transparent'
		  ],
		  'cursor': [
			'all-scroll',
			'auto',
			'col-resize',
			'crosshair',
			'default',
			'e-resize',
			'help',
			'move',
			'n-resize',
			'ne-resize',
			'no-drop',
			'not-allowed',
			'nw-resize',
			'pointer',
			'progress',
			'row-resize',
			's-resize',
			'se-resize',
			'sw-resize',
			'text',
			'vertical-text',
			'w-resize',
			'wait'
		  ],
		  'display': [
			'block',
			'inline',
			'inline-block',
			'inline-table',
			'list-item',
			'none',
			'table',
			'table-caption',
			'table-cell',
			'table-column',
			'table-column-group',
			'table-footer-group',
			'table-header-group',
			'table-row',
			'table-row-group'
		  ],
		  'float': [
			'left',
			'none',
			'right'
		  ],
		  'left': [
			'auto'
		  ],
		  'font': [
			'caption',
			'icon',
			'menu',
			'message-box',
			'small-caption',
			'status-bar',
			'unset'
		  ],
		  'font-size': [
			'large',
			'larger',
			'medium',
			'small',
			'smaller',
			'x-large',
			'x-small',
			'xx-large',
			'xx-small'
		  ],
		  'font-stretch': [
			'condensed',
			'expanded',
			'extra-condensed',
			'extra-expanded',
			'normal',
			'semi-condensed',
			'semi-expanded',
			'ultra-condensed',
			'ultra-expanded'
		  ],
		  'font-style': [
			'italic',
			'normal',
			'oblique'
		  ],
		  'font-variant': [
			'normal',
			'small-caps'
		  ],
		  'font-weight': [
			'100',
			'200',
			'300',
			'400',
			'500',
			'600',
			'700',
			'800',
			'900',
			'bold',
			'bolder',
			'lighter',
			'normal'
		  ],
		  'line-height': [
			'normal'
		  ],
		  'list-style-position': [
			'inside',
			'outside'
		  ],
		  'list-style-type': [
			'armenian',
			'circle',
			'decimal',
			'decimal-leading-zero',
			'disc',
			'decimal|disc', // this is the default value of list-style-type, see comment in compactable.js
			'georgian',
			'lower-alpha',
			'lower-greek',
			'lower-latin',
			'lower-roman',
			'none',
			'square',
			'upper-alpha',
			'upper-latin',
			'upper-roman'
		  ],
		  'overflow': [
			'auto',
			'hidden',
			'scroll',
			'visible'
		  ],
		  'position': [
			'absolute',
			'fixed',
			'relative',
			'static'
		  ],
		  'right': [
			'auto'
		  ],
		  'text-align': [
			'center',
			'justify',
			'left',
			'left|right', // this is the default value of list-style-type, see comment in compactable.js
			'right'
		  ],
		  'text-decoration': [
			'line-through',
			'none',
			'overline',
			'underline'
		  ],
		  'text-overflow': [
			'clip',
			'ellipsis'
		  ],
		  'top': [
			'auto'
		  ],
		  'vertical-align': [
			'baseline',
			'bottom',
			'middle',
			'sub',
			'super',
			'text-bottom',
			'text-top',
			'top'
		  ],
		  'visibility': [
			'collapse',
			'hidden',
			'visible'
		  ],
		  'white-space': [
			'normal',
			'nowrap',
			'pre'
		  ],
		  'width': [
			'inherit',
			'initial',
			'medium',
			'thick',
			'thin'
		  ]
		};

		var Units = [
		  '%',
		  'ch',
		  'cm',
		  'em',
		  'ex',
		  'in',
		  'mm',
		  'pc',
		  'pt',
		  'px',
		  'rem',
		  'vh',
		  'vm',
		  'vmax',
		  'vmin',
		  'vw'
		];

		function isAnimationTimingFunction() {
		  var isTimingFunctionKeyword = isKeyword('animation-timing-function');

		  return function (value) {
			return isTimingFunctionKeyword(value) || animationTimingFunctionRegex.test(value);
		  };
		}

		function isColor(value) {
		  return value != 'auto' &&
			(
			  isKeyword('color')(value) ||
			  isHexColor(value) ||
			  isColorFunction(value) ||
			  isNamedEntity(value)
			);
		}

		function isColorFunction(value) {
		  return isRgbColor(value) || isHslColor(value);
		}

		function isDynamicUnit(value) {
		  return calcRegex.test(value);
		}

		function isFunction(value) {
		  return functionAnyRegex.test(value);
		}

		function isHexColor(value) {
		  return shortHexColorRegex.test(value) || longHexColorRegex.test(value);
		}

		function isHslColor(value) {
		  return hslColorRegex.test(value);
		}

		function isIdentifier(value) {
		  return identifierRegex.test(value);
		}

		function isImage(value) {
		  return value == 'none' || value == 'inherit' || isUrl(value);
		}

		function isKeyword(propertyName) {
		  return function(value) {
			return Keywords[propertyName].indexOf(value) > -1;
		  };
		}

		function isNamedEntity(value) {
		  return namedEntityRegex.test(value);
		}

		function isNumber(value) {
		  return scanForNumber(value) == value.length;
		}

		function isRgbColor(value) {
		  return rgbColorRegex.test(value);
		}

		function isPrefixed(value) {
		  return prefixRegex.test(value);
		}

		function isPositiveNumber(value) {
		  return isNumber(value) &&
			parseFloat(value) >= 0;
		}

		function isVariable(value) {
		  return variableRegex.test(value);
		}

		function isTime(value) {
		  var numberUpTo = scanForNumber(value);

		  return numberUpTo == value.length && parseInt(value) === 0 ||
			numberUpTo > -1 && validTimeUnits.indexOf(value.slice(numberUpTo + 1)) > -1;
		}

		function isUnit(validUnits, value) {
		  var numberUpTo = scanForNumber(value);

		  return numberUpTo == value.length && parseInt(value) === 0 ||
			numberUpTo > -1 && validUnits.indexOf(value.slice(numberUpTo + 1)) > -1 ||
			value == 'auto' ||
			value == 'inherit';
		}

		function isUrl(value) {
		  return urlRegex.test(value);
		}

		function isZIndex(value) {
		  return value == 'auto' ||
			isNumber(value) ||
			isKeyword('^')(value);
		}

		function scanForNumber(value) {
		  var hasDot = false;
		  var hasSign = false;
		  var character;
		  var i, l;

		  for (i = 0, l = value.length; i < l; i++) {
			character = value[i];

			if (i === 0 && (character == PLUS_SIGN || character == MINUS_SIGN)) {
			  hasSign = true;
			} else if (i > 0 && hasSign && (character == PLUS_SIGN || character == MINUS_SIGN)) {
			  return i - 1;
			} else if (character == DECIMAL_DOT && !hasDot) {
			  hasDot = true;
			} else if (character == DECIMAL_DOT && hasDot) {
			  return i - 1;
			} else if (decimalRegex.test(character)) {
			  continue;
			} else {
			  return i - 1;
			}
		  }

		  return i;
		}

		function validator(compatibility) {
		  var validUnits = Units.slice(0).filter(function (value) {
			return !(value in compatibility.units) || compatibility.units[value] === true;
		  });

		  return {
			colorOpacity: compatibility.colors.opacity,
			isAnimationDirectionKeyword: isKeyword('animation-direction'),
			isAnimationFillModeKeyword: isKeyword('animation-fill-mode'),
			isAnimationIterationCountKeyword: isKeyword('animation-iteration-count'),
			isAnimationNameKeyword: isKeyword('animation-name'),
			isAnimationPlayStateKeyword: isKeyword('animation-play-state'),
			isAnimationTimingFunction: isAnimationTimingFunction(),
			isBackgroundAttachmentKeyword: isKeyword('background-attachment'),
			isBackgroundClipKeyword: isKeyword('background-clip'),
			isBackgroundOriginKeyword: isKeyword('background-origin'),
			isBackgroundPositionKeyword: isKeyword('background-position'),
			isBackgroundRepeatKeyword: isKeyword('background-repeat'),
			isBackgroundSizeKeyword: isKeyword('background-size'),
			isColor: isColor,
			isColorFunction: isColorFunction,
			isDynamicUnit: isDynamicUnit,
			isFontKeyword: isKeyword('font'),
			isFontSizeKeyword: isKeyword('font-size'),
			isFontStretchKeyword: isKeyword('font-stretch'),
			isFontStyleKeyword: isKeyword('font-style'),
			isFontVariantKeyword: isKeyword('font-variant'),
			isFontWeightKeyword: isKeyword('font-weight'),
			isFunction: isFunction,
			isGlobal: isKeyword('^'),
			isHslColor: isHslColor,
			isIdentifier: isIdentifier,
			isImage: isImage,
			isKeyword: isKeyword,
			isLineHeightKeyword: isKeyword('line-height'),
			isListStylePositionKeyword: isKeyword('list-style-position'),
			isListStyleTypeKeyword: isKeyword('list-style-type'),
			isNumber: isNumber,
			isPrefixed: isPrefixed,
			isPositiveNumber: isPositiveNumber,
			isRgbColor: isRgbColor,
			isStyleKeyword: isKeyword('*-style'),
			isTime: isTime,
			isUnit: isUnit.bind(null, validUnits),
			isUrl: isUrl,
			isVariable: isVariable,
			isWidth: isKeyword('width'),
			isZIndex: isZIndex
		  };
		}

		return validator;
	};
	//#endregion

	//#region URL: /optimizer/wrap-for-optimizing
	modules['/optimizer/wrap-for-optimizing'] = function () {
		var Hack = require('/optimizer/hack');

		var Marker = require('/tokenizer/marker');
		var Token = require('/tokenizer/token');

		var Match = {
		  ASTERISK: '*',
		  BACKSLASH: '\\',
		  BANG: '!',
		  BANG_SUFFIX_PATTERN: /!\w+$/,
		  IMPORTANT_TOKEN: '!important',
		  IMPORTANT_TOKEN_PATTERN: new RegExp('!important$', 'i'),
		  IMPORTANT_WORD: 'important',
		  IMPORTANT_WORD_PATTERN: new RegExp('important$', 'i'),
		  SUFFIX_BANG_PATTERN: /!$/,
		  UNDERSCORE: '_',
		  VARIABLE_REFERENCE_PATTERN: /var\(--.+\)$/
		};

		function wrapAll(properties, includeVariable, skipProperties) {
		  var wrapped = [];
		  var single;
		  var property;
		  var i;

		  for (i = properties.length - 1; i >= 0; i--) {
			property = properties[i];

			if (property[0] != Token.PROPERTY) {
			  continue;
			}

			if (!includeVariable && someVariableReferences(property)) {
			  continue;
			}

			if (skipProperties && skipProperties.indexOf(property[1][1]) > -1) {
			  continue;
			}

			single = wrapSingle(property);
			single.all = properties;
			single.position = i;
			wrapped.unshift(single);
		  }

		  return wrapped;
		}

		function someVariableReferences(property) {
		  var i, l;
		  var value;

		  // skipping `property` and property name tokens
		  for (i = 2, l = property.length; i < l; i++) {
			value = property[i];

			if (value[0] != Token.PROPERTY_VALUE) {
			  continue;
			}

			if (isVariableReference(value[1])) {
			  return true;
			}
		  }

		  return false;
		}

		function isVariableReference(value) {
		  return Match.VARIABLE_REFERENCE_PATTERN.test(value);
		}

		function isMultiplex(property) {
		  var value;
		  var i, l;

		  for (i = 3, l = property.length; i < l; i++) {
			value = property[i];

			if (value[0] == Token.PROPERTY_VALUE && (value[1] == Marker.COMMA || value[1] == Marker.FORWARD_SLASH)) {
			  return true;
			}
		  }

		  return false;
		}

		function hackFrom(property) {
		  var match = false;
		  var name = property[1][1];
		  var lastValue = property[property.length - 1];

		  if (name[0] == Match.UNDERSCORE) {
			match = [Hack.UNDERSCORE];
		  } else if (name[0] == Match.ASTERISK) {
			match = [Hack.ASTERISK];
		  } else if (lastValue[1][0] == Match.BANG && !lastValue[1].match(Match.IMPORTANT_WORD_PATTERN)) {
			match = [Hack.BANG];
		  } else if (lastValue[1].indexOf(Match.BANG) > 0 && !lastValue[1].match(Match.IMPORTANT_WORD_PATTERN) && Match.BANG_SUFFIX_PATTERN.test(lastValue[1])) {
			match = [Hack.BANG];
		  } else if (lastValue[1].indexOf(Match.BACKSLASH) > 0 && lastValue[1].indexOf(Match.BACKSLASH) == lastValue[1].length - Match.BACKSLASH.length - 1) {
			match = [Hack.BACKSLASH, lastValue[1].substring(lastValue[1].indexOf(Match.BACKSLASH) + 1)];
		  } else if (lastValue[1].indexOf(Match.BACKSLASH) === 0 && lastValue[1].length == 2) {
			match = [Hack.BACKSLASH, lastValue[1].substring(1)];
		  }

		  return match;
		}

		function isImportant(property) {
		  if (property.length < 3)
			return false;

		  var lastValue = property[property.length - 1];
		  if (Match.IMPORTANT_TOKEN_PATTERN.test(lastValue[1])) {
			return true;
		  } else if (Match.IMPORTANT_WORD_PATTERN.test(lastValue[1]) && Match.SUFFIX_BANG_PATTERN.test(property[property.length - 2][1])) {
			return true;
		  }

		  return false;
		}

		function stripImportant(property) {
		  var lastValue = property[property.length - 1];
		  var oneButLastValue = property[property.length - 2];

		  if (Match.IMPORTANT_TOKEN_PATTERN.test(lastValue[1])) {
			lastValue[1] = lastValue[1].replace(Match.IMPORTANT_TOKEN_PATTERN, '');
		  } else {
			lastValue[1] = lastValue[1].replace(Match.IMPORTANT_WORD_PATTERN, '');
			oneButLastValue[1] = oneButLastValue[1].replace(Match.SUFFIX_BANG_PATTERN, '');
		  }

		  if (lastValue[1].length === 0) {
			property.pop();
		  }

		  if (oneButLastValue[1].length === 0) {
			property.pop();
		  }
		}

		function stripPrefixHack(property) {
		  property[1][1] = property[1][1].substring(1);
		}

		function stripSuffixHack(property, hackFrom) {
		  var lastValue = property[property.length - 1];
		  lastValue[1] = lastValue[1]
			.substring(0, lastValue[1].indexOf(hackFrom[0] == Hack.BACKSLASH ? Match.BACKSLASH : Match.BANG))
			.trim();

		  if (lastValue[1].length === 0) {
			property.pop();
		  }
		}

		function wrapSingle(property) {
		  var importantProperty = isImportant(property);
		  if (importantProperty) {
			stripImportant(property);
		  }

		  var whichHack = hackFrom(property);
		  if (whichHack[0] == Hack.ASTERISK || whichHack[0] == Hack.UNDERSCORE) {
			stripPrefixHack(property);
		  } else if (whichHack[0] == Hack.BACKSLASH || whichHack[0] == Hack.BANG) {
			stripSuffixHack(property, whichHack);
		  }

		  return {
			block: property[2] && property[2][0] == Token.PROPERTY_BLOCK,
			components: [],
			dirty: false,
			hack: whichHack,
			important: importantProperty,
			name: property[1][1],
			multiplex: property.length > 3 ? isMultiplex(property) : false,
			position: 0,
			shorthand: false,
			unused: false,
			value: property.slice(2)
		  };
		}

		var exports = {
		  all: wrapAll,
		  single: wrapSingle
		};

		return exports;
	};
	//#endregion

	//#region URL: /options/compatibility
	modules['/options/compatibility'] = function () {
		var DEFAULTS = {
		  '*': {
			colors: {
			  opacity: true // rgba / hsla
			},
			properties: {
			  backgroundClipMerging: true, // background-clip to shorthand
			  backgroundOriginMerging: true, // background-origin to shorthand
			  backgroundSizeMerging: true, // background-size to shorthand
			  colors: true, // any kind of color transformations, like `#ff00ff` to `#f0f` or `#fff` into `red`
			  ieBangHack: false, // !ie suffix hacks on IE<8
			  ieFilters: false, // whether to preserve `filter` and `-ms-filter` properties
			  iePrefixHack: false, // underscore / asterisk prefix hacks on IE
			  ieSuffixHack: false, // \9 suffix hacks on IE6-9
			  merging: true, // merging properties into one
			  shorterLengthUnits: false, // optimize pixel units into `pt`, `pc` or `in` units
			  spaceAfterClosingBrace: true, // 'url() no-repeat' to 'url()no-repeat'
			  urlQuotes: false, // whether to wrap content of `url()` into quotes or not
			  zeroUnits: true // 0[unit] -> 0
			},
			selectors: {
			  adjacentSpace: false, // div+ nav Android stock browser hack
			  ie7Hack: false, // *+html hack
			  mergeablePseudoClasses: [
				':active',
				':after',
				':before',
				':empty',
				':checked',
				':disabled',
				':empty',
				':enabled',
				':first-child',
				':first-letter',
				':first-line',
				':first-of-type',
				':focus',
				':hover',
				':lang',
				':last-child',
				':last-of-type',
				':link',
				':not',
				':nth-child',
				':nth-last-child',
				':nth-last-of-type',
				':nth-of-type',
				':only-child',
				':only-of-type',
				':root',
				':target',
				':visited'
			  ], // selectors with these pseudo-classes can be merged as these are universally supported
			  mergeablePseudoElements: [
				'::after',
				'::before',
				'::first-letter',
				'::first-line'
			  ], // selectors with these pseudo-elements can be merged as these are universally supported
			  mergeLimit: 8191, // number of rules that can be safely merged together
			  multiplePseudoMerging: true
			},
			units: {
			  ch: true,
			  in: true,
			  pc: true,
			  pt: true,
			  rem: true,
			  vh: true,
			  vm: true, // vm is vmin on IE9+ see https://developer.mozilla.org/en-US/docs/Web/CSS/length
			  vmax: true,
			  vmin: true,
			  vw: true
			}
		  }
		};

		DEFAULTS.ie11 = DEFAULTS['*'];

		DEFAULTS.ie10 = DEFAULTS['*'];

		DEFAULTS.ie9 = merge(DEFAULTS['*'], {
		  properties: {
			ieFilters: true,
			ieSuffixHack: true
		  }
		});

		DEFAULTS.ie8 = merge(DEFAULTS.ie9, {
		  colors: {
			opacity: false
		  },
		  properties: {
			backgroundClipMerging: false,
			backgroundOriginMerging: false,
			backgroundSizeMerging: false,
			iePrefixHack: true,
			merging: false
		  },
		  selectors: {
			mergeablePseudoClasses: [
			  ':after',
			  ':before',
			  ':first-child',
			  ':first-letter',
			  ':focus',
			  ':hover',
			  ':visited'
			],
			mergeablePseudoElements: []
		  },
		  units: {
			ch: false,
			rem: false,
			vh: false,
			vm: false,
			vmax: false,
			vmin: false,
			vw: false
		  }
		});

		DEFAULTS.ie7 = merge(DEFAULTS.ie8, {
		  properties: {
			ieBangHack: true
		  },
		  selectors: {
			ie7Hack: true,
			mergeablePseudoClasses: [
			  ':first-child',
			  ':first-letter',
			  ':hover',
			  ':visited'
			]
		  },
		});

		function compatibilityFrom(source) {
		  return merge(DEFAULTS['*'], calculateSource(source));
		}

		function merge(source, target) {
		  for (var key in source) {
			var value = source[key];

			if (typeof value === 'object' && !Array.isArray(value)) {
			  target[key] = merge(value, target[key] || {});
			} else {
			  target[key] = key in target ? target[key] : value;
			}
		  }

		  return target;
		}

		function calculateSource(source) {
		  if (typeof source == 'object')
			return source;

		  if (!/[,\+\-]/.test(source))
			return DEFAULTS[source] || DEFAULTS['*'];

		  var parts = source.split(',');
		  var template = parts[0] in DEFAULTS ?
			DEFAULTS[parts.shift()] :
			DEFAULTS['*'];

		  source = {};

		  parts.forEach(function (part) {
			var isAdd = part[0] == '+';
			var key = part.substring(1).split('.');
			var group = key[0];
			var option = key[1];

			source[group] = source[group] || {};
			source[group][option] = isAdd;
		  });

		  return merge(template, source);
		}

		return compatibilityFrom;
	};
	//#endregion

	//#region URL: /options/format
	modules['/options/format'] = function () {
		var override = require('/utils/override');

		var Breaks = {
		  AfterAtRule: 'afterAtRule',
		  AfterBlockBegins: 'afterBlockBegins',
		  AfterBlockEnds: 'afterBlockEnds',
		  AfterComment: 'afterComment',
		  AfterProperty: 'afterProperty',
		  AfterRuleBegins: 'afterRuleBegins',
		  AfterRuleEnds: 'afterRuleEnds',
		  BeforeBlockEnds: 'beforeBlockEnds',
		  BetweenSelectors: 'betweenSelectors'
		};

		var IndentWith = {
		  Space: ' ',
		  Tab: '\t'
		};

		var Spaces = {
		  AroundSelectorRelation: 'aroundSelectorRelation',
		  BeforeBlockBegins: 'beforeBlockBegins',
		  BeforeValue: 'beforeValue'
		};

		var DEFAULTS = {
		  breaks: breaks(false),
		  indentBy: 0,
		  indentWith: IndentWith.Space,
		  spaces: spaces(false),
		  wrapAt: false
		};

		var BEAUTIFY_ALIAS = 'beautify';
		var KEEP_BREAKS_ALIAS = 'keep-breaks';

		var OPTION_SEPARATOR = ';';
		var OPTION_NAME_VALUE_SEPARATOR = ':';
		var HASH_VALUES_OPTION_SEPARATOR = ',';
		var HASH_VALUES_NAME_VALUE_SEPARATOR = '=';

		var FALSE_KEYWORD_1 = 'false';
		var FALSE_KEYWORD_2 = 'off';
		var TRUE_KEYWORD_1 = 'true';
		var TRUE_KEYWORD_2 = 'on';

		function breaks(value) {
		  var breakOptions = {};

		  breakOptions[Breaks.AfterAtRule] = value;
		  breakOptions[Breaks.AfterBlockBegins] = value;
		  breakOptions[Breaks.AfterBlockEnds] = value;
		  breakOptions[Breaks.AfterComment] = value;
		  breakOptions[Breaks.AfterProperty] = value;
		  breakOptions[Breaks.AfterRuleBegins] = value;
		  breakOptions[Breaks.AfterRuleEnds] = value;
		  breakOptions[Breaks.BeforeBlockEnds] = value;
		  breakOptions[Breaks.BetweenSelectors] = value;

		  return breakOptions;
		}

		function spaces(value) {
		  var spaceOptions = {};

		  spaceOptions[Spaces.AroundSelectorRelation] = value;
		  spaceOptions[Spaces.BeforeBlockBegins] = value;
		  spaceOptions[Spaces.BeforeValue] = value;

		  return spaceOptions;
		}

		function formatFrom(source) {
		  if (source === undefined || source === false) {
			return false;
		  }

		  if (typeof source == 'object' && 'indentBy' in source) {
			source = override(source, { indentBy: parseInt(source.indentBy) });
		  }

		  if (typeof source == 'object' && 'indentWith' in source) {
			source = override(source, { indentWith: mapIndentWith(source.indentWith) });
		  }

		  if (typeof source == 'object') {
			return override(DEFAULTS, source);
		  }

		  if (typeof source == 'object') {
			return override(DEFAULTS, source);
		  }

		  if (typeof source == 'string' && source == BEAUTIFY_ALIAS) {
			return override(DEFAULTS, {
			  breaks: breaks(true),
			  indentBy: 2,
			  spaces: spaces(true)
			});
		  }

		  if (typeof source == 'string' && source == KEEP_BREAKS_ALIAS) {
			return override(DEFAULTS, {
			  breaks: {
				afterAtRule: true,
				afterBlockBegins: true,
				afterBlockEnds: true,
				afterComment: true,
				afterRuleEnds: true,
				beforeBlockEnds: true
			  }
			});
		  }

		  if (typeof source == 'string') {
			return override(DEFAULTS, toHash(source));
		  }

		  return DEFAULTS;
		}

		function toHash(string) {
		  return string
			.split(OPTION_SEPARATOR)
			.reduce(function (accumulator, directive) {
			  var parts = directive.split(OPTION_NAME_VALUE_SEPARATOR);
			  var name = parts[0];
			  var value = parts[1];

			  if (name == 'breaks' || name == 'spaces') {
				accumulator[name] = hashValuesToHash(value);
			  } else if (name == 'indentBy' || name == 'wrapAt') {
				accumulator[name] = parseInt(value);
			  } else if (name == 'indentWith') {
				accumulator[name] = mapIndentWith(value);
			  }

			  return accumulator;
			}, {});
		}

		function hashValuesToHash(string) {
		  return string
			.split(HASH_VALUES_OPTION_SEPARATOR)
			.reduce(function (accumulator, directive) {
			  var parts = directive.split(HASH_VALUES_NAME_VALUE_SEPARATOR);
			  var name = parts[0];
			  var value = parts[1];

			  accumulator[name] = normalizeValue(value);

			  return accumulator;
			}, {});
		}


		function normalizeValue(value) {
		  switch (value) {
			case FALSE_KEYWORD_1:
			case FALSE_KEYWORD_2:
			  return false;
			case TRUE_KEYWORD_1:
			case TRUE_KEYWORD_2:
			  return true;
			default:
			  return value;
		  }
		}

		function mapIndentWith(value) {
		  switch (value) {
			case 'space':
			  return IndentWith.Space;
			case 'tab':
			  return IndentWith.Tab;
			default:
			  return value;
		  }
		}

		var exports = {
		  Breaks: Breaks,
		  Spaces: Spaces,
		  formatFrom: formatFrom
		};

		return exports;
	};
	//#endregion

	//#region URL: /options/optimization-level
	modules['/options/optimization-level'] = function () {
		var roundingPrecisionFrom = require('/options/rounding-precision').roundingPrecisionFrom;

		var override = require('/utils/override');

		var OptimizationLevel = {
		  Zero: '0',
		  One: '1',
		  Two: '2'
		};

		var DEFAULTS = {};

		DEFAULTS[OptimizationLevel.Zero] = {};
		DEFAULTS[OptimizationLevel.One] = {
		  cleanupCharsets: true,
		  normalizeUrls: true,
		  optimizeBackground: true,
		  optimizeBorderRadius: true,
		  optimizeFilter: true,
		  optimizeFontWeight: true,
		  optimizeOutline: true,
		  removeEmpty: true,
		  removeNegativePaddings: true,
		  removeQuotes: true,
		  removeWhitespace: true,
		  replaceMultipleZeros: true,
		  replaceTimeUnits: true,
		  replaceZeroUnits: true,
		  roundingPrecision: roundingPrecisionFrom(undefined),
		  selectorsSortingMethod: 'standard',
		  specialComments: 'all',
		  tidyAtRules: true,
		  tidyBlockScopes: true,
		  tidySelectors: true,
		  transform: noop
		};
		DEFAULTS[OptimizationLevel.Two] = {
		  mergeAdjacentRules: true,
		  mergeIntoShorthands: true,
		  mergeMedia: true,
		  mergeNonAdjacentRules: true,
		  mergeSemantically: false,
		  overrideProperties: true,
		  removeEmpty: true,
		  reduceNonAdjacentRules: true,
		  removeDuplicateFontRules: true,
		  removeDuplicateMediaBlocks: true,
		  removeDuplicateRules: true,
		  removeUnusedAtRules: false,
		  restructureRules: false,
		  skipProperties: []
		};

		var ALL_KEYWORD_1 = '*';
		var ALL_KEYWORD_2 = 'all';
		var FALSE_KEYWORD_1 = 'false';
		var FALSE_KEYWORD_2 = 'off';
		var TRUE_KEYWORD_1 = 'true';
		var TRUE_KEYWORD_2 = 'on';

		var LIST_VALUE_SEPARATOR = ',';
		var OPTION_SEPARATOR = ';';
		var OPTION_VALUE_SEPARATOR = ':';

		function noop() {}

		function optimizationLevelFrom(source) {
		  var level = override(DEFAULTS, {});
		  var Zero = OptimizationLevel.Zero;
		  var One = OptimizationLevel.One;
		  var Two = OptimizationLevel.Two;


		  if (undefined === source) {
			delete level[Two];
			return level;
		  }

		  if (typeof source == 'string') {
			source = parseInt(source);
		  }

		  if (typeof source == 'number' && source === parseInt(Two)) {
			return level;
		  }

		  if (typeof source == 'number' && source === parseInt(One)) {
			delete level[Two];
			return level;
		  }

		  if (typeof source == 'number' && source === parseInt(Zero)) {
			delete level[Two];
			delete level[One];
			return level;
		  }

		  if (typeof source == 'object') {
			source = covertValuesToHashes(source);
		  }

		  if (One in source && 'roundingPrecision' in source[One]) {
			source[One].roundingPrecision = roundingPrecisionFrom(source[One].roundingPrecision);
		  }

		  if (Two in source && 'skipProperties' in source[Two] && typeof(source[Two].skipProperties) == 'string') {
			source[Two].skipProperties = source[Two].skipProperties.split(LIST_VALUE_SEPARATOR);
		  }

		  if (Zero in source || One in source || Two in source) {
			level[Zero] = override(level[Zero], source[Zero]);
		  }

		  if (One in source && ALL_KEYWORD_1 in source[One]) {
			level[One] = override(level[One], defaults(One, normalizeValue(source[One][ALL_KEYWORD_1])));
			delete source[One][ALL_KEYWORD_1];
		  }

		  if (One in source && ALL_KEYWORD_2 in source[One]) {
			level[One] = override(level[One], defaults(One, normalizeValue(source[One][ALL_KEYWORD_2])));
			delete source[One][ALL_KEYWORD_2];
		  }

		  if (One in source || Two in source) {
			level[One] = override(level[One], source[One]);
		  } else {
			delete level[One];
		  }

		  if (Two in source && ALL_KEYWORD_1 in source[Two]) {
			level[Two] = override(level[Two], defaults(Two, normalizeValue(source[Two][ALL_KEYWORD_1])));
			delete source[Two][ALL_KEYWORD_1];
		  }

		  if (Two in source && ALL_KEYWORD_2 in source[Two]) {
			level[Two] = override(level[Two], defaults(Two, normalizeValue(source[Two][ALL_KEYWORD_2])));
			delete source[Two][ALL_KEYWORD_2];
		  }

		  if (Two in source) {
			level[Two] = override(level[Two], source[Two]);
		  } else {
			delete level[Two];
		  }

		  return level;
		}

		function defaults(level, value) {
		  var options = override(DEFAULTS[level], {});
		  var key;

		  for (key in options) {
			if (typeof options[key] == 'boolean') {
			  options[key] = value;
			}
		  }

		  return options;
		}

		function normalizeValue(value) {
		  switch (value) {
			case FALSE_KEYWORD_1:
			case FALSE_KEYWORD_2:
			  return false;
			case TRUE_KEYWORD_1:
			case TRUE_KEYWORD_2:
			  return true;
			default:
			  return value;
		  }
		}

		function covertValuesToHashes(source) {
		  var clonedSource = override(source, {});
		  var level;
		  var i;

		  for (i = 0; i <= 2; i++) {
			level = '' + i;

			if (level in clonedSource && (clonedSource[level] === undefined || clonedSource[level] === false)) {
			  delete clonedSource[level];
			}

			if (level in clonedSource && clonedSource[level] === true) {
			  clonedSource[level] = {};
			}

			if (level in clonedSource && typeof clonedSource[level] == 'string') {
			  clonedSource[level] = covertToHash(clonedSource[level], level);
			}
		  }

		  return clonedSource;
		}

		function covertToHash(asString, level) {
		  return asString
			.split(OPTION_SEPARATOR)
			.reduce(function (accumulator, directive) {
			  var parts = directive.split(OPTION_VALUE_SEPARATOR);
			  var name = parts[0];
			  var value = parts[1];
			  var normalizedValue = normalizeValue(value);

			  if (ALL_KEYWORD_1 == name || ALL_KEYWORD_2 == name) {
				accumulator = override(accumulator, defaults(level, normalizedValue));
			  } else {
				accumulator[name] = normalizedValue;
			  }

			  return accumulator;
			}, {});
		}

		var exports = {
		  OptimizationLevel: OptimizationLevel,
		  optimizationLevelFrom: optimizationLevelFrom,
		};

		return exports;
	};
	//#endregion

	//#region URL: /options/rounding-precision
	modules['/options/rounding-precision'] = function () {
		var override = require('/utils/override');

		var INTEGER_PATTERN = /^\d+$/;

		var ALL_UNITS = ['*', 'all'];
		var DEFAULT_PRECISION = 'off'; // all precision changes are disabled
		var DIRECTIVES_SEPARATOR = ','; // e.g. *=5,px=3
		var DIRECTIVE_VALUE_SEPARATOR = '='; // e.g. *=5

		function roundingPrecisionFrom(source) {
		  return override(defaults(DEFAULT_PRECISION), buildPrecisionFrom(source));
		}

		function defaults(value) {
		  return {
			'ch': value,
			'cm': value,
			'em': value,
			'ex': value,
			'in': value,
			'mm': value,
			'pc': value,
			'pt': value,
			'px': value,
			'q': value,
			'rem': value,
			'vh': value,
			'vmax': value,
			'vmin': value,
			'vw': value,
			'%': value
		  };
		}

		function buildPrecisionFrom(source) {
		  if (source === null || source === undefined) {
			return {};
		  }

		  if (typeof source == 'boolean') {
			return {};
		  }

		  if (typeof source == 'number' && source == -1) {
			return defaults(DEFAULT_PRECISION);
		  }

		  if (typeof source == 'number') {
			return defaults(source);
		  }

		  if (typeof source == 'string' && INTEGER_PATTERN.test(source)) {
			return defaults(parseInt(source));
		  }

		  if (typeof source == 'string' && source == DEFAULT_PRECISION) {
			return defaults(DEFAULT_PRECISION);
		  }

		  if (typeof source == 'object') {
			return source;
		  }

		  return source
			.split(DIRECTIVES_SEPARATOR)
			.reduce(function (accumulator, directive) {
			  var directiveParts = directive.split(DIRECTIVE_VALUE_SEPARATOR);
			  var name = directiveParts[0];
			  var value = parseInt(directiveParts[1]);

			  if (isNaN(value) || value == -1) {
				value = DEFAULT_PRECISION;
			  }

			  if (ALL_UNITS.indexOf(name) > -1) {
				accumulator = override(accumulator, defaults(value));
			  } else {
				accumulator[name] = value;
			  }

			  return accumulator;
			}, {});
		}

		var exports = {
		  DEFAULT: DEFAULT_PRECISION,
		  roundingPrecisionFrom: roundingPrecisionFrom
		};

		return exports;
	};
	//#endregion

	//#region URL: /reader/read-sources
	modules['/reader/read-sources'] = function () {
		/*BT-
		var fs = require('fs');
		var path = require('path');

		var applySourceMaps = require('/reader/apply-source-maps');
		var extractImportUrlAndMedia = require('/reader/extract-import-url-and-media');
		var isAllowedResource = require('/reader/is-allowed-resource');
		var loadOriginalSources = require('/reader/load-original-sources');
		var normalizePath = require('/reader/normalize-path');
		var rebase = require('/reader/rebase');
		var rebaseLocalMap = require('/reader/rebase-local-map');
		var rebaseRemoteMap = require('/reader/rebase-remote-map');
		var restoreImport = require('/reader/restore-import');
		*/

		var tokenize = require('/tokenizer/tokenize');
		/*BT-
		var Token = require('/tokenizer/token');
		var Marker = require('/tokenizer/marker');
		var hasProtocol = require('/utils/has-protocol');
		var isImport = require('/utils/is-import');
		var isRemoteResource = require('/utils/is-remote-resource');

		var UNKNOWN_URI = 'uri:unknown';
		*/

		function readSources(input, context, callback) {
		  return doReadSources(input, context, /*BT- function (tokens) {
			return applySourceMaps(tokens, context, function () {
			  return loadOriginalSources(context, function () { return callback(tokens); });
			});
		  }*/
		    callback //BT+
		  );
		}

		function doReadSources(input, context, callback) {
		  /*BT-
		  if (typeof input == 'string') {
		  */
			return fromString(input, context, callback);
		  /*BT-
		  } else if (Buffer.isBuffer(input)) {
			return fromString(input.toString(), context, callback);
		  } else if (Array.isArray(input)) {
			return fromArray(input, context, callback);
		  } else if (typeof input == 'object') {
			return fromHash(input, context, callback);
		  }
		  */
		}

		function fromString(input, context, callback) {
		  context.source = undefined;
		  context.sourcesContent[undefined] = input;
		  /*BT-
		  context.stats.originalSize += input.length;
		  */

		  return fromStyles(input, context, /*BT- { inline: context.options.inline }*/null, callback);
		}

		/*BT-
		function fromArray(input, context, callback) {
		  var inputAsImports = input.reduce(function (accumulator, uriOrHash) {
			if (typeof uriOrHash === 'string') {
			  return addStringSource(uriOrHash, accumulator);
			} else {
			  return addHashSource(uriOrHash, context, accumulator);
			}

		  }, []);

		  return fromStyles(inputAsImports.join(''), context, { inline: ['all'] }, callback);
		}

		function fromHash(input, context, callback) {
		  var inputAsImports = addHashSource(input, context, []);
		  return fromStyles(inputAsImports.join(''), context, { inline: ['all'] }, callback);
		}

		function addStringSource(input, imports) {
		  imports.push(restoreAsImport(normalizeUri(input)));
		  return imports;
		}

		function addHashSource(input, context, imports) {
		  var uri;
		  var normalizedUri;
		  var source;

		  for (uri in input) {
			source = input[uri];
			normalizedUri = normalizeUri(uri);

			imports.push(restoreAsImport(normalizedUri));

			context.sourcesContent[normalizedUri] = source.styles;

			if (source.sourceMap) {
			  trackSourceMap(source.sourceMap, normalizedUri, context);
			}
		  }

		  return imports;
		}

		function normalizeUri(uri) {
		  var currentPath = path.resolve('');
		  var absoluteUri;
		  var relativeToCurrentPath;
		  var normalizedUri;

		  if (isRemoteResource(uri)) {
			return uri;
		  }

		  absoluteUri = path.isAbsolute(uri) ?
			uri :
			path.resolve(uri);
		  relativeToCurrentPath = path.relative(currentPath, absoluteUri);
		  normalizedUri = normalizePath(relativeToCurrentPath);

		  return normalizedUri;
		}

		function trackSourceMap(sourceMap, uri, context) {
		  var parsedMap = typeof sourceMap == 'string' ?
			  JSON.parse(sourceMap) :
			  sourceMap;
		  var rebasedMap = isRemoteResource(uri) ?
			rebaseRemoteMap(parsedMap, uri) :
			rebaseLocalMap(parsedMap, uri || UNKNOWN_URI, context.options.rebaseTo);

		  context.inputSourceMapTracker.track(uri, rebasedMap);
		}

		function restoreAsImport(uri) {
		  return restoreImport('url(' + uri + ')', '') + Marker.SEMICOLON;
		}
		*/

		function fromStyles(styles, context, parentInlinerContext, callback) {
		  var tokens;
		  /*BT-
		  var rebaseConfig = {};

		  if (!context.source) {
			rebaseConfig.fromBase = path.resolve('');
			rebaseConfig.toBase = context.options.rebaseTo;
		  } else if (isRemoteResource(context.source)) {
			rebaseConfig.fromBase = context.source;
			rebaseConfig.toBase = context.source;
		  } else if (path.isAbsolute(context.source)) {
			rebaseConfig.fromBase = path.dirname(context.source);
			rebaseConfig.toBase = context.options.rebaseTo;
		  } else {
			rebaseConfig.fromBase = path.dirname(path.resolve(context.source));
			rebaseConfig.toBase = context.options.rebaseTo;
		  }
		  */

		  tokens = tokenize(styles, context);
		  /*BT-
		  tokens = rebase(tokens, context.options.rebase, context.validator, rebaseConfig);

		  return allowsAnyImports(parentInlinerContext.inline) ?
			inline(tokens, context, parentInlinerContext, callback) :
			callback(tokens);*/
		  return callback(tokens); //BT+
		}

		/*BT-
		function allowsAnyImports(inline) {
		  return !(inline.length == 1 && inline[0] == 'none');
		}

		function inline(tokens, externalContext, parentInlinerContext, callback) {
		  var inlinerContext = {
			afterContent: false,
			callback: callback,
			errors: externalContext.errors,
			externalContext: externalContext,
			fetch: externalContext.options.fetch,
			inlinedStylesheets: parentInlinerContext.inlinedStylesheets || externalContext.inlinedStylesheets,
			inline: parentInlinerContext.inline,
			inlineRequest: externalContext.options.inlineRequest,
			inlineTimeout: externalContext.options.inlineTimeout,
			isRemote: parentInlinerContext.isRemote || false,
			localOnly: externalContext.localOnly,
			outputTokens: [],
			rebaseTo: externalContext.options.rebaseTo,
			sourceTokens: tokens,
			warnings: externalContext.warnings
		  };

		  return doInlineImports(inlinerContext);
		}

		function doInlineImports(inlinerContext) {
		  var token;
		  var i, l;

		  for (i = 0, l = inlinerContext.sourceTokens.length; i < l; i++) {
			token = inlinerContext.sourceTokens[i];

			if (token[0] == Token.AT_RULE && isImport(token[1])) {
			  inlinerContext.sourceTokens.splice(0, i);
			  return inlineStylesheet(token, inlinerContext);
			} else if (token[0] == Token.AT_RULE || token[0] == Token.COMMENT) {
			  inlinerContext.outputTokens.push(token);
			} else {
			  inlinerContext.outputTokens.push(token);
			  inlinerContext.afterContent = true;
			}
		  }

		  inlinerContext.sourceTokens = [];
		  return inlinerContext.callback(inlinerContext.outputTokens);
		}

		function inlineStylesheet(token, inlinerContext) {
		  var uriAndMediaQuery = extractImportUrlAndMedia(token[1]);
		  var uri = uriAndMediaQuery[0];
		  var mediaQuery = uriAndMediaQuery[1];
		  var metadata = token[2];

		  return isRemoteResource(uri) ?
			inlineRemoteStylesheet(uri, mediaQuery, metadata, inlinerContext) :
			inlineLocalStylesheet(uri, mediaQuery, metadata, inlinerContext);
		}

		function inlineRemoteStylesheet(uri, mediaQuery, metadata, inlinerContext) {
		  var isAllowed = isAllowedResource(uri, true, inlinerContext.inline);
		  var originalUri = uri;
		  var isLoaded = uri in inlinerContext.externalContext.sourcesContent;
		  var isRuntimeResource = !hasProtocol(uri);

		  if (inlinerContext.inlinedStylesheets.indexOf(uri) > -1) {
			inlinerContext.warnings.push('Ignoring remote @import of "' + uri + '" as it has already been imported.');
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  } else if (inlinerContext.localOnly && inlinerContext.afterContent) {
			inlinerContext.warnings.push('Ignoring remote @import of "' + uri + '" as no callback given and after other content.');
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  } else if (isRuntimeResource) {
			inlinerContext.warnings.push('Skipping remote @import of "' + uri + '" as no protocol given.');
			inlinerContext.outputTokens = inlinerContext.outputTokens.concat(inlinerContext.sourceTokens.slice(0, 1));
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  } else if (inlinerContext.localOnly && !isLoaded) {
			inlinerContext.warnings.push('Skipping remote @import of "' + uri + '" as no callback given.');
			inlinerContext.outputTokens = inlinerContext.outputTokens.concat(inlinerContext.sourceTokens.slice(0, 1));
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  } else if (!isAllowed && inlinerContext.afterContent) {
			inlinerContext.warnings.push('Ignoring remote @import of "' + uri + '" as resource is not allowed and after other content.');
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  } else if (!isAllowed) {
			inlinerContext.warnings.push('Skipping remote @import of "' + uri + '" as resource is not allowed.');
			inlinerContext.outputTokens = inlinerContext.outputTokens.concat(inlinerContext.sourceTokens.slice(0, 1));
			inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
			return doInlineImports(inlinerContext);
		  }

		  inlinerContext.inlinedStylesheets.push(uri);

		  function whenLoaded(error, importedStyles) {
			if (error) {
			  inlinerContext.errors.push('Broken @import declaration of "' + uri + '" - ' + error);

			  return process.nextTick(function () {
				inlinerContext.outputTokens = inlinerContext.outputTokens.concat(inlinerContext.sourceTokens.slice(0, 1));
				inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);
				doInlineImports(inlinerContext);
			  });
			}

			inlinerContext.inline = inlinerContext.externalContext.options.inline;
			inlinerContext.isRemote = true;

			inlinerContext.externalContext.source = originalUri;
			inlinerContext.externalContext.sourcesContent[uri] = importedStyles;
			inlinerContext.externalContext.stats.originalSize += importedStyles.length;

			return fromStyles(importedStyles, inlinerContext.externalContext, inlinerContext, function (importedTokens) {
			  importedTokens = wrapInMedia(importedTokens, mediaQuery, metadata);

			  inlinerContext.outputTokens = inlinerContext.outputTokens.concat(importedTokens);
			  inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);

			  return doInlineImports(inlinerContext);
			});
		  }

		  return isLoaded ?
			whenLoaded(null, inlinerContext.externalContext.sourcesContent[uri]) :
			inlinerContext.fetch(uri, inlinerContext.inlineRequest, inlinerContext.inlineTimeout, whenLoaded);
		}

		function inlineLocalStylesheet(uri, mediaQuery, metadata, inlinerContext) {
		  var currentPath = path.resolve('');
		  var absoluteUri = path.isAbsolute(uri) ?
			path.resolve(currentPath, uri[0] == '/' ? uri.substring(1) : uri) :
			path.resolve(inlinerContext.rebaseTo, uri);
		  var relativeToCurrentPath = path.relative(currentPath, absoluteUri);
		  var importedStyles;
		  var isAllowed = isAllowedResource(uri, false, inlinerContext.inline);
		  var normalizedPath = normalizePath(relativeToCurrentPath);
		  var isLoaded = normalizedPath in inlinerContext.externalContext.sourcesContent;

		  if (inlinerContext.inlinedStylesheets.indexOf(absoluteUri) > -1) {
			inlinerContext.warnings.push('Ignoring local @import of "' + uri + '" as it has already been imported.');
		  } else if (!isLoaded && (!fs.existsSync(absoluteUri) || !fs.statSync(absoluteUri).isFile())) {
			inlinerContext.errors.push('Ignoring local @import of "' + uri + '" as resource is missing.');
		  } else if (!isAllowed && inlinerContext.afterContent) {
			inlinerContext.warnings.push('Ignoring local @import of "' + uri + '" as resource is not allowed and after other content.');
		  } else if (inlinerContext.afterContent) {
			inlinerContext.warnings.push('Ignoring local @import of "' + uri + '" as after other content.');
		  } else if (!isAllowed) {
			inlinerContext.warnings.push('Skipping local @import of "' + uri + '" as resource is not allowed.');
			inlinerContext.outputTokens = inlinerContext.outputTokens.concat(inlinerContext.sourceTokens.slice(0, 1));
		  } else {
			importedStyles = isLoaded ?
			  inlinerContext.externalContext.sourcesContent[normalizedPath] :
			  fs.readFileSync(absoluteUri, 'utf-8');

			inlinerContext.inlinedStylesheets.push(absoluteUri);
			inlinerContext.inline = inlinerContext.externalContext.options.inline;

			inlinerContext.externalContext.source = normalizedPath;
			inlinerContext.externalContext.sourcesContent[normalizedPath] = importedStyles;
			inlinerContext.externalContext.stats.originalSize += importedStyles.length;

			return fromStyles(importedStyles, inlinerContext.externalContext, inlinerContext, function (importedTokens) {
			  importedTokens = wrapInMedia(importedTokens, mediaQuery, metadata);

			  inlinerContext.outputTokens = inlinerContext.outputTokens.concat(importedTokens);
			  inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);

			  return doInlineImports(inlinerContext);
			});
		  }

		  inlinerContext.sourceTokens = inlinerContext.sourceTokens.slice(1);

		  return doInlineImports(inlinerContext);
		}

		function wrapInMedia(tokens, mediaQuery, metadata) {
		  if (mediaQuery) {
			return [[Token.NESTED_BLOCK, [[Token.NESTED_BLOCK_SCOPE, '@media ' + mediaQuery, metadata]], tokens]];
		  } else {
			return tokens;
		  }
		}
		*/

		return readSources;
	};
	//#endregion

	//#region URL: /tokenizer/marker
	modules['/tokenizer/marker'] = function () {
		var Marker = {
		  ASTERISK: '*',
		  AT: '@',
		  BACK_SLASH: '\\',
		  CLOSE_CURLY_BRACKET: '}',
		  CLOSE_ROUND_BRACKET: ')',
		  CLOSE_SQUARE_BRACKET: ']',
		  COLON: ':',
		  COMMA: ',',
		  DOUBLE_QUOTE: '"',
		  EXCLAMATION: '!',
		  FORWARD_SLASH: '/',
		  INTERNAL: '-clean-css-',
		  NEW_LINE_NIX: '\n',
		  NEW_LINE_WIN: '\r',
		  OPEN_CURLY_BRACKET: '{',
		  OPEN_ROUND_BRACKET: '(',
		  OPEN_SQUARE_BRACKET: '[',
		  SEMICOLON: ';',
		  SINGLE_QUOTE: '\'',
		  SPACE: ' ',
		  TAB: '\t',
		  UNDERSCORE: '_'
		};

		return Marker;
	};
	//#endregion

	//#region URL: /tokenizer/token
	modules['/tokenizer/token'] = function () {
		var Token = {
		  AT_RULE: 'at-rule', // e.g. `@import`, `@charset`
		  AT_RULE_BLOCK: 'at-rule-block', // e.g. `@font-face{...}`
		  AT_RULE_BLOCK_SCOPE: 'at-rule-block-scope', // e.g. `@font-face`
		  COMMENT: 'comment', // e.g. `/* comment */`
		  NESTED_BLOCK: 'nested-block', // e.g. `@media screen{...}`, `@keyframes animation {...}`
		  NESTED_BLOCK_SCOPE: 'nested-block-scope', // e.g. `@media`, `@keyframes`
		  PROPERTY: 'property', // e.g. `color:red`
		  PROPERTY_BLOCK: 'property-block', // e.g. `--var:{color:red}`
		  PROPERTY_NAME: 'property-name', // e.g. `color`
		  PROPERTY_VALUE: 'property-value', // e.g. `red`
		  RULE: 'rule', // e.g `div > a{...}`
		  RULE_SCOPE: 'rule-scope' // e.g `div > a`
		};

		return Token;
	};
	//#endregion

	//#region URL: /tokenizer/tokenize
	modules['/tokenizer/tokenize'] = function () {
		var Marker = require('/tokenizer/marker');
		var Token = require('/tokenizer/token');

		var formatPosition = require('/utils/format-position');

		var Level = {
		  BLOCK: 'block',
		  COMMENT: 'comment',
		  DOUBLE_QUOTE: 'double-quote',
		  RULE: 'rule',
		  SINGLE_QUOTE: 'single-quote'
		};

		var AT_RULES = [
		  '@charset',
		  '@import'
		];

		var BLOCK_RULES = [
		  '@-moz-document',
		  '@document',
		  '@-moz-keyframes',
		  '@-ms-keyframes',
		  '@-o-keyframes',
		  '@-webkit-keyframes',
		  '@keyframes',
		  '@media',
		  '@supports'
		];

		var PAGE_MARGIN_BOXES = [
		  '@bottom-center',
		  '@bottom-left',
		  '@bottom-left-corner',
		  '@bottom-right',
		  '@bottom-right-corner',
		  '@left-bottom',
		  '@left-middle',
		  '@left-top',
		  '@right-bottom',
		  '@right-middle',
		  '@right-top',
		  '@top-center',
		  '@top-left',
		  '@top-left-corner',
		  '@top-right',
		  '@top-right-corner'
		];

		var EXTRA_PAGE_BOXES = [
		  '@footnote',
		  '@footnotes',
		  '@left',
		  '@page-float-bottom',
		  '@page-float-top',
		  '@right'
		];

		var REPEAT_PATTERN = /^\[\s{0,31}\d+\s{0,31}\]$/;
		var RULE_WORD_SEPARATOR_PATTERN = /[\s\(]/;
		var TAIL_BROKEN_VALUE_PATTERN = /[\s|\}]*$/;

		function tokenize(source, externalContext) {
		  var internalContext = {
			level: Level.BLOCK,
			position: {
			  source: externalContext.source || undefined,
			  line: 1,
			  column: 0,
			  index: 0
			}
		  };

		  return intoTokens(source, externalContext, internalContext, false);
		}

		function intoTokens(source, externalContext, internalContext, isNested) {
		  var allTokens = [];
		  var newTokens = allTokens;
		  var lastToken;
		  var ruleToken;
		  var ruleTokens = [];
		  var propertyToken;
		  var metadata;
		  var metadatas = [];
		  var level = internalContext.level;
		  var levels = [];
		  var buffer = [];
		  var buffers = [];
		  var serializedBuffer;
		  var roundBracketLevel = 0;
		  var isQuoted;
		  var isSpace;
		  var isNewLineNix;
		  var isNewLineWin;
		  var isCommentStart;
		  var wasCommentStart = false;
		  var isCommentEnd;
		  var wasCommentEnd = false;
		  var isCommentEndMarker;
		  var isEscaped;
		  var wasEscaped = false;
		  var seekingValue = false;
		  var seekingPropertyBlockClosing = false;
		  var position = internalContext.position;

		  for (; position.index < source.length; position.index++) {
			var character = source[position.index];

			isQuoted = level == Level.SINGLE_QUOTE || level == Level.DOUBLE_QUOTE;
			isSpace = character == Marker.SPACE || character == Marker.TAB;
			isNewLineNix = character == Marker.NEW_LINE_NIX;
			isNewLineWin = character == Marker.NEW_LINE_NIX && source[position.index - 1] == Marker.NEW_LINE_WIN;
			isCommentStart = !wasCommentEnd && level != Level.COMMENT && !isQuoted && character == Marker.ASTERISK && source[position.index - 1] == Marker.FORWARD_SLASH;
			isCommentEndMarker = !wasCommentStart && !isQuoted && character == Marker.FORWARD_SLASH && source[position.index - 1] == Marker.ASTERISK;
			isCommentEnd = level == Level.COMMENT && isCommentEndMarker;
			roundBracketLevel = Math.max(roundBracketLevel, 0);

			metadata = buffer.length === 0 ?
			  [position.line, position.column, position.source] :
			  metadata;

			if (isEscaped) {
			  // previous character was a backslash
			  buffer.push(character);
			} else if (!isCommentEnd && level == Level.COMMENT) {
			  buffer.push(character);
			} else if (isCommentStart && (level == Level.BLOCK || level == Level.RULE) && buffer.length > 1) {
			  // comment start within block preceded by some content, e.g. div/*<--
			  metadatas.push(metadata);
			  buffer.push(character);
			  buffers.push(buffer.slice(0, buffer.length - 2));

			  buffer = buffer.slice(buffer.length - 2);
			  metadata = [position.line, position.column - 1, position.source];

			  levels.push(level);
			  level = Level.COMMENT;
			} else if (isCommentStart) {
			  // comment start, e.g. /*<--
			  levels.push(level);
			  level = Level.COMMENT;
			  buffer.push(character);
			} else if (isCommentEnd) {
			  // comment end, e.g. /* comment */<--
			  serializedBuffer = buffer.join('').trim() + character;
			  lastToken = [Token.COMMENT, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]];
			  newTokens.push(lastToken);

			  level = levels.pop();
			  metadata = metadatas.pop() || null;
			  buffer = buffers.pop() || [];
			} else if (isCommentEndMarker && source[position.index + 1] != Marker.ASTERISK) {
			  externalContext.warnings.push('Unexpected \'*/\' at ' + formatPosition([position.line, position.column, position.source]) + '.');
			  buffer = [];
			} else if (character == Marker.SINGLE_QUOTE && !isQuoted) {
			  // single quotation start, e.g. a[href^='https<--
			  levels.push(level);
			  level = Level.SINGLE_QUOTE;
			  buffer.push(character);
			} else if (character == Marker.SINGLE_QUOTE && level == Level.SINGLE_QUOTE) {
			  // single quotation end, e.g. a[href^='https'<--
			  level = levels.pop();
			  buffer.push(character);
			} else if (character == Marker.DOUBLE_QUOTE && !isQuoted) {
			  // double quotation start, e.g. a[href^="<--
			  levels.push(level);
			  level = Level.DOUBLE_QUOTE;
			  buffer.push(character);
			} else if (character == Marker.DOUBLE_QUOTE && level == Level.DOUBLE_QUOTE) {
			  // double quotation end, e.g. a[href^="https"<--
			  level = levels.pop();
			  buffer.push(character);
			} else if (!isCommentStart && !isCommentEnd && character != Marker.CLOSE_ROUND_BRACKET && character != Marker.OPEN_ROUND_BRACKET && level != Level.COMMENT && !isQuoted && roundBracketLevel > 0) {
			  // character inside any function, e.g. hsla(.<--
			  buffer.push(character);
			} else if (character == Marker.OPEN_ROUND_BRACKET && !isQuoted && level != Level.COMMENT && !seekingValue) {
			  // round open bracket, e.g. @import url(<--
			  buffer.push(character);

			  roundBracketLevel++;
			} else if (character == Marker.CLOSE_ROUND_BRACKET && !isQuoted && level != Level.COMMENT && !seekingValue) {
			  // round open bracket, e.g. @import url(test.css)<--
			  buffer.push(character);

			  roundBracketLevel--;
			} else if (character == Marker.SEMICOLON && level == Level.BLOCK && buffer[0] == Marker.AT) {
			  // semicolon ending rule at block level, e.g. @import '...';<--
			  serializedBuffer = buffer.join('').trim();
			  allTokens.push([Token.AT_RULE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  buffer = [];
			} else if (character == Marker.COMMA && level == Level.BLOCK && ruleToken) {
			  // comma separator at block level, e.g. a,div,<--
			  serializedBuffer = buffer.join('').trim();
			  ruleToken[1].push([tokenScopeFrom(ruleToken[0]), serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext, ruleToken[1].length)]]);

			  buffer = [];
			} else if (character == Marker.COMMA && level == Level.BLOCK && tokenTypeFrom(buffer) == Token.AT_RULE) {
			  // comma separator at block level, e.g. @import url(...) screen,<--
			  // keep iterating as end semicolon will create the token
			  buffer.push(character);
			} else if (character == Marker.COMMA && level == Level.BLOCK) {
			  // comma separator at block level, e.g. a,<--
			  ruleToken = [tokenTypeFrom(buffer), [], []];
			  serializedBuffer = buffer.join('').trim();
			  ruleToken[1].push([tokenScopeFrom(ruleToken[0]), serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext, 0)]]);

			  buffer = [];
			} else if (character == Marker.OPEN_CURLY_BRACKET && level == Level.BLOCK && ruleToken && ruleToken[0] == Token.NESTED_BLOCK) {
			  // open brace opening at-rule at block level, e.g. @media{<--
			  serializedBuffer = buffer.join('').trim();
			  ruleToken[1].push([Token.NESTED_BLOCK_SCOPE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  allTokens.push(ruleToken);

			  levels.push(level);
			  position.column++;
			  position.index++;
			  buffer = [];

			  ruleToken[2] = intoTokens(source, externalContext, internalContext, true);
			  ruleToken = null;
			} else if (character == Marker.OPEN_CURLY_BRACKET && level == Level.BLOCK && tokenTypeFrom(buffer) == Token.NESTED_BLOCK) {
			  // open brace opening at-rule at block level, e.g. @media{<--
			  serializedBuffer = buffer.join('').trim();
			  ruleToken = ruleToken || [Token.NESTED_BLOCK, [], []];
			  ruleToken[1].push([Token.NESTED_BLOCK_SCOPE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  allTokens.push(ruleToken);

			  levels.push(level);
			  position.column++;
			  position.index++;
			  buffer = [];

			  ruleToken[2] = intoTokens(source, externalContext, internalContext, true);
			  ruleToken = null;
			} else if (character == Marker.OPEN_CURLY_BRACKET && level == Level.BLOCK) {
			  // open brace opening rule at block level, e.g. div{<--
			  serializedBuffer = buffer.join('').trim();
			  ruleToken = ruleToken || [tokenTypeFrom(buffer), [], []];
			  ruleToken[1].push([tokenScopeFrom(ruleToken[0]), serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext, ruleToken[1].length)]]);
			  newTokens = ruleToken[2];
			  allTokens.push(ruleToken);

			  levels.push(level);
			  level = Level.RULE;
			  buffer = [];
			} else if (character == Marker.OPEN_CURLY_BRACKET && level == Level.RULE && seekingValue) {
			  // open brace opening rule at rule level, e.g. div{--variable:{<--
			  ruleTokens.push(ruleToken);
			  ruleToken = [Token.PROPERTY_BLOCK, []];
			  propertyToken.push(ruleToken);
			  newTokens = ruleToken[1];

			  levels.push(level);
			  level = Level.RULE;
			  seekingValue = false;
			} else if (character == Marker.OPEN_CURLY_BRACKET && level == Level.RULE && isPageMarginBox(buffer)) {
			  // open brace opening page-margin box at rule level, e.g. @page{@top-center{<--
			  serializedBuffer = buffer.join('').trim();
			  ruleTokens.push(ruleToken);
			  ruleToken = [Token.AT_RULE_BLOCK, [], []];
			  ruleToken[1].push([Token.AT_RULE_BLOCK_SCOPE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  newTokens.push(ruleToken);
			  newTokens = ruleToken[2];

			  levels.push(level);
			  level = Level.RULE;
			  buffer = [];
			} else if (character == Marker.COLON && level == Level.RULE && !seekingValue) {
			  // colon at rule level, e.g. a{color:<--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken = [Token.PROPERTY, [Token.PROPERTY_NAME, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]];
			  newTokens.push(propertyToken);

			  seekingValue = true;
			  buffer = [];
			} else if (character == Marker.SEMICOLON && level == Level.RULE && propertyToken && ruleTokens.length > 0 && buffer.length > 0 && buffer[0] == Marker.AT) {
			  // semicolon at rule level for at-rule, e.g. a{--color:{@apply(--other-color);<--
			  serializedBuffer = buffer.join('').trim();
			  ruleToken[1].push([Token.AT_RULE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  buffer = [];
			} else if (character == Marker.SEMICOLON && level == Level.RULE && propertyToken && buffer.length > 0) {
			  // semicolon at rule level, e.g. a{color:red;<--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  propertyToken = null;
			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.SEMICOLON && level == Level.RULE && propertyToken && buffer.length === 0) {
			  // semicolon after bracketed value at rule level, e.g. a{color:rgb(...);<--
			  propertyToken = null;
			  seekingValue = false;
			} else if (character == Marker.SEMICOLON && level == Level.RULE && buffer.length > 0 && buffer[0] == Marker.AT) {
			  // semicolon for at-rule at rule level, e.g. a{@apply(--variable);<--
			  serializedBuffer = buffer.join('');
			  newTokens.push([Token.AT_RULE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.SEMICOLON && level == Level.RULE && seekingPropertyBlockClosing) {
			  // close brace after a property block at rule level, e.g. a{--custom:{color:red;};<--
			  seekingPropertyBlockClosing = false;
			  buffer = [];
			} else if (character == Marker.SEMICOLON && level == Level.RULE && buffer.length === 0) {
			  // stray semicolon at rule level, e.g. a{;<--
			  // noop
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && propertyToken && seekingValue && buffer.length > 0 && ruleTokens.length > 0) {
			  // close brace at rule level, e.g. a{--color:{color:red}<--
			  serializedBuffer = buffer.join('');
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  propertyToken = null;
			  ruleToken = ruleTokens.pop();
			  newTokens = ruleToken[2];

			  level = levels.pop();
			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && propertyToken && buffer.length > 0 && buffer[0] == Marker.AT && ruleTokens.length > 0) {
			  // close brace at rule level for at-rule, e.g. a{--color:{@apply(--other-color)}<--
			  serializedBuffer = buffer.join('');
			  ruleToken[1].push([Token.AT_RULE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  propertyToken = null;
			  ruleToken = ruleTokens.pop();
			  newTokens = ruleToken[2];

			  level = levels.pop();
			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && propertyToken && ruleTokens.length > 0) {
			  // close brace at rule level after space, e.g. a{--color:{color:red }<--
			  propertyToken = null;
			  ruleToken = ruleTokens.pop();
			  newTokens = ruleToken[2];

			  level = levels.pop();
			  seekingValue = false;
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && propertyToken && buffer.length > 0) {
			  // close brace at rule level, e.g. a{color:red}<--
			  serializedBuffer = buffer.join('');
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  propertyToken = null;
			  ruleToken = ruleTokens.pop();
			  newTokens = allTokens;

			  level = levels.pop();
			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && buffer.length > 0 && buffer[0] == Marker.AT) {
			  // close brace after at-rule at rule level, e.g. a{@apply(--variable)}<--
			  propertyToken = null;
			  ruleToken = null;
			  serializedBuffer = buffer.join('').trim();
			  newTokens.push([Token.AT_RULE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  newTokens = allTokens;

			  level = levels.pop();
			  seekingValue = false;
			  buffer = [];
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE && levels[levels.length - 1] == Level.RULE) {
			  // close brace after a property block at rule level, e.g. a{--custom:{color:red;}<--
			  propertyToken = null;
			  ruleToken = ruleTokens.pop();
			  newTokens = ruleToken[2];

			  level = levels.pop();
			  seekingValue = false;
			  seekingPropertyBlockClosing = true;
			  buffer = [];
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.RULE) {
			  // close brace after a rule, e.g. a{color:red;}<--
			  propertyToken = null;
			  ruleToken = null;
			  newTokens = allTokens;

			  level = levels.pop();
			  seekingValue = false;
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.BLOCK && !isNested && position.index <= source.length - 1) {
			  // stray close brace at block level, e.g. a{color:red}color:blue}<--
			  externalContext.warnings.push('Unexpected \'}\' at ' + formatPosition([position.line, position.column, position.source]) + '.');
			  buffer.push(character);
			} else if (character == Marker.CLOSE_CURLY_BRACKET && level == Level.BLOCK) {
			  // close brace at block level, e.g. @media screen {...}<--
			  break;
			} else if (character == Marker.OPEN_ROUND_BRACKET && level == Level.RULE && seekingValue) {
			  // round open bracket, e.g. a{color:hsla(<--
			  buffer.push(character);
			  roundBracketLevel++;
			} else if (character == Marker.CLOSE_ROUND_BRACKET && level == Level.RULE && seekingValue && roundBracketLevel == 1) {
			  // round close bracket, e.g. a{color:hsla(0,0%,0%)<--
			  buffer.push(character);
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  roundBracketLevel--;
			  buffer = [];
			} else if (character == Marker.CLOSE_ROUND_BRACKET && level == Level.RULE && seekingValue) {
			  // round close bracket within other brackets, e.g. a{width:calc((10rem / 2)<--
			  buffer.push(character);
			  roundBracketLevel--;
			} else if (character == Marker.FORWARD_SLASH && source[position.index + 1] != Marker.ASTERISK && level == Level.RULE && seekingValue && buffer.length > 0) {
			  // forward slash within a property, e.g. a{background:url(image.png) 0 0/<--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  propertyToken.push([Token.PROPERTY_VALUE, character, [[position.line, position.column, position.source]]]);

			  buffer = [];
			} else if (character == Marker.FORWARD_SLASH && source[position.index + 1] != Marker.ASTERISK && level == Level.RULE && seekingValue) {
			  // forward slash within a property after space, e.g. a{background:url(image.png) 0 0 /<--
			  propertyToken.push([Token.PROPERTY_VALUE, character, [[position.line, position.column, position.source]]]);

			  buffer = [];
			} else if (character == Marker.COMMA && level == Level.RULE && seekingValue && buffer.length > 0) {
			  // comma within a property, e.g. a{background:url(image.png),<--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);
			  propertyToken.push([Token.PROPERTY_VALUE, character, [[position.line, position.column, position.source]]]);

			  buffer = [];
			} else if (character == Marker.COMMA && level == Level.RULE && seekingValue) {
			  // comma within a property after space, e.g. a{background:url(image.png) ,<--
			  propertyToken.push([Token.PROPERTY_VALUE, character, [[position.line, position.column, position.source]]]);

			  buffer = [];
			} else if (character == Marker.CLOSE_SQUARE_BRACKET && propertyToken && propertyToken.length > 1 && buffer.length > 0 && isRepeatToken(buffer)) {
			  buffer.push(character);
			  serializedBuffer = buffer.join('').trim();
			  propertyToken[propertyToken.length - 1][1] += serializedBuffer;

			  buffer = [];
			} else if ((isSpace || (isNewLineNix && !isNewLineWin)) && level == Level.RULE && seekingValue && propertyToken && buffer.length > 0) {
			  // space or *nix newline within property, e.g. a{margin:0 <--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  buffer = [];
			} else if (isNewLineWin && level == Level.RULE && seekingValue && propertyToken && buffer.length > 1) {
			  // win newline within property, e.g. a{margin:0\r\n<--
			  serializedBuffer = buffer.join('').trim();
			  propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			  buffer = [];
			} else if (isNewLineWin && level == Level.RULE && seekingValue) {
			  // win newline
			  buffer = [];
			} else if (buffer.length == 1 && isNewLineWin) {
			  // ignore windows newline which is composed of two characters
			  buffer.pop();
			} else if (buffer.length > 0 || !isSpace && !isNewLineNix && !isNewLineWin) {
			  // any character
			  buffer.push(character);
			}

			wasEscaped = isEscaped;
			isEscaped = !wasEscaped && character == Marker.BACK_SLASH;
			wasCommentStart = isCommentStart;
			wasCommentEnd = isCommentEnd;

			position.line = (isNewLineWin || isNewLineNix) ? position.line + 1 : position.line;
			position.column = (isNewLineWin || isNewLineNix) ? 0 : position.column + 1;
		  }

		  if (seekingValue) {
			externalContext.warnings.push('Missing \'}\' at ' + formatPosition([position.line, position.column, position.source]) + '.');
		  }

		  if (seekingValue && buffer.length > 0) {
			serializedBuffer = buffer.join('').replace(TAIL_BROKEN_VALUE_PATTERN, '');
			propertyToken.push([Token.PROPERTY_VALUE, serializedBuffer, [originalMetadata(metadata, serializedBuffer, externalContext)]]);

			buffer = [];
		  }

		  if (buffer.length > 0) {
			externalContext.warnings.push('Invalid character(s) \'' + buffer.join('') + '\' at ' + formatPosition(metadata) + '. Ignoring.');
		  }

		  return allTokens;
		}

		function originalMetadata(metadata, value, externalContext, selectorFallbacks) {
		  var source = metadata[2];

		  return /*BT- externalContext.inputSourceMapTracker.isTracking(source) ?
			externalContext.inputSourceMapTracker.originalPositionFor(metadata, value.length, selectorFallbacks) :
			*/metadata;
		}

		function tokenTypeFrom(buffer) {
		  var isAtRule = buffer[0] == Marker.AT || buffer[0] == Marker.UNDERSCORE;
		  var ruleWord = buffer.join('').split(RULE_WORD_SEPARATOR_PATTERN)[0];

		  if (isAtRule && BLOCK_RULES.indexOf(ruleWord) > -1) {
			return Token.NESTED_BLOCK;
		  } else if (isAtRule && AT_RULES.indexOf(ruleWord) > -1) {
			return Token.AT_RULE;
		  } else if (isAtRule) {
			return Token.AT_RULE_BLOCK;
		  } else {
			return Token.RULE;
		  }
		}

		function tokenScopeFrom(tokenType) {
		  if (tokenType == Token.RULE) {
			return Token.RULE_SCOPE;
		  } else if (tokenType == Token.NESTED_BLOCK) {
			return Token.NESTED_BLOCK_SCOPE;
		  } else if (tokenType == Token.AT_RULE_BLOCK) {
			return Token.AT_RULE_BLOCK_SCOPE;
		  }
		}

		function isPageMarginBox(buffer) {
		  var serializedBuffer = buffer.join('').trim();

		  return PAGE_MARGIN_BOXES.indexOf(serializedBuffer) > -1 || EXTRA_PAGE_BOXES.indexOf(serializedBuffer) > -1;
		}

		function isRepeatToken(buffer) {
		  return REPEAT_PATTERN.test(buffer.join('') + Marker.CLOSE_SQUARE_BRACKET);
		}

		return tokenize;
	};
	//#endregion

	//#region URL: /utils/clone-array
	modules['/utils/clone-array'] = function () {
		function cloneArray(array) {
		  var cloned = array.slice(0);

		  for (var i = 0, l = cloned.length; i < l; i++) {
			if (Array.isArray(cloned[i]))
			  cloned[i] = cloneArray(cloned[i]);
		  }

		  return cloned;
		}

		return cloneArray;
	};
	//#endregion

	//#region URL: /utils/format-position
	modules['/utils/format-position'] = function () {
		function formatPosition(metadata) {
		  var line = metadata[0];
		  var column = metadata[1];
		  var source = metadata[2];

		  return source ?
			source + ':' + line + ':' + column :
			line + ':' + column;
		}

		return formatPosition;
	};
	//#endregion

	//#region URL: /utils/natural-compare
	modules['/utils/natural-compare'] = function () {
		// adapted from http://nedbatchelder.com/blog/200712.html#e20071211T054956

		var NUMBER_PATTERN = /([0-9]+)/;

		function naturalCompare(value1, value2) {
		  var keys1 = ('' + value1).split(NUMBER_PATTERN).map(tryParseInt);
		  var keys2 = ('' + value2).split(NUMBER_PATTERN).map(tryParseInt);
		  var key1;
		  var key2;
		  var compareFirst = Math.min(keys1.length, keys2.length);
		  var i, l;

		  for (i = 0, l = compareFirst; i < l; i++) {
			key1 = keys1[i];
			key2 = keys2[i];

			if (key1 != key2) {
			  return key1 > key2 ? 1 : -1;
			}
		  }

		  return keys1.length > keys2.length ? 1 : (keys1.length == keys2.length ? 0 : -1);
		}

		function tryParseInt(value) {
		  return ('' + parseInt(value)) == value ?
			parseInt(value) :
			value;
		}

		return naturalCompare;
	};
	//#endregion

	//#region URL: /utils/override
	modules['/utils/override'] = function () {
		function override(source1, source2) {
		  var target = {};
		  var key1;
		  var key2;
		  var item;

		  for (key1 in source1) {
			item = source1[key1];

			if (Array.isArray(item)) {
			  target[key1] = item.slice(0);
			} else if (typeof item == 'object' && item !== null) {
			  target[key1] = override(item, {});
			} else {
			  target[key1] = item;
			}
		  }

		  for (key2 in source2) {
			item = source2[key2];

			if (key2 in target && Array.isArray(item)) {
			  target[key2] = item.slice(0);
			} else if (key2 in target && typeof item == 'object' && item !== null) {
			  target[key2] = override(target[key2], item);
			} else {
			  target[key2] = item;
			}
		  }

		  return target;
		}

		return override;
	};
	//#endregion

	//#region URL: /utils/split
	modules['/utils/split'] = function () {
		var Marker = require('/tokenizer/marker');

		function split(value, separator) {
		  var openLevel = Marker.OPEN_ROUND_BRACKET;
		  var closeLevel = Marker.CLOSE_ROUND_BRACKET;
		  var level = 0;
		  var cursor = 0;
		  var lastStart = 0;
		  var lastValue;
		  var lastCharacter;
		  var len = value.length;
		  var parts = [];

		  if (value.indexOf(separator) == -1) {
			return [value];
		  }

		  if (value.indexOf(openLevel) == -1) {
			return value.split(separator);
		  }

		  while (cursor < len) {
			if (value[cursor] == openLevel) {
			  level++;
			} else if (value[cursor] == closeLevel) {
			  level--;
			}

			if (level === 0 && cursor > 0 && cursor + 1 < len && value[cursor] == separator) {
			  parts.push(value.substring(lastStart, cursor));
			  lastStart = cursor + 1;
			}

			cursor++;
		  }

		  if (lastStart < cursor + 1) {
			lastValue = value.substring(lastStart);
			lastCharacter = lastValue[lastValue.length - 1];
			if (lastCharacter == separator) {
			  lastValue = lastValue.substring(0, lastValue.length - 1);
			}

			parts.push(lastValue);
		  }

		  return parts;
		}

		return split;
	};
	//#endregion

	//#region URL: /writer/helpers
	modules['/writer/helpers'] = function () {
		var lineBreak = require('os').EOL;
		var emptyCharacter = '';

		var Breaks = require('/options/format').Breaks;
		var Spaces = require('/options/format').Spaces;

		var Marker = require('/tokenizer/marker');
		var Token = require('/tokenizer/token');

		function supportsAfterClosingBrace(token) {
		  return token[1][1] == 'background' || token[1][1] == 'transform' || token[1][1] == 'src';
		}

		function afterClosingBrace(token, valueIndex) {
		  return token[valueIndex][1][token[valueIndex][1].length - 1] == Marker.CLOSE_ROUND_BRACKET;
		}

		function afterComma(token, valueIndex) {
		  return token[valueIndex][1] == Marker.COMMA;
		}

		function afterSlash(token, valueIndex) {
		  return token[valueIndex][1] == Marker.FORWARD_SLASH;
		}

		function beforeComma(token, valueIndex) {
		  return token[valueIndex + 1] && token[valueIndex + 1][1] == Marker.COMMA;
		}

		function beforeSlash(token, valueIndex) {
		  return token[valueIndex + 1] && token[valueIndex + 1][1] == Marker.FORWARD_SLASH;
		}

		function inFilter(token) {
		  return token[1][1] == 'filter' || token[1][1] == '-ms-filter';
		}

		function disallowsSpace(context, token, valueIndex) {
		  return !context.spaceAfterClosingBrace && supportsAfterClosingBrace(token) && afterClosingBrace(token, valueIndex) ||
			beforeSlash(token, valueIndex) ||
			afterSlash(token, valueIndex) ||
			beforeComma(token, valueIndex) ||
			afterComma(token, valueIndex);
		}

		function rules(context, tokens) {
		  var store = context.store;

		  for (var i = 0, l = tokens.length; i < l; i++) {
			store(context, tokens[i]);

			if (i < l - 1) {
			  store(context, comma(context));
			}
		  }
		}

		function body(context, tokens) {
		  var lastPropertyAt = lastPropertyIndex(tokens);

		  for (var i = 0, l = tokens.length; i < l; i++) {
			property(context, tokens, i, lastPropertyAt);
		  }
		}

		function lastPropertyIndex(tokens) {
		  var index = tokens.length - 1;

		  for (; index >= 0; index--) {
			if (tokens[index][0] != Token.COMMENT) {
			  break;
			}
		  }

		  return index;
		}

		function property(context, tokens, position, lastPropertyAt) {
		  var store = context.store;
		  var token = tokens[position];
		  var isPropertyBlock = token[2][0] == Token.PROPERTY_BLOCK;
		  var needsSemicolon = position < lastPropertyAt || isPropertyBlock;
		  var isLast = position === lastPropertyAt;

		  switch (token[0]) {
			case Token.AT_RULE:
			  store(context, token);
			  store(context, semicolon(context, Breaks.AfterProperty, false));
			  break;
			case Token.AT_RULE_BLOCK:
			  rules(context, token[1]);
			  store(context, openBrace(context, Breaks.AfterRuleBegins, true));
			  body(context, token[2]);
			  store(context, closeBrace(context, Breaks.AfterRuleEnds, false, isLast));
			  break;
			case Token.COMMENT:
			  store(context, token);
			  break;
			case Token.PROPERTY:
			  store(context, token[1]);
			  store(context, colon(context));
			  value(context, token);
			  store(context, needsSemicolon ? semicolon(context, Breaks.AfterProperty, isLast) : emptyCharacter);
		  }
		}

		function value(context, token) {
		  var store = context.store;
		  var j, m;

		  if (token[2][0] == Token.PROPERTY_BLOCK) {
			store(context, openBrace(context, Breaks.AfterBlockBegins, false));
			body(context, token[2][1]);
			store(context, closeBrace(context, Breaks.AfterBlockEnds, false, true));
		  } else {
			for (j = 2, m = token.length; j < m; j++) {
			  store(context, token[j]);

			  if (j < m - 1 && (inFilter(token) || !disallowsSpace(context, token, j))) {
				store(context, Marker.SPACE);
			  }
			}
		  }
		}

		function allowsBreak(context, where) {
		  return context.format && context.format.breaks[where];
		}

		function allowsSpace(context, where) {
		  return context.format && context.format.spaces[where];
		}

		function openBrace(context, where, needsPrefixSpace) {
		  if (context.format) {
			context.indentBy += context.format.indentBy;
			context.indentWith = context.format.indentWith.repeat(context.indentBy);
			return (needsPrefixSpace && allowsSpace(context, Spaces.BeforeBlockBegins) ? Marker.SPACE : emptyCharacter) +
			  Marker.OPEN_CURLY_BRACKET +
			  (allowsBreak(context, where) ? lineBreak : emptyCharacter) +
			  context.indentWith;
		  } else {
			return Marker.OPEN_CURLY_BRACKET;
		  }
		}

		function closeBrace(context, where, beforeBlockEnd, isLast) {
		  if (context.format) {
			context.indentBy -= context.format.indentBy;
			context.indentWith = context.format.indentWith.repeat(context.indentBy);
			return (allowsBreak(context, Breaks.AfterProperty) || beforeBlockEnd && allowsBreak(context, Breaks.BeforeBlockEnds) ? lineBreak : emptyCharacter) +
			  context.indentWith +
			  Marker.CLOSE_CURLY_BRACKET +
			  (isLast ? emptyCharacter : (allowsBreak(context, where) ? lineBreak : emptyCharacter) + context.indentWith);
		  } else {
			return Marker.CLOSE_CURLY_BRACKET;
		  }
		}

		function colon(context) {
		  return context.format ?
			Marker.COLON + (allowsSpace(context, Spaces.BeforeValue) ? Marker.SPACE : emptyCharacter) :
			Marker.COLON;
		}

		function semicolon(context, where, isLast) {
		  return context.format ?
			Marker.SEMICOLON + (isLast || !allowsBreak(context, where) ? emptyCharacter : lineBreak + context.indentWith) :
			Marker.SEMICOLON;
		}

		function comma(context) {
		  return context.format ?
			Marker.COMMA + (allowsBreak(context, Breaks.BetweenSelectors) ? lineBreak : emptyCharacter) + context.indentWith :
			Marker.COMMA;
		}

		function all(context, tokens) {
		  var store = context.store;
		  var token;
		  var isLast;
		  var i, l;

		  for (i = 0, l = tokens.length; i < l; i++) {
			token = tokens[i];
			isLast = i == l - 1;

			switch (token[0]) {
			  case Token.AT_RULE:
				store(context, token);
				store(context, semicolon(context, Breaks.AfterAtRule, isLast));
				break;
			  case Token.AT_RULE_BLOCK:
				rules(context, token[1]);
				store(context, openBrace(context, Breaks.AfterRuleBegins, true));
				body(context, token[2]);
				store(context, closeBrace(context, Breaks.AfterRuleEnds, false, isLast));
				break;
			  case Token.NESTED_BLOCK:
				rules(context, token[1]);
				store(context, openBrace(context, Breaks.AfterBlockBegins, true));
				all(context, token[2]);
				store(context, closeBrace(context, Breaks.AfterBlockEnds, true, isLast));
				break;
			  case Token.COMMENT:
				store(context, token);
				store(context, allowsBreak(context, Breaks.AfterComment) ? lineBreak : emptyCharacter);
				break;
			  case Token.RULE:
				rules(context, token[1]);
				store(context, openBrace(context, Breaks.AfterRuleBegins, true));
				body(context, token[2]);
				store(context, closeBrace(context, Breaks.AfterRuleEnds, false, isLast));
				break;
			}
		  }
		}

		var exports = {
		  all: all,
		  body: body,
		  property: property,
		  rules: rules,
		  value: value
		};

		return exports;
	};
	//#endregion

	//#region URL: /writer/one-time
	modules['/writer/one-time'] = function () {
		var helpers = require('/writer/helpers');

		function store(serializeContext, token) {
		  serializeContext.output.push(typeof token == 'string' ? token : token[1]);
		}

		function context() {
		  var newContext = {
			output: [],
			store: store
		  };

		  return newContext;
		}

		function all(tokens) {
		  var oneTimeContext = context();
		  helpers.all(oneTimeContext, tokens);
		  return oneTimeContext.output.join('');
		}

		function body(tokens) {
		  var oneTimeContext = context();
		  helpers.body(oneTimeContext, tokens);
		  return oneTimeContext.output.join('');
		}

		function property(tokens, position) {
		  var oneTimeContext = context();
		  helpers.property(oneTimeContext, tokens, position, true);
		  return oneTimeContext.output.join('');
		}

		function rules(tokens) {
		  var oneTimeContext = context();
		  helpers.rules(oneTimeContext, tokens);
		  return oneTimeContext.output.join('');
		}

		function value(tokens) {
		  var oneTimeContext = context();
		  helpers.value(oneTimeContext, tokens);
		  return oneTimeContext.output.join('');
		}

		var exports = {
		  all: all,
		  body: body,
		  property: property,
		  rules: rules,
		  value: value
		};

		return exports;
	};
	//#endregion

	//#region URL: /writer/simple
	modules['/writer/simple'] = function () {
		var all = require('/writer/helpers').all;

		var lineBreak = require('os').EOL;

		function store(serializeContext, token) {
		  var value = typeof token == 'string' ?
			token :
			token[1];
		  var wrap = serializeContext.wrap;

		  wrap(serializeContext, value);
		  track(serializeContext, value);
		  serializeContext.output.push(value);
		}

		function wrap(serializeContext, value) {
		  if (serializeContext.column + value.length > serializeContext.format.wrapAt) {
			track(serializeContext, lineBreak);
			serializeContext.output.push(lineBreak);
		  }
		}

		function track(serializeContext, value) {
		  var parts = value.split('\n');

		  serializeContext.line += parts.length - 1;
		  serializeContext.column = parts.length > 1 ? 0 : (serializeContext.column + parts.pop().length);
		}

		function serializeStyles(tokens, context) {
		  var serializeContext = {
			column: 0,
			format: context.options.format,
			indentBy: 0,
			indentWith: '',
			line: 1,
			output: [],
			spaceAfterClosingBrace: context.options.compatibility.properties.spaceAfterClosingBrace,
			store: store,
			wrap: context.options.format.wrapAt ?
			  wrap :
			  function () { /* noop */  }
		  };

		  all(serializeContext, tokens);

		  return {
			styles: serializeContext.output.join('')
		  };
		}

		return serializeStyles;
	};
	//#endregion

	//#region URL: /clean
	modules['/clean'] = function () {
		var level0Optimize = require('/optimizer/level-0/optimize');
		var level1Optimize = require('/optimizer/level-1/optimize');
		var level2Optimize = require('/optimizer/level-2/optimize');
		var validator = require('/optimizer/validator');

		var compatibilityFrom = require('/options/compatibility');
		/*BT-
		var fetchFrom = require('/options/fetch');
		*/
		var formatFrom = require('/options/format').formatFrom;
		/*BT-
		var inlineFrom = require('/options/inline');
		var inlineRequestFrom = require('/options/inline-request');
		var inlineTimeoutFrom = require('/options/inline-timeout');
		*/
		var OptimizationLevel = require('/options/optimization-level').OptimizationLevel;
		var optimizationLevelFrom = require('/options/optimization-level').optimizationLevelFrom;
		/*BT-
		var rebaseFrom = require('/options/rebase');
		var rebaseToFrom = require('/options/rebase-to');

		var inputSourceMapTracker = require('/reader/input-source-map-tracker');
		*/
		var readSources = require('/reader/read-sources');

		var serializeStyles = require('/writer/simple');
		/*BT-
		var serializeStylesAndSourceMap = require('/writer/source-maps');
		*/

		var CleanCSS = function CleanCSS(options) {
		  options = options || {};

		  this.options = {
			compatibility: compatibilityFrom(options.compatibility),
			/*BT-
			fetch: fetchFrom(options.fetch),
			*/
			format: formatFrom(options.format),
			/*BT-
			inline: inlineFrom(options.inline),
			inlineRequest: inlineRequestFrom(options.inlineRequest),
			inlineTimeout: inlineTimeoutFrom(options.inlineTimeout),
			*/
			level: optimizationLevelFrom(options.level)/*BT-,
			rebase: rebaseFrom(options.rebase),
			rebaseTo: rebaseToFrom(options.rebaseTo),
			returnPromise: !!options.returnPromise,
			sourceMap: !!options.sourceMap,
			sourceMapInlineSources: !!options.sourceMapInlineSources
			*/
		  };
		};

		CleanCSS.prototype.minify = function (input, maybeSourceMap, maybeCallback) {
		  var options = this.options;

		  /*BT-
		  if (options.returnPromise) {
			return new Promise(function (resolve, reject) {
			  minify(input, options, maybeSourceMap, function (errors, output) {
				return errors ?
				  reject(errors) :
				  resolve(output);
			  });
			});
		  } else {
		  */
			return minify(input, options, maybeSourceMap, maybeCallback);
		  /*BT-
		  }
		  */
		};

		function minify(input, options, maybeSourceMap, maybeCallback) {
		  /*BT-
		  var sourceMap = typeof maybeSourceMap != 'function' ?
			maybeSourceMap :
			null;
		  */
		  var callback = typeof maybeCallback == 'function' ?
			maybeCallback :
			/*BT- (typeof maybeSourceMap == 'function' ? maybeSourceMap : */null/*BT- )*/;
		  var context = {
			/*BT-
			stats: {
			  efficiency: 0,
			  minifiedSize: 0,
			  originalSize: 0,
			  startedAt: Date.now(),
			  timeSpent: 0
			},
			*/
			cache: {
			  specificity: {}
			},
			errors: [],
			/*BT-
			inlinedStylesheets: [],
			inputSourceMapTracker: inputSourceMapTracker(),
			*/
			localOnly: !callback,
			options: options,
			source: null,
			sourcesContent: {},
			validator: validator(options.compatibility),
			warnings: []
		  };

		  /*BT-
		  if (sourceMap) {
			context.inputSourceMapTracker.track(undefined, sourceMap);
		  }
		  */

		  return runner(context.localOnly)(function () {
			return readSources(input, context, function (tokens) {
			  var serialize = /*BT- context.options.sourceMap ?
				serializeStylesAndSourceMap :
				*/serializeStyles;

			  var optimizedTokens = optimize(tokens, context);
			  var optimizedStyles = serialize(optimizedTokens, context);
			  var output = withMetadata(optimizedStyles, context);

			  return callback ?
				callback(context.errors.length > 0 ? context.errors : null, output) :
				output;
			});
		  });
		}

		function runner(localOnly) {
		  // to always execute code asynchronously when a callback is given
		  // more at blog.izs.me/post/59142742143/designing-apis-for-asynchrony
		  return localOnly ?
			function (callback) { return callback(); } :
			process.nextTick;
		}

		function optimize(tokens, context) {
		  var optimized;

		  optimized = level0Optimize(tokens, context);
		  optimized = OptimizationLevel.One in context.options.level ?
			level1Optimize(tokens, context) :
			tokens;
		  optimized = OptimizationLevel.Two in context.options.level ?
			level2Optimize(tokens, context, true) :
			optimized;

		  return optimized;
		}

		function withMetadata(output, context) {
		  /*BT-
		  output.stats = calculateStatsFrom(output.styles, context);
		  */
		  output.errors = context.errors;
		  /*BT-
		  output.inlinedStylesheets = context.inlinedStylesheets;
		  */
		  output.warnings = context.warnings;

		  return output;
		}

		/*BT-
		function calculateStatsFrom(styles, context) {
		  var finishedAt = Date.now();
		  var timeSpent = finishedAt - context.stats.startedAt;

		  delete context.stats.startedAt;
		  context.stats.timeSpent = timeSpent;
		  context.stats.efficiency = 1 - styles.length / context.stats.originalSize;
		  context.stats.minifiedSize = styles.length;

		  return context.stats;
		}
		*/

		return CleanCSS;
	};
	//#endregion

	return require('/clean');
})();

function declensionOfNumerals(number, titles) {
	var result,
		titleIndex,
		cases = [2, 0, 1, 1, 1, 2],
		caseIndex
		;

	if (number % 100 > 4 && number % 100 < 20) {
		titleIndex = 2;
	}
	else {
		caseIndex = number % 10 < 5 ? number % 10 : 5;
		titleIndex = cases[caseIndex];
	}

	result = titles[titleIndex];

	return result;
}

function declinationOfSeconds(number) {
	return declensionOfNumerals(number, ['секунда', 'секунды', 'секунд']);
}