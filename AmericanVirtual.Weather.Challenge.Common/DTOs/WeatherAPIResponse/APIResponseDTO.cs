public class APIResponseDTO
{
    public string Lat { get; set; }
    public string Lon { get; set; }
    public double Elevation { get; set; }
    public string Timezone { get; set; }
    public string Units { get; set; }
    public CurrentAPIResponseDTO Current { get; set; }
    public HourlyAPIResponseDTO Hourly { get; set; }
    public DailyAPIResponseDTO Daily { get; set; }
}
