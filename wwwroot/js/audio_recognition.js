﻿
class AudioRecognitionStreamer {
    #socket;
    #audioContext;
    #processor;
    #intervalId;
    #targetSampleRate;

    constructor(socketUrl, recordingBufferSize, chunkTimeMs, targetSampleRate, onResult) {
        this.#recordingBufferSize = recordingBufferSize;
        this.#chunkTimeMs = chunkTimeMs;
        this.#audioBuffer = []; // Stores audio data before sending
        this.#targetSampleRate = targetSampleRate;
        this.onResult = onResult;
    }

    async startRecording() {
        try {
            // Initialize WebSocket connection
            this.#socket = new WebSocket(socketUrl);

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

    setupSocket() {

    }

    sendAudioData() {
        if (socket.readyState === WebSocket.OPEN && audioBuffer.length > 0) {
            let concatenatedBuffer = concatenateBuffers(audioBuffer);
            let resampledBuffer = resampleBuffer(concatenatedBuffer, 16000); // Resample to 16kHz as supported by speech
            let int16Buffer = convertFloat32ToInt16(resampledBuffer); // Convert to 16-bit signed integer PCM audio
            socket.send(int16Buffer);
            audioBuffer = []; // Clear the buffer after sending
        }
    }

    stopRecording() {

    }
}

// --- Helper functions for audio processing ---
// Inspired by https://medium.com/(AT)ragymorkos/gettineg-monochannel-16-bit-signed-integer-pcm-audio-samples-from-the-microphone-in-the-browser-8d4abf81164d
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

function convertFloat32ToInt16(buffer) {
    let l = buffer.length;
    let buf = new Int16Array(l);
    while (l--) {
        buf[l] = Math.min(1, buffer[l]) * 0x7FFF;
    }
    return buf.buffer;
}