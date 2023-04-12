﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualChatBot.Models;
using VisualChatBot.ViewModels;
using System.IO;
using VisualChatBot.Tools;
using Newtonsoft.Json;
using System.Windows.Media.Animation;

namespace VisualChatBot
{
    /// <summary>
    /// Interaction logic for VisualChat.xaml
    /// </summary>
    public partial class VisualChat : Window
    {
        VisualChatViewModel _visualChatViewModel;
        ReadWriteJson readReadJson = new();
        private string userConfigPath = $"{System.Environment.CurrentDirectory}//UserConfig.json";
        public VisualChat()
        {
            InitializeComponent();
            _visualChatViewModel = new VisualChatViewModel();
            this.DataContext = _visualChatViewModel;
            OptionalModelsComboBox.ItemsSource = new[]
            {
                "gpt-3.5-turbo",
                "gpt-3.5-turbo-0301",
            };
            ObjectDegreeCombobox.ItemsSource = new[]
            {
                0,
                0.1,
                0.2,
                0.3,
                0.4,
                0.5,
                0.6,
                0.7,
                0.8,
                0.9
            };
            ModeSwitch.Content = "\xe687";
        }

        private void MiniOrReSize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MoveWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ( _visualChatViewModel.ConfigGridHeight >= 60)
            {
                _visualChatViewModel.SettingBtnContent = "\xe799";
                Timer timer = new Timer(1);
                timer.Elapsed += (sender, e) =>
                {
                    _visualChatViewModel.ConfigGridHeight--;
                    if (_visualChatViewModel.ConfigGridHeight == 0)
                    {
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_visualChatViewModel.ConfigGridHeight >= 60)
            {
                _visualChatViewModel.SettingBtnContent = "\xe799";
                Timer timer = new Timer(1);
                timer.Elapsed += (sender, e) =>
                {
                    _visualChatViewModel.ConfigGridHeight--;
                    if (_visualChatViewModel.ConfigGridHeight == 0)
                    {
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }

        /// <summary>
        /// 模型改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionalModelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (File.Exists(userConfigPath))
            {
                string json = readReadJson.ReadJson(userConfigPath, null);
                readReadJson.WriteJson(json,"model", _visualChatViewModel.Modelstype);
            }
            else
            {
                Dictionary<string, string> UserConfig = new()
                {
                    { "model", _visualChatViewModel.Modelstype },
                    { "objectDegree",  _visualChatViewModel.ObjectDegree.ToString() },
                    { "maxTokens",  _visualChatViewModel.MaxToken.ToString() },
                    { "APIKey",  _visualChatViewModel.ApiKey }
                };
                var jsonStr = JsonConvert.SerializeObject(UserConfig);
                File.WriteAllText(userConfigPath, jsonStr);
            }
        }

        /// <summary>
        /// 主客观改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectDegreeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (File.Exists(userConfigPath))
            {
                string json = readReadJson.ReadJson(userConfigPath, null);
                readReadJson.WriteJson(json, "objectDegree", _visualChatViewModel.ObjectDegree.ToString());
            }
            else
            {
                Dictionary<string, string> UserConfig = new()
                {
                    { "model", _visualChatViewModel.Modelstype },
                    { "objectDegree",  _visualChatViewModel.ObjectDegree.ToString() },
                    { "maxTokens",  _visualChatViewModel.MaxToken.ToString() },
                    { "APIKey",  _visualChatViewModel.ApiKey }
                };
                var jsonStr = JsonConvert.SerializeObject(UserConfig);
                File.WriteAllText(userConfigPath, jsonStr);
            }
        }

        /// <summary>
        /// MaxTokens改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxTokensTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists(userConfigPath))
            {
                string json = readReadJson.ReadJson(userConfigPath, null);
                readReadJson.WriteJson(json, "maxTokens", _visualChatViewModel.MaxToken.ToString());
            }
            else
            {
                Dictionary<string, string> UserConfig = new()
                {
                    { "model", _visualChatViewModel.Modelstype },
                    { "objectDegree",  _visualChatViewModel.ObjectDegree.ToString() },
                    { "maxTokens",  _visualChatViewModel.MaxToken.ToString() },
                    { "APIKey",  _visualChatViewModel.ApiKey }
                };
                var jsonStr = JsonConvert.SerializeObject(UserConfig);
                File.WriteAllText(userConfigPath, jsonStr);
            }
        }
        /// <summary>
        /// 黑暗模式切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModeSwitch_Click(object sender, RoutedEventArgs e)
        {

            //如果是明亮模式
            if (ModeSwitch.Foreground == Brushes.White)
            {
                ModeSwitch.Foreground = Brushes.Black;
                //AppTitle
                AppTitle.Background = Application.Current.Resources["BackgroundColor"] as SolidColorBrush;
                miniBtn.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                closeBtn.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                MenuToggleBtn.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                OpenSettingBtn.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                sideMenu.Background = Application.Current.Resources["BackgroundColor"] as SolidColorBrush;
                MenuHistory.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                MenuOpenAI.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                MenuAbout.Foreground = Application.Current.Resources["ForegroundColor"] as SolidColorBrush;
                //AppBody
                AppBody.Background = Application.Current.Resources["BackgroundColor"] as SolidColorBrush;
                OutputBox.Background = Application.Current.Resources["TextBoxBackgroundColor"] as SolidColorBrush;
                OutputBox.Foreground = Application.Current.Resources["TextBoxForegroundColor"] as SolidColorBrush;
                loadingSignal.Foreground = Application.Current.Resources["LabelForegroundColor"] as SolidColorBrush;
                //AppButtom
                AppButtom.Background = Application.Current.Resources["BackgroundColor"] as SolidColorBrush;
                InputBox.Background = Application.Current.Resources["TextBoxBackgroundColor"] as SolidColorBrush;
                InputBox.Foreground = Application.Current.Resources["TextBoxForegroundColor"] as SolidColorBrush;
                SendBtn.Background = Application.Current.Resources["ButtonBackgroundColor"] as SolidColorBrush;

            }
            //如果是黑暗模式
            else if(ModeSwitch.Foreground == Brushes.Black)
            {
                ModeSwitch.Foreground = Brushes.White;
                //AppTitle
                AppTitle.Background = Application.Current.Resources["DarkBackgroundColor"] as SolidColorBrush;
                miniBtn.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                closeBtn.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                MenuToggleBtn.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                OpenSettingBtn.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                sideMenu.Background = Application.Current.Resources["DarkBackgroundColor"] as SolidColorBrush;
                MenuHistory.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                MenuOpenAI.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                MenuAbout.Foreground = Application.Current.Resources["DarkForegroundColor"] as SolidColorBrush;
                //AppBody
                AppBody.Background = Application.Current.Resources["DarkBackgroundColor"] as SolidColorBrush;
                OutputBox.Background = Application.Current.Resources["DarkTextBoxBackgroundColor"] as SolidColorBrush;
                OutputBox.Foreground = Application.Current.Resources["DarkTextBoxForegroundColor"] as SolidColorBrush;
                loadingSignal.Foreground = Application.Current.Resources["DarkLabelForegroundColor"] as SolidColorBrush;
                //AppButtom
                AppButtom.Background = Application.Current.Resources["DarkBackgroundColor"] as SolidColorBrush;
                InputBox.Background = Application.Current.Resources["DarkTextBoxBackgroundColor"] as SolidColorBrush;
                InputBox.Foreground = Application.Current.Resources["DarkTextBoxForegroundColor"] as SolidColorBrush;
                SendBtn.Background = Application.Current.Resources["DarkButtonBackgroundColor"] as SolidColorBrush;
            }
        }

        private void OpenSettingBtn_Checked(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(0,60,TimeSpan.FromSeconds(0.2));
            DoubleAnimation animation2 = new DoubleAnimation(60, 0, TimeSpan.FromSeconds(0.2));
            if (OpenSettingBtn.IsChecked == true)
            {
                ConfigBorder.BeginAnimation(HeightProperty, animation);
            }
            else if(OpenSettingBtn.IsChecked == false)
            {
                ConfigBorder.BeginAnimation(HeightProperty, animation2);
            }
            
        }
    }
}
