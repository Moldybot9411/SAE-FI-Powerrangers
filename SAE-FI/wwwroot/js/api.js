export async function getMeasurementsForSensor(
  sensorName,
  startTime = null,
  endTime = null,
) {
  if (!window.chrome.webview.hostObjects.backend) {
    throw new Error("Backend-API nicht verfügbar.");
  }
  const jsonMeasurements =
    await window.chrome.webview.hostObjects.backend.GetMeasurementsForSensor(
      sensorName,
      startTime,
      endTime,
    );
  return JSON.parse(jsonMeasurements);
}
