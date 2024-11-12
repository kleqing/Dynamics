var common = {
    init: function () {
        common.registerEvent();
    },
    registerEvent: function () {
        $('.btn-status').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');
            var currentStatus = parseInt(btn.data('status'));

            var newStatus = currentStatus === 1 ? -1 : 1;

            $.ajax({
                url: "/Admin/Requests/ChangeStatus",
                data: { id: id, status: newStatus },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.status === 1) {
                        btn.text('Accepted')
                            .removeClass('badge-warning-lighten badge-danger-lighten')
                            .addClass('badge-success-lighten');

                        Swal.fire({
                            title: 'Request Accepted',
                            text: 'The request has been successfully accepted.',
                            icon: 'success',
                            confirmButtonText: 'OK'
                        });
                    } else if (response.status === -1) {
                        btn.text('Canceled')
                            .removeClass('badge-warning-lighten badge-success-lighten')
                            .addClass('badge-danger-lighten');

                        Swal.fire({
                            title: 'Request Canceled',
                            text: 'The request has been successfully canceled.',
                            icon: 'success',
                            confirmButtonText: 'OK'
                        });
                    }

                    btn.data('status', newStatus);
                },
                error: function () {
                    Swal.fire({
                        title: 'Error',
                        text: 'An error occurred while updating the request status.',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            });
        });
    }
}

common.init();
