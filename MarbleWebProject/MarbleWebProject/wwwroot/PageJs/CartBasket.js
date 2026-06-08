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

function getCartDrawerLabels() {
    var el = document.getElementById('store-cart-drawer');
    if (!el) {
        return {
            total: 'Total',
            continueShopping: 'Continue Shopping',
            viewCart: 'View Cart',
            checkout: 'Proceed to checkout',
            emptyCart: 'Empty cart',
            remove: 'Remove',
            empty: 'Your cart is empty.',
            itemsTemplate: 'Sepetinizde {0} ürün bulunmaktadır'
        };
    }
    return {
        total: el.getAttribute('data-label-total') || 'Total',
        continueShopping: el.getAttribute('data-label-continue') || 'Continue Shopping',
        viewCart: el.getAttribute('data-label-view-cart') || 'View Cart',
        checkout: el.getAttribute('data-label-checkout') || 'Proceed to checkout',
        emptyCart: el.getAttribute('data-label-empty-cart') || 'Empty cart',
        remove: el.getAttribute('data-label-remove') || 'Remove',
        empty: el.getAttribute('data-label-empty') || 'Your cart is empty.',
        itemsTemplate: el.getAttribute('data-label-items-template') || 'Sepetinizde {0} ürün bulunmaktadır'
    };
}

function formatItemsSubtitle(template, count) {
    var tpl = template || 'Sepetinizde {0} ürün bulunmaktadır';
    if (tpl.indexOf('{0}') >= 0) {
        return tpl.replace('{0}', String(count));
    }
    return tpl + ' ' + count;
}

function updateCartDrawerSubtitle(totalQuantity) {
    var labels = getCartDrawerLabels();
    var $sub = $('#store-cart-drawer-subtitle');
    if (!$sub.length) return;
    if (!totalQuantity || totalQuantity < 1) {
        $sub.text(labels.empty);
        return;
    }
    $sub.text(formatItemsSubtitle(labels.itemsTemplate, totalQuantity));
}

function cartItemPid(item) {
    return item.productID || item.ProductID || 0;
}

function cartItemQty(item) {
    return parseInt(item.quantity || item.Quantity, 10) || 0;
}

function cartItemCurrency(item) {
    if (!item) return '';
    return item.currency
        || item.Currency
        || item.currencyName
        || item.CurrencyName
        || '';
}

function formatDrawerTotalHtml(dataTotal) {
    if (!dataTotal) return '';
    var priceHtml = dataTotal.totalPrice || dataTotal.TotalPrice || dataTotal.subtotalPrice || dataTotal.SubtotalPrice || '';
    var currency = dataTotal.currencyName || dataTotal.CurrencyName || '';
    if (!priceHtml) return '';
    if (currency && priceHtml.indexOf(currency) === -1) {
        return priceHtml + ' ' + currency;
    }
    return priceHtml;
}

function mergeCartLines(data) {
    if (!data || !data.length) return [];
    var map = {};
    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var pid = cartItemPid(item);
        if (!pid) continue;
        if (!map[pid]) {
            map[pid] = {
                productID: pid,
                name: item.name || item.Name || '',
                price: item.price != null ? item.price : item.Price,
                currency: cartItemCurrency(item),
                image: item.image || item.Image || '',
                url: item.url || item.Url || '',
                quantity: 0
            };
        }
        map[pid].quantity += cartItemQty(item);
    }
    var merged = [];
    for (var key in map) {
        if (Object.prototype.hasOwnProperty.call(map, key)) {
            merged.push(map[key]);
        }
    }
    return merged;
}

function formatLineMoney(value) {
    var n = parseFloat(value);
    if (isNaN(n)) return '0.00';
    return n.toFixed(2);
}

