(function () {
    function getOverlay() {
        return document.getElementById('store-page-loading');
    }

    function resolveLogoSrc() {
        var headerLogo = document.querySelector('.header .logo img');
        if (headerLogo && headerLogo.getAttribute('src')) {
            return headerLogo.getAttribute('src');
        }
        var overlay = getOverlay();
        return overlay ? overlay.getAttribute('data-logo-src') || '' : '';
    }

    function showStorePageLoading() {
        var overlay = getOverlay();
        if (!overlay) return;

        var img = overlay.querySelector('.store-page-loading-logo');
        var logoSrc = resolveLogoSrc();
        if (img && logoSrc) {
            img.setAttribute('src', logoSrc);
        }

        overlay.classList.add('is-visible');
        overlay.setAttribute('aria-hidden', 'false');
        document.body.classList.add('store-page-loading-active');
    }

    function hideStorePageLoading() {
        var overlay = getOverlay();
        if (!overlay) return;
        overlay.classList.remove('is-visible');
        overlay.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('store-page-loading-active');
    }

    function navigateWithLoading(href) {
        if (!href || href === '#') return;
        showStorePageLoading();
        window.location.href = href;
    }

    window.showStorePageLoading = showStorePageLoading;
    window.hideStorePageLoading = hideStorePageLoading;
    window.navigateWithStoreLoading = navigateWithLoading;

    $(function () {
        $(document).on('click', 'a.js-store-page-load', function (e) {
            var $link = $(this);
            var href = ($link.attr('href') || '').trim();
            if (!href || href === '#' || $link.attr('target') === '_blank') return;
            e.preventDefault();
            navigateWithLoading(href);
        });

        $(document).on('click', 'a.store-cart-drawer-checkout, a.btn-order[href="/checkout"]', function (e) {
            var href = ($(this).attr('href') || '').trim();
            if (!href || href === '#') return;
            e.preventDefault();
            navigateWithLoading(href);
        });

        $(document).on('click', 'a.store-cart-drawer-view-cart[href="/cart"]', function (e) {
            e.preventDefault();
            navigateWithLoading('/cart');
        });
    });

    window.addEventListener('pageshow', function () {
        hideStorePageLoading();
    });
})();
