using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginAPI;
using PluginAPI.Events.EventArgs;
using SCP_ET.Commands;
using SCP_ET.World.Doors;
using UnityEngine.Networking;
using MEC;
using UnityEngine;

namespace DiscordLogs
{
    public class EventHandlers
    {
        MainPlugin plugin;
        ConcurrentQueue<WebhookLog> logs = new ConcurrentQueue<WebhookLog>();

        public EventHandlers(MainPlugin plug)
        {
            plugin = plug;
            plug.loopHandle = Timing.RunCoroutine(MainLoop(), "DISCORDLOGS_MAINLOOP");
        }

        private string UserDisplay(Player ply, bool showSensitive = false)
        {
            if (showSensitive && plugin.Config.ShowSensitive)
            {
                return $"{ply.PlayerName} ({ply.SteamId}) ||[${ply.PlayerMain.Ip}]||";
            }
            else
            {
                return $"{ply.PlayerName} ({ply.SteamId})";
            }
        };


        private void AddLog(string log, bool isImportant = false, WebhookType web = WebhookType.Main)
        {
            string final = $"[{DateTime.UtcNow.ToString("HH:mm:ss")}] {log}";

            if (isImportant)
                final = $"**{final}**";

            WebhookLog obj = new WebhookLog { content = final, webhook = web };
            logs.Enqueue(obj);
        }

        private IEnumerator<float> MainLoop()
        {
            while (true)
            {
                if (logs.Count > 0)
                {
                    StringBuilder bldrMain = new StringBuilder();
                    StringBuilder bldrChat = new StringBuilder();
                    StringBuilder bldrAdmin = new StringBuilder();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        if (logs.TryDequeue(out WebhookLog res))
                        {
                            switch (res.webhook)
                            {
                                case WebhookType.Main:
                                    if (bldrMain.Length + res.content.Length >= 2000)
                                        continue;
                                    bldrMain.AppendLine(res.content);
                                    break;
                                case WebhookType.Chat:
                                    if (bldrChat.Length + res.content.Length >= 2000)
                                        continue;
                                    bldrChat.AppendLine(res.content);
                                    break;
                                case WebhookType.Admin:
                                    if (bldrAdmin.Length + res.content.Length >= 2000)
                                        continue;
                                    bldrAdmin.AppendLine(res.content);
                                    break;
                            }
                        }
                    }
                    if (bldrMain.ToString().Length > 0)
                    {
                        WebhookBody bodyMain = new WebhookBody { username = "EventLog", content = bldrMain.ToString() };
                        UnityWebRequest resp = UnityWebRequest.Put(plugin.Config.WebhookUrl, JsonConvert.SerializeObject(bodyMain));
                        resp.method = "POST";
                        resp.SetRequestHeader("Content-Type", "application/json");
                        yield return Timing.WaitUntilDone(resp.SendWebRequest());
                        if (resp.result != UnityWebRequest.Result.Success)
                        {
                            Log.Error($"Failed to send to main webhook (ERROR {resp.responseCode}): {resp.downloadHandler.text}");
                        }
                    }
                    if (bldrChat.ToString().Length > 0)
                    {
                        WebhookBody bodyChat = new WebhookBody { username = "ChatLog", content = bldrChat.ToString() };
                        UnityWebRequest resp = UnityWebRequest.Put(string.IsNullOrEmpty(plugin.Config.ChatWebhookUrl) ? plugin.Config.WebhookUrl : plugin.Config.ChatWebhookUrl, JsonConvert.SerializeObject(bodyChat));
                        resp.method = "POST";
                        resp.SetRequestHeader("Content-Type", "application/json");
                        yield return Timing.WaitUntilDone(resp.SendWebRequest());
                        if (resp.result != UnityWebRequest.Result.Success)
                        {
                            Log.Error($"Failed to send to chat webhook (ERROR {resp.responseCode}): {resp.downloadHandler.text}");
                        }
                    }
                    if (bldrAdmin.ToString().Length > 0)
                    {
                        WebhookBody bodyAdmin = new WebhookBody { username = "AdminLog", content = bldrAdmin.ToString() };
                        UnityWebRequest resp = UnityWebRequest.Put(string.IsNullOrEmpty(plugin.Config.AdminWebhookUrl) ? plugin.Config.WebhookUrl : plugin.Config.AdminWebhookUrl, JsonConvert.SerializeObject(bodyAdmin));
                        resp.method = "POST";
                        resp.SetRequestHeader("Content-Type", "application/json");
                        yield return Timing.WaitUntilDone(resp.SendWebRequest());
                        if (resp.result != UnityWebRequest.Result.Success)
                        {
                            Log.Error($"Failed to send to admin webhook (ERROR {resp.responseCode}): {resp.downloadHandler.text}");
                        }
                    }
                }
                yield return Timing.WaitForSeconds(5f);
            }
        }

