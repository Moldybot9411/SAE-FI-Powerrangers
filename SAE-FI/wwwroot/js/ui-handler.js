const sensorListEl = document.getElementById("sensorList");
const loadingIndicatorEl = document.getElementById("loadingIndicator");
const startDateEl = document.getElementById("startDate");
const endDateEl = document.getElementById("endDate");

export function applyHostStyles(backgroundColor) {
  document.documentElement.style.setProperty(
    "--app-background-color",
    backgroundColor,
  );
}

export function setLoading(isLoading) {
  loadingIndicatorEl.style.display = isLoading ? "flex" : "none";
  document.body.style.pointerEvents = isLoading ? "none" : "auto";
}

export function setActiveSensor(sensorName) {
  sensorListEl.querySelectorAll("li").forEach((li) => {
    li.classList.remove("active");
  });
  if (sensorName) {
    const activeListItem = sensorListEl.querySelector(
      `li[data-sensor-name="${sensorName}"]`,
    );
    if (activeListItem) {
      activeListItem.classList.add("active");
    }
  }
}

export function populateSensorList(sensors) {
  sensorListEl.innerHTML = "";
  if (sensors && sensors.length > 0) {
    sensors.forEach((sensor) => {
      const listItem = document.createElement("li");
      listItem.textContent = sensor;
      listItem.dataset.sensorName = sensor;
      sensorListEl.appendChild(listItem);
    });
  } else {
    sensorListEl.innerHTML = "<li>Keine Sensoren gefunden</li>";
  }
}

function toLocalInputFormat(utcTimestamp) {
  if (!utcTimestamp) return "";
  const date = new Date(utcTimestamp);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

export function setDatePickers(firstTimestamp, lastTimestamp) {
  if (startDateEl && firstTimestamp) {
    startDateEl.value = toLocalInputFormat(firstTimestamp);
  }
  if (endDateEl && lastTimestamp) {
    endDateEl.value = toLocalInputFormat(lastTimestamp);
  }
}

export function getDateRange() {
  const toUTCISOString = (localDateString) => {
    if (!localDateString) return null;
    return new Date(localDateString).toISOString();
  };

  return {
    start: toUTCISOString(startDateEl.value),
    end: toUTCISOString(endDateEl.value),
  };
}

export function clearDatePickers() {
  if (startDateEl) startDateEl.value = "";
  if (endDateEl) endDateEl.value = "";
}
