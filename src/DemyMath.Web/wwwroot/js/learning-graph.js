(() => {
    const viewport = document.getElementById('graphViewport');
    const canvas = document.getElementById('graphCanvas');

    // На сторінці без графа ініціалізація не потрібна.
    if (!viewport || !canvas) {
        return;
    }

    const zoomLabel = document.getElementById('zoomLevel');
    const zoomInButton = document.getElementById('zoomIn');
    const zoomOutButton = document.getElementById('zoomOut');
    const resetButton = document.getElementById('resetGraph');

    let scale = 1;
    let x = 0;
    let y = 0;

    let dragging = false;
    let moved = false;
    let activePointerId = null;
    let suppressClick = false;

    let startPointerX = 0;
    let startPointerY = 0;
    let startX = 0;
    let startY = 0;

    const minScale = 0.25;
    const maxScale = 3;
    const dragThreshold = 4;

    function render() {
        canvas.style.transform =
            `translate(${x}px, ${y}px) scale(${scale})`;

        if (zoomLabel) {
            zoomLabel.textContent = `${Math.round(scale * 100)}%`;
        }
    }

    function zoomAt(nextScale, clientX, clientY) {
        nextScale = Math.max(minScale, Math.min(maxScale, nextScale));

        const rect = viewport.getBoundingClientRect();
        const pointerX = clientX - rect.left;
        const pointerY = clientY - rect.top;

        // Зберігаємо точку графа під вказівником.
        const worldX = (pointerX - x) / scale;
        const worldY = (pointerY - y) / scale;

        scale = nextScale;

        x = pointerX - worldX * scale;
        y = pointerY - worldY * scale;

        render();
    }

    function fitGraph() {
        const width = canvas.offsetWidth;
        const height = canvas.offsetHeight;

        const availableWidth = Math.max(1, viewport.clientWidth - 40);
        const availableHeight = Math.max(1, viewport.clientHeight - 40);

        scale = Math.max(
            minScale,
            Math.min(1, availableWidth / width, availableHeight / height)
        );

        x = (viewport.clientWidth - width * scale) / 2;
        y = (viewport.clientHeight - height * scale) / 2;

        render();
    }

    function startDragging(event) {
        if (event.button !== 0 || dragging) {
            return;
        }

        // Клік по вузлу має відкривати тему, а не запускати перетягування.
        if (
            event.target instanceof Element &&
            event.target.closest('a, button')
        ) {
            return;
        }

        dragging = true;
        moved = false;
        activePointerId = event.pointerId;

        startPointerX = event.clientX;
        startPointerY = event.clientY;

        startX = x;
        startY = y;

        viewport.classList.add('is-dragging');
        viewport.setPointerCapture(event.pointerId);
    }

    function moveGraph(event) {
        if (!dragging || event.pointerId !== activePointerId) {
            return;
        }

        const dx = event.clientX - startPointerX;
        const dy = event.clientY - startPointerY;

        if (
            Math.abs(dx) > dragThreshold ||
            Math.abs(dy) > dragThreshold
        ) {
            moved = true;
        }

        if (!moved) {
            return;
        }

        x = startX + dx;
        y = startY + dy;

        render();
    }

    function stopDragging(event) {
        if (!dragging || event.pointerId !== activePointerId) {
            return;
        }

        dragging = false;
        activePointerId = null;

        viewport.classList.remove('is-dragging');

        if (moved) {
            suppressClick = true;

            setTimeout(() => {
                suppressClick = false;
            }, 0);
        }

        // Відпускаємо захоплення вказівника, якщо воно ще активне.
        if (viewport.hasPointerCapture(event.pointerId)) {
            viewport.releasePointerCapture(event.pointerId);
        }
    }

    viewport.addEventListener('pointerdown', startDragging);
    viewport.addEventListener('pointermove', moveGraph);
    viewport.addEventListener('pointerup', stopDragging);
    viewport.addEventListener('pointercancel', stopDragging);

    // Після перетягування не дозволяємо випадково відкрити тему.
    viewport.addEventListener('click', event => {
        if (!suppressClick) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();

        suppressClick = false;
    }, true);

    viewport.addEventListener('wheel', event => {
        event.preventDefault();

        const factor = event.deltaY < 0 ? 1.1 : 1 / 1.1;

        zoomAt(
            scale * factor,
            event.clientX,
            event.clientY
        );
    }, { passive: false });

    zoomInButton?.addEventListener('click', () => {
        const rect = viewport.getBoundingClientRect();

        zoomAt(
            scale * 1.2,
            rect.left + rect.width / 2,
            rect.top + rect.height / 2
        );
    });

    zoomOutButton?.addEventListener('click', () => {
        const rect = viewport.getBoundingClientRect();

        zoomAt(
            scale / 1.2,
            rect.left + rect.width / 2,
            rect.top + rect.height / 2
        );
    });

    resetButton?.addEventListener('click', fitGraph);
    window.addEventListener('resize', fitGraph);

    fitGraph();
})();