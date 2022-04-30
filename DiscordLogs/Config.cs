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
        public bool ShowSensitive { get; set; } = false;
        public string WebhookUrl { get; set; } = string.Empty;
        public string ChatWebhookUrl { get; set; } = string.Empty;
        public string AdminWebhookUrl { get; set; } = string.Empty;
        public bool BlockDamageSpam { get; set; } = false;
        public bool EnableAdminLogs { get; set; } = true;
        public bool EnableChatLogs { get; set; } = true;
        public bool LogAdminChat { get; set; } = true;
        public bool OnJoin { get; set; } = true;
        public bool OnLeave { get; set; } = true;
        public bool OnReady { get; set; } = true;
        public bool OnDamage { get; set; } = true;
        public bool OnDeath { get; set; } = true;
        public bool OnFemurBreaker { get; set; } = true;
        public bool OnCleanRoomTrigger { get; set; } = true;
        public bool OnLockdownToggle { get; set; } = true;
        public bool OnEffect { get; set; } = true;
        public bool OnTeslaTrigger { get; set; } = true;
        public bool OnClassChange { get; set; } = true;
        public bool OnCleanRoomToggle { get; set; } = true;
        public bool OnPocketDimension { get; set; } = true;
        public bool OnPlayerInspectBody { get; set; } = true;
        public bool OnDoorInteract { get; set; } = false;
        public bool On049AddTarget { get; set; } = false;
        public bool On049Cure { get; set; } = false;
        public bool On294Input { get; set; } = true;
        public bool On330PickupCandy { get; set; } = true;
        public bool On914Activate { get; set; } = true;
    }
}
