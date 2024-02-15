public class DataAPIResponseDTO
{
    public DateTime Date { get; set; }
    public string Weather { get; set; }
    public int Icon { get; set; }
    public string Summary { get; set; }
    public double Temperature { get; set; }
    public WindAPIResponseDTO Wind { get; set; }
    public CloudCoverAPIResponseDTO Cloud_cover { get; set; }
    public PrecipitationAPIResponseDTO Precipitation { get; set; }
    public string Day { get; set; }
    public AllDayAPIResponseDTO All_day { get; set; }
    public object Morning { get; set; }
    public object Afternoon { get; set; }
    public object Evening { get; set; }

    public DataAPIResponseDTO()
    {
        Date = Day != null ? Convert.ToDateTime(Day) : DateTime.MinValue;
    }
}