        // <---- EVENTS ---->
        // Server
        public void OnReady()
        {
            if (!plugin.Config.OnReady) return;
            AddLog($"✅ Server is ready and waiting for players! Version `{Application.version}`");
        }

        public void OnActivateFemur(ActivateFemurBreakerEvent ev)
        {
            if (!plugin.Config.OnFemurBreaker) return;
            if (!ev.Finalized) return;
            AddLog($"🦴 {UserDisplay(ev.Player)} has activated the femur breaker!");
        }

        public void OnCleanRoomTrigger(CleanRoomTriggerEvent ev)
        {
            if (!plugin.Config.OnCleanRoomTrigger) return;
            if (!ev.Finalized) return;
            Player ply = Player.GetPlayer(ev.Entity.GameObject);
            if (ply != null)
            {
                AddLog($"⛆ {UserDisplay(ply)} has triggered Clean Room #{ev.Controller.curCleanRoomId}.");
            }
        }

        public void OnLockdownToggle(LockdownToggleEvent ev)
        {
            if (!plugin.Config.OnLockdownToggle) return;
            if (!ev.Finalized) return;
            string emote = (ev.Locked ? "🔒" : "🔓");
            if (ev.LockdownType == PluginAPI.Enums.LockdownType.LCZLockdown)
            {
                AddLog($"{emote} {UserDisplay(ev.Player)} has {(ev.Locked ? "enabled" : "disabled")} LCZ lockdown.");
            }
            else if (ev.LockdownType == PluginAPI.Enums.LockdownType.SCP008)
            {
                AddLog($"{emote} {UserDisplay(ev.Player)} has {(ev.Locked ? "opened" : "closed")} SCP-008.");
            }
        }

        public void OnCleanRoomToggle(CleanRoomToggleEvent ev)
        {
            if (!plugin.Config.OnCleanRoomTrigger) return;
            if (!ev.Finalized) return;
            if (ev.IsDisabling == true)
                AddLog($"🚨 {UserDisplay(ev.Player)} has disabled Clean Room #{ev.Controller.curCleanRoomId}");
            else
                AddLog($"🚨 {UserDisplay(ev.Player)} has enabled Clean Room #{ev.Controller.curCleanRoomId}");
        }

        public void OnDoorInteract(InteractDoorEvent ev)
        {
            if (!plugin.Config.OnDoorInteract) return;
            if (!ev.Finalized) return;
            if (ev.IsNPCOrPlayer)
            {
                if (ev.InteractionType == DoorInteractType.Player && ev.Entity != null)
                {
                    var ply = Player.GetPlayer(ev.Entity.GameObject);
                    AddLog($"🚪 {UserDisplay(ply)} has {(ev.InteractionMethod == DoorInteractMethod.Close ? "closed" : "opened")} a door. Name: {ev.DoorName} | DoorType: {ev.DoorType}");
                }
            }
        }


        public void OnTeslaTriger(TeslaTriggerEvent ev)
        {
            if (!plugin.Config.OnTeslaTrigger) return;
            if (!ev.Finalized) return;
            Player ply = Player.GetPlayer(ev.Entity.GameObject);
            if (ply != null)
                AddLog($"⚡ {UserDisplay(ply)} has triggered a tesla gate.");
        }

        // Player
        public void OnJoin(PlayerJoinEvent ev)
        {
            if (!plugin.Config.OnJoin) return;
            AddLog($"➡️ {UserDisplay(ev.Player, true)} has joined the server.", true);
        }

        public void OnLeave(PlayerLeaveEvent ev)
        {
            if (!plugin.Config.OnLeave) return;
            AddLog($"⬅️ {UserDisplay(ev.Player, true)} has left the server.", true);
        }

