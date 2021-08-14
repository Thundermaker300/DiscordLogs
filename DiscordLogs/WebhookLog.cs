using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLogs
{
    public enum WebhookType
    {
        Main,
        Chat,
        Admin,
    }

    public class WebhookLog
    {
        public WebhookType webhook;
        public string content;
    }
}
