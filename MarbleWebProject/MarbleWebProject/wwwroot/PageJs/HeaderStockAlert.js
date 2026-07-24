(function ($) {
    'use strict';

    var alerts = { wishlist: null, cart: null };
    var dismissed = { wishlist: false, cart: false };

    function findAlertRoot(scope) {
        return $('.js-header-stock-alert[data-scope="' + scope + '"]').first();
    }

    function normalizePayload(payload) {
        if (!payload) return null;

        var ctaUrl = payload.ctaUrl || payload.CtaUrl || null;
        var items = payload.items || payload.Items;
        if (Array.isArray(items) && items.length) {
            return {
                ctaUrl: ctaUrl,
                items: items.map(mapItem).filter(Boolean)
            };
        }

        // Eski tek-ürün formatı
        var single = mapItem(payload);
        if (!single) return null;
        return { ctaUrl: ctaUrl, items: [single] };
    }

    function mapItem(raw) {
        if (!raw) return null;
        var id = parseInt(raw.productID != null ? raw.productID : raw.ProductID, 10);
        var qty = parseInt(raw.stockQuantity != null ? raw.stockQuantity : raw.StockQuantity, 10);
        if (!id || isNaN(qty) || qty <= 0) return null;
        return {
            productID: id,
            name: raw.name || raw.Name || '',
            image: raw.image || raw.Image || '',
            url: raw.url || raw.Url || '#',
            stockQuantity: qty
        };
    }

    function readStoredAlert(scope) {
        if (alerts[scope]) return alerts[scope];
        var $root = findAlertRoot(scope);
        var raw = ($root.attr('data-alert-json') || '').trim();
        if (!raw) return null;
        try {
            alerts[scope] = normalizePayload(JSON.parse(raw));
            return alerts[scope];
        } catch (e) {
            return null;
        }
    }

    function escapeHtml(str) {
        return String(str || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function renderProducts($root, data) {
        var $list = $root.find('.js-stock-alert-products');
        var qtyTemplate = ($root.attr('data-qty-template') || 'Only {0} left').trim();
        var html = '';

        (data.items || []).forEach(function (item) {
            var url = item.url || '#';
            var name = escapeHtml(item.name);
            var image = escapeHtml(item.image);
            var qtyText = escapeHtml(qtyTemplate.replace('{0}', String(item.stockQuantity)));
            html +=
                '<div class="product">' +
                    '<div class="product-cart-details">' +
                        '<h4 class="product-title"><a href="' + escapeHtml(url) + '">' + name + '</a></h4>' +
                        '<span class="cart-product-info">' + qtyText + '</span>' +
                    '</div>' +
                    '<figure class="product-image-container">' +
                        '<a href="' + escapeHtml(url) + '" class="product-image">' +
                            '<img src="' + image + '" alt="' + name + '">' +
                        '</a>' +
                    '</figure>' +
                '</div>';
        });

        $list.html(html);
    }

    function applyAlertState(scope) {
        var $root = findAlertRoot(scope);
        if (!$root.length) return;

        if (dismissed[scope]) {
            $root.removeClass('has-items');
            return;
        }

        var data = readStoredAlert(scope);
        if (!data || !data.items || !data.items.length) {
            $root.removeClass('has-items');
            $root.find('.js-stock-alert-products').empty();
            return;
        }

        var defaultText = $root.attr('data-default-text') || '';
        var ctaUrl = data.ctaUrl || (scope === 'cart' ? '/cart' : '/wishlist');
        $root.find('.js-stock-alert-text').text(defaultText);
        $root.find('.js-stock-alert-cta').attr('href', ctaUrl);
        renderProducts($root, data);
        $root.addClass('has-items');
    }

    /** Sadece veriyi saklar — hover Molla dropdown CSS ile açılır. */
    window.setHeaderLowStockAlert = function (scope, payload) {
        if (scope !== 'wishlist' && scope !== 'cart') return;
        dismissed[scope] = false;
        alerts[scope] = normalizePayload(payload);
        var $root = findAlertRoot(scope);
        if ($root.length) {
            $root.attr('data-alert-json', alerts[scope] ? JSON.stringify(alerts[scope]) : '');
        }
        applyAlertState(scope);
    };

    window.clearHeaderLowStockAlert = function (scope) {
        if (scope !== 'wishlist' && scope !== 'cart') return;
        alerts[scope] = null;
        dismissed[scope] = false;
        var $root = findAlertRoot(scope);
        if ($root.length) $root.attr('data-alert-json', '');
        applyAlertState(scope);
    };

    $(document).on('click', '.js-stock-alert-cta', function (e) {
        var scope = $(this).closest('.js-header-stock-alert').attr('data-scope');
        if (scope === 'cart') {
            e.preventDefault();
            if (typeof window.openStoreCartDrawer === 'function') {
                window.openStoreCartDrawer();
            } else if (typeof window.openCartDrawer === 'function') {
                window.openCartDrawer();
            } else {
                $('.js-open-cart-drawer').first().trigger('click');
            }
        }
    });

    applyAlertState('wishlist');
    applyAlertState('cart');
})(jQuery);
