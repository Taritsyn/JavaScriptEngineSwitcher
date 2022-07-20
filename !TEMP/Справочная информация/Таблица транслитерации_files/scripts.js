

    var standarts = {
    standartTranslitTsymbalSu:Array("translit.tsymbal.su",
        "a",    "b",    "v",    "g",    "d",
        "e",    "yo",   "zh",   "z",    "i",
        "y",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "h",    "ts",   "ch",
        "sh",   "sch",  "", "y",    "",
        "e",    "yu",   "ya"
        ),
    standartYandex:Array("Yandex.ru",
        "a",    "b",    "v",    "g",    "d",
        "e",    "e",    "zh",   "z",    "i",
        "j",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "c",    "ch",
        "sh",   "shh",  "", "y",    "",
        "e",    "yu",   "ya"
        ),
    standartTranslitRu:Array("Translit.Ru",
        "a",    "b",    "v",    "g",    "d",
        "e",    "jo",   "zh",   "z",    "i",
        "j",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "h",    "c",    "ch",
        "sh",   "shh",  "", "y",    "",
        "je",   "ju",   "ja"
        ),
    standartBukvyTsifri:Array("Буквы-цифры (SMS)",
        "a",    "b",    "8",    "g",    "9",
        "e",    "jo",   "#",    "3",    "i",
        "j",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "h",    "c",    "4",
        "6",    "w",    "", "y",    "",
        "je",   "ju",   "ja"
        ),
    standartGost7792000:Array("ГОСТ 7.79-2000",
        "a",    "b",    "v",    "g",    "d",
        "e",    "yo",   "zh",   "z",    "i",
        "j",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "x",    "c",    "ch",
        "sh",   "shh",  "", "y",    "",
        "e'",   "yu",   "ya"
        ),
    standartGost1687671:Array("ГОСТ 16876-71",
        "a",    "b",    "v",    "g",    "d",
        "e",    "jo",   "zh",   "z",    "i",
        "jj",   "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "c",    "ch",
        "sh",   "shh",  "", "y",    "",
        "eh",   "ju",   "ja"
        ),
    standartSev136278:Array("СЭВ 1362-78",
        "a",    "b",    "v",    "g",    "d",
        "e",    "jo",   "zh",   "z",    "i",
        "j",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "c",    "ch",
        "sh",   "shh",  "", "y",    "",
        "eh",   "ju",   "ja"
        ),
    standartMvd:Array("МВД",
        "a",    "b",    "v",    "g",    "d",
        "e",    "ye",   "zh",   "z",    "i",
        "y",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "ts",   "ch",
        "sh",   "shch", "", "y",    "",
        "e",    "yu",   "ya"
        ),
    standartLc:Array("LC",
        "a",    "b",    "v",    "g",    "d",
        "e",    "e",    "zh",   "z",    "i",
        "i",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "ts",   "ch",
        "sh",   "shch", "", "y",    "",
        "e",    "iu",   "ia"
        ),
    standartBgn:Array("BGN",
        "a",    "b",    "v",    "g",    "d",
        "e",    "e",    "zh",   "z",    "i",
        "y",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "ts",   "ch",
        "sh",   "shch", "", "y",    "",
        "e",    "yu",   "ya"
        ),
    standartBsi:Array("BSI",
        "a",    "b",    "v",    "g",    "d",
        "e",    "e",    "zh",   "z",    "i",
        "i",    "k",    "l",    "m",    "n",
        "o",    "p",    "r",    "s",    "t",
        "u",    "f",    "kh",   "ts",   "ch",
        "sh",   "shch", "", "y",    "",
        "e",    "yu",   "ya"
        )}


    var cyrillic = Array(
        "а",    "б",    "в",    "г",    "д",    "е",    "ё",
        "ж",    "з",    "и",    "й",    "к",    "л",    "м",
        "н",    "о",    "п",    "р",    "с",    "т",    "у",
        "ф",    "х",    "ц",    "ч",    "ш",    "щ",    "ъ",
        "ы",    "ь",    "э",    "ю",    "я");



