(function ($) {
    'use strict';

    function idOf(x) { return x.id != null ? x.id : x.Id; }
    function nameOf(x) { return x.name || x.Name || ''; }

    function fetchCatalog(path) {
        return $.getJSON(path).then(function (res) {
            var ok = res && (res.status === true || res.Status === true);
            var data = res && (res.data || res.Data);
            if (ok && data) return data;
            return [];
        }).catch(function () { return []; });
    }

    function resetSelect($sel, placeholder) {
        $sel.empty().append($('<option>', { value: '', text: placeholder }));
        $sel.prop('disabled', true).val('');
    }

    function fillSelect($sel, items, getValue, getLabel) {
        var placeholder = $sel.data('placeholder') || $sel.find('option:first').text();
        $sel.empty().append($('<option>', { value: '', text: placeholder }));
        items.forEach(function (item) {
            var val = getValue(item);
            var label = getLabel(item);
            if (val != null && val !== '' && label) {
                $sel.append($('<option>', { value: String(val), text: label }));
            }
        });
        $sel.prop('disabled', items.length === 0);
    }

    function generationLabel(g) {
        var name = nameOf(g);
        var start = g.startYear != null ? g.startYear : g.StartYear;
        var end = g.endYear != null ? g.endYear : g.EndYear;
        if (start && end) return name + ' (' + start + '-' + end + ')';
        if (start) return name + ' (' + start + '+)';
        return name;
    }

    function engineLabel(e) {
        var parts = [e.engineCode || e.EngineCode || ''];
        var fuel = e.fuelType || e.FuelType;
        var hp = e.powerHp != null ? e.powerHp : e.PowerHp;
        if (fuel) parts.push(fuel);
        if (hp) parts.push(hp + ' HP');
        return parts.filter(Boolean).join(' · ');
    }

    function initPanel($panel) {
        var $make = $panel.find('[data-vehicle-make]');
        var $model = $panel.find('[data-vehicle-model]');
        var $gen = $panel.find('[data-vehicle-generation]');
        var $engine = $panel.find('[data-vehicle-engine]');
        var $btn = $panel.find('[data-vehicle-search-btn]');

        [$model, $gen, $engine].forEach(function ($s) {
            $s.data('placeholder', $s.find('option:first').text());
        });

        function updateSearchBtn() {
            $btn.prop('disabled', !$engine.val());
        }

        $make.on('change', function () {
            var makeId = $(this).val();
            resetSelect($model, $model.data('placeholder'));
            resetSelect($gen, $gen.data('placeholder'));
            resetSelect($engine, $engine.data('placeholder'));
            updateSearchBtn();
            if (!makeId) return;

            fetchCatalog('/vehicle-catalog/makes/' + makeId + '/models').then(function (items) {
                fillSelect($model, items, idOf, nameOf);
            });
        });

        $model.on('change', function () {
            var modelId = $(this).val();
            resetSelect($gen, $gen.data('placeholder'));
            resetSelect($engine, $engine.data('placeholder'));
            updateSearchBtn();
            if (!modelId) return;

            fetchCatalog('/vehicle-catalog/models/' + modelId + '/generations').then(function (items) {
                fillSelect($gen, items, idOf, generationLabel);
            });
        });

        $gen.on('change', function () {
            var genId = $(this).val();
            resetSelect($engine, $engine.data('placeholder'));
            updateSearchBtn();
            if (!genId) return;

            fetchCatalog('/vehicle-catalog/generations/' + genId + '/engines').then(function (items) {
                fillSelect($engine, items, idOf, engineLabel);
            });
        });

        $engine.on('change', updateSearchBtn);

        $btn.on('click', function () {
            var engineId = $engine.val();
            if (!engineId) return;
            var params = new URLSearchParams();
            params.set('vehicleEngineId', engineId);
            var makeId = $make.val();
            var modelId = $model.val();
            var genId = $gen.val();
            if (makeId) params.set('vehicleMakeId', makeId);
            if (modelId) params.set('vehicleModelId', modelId);
            if (genId) params.set('vehicleGenerationId', genId);
            window.location.href = '/arama?' + params.toString();
        });
    }

    $(function () {
        $('[data-home-vehicle-search]').each(function () {
            initPanel($(this));
        });
    });
})(jQuery);
