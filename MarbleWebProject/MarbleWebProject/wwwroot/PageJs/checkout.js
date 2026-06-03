$(function () {
    var $form = $('#checkout-form');
    if ($form.length === 0) return;

    $form.on('submit', function (e) {
        e.preventDefault();
        var payload = {
            firstName: $('#co-firstname').val(),
            lastName: $('#co-lastname').val(),
            company: $('#co-company').val(),
            country: $('#co-country').val(),
            street: $('#co-street').val(),
            street2: $('#co-street2').val(),
            city: $('#co-city').val(),
            state: $('#co-state').val(),
            postcode: $('#co-postcode').val(),
            phone: $('#co-phone').val(),
            email: $('#co-email').val(),
            couponCode: $('#checkout-discount-input').val(),
            languageCode: 'tr'
        };

        $.ajax({
            type: 'POST',
            url: '/checkout/place-order',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                alert('Sipariş oluşturuldu #' + res.orderId + ' — Toplam: ' + res.grandTotal + ' ' + (res.currencyCode || ''));
                window.location.href = '/';
            },
            error: function (xhr) {
                var msg = xhr.responseJSON && xhr.responseJSON.message ? xhr.responseJSON.message : 'Sipariş başarısız.';
                alert(msg);
            }
        });
    });
});
