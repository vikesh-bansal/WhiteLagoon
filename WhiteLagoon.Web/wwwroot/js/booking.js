$(document).ready(function () {
    var urlParams = new URLSearchParams(window.location.search);
    var searchStatus = urlParams.get('status')
    loadBookings(searchStatus);
});

function loadBookings(searchStatus) {
    datable = $('#tblBookings').DataTable({
        ajax: 'Booking/GetAll?status=' + searchStatus,
        columns: [
            { data: 'id', width: '5%' },
            { data: 'name', width: '15%' },
            { data: 'phone', width: '10%' },
            { data: 'email', width: '15%' },
            { data: 'status', width: '10%' },
            { data: 'checkInDate', width: '10%' },
            { data: 'nights', width: '10%' },
            {data:'totalcost', render:$.fn.dataTable.render.number(',','.',2),"width":"10%"},
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group">
                    <a href="/booking/bookingDetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                    <i class="bi bi-pencil-square"></i> Details
                    </a>
                    </div>`
                }
            }

        ]
    });
}