        public void OnDamage(PlayerDamageEvent ev)
        {
            if (!plugin.Config.OnDamage) return;
            if (!ev.Finalized) return;
            if (plugin.Config.BlockDamageSpam && (ev.AttackerId == "GAS" || ev.AttackerId == "POCKET" || ev.AttackerId == "SCP106")) return;
            if (ev.AttackerId == "Player")
                AddLog($"🩸 {UserDisplay(ev.Player)} has taken {ev.Damage.ToString("F")} damage from user {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"🩸 {UserDisplay(ev.Player)} has taken {ev.Damage.ToString("F")} damage with AttackerId: {ev.AttackerId}.");
        }

        public void OnDeath(PlayerDeathEvent ev)
        {
            if (!plugin.Config.OnDeath) return;
            if (!ev.Finalized) return;
            if (ev.AttackerId == "Player")
                AddLog($"☠️ {UserDisplay(ev.Player)} was killed by {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"☠️ {UserDisplay(ev.Player)} was killed with AttackerId: {ev.AttackerId}.");
        }

        public void OnChat(PlayerChatEvent ev)
        {
            if (!ev.Finalized) return;
            if ((!plugin.Config.EnableChatLogs && ev.IsAdminChat == false) || (!plugin.Config.LogAdminChat && ev.IsAdminChat == true)) return;
            AddLog($"💬 {(ev.IsAdminChat ? "{ADMIN} " : string.Empty)}[{UserDisplay(ev.Player)}] `{ev.Message}`", false, WebhookType.Chat);
        }

        public void OnEffect(PlayerEffectEvent ev)
        {
            if (!plugin.Config.OnEffect) return;
            if (!ev.Finalized) return;
            AddLog($"{UserDisplay(ev.Player)} has the {ev.Effect.Id}");
        }

        public void OnAdminCommand(CommandExecuteEvent ev)
        {
            if (!plugin.Config.EnableAdminLogs) return;
            if (!ev.Finalized) return;
            if (ev.Invoker is PlayerInvoker plrInv)
                AddLog($"💬 {UserDisplay(plrInv.Player.GetPlayer())} executed {ev.CommandType} `{ev.RawInput}`, success: `{ev.IsSuccessful}`, response: {ev.ResponseMessage}.", false, WebhookType.Admin);
            else if (ev.Invoker is ServerInvoker servInv)
                AddLog($"💬 **SERVER** executed {ev.CommandType} `{ev.RawInput}`, success: `{ev.IsSuccessful}`, response: {ev.ResponseMessage}.", false, WebhookType.Admin);
        }

        public void OnClassChage(PlayerClassChangeEvent ev)
        {
            if (!plugin.Config.OnClassChange) return;
            if (!ev.Finalized) return;
            if (ev.NewClassId == 0)
                AddLog($"👻 {UserDisplay(ev.Player)} has been changed to Spectator");
            else
                AddLog($"🧍 {UserDisplay(ev.Player)} has been changed to Class-D");
        }

        public void OnEnterPocketDimension(EnterPocketDimensionEvent ev)
        {
            if (!plugin.Config.OnPocketDimension) return;
            if (!ev.Finalized) return;
            AddLog($"🕳 {UserDisplay(ev.Player)} has entered the Pocket Dimension.");
        }

        public void OnExitPocketDimension(ExitPocketDimensionEvent ev)
        {
            if (!plugin.Config.OnPocketDimension) return;
            if (!ev.Finalized) return;
            AddLog($"🚪 { UserDisplay(ev.Player)} has exited the Pocket Dimension.");
        }

        public void OnChangePocketDimensionStage(ChangePocketDimensionStageEvent ev)
        {
            if (!plugin.Config.OnPocketDimension) return;
            if (!ev.Finalized) return;
            AddLog($"🚪 { UserDisplay(ev.Player)} has entered stage {ev.Stage} of the Pocket Dimension.");
        }

        public void OnInspectBody(PlayerInspectBodyEvent ev)
        {
            if (!plugin.Config.OnPlayerInspectBody) return;
            if (!ev.Finalized) return;
            AddLog($"⚰️ { UserDisplay(ev.Player)} has inspected the body of {ev.DeadBody.owner} ({(ev.DeadBody.playerOwner != null ? "(Player)" : "(NPC)")}).");
        }

        // SCP
        public void On049Cure(Scp049CureEvent ev)
        {
            if (!plugin.Config.On049Cure) return;
            if (ev.Finalized) return;
            AddLog($"🧟 SCP-049 has created a new SCP-008");
        }

        public void On049AddTarget(Scp049AddTargetEvent ev)
        {
            if (!plugin.Config.On049AddTarget) return;
            if (!ev.Finalized) return;
            Player ply = Player.GetPlayer(ev.Target.GameObject);
            if (ply != null)
                AddLog($"🏃‍ {UserDisplay(ply)} is now a target of SCP-049.");
        }

        public void On294Input(Scp294InputEvent ev)
        {
            if (!plugin.Config.On294Input) return;
            if (!ev.Finalized) return;
            if (!ev.IsCustomDrink == true)
                AddLog($"🥤 {UserDisplay(ev.Player)} has requested a drink `{ev.Input}`. Is it valid {ev.IsValidDrink}");
            else
                AddLog($"🥤 {UserDisplay(ev.Player)} has requested a custom drink with the name of `{ev.Input}`");
        }

        public void On330Interact(Scp330InteractEvent ev)
        {
            if (!plugin.Config.On330PickupCandy) return;
            if (!ev.Finalized) return;
            AddLog($"🍬 {UserDisplay(ev.Player)} picked up a candy from SCP-330{(ev.IsRemovingHands ? " **and lost their hands!**" : ".")}");
        }

        public void On914Activate(Scp914ActivateEvent ev)
        {
            if (!plugin.Config.On914Activate) return;
            if (!ev.Finalized) return;
            AddLog($"⚙️ {UserDisplay(ev.Player)} has activated SCP-914 on setting {ev.SCP914.NetworkcurMode}");
        }
    }
}
