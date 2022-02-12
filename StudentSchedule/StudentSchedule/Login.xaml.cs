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
            webView.EvaluateJavaScriptAsync("document.getElementById('username').value = '"+login+"';");
            webView.EvaluateJavaScriptAsync("document.getElementById('password').value = '"+pass+"';");
            webView.EvaluateJavaScriptAsync("$('#form').submit();");
        }

        private void webView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            url = e.Url;
            Console.WriteLine("url " + url);
            if(url == "https://dot.tou.edu.kz/courses")
            {
                loginEntry.IsEnabled = false;
                passwordEntry.IsEnabled = false;
                webView.Source = "https://dot.tou.edu.kz/students-schedule";
            }
            if(url == "https://dot.tou.edu.kz/students-schedule")
            {
                Html_ScheduleAsync();
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