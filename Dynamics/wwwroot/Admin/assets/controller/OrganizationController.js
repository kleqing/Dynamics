var common = {
    init: function () {
        common.registerEvent();
    },
    registerEvent: function () {
        $('.btn-active').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');
            $.ajax({
                url: "/Admin/Organizations/BanOrganization",
                data: { id: id },
                datatype: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.isBanned == true) {
                        btn.text('Banned');
                        btn.removeClass('badge badge-success-lighten').addClass('badge badge-danger-lighten');
                    }
                    else {
                        btn.text('Active');
                        btn.removeClass('badge badge-danger-lighten').addClass('badge badge-success-lighten');
                    }
                }
            });
        });
    }
}
common.init();