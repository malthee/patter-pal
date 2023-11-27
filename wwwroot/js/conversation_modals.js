import * as conversationApi from '/js/conversation_api.js'

const deleteModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'))
const deleteConfirmButton = document.getElementById('deleteConfirmButton');
const modifyModal = new bootstrap.Modal(document.getElementById('modifyModal'));
const modifyTitle = document.getElementById('modifyConversationTitle');
const modifyConfirmButton = document.getElementById('modifyConfirmButton');

/**
 * Shows the deletion modal for a conversation.
 * @param {string} url of api endpoint
 * @param {string} conversationId
 * @param {function(conversationId)} onDelete called on success
 */
export function showDeleteModal(url, conversationId, onDelete) {
    // Handle deletion and call onDelete on success
    deleteConfirmButton.onclick = async () => {
        try {
            deleteModal.hide();
            await conversationApi.deleteConversation(url, conversationId);
            onDelete(conversationId);
            showToast('Successfully deleted.', 'success');
        } catch (e) {
            showToast(e.message, 'danger');
        }
    };

    deleteModal.show();
}

/**
 * Shows the modification modal for a conversation.
 * @param {string} url of api endpoint
 * @param {string} conversationId
 * @param {string} title
 * @param {function(ConversationModel)} onModify called on success with updated values
 */
export function showModifyModal(url, conversationId, title, onModify) {
    modifyTitle.value = title;

    // Do api logic and hide modal on click
    modifyConfirmButton.onclick = async () => {
        try {
            const conversation = { 'Title': modifyTitle.value, 'Id': conversationId };
            modifyModal.hide();
            await conversationApi.updateConversation(url, conversation);
            showToast('Successfully updated.', 'success');
            onModify(conversation);
        } catch (e) {
            showToast(e.message, 'danger');
        }
    };

    modifyModal.show();
}