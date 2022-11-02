(function ($) {
    'use strict';
    $.fn.dateTimePicker = function (options) {
        var sts = _.get(options,'selectedTimestamp');
        if (_.isUndefined(sts) || _.isEmpty(sts) || sts === 0){
            sts = "now";
        }
        var settings = $.extend({
            displayFormat: "YYYY-MM-DD HH:mm:ss",
            selectedTimestamp: sts,
            dateFormat: "YYYY-MM-DD HH:mm:ss",
            locale: 'en',
            positionShift: { top: 20, left: 0},
            title: "Select Date and Time",
            buttonTitle: "Select",
            timeSelect: "keyboard"//"mouse" || "keyboard"
        }, options);
        
        moment.locale(settings.locale);
        var elem = this;
        var limitation = {"hour": 23, "minute": 59, "second": 59};
        var mousedown = false;
        var timeout = 800;
        var selectDate;
        var startDate;
        var lastSelected;

        function updateMainElemGlobal() {
            var $s = $('<span>');
            $s.text(lastSelected.format(settings.displayFormat));
            elem.empty();
            elem.append($s);
        }

        function setBaseDateTime(){
            selectDate = settings.selectedTimestamp === "now" ? moment() : moment(settings.selectedTimestamp, settings.dateFormat);
            startDate = copyDate(moment());
            lastSelected = copyDate(selectDate);
            elem.next().val(selectDate.format(settings.dateFormat));
        }
        
        return this.each(function () {
            elem.addClass("dtp_main");
            setBaseDateTime();
            updateMainElemGlobal();
            elem.on('click', function () {
                setBaseDateTime();
                updateMainElemGlobal();
                if (lastSelected != selectDate) {
                    selectDate = copyDate(lastSelected);
                }
                var $win = $('<div id="dvDateTimePickerModal">');
                $win.addClass("dtp_modal-win");
                var $body = $('body');
                $body.append($win);
                var $content = createContent();
                $body.append($content);
                var offset = elem.offset();
                $content.css({top: (offset.top + settings.positionShift.top) + "px", left: (offset.left + settings.positionShift.left) + "px"});
                feelDates(selectDate);
                $win.on('click', function () {
                    $content.remove();
                    $win.remove();
                })
                
                var $fieldTime = $('#field-time');
                var $hour = $fieldTime.find('#d-hh');
                var $minute = $fieldTime.find('#d-mm');
                var $second = $fieldTime.find('#d-ss');
                attachChangeTime();

                function feelDates(selectM) {
                    var $fDate = $content.find('#field-data');
                    $fDate.empty();
                    $fDate.append(createMonthPanel(selectM));
                    $fDate.append(createCalendar(selectM));
                }

                function createCalendar(selectedMonth) {
                    var $c = $('<div>');
                    $c.addClass('dtp_modal-calendar');
                    for (var i = 0; i < 7; i++) {
                        var $e = $('<div>');
                        $e.addClass('dtp_modal-calendar-cell dtp_modal-colored');
                        $e.text(moment().weekday(i).format('ddd'));
                        $c.append($e);
                    }
                    var m = copyDate(selectedMonth);
                    m.date(1);
                    var flagStart = totalMonths(selectedMonth) === totalMonths(startDate);
                    var flagSelect = totalMonths(lastSelected) === totalMonths(selectedMonth);
                    var cerDay = parseInt(selectedMonth.format('D'));
                    var dayNow = parseInt(startDate.format('D'));
                    for (var i = 0; i < 6; i++) {
                        for (var j = 0; j < 7; j++) {
                            var $b = $('<div>');
                            $b.html('&nbsp;');
                            $b.addClass('dtp_modal-calendar-cell');
                            if (m.month() == selectedMonth.month() && m.weekday() == j) {
                                var day = parseInt(m.format('D'));
                                $b.text(day);
                                if (flagSelect && day == cerDay) {
                                    $b.addClass('dtp_modal-cell-selected');
                                }
                                else {
                                    $b.addClass('cursorily');
                                    $b.bind('click', changeDate);
                                }
                                m.add(1, 'days');
                            }
                            $c.append($b);
                        }
                    }
                    return $c;
                }

                function changeDate() {
                    var $div = $(this);
                    selectDate.date($div.text());
                    lastSelected = copyDate(selectDate);
                    updateDate();
                    var $fDate = $content.find('#field-data');
                    var old = $fDate.find('.dtp_modal-cell-selected');
                    old.removeClass('dtp_modal-cell-selected');
                    old.addClass('cursorily');
                    $div.addClass('dtp_modal-cell-selected');
                    $div.removeClass('cursorily');
                    old.bind('click', changeDate);
                    $div.unbind('click');
                }

                function createMonthPanel(selectMonth) {
                    var $d = $('<div>');
                    $d.addClass('dtp_modal-months');
                    var $s = $('<i></i>');
                    $s.addClass('fa fa-angle-left cursorily ico-size-month hov');
                    //$s.attr('data-fa-mask', 'fas fa-circle');
                    $s.bind('click', prevMonth);
                    $d.append($s);
                    $s = $('<span>');
                    $s.text(selectMonth.format("MMMM YYYY"));
                    $d.append($s);
                    $s = $('<i></i>');
                    $s.addClass('fa fa-angle-right cursorily ico-size-month hov');
                    $s.bind('click', nextMonth);
                    $d.append($s);
                    return $d;
                }

                function close() {
                    if (settings.timeSelect === 'keyboard'){
                        lastSelected.hour(parseInt($hour.val()));
                        lastSelected.minute(parseInt($minute.val()));
                        lastSelected.second(parseInt($second.val()));
                        selectDate.hour(parseInt($hour.val()));
                        selectDate.minute(parseInt($minute.val()));
                        selectDate.second(parseInt($second.val()));
                    }else{
                        lastSelected.hour(parseInt($hour.text()));
                        lastSelected.minute(parseInt($minute.text()));
                        lastSelected.second(parseInt($second.text()));
                        selectDate.hour(parseInt($hour.text()));
                        selectDate.minute(parseInt($minute.text()));
                        selectDate.second(parseInt($second.text()));
                    }
                    updateDate();
                    if (_.isFunction(settings.callback)){
                        settings.callback(lastSelected.format(settings.dateFormat), settings.injectionNumber);
                    }
                    $content.remove();
                    $win.remove();
                }

                function nextMonth() {
                    selectDate.add(1, 'month');
                    feelDates(selectDate);
                }

                function prevMonth() {
                    selectDate.add(-1, 'month');
                    feelDates(selectDate);
                }

                function attachChangeTime() {
                    if (settings.timeSelect === "keyboard"){
                        var h = selectDate.hours();
                        var m = selectDate.minutes();
                        var s = selectDate.seconds();
                        $hour.bind('keyup',function(){
                            updateTime("hour");
                        }).val(h);

                        $minute.bind('keyup',function(){
                            updateTime("minute");
                        }).val(m);

                        $second.bind('keyup',function(){
                            updateTime("second");
                        }).val(s);
                    }else{
                        var $angles = $($content).find('i[id^="angle-"]');
                        // $angles.bind('click', changeTime);
                        $angles.bind('mouseup', function () {
                            mousedown = false;
                            timeout = 800;
                        });
                        $angles.bind('mousedown', function () {
                            mousedown = true;
                            changeTime(this);
                        });
                    }
                }

                function changeTime(el) {
                    var $el = this || el;
                    $el = $($el);
                    ///angle-up-hour angle-up-minute angle-up-second angle-down-hour angle-down-minute angle-down-second
                    var arr = $el.attr('id').split('-');
                    var increment = 1;
                    if (arr[1] == 'down') {
                        increment = -1;
                    }
                    appendIncrement(arr[2], increment);
                    setTimeout(function () {
                        autoIncrement($el);
                    }, timeout);
                }

                function autoIncrement(el) {
                    if (mousedown) {
                        if (timeout > 200) {
                            timeout -= 200;
                        }
                        changeTime(el);
                    }
                }

                function appendIncrement(typeDigits, increment) {
                    var $i = "hour";
                    switch(typeDigits){
                        case "hour":
                            $i = $hour;
                            break;
                        case "minute":
                            $i = $minute;
                            break;
                        case "second":
                            $i = $second;
                            break;
                        default:
                            break;
                    }
                    //var $i = typeDigits == "hour" ? $hour : $minute;
                    var val = parseInt($i.text()) + increment;
                    if (val < 0) {
                        val = limitation[typeDigits];
                    }
                    else if (val > limitation[typeDigits]) {
                        val = 0;
                    }
                    $i.text(formatDigits(val));
                    updateDate();
                }

                function updateTime(typeDigits){
                    var $i = "hour";
                    switch(typeDigits){
                        case "hour":
                            $i = $hour;
                            break;
                        case "minute":
                            $i = $minute;
                            break;
                        case "second":
                            $i = $second;
                            break;
                        default:
                            break;
                    }
                    //var $i = typeDigits == "hour" ? $hour : $minute;
                    var val = $i.val();
                    if (val < 0) {
                        val = limitation[typeDigits];
                    }
                    else if (val > limitation[typeDigits]) {
                        val = 0;
                    }
                    //$i.val(formatDigits(val));
                    if ((''+val).length > 2){
                        val = val.substring(0,2);
                    }
                    $i.val(val);
                    updateDate();
                }

                function formatDigits(val) {
                    if (val < 10) {
                        return '0' + val;
                    }
                    return val;
                }

                function createTimer() {
                    var $div = $('<div>');
                    $div.addClass('dtp_modal-time-mechanic');
                    if (settings.timeSelect != "keyboard"){
                        var $panel = $('<div>');
                        $panel.addClass('dtp_modal-append');
                        var $i = $('<i>');
                        $i.attr('id', 'angle-up-hour');
                        $i.addClass('fa fa-angle-up ico-size-large cursorily hov');
                        $panel.append($i);

                        var $m = $('<span>');
                        $m.addClass('dtp_modal-midle');
                        $panel.append($m);

                        $i = $('<i>');
                        $i.attr('id', 'angle-up-minute');
                        $i.addClass('fa fa-angle-up ico-size-large cursorily hov');
                        $panel.append($i);

                        var $m = $('<span>');
                        $m.addClass('dtp_modal-midle');
                        $panel.append($m);

                        $i = $('<i>');
                        $i.attr('id', 'angle-up-second');
                        $i.addClass('fa fa-angle-up ico-size-large cursorily hov');
                        $panel.append($i);

                        $div.append($panel);
                    
                        $panel = $('<div>');
                        $panel.addClass('dtp_modal-digits');

                        var $h = $('<span>');
                        $h.addClass('dtp_modal-digit');
                        $h.attr('id', 'd-hh');
                        $h.text(lastSelected.format('HH'));
                        $panel.append($h);

                        var $c = $('<span>');
                        $c.addClass('dtp_modal-midle-dig');
                        $c.html(':');
                        $panel.append($c);

                        $m = $('<span>');
                        $m.addClass('dtp_modal-digit');
                        $m.attr('id', 'd-mm');
                        $m.text(lastSelected.format('mm'));
                        $panel.append($m);

                        var $c2 = $('<span>');
                        $c2.addClass('dtp_modal-midle-dig');
                        $c2.html(':');
                        $panel.append($c2);

                        var $s = $('<span>');
                        $s.addClass('dtp_modal-digit');
                        $s.attr('id', 'd-ss');
                        $s.text(lastSelected.format('ss'));
                        
                        $panel.append($s);
                        $div.append($panel);

                        $panel = $('<div>');
                        $panel.addClass('dtp_modal-append');
                        $i = $('<i>');
                        $i.attr('id', 'angle-down-hour');
                        $i.addClass('fa fa-angle-down ico-size-large cursorily hov');
                        $panel.append($i);

                        $m = $('<span>');
                        $m.addClass('dtp_modal-midle');
                        $panel.append($m);
                        $i = $('<i>');
                        $i.attr('id', 'angle-down-minute');
                        $i.addClass('fa fa-angle-down ico-size-large cursorily hov');
                        $panel.append($i);

                        $m = $('<span>');
                        $m.addClass('dtp_modal-midle');
                        $panel.append($m);

                        $i = $('<i>');
                        $i.attr('id', 'angle-down-second');
                        $i.addClass('fa fa-angle-down ico-size-large cursorily hov');
                        $panel.append($i);
                    }else{
                        $panel = $('<div>');
                        $panel.addClass('dtp_modal-digits');

                        var $h = $('<input type="text" inputmode="numeric">');
                        $h.addClass('dtp_modal-digit');
                        $h.attr('id', 'd-hh');
                        $h.text(lastSelected.format('HH'));
                        $h.change(updateDate());
                        $panel.append($h);

                        var $c = $('<span>');
                        $c.addClass('dtp_modal-midle-dig');
                        $c.html(':');
                        $panel.append($c);

                        $m = $('<input type="text" inputmode="numeric">');
                        $m.addClass('dtp_modal-digit');
                        $m.attr('id', 'd-mm');
                        $m.text(lastSelected.format('mm'));
                        $m.change(updateDate());
                        $panel.append($m);

                        var $c2 = $('<span>');
                        $c2.addClass('dtp_modal-midle-dig');
                        $c2.html(':');
                        $panel.append($c2);

                        var $s = $('<input type="text" inputmode="numeric">');
                        $s.addClass('dtp_modal-digit');
                        $s.attr('id', 'd-ss');
                        $s.text(lastSelected.format('ss'));
                        $s.change(updateDate());
                        
                        $panel.append($s);
                    }
                    $div.append($panel);
                    return $div;
                }

                function createContent() {
                    var $c = $('<div>');
                    $c.addClass("dtp_modal-content");
                    var $el = $('<div>');
                    $el.addClass("dtp_modal-title");
                    $el.text(settings.title);
                    $c.append($el);
                    $el = $('<div>');
                    $el.addClass('dtp_modal-cell-date');
                    $el.attr('id', 'field-data');
                    $c.append($el);
                    $el = $('<div>');
                    $el.addClass('dtp_modal-cell-time');
                    var $a = $('<div>');
                    $a.addClass('dtp_modal-time-block');
                    $a.attr('id', 'field-time');
                    $el.append($a);
                    var $line = $('<div>');
                    $line.attr('id', 'time-line');
                    $line.addClass('dtp_modal-time-line');
                    $line.text(lastSelected.format(settings.dateFormat));

                    $a.append($line);
                    $a.append(createTimer());
                    var $but = $('<div>');
                    $but.addClass('dpt_modal-button');
                    $but.text(settings.buttonTitle);
                    $but.bind('click', close);
                    $el.append($but);
                    $c.append($el);
                    return $c;
                }
                function updateDate() {
                    $('#time-line').text(lastSelected.format(settings.dateFormat));
                    updateMainElemGlobal();
                    elem.next().val(selectDate.format(settings.dateFormat));
                }

            });

        });

    };

    function copyDate(d) {
        return moment(d.toDate());
    }

    function totalMonths(m) {
        var r = m.format('YYYY') * 12 + parseInt(m.format('MM'));
        return r;
    }

}(jQuery));
// fa-caret-down