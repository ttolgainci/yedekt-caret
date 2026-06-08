$(function () {
    var $form = $('#checkout-form');
    if ($form.length === 0) return;

    var prefill = window.checkoutPrefill || {};
    var i18n = window.checkoutI18n || {};

    function tx(key) {
        return (i18n[key] !== undefined && i18n[key] !== null) ? i18n[key] : key;
    }

    function showCheckoutNotify(message, type) {
        if (window.alertify && alertify.notify) {
            alertify.notify(message, type || 'success', 2.5);
            return;
        }
        alert(message);
    }
    var savedAddresses = [];
    var selectedAddress = null;
    var selectedBillingAddress = null;
    var listSelectedId = null;
    var localAddrSeq = 1;
    var currentInvoiceType = 'Individual';
    var addrFormMode = 'individual'; // individual | corporate
    var saveDraftTimer = null;
    var restoringDraft = false;

    function nextLocalAddressId() {
        return 'local-' + (localAddrSeq++);
    }

    function findAddressById(id) {
        if (id === null || id === undefined || id === '') return null;
        return savedAddresses.find(function (a) { return String(a.id) === String(id); }) || null;
    }

    function getGuestEmail() {
        return ($('#co-guest-email').val() || $('#co-email').val() || '').trim();
    }

    function getGuestFirstName() {
        return ($('#co-guest-firstname').val() || '').trim();
    }

    function getGuestLastName() {
        return ($('#co-guest-lastname').val() || '').trim();
    }

    function prefillGuestIntoAddrForm() {
        if (prefill.isLoggedIn) return;
        if (!$('#checkout-addr-firstname').val()) {
            $('#checkout-addr-firstname').val(getGuestFirstName());
        }
        if (!$('#checkout-addr-lastname').val()) {
            $('#checkout-addr-lastname').val(getGuestLastName());
        }
    }

    function validateGuestInfo() {
        if (prefill.isLoggedIn) return true;
        var email = getGuestEmail();
        var first = getGuestFirstName();
        var last = getGuestLastName();
        if (!email) {
            alert(tx('pleaseEnterEmail'));
            $('#co-collapse-guest').collapse('show');
            $('#co-guest-email').focus();
            return false;
        }
        if (!first || !last) {
            alert(tx('pleaseEnterFirstLastName'));
            $('#co-collapse-guest').collapse('show');
            return false;
        }
        return true;
    }

    function maskPhone(phone) {
        if (!phone) return '';
        var digits = String(phone).replace(/\D/g, '');
        if (digits.length < 4) return phone;
        return '(' + digits.substring(0, 3) + ') *****' + digits.slice(-2);
    }

    function formatAddressLine(addr) {
        if (!addr) return '';
        var parts = [addr.addressLine1, addr.townName, addr.cityName].filter(Boolean);
        return parts.join(' / ');
    }

    function setInvoiceType(type) {
        currentInvoiceType = type === 'Corporate' ? 'Corporate' : 'Individual';
        $('#co-invoice-type').val(currentInvoiceType);
        $('.checkout-invoice-btn').removeClass('active');
        $('.checkout-invoice-btn[data-invoice="' + currentInvoiceType + '"]').addClass('active');
        var isCorp = currentInvoiceType === 'Corporate';
        $('#checkout-addr-corporate-fields').toggle(isCorp);
        $('#checkout-addr-tax-number, #checkout-addr-tax-office, #checkout-addr-company').prop('required', isCorp);
    }

    function isLocalAddressId(id) {
        return String(id || '').indexOf('local-') === 0;
    }

    function parseAddressFormId() {
        var raw = ($('#checkout-addr-id').val() || '').toString().trim();
        if (!raw || raw === '0') return 0;
        if (isLocalAddressId(raw)) return raw;
        var n = parseInt(raw, 10);
        return isNaN(n) ? 0 : n;
    }

    function renderAddressCollection($container, options) {
        if (!$container || !$container.length) return;
        options = options || {};
        var radioName = options.radioName || 'checkout-addr-select';
        var emptyText = options.emptyText || tx('noAddressYet');
        var selected = options.selected || selectedAddress;
        var cardMode = options.cardMode !== false;

        $container.empty();
        if (!savedAddresses.length) {
            $container.append('<p class="text-muted small mb-0">' + emptyText + '</p>');
            return;
        }

        savedAddresses.forEach(function (addr) {
            var id = addr.id;
            var checked = selected && String(selected.id) === String(id);
            if (options.useListSelection) {
                checked = listSelectedId === id || (!listSelectedId && checked);
                if (checked) listSelectedId = id;
            }

            var name = [addr.contactFirstName, addr.contactLastName].filter(Boolean).join(' ');
            var line = addr.displayLine || formatAddressLine(addr);
            var isCorp = (addr.invoiceMeta && addr.invoiceMeta.invoiceType === 'Corporate')
                || (addr.label || '').toLowerCase().indexOf(tx('corporateLabelMatch')) >= 0;
            var corpBadge = isCorp ? ' <span class="badge badge-secondary ml-1">' + tx('corporate') + '</span>' : '';

            if (!cardMode) {
                var $item = $('<label class="checkout-addr-list-item"></label>');
                $item.append(
                    '<input type="radio" name="' + radioName + '" value="' + id + '"' + (checked ? ' checked' : '') + ' />' +
                    '<div class="checkout-addr-list-item-body">' +
                    '<div class="d-flex justify-content-between align-items-start">' +
                    '<strong>' + (addr.label || tx('address')) + corpBadge + '</strong>' +
                    '<div class="checkout-addr-list-item-actions">' +
                    '<button type="button" class="checkout-addr-icon-btn checkout-addr-edit-btn" data-id="' + id + '" title="' + tx('edit') + '" aria-label="' + tx('edit') + '"><i class="icon-edit"></i></button>' +
                    '<button type="button" class="checkout-addr-icon-btn checkout-addr-delete-btn" data-id="' + id + '" title="' + tx('delete') + '" aria-label="' + tx('delete') + '"><i class="icon-close"></i></button>' +
                    '</div>' +
                    '</div>' +
                    '<span class="small text-muted d-block">' + name + (addr.contactPhone ? ' · ' + maskPhone(addr.contactPhone) : '') + '</span>' +
                    '<span class="small d-block">' + line + '</span>' +
                    '</div>'
                );
                $container.append($item);
                return;
            }

            var $card = $('<div class="checkout-addr-card"></div>');
            if (checked) $card.addClass('is-selected');
            $card.append(
                '<div class="checkout-addr-card-actions">' +
                '<button type="button" class="checkout-addr-icon-btn checkout-addr-edit-btn" data-id="' + id + '" title="' + tx('edit') + '" aria-label="' + tx('edit') + '"><i class="icon-edit"></i></button>' +
                '<button type="button" class="checkout-addr-icon-btn checkout-addr-delete-btn" data-id="' + id + '" title="' + tx('delete') + '" aria-label="' + tx('delete') + '"><i class="icon-close"></i></button>' +
                '</div>' +
                '<label class="checkout-addr-card-select">' +
                '<input type="radio" name="' + radioName + '" value="' + id + '"' + (checked ? ' checked' : '') + ' />' +
                '<span class="checkout-addr-card-radio" aria-hidden="true"></span>' +
                '</label>' +
                '<div class="checkout-addr-card-body">' +
                '<strong class="checkout-addr-card-title">' + (addr.label || tx('address')) + corpBadge + '</strong>' +
                '<span class="checkout-addr-card-name">' + name + '</span>' +
                '<span class="checkout-addr-card-line">' + line + '</span>' +
                '</div>'
            );
            $container.append($card);
        });
    }

    function renderCheckoutAddressPicker() {
        renderAddressCollection($('#co-address-picker'), {
            radioName: 'checkout-addr-select',
            emptyText: tx('noAddressYetAdd'),
            selected: selectedAddress
        });
    }

    function renderBillingAddressPicker() {
        renderAddressCollection($('#co-billing-address-picker'), {
            radioName: 'checkout-billing-addr-select',
            emptyText: tx('selectBillingAddress'),
            selected: selectedBillingAddress
        });
    }

    function renderAddressList() {
        renderAddressCollection($('#checkout-addr-list'), {
            radioName: 'checkout-addr-pick',
            emptyText: tx('noSavedAddress'),
            useListSelection: true,
            cardMode: false
        });
    }

    var checkoutSubtotal = Number(prefill.subtotal || 0);

    function formatShippingMoney(amount) {
        var num = Number(amount || 0);
        return num.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (prefill.currency || 'TL');
    }

    function updateCheckoutTotals(shippingPrice) {
        var ship = Number(shippingPrice || 0);
        var grand = checkoutSubtotal + ship;
        $('#co-summary-shipping').html(formatShippingMoney(ship));
        $('#co-summary-grand').text(formatShippingMoney(grand));
    }

    function selectCarrierOption($input) {
        if (!$input || !$input.length) return;
        $('#co-carrier-id').val($input.val());
        updateCheckoutTotals($input.data('price'));
        scheduleSaveDraft();
    }

    function renderCarrierOptions(options, selectedId) {
        var $box = $('#co-carrier-options').empty();
        if (!options || !options.length) {
            $box.html('<p class="text-muted small mb-0">' + tx('noCarrierOptions') + '</p>');
            return;
        }
        options.forEach(function (opt) {
            var id = opt.carrierId || opt.CarrierId;
            var name = opt.carrierName || opt.CarrierName || ('Kargo #' + id);
            var price = opt.shippingPrice != null ? opt.shippingPrice : (opt.ShippingPrice != null ? opt.ShippingPrice : 0);
            var checked = String(id) === String(selectedId);
            $box.append(
                '<label class="checkout-carrier-option' + (checked ? ' is-selected' : '') + '">' +
                '<input type="radio" name="co-carrier" value="' + id + '" data-price="' + price + '"' + (checked ? ' checked' : '') + ' />' +
                '<span class="checkout-carrier-body">' +
                '<strong class="checkout-carrier-name">' + name + '</strong>' +
                '<span class="checkout-carrier-price">' + formatShippingMoney(price) + '</span>' +
                '</span></label>'
            );
        });
    }

    function refreshShippingOptions() {
        if (!selectedAddress) {
            $('#co-carrier-options').html('<p class="text-muted small mb-0">' + tx('selectAddressForCarriers') + '</p>');
            return;
        }
        var cartItems = prefill.cartItems || [];
        if (!cartItems.length) return;

        var cityId = selectedAddress.cityId ? parseInt(selectedAddress.cityId, 10) : parseInt($('#co-shipping-city-id').val(), 10);
        var townId = selectedAddress.townId ? parseInt(selectedAddress.townId, 10) : parseInt($('#co-shipping-town-id').val(), 10);

        $('#co-carrier-options').html('<p class="text-muted small mb-0">' + tx('loadingCarriers') + '</p>');

        $.ajax({
            type: 'POST',
            url: '/checkout/shipping-options',
            contentType: 'application/json',
            data: JSON.stringify({
                cityId: cityId > 0 ? cityId : null,
                townId: townId > 0 ? townId : null,
                postalCode: selectedAddress.postalCode || null,
                cartItems: cartItems
            }),
            success: function (res) {
                var options = (res && res.options) || (res && res.Options) || [];
                var defaultId = (res && res.defaultCarrierId) || (res && res.DefaultCarrierId) || prefill.carrierId;
                var currentId = $('#co-carrier-id').val() || defaultId;
                renderCarrierOptions(options, currentId);
                var $sel = $('input[name="co-carrier"]:checked');
                if (!$sel.length) $sel = $('input[name="co-carrier"]').first();
                selectCarrierOption($sel);
            },
            error: function () {
                $('#co-carrier-options').html('<p class="text-danger small mb-0">' + tx('carriersLoadFailed') + '</p>');
            }
        });
    }

    function syncShippingHidden(addr) {
        if (!addr) return;
        $('#co-shipping-address-id').val(addr.id || '');
        $('#co-shipping-city-id').val(addr.cityId || '');
        $('#co-shipping-town-id').val(addr.townId || '');
    }

    function syncBillingHidden(addr, meta) {
        var invoice = meta || (addr && addr.invoiceMeta) || { invoiceType: 'Individual' };
        if (addr) $('#co-billing-address-id').val(addr.id || '');
        $('#co-invoice-type').val(invoice.invoiceType || 'Individual');
        currentInvoiceType = invoice.invoiceType || 'Individual';
        $('#co-tax-number').val(invoice.taxNumber || '');
        $('#co-tax-office').val(invoice.taxOffice || '');
        $('#co-company-name').val(invoice.companyName || '');
        $('#co-e-invoice').val(invoice.eInvoice ? 'true' : 'false');
    }

    function applyShippingAddress(addr, meta) {
        selectedAddress = addr;
        syncShippingHidden(addr);
        if (!$('#co-billing-different').is(':checked')) {
            selectedBillingAddress = addr;
            syncBillingHidden(addr, meta || (addr && addr.invoiceMeta));
        }
        renderCheckoutAddressPicker();
        renderBillingAddressPicker();
        renderAddressList();
        refreshShippingOptions();
        scheduleSaveDraft();
    }

    function applyBillingAddress(addr) {
        selectedBillingAddress = addr;
        syncBillingHidden(addr, addr && addr.invoiceMeta);
        renderBillingAddressPicker();
        scheduleSaveDraft();
    }

    function toggleBillingSection() {
        var show = $('#co-billing-different').is(':checked');
        $('#co-billing-section').toggle(show);
        if (show) {
            if (!selectedBillingAddress && selectedAddress) {
                applyBillingAddress(selectedAddress);
            } else {
                renderBillingAddressPicker();
            }
        } else if (selectedAddress) {
            selectedBillingAddress = selectedAddress;
            syncBillingHidden(selectedAddress, selectedAddress.invoiceMeta);
        }
    }

    function upsertSavedAddress(local, meta) {
        local.invoiceMeta = meta;
        var idx = savedAddresses.findIndex(function (a) { return String(a.id) === String(local.id); });
        if (idx >= 0) savedAddresses[idx] = local;
        else savedAddresses.push(local);
        listSelectedId = local.id;
        applyShippingAddress(local, meta);
    }

    function deleteAddress(id) {
        if (!confirm(tx('deleteAddressConfirm'))) return;

        var wasShipping = selectedAddress && String(selectedAddress.id) === String(id);
        var wasBilling = selectedBillingAddress && String(selectedBillingAddress.id) === String(id);

        function afterDelete() {
            savedAddresses = savedAddresses.filter(function (a) { return String(a.id) !== String(id); });
            if (wasShipping) {
                selectedAddress = savedAddresses[0] || null;
                if (selectedAddress) applyShippingAddress(selectedAddress, selectedAddress.invoiceMeta);
                else {
                    selectedBillingAddress = null;
                    renderCheckoutAddressPicker();
                    renderBillingAddressPicker();
                }
            }
            if (wasBilling && !wasShipping) {
                selectedBillingAddress = savedAddresses[0] || null;
                if (selectedBillingAddress) applyBillingAddress(selectedBillingAddress);
                else renderBillingAddressPicker();
            }
            renderAddressList();
            scheduleSaveDraft();
        }

        if (prefill.isLoggedIn && !isLocalAddressId(id)) {
            $.ajax({
                type: 'DELETE',
                url: '/account/addresses/' + id,
                success: afterDelete,
                error: function (xhr) {
                    alert((xhr.responseJSON && xhr.responseJSON.message) || tx('addressDeleteFailed'));
                }
            });
        } else {
            afterDelete();
        }
    }

    function loadCountries() {
        return $.getJSON('/account/locations/countries').then(function (items) {
            var $country = $('#checkout-addr-country').empty().append('<option value="">' + tx('selectOption') + '</option>');
            (items || []).forEach(function (item) {
                $country.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        }).catch(function () {
            $('#checkout-addr-country').html('<option value="1">' + tx('turkey') + '</option>');
        });
    }

    function loadCities(countryId) {
        var $city = $('#checkout-addr-city').empty().append('<option value="">' + tx('selectOption') + '</option>');
        $('#checkout-addr-town').empty().append('<option value="">' + tx('selectOption') + '</option>');
        if (!countryId) return $.when();
        return $.getJSON('/account/locations/cities', { countryId: countryId }).then(function (items) {
            (items || []).forEach(function (item) {
                $city.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        });
    }

    function loadTowns(cityId) {
        var $town = $('#checkout-addr-town').empty().append('<option value="">' + tx('selectOption') + '</option>');
        if (!cityId) return $.when();
        return $.getJSON('/account/locations/towns', { cityId: cityId }).then(function (items) {
            (items || []).forEach(function (item) {
                $town.append('<option value="' + item.id + '">' + item.label + '</option>');
            });
        });
    }

    function resetAddrForm() {
        $('#checkout-addr-id').val('0');
        $('#checkout-addr-form')[0].reset();
        $('#checkout-addr-city, #checkout-addr-town').empty().append('<option value="">' + tx('selectOption') + '</option>');
        setInvoiceType(addrFormMode === 'corporate' ? 'Corporate' : 'Individual');
    }

    function openAddrFormModal(mode, addr) {
        addrFormMode = mode === 'corporate' ? 'corporate' : 'individual';
        $('#checkout-addr-form-title').text(addr ? tx('editAddress') : tx('addAddress'));
        resetAddrForm();

        loadCountries().then(function () {
            if (!addr) prefillGuestIntoAddrForm();
            if (!addr) {
                var tr = $('#checkout-addr-country option').filter(function () {
                    return $(this).text().toLowerCase().indexOf(tx('turkeyMatch')) >= 0;
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

        $('#checkout-address-list-modal').modal('hide');
        $('#checkout-address-form-modal').modal('show');
    }

    function fillAddrForm(addr) {
        $('#checkout-addr-id').val(addr.id || 0);
        $('#checkout-addr-label').val(addr.label || '');
        $('#checkout-addr-firstname').val(addr.contactFirstName || '');
        $('#checkout-addr-lastname').val(addr.contactLastName || '');
        $('#checkout-addr-phone').val(addr.contactPhone || '');
        $('#checkout-addr-line1').val(addr.addressLine1 || '');

        var meta = addr.invoiceMeta || {};
        var isCorp = meta.invoiceType === 'Corporate' || (addr.label || '').toLowerCase().indexOf(tx('corporateLabelMatch')) >= 0;
        setInvoiceType(isCorp ? 'Corporate' : 'Individual');
        $('#checkout-addr-tax-number').val(meta.taxNumber || '');
        $('#checkout-addr-tax-office').val(meta.taxOffice || '');
        $('#checkout-addr-company').val(meta.companyName || '');
        $('#checkout-addr-e-invoice').prop('checked', !!meta.eInvoice);

        $('#checkout-addr-country').val(addr.countryId || '');
        loadCities(addr.countryId).then(function () {
            $('#checkout-addr-city').val(addr.cityId || '');
            return loadTowns(addr.cityId);
        }).then(function () {
            $('#checkout-addr-town').val(addr.townId || '');
        });
    }

    function collectCheckoutDraft() {
        return {
            guestEmail: getGuestEmail(),
            guestFirstName: getGuestFirstName(),
            guestLastName: getGuestLastName(),
            addresses: savedAddresses.slice(),
            selectedShippingId: selectedAddress ? selectedAddress.id : null,
            selectedBillingId: selectedBillingAddress ? selectedBillingAddress.id : null,
            billingDifferent: $('#co-billing-different').is(':checked'),
            paymentMethod: $('input[name="co-payment"]:checked').val() || null,
            bankAccountId: parseInt($('input[name="co-bank-account"]:checked').val(), 10) || null,
            orderNote: ($('#co-order-note').val() || '').trim(),
            localAddrSeq: localAddrSeq
        };
    }

    function scheduleSaveDraft() {
        if (restoringDraft) return;
        clearTimeout(saveDraftTimer);
        saveDraftTimer = setTimeout(saveCheckoutDraft, 500);
    }

    function saveCheckoutDraft(sync) {
        var payload = JSON.stringify(collectCheckoutDraft());
        if (sync && window.fetch) {
            fetch('/checkout/draft', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: payload,
                keepalive: true
            });
            return;
        }
        $.ajax({
            type: 'PUT',
            url: '/checkout/draft',
            contentType: 'application/json',
            data: payload
        });
    }

    function flushCheckoutDraft() {
        if (restoringDraft) return;
        clearTimeout(saveDraftTimer);
        saveCheckoutDraft(true);
    }

    function restoreCheckoutDraft() {
        return $.getJSON('/checkout/draft').then(function (draft) {
            if (!draft) return;
            applyCheckoutDraft(draft);
        }).catch(function () { /* taslak yok */ });
    }

    function applyCheckoutDraft(draft) {
        restoringDraft = true;
        try {
            if (!prefill.isLoggedIn && draft.addresses && draft.addresses.length) {
                savedAddresses = draft.addresses.slice();
                if (draft.localAddrSeq) localAddrSeq = draft.localAddrSeq;
            }

            if (draft.guestEmail) {
                $('#co-guest-email').val(draft.guestEmail);
            }
            if (draft.guestFirstName) {
                $('#co-guest-firstname').val(draft.guestFirstName);
            }
            if (draft.guestLastName) {
                $('#co-guest-lastname').val(draft.guestLastName);
            }

            $('#co-billing-different').prop('checked', !!draft.billingDifferent);
            toggleBillingSection();

            if (draft.orderNote) {
                $('#co-order-note').val(draft.orderNote);
            }

            if (draft.selectedShippingId) {
                var ship = findAddressById(draft.selectedShippingId);
                if (ship) {
                    applyShippingAddress(ship, ship.invoiceMeta || null);
                }
            } else if (!prefill.isLoggedIn) {
                renderCheckoutAddressPicker();
                renderBillingAddressPicker();
            }

            if (draft.billingDifferent && draft.selectedBillingId) {
                var bill = findAddressById(draft.selectedBillingId);
                if (bill) {
                    applyBillingAddress(bill);
                }
            }

            if (draft.paymentMethod) {
                var $pay = $('input[name="co-payment"][value="' + draft.paymentMethod + '"]');
                if ($pay.length) {
                    selectPaymentMethod($pay, true);
                }
            }

            if (draft.bankAccountId) {
                $('input[name="co-bank-account"][value="' + draft.bankAccountId + '"]').prop('checked', true);
            }
        } finally {
            restoringDraft = false;
        }
    }

    function loadSavedAddresses() {
        if (!prefill.isLoggedIn) {
            renderCheckoutAddressPicker();
            renderBillingAddressPicker();
            return $.when();
        }

        var previousId = selectedAddress ? selectedAddress.id : null;
        return $.getJSON('/account/addresses/list').then(function (items) {
            savedAddresses = items || [];
            if (!savedAddresses.length) {
                selectedAddress = null;
                renderCheckoutAddressPicker();
                renderAddressList();
                return;
            }

            var pick = previousId ? findAddressById(previousId) : null;
            if (!pick) {
                pick = savedAddresses.find(function (a) { return a.isDefaultShipping; }) || savedAddresses[0];
            }
            applyShippingAddress(pick, pick.invoiceMeta || { invoiceType: 'Individual' });
        }).catch(function () {
            renderCheckoutAddressPicker();
        });
    }

    function collectAddrFormPayload() {
        var formId = parseAddressFormId();
        return {
            id: typeof formId === 'number' ? formId : 0,
            label: $('#checkout-addr-label').val(),
            contactFirstName: $('#checkout-addr-firstname').val(),
            contactLastName: $('#checkout-addr-lastname').val(),
            contactPhone: $('#checkout-addr-phone').val(),
            countryId: parseInt($('#checkout-addr-country').val(), 10) || 0,
            cityId: parseInt($('#checkout-addr-city').val(), 10) || null,
            townId: parseInt($('#checkout-addr-town').val(), 10) || null,
            addressLine1: $('#checkout-addr-line1').val(),
            addressLine2: '',
            isDefaultBilling: false,
            isDefaultShipping: savedAddresses.length === 0
        };
    }

    function collectInvoiceMetaFromForm() {
        var type = currentInvoiceType;
        return {
            invoiceType: type,
            taxNumber: type === 'Corporate' ? $('#checkout-addr-tax-number').val() : '',
            taxOffice: type === 'Corporate' ? $('#checkout-addr-tax-office').val() : '',
            companyName: type === 'Corporate' ? $('#checkout-addr-company').val() : '',
            eInvoice: type === 'Corporate' && $('#checkout-addr-e-invoice').is(':checked')
        };
    }

    function validateAddrForm() {
        var form = document.getElementById('checkout-addr-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return false;
        }
        if (currentInvoiceType === 'Corporate') {
            if (!$('#checkout-addr-tax-number').val() || !$('#checkout-addr-tax-office').val() || !$('#checkout-addr-company').val()) {
                alert(tx('corporateInvoiceRequired'));
                return false;
            }
        }
        return true;
    }

    function buildLocalAddress(payload, meta, saved) {
        var cityLabel = $('#checkout-addr-city option:selected').text();
        var townLabel = $('#checkout-addr-town option:selected').text();
        var countryLabel = $('#checkout-addr-country option:selected').text();
        var formId = parseAddressFormId();
        var resolvedId = saved && saved.id ? saved.id : (formId && formId !== 0 && formId !== '0' ? formId : null);
        return {
            id: resolvedId,
            label: payload.label,
            contactFirstName: payload.contactFirstName,
            contactLastName: payload.contactLastName,
            contactPhone: payload.contactPhone,
            countryId: payload.countryId,
            countryName: countryLabel,
            cityId: payload.cityId,
            cityName: cityLabel !== tx('selectOption') ? cityLabel : '',
            townId: payload.townId,
            townName: townLabel !== tx('selectOption') ? townLabel : '',
            addressLine1: payload.addressLine1,
            displayLine: [payload.addressLine1, townLabel, cityLabel].filter(function (x) { return x && x !== tx('selectOption'); }).join(' / ')
        };
    }

    function updatePaymentPanels($panel) {
        $('.checkout-pay-panel').removeClass('is-active');
        $('.checkout-pay-header').addClass('collapsed').attr('aria-expanded', 'false');
        if ($panel && $panel.length) {
            $panel.addClass('is-active');
            $panel.find('.checkout-pay-header').removeClass('collapsed').attr('aria-expanded', 'true');
        }
    }

    function openPaymentPanel($panel) {
        $('#checkout-payment-accordion > .checkout-pay-panel > .collapse').removeClass('show');
        if ($panel && $panel.length) {
            $panel.children('.collapse').addClass('show');
        }
    }

    function selectPaymentMethod($radio, skipDraftSave) {
        if (!$radio || !$radio.length) return;
        $('input[name="co-payment"]').prop('checked', false);
        $radio.prop('checked', true);
        var $panel = $radio.closest('.checkout-pay-panel');
        updatePaymentPanels($panel);
        openPaymentPanel($panel);
        var $paymentSection = $('#co-collapse-payment');
        if ($paymentSection.length && !$paymentSection.hasClass('show')) {
            $paymentSection.addClass('show');
        }
        if (!skipDraftSave) scheduleSaveDraft();
    }

    // Tek handler — change + bootstrap collapse çift tetiklemesini önle
    $('#checkout-payment-accordion').on('click', 'label.checkout-pay-header', function (e) {
        if ($(e.target).closest('button, a').length) return;
        e.preventDefault();
        selectPaymentMethod($(this).find('input[name="co-payment"]'));
    });

    $('#checkout-payment-accordion').on('click', '.checkout-bank-card', function (e) {
        if ($(e.target).closest('.js-copy-iban').length) return;
        selectPaymentMethod($(this).closest('.checkout-pay-panel').find('input[name="co-payment"]'));
    });

    $(document).on('click', '.js-copy-iban', function () {
        var iban = ($(this).data('iban') || '').toString().trim();
        if (!iban) return;
        var normalized = iban.replace(/\s/g, '');
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(normalized).then(function () {
                showCheckoutNotify(tx('ibanCopied'), 'success');
            });
        } else {
            var $tmp = $('<textarea>').val(normalized).appendTo('body').select();
            document.execCommand('copy');
            $tmp.remove();
            showCheckoutNotify(tx('ibanCopied'), 'success');
        }
    });

    // Events
    $('#co-btn-add-address').on('click', function () {
        openAddrFormModal('individual');
    });

    $('#co-address-picker').on('change', 'input[name="checkout-addr-select"]', function () {
        var addr = findAddressById($(this).val());
        if (addr) applyShippingAddress(addr, addr.invoiceMeta || null);
    });

    $('#co-address-picker').on('click', '.checkout-addr-card-select', function (e) {
        if ($(e.target).closest('button').length) return;
        var $radio = $(this).find('input[name="checkout-addr-select"]');
        $radio.prop('checked', true).trigger('change');
    });

    $('#co-billing-address-picker').on('change', 'input[name="checkout-billing-addr-select"]', function () {
        var addr = findAddressById($(this).val());
        if (addr) applyBillingAddress(addr);
    });

    $('#co-billing-address-picker').on('click', '.checkout-addr-card-select', function (e) {
        if ($(e.target).closest('button').length) return;
        var $radio = $(this).find('input[name="checkout-billing-addr-select"]');
        $radio.prop('checked', true).trigger('change');
    });

    $(document).on('click', '.checkout-addr-edit-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var addr = findAddressById($(this).data('id'));
        if (addr) openAddrFormModal('individual', addr);
    });

    $(document).on('click', '.checkout-addr-delete-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        deleteAddress($(this).data('id'));
    });

    $('#co-billing-different').on('change', function () {
        toggleBillingSection();
        scheduleSaveDraft();
    });

    $('#co-guest-email, #co-guest-firstname, #co-guest-lastname, #co-order-note').on('input change', scheduleSaveDraft);
    $('#checkout-payment-accordion').on('change', 'input[name="co-bank-account"]', scheduleSaveDraft);

    $(window).on('pagehide', flushCheckoutDraft);
    $('a[href]').not('[href^="#"]').on('click', function () {
        var href = ($(this).attr('href') || '').toLowerCase();
        if (href.indexOf('/checkout') === 0) return;
        flushCheckoutDraft();
    });

    $('#checkout-addr-list-new-btn').on('click', function () {
        openAddrFormModal('individual');
    });

    $('#checkout-addr-list').on('click', '.checkout-addr-edit-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = parseInt($(this).data('id'), 10);
        var addr = findAddressById(id);
        if (addr) openAddrFormModal('individual', addr);
    });

    $('#checkout-addr-list').on('change', 'input[name="checkout-addr-pick"]', function () {
        listSelectedId = parseInt($(this).val(), 10);
    });

    $('#checkout-addr-list-select-btn').on('click', function () {
        var id = listSelectedId || parseInt($('input[name="checkout-addr-pick"]:checked').val(), 10);
        if (!id) {
            alert(tx('pleaseSelectAddress'));
            return;
        }
        var addr = findAddressById(id);
        if (addr) {
            applyShippingAddress(addr, addr.invoiceMeta || { invoiceType: 'Individual' });
            $('#checkout-address-list-modal').modal('hide');
        }
    });

    $('.checkout-invoice-btn').on('click', function () {
        setInvoiceType($(this).data('invoice'));
    });

    $('#checkout-addr-country').on('change', function () { loadCities($(this).val()); });
    $('#checkout-addr-city').on('change', function () { loadTowns($(this).val()); });

    $('#checkout-addr-form-save-btn').on('click', function () {
        if (!validateAddrForm()) return;

        var payload = collectAddrFormPayload();
        var meta = collectInvoiceMetaFromForm();
        if (meta.invoiceType === 'Corporate' && !payload.label.toLowerCase().includes(tx('corporateLabelMatch'))) {
            payload.label = tx('corporatePrefix') + payload.label;
        }

        var $btn = $(this).prop('disabled', true);

        function finish(saved) {
            var local = buildLocalAddress(payload, meta, saved);
            if (saved && saved.id) local.id = saved.id;
            else if (!local.id) local.id = nextLocalAddressId();

            if (prefill.isLoggedIn) {
                return loadSavedAddresses().then(function () {
                    var refreshed = findAddressById(local.id);
                    if (refreshed) applyShippingAddress(refreshed, meta);
                    else upsertSavedAddress(local, meta);
                    $('#checkout-address-form-modal').modal('hide');
                });
            }

            upsertSavedAddress(local, meta);
            $('#checkout-address-form-modal').modal('hide');
            return $.when();
        }

        if (prefill.isLoggedIn) {
            $.ajax({
                type: 'POST',
                url: '/account/addresses/save',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (saved) { finish(saved); },
                error: function (xhr) {
                    alert((xhr.responseJSON && xhr.responseJSON.message) || tx('addressSaveFailed'));
                },
                complete: function () { $btn.prop('disabled', false); }
            });
        } else {
            finish(null);
            $btn.prop('disabled', false);
        }
    });

    selectPaymentMethod($('input[name="co-payment"]:checked').first());

    $('#co-carrier-options').on('change', 'input[name="co-carrier"]', function () {
        $('.checkout-carrier-option').removeClass('is-selected');
        $(this).closest('.checkout-carrier-option').addClass('is-selected');
        selectCarrierOption($(this));
    });

    $form.on('submit', function (e) {
        e.preventDefault();

        if (!selectedAddress) {
            alert(tx('pleaseSelectShipping'));
            $('#co-collapse-address').collapse('show');
            return;
        }

        var billingDifferent = $('#co-billing-different').is(':checked');
        var billingAddr = billingDifferent ? selectedBillingAddress : selectedAddress;
        if (billingDifferent && !billingAddr) {
            alert(tx('pleaseSelectBilling'));
            $('#co-collapse-address').collapse('show');
            return;
        }

        if (!validateGuestInfo()) return;

        var orderEmail = getGuestEmail();
        if (prefill.isLoggedIn && !orderEmail) {
            alert(tx('emailRequired'));
            return;
        }

        var orderFirstName = (selectedAddress.contactFirstName || getGuestFirstName() || '').trim();
        var orderLastName = (selectedAddress.contactLastName || getGuestLastName() || '').trim();
        if (!orderFirstName || !orderLastName) {
            alert(tx('firstLastRequired'));
            if (!prefill.isLoggedIn) $('#co-collapse-guest').collapse('show');
            return;
        }

        var paymentMethod = $('input[name="co-payment"]:checked').val() || 'CashOnDelivery';
        if (paymentMethod === 'CreditCard') {
            alert(tx('creditCardNotAvailable'));
            return;
        }

        if (paymentMethod === 'BankTransfer' && !prefill.bankTransferEnabled) {
            alert(tx('bankTransferNotAvailable') || 'Havale / EFT şu an aktif değil.');
            return;
        }

        var bankAccountId = null;
        if (paymentMethod === 'BankTransfer') {
            var bankRaw = $('input[name="co-bank-account"]:checked').val();
            bankAccountId = bankRaw ? parseInt(bankRaw, 10) : null;
            if (!bankAccountId || bankAccountId <= 0) {
                alert('Lütfen havale için banka hesabı seçin.');
                $('#co-collapse-payment').collapse('show');
                openPaymentPanel($('.checkout-pay-panel[data-payment="BankTransfer"]'));
                return;
            }
        }

        var carrierRaw = $('#co-carrier-id').val();
        var carrierId = carrierRaw ? parseInt(carrierRaw, 10) : null;
        if (!carrierId || carrierId <= 0) {
            alert(tx('pleaseSelectCarrier'));
            $('#co-collapse-shipping').collapse('show');
            return;
        }

        var addressId = selectedAddress.id;
        var customerAddressId = (addressId && !isLocalAddressId(addressId))
            ? (parseInt(addressId, 10) || null)
            : null;

        var payload = {
            customerAddressId: customerAddressId,
            firstName: orderFirstName,
            lastName: orderLastName,
            company: $('#co-company-name').val() || '',
            country: selectedAddress.countryName || tx('turkey'),
            street: selectedAddress.addressLine1,
            street2: selectedAddress.addressLine2 || '',
            city: selectedAddress.cityName || '',
            state: selectedAddress.townName || '',
            postcode: selectedAddress.postalCode || '',
            phone: selectedAddress.contactPhone || '',
            email: orderEmail,
            couponCode: $('#checkout-discount-input').val(),
            languageCode: (prefill.languageCode || 'tr'),
            paymentMethod: paymentMethod,
            bankAccountId: bankAccountId,
            carrierId: carrierId && carrierId > 0 ? carrierId : null,
            shippingCityId: selectedAddress.cityId ? parseInt(selectedAddress.cityId, 10) : parseInt($('#co-shipping-city-id').val(), 10) || null,
            shippingTownId: selectedAddress.townId ? parseInt(selectedAddress.townId, 10) : parseInt($('#co-shipping-town-id').val(), 10) || null,
            billingSameAsShipping: !billingDifferent,
            billingCustomerAddressId: billingDifferent && billingAddr && !isLocalAddressId(billingAddr.id)
                ? (parseInt(billingAddr.id, 10) || null)
                : null,
            billingFirstName: billingDifferent ? billingAddr.contactFirstName : null,
            billingLastName: billingDifferent ? billingAddr.contactLastName : null,
            billingCountry: billingDifferent ? billingAddr.countryName : null,
            billingStreet: billingDifferent ? billingAddr.addressLine1 : null,
            billingStreet2: billingDifferent ? (billingAddr.addressLine2 || '') : null,
            billingCity: billingDifferent ? billingAddr.cityName : null,
            billingState: billingDifferent ? billingAddr.townName : null,
            billingPostcode: billingDifferent ? (billingAddr.postalCode || '') : null,
            billingPhone: billingDifferent ? billingAddr.contactPhone : null,
            orderNote: ($('#co-order-note').val() || '').trim() || null,
            invoiceType: $('#co-invoice-type').val() || 'Individual',
            taxNumber: $('#co-tax-number').val() || '',
            taxOffice: $('#co-tax-office').val() || '',
            companyName: $('#co-company-name').val() || '',
            isEInvoice: $('#co-e-invoice').val() === 'true'
        };

        var $btn = $form.find('button[type="submit"]');
        $btn.prop('disabled', true);
        if (window.showStorePageLoading) {
            window.showStorePageLoading();
        }

        $.ajax({
            type: 'POST',
            url: '/checkout/place-order',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                $.ajax({ type: 'DELETE', url: '/checkout/draft' });
                var payment = res.paymentMethod || payload.paymentMethod;
                if (window.showStorePageLoading) {
                    window.showStorePageLoading();
                }
                window.location.href = '/checkout/confirmation/' + res.orderId
                    + '?orderNumber=' + encodeURIComponent(res.orderNumber || '')
                    + '&payment=' + encodeURIComponent(payment);
            },
            error: function (xhr) {
                if (window.hideStorePageLoading) {
                    window.hideStorePageLoading();
                }
                $btn.prop('disabled', false);
                alert((xhr.responseJSON && xhr.responseJSON.message) || tx('orderFailed'));
            }
        });
    });

    if (prefill.isLoggedIn && prefill.email) {
        $('#co-email').val(prefill.email);
    }
    loadCountries();
    loadSavedAddresses().then(function () {
        return restoreCheckoutDraft();
    });
});