jQuery(document).ready(function($){

    var makesymbolstable = function(st) {
        var symbolstable = "";
        for (var i=0;i<33;i++) {
            iplus = i+1;
            if (iplus < 10) {iplus = "0"+iplus}
            symbolstable += "<label for='"+st+"_"+iplus+"'>"+cyrillic[i]+" = <input value='' type='text' maxlength='4' name='datas["+st+"]["+st+"_"+iplus+"]' id='"+st+"_"+iplus+"'></label>";
        }
        $("[data-resid="+st+"] .result-symbols .butcontent").html(symbolstable);
    }

    makesymbolstable("st1");
    makesymbolstable("st2");
    makesymbolstable("st3");

    var applyStandart = function(standart,standartobject,st) {
        for (var i=0;i<33;i++) {
            iplus = i+1;
            if (iplus < 10) {iplus = "0"+iplus}
            $("#"+st+"_"+iplus).val(standartobject[i+1]);
        }
        $("[data-resid="+st+"]").data("standart",standartobject[0]);
        if (st == "st1") {result = "result01";}
        if (st == "st2") {result = "result02";}
        if (st == "st3") {result = "result03";}
        $("."+result).attr("placeholder","результат "+standartobject[0]).attr("title","результат в соответствии со стандартом "+standartobject[0]);
        $("[data-resid="+st+"] .result-list ul li a").removeClass("active");
        $("[data-resid="+st+"] .result-list ul li a#"+standart).addClass("active");
    }

    applyStandart("standartTranslitRu",standarts["standartTranslitRu"],"st1");
    applyStandart("standartMvd",standarts["standartMvd"],"st2");
    applyStandart("standartLc",standarts["standartLc"],"st3");

    var showStandartsMenu = function(st) {
        var ul = "", active = "";
        ul = "<ul class='standarts'>";
        for (standart in standarts) {
            if (standarts[standart][0] == $("[data-resid="+st+"]").data("standart")) {
                active = "active";
            } else {
                active = "";
            }
            ul += "<li><a href='#' class='"+active+"' id='"+standart+"' data-st='"+st+"'>"+standarts[standart][0]+"</a></li>";

            ul += "";
        }
        ul += "</ul>";
        $("[data-resid="+st+"] .result-list .butcontent").html(ul);
    }

    showStandartsMenu("st1");
    showStandartsMenu("st2");
    showStandartsMenu("st3");


    $(".result-list .butcontent a").on("click",function(a){
        a.preventDefault();
        applyStandart($(this).attr("id"),standarts[$(this).attr("id")],$(this).data("st"));
    });

    $(".butholder").on("click",function(a){
        a.stopPropagation();
    })
    $("body").on("click",function(){
        $(".butholder .butcontent").hide()
        $(".butholder .but").removeClass("active");
    })

    $(".butholder .but").on("click",function(a){
        a.preventDefault();
        if(!$(this).is(".active")) {
            $(".butholder .butcontent").hide();
            $(".butholder .but").removeClass("active");
            $(this).addClass("active");
            $("+ .butcontent",this).slideDown(200);
        } else {
            $(this).removeClass("active");
            $("+ .butcontent",this).slideUp(200);
        }
    })



    var getlasttranslit = function(){
        $(".lasttranslit span").load("/lasttranslit.php");
    }

    var lasttranslitinterval = setInterval(function(){
        getlasttranslit();
    },180000);
    getlasttranslit();

    $(".results").on("scroll",function(){
        $(".results").scrollTop($(this).scrollTop());
    })

    $(".cleanhistory").on("click",function(a){
        a.preventDefault();
        $(".results").html("").attr("style","");
    })




    $(document).on("submit",".datas",function(){

        var thisform = $(this);
        //console.log(thisform.serialize());

        $.ajax({
          type: 'POST',
          url: thisform.attr("action"),
          data: thisform.serialize(),
          success: function(answer) {
            console.log(answer);
            answer = $.parseJSON(answer);
            if (answer.length > 2) {
                $(".result01").val(answer[0]);
                $(".result02").val(answer[1]);
                $(".result03").val(answer[2]);
                $(".result01-history").show().prepend("<input type='text' value='"+answer[0]+"' onMouseUp='select(this);'>").scrollTop(0);
                $(".result01-history input").slideDown(300);
                $(".result02-history").show().prepend("<input type='text' value='"+answer[1]+"' onMouseUp='select(this);'>").scrollTop(0);
                $(".result02-history input").slideDown(300);
                $(".result03-history").show().prepend("<input type='text' value='"+answer[2]+"' onMouseUp='select(this);'>").scrollTop(0);
                $(".result03-history input").slideDown(300);
            }
          },
          error:  function(xhr, str){
                alert('Возникла ошибка: ' + JSON.stringify(xhr) + '. Ведутся технические работы. Просим обновить страницу в теч. часа');
            }
        });
        return false;
    });






});