// 

using System.Linq;
using AutoHitCounter.Models;
using AutoHitCounter.ViewModels;

namespace AutoHitCounter.Mappers;

public static class OverlayMapper
{
    public static OverlayState MapFrom(MainViewModel vm)
    {
        return new OverlayState
        {
            IsRunComplete = vm.IsRunComplete,
            TotalHits = vm.TotalHits,
            TotalPb = vm.TotalPb,
            InGameTime = vm.InGameTime.ToString(@"hh\:mm\:ss"),
            Splits = vm.Splits.Select(s => new OverlaySplit
            {
                Name = s.Name,
                Hits = s.NumOfHits,
                Pb = s.PersonalBest,
                Diff = s.Diff,
                IsCurrent = s.IsCurrent,
                IsParent = s.IsParent
            }).ToList()
        };
    }
}