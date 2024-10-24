var common = {
    init: function () {
        common.Ban();
        common.Admin();
    },
    Ban: function () {
        $('.btn-ban').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');
            $.ajax({
                url: "/Admin/Users/BanUser",
                data: { id: id },
                datatype: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.isBanned == true) {
                        btn.text('Banned');
                        btn.removeClass('badge badge-success-lighten').addClass('badge badge-danger-lighten');
                    } else {
                        btn.text('Active');
                        btn.removeClass('badge badge-danger-lighten').addClass('badge badge-success-lighten');
                    }
                    location.reload();
                }
            });
        });
    },

    Admin: function () {
        $('.btn-admin').off('click').on('click', function (e) {
            e.preventDefault();
            var btn = $(this);
            var id = btn.data('id');
            $.ajax({
                url: "/Admin/Users/UserAsAdmin",
                data: { id: id },
                datatype: "json",
                type: "POST",
                success: function (response) {
                    if (response.isAdmin) {
                        btn.text('Admin');
                        btn.removeClass('badge badge-primary-lighten').addClass('badge badge-warning-lighten');
                    } else {
                        btn.text('User');
                        btn.removeClass('badge badge-warning-lighten').addClass('badge badge-primary-lighten');
                    }
                }
            });
        });
    }


}
common.init();
