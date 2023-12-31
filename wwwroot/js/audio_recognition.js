﻿/// Records audio in the browser and sends it to the server in chunks for speech recognition
export class AudioRecognitionStreamer {
    #socket;
    #audioContext;
    #processor;
    #intervalId;
    #targetSampleRate;
    #recordingBufferSize;
    #chunkTimeMs;
    #audioBuffer;

    constructor(socket, recordingBufferSize, chunkTimeMs, targetSampleRate) {
        this.#recordingBufferSize = recordingBufferSize;
        this.#chunkTimeMs = chunkTimeMs;
        this.#audioBuffer = []; // Stores audio data before sending
        this.#targetSampleRate = targetSampleRate;
        this.#socket = socket;
    }

    async startRecording() {
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
        this.#intervalId = setInterval(() => this.sendAudioData(), this.#chunkTimeMs);
    }

    /// Gracefully stops recording, signalling the end of the audio stream
    stopRecording() {
        clearInterval(this.#intervalId);
        this.sendAudioData(); // Send data one more time
        this.#socket.send(new ArrayBuffer(1)); // Mark end
        this.abortRecording();
    }

    /// Aborts recording, without signalling the end of the audio stream
    abortRecording() {
        clearInterval(this.#intervalId);
        this.#processor?.disconnect();
        this.#audioContext?.close();
        this.#processor = null;
        this.#audioContext = null;
        this.#audioBuffer = [];
    }

    // --- Private methods (since not fully supported by browser yet, they are not prefixxed with #) --- //
    sendAudioData() {
        if (this.#socket.readyState === WebSocket.OPEN && this.#audioBuffer.length > 0) {
            // Convert to supported audio format
            const concatenatedBuffer = concatenateBuffers(this.#audioBuffer);
            const resampledBuffer = resampleBuffer(concatenatedBuffer, this.#targetSampleRate, this.#audioContext);
            const int16Buffer = convertFloat32ToInt16(resampledBuffer);
            this.#socket.send(int16Buffer);
            this.#audioBuffer = []; // Clear the buffer after sending
        } 
    }
}

// --- Helper functions for audio processing --- //
/// Linear interpolation resampling
function resampleBuffer(buffer, targetSampleRate, audioContext) {
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