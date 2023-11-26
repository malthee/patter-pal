/**
 * Fetches all converstations of the user
 * @param {string} url
 * @returns {Promise<ConversationModel[]>}
 */
export async function fetchConversations(url) {
    const response = await fetch(url, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        console.warn(`Unsuccessful response when fetching conversations: ${response.status}`);
        throw new Error('Could not get conversations. Try again later.');
    }

    return await response.json();
}

/**
 * Fetches a chat list for a conversation
 * @param {string} url
 * @param {string} conversationId
 * @returns {Promise<ChatMessageModel[]>}
 */
export async function fetchChatsByConversationId(url, conversationId) {
    const response = await fetch(`${url}?conversationId=${encodeURIComponent(conversationId)}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        console.warn(`Unsuccessful response when fetching chats: ${response.status}`);
        throw new Error('Could not get list of chats. Try again later.');
    }

    return await response.json();
}

/**
 * Shallow updates the conversation
 * @param {string} url
 * @param {ConversationModel} conversation
 */
export async function updateConversation(url, conversation) {
    const response = await fetch(url, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(conversation)
    });

    if (!response.ok) {
        console.warn(`Unsuccessful response when updating conversation: ${response.status}`);
        if (response.status === 404) {
            throw new Error('Could not find the conversation. It may not exist anymore.');
        }
        throw new Error('Could not update the conversation. Try again later.');
    }
}

/**
 * Deletes a conversation
 * @param {string} url
 * @param {string} conversationId
 */
export async function deleteConversation(url, conversationId) {
    const response = await fetch(`${url}?conversationId=${encodeURIComponent(conversationId)}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        console.warn(`Unsuccessful response when deleting conversation: ${response.status}`);
        if (response.status === 404) {
            throw new Error('Could not find the conversation. It may not exist anymore.');
        }
        throw new Error('Could not delete the conversation. Try again later.');
    }
}
