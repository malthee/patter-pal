const HOST_URL = document.location.hostname + (document.location.port ? (":" + document.location.port) : "");
const HOST_URL_WITH_PROTOCOL = `${window.location.protocol}//${HOST_URL}`;

export function resolveHostWebSocketURL(path) {
    const wsProtocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    return `${wsProtocol}//${HOST_URL}/${path}`;
}

export function resolveHostURL(path) {
    return `${HOST_URL_WITH_PROTOCOL}/${path}`;
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