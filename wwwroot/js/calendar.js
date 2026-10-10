// Calendario de entregas: carga los datos desde el servidor con AJAX (jQuery), sin recargar la página.
// Nota: los nombres de opciones (initialView, headerToolbar, eventClick...) pertenecen a la librería
// Full Calendar y deben mantenerse en inglés; solo los textos visibles y las variables están en español.
$(function () {
    var contenedor = document.getElementById('calendario');
    if (!contenedor) return;
    $.ajax({
        url: '/Dashboard/EventosJson',
        method: 'GET',
        dataType: 'json',
        success: function (entregas) {
            var calendario = new FullCalendar.Calendar(contenedor, {
                initialView: 'dayGridMonth',
                locale: 'es',
                buttonText: {
                    today: 'Hoy',
                    month: 'Mes',
                    week: 'Semana',
                    day: 'Día',
                    list: 'Agenda'
                },
                noEventsText: 'No hay entregas programadas',
                headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,listWeek' },
                events: entregas,
                eventClick: function (datos) {
                    if (datos.event.url) {
                        datos.jsEvent.preventDefault();
                        window.location.href = datos.event.url;
                    }
                }
            });
            calendario.render();
        },
        error: function () {
            contenedor.innerHTML = '<div class="alert alert-danger">No se pudieron cargar las entregas del calendario.</div>';
        }
    });
});
