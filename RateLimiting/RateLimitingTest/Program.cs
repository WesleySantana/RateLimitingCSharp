using RateLimitingTest;

var client = new HttpClient();

// URL do endpoint da sua API
var url = "http://localhost:5299/WeatherForecast";

Console.WriteLine("Iniciando teste de Rate Limiter...\n");

await BurstTest.RateLimiterTest(url);

Console.WriteLine("\nTeste concluído!");
Console.ReadLine();