namespace RentCarX
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        //siemanoooooo000

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
