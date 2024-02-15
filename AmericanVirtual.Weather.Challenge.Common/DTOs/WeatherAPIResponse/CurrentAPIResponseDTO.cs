public class CurrentAPIResponseDTO
{
    public string Icon { get; set; }
    public int Icon_num { get; set; }
    public string Summary { get; set; }
    public double Temperature { get; set; }
    public WindAPIResponseDTO Wind { get; set; }
    public PrecipitationAPIResponseDTO Precipitation { get; set; }
    public double Cloud_cover { get; set; }
}
