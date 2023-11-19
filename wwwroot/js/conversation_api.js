export async function fetchConversationAnswer(url, text, language, id=null) {
    try {
        const body = {
            Text: text,
            Language: language,
            Id: id
        };

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Error fetching conversation data:', error);
        throw error;
    }
}