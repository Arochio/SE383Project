// Clicker Game JavaScript

document.addEventListener('DOMContentLoaded', function () {
    const clickBtn = document.getElementById('clickBtn');
    const pointsDisplay = document.getElementById('pointsDisplay');
    const clickValueDisplay = document.getElementById('clickValueDisplay');
    const clickForm = document.getElementById('clickForm');
    let displayedPoints = Number(pointsDisplay?.textContent ?? 0);
    const pointsPerSecond = Number(pointsDisplay?.dataset.pointsPerSecond ?? 0);

    // Add click animation
    if (clickBtn) {
        clickBtn.addEventListener('click', function (e) {
            // Add ripple effect
            addRippleEffect(e);

            // Add click animation
            clickBtn.style.animation = 'none';
            setTimeout(() => {
                clickBtn.style.animation = 'clickPulse 0.3s ease-out';
            }, 10);
        });
    }

    // Ripple effect on click
    function addRippleEffect(e) {
        const rect = clickBtn.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        const ripple = document.createElement('span');
        ripple.className = 'ripple';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';
        ripple.style.position = 'absolute';
        ripple.style.borderRadius = '50%';
        ripple.style.backgroundColor = 'rgba(255, 255, 255, 0.6)';
        ripple.style.width = '20px';
        ripple.style.height = '20px';
        ripple.style.pointerEvents = 'none';
        ripple.style.animation = 'ripple 0.6s ease-out';

        clickBtn.style.position = 'relative';
        clickBtn.style.overflow = 'hidden';
        clickBtn.appendChild(ripple);

        setTimeout(() => ripple.remove(), 600);
    }

    // Update points display via form submission
    if (clickForm) {
        clickForm.addEventListener('submit', function (e) {
            // Let the form submit normally to update the server state
        });
    }

    // Bonus: Add keyboard support (spacebar for clicking)
    document.addEventListener('keydown', function (e) {
        if (e.code === 'Space' || e.key === ' ') {
            if (clickBtn && document.activeElement !== document.querySelector('input')) {
                e.preventDefault();
                clickBtn.click();
            }
        }
    });

    // Auto-update visual feedback when page reloads after form submission
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'characterData' || mutation.type === 'childList') {
                updateVisuals();
            }
        });
    });

    function updateVisuals() {
        // Add any visual updates here when state changes
        const cards = document.querySelectorAll('.upgrade-card');
        cards.forEach(card => {
            const button = card.querySelector('.upgrade-button');
            if (button && button.disabled) {
                card.classList.add('disabled');
            } else {
                card.classList.remove('disabled');
            }
        });
    }

    function updatePointsVisually() {
        displayedPoints += pointsPerSecond;
        if (pointsDisplay) {
            pointsDisplay.textContent = Math.floor(displayedPoints).toString();
        }
    }

    async function refreshPointsFromServer() {
        try {
            const response = await fetch('?handler=State', {
                headers: { Accept: 'application/json' },
                cache: 'no-store'
            });

            if (!response.ok) {
                return;
            }

            const state = await response.json();
            displayedPoints = state.points;
            if (pointsDisplay) {
                pointsDisplay.textContent = displayedPoints.toString();
            }
        } catch {
            // Keep the visual estimate until the next server refresh succeeds.
        }
    }

    if (pointsDisplay && pointsPerSecond > 0) {
        window.setInterval(() => {
            updatePointsVisually();
            refreshPointsFromServer();
        }, 1000);
    }

    // Initial visual update
    updateVisuals();
});

// CSS Animations
const style = document.createElement('style');
style.textContent = `
    @keyframes clickPulse {
        0% {
            transform: scale(1);
        }
        50% {
            transform: scale(0.95);
        }
        100% {
            transform: scale(1);
        }
    }

    @keyframes ripple {
        0% {
            transform: scale(1);
            opacity: 1;
        }
        100% {
            transform: scale(4);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
