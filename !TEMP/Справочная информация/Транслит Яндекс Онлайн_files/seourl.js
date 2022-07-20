function get_data(){
    mydata = $('#url').val();
    return mydata.toLowerCase();
}

function write_url(){
    data = get_data();
    $("#resultconvert").text(toTranslit(data));
}

function toTranslit( text ) {
    return text.replace( /([а-яё])|([\s_-])|([^a-z\d])/gi,
        function( all, ch, space, words, i ) {
            if ( space || words ) {
                return space ? '-' : '';
            }
            var code = ch.charCodeAt(0),
                next = text.charAt( i + 1 ),
                index = code == 1025 || code == 1105 ? 0 :
                    code > 1071 ? code - 1071 : code - 1039,
                t = ['yo','a','b','v','g','d','e','zh',
                    'z','i','y','k','l','m','n','o','p',
                    'r','s','t','u','f','h','c','ch','sh',
                    'shch','','y','','e','yu','ya'
                ],
                next = next && next.toUpperCase() === next ? 1 : 0;
            return ch.toUpperCase() === ch ? next ? t[ index ].toUpperCase() :
                t[ index ].substr(0,1).toUpperCase() +
                    t[ index ].substring(1) : t[ index ];
        }
    );
}
