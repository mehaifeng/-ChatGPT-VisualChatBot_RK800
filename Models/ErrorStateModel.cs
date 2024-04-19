using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualChatBot.Models
{
    public class ErrorStateModel
    {
        [JsonProperty("error")]
        public bool error { get; set; }
        [JsonProperty("raw")]
        public string raw { get; set; }
    }
}
