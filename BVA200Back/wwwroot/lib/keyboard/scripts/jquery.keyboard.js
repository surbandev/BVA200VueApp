$.fn.attachKeyboard = function (options) {
    $(document).ready(function () {
        var input = undefined; //this is how we'll know which control to append value to

        var keyboard = $('#onScreenKeyboard');
        var kbd = `<div class="keyboard alphabetKeyboard">
            <div class="keys flexy">
                <a href="#">a</a>
                <a href="#">b</a>
                <a href="#">c</a>
                <a href="#">d</a>
                <a href="#">e</a>
                <a href="#">f</a>
                <a href="#">g</a>
            </div>
            <div class="keys flexy">
                <a href="#">h</a>
                <a href="#">i</a>
                <a href="#">j</a>
                <a href="#">k</a>
                <a href="#">l</a>
                <a href="#">m</a>
                <a href="#">n</a>
            </div>
            <div class="keys flexy">
                <a href="#">o</a>
                <a href="#">p</a>
                <a href="#">q</a>
                <a href="#">r</a>
                <a href="#">s</a>
                <a href="#">t</a>
                <a href="#">u</a>
            </div>
            <div class="keys flexy">
                <a href="#" class="shift">&uarr;</a>
                <a href="#">v</a>
                <a href="#">w</a>
                <a href="#">x</a>
                <a href="#">y</a>
                <a href="#">z</a>
                <a href="#" class="backspace">&larr;</a>

            </div>
            <div class="keys control flexy">
                <a class="characterset" href="#">.123</a>
                <a class="space" href="#">space</a>
                <a class="close" href="#">close</a>
            </div>
        </div>
        
        <div class="keyboard numericKeyboard" style="display:none">
            <div class="keys flexy">
                <a href="#">1</a>
                <a href="#">2</a>
                <a href="#">3</a>
                <a href="#">/</a>
            </div>
            <div class="keys flexy">
                <a href="#">4</a>
                <a href="#">5</a>
                <a href="#">6</a>
                <a href="#">*</a>
            </div>
            <div class="keys flexy">
                <a href="#">7</a>
                <a href="#">8</a>
                <a href="#">9</a>
                <a href="#">-</a>
            </div>
            <div class="keys flexy">
                <a href="#" class="zero">0</a>
                <a href="#">:</a>
                <a href="#">.</a>
                <a href="#">+</a>
            </div>

            <div class="keys control flexy">
                <a class="characterset" href="#">abc</a>
                <a class="space" href="#">space</a>
                <a href="#" class="backspace">&larr;</a>
                <a class="close" href="#">close</a>
            </div>
        </div>
        <div class="blur-bg"></div>
        `;
        $('#onScreenKeyboard').html(kbd);

        //this is a hack to deal with number fields not revealing keyboard on first click.
        $("input").each(function () {            
            $(this).focus(() => {
                let type = $(this).attr('inputmode');
                if (type === "text" || type === "password"){
                    $(this).trigger('click');
                    input = $(this);
                    var fldLength= $(input).val().length;
                    $(input)[0].setSelectionRange(fldLength, fldLength);
                }
            })
        });

        // bind keyboard characters:
        $(keyboard).find('a:not(.shift,.backspace,.characterset,.space,.close)').bind('click', function (e) {
            if (!keystrokeValid) {
                return;
            }
            var newValue = String($(input).val()) + '' + String($(this).text());
            keyPressed(newValue);
            e.preventDefault();
        });

        // space bar:
        $(keyboard).find('a.space').bind('click', function (e) {
            if (!keystrokeValid) {
                return;
            }
            var newValue = $(input).val() + ' ';
            keyPressed(newValue);
            e.preventDefault();
        });

        // backspace:
        $(keyboard).find('a.backspace').bind('click', function (e) {
            if (!keystrokeValid) {
                return;
            }
            var currentValue = $(input).val();
            if (currentValue.length == 0)
                return false;

            var newValue = currentValue.substr(0, currentValue.length - 1);
            keyPressed(newValue);
            e.preventDefault();
        });

        // shift key:
        $(keyboard).find('a.shift').click(function (e) {

            var isInverse = $(this).hasClass('inverse');

            $(keyboard).find('a:not(.shift,.backspace,.characterset,.space,.close)').each(function () {
                $(this).text($(this).text()[isInverse ? 'toLowerCase' : 'toUpperCase']());
            });

            $(this).toggleClass('inverse');

            e.preventDefault();
        });

        // character set shift:
        $(keyboard).find('a.characterset').click(function (e) {
            toggleKeyboardLayout();
            e.preventDefault();
        });

        function toggleKeyboardLayout() {
            $('.numericKeyboard').toggle();
            $('.alphabetKeyboard').toggle();
        }

        function showAlphabetKeyboard() {
            $('.numericKeyboard').hide();
            $('.alphabetKeyboard').show();
            $(keyboard).show();
        }

        function showNumericKeyboard() {
            $('.numericKeyboard').show();
            $('.alphabetKeyboard').hide();
            $(keyboard).show();
        }

        function keystrokeValid() {
            if ($(input).is(":disabled")) {
                return false;
            }
            return true;
        }
        function hideKeyboard() {
            $(keyboard).hide();
            input = undefined;
            handleVisibility();
        }
        
        function showKeyboard(type){
            //select all the text in the input so the user may begin typing immediately
            handleVisibility();
            if (type === "numeric") {
                showNumericKeyboard();
            }
            if (type === "text" || type === "password") {
                showAlphabetKeyboard();
            }
        }

        function handleVisibility(){
            if (!input){
                //keyboard was hidden
                $('#oskSpacer').hide();
            }else{
                //keyboard was shown
                //remember, 0,0 is the top-left
                var windowHeight = $(window).height();//should be the screen height since we're running in Kiosk mode
                var documentHeight = $(document).height();
                var keyboardHeight = $(keyboard).height();
                var inputHeight = $(input).height();

                var inputY = $(input).offset().top; //should be number of pixels from top of document

                //since the keyboard is fixed placed, can't use the offset. Going to have to take the height and calculate
                var keyboardY = windowHeight - keyboardHeight;
                var keyboardBlocking = inputY + inputHeight > keyboardY;
                
                //input height
                var inputLabelOffsetFactor = 2.25;//This is the height of the label plus the space in between; 2.25x the height of the input.
                var offset = (inputY - keyboardY) + (inputHeight * inputLabelOffsetFactor);
                /*
                    console.log('keyboardHeight',keyboardHeight);
                    console.log('windowHeight',windowHeight);
                    console.log('inputHeight',inputHeight);
                    console.log('inputY', inputY);
                    console.log('keyboardY',keyboardY);
                    console.log('keyboardBlocking',keyboardBlocking);
                    console.log('offset',offset);
                */

                if (keyboardBlocking){
                    $('#oskSpacer').show();
                    $('#oskSpacer').height(offset);
                    $(window).scrollTop(documentHeight);
                    /*
                        If the tour guide is in place, we need to wait for the animation to finish before we try to move the control where
                        it's visible. 
                    */
                    if ($(".enjoyhint")){
                        setTimeout(function(){
                            $(window).trigger('resize');//this will trigger any listeners anywhere in the program to do special handling.
                        }, 350);
                    }else{
                        $(window).trigger('resize');//this will trigger any listeners anywhere in the program to do special handling.
                    }
                }
            }
        }

        function keyPressed(newValue) {
            $(input).val(newValue);
            //this "hack" trigger's vue's logic to fire
            $(input)[0].dispatchEvent(new Event('input'));
        }

        // close:
        $(keyboard).find('a.close').bind('click', function (e) {
            $(input).trigger(
                jQuery.Event('keydown', { which: 13 })
              );
            hideKeyboard();
            e.preventDefault();
        });

        // document click function:
        $(document).bind('click', function (e) {
            let tgt = e.target;
            let type = $(tgt).attr('inputmode');
            let validInputs = ["text", "numeric", "password"];
            var inputClicked = tgt.tagName === "INPUT" && _.includes(validInputs, type);
            var $clicked = $(tgt); // get the element clicked
            if (!$clicked.is('.keyboard') && !$clicked.parents().is('.keyboard') && !$clicked.is(":disabled")) {
                if (inputClicked) {
                    input = tgt;
                    showKeyboard(type);
                } else {
                    hideKeyboard();
                }
            }
        });
    });
};
