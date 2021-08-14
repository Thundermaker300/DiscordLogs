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

namespace DiscordLogs
{
    public class EventHandlers
    {
        MainPlugin plugin;
        ConcurrentQueue<WebhookLog> logs = new ConcurrentQueue<WebhookLog>();

        public EventHandlers(MainPlugin plug)
        {
            plugin = plug;
            Task.Factory.StartNew(MainLoop);
        }

        private string UserDisplay(Player ply) => $"{ply.PlayerName} ({ply.SteamID})";


        private void AddLog(string log, bool isImportant = false, WebhookType web = WebhookType.Main)
        {
            string final = $"[{DateTime.UtcNow.ToString("HH:mm:ss")}] {log}";

            if (isImportant)
                final = $"**{final}**";

            WebhookLog obj = new WebhookLog { content = final, webhook = web };
            logs.Enqueue(obj);
        }

        private async void MainLoop()
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
                        using (HttpClient clientMain = new HttpClient())
                        {
                            WebhookBody bodyMain = new WebhookBody { content = bldrMain.ToString() };
                            StringContent content = new StringContent(JsonConvert.SerializeObject(bodyMain), Encoding.UTF8, "application/json");
                            HttpResponseMessage resp = await clientMain.PostAsync(plugin.Config.WebhookUrl, content);
                            if (!resp.IsSuccessStatusCode)
                            {
                                Log.Error($"Failed to send to main webhook: {resp.ReasonPhrase}");
                            }
                        }
                    }
                    if (bldrChat.ToString().Length > 0)
                    {
                        using (HttpClient clientChat = new HttpClient())
                        {
                            WebhookBody bodyChat = new WebhookBody { content = bldrChat.ToString() };
                            StringContent content = new StringContent(JsonConvert.SerializeObject(bodyChat), Encoding.UTF8, "application/json");
                            HttpResponseMessage resp = await clientChat.PostAsync(string.IsNullOrEmpty(plugin.Config.ChatWebhookUrl) ? plugin.Config.WebhookUrl : plugin.Config.ChatWebhookUrl, content);
                            if (!resp.IsSuccessStatusCode)
                            {
                                Log.Error($"Failed to send to chat webhook: {resp.ReasonPhrase}");
                            }
                        }
                    }
                    if (bldrAdmin.ToString().Length > 0)
                    {
                        using (HttpClient clientAdmin = new HttpClient())
                        {
                            WebhookBody bodyAdmin = new WebhookBody { content = bldrAdmin.ToString() };
                            StringContent content = new StringContent(JsonConvert.SerializeObject(bodyAdmin), Encoding.UTF8, "application/json");
                            HttpResponseMessage resp = await clientAdmin.PostAsync(string.IsNullOrEmpty(plugin.Config.AdminWebhookUrl) ? plugin.Config.WebhookUrl : plugin.Config.AdminWebhookUrl, content);
                            if (!resp.IsSuccessStatusCode)
                            {
                                Log.Error($"Failed to send to admin webhook: {resp.ReasonPhrase}");
                            }
                        }
                    }
                }
                await Task.Delay(5000);
            }
        }

        // <---- EVENTS ---->
        // Server
        public void OnReady()
        {
            if (!plugin.Config.OnReady) return;
            AddLog($"Server is ready and waiting for players!");
        }

        public void OnActivateFemur(ActivateFemurBreakerEvent ev)
        {
            if (!plugin.Config.OnFemurBreaker) return;
            if (!ev.Finalized) return;
            AddLog($"{UserDisplay(ev.Player)} has activated the femur breaker!");
        }

        public void OnCleanRoomTrigger(CleanRoomTriggerEvent ev)
        {
            if (!plugin.Config.OnCleanRoomTrigger) return;
            if (!ev.Finalized) return;
            Player ply = Player.GetPlayer(ev.Target);
            if (ply != null)
            {
                AddLog($"{UserDisplay(ply)} has triggered clean room {ev.CleanRoom.curCleanRoomCount}.");
            }
        }

        public void OnLockdownToggle(LockdownToggleEvent ev)
        {
            if (!plugin.Config.OnLockdownToggle) return;
            if (!ev.Finalized) return;
            if (ev.LockdownType == PluginAPI.Enums.LockdownType.LCZLockdown)
            {
                AddLog($"{UserDisplay(ev.Player)} has {(ev.Locked ? "enabled" : "disabled")} LCZ lockdown.");
            }
            else if (ev.LockdownType == PluginAPI.Enums.LockdownType.SCP008)
            {
                AddLog($"{UserDisplay(ev.Player)} has {(ev.Locked ? "opened" : "closed")} SCP-008.");
            }
        }

        public void OnTeslaTriger(TeslaTriggerEvent ev)
        {
            if (!plugin.Config.OnTeslaTrigger) return;
            if (!ev.Finalized) return;
            Player ply = Player.GetPlayer(ev.Target);
            AddLog($"{UserDisplay(ply)} has triggered a tesla gate.");
        }

        // Player
        public void OnJoin(PlayerJoinEvent ev)
        {
            if (!plugin.Config.OnJoin) return;
            AddLog($"{UserDisplay(ev.Player)} has joined the server.", true);
        }

        public void OnLeave(PlayerLeaveEvent ev)
        {
            if (!plugin.Config.OnLeave) return;
            AddLog($"{UserDisplay(ev.Player)} has left the server.", true);
        }

        public void OnDamage(PlayerDamageEvent ev)
        {
            if (!plugin.Config.OnDamage) return;
            if (!ev.Finalized) return;
            if (ev.AttackerId == "Player")
                AddLog($"{UserDisplay(ev.Player)} has taken {ev.Damage.ToString("F")} damage from user {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"{UserDisplay(ev.Player)} has taken {ev.Damage.ToString("F")} damage with AttackerId: {ev.AttackerId}.");
        }

        public void OnDeath(PlayerDeathEvent ev)
        {
            if (!plugin.Config.OnDeath) return;
            if (!ev.Finalized) return;
            if (ev.AttackerId == "Player")
                AddLog($"{UserDisplay(ev.Player)} was killed by {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"{UserDisplay(ev.Player)} was killed with AttackerId: {ev.AttackerId}.");
        }

        public void OnChat(PlayerChatEvent ev)
        {
            if (!ev.Finalized) return;
            if ((!plugin.Config.OnChat && ev.IsAdminChat == false) || (!plugin.Config.OnAdminChat && ev.IsAdminChat == true)) return;
            AddLog($"{(ev.IsAdminChat ? "{ADMIN} " : string.Empty)}[{UserDisplay(ev.Player)}] `{ev.Message}`", false, WebhookType.Chat);
        }

        public void OnEffect(PlayerEffectEvent ev)
        {
            if (!plugin.Config.OnEffect) return;
            if (!ev.Finalized) return;
            AddLog($"{UserDisplay(ev.Player)} has the {ev.Effect.id}");
        }

        public void OnAdminCommand(PlayerExecuteCommandEvent ev)
        {
            if (!plugin.Config.EnableAdminLogs) return;
            if (!ev.Finalized) return;
            AddLog($"{UserDisplay(ev.Player)} executed {ev.CommandType} `{ev.RawInput}`, success: `{ev.IsSuccessful}`, response: {ev.ResponseMessage}.");
        }
    }
}
