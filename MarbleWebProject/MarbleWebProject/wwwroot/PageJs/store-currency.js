(function (global) {
    'use strict';

    function getCurrencies() {
        return (global.MarbleStore && global.MarbleStore.currencies) || [];
    }

    function resolveCulture(displayLocale) {
        var locale = (displayLocale || 'tr-TR').trim();
        try {
            return new Intl.NumberFormat(locale).resolvedOptions().locale || locale;
        } catch (e) {
            return 'tr-TR';
        }
    }

    function usesBuiltInCurrencySymbol(priceFormat) {
        return /C|¤/i.test(priceFormat || '');
    }

    function joinWithSpace(left, right) {
        if (!left) return right || '';
        if (!right) return left;
        return left + ' ' + right;
    }

    function getLabel(currency, fallback) {
        if (!currency) return fallback || '';
        if (currency.symbol && String(currency.symbol).trim()) return String(currency.symbol).trim();
        if (currency.code && String(currency.code).trim()) return String(currency.code).trim();
        if (currency.title && String(currency.title).trim()) return String(currency.title).trim();
        return fallback || '';
    }

    function resolveCurrency(currencyLabel, currencyId) {
        var currencies = getCurrencies();
        if (!currencies.length) return null;

        if (currencyId != null && !isNaN(parseInt(currencyId, 10))) {
            var id = parseInt(currencyId, 10);
            for (var i = 0; i < currencies.length; i++) {
                if (parseInt(currencies[i].id, 10) === id) return currencies[i];
            }
        }

        if (currencyLabel) {
            var label = String(currencyLabel).trim().toLowerCase();
            for (var j = 0; j < currencies.length; j++) {
                var c = currencies[j];
                if ((c.symbol && String(c.symbol).trim().toLowerCase() === label)
                    || (c.code && String(c.code).trim().toLowerCase() === label)
                    || (c.title && String(c.title).trim().toLowerCase() === label)) {
                    return c;
                }
            }
        }

        return currencies.slice().sort(function (a, b) {
            return (parseInt(a.displayOrder, 10) || 0) - (parseInt(b.displayOrder, 10) || 0)
                || (parseInt(a.id, 10) || 0) - (parseInt(b.id, 10) || 0);
        })[0] || null;
    }

    function formatNumberWithPattern(amount, culture, priceFormat) {
        var format = (priceFormat || '{0:n2}').trim();
        var value = Number(amount);
        if (isNaN(value)) value = 0;

        if (usesBuiltInCurrencySymbol(format)) {
            var code = (currency && currency.code) ? String(currency.code).trim() : 'TRY';
            try {
                return new Intl.NumberFormat(culture, { style: 'currency', currency: code }).format(value);
            } catch (e) {
                return value.toLocaleString(culture, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            }
        }

        var decimals = 2;
        var match = format.match(/n(\d+)/i);
        if (match) decimals = parseInt(match[1], 10) || 2;

        return value.toLocaleString(culture, {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    }

    function formatStorePrice(amount, currencyLabel, currencyId) {
        var value = Number(amount);
        if (isNaN(value)) return '';

        var currency = resolveCurrency(currencyLabel, currencyId);
        var culture = resolveCulture(currency && currency.displayLocale);
        var priceFormat = (currency && currency.priceFormat) || '{0:n2}';
        var formattedAmount = formatNumberWithPattern(value, culture, priceFormat);

        if (usesBuiltInCurrencySymbol(priceFormat)) {
            return formattedAmount;
        }

        var symbol = getLabel(currency, currencyLabel);
        if (!symbol) return formattedAmount;

        return currency && currency.position
            ? joinWithSpace(formattedAmount, symbol)
            : joinWithSpace(symbol, formattedAmount);
    }

    global.formatStorePrice = formatStorePrice;
    global.resolveStoreCurrency = resolveCurrency;
})(window);
