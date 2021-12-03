public class WeatherResponse
{
    public Coordinate coord { get; set; }

    public WeatherCity[] weather; 

    public Temperature main { get; set; }

    public Wind wind { get; set; }

    public string name { get; set; }

    public Sys sys { get; set; }
}
