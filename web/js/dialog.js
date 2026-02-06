function showInfoDialog(title, content) {
    // Check if dialog already exists
    let overlay = document.getElementById('info-modal-overlay');
    if (!overlay) {
        // Create Modal HTML
        overlay = document.createElement('div');
        overlay.id = 'info-modal-overlay';
        overlay.innerHTML = `
            <div class="info-modal-dialog">
                <div class="info-modal-header">
                    <h5 class="info-modal-title"></h5>
                    <button type="button" class="info-modal-close">&times;</button>
                </div>
                <div class="info-modal-body">
                    <p class="info-modal-text"></p>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);

        // Close logic
        const closeBtn = overlay.querySelector('.info-modal-close');
        const closeModal = () => {
            overlay.style.display = 'none';
            document.body.style.overflow = ''; // Restore scrolling
        };

        closeBtn.addEventListener('click', closeModal);
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) closeModal();
        });
    }

    // Set Content
    const titleEl = overlay.querySelector('.info-modal-title');
    const textEl = overlay.querySelector('.info-modal-text');

    titleEl.textContent = title;

    textEl.innerHTML = content;

    // Show
    overlay.style.display = 'flex';
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
}
