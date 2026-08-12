(function ($) {
    'use strict';

    var wishlistProductIds = [];

    function updateWishlistBadges(count) {
        var safeCount = parseInt(count, 10);
        if (isNaN(safeCount) || safeCount < 0) safeCount = 0;
        $('.wishlist-count').text(safeCount);
        if (safeCount > 0) {
            $('.wishlist-count').show();
        } else {
            $('.wishlist-count').hide();
        }
    }

    function setWishlistButtonState($btn, isActive) {
        if (!$btn || !$btn.length) return;
        var addLabel = $btn.data('add-label') || $btn.attr('data-add-label') || 'Add to Wishlist';
        var removeLabel = $btn.data('remove-label') || $btn.attr('data-remove-label') || 'Remove from Wishlist';
        var label = isActive ? removeLabel : addLabel;
        $btn.toggleClass('is-in-wishlist', !!isActive);
        $btn.removeAttr('title');
        $btn.attr('aria-label', label);
        var $label = $btn.find('span').first();
        if ($label.length) {
            $label.text(label);
        }
    }

    function syncWishlistButtons() {
        $('.js-toggle-wishlist').each(function () {
            var pid = parseInt($(this).data('pid'), 10);
            setWishlistButtonState($(this), wishlistProductIds.indexOf(pid) >= 0);
        });
    }

    function applySnapshot(data) {
        wishlistProductIds = (data && data.productIds) ? data.productIds.slice() : [];
        updateWishlistBadges(data ? data.totalCount : 0);
        syncWishlistButtons();
        if (typeof window.setHeaderLowStockAlert === 'function') {
            if (data && data.requiresLogin) {
                window.clearHeaderLowStockAlert('wishlist');
            } else {
                window.setHeaderLowStockAlert('wishlist', data && data.lowStockAlert ? data.lowStockAlert : null);
            }
        }
    }

    function openLoginModal() {
        var $modal = $('#signin-modal');
        if ($modal.length && typeof $modal.modal === 'function') {
            $modal.modal('show');
            return true;
        }
        window.location.href = '/#signin-modal';
        return false;
    }

    function ensureLoggedIn() {
        return $.ajax({ type: 'GET', url: '/account/me' }).then(function (result) {
            if (result && result.loggedIn) return true;
            openLoginModal();
            return false;
        }).catch(function () {
            openLoginModal();
            return false;
        });
    }

    function loadWishlistSnapshot() {
        return $.getJSON('/Wishlist/Snapshot').then(function (data) {
            if (data && data.requiresLogin) {
                applySnapshot({ totalCount: 0, productIds: [] });
                return data;
            }
            applySnapshot(data);
            return data;
        }).catch(function () {
            applySnapshot({ totalCount: 0, productIds: [] });
        });
    }

    window.toggleWishlist = function (el) {
        var $btn = $(el);
        var pid = parseInt($btn.data('pid'), 10);
        var purl = ($btn.data('purl') || '').toString();
        if (!pid) return false;

        ensureLoggedIn().then(function (ok) {
            if (!ok) return;

            $btn.addClass('is-loading');
            $.ajax({
                type: 'POST',
                url: '/Wishlist/Toggle',
                data: { productId: pid, url: purl }
            }).done(function (res) {
                wishlistProductIds = (res.productIds || []).slice();
                updateWishlistBadges(res.totalCount);
                setWishlistButtonState($btn, !!res.isInWishlist);
                syncWishlistButtons();
            }).fail(function (xhr) {
                if (xhr && xhr.status === 401) openLoginModal();
            }).always(function () {
                $btn.removeClass('is-loading');
            });
        });

        return false;
    };

    window.refreshWishlistSnapshot = loadWishlistSnapshot;

    $(function () {
        loadWishlistSnapshot();

        if ($('body').attr('data-wishlist-requires-login') === 'true') {
            openLoginModal();
        }

        $(document).on('click', '.js-header-wishlist', function (e) {
            var href = $(this).attr('href') || '/wishlist';
            e.preventDefault();
            ensureLoggedIn().then(function (ok) {
                if (ok) window.location.href = href;
            });
        });

        $(document).on('click', '.js-wishlist-remove', function (e) {
            e.preventDefault();
            var pid = parseInt($(this).data('pid'), 10);
            if (!pid) return;

            var $row = $(this).closest('.wishlist-product-item');
            ensureLoggedIn().then(function (ok) {
                if (!ok) return;
                $.post('/Wishlist/Remove', { productId: pid }).done(function (res) {
                    wishlistProductIds = (res.productIds || []).slice();
                    updateWishlistBadges(res.totalCount);
                    $row.remove();
                    syncWishlistButtons();
                    if (window.StoreWishlistFilters && typeof window.StoreWishlistFilters.refresh === 'function') {
                        window.StoreWishlistFilters.refresh();
                    }
                    if (!res.totalCount) {
                        $('#wishlist-page-content').hide();
                        $('#wishlist-empty-state').prop('hidden', false).show();
                    }
                }).fail(function (xhr) {
                    if (xhr && xhr.status === 401) openLoginModal();
                });
            });
        });
    });
})(jQuery);
