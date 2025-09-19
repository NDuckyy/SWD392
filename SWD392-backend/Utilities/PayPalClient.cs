using PayPalCheckoutSdk.Core;
using PayPalHttp;
using Microsoft.Extensions.Configuration;

public class PayPalClient
{
    private readonly IConfiguration _config;

    public PayPalClient(IConfiguration config)
    {
        _config = config;
    }

    public PayPalEnvironment Environment =>
        new SandboxEnvironment(_config["PayPal:ClientId"], _config["PayPal:Secret"]);

    public PayPalHttpClient Client => new PayPalHttpClient(Environment); // 👈 KHÔNG phải System.Net.Http.HttpClient
}