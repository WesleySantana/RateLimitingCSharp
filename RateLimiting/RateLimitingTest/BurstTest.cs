namespace RateLimitingTest;
public static class BurstTest
{
    public static async Task RateLimiterTest(string url)
    {
        using var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        int total = 150; // mais do que o PermitLimit
        var tasks = Enumerable.Range(1, total).Select(async i =>
        {
            var resp = await client.GetAsync(url);
            Console.WriteLine($"[{i}] {(int)resp.StatusCode} {resp.ReasonPhrase}");
        });

        await Task.WhenAll(tasks);
    }
}
