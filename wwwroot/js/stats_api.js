/**
 * Gets the pronunciation analytics for a user
 * @param {string} url
 * @param {string} language
 * @param {int} maxDaysAgo
 * @returns {PronounciationAnalyticsModel}
 */
export async function fetchPronunciationAnalytics(url, language = null, maxDaysAgo = null) {
    // Building the query string with optional parameters
    const queryParams = new URLSearchParams();
    if (language) queryParams.append('language', language);
    if (maxDaysAgo) queryParams.append('maxDaysAgo', maxDaysAgo.toString());

    const fullUrl = `${url}?${queryParams.toString()}`;

    const response = await fetch(fullUrl, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        if (response.status === 404) {
            throw new Error('Analytics data not found. You might have requested a deletion or we are having troubles getting it for you.');
        }
        console.warn(`Unsuccessful response when fetching pronunciation analytics: ${response.status}`);
        throw new Error('Could not fetch pronunciation analytics. Try again later.');
    }

    return await response.json();
}