function buildCartDrawerHtml(data, dataTotal) {
    var labels = getCartDrawerLabels();
    var str = '';
    var lines = mergeCartLines(data);

    if (!lines.length) {
        str += "<div class='store-cart-drawer-empty'><p>" + labels.empty + "</p></div>";
        return str;
    }

    str += "<div class='store-cart-lines basket-list'>";
    for (var i = 0; i < lines.length; i++) {
        var item = lines[i];
        var qty = parseInt(item.quantity, 10) || 1;
        var unitPrice = parseFloat(item.price) || 0;
        var lineTotal = unitPrice * qty;
        var url = item.url || '#';
        if (url.charAt(0) !== '/') url = '/' + url.replace(/^\/+/, '');

        str += "<div class='store-cart-line cartProductItem basket-product-code_" + item.productID + "' data-pid='" + item.productID + "' data-url='" + url + "' data-qty='" + qty + "'>";
        str += "<a href='" + url + "' class='store-cart-line-image'>";
        str += "<img src='" + resolveMediaUrl(item.image) + "' alt=''>";
        str += "</a>";
        str += "<div class='store-cart-line-info'>";
        str += "<a href='" + url + "' class='store-cart-line-title'>" + item.name + "</a>";
        str += "<div class='store-cart-line-unit'><span class='cartProductPrice basket-product-price'>" + formatLineMoney(unitPrice) + "</span> " + (item.currency || '') + "</div>";
        str += "<div class='store-cart-line-qty'>";
        str += "<button type='button' class='store-cart-qty-btn js-cart-qty-minus' aria-label='-'>−</button>";
        str += "<span class='store-cart-qty-value'>" + qty + "</span>";
        str += "<button type='button' class='store-cart-qty-btn js-cart-qty-plus' aria-label='+'>+</button>";
        str += "</div></div>";
        str += "<div class='store-cart-line-total'>" + formatLineMoney(lineTotal) + " " + (item.currency || '') + "</div>";
        str += "<button type='button' class='store-cart-line-remove js-cart-line-remove' data-pid='" + item.productID + "' title='" + labels.remove + "'><i class='icon-close'></i></button>";
        str += "</div>";
    }
    str += "</div>";
    str += "<div class='dropdown-cart-total'>";
    str += "<span>" + labels.total + "</span>";
    str += "<span class='cart-total-price'>" + formatDrawerTotalHtml(dataTotal) + "</span>";
    str += "</div>";
    str += "<div class='dropdown-cart-action store-cart-drawer-actions'>";
    str += "<a href='/checkout' class='btn btn-outline-primary-2 btn-block store-cart-drawer-checkout'>" + labels.checkout + "</a>";
    str += "<a href='/cart' class='btn btn-primary btn-block store-cart-drawer-view-cart'>" + labels.viewCart + "</a>";
    str += "<a href='#' class='btn btn-outline-primary-2 btn-block js-cart-drawer-continue'><span>" + labels.continueShopping + "</span></a>";
    str += "</div>";
    return str;
}

function updateCartDrawerFooter(totalQty) {
    var $footer = $('#store-cart-drawer-footer');
    if (!$footer.length) return;
    if (totalQty > 0) {
        $footer.removeClass('is-hidden');
    } else {
        $footer.addClass('is-hidden');
    }
}

var cartClearPending = false;

function clearCartDrawer() {
    if (cartClearPending) return;
    cartClearPending = true;

    $.ajax({
        type: 'POST',
        url: '/Cart/ClearCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (result) {
            updateCartFromResponse(result);
        },
        error: function () {
            $('#store-cart-drawer-body').html(
                "<div class='store-cart-drawer-empty'><p>" + getCartDrawerLabels().empty + "</p></div>"
            );
            updateCartDrawerFooter(0);
            updateCartDrawerSubtitle(0);
            $('.basket-quantity-count').empty().hide();
        },
        complete: function () {
            cartClearPending = false;
        }
    });
}

var cartQtyRequestPending = false;

function changeCartLineQuantity(productId, url, delta) {
    if (cartQtyRequestPending) return;
    cartQtyRequestPending = true;

    $.ajax({
        type: 'POST',
        url: '/Cart/ChangeCartQuantity',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: productId, Url: url, Delta: delta },
        success: function (result) {
            updateCartFromResponse(result);
        },
        complete: function () {
            cartQtyRequestPending = false;
        }
    });
}

