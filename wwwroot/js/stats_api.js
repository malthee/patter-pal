/**
 * Gets the pronunciation analytics for a user
 * @param {string} url
 * @param {string} language
 * @param {string} timePeriod
 * @param {string} timeResolution
 * @returns {PronounciationAnalyticsModel}
 */
export async function fetchPronunciationAnalytics(url, language = null, timePeriod = null, timeResolution = null) {
    // Building the query string with optional parameters
    const queryParams = new URLSearchParams();
    if (language) queryParams.append('language', language.toString());
    if (timePeriod) queryParams.append('timePeriod', timePeriod.toString());
    if (timeResolution) queryParams.append('timeResolution', timeResolution.toString());

    const fullUrl = `${url}?${queryParams.toString()}`;
    console.log('fullUrl: ', fullUrl);

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
