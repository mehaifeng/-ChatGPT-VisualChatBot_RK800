using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using VisualChatBot.Models;
using VisualChatBot.Tools;

namespace VisualChatBot.ViewModels
{
    public partial class VisualChatViewModel : ObservableObject, INotifyPropertyChanged
    {
        /// <summary>
        /// 配置文件地址
        /// </summary>
        string configPath = $"{System.Environment.CurrentDirectory}//UserConfig.json";
        /// <summary>
        /// 历史文件地址
        /// </summary>
        string historyChatPath = $"{System.Environment.CurrentDirectory}//HistoryChat.json";
        /// <summary>
        /// API地址
        /// </summary>
        string urlAddress = $"https://api.nextapi.fun/v1/chat/completions";
        /// <summary>
        /// 是否处于发送状态
        /// </summary>
        bool isSending = false;
        /// <summary>
        /// 是否是历史对话导出的
        /// </summary>
        bool isHistoryChat = false;
        /// <summary>
        /// 发送次数
        /// </summary>
        int sendTimes = 0;
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
                    AddSystemOrder();
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
            catch (Exception ex)
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
            if (isSending) return;
            if (!string.IsNullOrEmpty(UserConfig.Apikey) && !string.IsNullOrEmpty(MyInput))
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
                    MaxWidth = o.ActualWidth,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 10, 5, 10),
                    IsReadOnly = true,
                    Text = MyInput.ToString(),
                };
                Grid sendGrid = new Grid();
                sendbox.MaxHeight = o.ActualWidth;
                sendGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                sendGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
                Grid.SetColumn(sendMessage, 0);
                sendGrid.Children.Add(sendMessage);
                Button deleteSend = new Button()
                {
                    Content = "×",
                    Foreground = Brushes.Black,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Style = (Style)Application.Current.FindResource("NoFocusButton"),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 10, 5, 10),
                    Width = 15,
                    Height = 15,
                };
                Grid.SetColumn(deleteSend, 1);
                //Grid里面有文本内容和删除按钮
                sendGrid.Children.Add(deleteSend);
                //次级Grid
                sendbox.Child = sendGrid;
                //最大一级StackPanel
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
                deleteSend.Click += (s, e) =>
                {
                    o.Children.Remove(sendbox);
                    messageList.Remove(messageList.Where(t=>t.content==(((((s as Button).Parent)as Grid).Children[0]) as TextBox).Text).First());
                };
                var input = new
                {
                    presence_penalty = UserConfig.Presence_penalty,
                    frequency_penalty = UserConfig.Frequency_penalty,
                    temperature = Convert.ToDouble(UserConfig.Temperature),
                    max_tokens =Convert.ToInt32(UserConfig.MaxTokens),
                    model = UserConfig.Model,
                    messages = messageList
                };
                MyInput = string.Empty;
                string inputJson = JsonConvert.SerializeObject(input);
                isSending = true;
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
                if (HttpGetModel.IsRequestSuccess == true)
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
                Grid respondGrid = new Grid();
                respondGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
                respondGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                Button deleteRespond = new Button()
                {
                    Content = "×",
                    Foreground = Brushes.Black,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Style = (Style)Application.Current.FindResource("NoFocusButton"),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 10, 0, 10),
                    Width = 15,
                    Height = 15,
                };
                Grid.SetColumn(deleteRespond, 0);
                Grid.SetColumn(respondbox, 1);
                respondGrid.Children.Add(respondbox);
                respondGrid.Children.Add(deleteRespond);
                border.Child = respondGrid;
                respondbox.SetBinding(TextBox.TextProperty, binding);
                o.Children.Add(border);
                //逐字显示
                if(HttpGetModel.IsRequestSuccess == true)
                {
                    ShowGenerateText(receivedViewModel);
                }
                else
                {
                    receivedViewModel.Content = respondTemp;
                    if (respondTemp.Contains("#未经处理的异常"))
                    {
                        receivedViewModel.Content = "服务器连接失败，请检查网络连接或稍后再试";
                        HttpGetModel.IsRequestSuccess = false;
                    }
                }
                deleteRespond.Click += (s, e) =>
                {
                    o.Children.Remove(border);
                    if(HttpGetModel.IsRequestSuccess == true)
                    {
                        messageList.Remove(messageList.Where(t => t.content == (((((s as Button).Parent) as Grid).Children[0]) as TextBox).Text).First());
                    }
                };
                //总结主要话题
                SummarizeTitle(o);
                isSending = false;
                #endregion
            }
            if (string.IsNullOrEmpty(UserConfig.Apikey))
            {

            }
        }

        private void DeleteRespond_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeleteSend_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("explorer.exe", "https://chat.openai.com/");
        }

        /// <summary>
        /// 总结对话标题
        /// </summary>
        async Task SummarizeTitle(StackPanel o)
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
            if (o.Children.Count < 6 && o.Children.Count > 0 && isHistoryChat == false)
            {
                Title = $"#{messageList[messageList.Count - 2].content}【{System.DateTime.Now}】#";
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
                if (LoadSignText.Length == 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) { return; }
                        LoadSignText += "/";
                        await Task.Delay(80);
                    }
                }
                if (LoadSignText.Length == 20 && !cancellationToken.IsCancellationRequested)
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
            if(messageList.Count == 0)
            {
                AddSystemOrder();
            }
            string configStr = JsonConvert.SerializeObject(userConfig, Formatting.Indented);
            File.WriteAllText(configPath, configStr);
        }
        /// <summary>
        /// 存档并清除当前对话
        /// </summary>
        /// <param name="o"></param>
        [RelayCommand]
        void ClearAll(StackPanel o)
        {
            if (o.Children.Count == 0)
            {
                isHistoryChat = false;
                AddSystemOrder();
                return;
            }
            if (!historyMessages.Any(t => t.Title == Title))
            {
                string currentChatElement = o.ToXAMLString();
                historyMessages.Add(new HistoryMessage
                {
                    AllMessage = new List<Message>(messageList),
                    Title = Title,
                    ControlStruct = currentChatElement
                });
                string historyToStr = JsonConvert.SerializeObject(historyMessages, Formatting.Indented);
                File.WriteAllText(historyChatPath, historyToStr);
                IsHistoryPopupAvaliable = true;
                sendTimes = 0;
                o.Children.Clear();
                MenuHistorySources.Add(Title);
                messageList.Clear();
            }
            else if (historyMessages.Any(t => t.Title == Title) && o.Children.Count > 0)
            {
                var item = historyMessages.First(t => t.Title == Title);
                string currentChatElement = o.ToXAMLString();
                item.ControlStruct = currentChatElement;
                item.AllMessage = new List<Message>(messageList);
                string historyToStr = JsonConvert.SerializeObject(historyMessages, Formatting.Indented);
                File.WriteAllText(historyChatPath, historyToStr);
                o.Children.Clear();
                messageList.Clear();
            }
            AddSystemOrder();
            isHistoryChat = false;
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
            if (title != null && stackPanel != null)
            {
                var item = historyMessages.First(t => t.Title == title);
                var controlStructStr = item.ControlStruct;
                StackPanel replacePanel = (StackPanel)XamlReader.Parse(controlStructStr);
                stackPanel.Children.Clear();
                List<UIElement> replacePanelTemp = new List<UIElement>();
                foreach (UIElement childTemp in replacePanel.Children)
                {
                    replacePanelTemp.Add(childTemp);
                }
                foreach (UIElement child in replacePanelTemp)
                {
                    replacePanel.Children.Remove(child);
                    stackPanel.Children.Add(child);
                }
                isHistoryChat = true;
                messageList = item.AllMessage;
                Title = title;
                RecoveryThemeOnChatBox(stackPanel);
            }
        }
        /// <summary>
        /// 移除一项历史记录
        /// </summary>
        /// <param name="o"></param>
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
                    if (messageList.Count > 0 && historyMessages.Count > 0 && Title == o[0].ToString())
                    {
                        var stackpanel = (StackPanel)o[1];
                        stackpanel.Children.Clear();
                        isHistoryChat = false;
                    }
                    else if (historyMessages.Count == 0)
                    {
                        var stackpanel = (StackPanel)o[1];
                        stackpanel.Children.Clear();
                        IsHistoryPopupAvaliable = false;
                        isHistoryChat = false;
                    }
                }
            }
        }
        /// <summary>
        /// 添加系统命令
        /// </summary>
        void AddSystemOrder()
        {
            if (!string.IsNullOrEmpty(userConfig.SystemOrder))
            {
                messageList.Clear();
                string[] systemOrder = userConfig.SystemOrder.Split('；');
                foreach (var item in systemOrder)
                {
                    Message message = new Message()
                    {
                        role = "system",
                        content = item
                    };
                    messageList.Add(message);
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
                        (((border.Child) as Grid).Children[0] as TextBox).Foreground = Brushes.Black;
                    }
                }
            }
            else if (UserConfig.EnableDarkMode == true)
            {
                foreach (Border border in o.Children.OfType<Border>())
                {
                    if (border.Tag.ToString() == "respondbox")
                    {
                        border.Background = (Brush)converter.ConvertFromString("#2a52be");
                        (((border.Child) as Grid).Children[0] as TextBox).Foreground = Brushes.White;
                    }
                }
            }
        }
    }
}
