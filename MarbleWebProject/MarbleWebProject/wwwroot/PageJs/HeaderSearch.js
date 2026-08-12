(function () {
    'use strict';

    var MIN_LEN = 2;
    var DEBOUNCE_MS = 280;
    var LIMIT = 8;

    function escapeHtml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function groupHtml(title, items) {
        if (!items || !items.length) return '';
        var rows = items.map(function (item) {
            var sub = item.subLabel
                ? '<span class="header-search-suggest__sub">' + escapeHtml(item.subLabel) + '</span>'
                : '';
            return (
                '<a class="header-search-suggest__item" href="' + escapeHtml(item.url || '#') + '">' +
                '<span class="header-search-suggest__label">' + escapeHtml(item.label || '') + '</span>' +
                sub +
                '</a>'
            );
        }).join('');
        return (
            '<div class="header-search-suggest__group">' +
            '<div class="header-search-suggest__title">' + escapeHtml(title) + '</div>' +
            rows +
            '</div>'
        );
    }

    function renderSuggest(panel, data, query) {
        if (!panel) return;
        var brands = (data && data.brands) || [];
        var categories = (data && data.categories) || [];
        var products = (data && data.products) || [];
        var vehicles = (data && data.vehicles) || [];
        var html =
            groupHtml('Markalar', brands) +
            groupHtml('Kategoriler', categories) +
            groupHtml('Ürünler', products) +
            groupHtml('Araçlar', vehicles);

        if (!html) {
            html =
                '<div class="header-search-suggest__empty">"' +
                escapeHtml(query) +
                '" için sonuç yok. Enter ile tüm sonuçlara gidin.</div>';
        } else {
            html +=
                '<a class="header-search-suggest__all" href="/arama?q=' +
                encodeURIComponent(query) +
                '">Tüm sonuçları göster</a>';
        }

        panel.innerHTML = html;
        panel.hidden = false;
    }

    function hideSuggest(panel, input) {
        if (panel) {
            panel.hidden = true;
            panel.innerHTML = '';
        }
        if (input) input.setAttribute('aria-expanded', 'false');
    }

    function bindRoot(root) {
        var form = root.matches('form') ? root : root.querySelector('.js-header-search-form');
        var input = root.querySelector('.js-header-search-input');
        var panel = root.querySelector('.header-search-suggest');
        if (!form || !input || !panel) return;

        var timer = null;
        var abort = null;
        var seq = 0;

        function schedule(term) {
            if (timer) clearTimeout(timer);
            timer = setTimeout(function () { runSuggest(term); }, DEBOUNCE_MS);
        }

        function runSuggest(term) {
            var q = (term || '').trim();
            if (q.length < MIN_LEN) {
                hideSuggest(panel, input);
                return;
            }

            if (abort) abort.abort();
            abort = typeof AbortController !== 'undefined' ? new AbortController() : null;
            var mySeq = ++seq;
            input.setAttribute('aria-expanded', 'true');

            var url = '/catalog-search/suggest?q=' + encodeURIComponent(q) + '&limit=' + LIMIT;
            fetch(url, {
                headers: { Accept: 'application/json' },
                signal: abort ? abort.signal : undefined
            })
                .then(function (r) { return r.json(); })
                .then(function (payload) {
                    if (mySeq !== seq) return;
                    var data = payload && (payload.data || payload.Data) ? (payload.data || payload.Data) : null;
                    if (!data) {
                        hideSuggest(panel, input);
                        return;
                    }
                    // Normalize casing from API
                    data = {
                        brands: data.brands || data.Brands || [],
                        categories: data.categories || data.Categories || [],
                        products: data.products || data.Products || [],
                        vehicles: data.vehicles || data.Vehicles || []
                    };
                    ['brands', 'categories', 'products', 'vehicles'].forEach(function (key) {
                        data[key] = (data[key] || []).map(function (item) {
                            return {
                                label: item.label || item.Label || '',
                                subLabel: item.subLabel || item.SubLabel || '',
                                url: item.url || item.Url || '#'
                            };
                        });
                    });
                    renderSuggest(panel, data, q);
                })
                .catch(function (err) {
                    if (err && err.name === 'AbortError') return;
                    hideSuggest(panel, input);
                });
        }

        input.addEventListener('input', function () {
            schedule(input.value);
        });

        input.addEventListener('focus', function () {
            if ((input.value || '').trim().length >= MIN_LEN)
                schedule(input.value);
        });

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                hideSuggest(panel, input);
                input.blur();
            }
        });

        form.addEventListener('submit', function (e) {
            var q = (input.value || '').trim();
            if (q.length < MIN_LEN) {
                e.preventDefault();
                return;
            }
            hideSuggest(panel, input);
        });

        document.addEventListener('click', function (e) {
            if (!root.contains(e.target))
                hideSuggest(panel, input);
        });
    }

    function init() {
        document.querySelectorAll('[data-header-search]').forEach(bindRoot);
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', init);
    else
        init();
})();
