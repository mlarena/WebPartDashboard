/**
 * dashboard.core.js
 * Ядро системы веб-частей. Управляет сеткой GridStack и жизненным циклом загрузки.
 */

let grid = null;

$(document).ready(function() {
    DashboardCore.init();
});

const DashboardCore = {
    init: function() {
        this.initCharts();
        this.initGridStack();
        this.setupGlobalEvents();
        
        // Ленивая загрузка контента всех веб-частей
        setTimeout(() => this.loadAllWebParts(), 200);
    },

    initCharts: function() {
        if (typeof Chart !== 'undefined' && Chart.registerables) {
            Chart.register(...Chart.registerables);
            console.log('Chart.js initialized');
        }
    },

    initGridStack: function() {
        if ($('#dashboard-grid').length) {
            grid = GridStack.init({
                cellHeight: '120px',
                verticalMargin: 15,
                minRow: 3,
                disableDrag: false,
                disableResize: false,
                draggable: { handle: '.card-header', scroll: true, appendTo: 'body' },
                resizable: { handles: 'e, se, s, sw, w' },
                animate: true
            });
            
            grid.on('change', (event, items) => {
                if (!items || !Array.isArray(items)) return;
                items.forEach(item => {
                    if (item.id) {
                        this.api.updatePosition(item.id, item.x, item.y);
                        this.api.updateSize(item.id, item.w, item.h);
                    }
                });
            });
        }
    },

    setupGlobalEvents: function() {
        $(document).ajaxError((event, xhr, settings, error) => {
            console.error('AJAX Error:', error, settings.url);
        });
    },

    loadAllWebParts: function() {
        $('.grid-stack-item[gs-id]').each((index, el) => {
            const id = $(el).attr('gs-id');
            if (id) this.loadWebPartData(parseInt(id));
        });
    },

    loadWebPartData: function(id) {
        const $card = $(`#webpart-${id}`);
        const type = $card.attr('data-type');
        const $container = $card.find('.webpart-content');

        if (!type) return;

        // 1. HTML-веб-части (Сложные CRUD интерфейсы)
        if (['Tasks', 'Monitoring'].includes(type)) {
            this.renderers.loadHtml($container, type);
        } 
        // 2. JSON-веб-части (Графики, Таблицы, Информеры)
        else {
            $.get(`/Dashboard/GetWebPartData?id=${id}`, (data) => {
                this.renderers.renderJson($container, type, data);
            }).fail(() => {
                $container.html('<div class="alert alert-danger small">Ошибка загрузки данных</div>');
            });
        }
    },

    // API методы для взаимодействия с сервером
    api: {
        add: function(type, title) {
            $.post('/Dashboard/AddWebPart', { type: type, title: title || '' }, (html) => {
                const $newWidget = $(html);
                // Извлекаем размеры из атрибутов gs-w и gs-h, которые приходят с сервера
                const w = parseInt($newWidget.attr('gs-w')) || 6;
                const h = parseInt($newWidget.attr('gs-h')) || 4;
                
                const el = grid.addWidget($newWidget[0], { w: w, h: h });
                const id = $(el).attr('gs-id');
                if (id) DashboardCore.loadWebPartData(parseInt(id));
            });
        },        remove: function(id) {
            if (confirm('Удалить веб-часть?')) {
                $.post('/Dashboard/RemoveWebPart', { webPartId: id }, () => {
                    grid.removeWidget($(`.grid-stack-item[gs-id="${id}"]`)[0]);
                });
            }
        },
        updatePosition: (id, x, y) => $.post('/Dashboard/UpdateWebPartPosition', { webPartId: id, x: x, y: y }),
        updateSize: (id, w, h) => $.post('/Dashboard/UpdateWebPartSize', { webPartId: id, width: w, height: h })
    },

    // Рендереры контента
    renderers: {
        loadHtml: function($container, type) {
            $container.html('<div class="text-center my-auto"><div class="spinner-border text-primary spinner-border-sm"></div></div>');
            const url = type === 'Tasks' ? '/Tasks/GetTasksWebPart' : '/Monitoring/GetMonitoringWebPart';
            $.get(url, (html) => $container.html(html))
             .fail(() => $container.html(`<div class="alert alert-danger small">Ошибка загрузки ${type}</div>`));
        },

        renderJson: function($container, type, data) {
            if (!data || data.Error) {
                $container.html(`<div class="alert alert-warning small">${data?.Error || 'Нет данных'}</div>`);
                return;
            }
            switch(type) {
                case 'DataTable': WebPartRenderers.dataTable($container, data); break;
                case 'Chart': WebPartRenderers.chart($container, data); break;
                case 'Informer': WebPartRenderers.informer($container, data); break;
                default: $container.html(`<div class="text-muted small">Неизвестный тип: ${type}</div>`);
            }
        }
    }
};

