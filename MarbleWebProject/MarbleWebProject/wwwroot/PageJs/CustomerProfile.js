$(function () {
    var $form = $('#profile-form');
    if (!$form.length) return;

    var $message = $('#profile-message');
    var monthNames = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

    function showMessage(text, isError) {
        $message
            .removeClass('d-none store-account-message-error store-account-message-success')
            .addClass(isError ? 'store-account-message-error' : 'store-account-message-success')
            .text(text);
    }

    function normalizePhone(phone) {
        if (!phone) return '';
        return String(phone).replace(/\D/g, '').replace(/^90/, '');
    }

    function initBirthDropdowns() {
        var $day = $('#profile-birth-day');
        var $month = $('#profile-birth-month');
        var $year = $('#profile-birth-year');
        var currentYear = new Date().getFullYear();

        for (var d = 1; d <= 31; d++) {
            $day.append('<option value="' + d + '">' + d + '</option>');
        }

        for (var m = 1; m <= 12; m++) {
            $month.append('<option value="' + m + '">' + monthNames[m - 1] + '</option>');
        }

        for (var y = currentYear; y >= currentYear - 100; y--) {
            $year.append('<option value="' + y + '">' + y + '</option>');
        }
    }

    function setBirthDate(value) {
        $('#profile-birth-day, #profile-birth-month, #profile-birth-year').val('');
        if (!value) return;

        var date = new Date(value);
        if (isNaN(date.getTime())) return;

        $('#profile-birth-day').val(String(date.getUTCDate()));
        $('#profile-birth-month').val(String(date.getUTCMonth() + 1));
        $('#profile-birth-year').val(String(date.getUTCFullYear()));
    }

    function collectBirthDate() {
        var day = parseInt($('#profile-birth-day').val(), 10);
        var month = parseInt($('#profile-birth-month').val(), 10);
        var year = parseInt($('#profile-birth-year').val(), 10);
        if (!day || !month || !year) return null;

        var date = new Date(Date.UTC(year, month - 1, day));
        if (date.getUTCFullYear() !== year || date.getUTCMonth() !== month - 1 || date.getUTCDate() !== day) {
            return null;
        }

        var mm = String(month).padStart(2, '0');
        var dd = String(day).padStart(2, '0');
        return year + '-' + mm + '-' + dd;
    }

    function fillForm(customer) {
        $('#profile-firstname').val(customer.firstName || '');
        $('#profile-lastname').val(customer.lastName || '');
        $('#profile-email').val(customer.email || '');
        $('#profile-phone').val(normalizePhone(customer.phone));
        $('#profile-company').val(customer.companyName || '');
        $('#profile-taxoffice').val(customer.taxOffice || '');
        $('#profile-taxnumber').val(customer.taxNumber || '');
        setBirthDate(customer.birthDate);

        if (customer.customerType && customer.customerType !== 'Retail') {
            $('#profile-corporate-fields').removeClass('d-none');
        }
    }

    function loadProfile() {
        return $.getJSON('/account/profile/data').done(fillForm).fail(function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/';
                return;
            }
            showMessage((xhr.responseJSON && xhr.responseJSON.message) || 'Profil yüklenemedi.', true);
        });
    }

    $form.on('submit', function (e) {
        e.preventDefault();

        var birthDate = collectBirthDate();
        var hasPartialBirth = $('#profile-birth-day').val() || $('#profile-birth-month').val() || $('#profile-birth-year').val();
        if (hasPartialBirth && !birthDate) {
            showMessage('Geçerli bir doğum tarihi seçin.', true);
            return;
        }

        var payload = {
            firstName: $('#profile-firstname').val(),
            lastName: $('#profile-lastname').val(),
            phone: $('#profile-phone').val(),
            companyName: $('#profile-company').val(),
            taxOffice: $('#profile-taxoffice').val(),
            taxNumber: $('#profile-taxnumber').val(),
            birthDate: birthDate
        };

        $('#profile-save-btn').prop('disabled', true);

        $.ajax({
            url: '/account/profile',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        }).done(function (res) {
            if (res && res.displayName) {
                $('.js-customer-display-name').text(res.displayName);
                $('.store-account-sidebar-user').text(res.displayName);
            }
            showMessage('Bilgileriniz güncellendi.', false);
        }).fail(function (xhr) {
            showMessage((xhr.responseJSON && xhr.responseJSON.message) || 'Güncelleme başarısız.', true);
        }).always(function () {
            $('#profile-save-btn').prop('disabled', false);
        });
    });

    initBirthDropdowns();
    loadProfile();
});
