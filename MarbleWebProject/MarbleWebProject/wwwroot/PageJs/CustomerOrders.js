$(function () {
    var orders = [];
    var activeFilter = 'all';
    var searchQuery = '';
    var recipient = $('#orders-list').data('recipient') || '';

    function resolveMediaUrl(path) {
        if (!path) return '';
        if (path.indexOf('http://') === 0 || path.indexOf('https://') === 0) {
            return path.replace(/^https:\/\/(localhost|127\.0\.0\.1)/i, 'http://$1');
        }
        var base = (window.MarbleStore && window.MarbleStore.mediaBase) || '';
        if (!base) return path.charAt(0) === '/' ? path : '/' + path;
        if (base.charAt(base.length - 1) !== '/') base += '/';
        return base + path.replace(/^\/+/, '');
    }

    function escapeAttr(value) {
        return String(value || '')
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;');
    }

    function escapeHtml(value) {
        return escapeAttr(value).replace(/\n/g, '<br>');
    }

    function toggleListChrome(show) {
        $('.store-account-orders-header, #orders-tabs').toggleClass('d-none', !show);
    }

    function packageSummaryText(data) {
        var qty = Number(data.totalQuantity || data.lineCount || 0);
        if (qty < 1) qty = 1;
        return '1 paket, ' + qty + ' ürün';
    }

    function summaryStatusText(data) {
        var qty = Number(data.totalQuantity || data.lineCount || 1);
        switch (data.orderStatus) {
            case 3: return qty + ' ürün teslim edildi';
            case 2: return qty + ' ürün kargoda';
            case 9: return 'Sipariş iptal edildi';
            case 1: return 'Sipariş onaylandı';
            default: return 'Onay bekleniyor';
        }
    }

    function shipmentStatusMessage(data) {
        var qty = Number(data.totalQuantity || data.lineCount || 1);
        var date = formatDate(data.createdAt);
        switch (data.orderStatus) {
            case 3:
                return date + ' tarihinde ' + qty + ' ürün teslim edilmiştir.';
            case 2:
                return qty + ' ürün kargoya verilmiştir.';
            case 9:
                return 'Bu sipariş iptal edilmiştir.';
            case 1:
                return 'Siparişiniz onaylandı, hazırlanıyor.';
            default:
                return 'Siparişiniz onay bekliyor.';
        }
    }

    function formatAddressBlock(addr) {
        if (!addr) {
            return '<p class="store-od-addr-empty">—</p>';
        }
        var name = ((addr.contactFirstName || '') + ' ' + (addr.contactLastName || '')).trim();
        var lines = [
            addr.addressLine1,
            addr.addressLine2,
            [addr.townName, addr.cityName, addr.postalCode].filter(Boolean).join(' / '),
            addr.countryName
        ].filter(Boolean);
        var html = '';
        if (name) {
            html += '<p class="store-od-addr-name"><strong>' + escapeHtml(name) + '</strong></p>';
        }
        if (lines.length) {
            html += '<p class="store-od-addr-lines">' + lines.map(escapeHtml).join('<br>') + '</p>';
        }
        if (addr.contactPhone) {
            html += '<p class="store-od-addr-phone">' + escapeHtml(addr.contactPhone) + '</p>';
        }
        return html || '<p class="store-od-addr-empty">—</p>';
    }

    function linePicture(line) {
        return line.picture || line.Picture || line.image || line.Image || '';
    }

    function getShipment(data) {
        return data.shipment || data.Shipment || data.shipping || data.Shipping || {};
    }

    function renderProductImage(picture, alt) {
        var url = picture && (picture.indexOf('http://') === 0 || picture.indexOf('https://') === 0)
            ? picture.replace(/^https:\/\/(localhost|127\.0\.0\.1)/i, 'http://$1')
            : resolveMediaUrl(picture);
        if (!url) {
            return '<span class="store-od-product-placeholder"></span>';
        }
        return '<img src="' + escapeAttr(url) + '" alt="' + escapeAttr(alt || '') + '" loading="lazy" />';
    }

    function renderProductLines(data) {
        return (data.lines || []).map(function (line) {
            var meta = [];
            var sku = line.skuSnapshot || line.SkuSnapshot;
            var name = line.productNameSnapshot || line.ProductNameSnapshot || '';
            var qty = line.quantity || line.Quantity || 0;
            if (sku) meta.push('SKU: ' + sku);
            meta.push('Adet: ' + qty);
            return (
                '<article class="store-od-product">' +
                    '<div class="store-od-product-media">' + renderProductImage(linePicture(line), name) + '</div>' +
                    '<div class="store-od-product-body">' +
                        '<h4 class="store-od-product-title">' + escapeHtml(name) + '</h4>' +
                        '<p class="store-od-product-meta">' + escapeHtml(meta.join(' · ')) + '</p>' +
                        '<p class="store-od-product-price">' + formatMoney(line.lineTotal || line.LineTotal, data) + '</p>' +
                    '</div>' +
                '</article>'
            );
        }).join('');
    }

    function renderTrackingBox(data) {
        var shipment = getShipment(data);
        var tracking = shipment.trackingNumber || shipment.TrackingNumber || '';
        var carrier = shipment.carrierName || shipment.CarrierName || '';
        var statusText = shipment.statusText || shipment.StatusText || '';
        var shippingPrice = shipment.shippingPrice ?? shipment.ShippingPrice;
        var hasTracking = !!String(tracking).trim();
        var hasCarrier = !!String(carrier).trim();
        var rows = '';
        if (statusText) {
            rows += '<div class="store-od-tracking-row"><span>Kargo durumu:</span><strong>' + escapeHtml(statusText) + '</strong></div>';
        }
        if (hasCarrier) {
            rows += '<div class="store-od-tracking-row"><span>Kargo Firması:</span><strong>' + escapeHtml(carrier) + '</strong></div>';
        }
        if (hasTracking) {
            rows += '<div class="store-od-tracking-row"><span>Takip Numarası:</span><strong>' + escapeHtml(tracking) + '</strong></div>';
        }
        if (shippingPrice != null && !isNaN(Number(shippingPrice))) {
            rows += '<div class="store-od-tracking-row"><span>Kargo ücreti:</span><strong>' + formatMoney(shippingPrice, data) + '</strong></div>';
        }
        var btn = hasTracking
            ? '<button type="button" class="btn btn-sm store-od-track-btn" data-tracking="' + escapeAttr(tracking) + '">Kargom Nerede?</button>'
            : '';
        return (
            '<div class="store-od-tracking">' +
                '<p class="store-od-tracking-msg">' + escapeHtml(shipmentStatusMessage(data)) + '</p>' +
                (rows ? '<div class="store-od-tracking-meta">' + rows + '</div>' : '') +
                btn +
            '</div>'
        );
    }

    function getBankTransfer(data) {
        return data.bankTransfer || data.BankTransfer || {};
    }

    function renderBankTransferBlock(data) {
        if ((data.paymentMethod || '').toLowerCase() !== 'banktransfer') return '';
        var bt = getBankTransfer(data);
        var due = bt.paymentDueAt || bt.PaymentDueAt;
        var dueText = due ? formatDate(due) : '';
        var receiptUrl = bt.receiptUrl || bt.ReceiptUrl || '';
        var receiptUploaded = bt.receiptUploadedAt || bt.ReceiptUploadedAt;
        var verifyText = bt.bankVerificationStatusText || bt.BankVerificationStatusText || '';
        var html = '<section class="store-od-bank-transfer mt-3">' +
            '<h3 class="store-od-footer-title">Havale / EFT</h3>' +
            '<div class="store-od-bank-transfer-body">';
        if (bt.bankName || bt.BankName) {
            html += '<p class="mb-1"><span class="text-muted">Banka:</span> <strong>' + escapeHtml(bt.bankName || bt.BankName) + '</strong></p>';
        }
        if (bt.accountHolder || bt.AccountHolder) {
            html += '<p class="mb-1"><span class="text-muted">Hesap sahibi:</span> ' + escapeHtml(bt.accountHolder || bt.AccountHolder) + '</p>';
        }
        if (bt.iban || bt.Iban) {
            html += '<p class="mb-1"><code>' + escapeHtml(bt.iban || bt.Iban) + '</code></p>';
        }
        html += '<p class="mb-1"><span class="text-muted">Tutar:</span> <strong>' + formatMoney(data.grandTotal, data) + '</strong></p>';
        html += '<p class="mb-2"><span class="text-muted">Açıklama:</span> <strong>' + escapeHtml(data.orderNumber || ('#' + data.id)) + '</strong></p>';
        if (dueText) {
            html += '<p class="small text-muted mb-2">Son ödeme: <strong>' + escapeHtml(dueText) + '</strong></p>';
        }
        if (verifyText) {
            html += '<p class="small mb-2">Doğrulama: ' + escapeHtml(verifyText) + '</p>';
        }
        if (receiptUrl) {
            html += '<p class="small mb-2"><a href="' + escapeAttr(receiptUrl) + '" target="_blank" rel="noopener">Yüklenen dekontu görüntüle</a></p>';
        }
        if (Number(data.paymentStatus) !== 1 && Number(data.orderStatus) !== 9) {
            html += '<div class="store-od-receipt-upload" data-order-id="' + data.id + '">';
            html += receiptUploaded
                ? '<p class="text-success small mb-2 store-od-receipt-status">Dekont alındı, ödeme onayı bekleniyor.</p>'
                : '<p class="text-muted small mb-2 store-od-receipt-status">Havale dekontunuzu yükleyin.</p>';
            html += '<div class="d-flex flex-wrap gap-2 align-items-center">' +
                '<input type="file" class="form-control form-control-sm store-od-receipt-file" accept=".jpg,.jpeg,.png,.gif,.webp,.pdf" />' +
                '<button type="button" class="btn btn-sm btn-outline-primary store-od-receipt-submit">Dekont yükle</button>' +
                '</div></div>';
        }
        html += '</div></section>';
        return html;
    }

    function renderPaymentBlock(data) {
        var method = paymentMethodLabel(data.paymentMethod);
        var status = paymentLabel(data.paymentStatus);
        var rows = [
            '<div class="store-od-pay-row"><span>Ödeme yöntemi</span><strong>' + escapeHtml(method) + '</strong></div>',
            '<div class="store-od-pay-row"><span>Ödeme durumu</span><strong>' + escapeHtml(status) + '</strong></div>',
            '<div class="store-od-pay-row"><span>Ara toplam</span><strong>' + formatMoney(data.subTotal, data) + '</strong></div>',
            '<div class="store-od-pay-row"><span>Kargo</span><strong>' + formatMoney(data.shippingTotal, data) + '</strong></div>'
        ];
        if (Number(data.campaignDiscount) > 0) {
            rows.push('<div class="store-od-pay-row is-discount"><span>İndirim</span><strong>-' + formatMoney(data.campaignDiscount, data) + '</strong></div>');
        }
        rows.push('<div class="store-od-pay-row store-od-pay-total"><span>Toplam <small>(KDV dahil)</small></span><strong>' + formatMoney(data.grandTotal, data) + '</strong></div>');
        return rows.join('');
    }

    function orderSummaryText(order) {
        var qty = Number(order.totalQuantity || order.lineCount || 0);
        if (qty < 1) qty = 1;
        return '1 Teslimat, ' + qty + ' Ürün';
    }

    function renderThumbs(order) {
        var thumbs = order.thumbnails || [];
        if (!thumbs.length) {
            return '<span class="store-order-thumb-placeholder"></span>';
        }
        return thumbs.map(function (path) {
            var url = path && (path.indexOf('http://') === 0 || path.indexOf('https://') === 0)
                ? path
                : resolveMediaUrl(path);
            if (!url) {
                return '<span class="store-order-thumb-placeholder"></span>';
            }
            return '<img class="store-order-thumb" src="' + escapeAttr(url) + '" alt="" loading="lazy" />';
        }).join('');
    }

    function statusLabel(code) {
        switch (code) {
            case 0: return 'Onay bekliyor';
            case 1: return 'Onaylandı';
            case 2: return 'Kargoda';
            case 3: return 'Teslimat edildi';
            case 9: return 'İptal edildi';
            default: return 'Sipariş alındı';
        }
    }

    function statusClass(code) {
        if (code === 3) return 'is-delivered';
        if (code === 9) return 'is-cancelled';
        return 'is-ongoing';
    }

    function paymentLabel(code) {
        switch (code) {
            case 1: return 'Ödendi';
            case 2: return 'Başarısız';
            case 3: return 'İade';
            default: return 'Bekliyor';
        }
    }

    function paymentMethodLabel(method) {
        if (!method) return '-';
        if (method === 'CashOnDelivery') return 'Kapıda ödeme';
        if (method === 'BankTransfer') return 'Havale/EFT';
        return method;
    }

    function formatDate(value) {
        return new Date(value).toLocaleDateString('tr-TR', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    }

    function currencyLabel(source) {
        if (source == null) return 'TL';
        if (typeof source === 'string') return source.trim() || 'TL';
        var sym = source.currencySymbol || source.CurrencySymbol;
        var code = source.currencyCode || source.CurrencyCode;
        if (sym && String(sym).trim()) return String(sym).trim();
        if (code && String(code).trim()) return String(code).trim();
        return 'TL';
    }

    function formatMoney(amount, currencySource) {
        var num = Number(amount || 0);
        return num.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + currencyLabel(currencySource);
    }

    function matchesFilter(order) {
        var status = order.orderStatus;
        if (activeFilter === 'ongoing') return status !== 3 && status !== 9;
        if (activeFilter === 'cancelled') return status === 9;
        if (activeFilter === 'returns') return false;
        return true;
    }

    function matchesSearch(order) {
        if (!searchQuery) return true;
        var q = searchQuery.toLowerCase();
        var label = (order.orderNumber || ('#' + order.id)).toLowerCase();
        return label.indexOf(q) !== -1;
    }

    function renderList() {
        var $list = $('#orders-list').empty();
        $('#order-detail').addClass('d-none');
        toggleListChrome(true);

        var visible = orders.filter(function (o) {
            return matchesFilter(o) && matchesSearch(o);
        });

        if (!visible.length) {
            $list.append('<div class="store-account-empty">Henüz siparişiniz yok.</div>');
            return;
        }

        visible.forEach(function (order) {
            var label = order.orderNumber || ('#' + order.id);
            var $card = $('<article class="store-order-card"></article>');
            var $top = $('<div class="store-order-card-top"></div>');
            $top.append(
                '<div class="store-order-meta"><span class="store-order-meta-label">Sipariş Tarihi</span><strong>' + formatDate(order.createdAt) + '</strong></div>' +
                '<div class="store-order-meta"><span class="store-order-meta-label">Sipariş Özeti</span><strong>' + orderSummaryText(order) + '</strong></div>' +
                '<div class="store-order-meta"><span class="store-order-meta-label">Alıcı</span><strong>' + (recipient || '-') + '</strong></div>' +
                '<div class="store-order-meta store-order-total"><span class="store-order-meta-label">Toplam</span><strong>' + formatMoney(order.grandTotal, order) + '</strong></div>' +
                '<div class="store-order-actions"><button type="button" class="btn btn-primary btn-sm store-account-btn-primary order-view" data-id="' + order.id + '">Detaylar</button></div>'
            );

            var $bottom = $('<div class="store-order-card-bottom ' + statusClass(order.orderStatus) + '"></div>');
            $bottom.append(
                '<div class="store-order-status"><i class="icon-check"></i><span>' + statusLabel(order.orderStatus) + '</span></div>' +
                '<div class="store-order-thumbs">' + renderThumbs(order) + '</div>' +
                '<div class="store-order-review"></div>'
            );

            $card.append($top).append($bottom);
            $list.append($card);
        });
    }

    function renderDetail(data) {
        $('#orders-list').addClass('d-none');
        $('#order-detail').removeClass('d-none');
        toggleListChrome(false);

        var orderNo = data.orderNumber || ('#' + data.id);
        var deliveryNo = data.orderNumber || ('#' + data.id);
        var statusCls = statusClass(data.orderStatus);
        var statusIcon = data.orderStatus === 3 ? '✓' : (data.orderStatus === 9 ? '✕' : (data.orderStatus === 2 ? '↗' : '●'));

        $('#order-detail-body').html(
            '<div class="store-od">' +
                '<button type="button" class="store-od-back" id="orders-back-btn">← Tüm Siparişler</button>' +
                '<div class="store-od-summary">' +
                    '<div class="store-od-summary-item"><span class="store-od-label">Sipariş No</span><strong>' + escapeHtml(orderNo) + '</strong></div>' +
                    '<div class="store-od-summary-item"><span class="store-od-label">Sipariş Tarihi</span><strong>' + escapeHtml(formatDate(data.createdAt)) + '</strong></div>' +
                    '<div class="store-od-summary-item"><span class="store-od-label">Paket Detayları</span><strong>' + escapeHtml(packageSummaryText(data)) + '</strong></div>' +
                    '<div class="store-od-summary-item store-od-summary-status ' + statusCls + '"><span class="store-od-label">Durumu</span><strong>' + escapeHtml(summaryStatusText(data)) + '</strong></div>' +
                '</div>' +
                '<section class="store-od-shipment">' +
                    '<header class="store-od-shipment-head"><span>Teslimat No: <strong>' + escapeHtml(deliveryNo) + '</strong></span></header>' +
                    '<div class="store-od-shipment-body">' +
                        '<div class="store-od-status-banner ' + statusCls + '">' +
                            '<span class="store-od-status-icon" aria-hidden="true">' + statusIcon + '</span>' +
                            '<span>' + escapeHtml(statusLabel(data.orderStatus)) + '</span>' +
                        '</div>' +
                        renderTrackingBox(data) +
                        '<div class="store-od-products">' + renderProductLines(data) + '</div>' +
                    '</div>' +
                '</section>' +
                renderBankTransferBlock(data) +
                '<section class="store-od-footer">' +
                    '<div class="store-od-footer-col">' +
                        '<h3 class="store-od-footer-title">Teslimat Adresi</h3>' +
                        formatAddressBlock(data.shippingAddress) +
                    '</div>' +
                    '<div class="store-od-footer-col">' +
                        '<h3 class="store-od-footer-title">Fatura Adresi</h3>' +
                        formatAddressBlock(data.billingAddress) +
                    '</div>' +
                    '<div class="store-od-footer-col">' +
                        '<h3 class="store-od-footer-title">Ödeme Bilgileri</h3>' +
                        '<div class="store-od-pay-block">' + renderPaymentBlock(data) + '</div>' +
                    '</div>' +
                '</section>' +
            '</div>'
        );
    }

    function loadOrders() {
        return $.getJSON('/account/orders/list').then(function (items) {
            orders = items || [];
            renderList();
        }).fail(function () {
            window.location.href = '/';
        });
    }

    $('#orders-tabs').on('click', '.nav-link', function () {
        $('#orders-tabs .nav-link').removeClass('active');
        $(this).addClass('active');
        activeFilter = $(this).data('filter');
        renderList();
    });

    $('#orders-search').on('input', function () {
        searchQuery = $(this).val().trim();
        renderList();
    });

    $('#orders-list').on('click', '.order-view', function () {
        var id = $(this).data('id');
        $.getJSON('/account/orders/' + id).done(renderDetail);
    });

    $('#order-detail').on('click', '#orders-back-btn', function () {
        $('#order-detail').addClass('d-none');
        $('#orders-list').removeClass('d-none');
        toggleListChrome(true);
    });

    $('#order-detail').on('click', '.store-od-receipt-submit', function () {
        var $wrap = $(this).closest('.store-od-receipt-upload');
        var orderId = $wrap.data('order-id');
        var fileInput = $wrap.find('.store-od-receipt-file')[0];
        if (!fileInput || !fileInput.files || !fileInput.files.length) {
            alert('Lütfen bir dosya seçin.');
            return;
        }
        var fd = new FormData();
        fd.append('file', fileInput.files[0]);
        var $btn = $(this).prop('disabled', true);
        fetch('/account/orders/' + orderId + '/payment-receipt', { method: 'POST', body: fd })
            .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, body: j }; }); })
            .then(function (res) {
                if (!res.ok) throw new Error((res.body && res.body.message) || 'Yükleme başarısız.');
                $wrap.find('.store-od-receipt-status').removeClass('text-muted').addClass('text-success')
                    .text('Dekontunuz alındı. Ödeme onayı bekleniyor.');
                fileInput.value = '';
                if (res.body && res.body.receiptUrl) {
                    $wrap.find('.store-od-receipt-view').remove();
                    $wrap.prepend('<p class="small mb-2 store-od-receipt-view"><a href="' + res.body.receiptUrl + '" target="_blank" rel="noopener">Yüklenen dekontu görüntüle</a></p>');
                }
            })
            .catch(function (err) { alert(err.message || 'Yükleme başarısız.'); })
            .finally(function () { $btn.prop('disabled', false); });
    });

    $('#order-detail').on('click', '.store-od-track-btn', function () {
        var tracking = $(this).data('tracking');
        if (!tracking) return;
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(String(tracking)).then(function () {
                alert('Takip numarası kopyalandı: ' + tracking);
            }).catch(function () {
                alert('Takip numarası: ' + tracking);
            });
        } else {
            alert('Takip numarası: ' + tracking);
        }
    });

    loadOrders();
});
