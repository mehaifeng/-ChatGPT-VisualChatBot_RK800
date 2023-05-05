﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using VisualChatBot.Tools;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using VisualChatBot.Models;
using System.Reflection.Metadata;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Windows.Data;
using VisualChatBot.Converters;
using System.Windows.Media.Effects;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace VisualChatBot.ViewModels
{
    public partial class VisualChatViewModel : ObservableObject, INotifyPropertyChanged
    {
        //配置文件地址
        string configPath = $"{System.Environment.CurrentDirectory}//UserConfig.json";
        //历史文件地址
        string historyChatPath = $"{System.Environment.CurrentDirectory}//HistoryChat.json";
        //API地址
        string urlAddress = $"https://api.openai.com/v1/chat/completions";
        //发送次数
        int sendTimes = 0 ;
        private List<HistoryMessage> historyMessages = new List<HistoryMessage>();
        Tools.WebRequest request;
        public VisualChatViewModel()
        {
            UserConfig = new UserConfig();
            messageList = new List<Message>();
            request = new Tools.WebRequest();
            MenuHistorySources = new ObservableCollection<string>();
            InitLoad();
        }

        void InitLoad()
        {
            try
            {
                #region 配置信息
                if (File.Exists(configPath))
                {
                    string configJson = File.ReadAllText(configPath);
                    UserConfig = JsonConvert.DeserializeObject<UserConfig>(configJson);
                }
                #endregion
                #region 历史对话
                if (File.Exists(historyChatPath))
                {
                    string historyMessageStr = File.ReadAllText(historyChatPath);
                    historyMessages = JsonConvert.DeserializeObject<List<HistoryMessage>>(historyMessageStr);
                    MenuHistorySources = new ObservableCollection<string>(historyMessages.Select(m => m.Title));
                }
                if (MenuHistorySources.Count == 0)
                {
                    IsHistoryPopupAvaliable = false;
                }
                else
                {
                    IsHistoryPopupAvaliable = true;
                }
                #endregion
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [ObservableProperty]
        private string selectedItem;

        /// <summary>
        /// 消息对象集合
        /// </summary>
        public List<Message> messageList { get; set; }

        [ObservableProperty]
        private UserConfig userConfig;

        /// <summary>
        /// 输入消息
        /// </summary>
        [ObservableProperty]
        private string myInput;

        /// <summary>
        /// 收到的消息（暂存）
        /// </summary>
        public string? respondTemp;

        /// <summary>
        /// 接收到的消息
        /// </summary>
        [ObservableProperty]
        public string respondContent;

        /// <summary>
        /// 是否滚动到底部
        /// </summary>
        [ObservableProperty]
        private bool isScrollToButtom;

        /// <summary>
        /// 加载标识
        /// </summary>
        [ObservableProperty]
        private string loadSignText = "////////////////////";

        /// <summary>
        /// 菜单->历史记录数据集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> menuHistorySources;

        /// <summary>
        /// 菜单->历史记录数据
        /// </summary>
        [ObservableProperty]
        private string title;

        /// <summary>
        /// 历史记录是否可用
        /// </summary>
        [ObservableProperty]
        private bool isHistoryPopupAvaliable;

        /// <summary>
        /// 发送+以及接收到消息
        /// </summary>
        /// <param name="o"></param>
        [RelayCommand]
        async void Send(StackPanel o)
        {
            if (!string.IsNullOrEmpty(UserConfig.Apikey)&&!string.IsNullOrEmpty(MyInput))
            {
                #region 控件生成流
                BrushConverter converter = new BrushConverter();
                Border sendbox = new Border()
                {
                    Tag = "sendbox",
                    //阴影
                    Effect = new DropShadowEffect()
                    {
                        Color = Colors.Black,
                        Direction = 0,
                        ShadowDepth = 0,
                        Opacity = 0.5,
                        BlurRadius = 5,
                    },
                    Margin = new Thickness(5, 0, 5, 5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    CornerRadius = new CornerRadius(7),
                    Background = (Brush)converter.ConvertFromString("#95EC69")
                };
                TextBox sendMessage = new TextBox()
                {
                    Style = (Style)Application.Current.FindResource("NoBorderTextBox"),
                    BorderBrush = Brushes.Transparent,
                    Background = Brushes.Transparent,
                    MaxWidth = o.ActualWidth,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(5,10,5,10),
                    IsReadOnly = true,
                    Text = MyInput.ToString(),
                };
                sendbox.Child = sendMessage;
                o.Children.Add(sendbox);
                IsScrollToButtom = true;
                IsScrollToButtom = false;
                #endregion
                #region 发送处理流
                messageList.Add(
                    new Message
                    {
                        role = "user",
                        content = MyInput
                    }
                );
                var input = new
                {
                    model = UserConfig.Model,
                    messages = messageList
                };
                MyInput = string.Empty;
                string inputJson = JsonConvert.SerializeObject(input);
                StringContent requestContent = new StringContent(inputJson, Encoding.UTF8, "application/json");
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                var getApiRespond = GetAPIRespond(requestContent);
                await Task.WhenAny(getApiRespond, LoadingEffect(tokenSource));
                if (getApiRespond.IsCompleted)
                {
                    tokenSource.Cancel();
                    LoadSignText = "////////////////////";
                }
                respondTemp = getApiRespond.Result;
                //将生成的回复加入到下一次入参(只有Http请求成功的才加入)
                if(HttpGetModel.IsRequestSuccess == true)
                {
                    messageList.Add(new Message
                    {
                        role = "assistant",
                        content = respondTemp
                    });
                    sendTimes++;
                }
                MyInput = string.Empty;
                #endregion
                #region 接收处理流
                ReceivedViewModel receivedViewModel = new ReceivedViewModel();
                Border border = new Border()
                {
                    Tag = "respondbox",
                    //阴影
                    Effect = new DropShadowEffect()
                    {
                        Color = Colors.Black,
                        Direction = 0,
                        ShadowDepth = 0,
                        Opacity = 0.5,
                        BlurRadius = 5,
                    },
                    Margin = new Thickness(5, 0, 5, 5),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    CornerRadius = new CornerRadius(7),
                    Background = UserConfig.EnableDarkMode ? (Brush)converter.ConvertFromString("#2a52be") : Brushes.White
                };
                //设置绑定
                Binding binding = new Binding();
                //绑定到数据源
                binding = new Binding(nameof(receivedViewModel.Content)) { Source = receivedViewModel };
                //设置绑定触发机制
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                TextBox respondbox = new TextBox()
                {
                    Style = (Style)Application.Current.FindResource("NoBorderTextBox"),
                    BorderBrush = Brushes.Transparent,
                    Background = Brushes.Transparent,
                    MaxWidth = o.ActualWidth,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(5, 10, 5, 10),
                    IsReadOnly = true,
                    Foreground = UserConfig.EnableDarkMode ? Brushes.White : Brushes.Black
                };
                respondbox.SetBinding(TextBox.TextProperty, binding);
                border.Child = respondbox;
                o.Children.Add(border);
                //逐字显示
                ShowGenerateText(receivedViewModel);
                //总结主要话题
                SummarizeTitle();
                #endregion
            }
            if (string.IsNullOrEmpty(UserConfig.Apikey))
            {

            }
        }

        /// <summary>
        /// 总结对话标题
        /// </summary>
        async Task SummarizeTitle()
        {
            if (!historyMessages.Any(t => t.Title == Title))
            {
                if (sendTimes == 3)
                {
                    Message item = new Message();
                    item.content = "请总结以上对话并返回一个标题";
                    item.role = "system";
                    messageList.Add(item);
                    var input = new
                    {
                        model = UserConfig.Model,
                        messages = messageList
                    };
                    StringContent content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                    string titletemp = await request.WebRequestMethon(userConfig.Apikey, urlAddress, content);
                    if (HttpGetModel.IsRequestSuccess == true)
                    {
                        Title = $"#{titletemp}#";
                        messageList.Remove(item);
                    }
                }
            }
        }

        /// <summary>
        /// 加载效果
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task LoadingEffect(CancellationTokenSource cancellationToken)
        {
            LoadSignText = string.Empty;
            while (!cancellationToken.IsCancellationRequested)
            {
                if(LoadSignText.Length==0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) { return; }
                        LoadSignText += "/";
                        await Task.Delay(80);
                    }
                }
                if(LoadSignText.Length==20 && !cancellationToken.IsCancellationRequested)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) { return; }
                        LoadSignText = LoadSignText.Substring(0, LoadSignText.Length - 1);
                        await Task.Delay(80);
                    }
                }
            }
        }

        /// <summary>
        /// 接收API返回
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="url"></param>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        async Task<string> GetAPIRespond(StringContent requestContent)
        {
            return await request.WebRequestMethon(userConfig.Apikey, urlAddress, requestContent);
        }

        /// <summary>
        /// 逐字显示回复
        /// </summary>
        async void ShowGenerateText(ReceivedViewModel receivedViewModel)
        {
            if (respondTemp != null)
            {
                for (int i = 0; i < respondTemp.Length; i++)
                {
                    await Task.Delay(50);
                    receivedViewModel.Content += respondTemp[i];
                }
            }
        }

        [RelayCommand]
        void SaveConfig()
        {
            string configStr = JsonConvert.SerializeObject(userConfig, Formatting.Indented);
            File.WriteAllText(configPath, configStr);
        }

        [RelayCommand]
        void ClearAll(StackPanel o)
        {
            if(o.Children.Count == 0)
            {
                return;
            }
            if (!historyMessages.Any(t => t.Title == Title))
            {
                string currentChatElement = o.ToXAMLString();
                historyMessages.Add(new HistoryMessage
                {
                    AllMessage = messageList,
                    Title = !string.IsNullOrEmpty(Title) ? Title : $"{messageList.First().content}【{System.DateTime.Now}】",
                    ControlStruct = currentChatElement
                });
                string historyToStr = JsonConvert.SerializeObject(historyMessages, Formatting.Indented);
                File.WriteAllText(historyChatPath, historyToStr);
                IsHistoryPopupAvaliable = true;
                sendTimes = 0;
                o.Children.Clear();
                MenuHistorySources.Add(Title);
            }
            else if(historyMessages.Any(t => t.Title == Title)&&o.Children.Count>0)
            {
                var item = historyMessages.First(t => t.Title == Title);
                string currentChatElement = o.ToXAMLString();
                item.ControlStruct = currentChatElement;
                item.AllMessage = messageList;
                string historyToStr = JsonConvert.SerializeObject(historyMessages, Formatting.Indented);
                File.WriteAllText(historyChatPath, historyToStr);
                o.Children.Clear();
            }
        }
        /// <summary>
        /// 切换到历史对话
        /// </summary>
        /// <param name="o"></param>
        [RelayCommand]
        void ReviewHistoryChat(object[] o)
        {
            string title = o[0].ToString(); 
            StackPanel stackPanel = (StackPanel)o[1];
             if (title != null&&stackPanel !=null)
            {
                var item = historyMessages.First(t => t.Title == title);
                var controlStructStr = item.ControlStruct;
                StackPanel replacePanel = (StackPanel)XamlReader.Parse(controlStructStr);
                stackPanel.Children.Clear();    
                List<UIElement> replacePanelTemp = new List<UIElement>();
                foreach(UIElement childTemp in replacePanel.Children)
                {
                    replacePanelTemp.Add(childTemp);
                }
                foreach(UIElement child in replacePanelTemp)
                {
                    replacePanel.Children.Remove(child);
                    stackPanel.Children.Add(child);
                }
                messageList = item.AllMessage;
                Title = item.Title;
                RecoveryThemeOnChatBox(stackPanel);
            }
        }

        [RelayCommand]
        void DeleteHistoryChat(object[] o)
        {
            if (o[0] != null && o[1] != null)
            {
                if (o[0].ToString() != null)
                {
                    var item = historyMessages.First(t => t.Title == o[0].ToString());
                    historyMessages.Remove(item);
                    MenuHistorySources.Remove(o[0].ToString());
                    string historyToStr = JsonConvert.SerializeObject(historyMessages, Formatting.Indented);
                    File.WriteAllText(historyChatPath, historyToStr);
                    if (messageList.Count > 0 && historyMessages.Count > 0 && historyMessages.Last().Title == o[0].ToString())
                    {
                        var stackpanel = (StackPanel)o[1];
                        stackpanel.Children.Clear();
                    }
                    else if (historyMessages.Count == 0)
                    {
                        var stackpanel = (StackPanel)o[1];
                        stackpanel.Children.Clear();
                        IsHistoryPopupAvaliable = false;
                    }
                }
            }
        }
        /// <summary>
        /// 恢复主题色
        /// </summary>
        /// <param name="o"></param>
        void RecoveryThemeOnChatBox(StackPanel o)
        {
            BrushConverter converter = new BrushConverter();
            if (UserConfig.EnableDarkMode == false)
            {
                foreach (Border border in o.Children.OfType<Border>())
                {
                    if (border.Tag.ToString() == "respondbox")
                    {
                        border.Background = Brushes.White;
                        (border.Child as TextBox).Foreground = Brushes.Black;
                    }
                }
            }
            else if(UserConfig.EnableDarkMode == true)
            {
                foreach (Border border in o.Children.OfType<Border>())
                {
                    if (border.Tag.ToString() == "respondbox")
                    {
                        border.Background = (Brush)converter.ConvertFromString("#2a52be");
                        (border.Child as TextBox).Foreground = Brushes.White;
                    }
                }
            }
        }
    }
}
