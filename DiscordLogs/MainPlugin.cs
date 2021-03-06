using MEC;
using PluginAPI;
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
        public override Version Version => new Version(0, 2, 3);
        public override PluginPriority Priority => PluginPriority.High;
        public override PluginType Type => PluginType.Moderation | PluginType.Utility;

        EventHandlers handler;
        internal CoroutineHandle loopHandle;

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
            ServerEvents.Booting += handler.OnBooting;
            ServerEvents.Ready += handler.OnReady;
            ServerEvents.ActivateFemurBreaker += handler.OnActivateFemur;
            ServerEvents.CleanRoomTrigger += handler.OnCleanRoomTrigger;
            ServerEvents.LockdownToggle += handler.OnLockdownToggle;
            ServerEvents.TeslaTrigger += handler.OnTeslaTriger;
            ServerEvents.CleanRoomToggle += handler.OnCleanRoomToggle;
            ServerEvents.InteractDoor += handler.OnDoorInteract;
            ServerEvents.CommandExecute += handler.OnAdminCommand;
            

            // ! Player
            PlayerEvents.PlayerDamage += handler.OnDamage;
            PlayerEvents.PlayerDeath += handler.OnDeath;
            PlayerEvents.PlayerJoin += handler.OnJoin;
            PlayerEvents.PlayerLeave += handler.OnLeave;
            PlayerEvents.PlayerChat += handler.OnChat;
            PlayerEvents.PlayerEffect += handler.OnEffect;
            PlayerEvents.PlayerClassChange += handler.OnClassChage;
            PlayerEvents.EnterPocketDimension += handler.OnEnterPocketDimension;
            PlayerEvents.ExitPocketDimension += handler.OnExitPocketDimension;
            PlayerEvents.ChangePocketDimensionStage += handler.OnChangePocketDimensionStage;
            PlayerEvents.PlayerInspectBody += handler.OnInspectBody;

            // ! SCP
            ScpEvents.Scp049AddTarget += handler.On049AddTarget;
            ScpEvents.Scp049Cure += handler.On049Cure;
            ScpEvents.Scp330Interact += handler.On330Interact;
            ScpEvents.Scp294Input += handler.On294Input;
            ScpEvents.Scp914Activate += handler.On914Activate;

            // Pretty message
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (loopHandle != null)
            {
                Timing.KillCoroutines(loopHandle);
            }
            if (handler != null)
            {
                // Unregister events
                // ! Server
                ServerEvents.Booting -= handler.OnBooting;
                ServerEvents.Ready -= handler.OnReady;
                ServerEvents.ActivateFemurBreaker -= handler.OnActivateFemur;
                ServerEvents.CleanRoomTrigger -= handler.OnCleanRoomTrigger;
                ServerEvents.LockdownToggle -= handler.OnLockdownToggle;
                ServerEvents.TeslaTrigger -= handler.OnTeslaTriger;
                ServerEvents.CleanRoomToggle -= handler.OnCleanRoomToggle;
                ServerEvents.InteractDoor -= handler.OnDoorInteract;
                ServerEvents.CommandExecute -= handler.OnAdminCommand;

                // ! Player
                PlayerEvents.PlayerDamage -= handler.OnDamage;
                PlayerEvents.PlayerDeath -= handler.OnDeath;
                PlayerEvents.PlayerJoin -= handler.OnJoin;
                PlayerEvents.PlayerLeave -= handler.OnLeave;
                PlayerEvents.PlayerChat -= handler.OnChat;
                PlayerEvents.PlayerEffect -= handler.OnEffect;
                PlayerEvents.PlayerClassChange -= handler.OnClassChage;
                PlayerEvents.EnterPocketDimension -= handler.OnEnterPocketDimension;
                PlayerEvents.ExitPocketDimension -= handler.OnExitPocketDimension;
                PlayerEvents.ChangePocketDimensionStage -= handler.OnChangePocketDimensionStage;
                PlayerEvents.PlayerInspectBody -= handler.OnInspectBody;

                // ! SCP
                ScpEvents.Scp049AddTarget -= handler.On049AddTarget;
                ScpEvents.Scp049Cure -= handler.On049Cure;
                ScpEvents.Scp294Input -= handler.On294Input;
                ScpEvents.Scp330Interact -= handler.On330Interact;
                ScpEvents.Scp914Activate -= handler.On914Activate;


                handler = null;
            }

            base.OnDisabled();
        }
    }
}
