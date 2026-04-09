//
using System;

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
    }
}