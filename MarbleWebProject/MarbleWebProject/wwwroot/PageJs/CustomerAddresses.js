$(function () {
    if (!$('#account-addr-list').length) return;

    var addresses = [];
    var selectedId = null;
    var currentInvoiceType = 'Individual';

    function findAddressById(id) {
        return addresses.find(function (a) { return String(a.id) === String(id); }) || null;
    }

    function formatAddressLine(addr) {
        if (!addr) return '';
        if (addr.displayLine) return addr.displayLine;
        return [addr.addressLine1, addr.townName, addr.cityName].filter(Boolean).join(' / ');
    }

    function setInvoiceType(type) {
        currentInvoiceType = type === 'Corporate' ? 'Corporate' : 'Individual';
        $('.checkout-invoice-btn').removeClass('active');
        $('.checkout-invoice-btn[data-invoice="' + currentInvoiceType + '"]').addClass('active');
        var isCorp = currentInvoiceType === 'Corporate';
        $('#checkout-addr-corporate-fields').toggle(isCorp);
        $('#checkout-addr-tax-number, #checkout-addr-tax-office, #checkout-addr-company').prop('required', isCorp);
    }

    function loadCountries() {
        return $.getJSON('/account/locations/countries').then(function (items) {
            var $country = $('#checkout-addr-country').empty().append('<option value="">Seçiniz</option>');
            (items || []).forEach(function (item) {
                $country.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        }).catch(function () {
            $('#checkout-addr-country').html('<option value="1">Türkiye</option>');
        });
    }

    function loadCities(countryId) {
        var $city = $('#checkout-addr-city').empty().append('<option value="">Seçiniz</option>');
        $('#checkout-addr-town').empty().append('<option value="">Seçiniz</option>');
        if (!countryId) return $.when();
        return $.getJSON('/account/locations/cities', { countryId: countryId }).then(function (items) {
            (items || []).forEach(function (item) {
                $city.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        });
    }

    function loadTowns(cityId) {
        var $town = $('#checkout-addr-town').empty().append('<option value="">Seçiniz</option>');
        if (!cityId) return $.when();
        return $.getJSON('/account/locations/towns', { cityId: cityId }).then(function (items) {
            (items || []).forEach(function (item) {
                $town.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        });
    }

    function renderList() {
        var $list = $('#account-addr-list').empty();
        var $empty = $('#account-addr-empty');

        if (!addresses.length) {
            $empty.removeClass('d-none');
            return;
        }

        $empty.addClass('d-none');

        if (!selectedId) {
            var defaultAddr = addresses.find(function (a) { return a.isDefaultShipping; }) || addresses[0];
            selectedId = defaultAddr ? defaultAddr.id : null;
        }

        addresses.forEach(function (addr) {
            var id = addr.id;
            var checked = selectedId && String(selectedId) === String(id);
            var name = [addr.contactFirstName, addr.contactLastName].filter(Boolean).join(' ');
            var line = formatAddressLine(addr);
            var badges = [];
            if (addr.isDefaultShipping) badges.push('Teslimat');
            if (addr.isDefaultBilling) badges.push('Fatura');
            var badgeHtml = badges.length ? ' <span class="badge badge-light ml-1">' + badges.join(' · ') + '</span>' : '';

            var $card = $('<div class="checkout-addr-card"></div>');
            if (checked) $card.addClass('is-selected');

            $card.append(
                '<div class="checkout-addr-card-actions">' +
                '<button type="button" class="checkout-addr-icon-btn checkout-addr-edit-btn" data-id="' + id + '" title="Düzenle" aria-label="Düzenle"><i class="icon-edit"></i></button>' +
                '<button type="button" class="checkout-addr-icon-btn checkout-addr-delete-btn" data-id="' + id + '" title="Sil" aria-label="Sil"><i class="icon-close"></i></button>' +
                '</div>' +
                '<label class="checkout-addr-card-select">' +
                '<input type="radio" name="account-addr-select" value="' + id + '"' + (checked ? ' checked' : '') + ' />' +
                '<span class="checkout-addr-card-radio" aria-hidden="true"></span>' +
                '</label>' +
                '<div class="checkout-addr-card-body">' +
                '<strong class="checkout-addr-card-title">' + (addr.label || 'Adres') + badgeHtml + '</strong>' +
                '<span class="checkout-addr-card-name">' + name + (addr.contactPhone ? ' · ' + addr.contactPhone : '') + '</span>' +
                '<span class="checkout-addr-card-line">' + line + '</span>' +
                '</div>'
            );

            $list.append($card);
        });
    }

    function resetAddrForm() {
        $('#checkout-addr-id').val('0');
        $('#checkout-addr-form')[0].reset();
        $('#checkout-addr-city, #checkout-addr-town').empty().append('<option value="">Seçiniz</option>');
        setInvoiceType('Individual');
    }

    function fillAddrForm(addr) {
        $('#checkout-addr-id').val(addr.id || 0);
        $('#checkout-addr-label').val(addr.label || '');
        $('#checkout-addr-firstname').val(addr.contactFirstName || '');
        $('#checkout-addr-lastname').val(addr.contactLastName || '');
        $('#checkout-addr-phone').val(addr.contactPhone || '');
        $('#checkout-addr-line1').val(addr.addressLine1 || '');

        var isCorp = (addr.label || '').toLowerCase().indexOf('kurumsal') >= 0;
        setInvoiceType(isCorp ? 'Corporate' : 'Individual');
        $('#checkout-addr-tax-number').val(addr.taxNumber || '');
        $('#checkout-addr-tax-office').val(addr.taxOffice || '');
        $('#checkout-addr-company').val(addr.companyName || '');

        $('#checkout-addr-country').val(addr.countryId || '');
        loadCities(addr.countryId).then(function () {
            $('#checkout-addr-city').val(addr.cityId || '');
            return loadTowns(addr.cityId);
        }).then(function () {
            $('#checkout-addr-town').val(addr.townId || '');
        });
    }

    function openAddrFormModal(addr) {
        $('#checkout-addr-form-title').text(addr ? 'Adres Düzenle' : 'Yeni Adres Ekle');
        resetAddrForm();

        loadCountries().then(function () {
            if (!addr) {
                var tr = $('#checkout-addr-country option').filter(function () {
                    return $(this).text().toLowerCase().indexOf('türk') >= 0;
                }).first().val();
                if (tr) {
                    $('#checkout-addr-country').val(tr);
                    return loadCities(tr);
                }
            }
            return $.when();
        }).then(function () {
            if (addr) fillAddrForm(addr);
        });

        $('#checkout-address-form-modal').modal('show');
    }

    function validateAddrForm() {
        var form = document.getElementById('checkout-addr-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return false;
        }
        if (currentInvoiceType === 'Corporate') {
            if (!$('#checkout-addr-tax-number').val() || !$('#checkout-addr-tax-office').val() || !$('#checkout-addr-company').val()) {
                alert('Kurumsal fatura için vergi bilgileri zorunludur.');
                return false;
            }
        }
        return true;
    }

    function collectAddrFormPayload() {
        var id = parseInt($('#checkout-addr-id').val(), 10) || 0;
        var label = $('#checkout-addr-label').val();
        if (currentInvoiceType === 'Corporate' && label.toLowerCase().indexOf('kurumsal') < 0) {
            label = 'Kurumsal - ' + label;
        }

        return {
            id: id,
            label: label,
            contactFirstName: $('#checkout-addr-firstname').val(),
            contactLastName: $('#checkout-addr-lastname').val(),
            contactPhone: $('#checkout-addr-phone').val(),
            countryId: parseInt($('#checkout-addr-country').val(), 10) || 0,
            cityId: parseInt($('#checkout-addr-city').val(), 10) || null,
            townId: parseInt($('#checkout-addr-town').val(), 10) || null,
            addressLine1: $('#checkout-addr-line1').val(),
            addressLine2: '',
            isDefaultBilling: addresses.length === 0,
            isDefaultShipping: addresses.length === 0
        };
    }

    function loadAddresses() {
        return $.getJSON('/account/addresses/list').then(function (items) {
            addresses = items || [];
            if (selectedId && !findAddressById(selectedId)) {
                selectedId = null;
            }
            renderList();
        }).fail(function () {
            window.location.href = '/';
        });
    }

    $('#account-addr-new-btn').on('click', function () {
        openAddrFormModal(null);
    });

    $('#account-addr-list').on('click', '.checkout-addr-edit-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var addr = findAddressById($(this).data('id'));
        if (addr) openAddrFormModal(addr);
    });

    $('#account-addr-list').on('click', '.checkout-addr-delete-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('id');
        if (!confirm('Bu adres silinsin mi?')) return;

        $.ajax({
            type: 'DELETE',
            url: '/account/addresses/' + id,
            success: function () {
                if (String(selectedId) === String(id)) selectedId = null;
                loadAddresses();
            },
            error: function (xhr) {
                alert((xhr.responseJSON && xhr.responseJSON.message) || 'Silinemedi.');
            }
        });
    });

    $('#account-addr-list').on('change', 'input[name="account-addr-select"]', function () {
        selectedId = parseInt($(this).val(), 10);
        $('#account-addr-list .checkout-addr-card').removeClass('is-selected');
        $(this).closest('.checkout-addr-card').addClass('is-selected');
    });

    $('#account-addr-list').on('click', '.checkout-addr-card-select', function (e) {
        if ($(e.target).is('input')) return;
        var $radio = $(this).find('input[type="radio"]');
        $radio.prop('checked', true).trigger('change');
    });

    $('.checkout-invoice-btn').on('click', function () {
        setInvoiceType($(this).data('invoice'));
    });

    $('#checkout-addr-country').on('change', function () { loadCities($(this).val()); });
    $('#checkout-addr-city').on('change', function () { loadTowns($(this).val()); });

    $('#checkout-addr-form-save-btn').on('click', function () {
        if (!validateAddrForm()) return;

        var payload = collectAddrFormPayload();
        var $btn = $(this).prop('disabled', true);

        $.ajax({
            type: 'POST',
            url: '/account/addresses/save',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (saved) {
                if (saved && saved.id) selectedId = saved.id;
                $('#checkout-address-form-modal').modal('hide');
                loadAddresses();
            },
            error: function (xhr) {
                alert((xhr.responseJSON && xhr.responseJSON.message) || 'Kaydedilemedi.');
            },
            complete: function () {
                $btn.prop('disabled', false);
            }
        });
    });

    loadAddresses();
});
