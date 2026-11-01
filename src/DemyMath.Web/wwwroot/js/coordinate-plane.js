(() => {
    const svgNamespace = 'http://www.w3.org/2000/svg';

    function createSvgElement(name, attributes = {}) {
        const element = document.createElementNS(svgNamespace, name);

        for (const [key, value] of Object.entries(attributes)) {
            element.setAttribute(key, value);
        }

        return element;
    }

    function formatNumber(value) {
        return Number(value.toFixed(6)).toString();
    }

    function initializePlane(container) {
        const svg = container.querySelector('.coordinate-plane');
        const inputX = container.querySelector('.point-x');
        const inputY = container.querySelector('.point-y');
        const selection = container.querySelector('.coordinate-selection');

        const minX = Number(container.dataset.minX);
        const maxX = Number(container.dataset.maxX);
        const minY = Number(container.dataset.minY);
        const maxY = Number(container.dataset.maxY);
        const step = Number(container.dataset.gridStep);

        if (
            !Number.isFinite(minX) ||
            !Number.isFinite(maxX) ||
            !Number.isFinite(minY) ||
            !Number.isFinite(maxY) ||
            !Number.isFinite(step) ||
            minX >= maxX ||
            minY >= maxY ||
            step <= 0
        ) {
            return;
        }

        const width = 600;
        const height = 400;
        const padding = 35;

        const plotWidth = width - padding * 2;
        const plotHeight = height - padding * 2;

        const toSvgX = value =>
            padding + (value - minX) / (maxX - minX) * plotWidth;

        const toSvgY = value =>
            padding + (maxY - value) / (maxY - minY) * plotHeight;

        svg.replaceChildren();

        const grid = createSvgElement('g');
        const axes = createSvgElement('g');

        function addLine(group, x1, y1, x2, y2, className) {
            group.appendChild(createSvgElement('line', {
                x1, y1, x2, y2,
                class: className
            }));
        }

        function addLabel(group, x, y, text) {
            const label = createSvgElement('text', {
                x,
                y,
                class: 'axis-label',
                'text-anchor': 'middle'
            });

            label.textContent = text;
            group.appendChild(label);
        }

        const xCount = Math.floor((maxX - minX) / step);
        const yCount = Math.floor((maxY - minY) / step);

        // Обмежуємо кількість ліній, щоб велика сітка не перевантажувала браузер.
        const xStride = Math.max(1, Math.ceil(xCount / 100));
        const yStride = Math.max(1, Math.ceil(yCount / 100));

        for (let i = 0; i <= xCount; i += xStride) {
            const value = minX + i * step;
            const x = toSvgX(value);

            addLine(grid, x, padding, x, height - padding, 'grid-line');
            addLabel(grid, x, height - padding + 18, formatNumber(value));
        }

        for (let i = 0; i <= yCount; i += yStride) {
            const value = minY + i * step;
            const y = toSvgY(value);

            addLine(grid, padding, y, width - padding, y, 'grid-line');
            addLabel(grid, padding - 18, y + 4, formatNumber(value));
        }

        if (minX <= 0 && maxX >= 0) {
            const x = toSvgX(0);
            addLine(axes, x, padding, x, height - padding, 'axis-line');
        }

        if (minY <= 0 && maxY >= 0) {
            const y = toSvgY(0);
            addLine(axes, padding, y, width - padding, y, 'axis-line');
        }

        svg.append(grid, axes);

        const point = createSvgElement('circle', {
            r: 7,
            class: 'selected-point',
            visibility: 'hidden'
        });

        svg.appendChild(point);

        function selectPoint(event) {
            const rect = svg.getBoundingClientRect();

            const svgX = (event.clientX - rect.left) / rect.width * width;
            const svgY = (event.clientY - rect.top) / rect.height * height;

            const rawX = minX + (svgX - padding) / plotWidth * (maxX - minX);
            const rawY = maxY - (svgY - padding) / plotHeight * (maxY - minY);

            if (
                svgX < padding ||
                svgX > width - padding ||
                svgY < padding ||
                svgY > height - padding
            ) {
                return;
            }

            const selectedX = Math.min(
                maxX,
                Math.max(minX, minX + Math.round((rawX - minX) / step) * step)
            );

            const selectedY = Math.min(
                maxY,
                Math.max(minY, minY + Math.round((rawY - minY) / step) * step)
            );

            const x = Number(selectedX.toFixed(8));
            const y = Number(selectedY.toFixed(8));

            inputX.value = formatNumber(x);
            inputY.value = formatNumber(y);

            point.setAttribute('cx', toSvgX(x));
            point.setAttribute('cy', toSvgY(y));
            point.setAttribute('visibility', 'visible');

            selection.textContent =
                `${container.dataset.selectedLabel}: (${formatNumber(x)}, ${formatNumber(y)})`;
        }

        svg.addEventListener('click', selectPoint);
    }

    document
        .querySelectorAll('.coordinate-question')
        .forEach(initializePlane);
})();