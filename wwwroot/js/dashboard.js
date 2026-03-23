// wwwroot/js/dashboard.js
// Основные функции для работы с дашбордом и веб-частями

console.log('Dashboard.js loaded');
console.log('jQuery version:', $.fn.jquery);
console.log('GridStack available:', typeof GridStack !== 'undefined');
console.log('Chart.js available:', typeof Chart !== 'undefined');

let grid = null;

// Инициализация при загрузке страницы
$(document).ready(function() {
    initializeGridStack();
    setupEventHandlers();
    loadAllWebPartsData();
});

// Инициализация GridStack
function initializeGridStack() {
    if ($('#dashboard-grid').length) {
        grid = GridStack.init({
            cellHeight: '120px',
            verticalMargin: 15,
            minRow: 3,
            disableDrag: false,
            disableResize: false,
            draggable: {
                handle: '.card-header',
                scroll: true,
                appendTo: 'body'
            },
            resizable: {
                handles: 'e, se, s, sw, w'
            },
            animate: true
        });
        
        // Обработка изменения позиции/размера
        grid.on('change', function(event, items) {
            items.forEach(item => {
                if (item.id) {
                    updateWebPartPosition(item.id, item.x, item.y);
                    updateWebPartSize(item.id, item.w, item.h);
                }
            });
        });
        
        // Обработка добавления/удаления
        grid.on('added', function(event, items) {
            items.forEach(item => {
                if (item.id) {
                    console.log('WebPart added:', item.id);
                }
            });
        });
        
        grid.on('removed', function(event, items) {
            items.forEach(item => {
                if (item.id) {
                    console.log('WebPart removed:', item.id);
                }
            });
        });
    }
}

// Настройка обработчиков событий
function setupEventHandlers() {
    // Глобальный обработчик AJAX ошибок
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('AJAX Error:', error, settings.url);
        if (xhr.status === 404) {
            showNotification('Страница не найдена', 'error');
        } else if (xhr.status === 500) {
            showNotification('Ошибка сервера', 'error');
        } else {
            showNotification('Ошибка соединения', 'error');
        }
    });
    
    // Обработка клавиши Escape для закрытия модальных окон
    $(document).keydown(function(e) {
        if (e.key === 'Escape') {
            closeAllModals();
        }
    });
}

// Загрузка данных для всех веб-частей
function loadAllWebPartsData() {
    console.log('Loading all webparts data...');
    
    // Используем селектор для поиска всех элементов с gs-id
    const items = document.querySelectorAll('.grid-stack-item[gs-id]');
    console.log(`Found ${items.length} grid items with gs-id`);
    
    items.forEach(function(item) {
        const id = item.getAttribute('gs-id');
        console.log(`Found webpart with gs-id: ${id}`);
        
        if (id) {
            const webPartId = parseInt(id);
            if (!isNaN(webPartId)) {
                loadWebPartData(webPartId);
            } else {
                console.error(`Invalid ID: ${id}`);
            }
        }
    });
}

// Загрузка данных для конкретной веб-части
function loadWebPartData(id) {
    console.log(`Loading webpart data for ID: ${id}`);
    
    const webPartId = parseInt(id);
    if (isNaN(webPartId)) {
        console.error(`Invalid webpart ID: ${id}`);
        return;
    }
    
    $.ajax({
        url: `/Dashboard/GetWebPartData?id=${webPartId}`,
        type: 'GET',
        dataType: 'json',
        timeout: 10000,
        success: function(data) {
            console.log(`WebPart ${webPartId} data loaded successfully`);
            const $container = $(`#webpart-${webPartId} .webpart-content`);
            if ($container.length === 0) {
                console.error(`Container for webpart ${webPartId} not found`);
                return;
            }
            const type = $(`#webpart-${webPartId}`).data('type');
            renderWebPartContent($container, type, data);
        },
        error: function(xhr, status, error) {
            console.error(`Error loading webpart ${webPartId}:`, error);
            const $container = $(`#webpart-${webPartId} .webpart-content`);
            if ($container.length === 0) {
                console.error(`Container for webpart ${webPartId} not found`);
                return;
            }
            $container.html(`
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    Ошибка загрузки данных<br>
                    <small>${xhr.responseJSON?.error || error || 'Неизвестная ошибка'}</small>
                    <button class="btn btn-sm btn-outline-danger mt-2" onclick="refreshWebPart(${webPartId})">
                        <i class="bi bi-arrow-repeat"></i> Повторить
                    </button>
                </div>
            `);
        }
    });
}

