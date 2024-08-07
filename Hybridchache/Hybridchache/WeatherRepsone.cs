namespace Hybridchache;

public class Weather
{
    public int Id { get; set; }
    public string Main { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class WeatherResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Weather> Weather { get; set; }
    public string Base { get; set; }
    public int Visibility { get; set; }
    public int Timezone { get; set; }
}