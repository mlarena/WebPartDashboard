// wwwroot/js/dashboard.js
// Ядро управления дашбордом и жизненным циклом веб-частей

let grid = null;

// Функция принудительной регистрации компонентов Chart.js
function initCharts() {
    if (typeof Chart !== 'undefined') {
        // В версии 4+ UMD компоненты должны регистрироваться автоматически, 
        // но если этого не произошло, делаем это вручную.
        if (Chart.registerables) {
            Chart.register(...Chart.registerables);
        }
        console.log('Chart.js initialized');
    }
}

$(document).ready(function() {
    initCharts();
    initializeGridStack();
    setupEventHandlers();
    
    // Небольшая задержка перед загрузкой данных, чтобы DOM полностью стабилизировался
    setTimeout(loadAllWebPartsData, 100);
});

function initializeGridStack() {
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
        
        grid.on('change', function(event, items) {
            if (!items || !Array.isArray(items)) return;
            items.forEach(item => {
                if (item.id) {
                    updateWebPartPosition(item.id, item.x, item.y);
                    updateWebPartSize(item.id, item.w, item.h);
                }
            });
        });
    }
}

function setupEventHandlers() {
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('AJAX Error:', error, settings.url);
    });
}

function loadAllWebPartsData() {
    console.log('Starting to load all webparts...');
    $('.grid-stack-item').each(function() {
        const id = $(this).attr('gs-id');
        if (id) loadWebPartData(parseInt(id));
    });
}

function loadWebPartData(id) {
    const $card = $(`#webpart-${id}`);
    // Читаем тип напрямую из атрибута, чтобы избежать проблем с кэшем jQuery .data()
    const type = $card.attr('data-type');
    const $container = $card.find('.webpart-content');

    console.log(`Loading data for webpart ${id}, type: ${type}`);

    if (!type) {
        console.warn(`Type not found for webpart ${id}`);
        return;
    }

    // CRUD веб-части (загружают HTML)
    if (type === 'Tasks' || type === 'Monitoring') {
        renderHtmlWebPart($container, type);
    } 
    // Информационные веб-части (загружают JSON)
    else {
        $.ajax({
            url: `/Dashboard/GetWebPartData?id=${id}`,
            type: 'GET',
            dataType: 'json',
            success: function(data) {
                renderJsonWebPartContent($container, type, data);
            },
            error: function() {
                $container.html('<div class="alert alert-danger">Ошибка загрузки данных</div>');
            }
        });
    }
}

function renderHtmlWebPart($container, type) {
    $container.html('<div class="text-center py-5"><div class="spinner-border text-primary"></div></div>');
    const url = type === 'Tasks' ? '/Tasks/GetTasksWebPart' : '/Monitoring/GetMonitoringWebPart';
    
    $.get(url, function(html) {
        $container.html(html);
    }).fail(function(xhr) {
        $container.html(`<div class="alert alert-danger">Ошибка загрузки ${type} (Status: ${xhr.status})</div>`);
    });
}

function renderJsonWebPartContent($container, type, data) {
    if (!data || data.Error) {
        $container.html('<div class="alert alert-warning">' + (data?.Error || 'Нет данных') + '</div>');
        return;
    }
    
    switch(type) {
        case 'DataTable': renderDataTable($container, data); break;
        case 'Chart': renderChart($container, data); break;
        case 'Informer': renderInformer($container, data); break;
        default: $container.html('<div class="alert alert-info">Неизвестный тип: ' + type + '</div>');
    }
}

function renderDataTable($container, data) {
    const cols = data.Columns || data.columns || [];
    const rows = data.Rows || data.rows || [];
    if (cols.length === 0) { $container.html('<div class="p-3 text-muted">Нет данных</div>'); return; }

    let html = '<div class="table-responsive"><table class="table table-sm table-hover"><thead><tr>';
    cols.forEach(col => { html += '<th>' + escapeHtml(col) + '</th>'; });
    html += '</tr></thead><tbody>';
    rows.forEach(row => {
        html += '<tr>';
        row.forEach(cell => { html += '<td>' + escapeHtml(cell) + '</td>'; });
        html += '</tr>';
    });
    html += '</tbody></table></div>';
    $container.html(html);
}

function renderChart($container, data) {
    const canvasId = 'chart-' + Math.random().toString(36).substr(2, 9);
    $container.html('<canvas id="' + canvasId + '"></canvas>');
    
    let chartType = 'bar';
    const rawType = data.type || data.Type;
    if (rawType) chartType = String(rawType).toLowerCase();
    
    const labels = data.labels || data.Labels || [];
    const chartData = data.data || data.Data || [];

    const ctx = document.getElementById(canvasId).getContext('2d');
    
    try {
        new Chart(ctx, {
            type: chartType,
            data: {
                labels: labels,
                datasets: [{
                    label: 'Значение',
                    data: chartData,
                    backgroundColor: data.backgroundColor || data.BackgroundColor || 'rgba(54, 162, 235, 0.5)',
                    borderColor: data.borderColor || data.BorderColor || 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: { 
                responsive: true, 
                maintainAspectRatio: false,
                plugins: { legend: { display: true } }
            }
        });
    } catch (e) {
        console.error('Chart creation failed:', e);
        $container.html('<div class="alert alert-danger">Ошибка отрисовки графика: ' + e.message + '</div>');
    }
}

function renderInformer($container, data) {
    const msg = data.message || data.Message || '';
    const type = data.type || data.Type || 'info';
    let html = `<div class="alert alert-${type} mb-0"><h6>${escapeHtml(msg)}</h6>`;
    const details = data.details || data.Details || [];
    if (details.length > 0) {
        html += '<ul class="mb-0 mt-2 small">';
        details.forEach(d => { html += '<li>' + escapeHtml(d) + '</li>'; });
        html += '</ul>';
    }
    html += '</div>';
    $container.html(html);
}

function addWebPart(type, title) {
    $.post('/Dashboard/AddWebPart', { type: type, title: title || '' }, function(html) {
        const $newWidget = $(html);
        const el = grid.addWidget($newWidget[0]);
        const id = $(el).attr('gs-id');
        if (id) loadWebPartData(parseInt(id));
    });
}

function removeWebPart(id) {
    if (confirm('Удалить веб-часть?')) {
        $.post('/Dashboard/RemoveWebPart', { webPartId: id }, function() {
            const el = $(`.grid-stack-item[gs-id="${id}"]`)[0];
            if (el) grid.removeWidget(el);
        });
    }
}

function updateWebPartPosition(id, x, y) { $.post('/Dashboard/UpdateWebPartPosition', { webPartId: id, x: x, y: y }); }
function updateWebPartSize(id, width, height) { $.post('/Dashboard/UpdateWebPartSize', { webPartId: id, width: width, height: height }); }
function refreshWebPart(id) { loadWebPartData(id); }

function escapeHtml(t) {
    if (!t) return '';
    const map = { '&': '&', '<': '<', '>': '>', '"': '"', "'": '&#039;' };
    return t.toString().replace(/[&<>"']/g, m => map[m]);
}
