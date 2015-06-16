if (!Modernizr.inputtypes.time) {

    $(function () {

        $(".timecontrol").timepicker({
            'step': '15',
            'timeFormat': 'h:i A'
        });

    });

}