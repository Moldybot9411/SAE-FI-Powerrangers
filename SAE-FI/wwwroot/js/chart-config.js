let temperatureChart;

export function updateChart(measurements, sensorName) {
  const ctx = document.getElementById("temperatureChart").getContext("2d");

  const chartData = (measurements || []).map((m) => ({
    x: moment.utc(m.Timestamp),
    y: m.Value,
  }));

  if (temperatureChart) {
    temperatureChart.destroy();
  }

  temperatureChart = new Chart(ctx, {
    type: "line",
    data: {
      datasets: [
        {
          label: `Temperatur (${sensorName || "Unbekannt"})`,
          data: chartData,
          borderColor: "rgb(0, 150, 255)",
          backgroundColor: "rgba(0, 150, 255, 0.2)",
          tension: 0.1,
          fill: false,
          pointRadius: 0,
          hitRadius: 10,
          hoverRadius: 5,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      color: "#e0e0e0",
      animation: false,
      elements: {
        line: {
          tension: 0.4,
        },
        point: {
          radius: 0,
          hoverRadius: 1,
        },
      },
      scales: {
        x: {
          type: "time",
          adapters: {
            date: {
              zone: "UTC",
            },
          },
          time: {
            displayFormats: {
              millisecond: "HH:mm:ss.SSS",
              second: "HH:mm:ss",
              minute: "HH:mm",
              hour: "HH:mm",
              day: "DD. MMM",
              week: "DD. MMM",
              month: "MMM YYYY",
              quarter: "[Q]Q YYYY",
              year: "YYYY",
            },
            tooltipFormat: "DD.MM.YYYY HH:mm:ss",
          },
          title: { display: true, text: "Zeitstempel", color: "#c0c0c0" },
          ticks: { color: "#c0c0c0" },
          grid: { color: "rgba(255, 255, 255, 0.1)" },
        },
        y: {
          title: { display: true, text: "Temperatur (°C)", color: "#c0c0c0" },
          ticks: { color: "#c0c0c0" },
          grid: { color: "rgba(255, 255, 255, 0.1)" },
        },
      },
      plugins: {
        legend: { labels: { color: "#e0c0c0" } },
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          titleColor: "#ffffff",
          bodyColor: "#dddddd",
          callbacks: {
            title: function (context) {
              return moment
                .utc(context[0].parsed.x)
                .format("DD.MM.YYYY HH:mm:ss");
            },
            label: (ctx) => `Temperatur: ${ctx.parsed.y.toFixed(2)} °C`,
          },
        },
        decimation: {
          enabled: true,
          algorithm: "lttb",
          samples: 1000,
          threshold: 5000,
        },
        zoom: {
          pan: { enabled: true, mode: "x" },
          zoom: {
            wheel: { enabled: true },
            pinch: { enabled: true },
            mode: "x",
          },
        },
      },
    },
  });
}

export function resetChartZoom() {
  if (temperatureChart) {
    temperatureChart.resetZoom();
  }
}
