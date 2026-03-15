//

using AutoHitCounter.Enums;

namespace AutoHitCounter.Models
{
    public class Game
    {
        public GameTitle Title { get; set; }
        public string GameName { get; set; }
        public string ProcessName { get; set; }
        public bool IsEventLogSupported { get; set; }
        public bool IsManual { get; set; }
    }
}