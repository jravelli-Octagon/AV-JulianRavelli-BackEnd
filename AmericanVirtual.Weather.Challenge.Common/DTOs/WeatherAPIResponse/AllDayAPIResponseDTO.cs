public class AllDayAPIResponseDTO
{
    public string Weather { get; set; }
    public int Icon { get; set; }
    public double Temperature { get; set; }
    public double Temperature_min { get; set; }
    public double Temperature_max { get; set; }
    public WindAPIResponseDTO Wind { get; set; }
    public CloudCoverAPIResponseDTO Cloud_cover { get; set; }
    public PrecipitationAPIResponseDTO Precipitation { get; set; }
}
