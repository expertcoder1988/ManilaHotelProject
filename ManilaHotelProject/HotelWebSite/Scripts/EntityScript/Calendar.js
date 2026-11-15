$(document).ready(function () {
    if ($('#AvoidReload').val() != "True") {
        // alert("g");
        //loadCalendar();
    }

    //var d = $('#calendar').fullCalendar('getDate');
    //var responseDate = moment(d).format('MM/DD/YYYY');
    //loadCal(1, responseDate);


    $('body').on('click', 'button.fc-prev-button', function () {
        //  debugger;


    });
    $('body').on('click', 'button.fc-next-button', function () {
        // debugger;

    });

    $(function () {
        /*        $('#calendar').fullCalendar('getDate');*/
        try {
            $.ajax({
                url: '/Property/Properties',
                type: 'GET',
                dataType: 'json',
                contentType: 'application/json',
                success: function (data) {
                    // console.log("K");
                    $('#AvoidReload').val("True");
                    // alert("f");
                    //var ddlPropertyTypes = $("[id*=PropertyTypes]");
                    $('#Properties').empty().append('<option selected="selected" value="0">Please select</option>');
                    //if (data.length > 0) {

                    $.each(data, function (i, item) {
                        $('#Properties').append($('<option></option>').attr('value', item.Id).text(item.PropertyName));
                    });
                    $("select#Properties").prop('selectedIndex', 1);
                    loadCal($('#Properties').val());
                    //document.getElementById('#PropertyTypes').selectedIndex = 1;
                    //}
                },
                error: function (err) {
                    //  alert(err.status + " : " + err.statusText);
                }

            });
        } catch (e) {
            window.location.href = '/Property/RateSlab';
        }
    })


    $('#Properties').change(function () {
        var propertyID = new Object();
        propertyID = $('#Properties').val();

        loadCal(propertyID);

    });


    function loadCal(propId, responseDate) {

        // console.log(propertyID);
        $.ajax({
            url: "/Calander/GetEventsNew",
            type: 'GET',
            dataType: 'json',
            contentType: 'application/json',
            data: 'propertyID=' + propId + '&responseDate=' + responseDate,
            success: function (data) {
                // console.log(data);
                initThemeChooser({
                    init: function (themeSystem) {
                        $('#calendar').fullCalendar({
                            schedulerLicenseKey: 'GPL-My-Project-Is-Open-Source',
                            defaultDate: $('#calendar').fullCalendar('getDate'),
                            editable: true,
                            aspectRatio: 1.8,
                            theme: true,
                            themeSystem: 'jquery-ui',
                            scrollTime: '00:00',
                            header: {
                                left: 'prev,next',
                                center: 'title',
                                right: 'customWeek,agendaSevenDays'
                            },
                            firstDay: 1,
                            defaultView: 'customWeek',
                            views: {
                                customWeek: {
                                    type: 'timeline',
                                    duration: { days: 31 },
                                    buttonText: 'month'
                                },
                                //customWeek: {
                                //    type: 'agendaWeek',
                                //    duration: { days: 31 },
                                //    slotDuration: { days: 1 },
                                //    buttonText: 'month'
                                // }
                            },
                            slotLabelFormat: ['MMMM YYYY', 'D'],
                            timeFormat: 'YYYY, MM, DD',
                            navLinks: true,
                            resourceAreaWidth: '10%',
                            resourceLabelText: 'Rooms List',
                            resources: $.map(data, function (item, i) {
                                var resource = new Object();
                                resource.id = item.RoomNo;
                                resource.category = item.ProductName
                                resource.title = item.RoomNo;
                                //console.log(resource);
                                return resource;
                            }),
                            eventSources: [
                                {
                                    events: $.map(data, function (item, i) {
                                        //console.log(item.BookingStatus);
                                        if (item.BookingStatus == "Paid") {
                                            var event = new Object();
                                            event.id = item.EventID;
                                            event.resourceId = item.RoomNo;
                                            event.start = new Date(moment(item.StartDate).format('MM/DD/YYYY'));
                                            event.end = new Date(moment(item.EndDate).format('MM/DD/YYYY'));
                                            event.title = item.EventName;
                                            event.description = "<b>Source: </b>" + item.PropertySource + "<br/><br/><b>Booking No: </b>" + item.ConfirmationNo;
                                            return event;
                                        }
                                    }),
                                    backgroundColor: 'green',
                                    borderColor: 'green',
                                    textColor: 'white'
                                },
                                {
                                    events:
                                        $.map(data, function (item, i) {
                                            // console.log(item.StartDate);
                                            if (item.BookingStatus == "Booked") {
                                                var event = new Object();
                                                event.id = item.EventID;
                                                event.resourceId = item.RoomNo;
                                                event.start = new Date(moment(item.StartDate).format('MM/DD/YYYY'));
                                                event.end = new Date(moment(item.EndDate).format('MM/DD/YYYY'));
                                                event.title = item.EventName;
                                                event.description = "<b>Source: </b>" + item.PropertySource + "<br/><br/><b>Booking No: </b>" + item.ConfirmationNo;
                                                // console.log(event);
                                                return event;
                                            }
                                        }),
                                    backgroundColor: 'red',
                                    borderColor: 'red',
                                    textColor: 'white'
                                },
                                {
                                    events: $.map(data, function (item, i) {
                                        if (item.BookingStatus == "CheckIn") {
                                            var event = new Object();
                                            event.id = item.EventID;
                                            event.resourceId = item.RoomNo;
                                            event.start = new Date(moment(item.StartDate).format('MM/DD/YYYY'));
                                            event.end = new Date(moment(item.EndDate).format('MM/DD/YYYY'));
                                            event.title = item.EventName;
                                            event.description = "<b>Source: </b>" + item.PropertySource + "<br/><br/><b>Booking No: </b>" + item.ConfirmationNo;
                                            return event;
                                        }
                                    }),
                                    backgroundColor: 'blue',
                                    borderColor: 'blue',
                                    textColor: 'white'
                                }

                            ],
                            eventMouseover: function (data, event, view) {

                                tooltip =
                                    '<div class="tooltiptopicevent" style="width:auto;height:auto;background:#feb811;position:absolute;z-index:10001;padding:10px 10px 10px 10px ;  line-height: 200%;">' + '<b>title:</b> ' + data.title + '</br>' + '<b>start:</b> ' + ': ' + data.start.format('YYYY-MM-DD') + '</br>' + '<b>End:</b> ' + ': ' + data.end.format('YYYY-MM-DD') + '</br>' + data.description + '</div>';


                                $("body").append(tooltip);
                                $(this).mouseover(function (e) {
                                    $(this).css('z-index', 10000);
                                    $('.tooltiptopicevent').fadeIn('500');
                                    $('.tooltiptopicevent').fadeTo('10', 1.9);
                                }).mousemove(function (e) {
                                    $('.tooltiptopicevent').css('top', e.pageY + 10);
                                    $('.tooltiptopicevent').css('left', e.pageX + 20);
                                });


                            },
                            eventMouseout: function (data, event, view) {
                                $(this).css('z-index', 8);

                                $('.tooltiptopicevent').remove();

                            },
                            //eventColor: '#378006',
                            eventClick: function (calEvent, jsEvent, view) {

                                $('#myModal #eventTitle').text(calEvent.title);
                                var $description = $('<div/>');
                                var $link = "../EditBooking.aspx?BookingID=" + calEvent.id;
                                $('#ed').attr('href', $link);
                                $description.append($('<p/>').html('<b>Start:</b>' + calEvent.start.format('DD-MMM-YYYY HH:mm a')));
                                if (calEvent.end != null) {
                                    $description.append($('<p/>').html('<b>End:</b>' + calEvent.end.format('DD-MMM-YYYY HH:mm a')));
                                }
                                $description.append($('<p/>').html('<b>Source:</b>' + calEvent.description));
                                $('#myModal #pDetails').empty().html($description);
                                $('#myModal').modal('show');
                            },
                            eventAfterAllRender: function (view) {
                                var viewStartDate;
                                viewStartDate = 4 * 12 * (moment().weekday() - 1);
                                $('.fc-body .fc-time-area .fc-scrollpane').children('div').scrollLeft(4 * 12 * (moment().weekday() - 1) * view.options.slotWidth);
                            }
                        });
                    },
                    change: function (themeSystem) {
                        $('#calendar').fullCalendar('option', 'themeSystem', themeSystem);
                    }
                });
            }
        });

    }
});