﻿using PluginAPI;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLogs
{
    public class MainPlugin : Plugin<Config>
    {
        public override string Name => "DiscordLogs";
        public override string Author => "Thunder";
        public override Version Version => new Version(0, 1, 0);
        public override PluginPriority Priority => PluginPriority.High;

        EventHandlers handler;

        public override void OnEnabled()
        {
            if (!Config.WebhookUrl.Contains("https://discord.com/api/webhooks/"))
            {
                Log.Warn("The Discord Webhook URL is invalid! Please provide a valid webhook URL and try again.");
                return;
            }

            // Register events
            handler = new EventHandlers(this);

            // ! Server
            ServerEvents.Ready += handler.OnReady;
            ServerEvents.ActivateFemurBreaker += handler.OnActivateFemur;
            ServerEvents.CleanRoomTrigger += handler.OnCleanRoomTrigger;
            ServerEvents.LockdownToggle += handler.OnLockdownToggle;
            ServerEvents.TeslaTrigger += handler.OnTeslaTriger;

            // ! Player
            PlayerEvents.PlayerDamage += handler.OnDamage;
            PlayerEvents.PlayerDeath += handler.OnDeath;
            PlayerEvents.PlayerJoin += handler.OnJoin;
            PlayerEvents.PlayerLeave += handler.OnLeave;
            PlayerEvents.PlayerChat += handler.OnChat;
            PlayerEvents.PlayerEffect += handler.OnEffect;
            PlayerEvents.PlayerExecuteCommand += handler.OnAdminCommand;
            PlayerEvents.PlayerClassChange += handler.OnClassChage;

            // Pretty message
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (handler != null)
            {
                // Unregister events
                // ! Server
                ServerEvents.Ready -= handler.OnReady;
                ServerEvents.ActivateFemurBreaker -= handler.OnActivateFemur;
                ServerEvents.CleanRoomTrigger -= handler.OnCleanRoomTrigger;
                ServerEvents.LockdownToggle -= handler.OnLockdownToggle;
                ServerEvents.TeslaTrigger += handler.OnTeslaTriger;

                // ! Player
                PlayerEvents.PlayerDamage -= handler.OnDamage;
                PlayerEvents.PlayerDeath -= handler.OnDeath;
                PlayerEvents.PlayerJoin -= handler.OnJoin;
                PlayerEvents.PlayerLeave -= handler.OnLeave;
                PlayerEvents.PlayerChat -= handler.OnChat;
                PlayerEvents.PlayerEffect -= handler.OnEffect;
                PlayerEvents.PlayerExecuteCommand -= handler.OnAdminCommand;
                PlayerEvents.PlayerClassChange -= handler.OnClassChage;

                handler = null;
            }

            base.OnDisabled();
        }
    }
}
