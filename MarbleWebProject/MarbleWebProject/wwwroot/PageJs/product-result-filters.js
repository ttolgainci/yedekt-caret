(function () {
    var scrollKey = 'aramaSidebarScrollY';

    function saveScroll() {
        try {
            sessionStorage.setItem(scrollKey, String(window.scrollY || 0));
        } catch (e) { }
    }

    function goFilter(url) {
        if (!url) return;
        saveScroll();
        window.location.href = url;
    }

    document.querySelectorAll('.filter-item--toggle').forEach(function (row) {
        row.addEventListener('click', function (e) {
            if (e.target.closest('.item-count')) return;
            e.preventDefault();
            goFilter(row.getAttribute('data-filter-url'));
        });
    });

    document.querySelectorAll('.sidebar-filter-clear').forEach(function (el) {
        el.addEventListener('click', saveScroll);
    });

    initPriceSlider();

    try {
        var saved = sessionStorage.getItem(scrollKey);
        if (saved !== null) {
            sessionStorage.removeItem(scrollKey);
            var y = parseInt(saved, 10);
            if (!isNaN(y) && y >= 0) {
                window.scrollTo(0, y);
            }
        }
    } catch (e) { }

    function initPriceSlider() {
        if (typeof noUiSlider !== 'object' || typeof wNumb !== 'function') return;

        var root = document.getElementById('arama-price-filter');
        var sliderEl = document.getElementById('arama-price-slider');
        var rangeText = document.getElementById('arama-filter-price-range');
        if (!root || !sliderEl || !rangeText) return;

        var rangeMin = parseFloat(root.getAttribute('data-range-min'));
        var rangeMax = parseFloat(root.getAttribute('data-range-max'));
        var startMin = parseFloat(root.getAttribute('data-start-min'));
        var startMax = parseFloat(root.getAttribute('data-start-max'));
        var currency = root.getAttribute('data-currency') || '₺';
        var baseUrl = root.getAttribute('data-url-without-price') || '';

        if (isNaN(rangeMin) || isNaN(rangeMax) || rangeMax <= rangeMin) return;
        if (isNaN(startMin)) startMin = rangeMin;
        if (isNaN(startMax)) startMax = rangeMax;
        startMin = Math.max(rangeMin, Math.min(startMin, rangeMax));
        startMax = Math.max(startMin, Math.min(startMax, rangeMax));

        var span = rangeMax - rangeMin;
        var step = span <= 100 ? 1 : span <= 500 ? 10 : 50;

        noUiSlider.create(sliderEl, {
            start: [startMin, startMax],
            connect: true,
            step: step,
            range: {
                min: rangeMin,
                max: rangeMax
            },
            tooltips: [true, true],
            format: wNumb({
                decimals: 0,
                suffix: ' ' + currency
            })
        });

        function updateRangeLabel(values) {
            rangeText.textContent = values.join(' - ');
        }

        function buildPriceUrl(minVal, maxVal) {
            var min = Math.round(parseFloat(minVal));
            var max = Math.round(parseFloat(maxVal));
            if (isNaN(min) || isNaN(max)) return baseUrl;
            if (min <= rangeMin && max >= rangeMax) return baseUrl;

            var sep = baseUrl.indexOf('?') >= 0 ? '&' : '?';
            return baseUrl + sep +
                'minPrice=' + encodeURIComponent(min.toFixed(0)) +
                '&maxPrice=' + encodeURIComponent(max.toFixed(0));
        }

        sliderEl.noUiSlider.on('update', function (values) {
            updateRangeLabel(values);
        });

        updateRangeLabel(sliderEl.noUiSlider.get());

        sliderEl.noUiSlider.on('set', function (values) {
            var url = buildPriceUrl(values[0], values[1]);
            if (url && url !== window.location.pathname + window.location.search) {
                goFilter(url);
            }
        });
    }
})();
