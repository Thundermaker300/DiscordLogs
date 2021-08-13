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
        public bool OnJoin = true;
        public bool OnLeave = true;
        public bool OnReady = true;
        public bool OnDamage = true;
        public bool OnDeath = true;
        public bool OnChat = true;
        public bool OnAdminChat = true;
    }
}
