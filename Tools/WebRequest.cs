using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VisualChatBot.Models;

namespace VisualChatBot.Tools
{
    public class WebRequest
    {
        public async Task<string?> WebRequestMethon(string? apikey, string? requestUrl, StringContent? input)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
                var response = await client.PostAsync(requestUrl, input);
                var responseContent = await response.Content.ReadAsStringAsync();
                var responType = JsonConvert.DeserializeObject<HttpGetModel>(responseContent);
                if (response.IsSuccessStatusCode == false)
                {
                    HttpGetModel.IsRequestSuccess = false;
                    string errorInfo = $"#发生错误,Log: IsSuccessStatusCode == false";
                    return errorInfo;
                }
                else
                {
                    if (responseContent.Contains("\"error\":true"))
                    {
                        ErrorStateModel errorState = JsonConvert.DeserializeObject<ErrorStateModel>(responseContent);
                        if (null != errorState)
                        {
                            return errorState.raw;
                        }
                    }
                    HttpGetModel.IsRequestSuccess = true;
                    HttpGetModel.IsValidApiKey = true;
                    // 返回接收到的内容
                    return await Task.FromResult(result: responType?.choices[0].message.content);
                }
            }
            catch (Exception ex)
            {
                HttpGetModel.IsRequestSuccess = false;
                return $"#未经处理的异常：\n{ex.ToString().Substring(0,280)}";
            }
        }
    }
}
