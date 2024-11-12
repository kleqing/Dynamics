var common = {
    init: function () {
        common.registerEvent();
    },
    registerEvent: function () {
        $('.btn-ban').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');

            // Find badge and endDate elements using data attributes
            var badge = $('.badge[data-project-id="' + id + '"]');
            var endDate = $('.end-date[data-project-id="' + id + '"]');

            $.ajax({
                url: "/Admin/Projects/ChangeStatus",
                data: { id: id },
                dataType: "json",
                type: "POST",
                success: function (response) {

                    if (response.isBanned) {
                        btn.text('Banned')
                            .removeClass('badge badge-success-lighten')
                            .addClass('badge badge-danger-lighten');

                        badge.removeClass('bg-success bg-primary')
                            .addClass('bg-danger')
                            .text('Canceled');
                        badge.attr('data-project-status', '-1');

                        endDate.text('Banned').addClass('text-danger');

                        Swal.fire({
                            title: 'Project Banned',
                            text: 'The project has been successfully banned.',
                            icon: 'success',
                            confirmButtonText: 'OK'
                        });
                    } else {
                        btn.text('Active')
                            .removeClass('badge badge-danger-lighten')
                            .addClass('badge badge-success-lighten');

                        badge.removeClass('bg-danger bg-primary')
                            .addClass('bg-warning')
                            .text('Preparing');
                        badge.attr('data-project-status', '0');

                        endDate.text(response.endTime || 'No end date').removeClass('text-danger');

                        Swal.fire({
                            title: 'Project Activated',
                            text: 'The project status changed to active.',
                            icon: 'success',
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        title: 'Error',
                        text: 'An error occurred while updating the project status.',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            });
        });
    }
}

common.init();
