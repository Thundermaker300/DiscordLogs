using PluginAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLogs
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public string WebhookUrl { get; set; } = string.Empty;
        public string ChatWebhookUrl { get; set; } = string.Empty;
        public bool OnJoin { get; set; } = true;
        public bool OnLeave { get; set; } = true;
        public bool OnReady { get; set; } = true;
        public bool OnDamage { get; set; } = true;
        public bool OnDeath { get; set; } = true;
        public bool OnChat { get; set; } = true;
        public bool OnAdminChat { get; set; } = true;
        public bool OnFemurBreaker { get; set; } = true;
        public bool OnCleanRoomTrigger { get; set; } = true;
        public bool OnLockdownToggle { get; set; } = true;
    }
}