function updateShippingSummary(info) {
    if (!info) return;

    if (info.subtotalPrice) {
        $('.cart-summary-subtotal').html(info.subtotalPrice);
    }

    var currency = info.currencyName || info.CurrencyName || '';

    var $shipCell = $('.cart-summary-shipping');
    if ($shipCell.length) {
        if (info.shippingPrice != null && !isNaN(parseFloat(info.shippingPrice))) {
            var shipText = parseFloat(info.shippingPrice).toFixed(2);
            if (currency) {
                shipText += ' ' + currency;
            }
            $shipCell.text(shipText);
        } else {
            $shipCell.html('<span>—</span>');
        }
    }

    var meta = '';
    if (info.carrierName) {
        meta = info.carrierName;
        if (info.totalDesi != null && !isNaN(parseFloat(info.totalDesi))) {
            meta += ' · ' + parseFloat(info.totalDesi).toFixed(2) + ' desi';
        }
    }
    var $metaRow = $('#cart-summary-shipping-meta-row');
    if ($metaRow.length) {
        $('.cart-summary-shipping-meta').text(meta);
        if (meta) {
            $metaRow.removeClass('d-none');
        } else {
            $metaRow.addClass('d-none');
        }
    }

    if (info.totalPrice) {
        $('.cart-summary-grandtotal').html(info.totalPrice);
        $('.cart-table-total .basket-total-price').html(info.totalPrice);
    }
}

function updateCartFromResponse(result) {
    if (!result) return;
    var data = mergeCartLines(result.cartList || []);
    var dataTotal = result.info || {};
    var html = buildCartDrawerHtml(data, dataTotal);
    $('#store-cart-drawer-body').html(html);

    var totalQty = dataTotal.totalQuantity ? parseInt(dataTotal.totalQuantity, 10) : 0;
    if (!totalQty && data.length) {
        totalQty = 0;
        for (var i = 0; i < data.length; i++) {
            totalQty += cartItemQty(data[i]);
        }
    }

    if (totalQty > 0) {
        $('.basket-quantity-count').text(totalQty).show();
    } else {
        $('.basket-quantity-count').empty().hide();
    }

    updateCartDrawerSubtitle(totalQty);
    updateCartDrawerFooter(totalQty);
    updateShippingSummary(dataTotal);
}

function openStoreCartDrawer() {
    $('body').addClass('store-cart-drawer-open');
    $('#store-cart-drawer').attr('aria-hidden', 'false');
    $('#store-cart-drawer-overlay').attr('aria-hidden', 'false');
}

function closeStoreCartDrawer() {
    $('body').removeClass('store-cart-drawer-open');
    $('#store-cart-drawer').attr('aria-hidden', 'true');
    $('#store-cart-drawer-overlay').attr('aria-hidden', 'true');
}

function setProductToBasket(d) {
    var pID = d.getAttribute("data-pid");
    var qty = $('.product-detail-code_' + pID + ' input[type="number"].product-detail-Quantity').val();
    if (!qty || isNaN(parseInt(qty, 10))) {
        qty = d.getAttribute("data-qty") || '1';
    }
    var cartQty = parseInt(qty, 10);
    if (isNaN(cartQty) || cartQty < 1) {
        cartQty = 1;
    }
    var purl = d.getAttribute("data-purl");
    var inBasketLabel = d.getAttribute("data-inbasket-label") || '';

    $.ajax({
        type: 'POST',
        url: '/Cart/AddToCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID, Url: purl, CartQuantity: cartQty },
        success: function (result) {
            updateCartFromResponse(result);
            openStoreCartDrawer();

            var addLabel = d.getAttribute("data-add-label") || "";
            if (inBasketLabel && addLabel) {
                var $btnLabels = $();
                var $detailBtn = $('.product-detail-code_' + pID + ' .btn-product.btn-cart span');
                var $listBtn = $(d).find('span');
                if ($detailBtn.length) {
                    $btnLabels = $btnLabels.add($detailBtn);
                }
                if ($listBtn.length) {
                    $btnLabels = $btnLabels.add($listBtn);
                }
                $btnLabels.text(inBasketLabel);
                setTimeout(function () {
                    $btnLabels.text(addLabel);
                }, 1000);
            }
        },
        error: function () { }
    });
}

