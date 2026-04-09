using AutoHitCounter.Models;
using System.Threading.Tasks;

namespace AutoHitCounter.Interfaces;

public interface IExternalHitService
{
    Task SendHitAsync(HitPayload payload);
}
