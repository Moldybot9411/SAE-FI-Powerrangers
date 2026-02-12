// js/main.js

import { updateChart, resetChartZoom } from "./chart-config.js";
import { getMeasurementsForSensor } from "./api.js";
import {
  applyHostStyles,
  setLoading,
  populateSensorList,
  setActiveSensor,
  setDatePickers,
  getDateRange,
  clearDatePickers,
} from "./ui-handler.js";

// Globale Schnittstellen für die WebView2-Hostanwendung
window.applyHostStyles = applyHostStyles;
window.setupDashboard = setupDashboard;

let currentSensor = null;

// Initialisiert das deutsche Locale für Moment.js
moment.locale("de");

/**
 * Haupt-Setup-Funktion, die vom Host aufgerufen wird.
 */
async function setupDashboard(
  availableSensors,
  initialSensorData,
  initialSelectedSensor,
) {
  populateSensorList(availableSensors);

  currentSensor = initialSelectedSensor;
  if (currentSensor) {
    updateChart(initialSensorData, currentSensor);
    setActiveSensor(currentSensor);
    if (initialSensorData && initialSensorData.length > 0) {
      const firstTimestamp = initialSensorData[0].Timestamp;
      const lastTimestamp =
        initialSensorData[initialSensorData.length - 1].Timestamp;
      setDatePickers(firstTimestamp, lastTimestamp);
    }
  } else {
    updateChart([], "Keine Daten");
  }

  // Event Listener registrieren
  document
    .getElementById("sensorList")
    .addEventListener("click", handleSensorClick);
  document
    .getElementById("filterButton")
    .addEventListener("click", handleFilterClick);
  document
    .getElementById("resetFilterButton")
    .addEventListener("click", handleResetFilterClick);
  document
    .getElementById("resetZoomButton")
    .addEventListener("click", resetChartZoom);
}

/**
 * Lädt Daten für einen neu ausgewählten Sensor.
 */
async function handleSensorClick(event) {
  const target = event.target;
  if (target && target.nodeName === "LI" && target.dataset.sensorName) {
    currentSensor = target.dataset.sensorName;
    await loadDataForSensor(currentSensor);
  }
}

/**
 * Wendet den ausgewählten Zeitfilter an.
 */
async function handleFilterClick() {
  if (!currentSensor) {
    alert("Bitte wählen Sie zuerst einen Sensor aus.");
    return;
  }
  const { start, end } = getDateRange();
  await loadDataForSensor(currentSensor, start, end);
}

/**
 * Setzt den Zeitfilter zurück und lädt die Daten für den gesamten Zeitraum.
 */
async function handleResetFilterClick() {
  if (!currentSensor) return;

  clearDatePickers();
  await loadDataForSensor(currentSensor);
}

/**
 * Lade-Wrapper-Funktion, die die UI blockiert und Daten von der API abruft.
 */
async function loadDataForSensor(sensorName, startTime = null, endTime = null) {
  setLoading(true);
  try {
    const measurements = await getMeasurementsForSensor(
      sensorName,
      startTime,
      endTime,
    );
    updateChart(measurements, sensorName);
    setActiveSensor(sensorName);

    // Wenn ohne Filter geladen wurde, die Datepicker auf den vollen Bereich setzen
    if (measurements.length > 0 && !startTime && !endTime) {
      const firstTimestamp = measurements[0].Timestamp;
      const lastTimestamp = measurements[measurements.length - 1].Timestamp;
      setDatePickers(firstTimestamp, lastTimestamp);
    }
  } catch (error) {
    console.error("Fehler beim Laden der Sensordaten:", error);
    alert(
      "Fehler beim Laden der Sensordaten. Prüfen Sie die Konsole für Details.",
    );
  } finally {
    setLoading(false);
  }
}
