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
    // Allow basic HTML in content (e.g. line breaks) or just text
    // The previous implementation used textContent, but description might have formatting.
    // Let's use innerHTML but be careful, or textContent if we want to be safe.
    // For now, let's assume textContent to match previous behavior for Play Count, 
    // but descriptions might need formatting.
    // However, synopsys is usually plain text in standard Valkyrie? 
    // Let's use innerText or textContent to escape HTML for safety, unless user wants HTML.
    // The request mentions "fallback logic... e.g. synopsys.English".
    // Let's stick to textContent for safety unless needed.
    textEl.textContent = content;

    // Show
    overlay.style.display = 'flex';
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
}
