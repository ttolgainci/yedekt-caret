(function ($) {
    'use strict';

    var state = {
        statusFilter: 'all',
        categoryIds: []
    };

    function $items() {
        return $('#wishlist-products .wishlist-product-item');
    }

    function matches($item) {
        if (state.statusFilter === 'price-drop' && $item.attr('data-has-discount') !== 'true') {
            return false;
        }
        if (state.statusFilter === 'low-stock' && $item.attr('data-low-stock') !== 'true') {
            return false;
        }
        if (state.statusFilter === 'in-stock' && $item.attr('data-in-stock') !== 'true') {
            return false;
        }
        if (state.categoryIds.length > 0) {
            var categoryId = ($item.attr('data-category-id') || '').toString();
            if (state.categoryIds.indexOf(categoryId) < 0) {
                return false;
            }
        }
        return true;
    }

    function applyFilters() {
        var visibleCount = 0;
        $items().each(function () {
            var $item = $(this);
            var show = matches($item);
            $item.toggle(show);
            if (show) {
                visibleCount++;
            }
        });

        var hasItems = $items().length > 0;
        var hasActiveFilter = state.statusFilter !== 'all' || state.categoryIds.length > 0;
        $('#wishlist-filter-empty').prop('hidden', !(hasItems && hasActiveFilter && visibleCount === 0));
        $('#wishlist-products').toggle(!(hasItems && hasActiveFilter && visibleCount === 0));
    }

    function setStatusFilter(filter) {
        state.statusFilter = filter || 'all';
        var $tabs = $('[data-store-wishlist-status-filter] li');
        $tabs.removeClass('active');
        $tabs.filter('[data-filter="' + state.statusFilter + '"]').addClass('active');
        applyFilters();
    }

    function readCategoryFilters() {
        state.categoryIds = [];
        $('.js-wishlist-category-filter:checked').each(function () {
            state.categoryIds.push(($(this).val() || '').toString());
        });
        applyFilters();
    }

    function clearFilters() {
        state.statusFilter = 'all';
        state.categoryIds = [];
        $('.js-wishlist-category-filter').prop('checked', false);
        setStatusFilter('all');
    }

    function bindToolbox() {
        $('.store-wishlist-page .filter-toggler').on('click', function (e) {
            e.preventDefault();
            $(this).toggleClass('active');
            $('.store-wishlist-page .store-wishlist-status-filter').fadeToggle('fast');
            $('#wishlist-filter-area').slideToggle(500);
        });
    }

    function bindStatusTabs() {
        $(document).on('click', '[data-store-wishlist-status-filter] a', function (e) {
            e.preventDefault();
            var $li = $(this).closest('li');
            var filter = ($li.attr('data-filter') || 'all').toString();
            if ($li.hasClass('active') && filter !== 'all') {
                setStatusFilter('all');
                return;
            }
            setStatusFilter(filter);
        });
    }

    function bindCategoryFilters() {
        $(document).on('change', '.js-wishlist-category-filter', readCategoryFilters);
    }

    function bindClearButtons() {
        $(document).on('click', '[data-store-wishlist-clear]', function (e) {
            e.preventDefault();
            clearFilters();
        });

        $('.store-wishlist-page .widget-filter-clear').on('click', function (e) {
            e.preventDefault();
            clearFilters();
        });
    }

    window.StoreWishlistFilters = {
        refresh: applyFilters,
        clear: clearFilters
    };

    $(function () {
        if (!$('.store-wishlist-page').length) {
            return;
        }
        bindToolbox();
        bindStatusTabs();
        bindCategoryFilters();
        bindClearButtons();
        applyFilters();
    });
})(jQuery);
