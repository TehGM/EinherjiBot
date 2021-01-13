using System;
using System.Linq;

namespace TehGM.EinherjiBot.RandomStatus
{
    public class RandomStatusOptions
    {
        public Status[] Statuses { get; set; } = new Status[0];
        public TimeSpan ChangeRate { get; set; } = TimeSpan.FromMinutes(10);
        public bool Enable { get; set; } = true;

        public bool IsEnabled => this.Enable && this.Statuses?.Any() == true;
    }
}
