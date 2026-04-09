using AutoHitCounter.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Utilities;

namespace AutoHitCounter.Services;

public class ExternalIntegrationService : IExternalHitService
{
    private readonly HttpClient _httpClient;

    public ExternalIntegrationService()
    {
        _httpClient = new HttpClient();
    }

    public async Task SendHitAsync(HitPayload payload)
    {
        if (!SettingsManager.Default.ExternalIntegrationEnabled) return;
        if (string.IsNullOrWhiteSpace(SettingsManager.Default.ExternalIntegrationEndpointUrl)) return;

        try
        {
            var content = new StringContent(
                payload.toJson(),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                SettingsManager.Default.ExternalIntegrationEndpointUrl,
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"POST failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POST error: {ex.Message}");
        }
    }
}
