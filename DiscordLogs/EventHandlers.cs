using System;
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
        Queue<string> logs = new Queue<string>(0);

        public EventHandlers(MainPlugin plug)
        {
            plugin = plug;
            Task.Factory.StartNew(MainLoop);
        }

        private string UserDisplay(Player ply) => $"{ply.PlayerName} ({ply.SteamID})";


        private void AddLog(string log, bool isImportant = false)
        {
            string final = $"[{DateTime.UtcNow.ToString("HH:mm:ss")}] {log}";

            if (isImportant)
                final = $"**{final}**";

            logs.Enqueue(final);
        }

        private async void MainLoop()
        {
            while (true)
            {
                if (logs.Count > 0)
                {
                    StringBuilder bldr = new StringBuilder();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        bldr.AppendLine(logs.Dequeue());
                    }
                    WebhookBody body = new WebhookBody { content = bldr.ToString() };
                    HttpClient client = new HttpClient();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                    await client.PostAsync(plugin.Config.WebhookUrl, content);
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

        // Player
        public void OnJoin(PlayerJoinEvent ev)
        {
            if (!plugin.Config.OnJoin) return;
            AddLog($"{UserDisplay(ev.Player)} has joined the server.", true);
        }

        public void OnLeave(PlayerLeaveEvent ev)
        {
            if (!plugin.Config.OnJoin) return;
            AddLog($"{UserDisplay(ev.Player)} has left the server.", true);
        }

        public void OnDamage(PlayerDamageEvent ev)
        {
            if (!plugin.Config.OnDamage) return;
            if (!ev.Finalized) return;
            if (ev.AttackInfo.AttackerId == "Player")
                AddLog($"{UserDisplay(ev.Player)} has taken {(int)ev.Damage} damage from user {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"{UserDisplay(ev.Player)} has taken {(int)ev.Damage} damage with AttackerId: {ev.AttackInfo.AttackerId}.");
        }

        public void OnDeath(PlayerDeathEvent ev)
        {
            if (!plugin.Config.OnDeath) return;
            if (!ev.Finalized) return;
            if (ev.AttackInfo.AttackerId == "Player")
                AddLog($"{UserDisplay(ev.Player)} was killed by {UserDisplay(Player.GetPlayer(ev.Attacker))}", true);
            else
                AddLog($"{UserDisplay(ev.Player)} was killed with AttackerId: {ev.AttackInfo.AttackerId}.");
        }
    }
}