// Отображение контента веб-части в зависимости от типа
function renderWebPartContent($container, type, data) {
    if (!data) {
        $container.html('<div class="alert alert-warning">Нет данных для отображения</div>');
        return;
    }
    
    if (data.Error) {
        $container.html(`<div class="alert alert-danger">${data.Error}</div>`);
        return;
    }
    
    switch(type) {
        case 'DataTable':
            renderDataTable($container, data);
            break;
        case 'Chart':
            renderChart($container, data);
            break;
        case 'Informer':
            renderInformer($container, data);
            break;
        default:
            $container.html('<div class="alert alert-warning">Неизвестный тип веб-части</div>');
    }
}

// Отображение таблицы данных
function renderDataTable($container, data) {
    if (!data.columns || !data.rows || data.rows.length === 0) {
        $container.html(`
            <div class="alert alert-info">
                <i class="bi bi-info-circle-fill me-2"></i>
                Нет данных для отображения
            </div>
        `);
        return;
    }
    
    let html = '<div class="table-responsive" style="max-height: 100%;">';
    html += '<table class="table table-hover table-sm">';
    html += '<thead class="table-light sticky-top">';
    html += '<tr>';
    data.columns.forEach(col => {
        html += `<th>${escapeHtml(col)}</th>`;
    });
    html += '</tr></thead><tbody>';
    
    data.rows.forEach(row => {
        html += '<tr>';
        row.forEach(cell => {
            html += `<td>${escapeHtml(cell)}</td>`;
        });
        html += '</tr>';
    });
    
    html += '</tbody></table>';
    
    // Добавляем информацию о количестве записей
    if (data.rows.length > 0) {
        html += `<div class="mt-2 text-muted small">Всего записей: ${data.rows.length}</div>`;
    }
    
    html += '</div>';
    $container.html(html);
}

// Отображение графика
function renderChart($container, data) {
    const canvasId = `chart-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    $container.html(`<canvas id="${canvasId}" style="width: 100%; height: 100%;"></canvas>`);
    
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    // Настройки графика
    const chartConfig = {
        type: data.type || 'bar',
        data: {
            labels: data.labels || [],
            datasets: [{
                label: data.datasetLabel || 'Значения',
                data: data.data || [],
                backgroundColor: data.backgroundColor || 'rgba(54, 162, 235, 0.5)',
                borderColor: data.borderColor || 'rgba(54, 162, 235, 1)',
                borderWidth: 1,
                tension: data.type === 'line' ? 0.4 : 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        drawBorder: true
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    };
    
    // Создаем график
    try {
        new Chart(ctx, chartConfig);
    } catch (error) {
        console.error('Error creating chart:', error);
        $container.html(`
            <div class="alert alert-warning">
                <i class="bi bi-exclamation-triangle-fill me-2"></i>
                Ошибка при создании графика
            </div>
        `);
    }
}

// Отображение информера
function renderInformer($container, data) {
    const alertClass = data.type === 'warning' ? 'alert-warning' : 
                      data.type === 'error' ? 'alert-danger' : 'alert-info';
    
    const iconClass = data.type === 'warning' ? 'bi-exclamation-triangle-fill' :
                     data.type === 'error' ? 'bi-x-octagon-fill' : 'bi-info-circle-fill';
    
    let detailsHtml = '';
    if (data.details && data.details.length) {
        detailsHtml = '<ul class="mt-2 mb-0">';
        data.details.forEach(detail => {
            detailsHtml += `<li>${escapeHtml(detail)}</li>`;
        });
        detailsHtml += '</ul>';
    }
    
    const lastUpdate = data.lastUpdate ? new Date(data.lastUpdate) : new Date();
    const formattedDate = lastUpdate.toLocaleString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
    
    $container.html(`
        <div class="alert ${alertClass} mb-0 animate__animated animate__fadeIn">
            <div class="d-flex">
                <div class="me-3">
                    <i class="bi ${iconClass} fs-3"></i>
                </div>
                <div class="flex-grow-1">
                    <strong>${escapeHtml(data.message || 'Информация')}</strong>
                    ${detailsHtml}
                    <hr class="my-2">
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-muted">
                            <i class="bi bi-clock"></i> Обновлено: ${formattedDate}
                        </small>
                        <button class="btn btn-sm btn-link" onclick="refreshWebPart($(this).closest('.webpart').data('id'))">
                            <i class="bi bi-arrow-repeat"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `);
}

// Добавление новой веб-части
function addWebPart(type) {
    let defaultTitle = '';
    switch(type) {
        case 'DataTable':
            defaultTitle = 'Таблица данных';
            break;
        case 'Chart':
            defaultTitle = 'График';
            break;
        case 'Informer':
            defaultTitle = 'Информер';
            break;
        default:
            defaultTitle = 'Новая веб-часть';
    }
    
    const title = prompt('Введите название веб-части:', defaultTitle);
    if (title === null) return;
    
    // Показываем индикатор загрузки
    showNotification('Добавление веб-части...', 'info', false);
    
    $.ajax({
        url: '/Dashboard/AddWebPart',
        type: 'POST',
        data: { type: type, title: title },
        success: function(html) {
            const $webPart = $(html);
            const id = $webPart.data('id');
            
            // Создаем элемент GridStack
            const widget = document.createElement('div');
            widget.className = 'grid-stack-item';
            widget.setAttribute('gs-id', id);
            widget.setAttribute('gs-x', 0);
            widget.setAttribute('gs-y', grid ? grid.getRow() : 0);
            widget.setAttribute('gs-w', 4);
            widget.setAttribute('gs-h', 3);
            widget.innerHTML = `<div class="grid-stack-item-content">${html}</div>`;
            
            if (grid) {
                grid.addWidget(widget);
            } else {
                $('#dashboard-grid').append(widget);
            }
            
            loadWebPartData(id);
            showNotification('Веб-часть успешно добавлена', 'success');
        },
        error: function() {
            showNotification('Ошибка при добавлении веб-части', 'error');
        }
    });
}

// Удаление веб-части
function removeWebPart(id) {
    if (confirm('Удалить эту веб-часть?')) {
        $.ajax({
            url: '/Dashboard/RemoveWebPart',
            type: 'POST',
            data: { webPartId: id },
            success: function() {
                if (grid) {
                    const widget = grid.getGridItems().find(el => el.getAttribute('gs-id') == id);
                    if (widget) {
                        grid.removeWidget(widget);
                    }
                } else {
                    $(`#webpart-${id}`).closest('.grid-stack-item').remove();
                }
                showNotification('Веб-часть удалена', 'success');
            },
            error: function() {
                showNotification('Ошибка при удалении веб-части', 'error');
            }
        });
    }
}

