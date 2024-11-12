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
                url: "/Admin/Organizations/ChangeStatus",
                data: { id: id, status: newStatus },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.status === 1) {
                        btn.text('Active')
                            .removeClass('badge-warning-lighten badge-danger-lighten')
                            .addClass('badge-success-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'Status Updated',
                            text: 'The organization is now active.',
                            confirmButtonText: 'OK'
                        });
                    } else if (response.status === -1) {
                        btn.text('Banned')
                            .removeClass('badge-warning-lighten badge-success-lighten')
                            .addClass('badge-danger-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'Status Updated',
                            text: 'The organization is banned.',
                            confirmButtonText: 'OK'
                        });
                    }

                    btn.data('status', newStatus);
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Update Failed',
                        text: 'An error occurred while changing the organization status. Please try again later.',
                        confirmButtonText: 'OK'
                    });
                }
            });
        });
    }
};

common.init();
