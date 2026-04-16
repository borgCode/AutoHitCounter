using AutoHitCounter.Models;
using System.Threading.Tasks;

namespace AutoHitCounter.Interfaces;

public interface IExternalIntegrationService
{
    Task SendHitAsync(HitPayload payload);
}
