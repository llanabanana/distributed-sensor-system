// Konfiguracija – prilagodi adresu svog NotificationService-a
const NOTIFICATION_URL = "http://localhost:5163";   // tvoj HTTPS port (ili HTTP: http://localhost:5163)
const HUB_ENDPOINT = "/alarmHub";

let connection = null;

// Elementi DOM-a
const statusSpan = document.getElementById("connectionStatus");
const reconnectBtn = document.getElementById("reconnectBtn");
const alarmList = document.getElementById("alarmList");

// Pomoćna funkcija za prikazivanje datuma u lokalnom vremenu
function formatTime(isoString) {
    if (!isoString) return new Date().toLocaleTimeString();
    const date = new Date(isoString);
    return date.toLocaleTimeString() + ":" + date.getMilliseconds().toString().padStart(3, '0');
}

// Dodavanje alarma u listu
function addAlarmToList(alarm) {
    // Ukloni placeholder ako postoji
    if (alarmList.children.length === 1 && alarmList.children[0].classList.contains("placeholder")) {
        alarmList.innerHTML = "";
    }

    const li = document.createElement("li");
    li.className = `alarm-item priority-${alarm.priority}`;

    const sensorSpan = document.createElement("span");
    sensorSpan.className = "sensor";
    sensorSpan.textContent = `📡 ${alarm.sensorId}`;

    const tempSpan = document.createElement("span");
    tempSpan.className = "temp";
    tempSpan.textContent = `${alarm.temperature.toFixed(1)}°C`;

    const prioritySpan = document.createElement("span");
    prioritySpan.className = "priority";
    prioritySpan.textContent = `PRIORITET ${alarm.priority}`;

    const timeSpan = document.createElement("span");
    timeSpan.className = "time";
    timeSpan.textContent = formatTime(alarm.timestamp);

    li.appendChild(sensorSpan);
    li.appendChild(tempSpan);
    li.appendChild(prioritySpan);
    li.appendChild(timeSpan);

    // Dodaj na početak liste (najnoviji gore)
    alarmList.prepend(li);

    // Opcionalno: zvučni signal ako je prioritet 3
    if (alarm.priority >= 2) {
        playBeep(alarm.priority);
    }
}

// Zvučni signal (jednostavan "pip" preko Web Audio API)
function playBeep(priority) {
    try {
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();
        oscillator.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        oscillator.frequency.value = priority === 3 ? 880 : 660;
        gainNode.gain.value = 0.3;
        oscillator.start();
        gainNode.gain.exponentialRampToValueAtTime(0.00001, audioCtx.currentTime + 0.6);
        oscillator.stop(audioCtx.currentTime + 0.6);
        // zatvori audio context nakon kraja
        setTimeout(() => audioCtx.close(), 700);
    } catch(e) {
        // ako browser blokira audio, ignoriši
        console.warn("Audio nije dozvoljen ili nije podržan");
    }
}

// Ažuriranje statusa konekcije
function updateStatus(connected, message = "") {
    if (connected) {
        statusSpan.textContent = "Connected to SignalR";
        statusSpan.className = "connected";
        reconnectBtn.style.display = "none";
    } else {
        statusSpan.textContent = message || "Connection broken";
        statusSpan.className = "disconnected";
        reconnectBtn.style.display = "inline-block";
    }
}

// Glavna funkcija za uspostavljanje konekcije
async function startConnection() {
    if (connection) {
        try { await connection.stop(); } catch(e) { }
    }

    updateStatus(false, "Connecting...");
    statusSpan.className = "connecting";

    const fullUrl = `${NOTIFICATION_URL}${HUB_ENDPOINT}`;

    connection = new signalR.HubConnectionBuilder()
        .withUrl(fullUrl)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // retry intervals
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Registracija handlera za ReceiveAlarm
    connection.on("ReceiveAlarm", (alarm) => {
        console.log("Alarm received:", alarm);
        addAlarmToList(alarm);
    });

    // Handlers za konekciju
    connection.onclose((error) => {
        console.error("Connection closed:", error);
        updateStatus(false, "Connection closed");
    });

    connection.onreconnecting((error) => {
        console.warn("Reconnecting...", error);
        updateStatus(false, "Reconnecting...");
    });

    connection.onreconnected((connectionId) => {
        console.log("Reconnected, connectionId:", connectionId);
        updateStatus(true);
    });

    try {
        await connection.start();
        console.log("SignalR connected!");
        updateStatus(true);
    } catch (err) {
        console.error("Failed to connect:", err);
        updateStatus(false, "Failed connecting");
    }
}

// Dugme za ručno ponovno povezivanje
reconnectBtn.addEventListener("click", () => {
    startConnection();
});

// Pokretanje konekcije pri učitavanju stranice
startConnection();

// Opciono: osvježi listu svakih 30 sekundi (čisto vizuelno)
setInterval(() => {
    if (alarmList.children.length === 0) {
        if (alarmList.innerHTML === "" || !document.querySelector(".placeholder")) {
            alarmList.innerHTML = '<li class="placeholder">Čekam alarme...</li>';
        }
    }
}, 10000);