function removeProductFromBasket(d) {
    var $el = $(d);
    var $line = $el.closest('.store-cart-line');
    var pID = parseInt($line.attr('data-pid') || $el.attr('data-pid'), 10);
    if (!pID) return;

    $.ajax({
        type: 'POST',
        url: '/Cart/DeleteFromCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID },
        success: function (result) {
            updateCartFromResponse(result);
        }
    });
}

function removeProductFromCart(d) {
    var pID = d.getAttribute("data-pid");

    $.ajax({
        type: 'POST',
        url: '/Cart/DeleteFromCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID },
        success: function (result) {
            updateCartFromResponse(result);
            $('.cart-product-code_' + pID).remove();
        },
        error: function () { }
    });
}

$(function () {
    $(document).on('click', '.js-open-cart-drawer', function (e) {
        e.preventDefault();
        openStoreCartDrawer();
    });

    $(document).on('click', '.store-cart-drawer-close, .store-cart-drawer-overlay, .js-cart-drawer-continue', function (e) {
        e.preventDefault();
        closeStoreCartDrawer();
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $('body').hasClass('store-cart-drawer-open')) {
            closeStoreCartDrawer();
        }
    });

    $(document).on('click', '.js-cart-line-remove', function (e) {
        e.preventDefault();
        e.stopPropagation();
        removeProductFromBasket(this);
    });

    $(document).on('click', '.js-cart-qty-plus', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $line = $(this).closest('.store-cart-line');
        var pid = parseInt($line.attr('data-pid'), 10);
        var url = $line.attr('data-url') || '';
        if (!pid) return;
        changeCartLineQuantity(pid, url, 1);
    });

    $(document).on('click', '.js-cart-qty-minus', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $line = $(this).closest('.store-cart-line');
        var pid = parseInt($line.attr('data-pid'), 10);
        var url = $line.attr('data-url') || '';
        if (!pid) return;
        changeCartLineQuantity(pid, url, -1);
    });

    $(document).on('click', '.js-cart-empty-cart', function (e) {
        e.preventDefault();
        clearCartDrawer();
    });
});

(function () {
    $('body', document)
        .on('click', '.cartBasketList .icon-plus', function () {
            var mainDivId = $(this).closest('.cartProductItem').attr('id').split("_")[1];
            var getHasQty = $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val();
            var getQty = $('.cart-product-code_' + mainDivId + ' input[type="number"].cart-product-quantity-new').val();
            var getUrl = $('.cart-product-code_' + mainDivId + ' .cart-product-url').attr("href");

            if (parseInt(getQty, 10) > parseInt(getHasQty, 10)) {
                $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val(parseInt(getQty, 10));
            }
            $.ajax({
                type: 'POST',
                url: '/Cart/AddToCart',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: { ProductID: mainDivId, Url: getUrl, CartQuantity: getQty },
                success: function (result) {
                    updateCartFromResponse(result);
                }
            });
        })
        .on('click', '.cartBasketList .icon-minus', function () {
            var mainDivId = $(this).closest('.cartProductItem').attr('id').split("_")[1];
            var getHasQty = $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val();
            var getQty = $('.cart-product-code_' + mainDivId + ' input[type="number"].cart-product-quantity-new').val();
            var getUrl = $('.cart-product-code_' + mainDivId + ' .cart-product-url').attr("href");
            if (parseInt(getQty, 10) < parseInt(getHasQty, 10)) {
                $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val(parseInt(getQty, 10));
            }
            $.ajax({
                type: 'POST',
                url: '/Cart/AddToCart',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: { ProductID: mainDivId, Url: getUrl, CartQuantity: getQty },
                success: function (result) {
                    updateCartFromResponse(result);
                }
            });
        });
})();

function reloadCartFromServer() {
    $.getJSON('/Cart/Snapshot')
        .done(function (result) {
            if (result) {
                updateCartFromResponse(result);
            }
        });
}

window.reloadCartFromServer = reloadCartFromServer;
