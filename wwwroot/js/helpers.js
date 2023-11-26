const HOST_URL = document.location.hostname + (document.location.port ? (":" + document.location.port) : "");
const HOST_URL_WITH_PROTOCOL = `${window.location.protocol}//${HOST_URL}`;

export function resolveHostWebSocketURL(path) {
    const wsProtocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    return `${wsProtocol}//${HOST_URL}/${path}`;
}

export function resolveHostURL(path) {
    return `${HOST_URL_WITH_PROTOCOL}/${path}`;
}

export function getQueryParameters() {
    const params = new URLSearchParams(window.location.search);
    const paramsObj = {};
    for (const [key, value] of params.entries()) {
        paramsObj[key] = value;
    }
    return paramsObj;
}

export function updateQueryParameters(newParams) {
    const currentUrl = new URL(window.location);
    const searchParams = new URLSearchParams(currentUrl.search);

    // Update existing parameters or add new ones
    for (const key in newParams) {
        searchParams.set(key, newParams[key]);
    }

    // Construct the new URL with updated query parameters
    const newUrl = `${currentUrl.pathname}?${searchParams.toString()}`;

    // Update the browser's history
    window.history.pushState({}, '', newUrl);
}


export function htmlEscape(str) {
    if (str === null || str === undefined) {
        return '';
    }

    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/`/g, '&#96;')
        .replace(/\//g, '&#x2F;');
}