// Обновление веб-части
function refreshWebPart(id) {
    const $webpart = $(`#webpart-${id}`);
    const $content = $webpart.find('.webpart-content');
    
    // Показываем спиннер
    $content.html(`
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Обновление...</span>
            </div>
            <p class="mt-2 text-muted">Обновление данных...</p>
        </div>
    `);
    
    loadWebPartData(id);
}

// Обновление позиции веб-части
function updateWebPartPosition(id, x, y) {
    $.ajax({
        url: '/Dashboard/UpdateWebPartPosition',
        type: 'POST',
        data: { webPartId: id, x: x, y: y },
        error: function() {
            console.error(`Failed to update position for webpart ${id}`);
        }
    });
}

// Обновление размера веб-части
function updateWebPartSize(id, w, h) {
    $.ajax({
        url: '/Dashboard/UpdateWebPartSize',
        type: 'POST',
        data: { webPartId: id, width: w, height: h },
        error: function() {
            console.error(`Failed to update size for webpart ${id}`);
        }
    });
}

// Редактирование заголовка веб-части
function editWebPartTitle(id) {
    const currentTitle = $(`#webpart-${id} .card-title span`).text();
    const newTitle = prompt('Введите новый заголовок:', currentTitle);
    
    if (newTitle && newTitle !== currentTitle) {
        $.ajax({
            url: '/Dashboard/UpdateWebPartTitle',
            type: 'POST',
            data: { webPartId: id, title: newTitle },
            success: function() {
                $(`#webpart-${id} .card-title span`).text(newTitle);
                showNotification('Заголовок обновлен', 'success');
            },
            error: function() {
                showNotification('Ошибка при обновлении заголовка', 'error');
            }
        });
    }
}

// Показать уведомление
function showNotification(message, type = 'info', autoHide = true) {
    // Создаем элемент уведомления
    const notification = $(`
        <div class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'} border-0 position-fixed bottom-0 end-0 m-3" 
             role="alert" 
             aria-live="assertive" 
             aria-atomic="true"
             style="z-index: 9999;">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${type === 'error' ? 'bi-exclamation-triangle-fill' : type === 'success' ? 'bi-check-circle-fill' : 'bi-info-circle-fill'} me-2"></i>
                    ${escapeHtml(message)}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `);
    
    $('body').append(notification);
    const toast = new bootstrap.Toast(notification[0]);
    toast.show();
    
    if (autoHide) {
        setTimeout(() => {
            toast.hide();
            setTimeout(() => notification.remove(), 500);
        }, 3000);
    }
}

