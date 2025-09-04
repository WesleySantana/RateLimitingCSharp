
# .NET 8 Rate Limiting — Minimal API + Burst Test

A tiny, no-frills sample that demonstrates the **native Rate Limiter** in **.NET 8** using a **Fixed Window** policy on a simple `WeatherForecast` endpoint — plus a separate **console client** that fires a burst of requests to trigger **HTTP 429 (Too Many Requests)**.

## Why this project?

- Show a **minimal** setup of the built-in rate limiter (no libraries needed).
- Apply the policy to a specific controller with `[EnableRateLimiting("Fixed")]`.
- Reproduce 429s reliably using a small **burst test** console app.

---

## Requirements

- .NET SDK **8.0+**
- Windows/macOS/Linux
- (Optional) Trust the local dev cert for HTTPS:
  ```bash
  dotnet dev-certs https --trust
  ```

---

## Project layout

```
/RateLimiting.Api         # ASP.NET Core Web API (WeatherForecast)
  Program.cs
  Controllers/
    WeatherForecastController.cs  # [EnableRateLimiting("Fixed")]

/BurstTest                 # Console app that fires N parallel requests
  BurstTest.cs
```

---

## How it works (concepts)

- **Fixed Window limiter**: counts requests in fixed time blocks (e.g., every 10s or 30s). When `PermitLimit` is reached, extra requests are either **queued** (if `QueueLimit > 0`) or **rejected** with the configured status (e.g., **429**).
- **Policy name**: we call it `"Fixed"`.
- **Applying to endpoints**: either via the attribute `[EnableRateLimiting("Fixed")]` in the controller/action, or globally with `app.MapControllers().RequireRateLimiting("Fixed")`.

---

## Quick start

### 1) Run the API

From the API project folder:

```bash
dotnet restore
dotnet run
```

You should see the application URLs in the console, for example:
- `https://localhost:5001`
- `http://localhost:5000`

Swagger is usually at `/swagger`.

> The `WeatherForecast` controller is decorated with `[EnableRateLimiting("Fixed")]`, so the policy applies to that endpoint only.

### 2) Run the Burst Test (console)

From the console project folder:

```bash
dotnet restore
dotnet run -- https://localhost:5001/WeatherForecast
```

- If you omit the URL argument, the client uses its default URL; passing the API URL explicitly is recommended.
- The sample client sends many **parallel** requests to exceed the limit in a single window and print results (200 vs 429).

---

## Recommended test settings

You have two easy ways to see **429** quickly:

### Option A — Keep your real limits, increase the burst
If your API limiter is something like:

- `PermitLimit = 100`
- `Window = 30s`
- `QueueLimit = 2`
- `RejectionStatusCode = 429`

Then run the Burst Test with a **burst bigger than (PermitLimit + QueueLimit)**, e.g., 150+ requests sent **simultaneously**.

### Option B — Make a tiny window (for demo)
Temporarily lower the limits on the API to force 429 more easily:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddFixedWindowLimiter("Fixed", config =>
    {
        config.PermitLimit = 10;                    // small quota (easy to exceed)
        config.Window = TimeSpan.FromSeconds(10);   // short window
        config.QueueLimit = 0;                      // no queue -> exceed => reject
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
```

Now, in the Burst Test, send more than 10 simultaneous requests (e.g., 50). You should see several **429** entries.

---

## Expected output (console client)

You should see a mix of:

```
[1] 200 OK
[2] 200 OK
...
[11] 429 Too Many Requests
[12] 429 Too Many Requests
...
```

If you only see `200 OK` for all requests, check the **Troubleshooting** section below.

---

## Applying the policy to the controller

In the `WeatherForecastController`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("Fixed")]
public class WeatherForecastController : ControllerBase
{
    // actions...
}
```

Alternatively, you can apply it globally to all controllers:

```csharp
app.MapControllers().RequireRateLimiting("Fixed");
```

> Make sure `app.UseRateLimiter();` is in the middleware pipeline (typically after Swagger and before `MapControllers()`).

---

## Troubleshooting (no 429?)

- **Requests not parallel**: If your client sends requests **sequentially**, you may never exceed the quota within the same window. Use the provided **parallel** burst approach.
- **Burst too small**: Send more requests than `(PermitLimit + QueueLimit)` **inside the same window**.
- **Window too long**: With long windows and slow clients, the window may **roll** before you exceed the limit. Use a **shorter** window for testing.
- **Queue hides 429**: If `QueueLimit` > 0, some over-limit calls will wait instead of being rejected. Set `QueueLimit = 0` to force rejection, or send a much bigger burst.
- **Policy not applied**: Ensure `[EnableRateLimiting("Fixed")]` is on the controller/action, or you used `RequireRateLimiting("Fixed")` at mapping time.
- **Wrong URL/port**: The console client must target the exact `https://localhost:{port}/WeatherForecast` your API printed at startup.
- **HTTPS dev cert**: If you see cert errors, either trust the dev cert (`dotnet dev-certs https --trust`) or have the client bypass it (the sample handler can ignore cert errors in dev).

---

## Notes

- This sample uses **Fixed Window** for clarity. For production, consider other strategies (e.g., Sliding Window, Token Bucket) depending on your traffic shape.
- Tune `PermitLimit`, `Window`, and `QueueLimit` according to your SLA and expected traffic bursts.

---

## License

MIT — do whatever you like, attribution appreciated.
