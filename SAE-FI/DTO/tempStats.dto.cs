public record TemperatureExtreme(
    long Id,
    double Value,
    string Sensor,
    DateTime Timestamp
);

public record TemperatureAverage(
    double Value
);

public record TemperatureStats(
    DateTime StartDate,
    DateTime EndDate,
    TemperatureAverage Average,
    TemperatureExtreme Min,
    TemperatureExtreme Max
);

public record SensorStats(
    string SensorId,
    double Temperature
);

