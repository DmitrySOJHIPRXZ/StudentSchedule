using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StudentSchedule
{
    public class Lesson
    {
        public string lesson { get; set; }
        public string data_time { get; set; }
        public string teacher { get; set; }
    }
    public partial class MainPage : ContentPage
    {
        ArrayList classes;
        ArrayList days_of_weeks = new ArrayList();

        public ObservableCollection<Lesson> Lessons { get; set; }
        string DayOfWeek = "";
        string type_week = "";
        string type_week_s = "";
        string file_name = "page_data";
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string URL = "https://tou.edu.kz/armp/?lang=rus&menu=calendar&mod=shedule_prep_kaf";
        public MainPage()
        {
            //https://tou.edu.kz/armp/?lang=rus&menu=calendar&mod=shedule_prep_kaf
            InitializeComponent();
            object status = "";
            if (App.Current.Properties.TryGetValue("login_user", out status))
            {
                if((string)status == "Test")
                {
                    URL = "https://bikli.000webhostapp.com/site.html";
                }
            }
            webView.Source = URL;
            NavigationPage.SetHasBackButton(this, true);
            Lessons = new ObservableCollection<Lesson> {};
            this.BindingContext = this;
        }

        protected override void OnAppearing()
        {
            NavigationPage.SetHasBackButton(this, false);
        }
        protected override bool OnBackButtonPressed()
        {
            App.Current.Properties.Remove("login_user");
            App.Current.Properties.Remove("pass_user");
            App.Current.Properties.Remove("switch_toogle");
            return true;
        }



        public async void LoadSchedule()
        {
            //Определения дня недели
            DateTime dateTime = DateTime.Now;
            DayOfWeek = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dateTime.DayOfWeek);
            //Получение HTML разметки с WebView
            var page = await webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");

            // Форматирование HTML разметки с WebView
            await webView.EvaluateJavaScriptAsync("document.getElementsByName( 'week_no' ).item( 0 ).selectedIndex = 24;");
            page = Regex.Replace(page, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            page = Regex.Unescape(page);
            Path.Combine(folderPath, file_name);
            File.WriteAllText(Path.Combine(folderPath, file_name),page.ToString());
            var html = new HtmlDocument();
            html.LoadHtml(page);
            var table = html.DocumentNode.SelectSingleNode("//table[1]");//Получение первой табиоцы
            var week = html.DocumentNode.SelectSingleNode("//div[@class='hide-on-med-and-up']/div[2]");//Получение типа недели
            type_week = week.InnerText;
            string[] words = type_week.ToString().Replace(" ","").Split(',');
            string type_weeks = words[0].Substring(8);
            string week_s = words[1].Replace("\n","");
            //Определение типа недели и запись в переменную
            if (week_s == "Числитель")
            {
                type_week_s = "(числ.)";
            }
            else if (week_s == "Знаменатель")
            {
                type_week_s = "(знам.)";
            }
            Console.WriteLine(type_week_s);
            //Получение всех строк таблицы 
            var rows = table.Descendants("tr")
                .Select(tr => tr.Descendants("td").Select(td => td.InnerText).ToList());

            //Сортировка строк таблицы
            foreach (var row in rows)
            {
                foreach (var item in row)
                {
                    switch (item)
                    {
                        case "Понедельник":
                            classes = new ArrayList();
                            days_of_weeks.Insert(0, classes);
                            break;
                        case "Вторник":
                            classes = new ArrayList();
                            days_of_weeks.Insert(1, classes);
                            break;
                        case "Среда":
                            classes = new ArrayList();
                            days_of_weeks.Insert(2, classes);
                            break;
                        case "Четверг":
                            classes = new ArrayList();
                            days_of_weeks.Insert(3, classes);
                            break;
                        case "Пятница":
                            classes = new ArrayList();
                            days_of_weeks.Insert(4, classes);
                            break;
                        case "Суббота":
                            break;
                        case "Воскресенье":
                            break;
                        default:
                            if(item != "")
                            {
                                if(item.Length > 3)
                                {
                                    classes.Add(item);
                                    break;
                                }
                            }
                            break;
                    }
                }
            }


            int index_day = 0;
            NavTitle.Text = "Расписание на " + DayOfWeek + ", " + week_s;
            //Вывод расписания конкретного дня
            if(DayOfWeek == "понедельник")
            {
                index_day = 0;
            }
            if(DayOfWeek == "вторник")
            {
                index_day = 1;
            }
            if(DayOfWeek == "среда")
            {
                index_day = 2;
            }
            if (DayOfWeek == "четверг")
            {
                index_day = 3;
            }
            if (DayOfWeek == "пятница")
            {
                index_day = 4;
            }
            if (DayOfWeek == "суббота")
            {
                index_day = 0;
            }
            if (DayOfWeek == "воскресенье")
            {
                index_day = 0;
            }
            ArrayList day = (ArrayList)days_of_weeks[index_day];
            Lessons.Clear();
            int i = 0;
            //Lessons = new List<Lesson>();
            string data_time = "";
            string teacher = "";
            foreach (var item in day)
            {
                if (i == 0)
                {
                    data_time = item.ToString();
                    i++;
                }
                else if (i == 1)
                {
                    teacher = item.ToString().Replace("\n", "");
                    if (type_week_s == "(числ.)")
                    {
                        if (teacher.IndexOf(type_week_s) != -1)
                        {
                            teacher = teacher.Substring(0, teacher.IndexOf(type_week_s));
                        }
                    }
                    else if (type_week_s == "(знам.)")
                    {
                        if (teacher.IndexOf(type_week_s) != -1)
                        {
                            teacher = teacher.Substring(teacher.IndexOf("(числ.)"));
                            if (teacher.Length == 14)
                            {
                                teacher = "";
                            }
                        }
                    }
                    i = 0;
                }
                if (data_time != "")
                {
                    if (teacher != "")
                    {
                        Lessons.Add(new Lesson { lesson = "Занятие", data_time = data_time, teacher = teacher });
                        data_time = "";
                        teacher = "";
                    }
                }


            }
            if (Lessons.Count == 0)
            {
                Lessons.Add(new Lesson { lesson = "Занятие", data_time = "00:00", teacher = "Загятий нет" });
            }
            Preloder.IsVisible = false;
        }

        public void LoadScheduleFile()
        {
            DateTime dateTime = DateTime.Now;
            DayOfWeek = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(dateTime.DayOfWeek);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(File.ReadAllText(Path.Combine(folderPath, file_name)));
            var table = htmlDoc.DocumentNode.SelectSingleNode("//table[1]");//Получение первой табиоцы
            var week = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='hide-on-med-and-up']/div[2]");//Получение типа недели

            type_week = week.InnerText;
            string[] words = type_week.ToString().Replace(" ", "").Split(',');
            string type_weeks = words[0].Substring(8);
            string week_s = words[1].Replace("\n", "");
            //Определение типа недели и запись в переменную
            if (week_s == "Числитель")
            {
                type_week_s = "(числ.)";
            }
            else if (week_s == "Знаменатель")
            {
                type_week_s = "(знам.)";
            }
            Console.WriteLine(type_week_s);
            //Получение всех строк таблицы 
            var rows = table.Descendants("tr")
                .Select(tr => tr.Descendants("td").Select(td => td.InnerText).ToList());

            //Сортировка строк таблицы
            foreach (var row in rows)
            {
                foreach (var item in row)
                {
                    switch (item)
                    {
                        case "Понедельник":
                            classes = new ArrayList();
                            days_of_weeks.Insert(0, classes);
                            break;
                        case "Вторник":
                            classes = new ArrayList();
                            days_of_weeks.Insert(1, classes);
                            break;
                        case "Среда":
                            classes = new ArrayList();
                            days_of_weeks.Insert(2, classes);
                            break;
                        case "Четверг":
                            classes = new ArrayList();
                            days_of_weeks.Insert(3, classes);
                            break;
                        case "Пятница":
                            classes = new ArrayList();
                            days_of_weeks.Insert(4, classes);
                            break;
                        case "Суббота":
                            break;
                        case "Воскресенье":
                            break;
                        default:
                            if (item != "")
                            {
                                if (item.Length > 3)
                                {
                                    classes.Add(item);
                                    break;
                                }
                            }
                            break;
                    }
                }
            }


            int index_day = 0;
            NavTitle.Text = "Расписание на " + DayOfWeek + ", " + week_s;
            //Вывод расписания конкретного дня
            if (DayOfWeek == "понедельник")
            {
                index_day = 0;
            }
            if (DayOfWeek == "вторник")
            {
                index_day = 1;
            }
            if (DayOfWeek == "среда")
            {
                index_day = 2;
            }
            if (DayOfWeek == "четверг")
            {
                index_day = 3;
            }
            if (DayOfWeek == "пятница")
            {
                index_day = 4;
            }
            if (DayOfWeek == "суббота")
            {
                index_day = 0;
            }
            if (DayOfWeek == "воскресенье")
            {
                index_day = 0;
            }
            ArrayList day = (ArrayList)days_of_weeks[index_day];
            Lessons.Clear();
            int i = 0;
            //Lessons = new List<Lesson>();
            string data_time = "";
            string teacher = "";
            foreach (var item in day)
            {
                if (i == 0)
                {
                    data_time = item.ToString();
                    i++;
                }
                else if (i == 1)
                {
                    teacher = item.ToString().Replace("\n", "");
                    if (type_week_s == "(числ.)")
                    {
                        if (teacher.IndexOf(type_week_s) != -1)
                        {
                            teacher = teacher.Substring(0, teacher.IndexOf(type_week_s));
                        }
                    }
                    else if (type_week_s == "(знам.)")
                    {
                        if (teacher.IndexOf(type_week_s) != -1)
                        {
                            teacher = teacher.Substring(teacher.IndexOf("(числ.)"));
                            if (teacher.Length == 14)
                            {
                                teacher = "";
                            }
                        }
                    }
                    i = 0;
                }
                if (data_time != "")
                {
                    if (teacher != "")
                    {
                        Lessons.Add(new Lesson { lesson = "Занятие", data_time = data_time, teacher = teacher });
                        data_time = "";
                        teacher = "";
                    }
                }


            }
            if(Lessons.Count == 0)
            {
                Lessons.Add(new Lesson { lesson = "Занятие", data_time = "00:00", teacher = "Загятий нет" });
            }
            Preloder.IsVisible = false;
        }

        private void webView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var url = e.Url;
            if (url == URL)
            {
                if (File.Exists(Path.Combine(folderPath, file_name)))
                {
                    scroll.IsVisible = false;
                    webView.IsVisible = false;
                    collectionView.IsVisible = true;
                    btn1.IsVisible = true;
                    LoadScheduleFile();
                }
                else
                {
                    scroll.IsVisible = false;
                    webView.IsVisible = false;
                    collectionView.IsVisible = true;
                    btn1.IsVisible = true;
                    LoadSchedule();
                }
            }
            if(url == "https://tou.edu.kz/armp/?mod=shedule_prep_kaf")
            {
                scroll.IsVisible = false;
                webView.IsVisible = false;
                collectionView.IsVisible = true;
                btn1.IsVisible = true;
                LoadSchedule();
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            App.Current.Properties.Remove("login_user");
            App.Current.Properties.Remove("pass_user");
            App.Current.Properties.Remove("switch_toogle");
            await Navigation.PopAsync();
        }


        private void Button_Clicked(object sender, EventArgs e)
        {
            btn1.IsVisible = false;
            webView.IsVisible = true;
            collectionView.IsVisible = false;
            scroll.IsVisible = true;
            Preloder.IsVisible = false;
        }
    }
}
