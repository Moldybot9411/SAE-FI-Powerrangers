using Newtonsoft.Json;
using SAE_FI.Services;

public class BackendApi(MeasurementRepository repository)
{
    private readonly MeasurementRepository _repository = repository;

    public string GetDistinctSensors()
    {
        var sensors = _repository.GetDistinctSensors();
        return JsonConvert.SerializeObject(sensors);
    }

    public string GetMeasurementsForSensor(string sensorName, string? startTime = null, string? endTime = null)
    {
        DateTime? start = null;
        if (!string.IsNullOrEmpty(startTime))
        {
            start = DateTime.Parse(startTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        DateTime? end = null;
        if (!string.IsNullOrEmpty(endTime))
        {
            end = DateTime.Parse(endTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        var measurements = _repository.GetMeasurementsForSensor(sensorName, start, end);

        var chartData = measurements.Select(m => new { m.Timestamp, m.Value }).ToList();
        
        var jsonSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified 
        };
        return JsonConvert.SerializeObject(chartData, jsonSettings);
    }
}