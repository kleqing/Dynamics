var common = {
    init: function () {
        common.registerEvent();
    },
    registerEvent: function () {
        $('.btn-ban').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');

            // Find badge by data-project-id
            var badge = $('.badge[data-project-id="' + id + '"]');
            var endDate = $('.end-date[data-project-id="' + id + '"]');

            $.ajax({
                url: "/Admin/Projects/ChangeStatus",
                data: { id: id },
                datatype: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.isBanned) {
                        btn.text('Banned');
                        btn.removeClass('badge badge-success-lighten').addClass('badge badge-danger-lighten');
                        badge.removeClass('bg-success bg-primary').addClass('bg-danger').text('Canceled');
                        badge.attr('data-project-status', '-1');

                        endDate.text('Banned').addClass('text-danger');
                    } else {
                        btn.text('Active');
                        btn.removeClass('badge badge-danger-lighten').addClass('badge badge-success-lighten');
                        badge.removeClass('bg-danger bg-warning').addClass('bg-primary').text('On-going');
                        badge.attr('data-project-status', '1');

                        endDate.text(response.endTime).removeClass('text-danger');
                    }
                }
            });
        });
    }
}

common.init();
