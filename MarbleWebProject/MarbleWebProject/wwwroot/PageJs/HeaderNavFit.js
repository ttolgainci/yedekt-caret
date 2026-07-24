(function () {
    'use strict';

    var resizeTimer = null;

    function getMainItems(row) {
        var menu = row.querySelector('.store-header-main-nav .main-nav > .menu');
        if (!menu) {
            return [];
        }
        return Array.prototype.slice.call(menu.children).filter(function (el) {
            return el.nodeType === 1 && el.hasAttribute('data-nav-id');
        });
    }

    function getOverflowItems(browse) {
        var list = browse && browse.querySelector('[data-store-nav-overflow]');
        if (!list) {
            return [];
        }
        return Array.prototype.slice.call(list.children).filter(function (el) {
            return el.nodeType === 1 && el.hasAttribute('data-nav-id');
        });
    }

    function itemGap(items) {
        if (items.length < 2) {
            return 0;
        }
        var style = window.getComputedStyle(items[1]);
        return parseFloat(style.marginLeft) || 0;
    }

    function findOverflowItem(overflowItems, id) {
        for (var i = 0; i < overflowItems.length; i++) {
            if (overflowItems[i].getAttribute('data-nav-id') === id) {
                return overflowItems[i];
            }
        }
        return null;
    }

    function fitHeaderNav() {
        var row = document.querySelector('.store-header-nav-row');
        if (!row) {
            return;
        }

        var mainItems = getMainItems(row);
        var browse = row.querySelector('[data-store-browse-categories]');
        var overflowItems = getOverflowItems(browse);
        if (!mainItems.length) {
            if (browse) {
                browse.hidden = true;
            }
            return;
        }

        mainItems.forEach(function (li) {
            li.hidden = false;
        });
        overflowItems.forEach(function (li) {
            li.hidden = true;
        });
        if (browse) {
            browse.hidden = true;
            browse.style.visibility = '';
        }

        var gap = itemGap(mainItems);
        var widths = mainItems.map(function (li) {
            return li.getBoundingClientRect().width;
        });
        var total = widths.reduce(function (sum, w, index) {
            return sum + w + (index > 0 ? gap : 0);
        }, 0);

        if (total <= row.clientWidth) {
            return;
        }

        if (!browse) {
            return;
        }

        browse.hidden = false;
        browse.style.visibility = 'hidden';
        var browseWidth = browse.getBoundingClientRect().width;
        browse.style.visibility = '';

        var available = Math.max(0, row.clientWidth - browseWidth - gap);
        var used = 0;
        var fitCount = 0;

        for (var i = 0; i < widths.length; i++) {
            var next = used + widths[i] + (i > 0 ? gap : 0);
            if (next > available) {
                break;
            }
            used = next;
            fitCount++;
        }

        if (fitCount >= mainItems.length) {
            browse.hidden = true;
            mainItems.forEach(function (li) {
                li.hidden = false;
            });
            overflowItems.forEach(function (li) {
                li.hidden = true;
            });
            return;
        }

        for (var j = 0; j < mainItems.length; j++) {
            var id = mainItems[j].getAttribute('data-nav-id');
            var overflowLi = findOverflowItem(overflowItems, id);
            var isVisible = j < fitCount;
            mainItems[j].hidden = !isVisible;
            if (overflowLi) {
                overflowLi.hidden = isVisible;
            }
        }

        browse.hidden = false;
    }

    function scheduleFit() {
        window.clearTimeout(resizeTimer);
        resizeTimer = window.setTimeout(fitHeaderNav, 50);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', fitHeaderNav);
    } else {
        fitHeaderNav();
    }

    window.addEventListener('resize', scheduleFit);

    if (document.fonts && document.fonts.ready) {
        document.fonts.ready.then(fitHeaderNav).catch(function () { /* ignore */ });
    }

    window.StoreHeaderNavFit = fitHeaderNav;
})();
