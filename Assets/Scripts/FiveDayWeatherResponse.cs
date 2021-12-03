using System.Collections.Generic;

public class FiveDayWeatherResponse
{
    public Coordinate coord { get; set; }

    //public Temperature main { get; set; }

    public City city { get; set; }

    public List<FiveDayMain> list = new List<FiveDayMain>();
}
