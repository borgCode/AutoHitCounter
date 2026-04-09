//
using AutoHitCounter.Utilities;
using AutoHitCounter.ViewModels;
using System;
using System.Text.Json;

namespace AutoHitCounter.Models
{
    public class HitPayload
    {      

        public string UserId { get; set; }
        public string GameName { get; set; }
        public string GameProfile { get; set; }
        public string SplitName { get; set; }
        public int SplitHits { get; set; }
        public int TotalHits { get; set; }
        public DateTime Timestamp { get; set; }
        public int AttemptCount { get; set; }
        public int SplitPB { get; set; }
        public int TotalPB { get; set; }
        public double IgtMilliseconds { get; set; }
        public string IgtFormatted
        {
            get
            {
                var ts = TimeSpan.FromMilliseconds(IgtMilliseconds);
                return ts.ToString(@"hh\:mm\:ss\:fff");
            }
        }

        public HitPayload(Game game, Profile profile, SplitViewModel split, int _totalHits, int _totalPb, TimeSpan _inGameTime)
        {
            UserId = SettingsManager.Default.ExternalIntegrationUserIdentifier;
            GameName = game.GameName;
            GameProfile = profile?.Name ?? "";
            SplitName = split.Name;
            SplitHits = split.NumOfHits;
            SplitPB = split.PersonalBest;
            TotalHits = _totalHits;
            TotalPB = _totalPb;
            Timestamp = DateTime.UtcNow;
            AttemptCount = profile?.AttemptCount ?? 0;
            IgtMilliseconds = _inGameTime.TotalMilliseconds;
        }


        public string toJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}