// Archivo: CRM.AplicacionWeb\wwwroot\js\legacy\js.js
// Script heredado con interacciones visuales reutilizadas por las paginas antiguas.

/* 1. INTERACTIVIDAD VISUAL: EFECTO FLIP */
// Nota: Asegúrate de que en tu PHP las cards tengan la clase "card-pelicula"
document.querySelectorAll(".card-pelicula").forEach(function (element) {
    element.addEventListener('click', function () {
        this.classList.toggle("animacionFlip");
    });
});

/* 2. BÚSQUEDA INTELIGENTE (Auto-submit) */
document.addEventListener("DOMContentLoaded", function() {
    // Referencia el formulario de filtros para enviarlo automaticamente al buscar.
    const form = document.querySelector(".filtros-form");
    // Campo de texto usado para filtrar el catalogo por titulo.
    const filterInput = document.querySelector(".input-busqueda");

    if (filterInput && form) {
        // Retrasa el envio para evitar una peticion por cada pulsacion.
        let timeout = null;
        filterInput.addEventListener('keyup', function() {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                if (this.value.length >= 3 || this.value.length === 0) {
                    form.submit();
                }
            }, 800); 
        });
    }
});

/* 3. VALORACIONES POR AJAX (Sin recargar página) */
document.querySelectorAll('.btnEstrella').forEach(btn => {
    btn.addEventListener('click', function(e) {
        e.preventDefault();
        // Formulario de valoracion asociado al boton de estrella pulsado.
        let form = this.closest('form');
        // Datos enviados al endpoint legacy de valoraciones.
        let formData = new FormData(form);
        formData.append('rating', this.value);

        fetch('guardarValoracion.php', {
            method: 'POST',
            body: formData
        })
        .then(response => response.text())
        .then(data => {
            alert('¡Gracias por tu valoración!');
            // Aquí podrías añadir lógica para iluminar las estrellas fijas
        });
    });
});


/* 4. Perfil de Usuario: Edición en Modal (Ejemplo básico) */
document.addEventListener('DOMContentLoaded', () => {
    // 1. Inicializar Gráfica si existen los datos en el objeto global
    if (window.chartData) {
        new Chart(document.getElementById('graficaVistas'), {
            type: 'line',
            data: {
                labels: window.chartData.labels,
                datasets: [{ 
                    data: window.chartData.counts,
                    borderColor: '#e2b616', 
                    tension: 0.4, 
                    fill: true, 
                    backgroundColor: 'rgba(226, 182, 22, 0.1)' 
                }]
            },
            options: { 
                responsive: true, 
                maintainAspectRatio: false, 
                plugins: { legend: { display: false } } 
            }
        });
    }

    // 2. Lógica de Logros
    // Numero maximo de insignias visibles por pagina.
    const ACHIEVEMENTS_PER_PAGE = 4;
    // Pagina de insignias que se esta mostrando en este momento.
    let currentPage = 1;
    // Lista filtrada que alimenta el renderizado y el paginador.
    let filteredAchievements = [...window.allAchievements];

    // Contenedor donde se pintan las tarjetas de insignias.
    const container = document.getElementById('achievementContainer');
    // Contenedor de botones para navegar entre paginas de insignias.
    const pager = document.getElementById('achievementPager');
    // Campo de busqueda usado para filtrar insignias por texto.
    const searchInput = document.getElementById('achievementSearch');

    // Renderiza las insignias de la pagina actual y gestiona el estado vacio.
    function showAchievements() {
        container.innerHTML = "";
        if (filteredAchievements.length === 0) {
            container.innerHTML = "<p style='grid-column: 1/-1; text-align: center; color: #666;'>No se encontraron resultados.</p>";
            pager.innerHTML = "";
            return;
        }

        const startIndex = (currentPage - 1) * ACHIEVEMENTS_PER_PAGE;
        const pageItems = filteredAchievements.slice(startIndex, startIndex + ACHIEVEMENTS_PER_PAGE);

        pageItems.forEach(achievement => {
            container.innerHTML += `
                <div class="logro-card">
                    <img src="${achievement.imagePath}" class="logro-img" draggable="false">
                    <div class="logro-txt">
                        <h3>${achievement.title}</h3>
                        <p>${achievement.description}</p>
                        <div style="margin-top: 5px; color: #e2b616; font-size: 0.7rem;">✓ COMPLETADO</div>
                    </div>
                </div>`;
        });
        createPager();
    }

    // Crea los botones de paginacion segun el numero de insignias filtradas.
    function createPager() {
        const pageCount = Math.ceil(filteredAchievements.length / ACHIEVEMENTS_PER_PAGE);
        pager.innerHTML = "";
        if (pageCount <= 1) return;

        for (let i = 1; i <= pageCount; i++) {
            const btn = document.createElement('button');
            btn.innerText = i;
            btn.className = `btn-pag ${i === currentPage ? 'active' : ''}`;
            btn.onclick = () => { 
                currentPage = i;
                showAchievements();
                window.scrollTo({top: container.offsetTop - 100, behavior: 'smooth'});
            };
            pager.appendChild(btn);
        }
    }

    // Filtra las insignias desde el buscador del perfil y reinicia la pagina actual.
    window.startFiltering = () => {
        const searchText = searchInput.value.toLowerCase();
        filteredAchievements = window.allAchievements.filter(achievement =>
            achievement.title.toLowerCase().includes(searchText) ||
            achievement.description.toLowerCase().includes(searchText)
        );
        currentPage = 1;
        showAchievements();
    };

    showAchievements();
});
