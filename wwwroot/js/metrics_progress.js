
const pronounciationProgress = document.getElementById('pronounciationProgress');
const accuracyBar = document.getElementById('accuracyBar');
const fluencyBar = document.getElementById('fluencyBar');
const prosodyBar = document.getElementById('prosodyBar');
const accuracyScore = document.getElementById('accuracyScore');
const fluencyScore = document.getElementById('fluencyScore');
const prosodyScore = document.getElementById('prosodyScore');

export function showMetrics(accuracy = null, fluency = null, prosody = null) {
    pronounciationProgress.classList.remove('d-none');
    pronounciationProgress.classList.remove('fade-out');
    accuracy ??= 100;
    fluency ??= 100;
    prosody ??= 100; // 100 default value for all metrics

    accuracyBar.style.width = `${accuracy}%`;
    fluencyBar.style.width = `${fluency}%`;
    prosodyBar.style.width = `${prosody}%`;

    accuracyScore.innerText = `${Math.round(accuracy)}%`;
    fluencyScore.innerText = `${Math.round(fluency)}%`;
    prosodyScore.innerText = `${Math.round(prosody)}%`;
}

export function hideMetrics() {
    pronounciationProgress.classList.add('fade-out');
}