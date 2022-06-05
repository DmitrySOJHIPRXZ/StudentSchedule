using Android.OS;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StudentSchedule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        [Obsolete]
        Handler h = new Handler();
        INotificationManager notificationManager;

        string url;
        bool chek_one_start = false;
        public Login()
        {
            InitializeComponent();
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };
            //notificationManager.SendNotification("Title", "Message", DateTime.Now.AddSeconds(10));
        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
            });
        }

        public void Html_ScheduleAsync()
        {
            ToCommonMainPage();
        }

        public async void ToCommonMainPage()
        {
            await Navigation.PushAsync(new MainPage());
        }

        private void btn_login_Clicked(object sender, EventArgs e)
        {
            string login = "";
            string pass = "";
            login = loginEntry.Text;
            pass = passwordEntry.Text;
            if(login == "Test")
            {
                if(pass == "test")
                {
                    App.Current.Properties.Add("login_user", login);
                    App.Current.Properties.Add("pass_user", pass);
                    App.Current.Properties.Add("switch_toogle", "true");
                    Html_ScheduleAsync();
                }
            }
            else
            {
                if (login != null)
                {
                    if (login != "")
                    {
                        if (pass != null)
                        {
                            if (pass != "")
                            {
                                webView.EvaluateJavaScriptAsync("document.getElementById('last_name').value = '" + login + "';");
                                webView.EvaluateJavaScriptAsync("document.getElementById('password').value = '" + pass + "';");
                                webView.EvaluateJavaScriptAsync("$('#login-form').submit();");
                            }
                            else
                            {
                                DisplayAlert("Уведомление", "Введите пароль", "ОK");
                            }
                        }
                        else
                        {
                            DisplayAlert("Уведомление", "Введите пароль", "ОK");
                        }
                    }
                    else
                    {
                        DisplayAlert("Уведомление", "Введите логин", "ОK");
                    }
                }
                else
                {
                    DisplayAlert("Уведомление", "Введите логин", "ОK");
                }
                chek_one_start = true;
            }
        }

        public async void ChekLogin()
        {
            string login = "";
            string pass = "";
            login = loginEntry.Text;
            pass = passwordEntry.Text;
            string toast_container_str = "";
            var page = await webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");

            // Форматирование HTML разметки с WebView
            page = Regex.Replace(page, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            page = Regex.Unescape(page);
            var html = new HtmlDocument();
            html.LoadHtml(page);
            var toast = html.DocumentNode.SelectSingleNode("//div[@id='toast-container']/div[1]");//Получение уведомления 
            if(toast != null)
            {
                toast_container_str = toast.InnerText;
                if (toast_container_str != "")
                {
                    await DisplayAlert("Уведомление", toast_container_str, "ОK");
                }
                else
                {
                    if (Switch.IsToggled)
                    {
                        Save_user_data(login, pass);
                    }
                    Html_ScheduleAsync();
                }
            }
            else
            {
                if(Switch.IsToggled)
                {
                    Save_user_data(login, pass);
                }
                Html_ScheduleAsync();
            }
        }

        public async void CheckSession()
        {
            var page = await webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");
            // Форматирование HTML разметки с WebView
            page = Regex.Replace(page, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            page = Regex.Unescape(page);
            var html = new HtmlDocument();
            html.LoadHtml(page);
            var week = html.DocumentNode.SelectSingleNode("//div[@class='hide-on-med-and-up']/div[2]");//Получение типа недели
            if(week != null)
            {
                Html_ScheduleAsync();
            }
            else
            {
                if (Switch.IsToggled)
                {
                    AutoLog();
                }
            }
        }

        private void webView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            url = e.Url;
            if (chek_one_start)
            {
                if (url == "https://tou.edu.kz/armp/index.php")
                {
                    ChekLogin();
                }
            }
            else
            {
                if (url == "https://tou.edu.kz/armp/index.php")
                {
                    CheckSession();
                }
            }
        }

        private void AutoLog()
        {
            object status = "";
            if (App.Current.Properties.TryGetValue("login_user", out status))
            {
                string login = App.Current.Properties["login_user"].ToString();
                string pass = App.Current.Properties["pass_user"].ToString();
                if (login != null)
                {
                    if (login != "")
                    {
                        if (pass != null)
                        {
                            if (pass != "")
                            {
                                loginEntry.Text = login;
                                passwordEntry.Text = pass;
                                webView.EvaluateJavaScriptAsync("document.getElementById('last_name').value = '" + login + "';");
                                webView.EvaluateJavaScriptAsync("document.getElementById('password').value = '" + pass + "';");
                                webView.EvaluateJavaScriptAsync("$('#login-form').submit();");
                            }
                            else
                            {
                                DisplayAlert("Уведомление", "Введите пароль", "ОK");
                            }
                        }
                        else
                        {
                            DisplayAlert("Уведомление", "Введите пароль", "ОK");
                        }
                    }
                    else
                    {
                        DisplayAlert("Уведомление", "Введите логин", "ОK");
                    }
                }
                else
                {
                    DisplayAlert("Уведомление", "Введите логин", "ОK");
                }
                chek_one_start = true;
            }
            
        }


        public void Save_user_data(string login,string pass)
        {
            object status = "";
            if (App.Current.Properties.TryGetValue("login_user", out status))
            { }
            else
            {
                App.Current.Properties.Add("login_user", login);
                App.Current.Properties.Add("pass_user", pass);
                App.Current.Properties.Add("login", "in");
                App.Current.Properties.Add("switch_toogle", "true");
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)//исправить ошибку, запоминанеи только после успешной авторизации 
        {
        }

        protected override void OnAppearing()
        {
            object status = "";
            if (App.Current.Properties.TryGetValue("login_user", out status))
            {
                Console.WriteLine(status);
            }
            else
            {
                passwordEntry.Text = "";
                loginEntry.Text = "";
                Switch.IsToggled = false;
                webView.Source = "https://tou.edu.kz/armp/index.php?lang=rus&mod=logout";
            }

            if (App.Current.Properties.TryGetValue("switch_toogle", out status))
            {
                Switch.IsToggled = true;
            }
            else
            {
                Switch.IsToggled = false;
            }
        }
    }
}