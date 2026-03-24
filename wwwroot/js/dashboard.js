// wwwroot/js/dashboard.js
// Основной скрипт управления дашбордом и жизненным циклом веб-частей

console.log('Dashboard.js loaded');

let grid = null;

// Инициализация при загрузке страницы
$(document).ready(function() {
    initializeGridStack();
    setupEventHandlers();
    loadAllWebPartsData();
});

// Инициализация библиотеки GridStack (сетка drag-and-drop)
function initializeGridStack() {
    if ($('#dashboard-grid').length) {
        grid = GridStack.init({
            cellHeight: '120px',
            verticalMargin: 15,
            minRow: 3,
            disableDrag: false,
            disableResize: false,
            draggable: {
                handle: '.card-header', // Перетаскивание только за заголовок
                scroll: true,
                appendTo: 'body'
            },
            resizable: {
                handles: 'e, se, s, sw, w' // Стороны, за которые можно менять размер
            },
            animate: true
        });
        
        // Событие изменения позиции или размера плитки
        grid.on('change', function(event, items) {
            if (!items || !Array.isArray(items)) return;
            
            items.forEach(item => {
                if (item.id) {
                    // Сохраняем новые координаты и размеры на сервере
                    updateWebPartPosition(item.id, item.x, item.y);
                    updateWebPartSize(item.id, item.w, item.h);
                }
            });
        });
    }
}

// Настройка глобальных обработчиков событий
function setupEventHandlers() {
    // Глобальный перехват AJAX ошибок для уведомления пользователя
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('AJAX Error:', error, settings.url);
        showNotification('Ошибка при выполнении запроса: ' + (error || 'Неизвестная ошибка'), 'error');
    });
}

// Загрузка контента для всех веб-частей, присутствующих на странице
function loadAllWebPartsData() {
    const items = document.querySelectorAll('.grid-stack-item[gs-id]');
    items.forEach(function(item) {
        const id = item.getAttribute('gs-id');
        if (id) {
            const webPartId = parseInt(id);
            if (!isNaN(webPartId)) {
                loadWebPartData(webPartId);
            }
        }
    });
}

// Загрузка данных конкретной веб-части через AJAX
function loadWebPartData(id) {
    const webPartId = parseInt(id);
    const $container = $(`#webpart-${webPartId} .webpart-content`);
    const type = $(`#webpart-${webPartId}`).data('type');

    // Если это веб-часть задач, загружаем её HTML-представление
    if (type === 'Tasks') {
        renderTasksWebPart($container);
        return;
    }

    // Для остальных типов (графики, таблицы) запрашиваем JSON данные
    $.ajax({
        url: `/Dashboard/GetWebPartData?id=${webPartId}`,
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            renderWebPartContent($container, type, data);
        }
    });
}

// Отрисовка содержимого веб-части на основе её типа
function renderWebPartContent($container, type, data) {
    if (!data) {
        $container.html('<div class="alert alert-warning">Нет данных</div>');
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
    }
}

// Специальный метод для загрузки веб-части задач (загружает частичное представление)
function renderTasksWebPart($container) {
    $container.html('<div class="text-center py-5"><div class="spinner-border text-primary"></div><p>Загрузка задач...</p></div>');
    
    $.get('/Tasks/GetTasksWebPart', function(html) {
        $container.html(html);
    }).fail(function(xhr) {
        $container.html(`<div class="alert alert-danger">Ошибка загрузки задач: ${xhr.status}</div>`);
    });
}

// --- Функции взаимодействия с API DashboardController ---

function addWebPart(type, title) {
    $.post('/Dashboard/AddWebPart', { type: type, title: title || '' }, function(html) {
        // Добавляем новую плитку в сетку GridStack
        const el = grid.addWidget(html);
        const id = $(el).attr('gs-id');
        if (id) loadWebPartData(id);
    });
}

function removeWebPart(id) {
    if (confirm('Удалить эту веб-часть?')) {
        $.post('/Dashboard/RemoveWebPart', { webPartId: id }, function() {
            const el = document.querySelector(`.grid-stack-item[gs-id="${id}"]`);
            if (el) grid.removeWidget(el);
        });
    }
}

function updateWebPartPosition(id, x, y) {
    $.post('/Dashboard/UpdateWebPartPosition', { webPartId: id, x: x, y: y });
}

function updateWebPartSize(id, width, height) {
    $.post('/Dashboard/UpdateWebPartSize', { webPartId: id, width: width, height: height });
}

function refreshWebPart(id) {
    loadWebPartData(id);
}

function showNotification(message, type) {
    console.log(`Notification [${type}]: ${message}`);
}

function escapeHtml(text) {
    if (!text) return '';
    return text.toString().replace(/&/g, "&").replace(/</g, "<").replace(/>/g, ">");
}