// Закрыть все модальные окна
function closeAllModals() {
    $('.modal').modal('hide');
}

// Экранирование HTML для безопасности
function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return String(text).replace(/[&<>"']/g, function(m) { return map[m]; });
}

// Экспорт данных веб-части в CSV
function exportWebPartData(id, format = 'csv') {
    const $webpart = $(`#webpart-${id}`);
    const type = $webpart.data('type');
    
    if (type !== 'DataTable') {
        showNotification('Экспорт доступен только для таблиц данных', 'warning');
        return;
    }
    
    // Получаем данные
    $.get(`/Dashboard/GetWebPartData/${id}`, function(data) {
        if (!data.columns || !data.rows) {
            showNotification('Нет данных для экспорта', 'warning');
            return;
        }
        
        if (format === 'csv') {
            let csv = data.columns.join(',') + '\n';
            data.rows.forEach(row => {
                csv += row.map(cell => `"${cell}"`).join(',') + '\n';
            });
            
            // Скачиваем файл
            const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            const url = URL.createObjectURL(blob);
            link.setAttribute('href', url);
            link.setAttribute('download', `webpart_${id}_${new Date().toISOString()}.csv`);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
            
            showNotification('Экспорт завершен', 'success');
        }
    }).fail(function() {
        showNotification('Ошибка при экспорте данных', 'error');
    });
}

// Обновление всех веб-частей
function refreshAllWebParts() {
    $('.grid-stack-item').each(function() {
        const id = $(this).attr('gs-id');
        if (id) {
            refreshWebPart(parseInt(id));
        }
    });
    showNotification('Все веб-части обновлены', 'success');
}

// Сохранение раскладки дашборда
function saveDashboardLayout() {
    if (!grid) return;
    
    const items = [];
    grid.getGridItems().forEach(item => {
        const el = $(item);
        const id = el.attr('gs-id');
        if (id) {
            items.push({
                id: parseInt(id),
                x: parseInt(el.attr('gs-x')),
                y: parseInt(el.attr('gs-y')),
                w: parseInt(el.attr('gs-w')),
                h: parseInt(el.attr('gs-h'))
            });
        }
    });
    
    $.ajax({
        url: '/Dashboard/SaveLayout',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(items),
        success: function() {
            showNotification('Раскладка сохранена', 'success');
        },
        error: function() {
            showNotification('Ошибка при сохранении раскладки', 'error');
        }
    });
}

// Загрузка сохраненной раскладки
function loadDashboardLayout() {
    $.get('/Dashboard/GetLayout', function(layout) {
        if (layout && layout.length && grid) {
            layout.forEach(item => {
                const widget = grid.getGridItems().find(el => el.getAttribute('gs-id') == item.id);
                if (widget) {
                    grid.update(widget, {
                        x: item.x,
                        y: item.y,
                        w: item.w,
                        h: item.h
                    });
                }
            });
            showNotification('Раскладка загружена', 'success');
        }
    }).fail(function() {
        console.log('No saved layout found');
    });
}

function renderWebPartContent($container, type, data) {
    if (!data) {
        $container.html('<div class="alert alert-warning">Нет данных для отображения</div>');
        return;
    }
    
    if (data.Error) {
        $container.html(`<div class="alert alert-danger">${data.Error}</div>`);
        return;
    }
    
    switch(type) {
        case 'DataTable':
            renderDataTable($container, data);
            break;
        case 'Chart':
            renderChart($container, data);
            break;
        case 'Informer':
            renderInformer($container, data);
            break;
        case 'Tasks':
            // Загружаем представление для задач
            $.get('/Tasks/GetTasksWebPart', function(html) {
                $container.html(html);
            });
            break;
        default:
            $container.html('<div class="alert alert-warning">Неизвестный тип веб-части</div>');
    }
}

// Экспорт функций в глобальную область видимости
window.addWebPart = addWebPart;
window.removeWebPart = removeWebPart;
window.refreshWebPart = refreshWebPart;
window.editWebPartTitle = editWebPartTitle;
window.exportWebPartData = exportWebPartData;
window.refreshAllWebParts = refreshAllWebParts;
window.saveDashboardLayout = saveDashboardLayout;
window.loadDashboardLayout = loadDashboardLayout;
window.showNotification = showNotification;

console.log('Dashboard JS initialized');