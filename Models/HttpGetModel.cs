using Newtonsoft.Json;
using System.Collections.Generic;

namespace VisualChatBot.Models
{
    class HttpGetModel
    {
        /// <summary>
        /// Api_Key是否有效
        /// </summary>
        [JsonIgnore]
        public static bool IsValidApiKey { get; set; } = true;
        /// <summary>
        /// 请求是否成功
        /// </summary>
        [JsonIgnore]
        public static bool IsRequestSuccess { get; set; }

        [JsonProperty("choices")]
        public List<Choice> choices { get; set; }
        [JsonProperty("created")]
        public int created { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("model")]
        public string model { get; set; }
        [JsonProperty("object")]
        public string @object { get; set; }
        [JsonProperty("usage")]
        public Usage usage { get; set; }
        [JsonProperty("system_fingerprint")]
        public string system_fingerprint { get; set; }

        public class Choice
        {
            [JsonProperty("finish_reason")]
            public string finish_reason { get; set; }
            [JsonProperty("index")]
            public int index { get; set; }
            [JsonProperty("message")]
            public Message message { get; set; }
            [JsonProperty("logprobs")]
            public object logprobs { get; set; }
        }
        public class Usage
        {
            [JsonProperty("completion_tokens")]
            public int completion_tokens { get; set; }
            [JsonProperty("prompt_tokens")]
            public int prompt_tokens { get; set; }
            [JsonProperty("total_tokens")]
            public int total_tokens { get; set; }
        }
    }
}
