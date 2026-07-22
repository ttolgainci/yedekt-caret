(function () {
    'use strict';

    var $list = $('#invoices-list');
    var $detail = $('#invoice-detail');
    var $detailBody = $('#invoice-detail-body');

    function formatMoney(value, currency) {
        var n = Number(value);
        if (isNaN(n)) return '—';
        return n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (currency || 'TRY');
    }

    function formatDate(value) {
        if (!value) return '—';
        var d = new Date(value);
        if (isNaN(d.getTime())) return '—';
        return d.toLocaleString('tr-TR');
    }

    function renderList(items) {
        if (!items || !items.length) {
            $list.html('<p class="text-muted">Henüz görüntülenebilir faturanız yok.</p>');
            return;
        }

        var html = '<div class="table-responsive"><table class="table table-sm store-account-table"><thead><tr>' +
            '<th>Fatura no</th><th>Sipariş</th><th>Tarih</th><th>Tutar</th><th>Durum</th><th></th>' +
            '</tr></thead><tbody>';

        items.forEach(function (item) {
            html += '<tr>' +
                '<td>' + escapeHtml(item.invoiceNumber || '') + '</td>' +
                '<td>' + escapeHtml(item.orderNumber || '—') + '</td>' +
                '<td>' + formatDate(item.issueDate) + '</td>' +
                '<td>' + formatMoney(item.grandTotal, item.currencyCode) + '</td>' +
                '<td>' + escapeHtml(item.statusText || '') + '</td>' +
                '<td class="text-nowrap">' +
                '<button type="button" class="btn btn-sm btn-outline-primary js-invoice-view" data-id="' + item.id + '">Detay</button>';

            if (item.pdfUrl || item.hasPdf || item.status >= 1) {
                var pdfHref = item.pdfUrl || ('/account/invoices/' + item.id + '/pdf');
                html += ' <a class="btn btn-sm btn-outline-secondary" href="' + escapeHtml(pdfHref) + '" target="_blank" rel="noopener">PDF</a>';
            }

            html += '</td></tr>';
        });

        html += '</tbody></table></div>';
        $list.html(html);
    }

    function renderDetail(data) {
        var lines = (data.items || []).map(function (line) {
            return '<tr><td>' + escapeHtml(line.productName) + '</td><td>' + line.quantity + '</td><td>' +
                formatMoney(line.unitPrice, data.currencyCode) + '</td><td>' +
                formatMoney(line.lineTotal, data.currencyCode) + '</td></tr>';
        }).join('');

        var pdfHref = data.pdfUrl || (data.status >= 1 ? '/account/invoices/' + data.id + '/pdf' : '');
        var pdfBtn = pdfHref
            ? '<a class="btn btn-primary btn-sm" href="' + escapeHtml(pdfHref) + '" target="_blank" rel="noopener">PDF indir</a> '
            : '';
        var printBtn = '<a class="btn btn-outline-secondary btn-sm" href="/account/invoices/' + data.id + '/preview" target="_blank" rel="noopener">Fatura önizle / yazdır</a>';

        $detailBody.html(
            '<div class="card"><div class="card-body">' +
            '<h3 class="h5">' + escapeHtml(data.invoiceNumber || '') + '</h3>' +
            '<p class="mb-1"><strong>Sipariş:</strong> ' + escapeHtml(data.orderNumber || '—') + '</p>' +
            '<p class="mb-1"><strong>Tarih:</strong> ' + formatDate(data.issueDate) + '</p>' +
            '<p class="mb-3"><strong>Toplam:</strong> ' + formatMoney(data.grandTotal, data.currencyCode) + '</p>' +
            pdfBtn + printBtn +
            '<div class="table-responsive mt-3"><table class="table table-sm"><thead><tr><th>Ürün</th><th>Adet</th><th>Birim</th><th>Toplam</th></tr></thead><tbody>' +
            lines + '</tbody></table></div></div></div>'
        );
        $list.addClass('d-none');
        $detail.removeClass('d-none');
    }

    function escapeHtml(text) {
        return String(text || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function loadList() {
        $list.removeClass('d-none');
        $detail.addClass('d-none');
        $list.html('<p class="text-muted">Yükleniyor...</p>');
        $.getJSON('/account/invoices/list')
            .done(function (items) { renderList(items); })
            .fail(function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.message) || 'Faturalar yüklenemedi.';
                $list.html('<p class="text-danger">' + escapeHtml(msg) + '</p>');
            });
    }

    $list.on('click', '.js-invoice-view', function () {
        var id = $(this).data('id');
        $.getJSON('/account/invoices/' + id)
            .done(renderDetail)
            .fail(function () {
                alert('Fatura detayı yüklenemedi.');
            });
    });

    $('#invoice-back').on('click', loadList);

    loadList();
})();
