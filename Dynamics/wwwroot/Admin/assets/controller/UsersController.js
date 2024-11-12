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
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.isBanned === true) {
                        btn.text('Banned');
                        btn.removeClass('badge-success-lighten').addClass('badge-danger-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'User Banned',
                            text: 'The user has been banned!',
                            confirmButtonText: 'OK'
                        });
                    } else {
                        btn.text('Active');
                        btn.removeClass('badge-danger-lighten').addClass('badge-success-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'User Activated',
                            text: 'The user has been activated!',
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred while banning the user. Please try again later.',
                        confirmButtonText: 'OK'
                    });
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
                dataType: "json",
                type: "POST",
                success: function (response) {
                    if (response.isAdmin) {
                        btn.text('Admin');
                        btn.removeClass('badge-primary-lighten').addClass('badge-warning-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'Role Updated',
                            text: 'The user has been promoted to admin!',
                            confirmButtonText: 'OK'
                        });
                    } else {
                        btn.text('User');
                        btn.removeClass('badge-warning-lighten').addClass('badge-primary-lighten');

                        Swal.fire({
                            icon: 'success',
                            title: 'Role Updated',
                            text: 'The user role has been changed to user!',
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred while updating the user role. Please try again later.',
                        confirmButtonText: 'OK'
                    });
                }
            });
        });
    }
};

common.init();