/**
 * webpart.renderers.js
 * Чистые функции отрисовки данных.
 */
const WebPartRenderers = {
    dataTable: function($container, data) {
        const cols = data.Columns || data.columns || [];
        const rows = data.Rows || data.rows || [];
        if (cols.length === 0) { $container.html('Нет данных'); return; }

        let html = '<div class="table-responsive"><table class="table table-sm table-hover"><thead><tr>';
        cols.forEach(col => { html += `<th>${this.utils.escape(col)}</th>`; });
        html += '</tr></thead><tbody>';
        rows.forEach(row => {
            html += '<tr>';
            row.forEach(cell => { html += `<td>${this.utils.escape(cell)}</td>`; });
            html += '</tr>';
        });
        html += '</tbody></table></div>';
        $container.html(html);
    },

    chart: function($container, data) {
        const canvasId = `chart-${Math.random().toString(36).substr(2, 9)}`;
        $container.html(`<canvas id="${canvasId}"></canvas>`);
        
        let chartType = String(data.type || data.Type || 'bar').toLowerCase();
        if (!['bar', 'line', 'pie', 'doughnut'].includes(chartType)) chartType = 'bar';

        const ctx = document.getElementById(canvasId).getContext('2d');
        try {
            new Chart(ctx, {
                type: chartType,
                data: {
                    labels: data.labels || data.Labels || [],
                    datasets: [{
                        label: 'Значение',
                        data: data.data || data.Data || [],
                        backgroundColor: data.backgroundColor || data.BackgroundColor || 'rgba(54, 162, 235, 0.5)',
                        borderColor: data.borderColor || data.BorderColor || 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });
        } catch (e) {
            $container.html(`<div class="text-danger small">Ошибка графика: ${e.message}</div>`);
        }
    },

    informer: function($container, data) {
        const msg = data.message || data.Message || '';
        const type = data.type || data.Type || 'info';
        let html = `<div class="alert alert-${type} mb-0"><h6>${this.utils.escape(msg)}</h6>`;
        const details = data.details || data.Details || [];
        if (details.length > 0) {
            html += '<ul class="mb-0 mt-2 small">';
            details.forEach(d => { html += `<li>${this.utils.escape(d)}</li>`; });
            html += '</ul>';
        }
        html += '</div>';
        $container.html(html);
    },

    utils: {
        escape: (t) => {
            if (!t) return '';
            const map = { '&': '&', '<': '<', '>': '>', '"': '"', "'": '&#039;' };
            return t.toString().replace(/[&<>"']/g, m => map[m]);
        }
    }
};

// Глобальные алиасы для совместимости со старыми onclick в HTML
window.addWebPart = (type, title) => DashboardCore.api.add(type, title);
window.removeWebPart = (id) => DashboardCore.api.remove(id);
window.refreshWebPart = (id) => DashboardCore.loadWebPartData(id);
window.editWebPartTitle = (id) => {
    const newTitle = prompt("Введите новый заголовок:");
    if (newTitle) {
        $.post('/Dashboard/UpdateWebPartTitle', { webPartId: id, title: newTitle }, () => {
            $(`#webpart-${id} .webpart-title-text`).text(newTitle);
        });
    }
};
