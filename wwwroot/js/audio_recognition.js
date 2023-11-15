
// TODO move logic from index.cshtml
const TARGET_SAMPLE_RATE = 16000; // As required by Azure Speech

class AudioRecognitionStreamer {
    #socket;
    #audioContext;
    #processor;
    #intervalId;
    #targetSampleRate;

    constructor(socket, recordingBufferSize, chunkTimeMs, targetSampleRate, onResult) {
        this.#recordingBufferSize = recordingBufferSize;
        this.#chunkTimeMs = chunkTimeMs;
        this.#audioBuffer = []; // Stores audio data before sending
        this.#targetSampleRate = targetSampleRate;
        this.#socket = socket;
        this.onResult = onResult;
    }

    async startRecording() {
        try {     
            // Initialize Audio Context and Processor
            this.#audioContext = new (window.AudioContext || window.webkitAudioContext)();
            this.#processor = this.#audioContext.createScriptProcessor(this.#recordingBufferSize, 1, 1); // Buffer size, input channels, output channels

            // Get audio stream from microphone
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const source = this.#audioContext.createMediaStreamSource(stream);
            source.connect(this.#processor);
            this.#processor.connect(this.#audioContext.destination);

            this.#processor.onaudioprocess = (e) => {
                const inputData = e.inputBuffer.getChannelData(0);
                this.#audioBuffer.push(new Float32Array(inputData)); // Store raw float32 data to be processed later
            };

            // Set an interval to send audio data every few seconds
            this.#intervalId = setInterval(sendAudioData, this.#chunkTimeMs);
        } catch (error) {
            alert(error);
        }
    }

    sendAudioData() {
        if (this.#socket.readyState === WebSocket.OPEN && audioBuffer.length > 0) {
            // Convert to 16khz 16-bit signed integer PCM audio
            const concatenatedBuffer = concatenateBuffers(audioBuffer);
            const resampledBuffer = resampleBuffer(concatenatedBuffer, TARGET_SAMPLE_RATE);  
            const int16Buffer = convertFloat32ToInt16(resampledBuffer); 
            this.#socket.send(int16Buffer);
            audioBuffer = []; // Clear the buffer after sending
        }
    }

    stopRecording() {
        clearInterval(this.#intervalId);
        this.#processor?.disconnect();
        this.#audioContext?.close();
    }
}

// --- Helper functions for audio processing --- //
/// Linear interpolation resampling
function resampleBuffer(buffer, targetSampleRate) {
    if (!(audioContext?.sampleRate)) {
        console.error('Audio context not initialized, cannot resample');
        return buffer;
    }

    const sourceSampleRate = audioContext.sampleRate;
    const sourceLength = buffer.length;
    const targetLength = Math.round(sourceLength * targetSampleRate / sourceSampleRate);
    const resampledBuffer = new Float32Array(targetLength);

    for (let i = 0; i < targetLength; i++) {
        const srcIndex = i * sourceSampleRate / targetSampleRate;
        const srcIndexFloor = Math.floor(srcIndex);
        const srcIndexCeil = Math.min(sourceLength - 1, srcIndexFloor + 1);
        const weight = srcIndex - srcIndexFloor;
        resampledBuffer[i] = (1 - weight) * buffer[srcIndexFloor] + weight * buffer[srcIndexCeil];
    }

    return resampledBuffer;
}

/// Merge the array of buffers into one Float32Array
function concatenateBuffers(buffers) {
    let totalLength = buffers.reduce((acc, value) => acc + value.length, 0);
    let result = new Float32Array(totalLength);
    let offset = 0;

    for (let buffer of buffers) {
        result.set(buffer, offset);
        offset += buffer.length;
    }

    return result;
}

/// Converts the buffer from Float32 to Int16
function convertFloat32ToInt16(buffer) {
    let l = buffer.length;
    let buf = new Int16Array(l);
    while (l--) {
        buf[l] = Math.min(1, buffer[l]) * 0x7FFF;
    }
    return buf.buffer;
}