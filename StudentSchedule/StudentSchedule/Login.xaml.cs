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

        string url;
        bool chek_one_start = false;
        public Login()
        {
            InitializeComponent();
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
            string login = loginEntry.Text;
            string pass = passwordEntry.Text;
            webView.EvaluateJavaScriptAsync("document.getElementById('last_name').value = '" + login+"';");
            webView.EvaluateJavaScriptAsync("document.getElementById('password').value = '" + pass+"';");
            webView.EvaluateJavaScriptAsync("$('#login-form').submit();");
            App.Current.Properties.Add("login_user", login);
            App.Current.Properties.Add("pass_user", pass);
            chek_one_start=true;
        }

        public async void ChekLogin()
        {
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
            }
            else
            {
                Html_ScheduleAsync();
            }

            if (toast_container_str != "")
            {
                await DisplayAlert("Уведомление", toast_container_str, "ОK");
            }
            else
            {
                Html_ScheduleAsync();
            }
        }

        private void webView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            url = e.Url;
            Console.WriteLine(url);
            if (chek_one_start)
            {
                if (url == "https://tou.edu.kz/armp/index.php")
                {
                    ChekLogin();
                }
            }
        }

        protected override void OnAppearing()
        {
            object status = "";
            if (App.Current.Properties.TryGetValue("login", out status))
            {
                if ((string)status == "out")
                {
                    loginEntry.IsEnabled = true;
                    passwordEntry.IsEnabled = true;
                    webView.Source = "https://dot.tou.edu.kz/auth/logout";
                }
            }
        }
    }
}