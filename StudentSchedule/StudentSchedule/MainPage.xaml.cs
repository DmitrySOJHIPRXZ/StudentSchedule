using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        public List<Lesson> Lessons { get; set; }
        string znam = "знам.";
        string chisl = "числ.";
        string DayOfWeek = "";
        string type_week = "";
        string type_week_s = "";
        public MainPage()
        {
            InitializeComponent();
        }

        public async void LoadSchedule()
        {
            Console.WriteLine("123");
            //Определения дня недели
            DateTime dateTime = DateTime.Now;
            DayOfWeek = dateTime.DayOfWeek.ToString();
            Console.WriteLine(DayOfWeek);
            //Получение HTML разметки с WebView
            var page = await webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML");

            // Форматирование HTML разметки с WebView
            page = Regex.Replace(page, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            page = Regex.Unescape(page);
            var html = new HtmlDocument();
            html.LoadHtml(page);
            var table = html.DocumentNode.SelectSingleNode("//table[1]");//Получение первой табиоцы
            var week = html.DocumentNode.SelectSingleNode("//p[1]");//Получение типа недели
            type_week = week.InnerText;


            //Определение типа недели и запись в переменную
            if (week.InnerText == "Сейчас Числитель")
            {
                type_week_s = "числ.";
            }
            else if (week.InnerText == "Сейчас Знаменатель")
            {
                type_week_s = "знам.";
            }
            Console.WriteLine(type_week);
            //Получение всех строк таблицы 
            var rows = table.Descendants("tr")
                .Select(tr => tr.Descendants("td").Select(td => td.InnerText).ToList());
            Console.WriteLine(rows);

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
                        default:
                            classes.Add(item);
                            break;
                    }
                }
            }

            //Вывод расписания конкретного дня
            ArrayList day = (ArrayList)days_of_weeks[2];
            int i = 0;
            Lessons = new List<Lesson>();
            string lesson = "";
            string data_time = "";
            string teacher = "";
            foreach (var item in day)
            {
                if (i == 0)
                {
                    lesson = item.ToString();
                    Console.WriteLine("Аудитория " + item.ToString());
                    i++;
                }
                else if (i == 1)
                {
                    data_time = item.ToString();
                    Console.WriteLine("Время занятий " + item.ToString());
                    i++;
                }
                else if (i == 2)
                {
                    string str_item = item.ToString();
                    int len = str_item.Length;
                    int index_item_znam = str_item.IndexOf(znam);
                    int index_item_chisl = str_item.IndexOf(chisl);
                    int length_chisl = index_item_znam - index_item_chisl;
                    int length_znam = len - index_item_znam;
                    if (index_item_chisl == -1 && index_item_znam == -1)
                    {
                        Console.WriteLine("Преподователь " + item.ToString());
                        teacher = item.ToString();
                        i = 0;
                    }
                    else
                    {
                        if (type_week_s == "числ.")
                        {
                            string sub_str = str_item.Substring(index_item_chisl, length_chisl);
                            Console.WriteLine("Преподователь " + sub_str.ToString());
                            teacher = sub_str.ToString();
                            i = 0;
                        }
                        else if (type_week_s == "знам.")
                        {
                            string sub_str = str_item.Substring(index_item_znam, length_znam);
                            Console.WriteLine("Преподователь " + sub_str.ToString());
                            teacher = sub_str.ToString();
                            i = 0;
                        }
                    }
                }
                if (lesson != "")
                {
                    if (teacher != "")
                    {
                        if (data_time != "")
                        {
                            Lessons.Add(new Lesson { lesson = lesson, data_time = data_time, teacher = teacher });
                            lesson = "";
                            data_time = "";
                            teacher = "";
                        }
                    }
                }

            }
            this.BindingContext = this;
        }

        private void webView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var url = e.Url;
            if (url == "https://dot.tou.edu.kz/students-schedule")
            {
                LoadSchedule();
            }
        }
    }
}
