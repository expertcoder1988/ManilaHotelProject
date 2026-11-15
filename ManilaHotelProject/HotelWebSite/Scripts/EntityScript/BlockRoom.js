$(document).ready(function () {
    if ($('#AvoidReload').val() != "True") {
        // alert("g");
        //  loadRooms();
    }

    $(function () {
        try {
            $.ajax({
                url: '/Property/Properties',
                type: 'GET',
                dataType: 'json',
                contentType: 'application/json',
                success: function (data) {
                    // console.log(data);
                    $('#AvoidReload').val("True");
                    //  alert("f");
                    //var ddlPropertyTypes = $("[id*=PropertyTypes]");
                    $('#Properties').empty().append('<option selected="selected" value="0">Please select</option>');
                    //if (data.length > 0) {

                    $.each(data, function (i, item) {
                        $('#Properties').append($('<option></option>').attr('value', item.Id).text(item.PropertyName));
                    });
                    //  $("select#Properties").prop('selectedIndex', 1);
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


    $('#rooms').change(function () {
        //  LoadMinPrice($('#rooms').val());
      //  loadRoomNums();
        loadNumberOfRooms();
        LoadBlockedRooms();
    });

    $('#Properties').change(function () {
        loadRooms();

    });

    function loadRooms() {
        try {
            var propertyID = new Object();
            propertyID = $('#Properties').val();

            $.ajax({
                url: '/Property/LoadpropertyCategories',
                type: 'GET',
                dataType: 'json',
                contentType: 'application/json',
                data: 'propertyID=' + propertyID,
                success: function (data) {
                    //console.log(data);
                    $('#rooms').empty().append('<option selected="selected" value="0">Please Select Room</option>');
                    //if (data.length > 0) {

                    $.each(data, function (i, item) {
                        $('#rooms').append($('<option></option>').attr('value', item.CateId).text(item.Category));
                    });
                },
                error: function (err) {
                    alert(err.status + " : " + err.statusText);
                }

            });
        } catch (e) {
            alert(e.status + " : " + e.statusText);
            window.location.href = '/Property/RateSlab';
        }
    }

    function loadNumberOfRooms() {
        try {
            var roomID = new Object();
            roomID = $('#rooms').val();

            $.ajax({
                url: '/Property/loadNumberOfRooms',
                type: 'GET',
                dataType: 'json',
                contentType: 'application/json',
                data: 'CateId=' + roomID,
                success: function (data) {
                   // console.log(data);
                    $('#TotalRooms').val(data);
                    $('#RoomNum').val(data);
                },
                error: function (err) {
                    alert(err.status + " : " + err.statusText);
                }

            });
        } catch (e) {
            alert(e.status + " : " + e.statusText);
            window.location.href = '/Property/RateSlab';
        }
    }

    function loadRoomNums() {
        try {
            var roomID = new Object();
            roomID = $('#rooms').val();

            $.ajax({
                url: '/Property/loadRoomNums',
                type: 'GET',
                dataType: 'json',
                contentType: 'application/json',
                data: 'CateId=' + roomID,
                success: function (data) {
                   // console.log(data);
                    $('#RoomNum').empty().append('<option selected="selected" value="0">Please Select Room</option>');
                    //if (data.length > 0) {

                    $.each(data, function (i, item) {
                        $('#RoomNum').append($('<option></option>').attr('value', item.RoomNumber).text(item.RoomNumber));
                    });
                },
                error: function (err) {
                    alert(err.status + " : " + err.statusText);
                }

            });
        } catch (e) {
            alert(e.status + " : " + e.statusText);
            window.location.href = '/Property/BlockRoom';
        }
    }

    $("table").on("click", ".Edit", function (ev) {
        var id = this.id;
        //alert(id);
        //window.location.href = '/Property/Features?id=' + id;

        //$('#RateSlabId').val(id);
        UnBlockRooms(id);
    })

    $("table").on("click", ".Delete", function (ev) {
        // ev.preventDefault();
        var id = this.id;
        if (confirm("Are you sure to delete this rate?")) {
            DeleteRateSlab(id);
        }
        else {
            return false;
        }
    });


    function UnBlockRooms(id) {

        try {
            $.ajax({
                url: '/Property/UnBlockRooms/',
                type: 'POST',
                dataType: 'json',
                data: 'Id=' + id,
                //contentType: 'application/json',
                success: function (data) {
                    alert("Selected Rooms Unblocked successfully.");
                    LoadBlockedRooms();
                },
                error: function (err) {
                    alert("Selected Rooms Unblocked successfully.");
                    LoadBlockedRooms();
                    //  alert(err.status + " : " + err.statusText);
                }

            });
        } catch (e) {
            window.location.href = '/Property/Rooms';
        }
    }

    $('#btnSave').click(function () {
        SaveData();
    })


    function SaveData() {
        try {

            if ($("#Properties")[0].selectedIndex == 0) {
                alert("Please select a Property");
                return;
            }

            if ($("#rooms")[0].selectedIndex == 0) {
                alert("Please select a room category");
                return;
            }

            if ($("#RoomNum").val() == "0") {
                alert("Please enter valid number of rooms to blocked.");
                return;
            }

            if ($("#RoomNum").val() == "") {
                alert("Please enter valid number of rooms to blocked.");
                return;
            }

            if (parseInt($("#RoomNum").val()) <= 0) {
                alert("Please enter valid number of rooms to blocked.");
                return;
            }

            if (parseInt($('#TotalRooms').val()) < parseInt($('#RoomNum').val())) {
                alert("Number of Blocked rooms must be equal to or less then available rooms.");
                return;
            }

            if ($('#StartDate').val() == "") {
                alert("Please select start date");
                return;
            }

            if ($('#EndDate').val() == "") {
                alert("Please select end date");
                return;
            }

            if ($('#StartDate').val() == $('#EndDate').val()) {
                alert("Please select valid date range");
                return;
            }

            var propertyID = new Object();
            propertyID = $('#Properties').val();
            var sdate = moment.utc($('#StartDate').val());
            var edate = moment.utc($('#EndDate').val());
            var br = new Object();
            br.Id = 0;
            br.CateId = $('#rooms').val();
            br.RoomNum = $('#RoomNum').val();
            br.PropertyId = propertyID;
            br.StartDate = sdate.format('YYYY-MM-DD');
            br.EndDate = edate.format('YYYY-MM-DD');


            //  alert(propertyID);

            $.ajax({
                url: '/Property/BlockRooms',
                type: 'POST',
                dataType: 'json',
                data: JSON.stringify(br),
                contentType: 'application/json',
                success: function () {
                    alert("New Rate Plan Succesfully Added.");
                    // window.location.href = '/Property/Rooms';
                    loadRoomRates();
                    ClearAll();
                },
                error: function (err) {
                    // alert(err.status + " : " + err.statusText);
                    //loadRooms(); //window.location.href = '/Property/Rooms';
                    LoadBlockedRooms();
                },
            });
        } catch (e) {
            //alert(e);
            window.location.href = '/Property/RateSlab';

        }
    }

    function LoadBlockedRooms() {

        try {
            var roomID = new Object();
            roomID = $('#rooms').val();
            $.ajax({
                url: '/Property/LoadBlockedRooms/',
                type: 'GET',
                dataType: 'json',
                data: 'RoomCode=' + roomID,
                contentType: 'application/json',
                success: function (data) {
                    //console.log(data);
                    $("#RatePlan").empty();
                    var trHTML = '';
                    trHTML = '<tr><th style=display:none>Room Caterogy ID</th><th>Room Category</th><th>Room Num</th><th>Slab Start Date</th><th>Slab End Date</th><th></th><th></th></tr > ';

                    if (data.length > 0) {
                        $.each(data, function (i, item) {
                            var sDate = moment(item.StartDate); //new Date(parseInt(item.StartDate.substr(6)));
                            var eDate = moment(item.EndDate);
                            var desc = item.Days;
                            //console.log(desc);
                            if (desc == null) {
                                desc = "";
                            }
                            //  console.log(myDate.fomat('DD-MM-YYYY'));
                            trHTML += '<tr><td style=display:none>' + item.Id + '</td><td>' + item.Category + '</td>' +
                                "<td>" + item.RoomNumber + "</td>" +
                                "<td>" + sDate.format('DD-MM-YYYY') + "</td>" +
                                "<td>" + eDate.format('DD-MM-YYYY') + "</td>" +
                                "<td><input type='button' ID='" + item.Id + "' class='Edit btn btn-primary' value='Unblock' /></td >" +
                                "<td><input type='button' ID='" + item.Id + "' style='display:none' class='Delete btn btn-danger' value='Delete' /></td ></tr > ";
                        });
                        $('#RatePlan').append(trHTML);
                    }
                },
                error: function (err) {
                    //  alert(err.status + " : " + err.statusText);
                }
            });
        } catch (e) {
            window.location.href = '/Property/RateSlab';
        }
    }


    function ClearAll() {
        $('#NormalPrice').val() = "";
        $('#Discount').val() = "";
        $('#StartDate').val() = "";
        $('#EndDate').val() = "";
        $('#RateSlabId').val() = -1;
    }
});