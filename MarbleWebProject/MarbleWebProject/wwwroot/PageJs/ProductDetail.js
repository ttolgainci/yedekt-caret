(function ($) {
    'use strict';

    function destroyProductZoom() {
        var $img = $('#product-zoom');
        if (!$img.length) return;
        var ez = $img.data('elevateZoom');
        if (ez && typeof ez.closeAll === 'function') {
            try { ez.closeAll(); } catch (e) { /* ignore */ }
        }
        $('.zoomContainer').remove();
        $img.removeData('elevateZoom');
    }

    function bindGalleryButton() {
        var $img = $('#product-zoom');
        var ez = $img.data('elevateZoom');
        $('#btn-product-gallery').off('click.productZoom').on('click.productZoom', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if (!ez || !$.fn.magnificPopup) return;
            $.magnificPopup.open({
                items: ez.getGalleryList(),
                type: 'image',
                gallery: { enabled: true },
                fixedContentPos: false,
                removalDelay: 600,
                closeBtnInside: false
            }, 0);
        });
    }

    function initProductZoom() {
        var $img = $('#product-zoom');
        if (!$img.length || !$.fn.elevateZoom) return;

        destroyProductZoom();

        $img.elevateZoom({
            gallery: 'product-zoom-gallery',
            galleryActiveClass: 'active',
            zoomType: 'inner',
            cursor: 'crosshair',
            zoomWindowFadeIn: 400,
            zoomWindowFadeOut: 400,
            responsive: true
        });

        $('.product-gallery-item').off('click.productZoom').on('click.productZoom', function (e) {
            $('#product-zoom-gallery').find('a').removeClass('active');
            $(this).addClass('active');
            e.preventDefault();
        });

        bindGalleryButton();
    }

    function whenImageReady(callback) {
        var $img = $('#product-zoom');
        if (!$img.length) return;
        var el = $img[0];
        if (el.complete && el.naturalWidth > 0) {
            callback();
            return;
        }
        $img.one('load', callback);
        // CDN / cache race
        setTimeout(function () {
            if (el.complete) callback();
        }, 800);
    }

    $(function () {
        whenImageReady(function () {
            // main.js erken init etmiş olabilir; boyutlar netleşince yeniden kur
            setTimeout(initProductZoom, 50);
        });
    });

    $(window).on('load', function () {
        setTimeout(initProductZoom, 100);
    });
})(jQuery);
