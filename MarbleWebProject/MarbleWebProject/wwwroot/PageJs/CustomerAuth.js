(function () {
    function showAuthError(message) {
        var $alert = $('#customer-auth-alert');
        if (!$alert.length) return;
        if (!message) {
            $alert.addClass('d-none').text('');
            return;
        }
        $alert.removeClass('d-none').text(message);
    }

    function setCustomerLoggedIn(displayName) {
        if (displayName) {
            $('.store-account-menu-user .js-customer-display-name').text(displayName);
        }
        $('.js-customer-guest-only').addClass('d-none');
        $('.js-customer-guest-only').closest('li').addClass('d-none');
        $('.js-customer-logged-only').removeClass('d-none');
    }

    function setCustomerLoggedOut() {
        $('.js-customer-guest-only').removeClass('d-none');
        $('.js-customer-guest-only').closest('li').removeClass('d-none');
        $('.js-customer-logged-only').addClass('d-none');
        $('.store-account-menu-user .js-customer-display-name').text('');
    }

    function refreshCustomerHeader() {
        return $.ajax({
            type: 'GET',
            url: '/account/me'
        }).then(function (result) {
            if (result && result.displayName) {
                setCustomerLoggedIn(result.displayName);
            }
        }).catch(function () {
            setCustomerLoggedOut();
        });
    }

    function refreshCartAfterAuth() {
        if (typeof window.reloadCartFromServer === 'function') {
            window.reloadCartFromServer();
        }
    }

    $(document).on('submit', '#customer-login-form', function (e) {
        e.preventDefault();
        showAuthError('');

        $.ajax({
            type: 'POST',
            url: '/account/login',
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify({
                email: $('#singin-email').val(),
                password: $('#singin-password').val()
            }),
            success: function (result) {
                setCustomerLoggedIn(result.displayName || '');
                $('#signin-modal').modal('hide');
                refreshCartAfterAuth();
            },
            error: function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'Giriş başarısız.';
                showAuthError(msg);
            }
        });
    });

    $(document).on('submit', '#customer-register-form', function (e) {
        e.preventDefault();
        showAuthError('');

        var password = $('#register-password').val();
        var confirm = $('#register-password-confirm').val();
        if (password !== confirm) {
            showAuthError('Şifreler eşleşmiyor.');
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/account/register',
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify({
                email: $('#register-email').val(),
                password: password,
                firstName: $('#register-firstname').val(),
                lastName: $('#register-lastname').val(),
                phone: $('#register-phone').val(),
                customerType: 'Retail'
            }),
            success: function (result) {
                setCustomerLoggedIn(result.displayName || '');
                $('#signin-modal').modal('hide');
                refreshCartAfterAuth();
            },
            error: function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'Kayıt başarısız.';
                showAuthError(msg);
            }
        });
    });

    $(document).on('click', '.js-customer-logout', function (e) {
        e.preventDefault();
        $.ajax({
            type: 'POST',
            url: '/account/logout'
        }).always(function () {
            setCustomerLoggedOut();
            if (typeof window.reloadCartFromServer === 'function') {
                window.reloadCartFromServer();
            }
            window.location.href = '/';
        });
    });

    $(function () {
        refreshCustomerHeader();
    });
})();
