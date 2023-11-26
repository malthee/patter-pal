// May add minification and bundling in the future https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification

/// Allows toasts to be shown anywhere in the app
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    const toastId = `toast-${Date.now()}`; // Unique ID for each toast
    const toastHtml = `
        <div id="${toastId}" class="toast hide align-items-center text-white bg-${type} border-0 mt-2" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    // Append the new toast to the container
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    // Initialize the toast
    const toast = new bootstrap.Toast(document.getElementById(toastId));
    toast.show();
}