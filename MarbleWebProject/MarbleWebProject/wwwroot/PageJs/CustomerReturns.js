(function () {
    'use strict';

    var $list = $('#returns-list');
    var $detail = $('#return-detail');
    var $detailBody = $('#return-detail-body');

    function formatDate(value) {
        if (!value) return '—';
        var d = new Date(value);
        if (isNaN(d.getTime())) return '—';
        return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' });
    }

    function escapeHtml(text) {
        return String(text || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function statusBadgeClass(status) {
        if (status === 1) return 'text-bg-success';
        if (status === 2) return 'text-bg-danger';
        if (status === 3) return 'text-bg-secondary';
        return 'text-bg-warning';
    }

    function renderList(items) {
        if (!items || !items.length) {
            $list.html(
                '<p class="text-muted">Henüz iade talebiniz yok.</p>' +
                '<p class="small"><a href="/account/orders">Siparişlerim</a> sayfasından uygun siparişler için iade talebi oluşturabilirsiniz.</p>'
            );
            return;
        }

        var html = '<div class="table-responsive"><table class="table table-sm store-account-table"><thead><tr>' +
            '<th>Talep no</th><th>Sipariş</th><th>Tarih</th><th>Durum</th><th></th>' +
            '</tr></thead><tbody>';

        items.forEach(function (item) {
            var id = item.id || item.ID;
            var orderId = item.shopOrderID || item.ShopOrderID;
            html += '<tr>' +
                '<td>#' + escapeHtml(String(id)) + '</td>' +
                '<td><a href="/account/orders" class="text-decoration-none">Sipariş #' + escapeHtml(String(orderId)) + '</a></td>' +
                '<td>' + formatDate(item.createdAt || item.CreatedAt) + '</td>' +
                '<td><span class="badge ' + statusBadgeClass(item.status ?? item.Status) + '">' +
                escapeHtml(item.statusText || item.StatusText || '') + '</span></td>' +
                '<td><button type="button" class="btn btn-sm btn-outline-primary js-return-view" data-id="' + id + '">Detay</button></td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        $list.html(html);
    }

    function renderDetail(data) {
        var lines = (data.lines || data.Lines || []).map(function (line) {
            var name = line.productNameSnapshot || line.ProductNameSnapshot || 'Ürün';
            return '<tr><td>' + escapeHtml(name) + '</td><td>' + (line.quantity || line.Quantity) + '</td><td>' +
                escapeHtml(line.note || line.Note || '—') + '</td></tr>';
        }).join('');

        var adminNote = data.adminNote || data.AdminNote;
        var adminHtml = adminNote
            ? '<p class="mb-2"><strong>Mağaza notu:</strong> ' + escapeHtml(adminNote) + '</p>'
            : '';

        $detailBody.html(
            '<div class="card"><div class="card-body">' +
            '<h3 class="h5">İade talebi #' + escapeHtml(String(data.id || data.ID)) + '</h3>' +
            '<p class="mb-1"><strong>Sipariş:</strong> #' + escapeHtml(String(data.shopOrderID || data.ShopOrderID)) + '</p>' +
            '<p class="mb-1"><strong>Tarih:</strong> ' + formatDate(data.createdAt || data.CreatedAt) + '</p>' +
            '<p class="mb-2"><strong>Durum:</strong> <span class="badge ' + statusBadgeClass(data.status ?? data.Status) + '">' +
            escapeHtml(data.statusText || data.StatusText || '') + '</span></p>' +
            (data.reason || data.Reason ? '<p class="mb-2"><strong>Gerekçe:</strong> ' + escapeHtml(data.reason || data.Reason) + '</p>' : '') +
            adminHtml +
            '<div class="table-responsive mt-3"><table class="table table-sm"><thead><tr><th>Ürün</th><th>Adet</th><th>Not</th></tr></thead><tbody>' +
            lines + '</tbody></table></div></div></div>'
        );
        $list.addClass('d-none');
        $detail.removeClass('d-none');
    }

    function loadList() {
        $list.removeClass('d-none');
        $detail.addClass('d-none');
        $list.html('<p class="text-muted">Yükleniyor...</p>');
        $.getJSON('/account/returns/list')
            .done(function (items) { renderList(items); })
            .fail(function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.message) || 'İade talepleri yüklenemedi.';
                $list.html('<p class="text-danger">' + escapeHtml(msg) + '</p>');
            });
    }

    $list.on('click', '.js-return-view', function () {
        var id = $(this).data('id');
        $.getJSON('/account/returns/' + id)
            .done(renderDetail)
            .fail(function () { alert('İade talebi detayı yüklenemedi.'); });
    });

    $('#return-back').on('click', loadList);

    loadList();
})();
