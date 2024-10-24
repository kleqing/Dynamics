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
                datatype: "json",
                type: "POST",
                success: function (response) {
                    if (response.status === 1) {
                        btn.text('Accepted')
                            .removeClass('badge-warning-lighten badge-danger-lighten')
                            .addClass('badge-success-lighten');
                    } else if (response.status === -1) {
                        btn.text('Canceled')
                            .removeClass('badge-warning-lighten badge-success-lighten')
                            .addClass('badge-danger-lighten');
                    }

                    // Cập nhật status mới vào data-status
                    btn.data('status', newStatus);
                }
            });
        });
    }

}
common.init();