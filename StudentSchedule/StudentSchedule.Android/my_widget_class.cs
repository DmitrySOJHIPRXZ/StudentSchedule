using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HtmlAgilityPack;
using StudentSchedule.Droid;

namespace WidgetButtonClick.Droid
{
    public class Lesson
    {
        public string lesson { get; set; }
        public string data_time { get; set; }
        public string teacher { get; set; }
    }

    [BroadcastReceiver(Label = "StudentSchedule")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/my_widget_provider")]
    public class my_widget_class : AppWidgetProvider
    {
        ArrayList classes;
        ArrayList days_of_weeks = new ArrayList();
        public List<Lesson> Lessons { get; set; }
        string znam = "знам.";
        string chisl = "числ.";
        string DayOfWeek = "";
        string type_week = "";
        string type_week_s = "";
        public static String ACTION_WIDGET_TURNON = "Button 1 click";
        public static String ACTION_WIDGET_TURNOFF = "Button 2 clickedddddd";
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            //Update Widget layout
            //Run when create widget or meet update time
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(my_widget_class)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {
            //Build widget layout
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.my_widget);

            //Change text of element on Widget
            SetTextViewText(widgetView);

            //Handle click event of button on Widget
            RegisterClicks(context, appWidgetIds, widgetView);

            return widgetView;
        }

        private void SetTextViewText(RemoteViews widgetView)
        {
            var html = @"https://bikli.000webhostapp.com/site.html";
            HtmlWeb web  = new HtmlWeb();
            var htmlDoc = web.Load(html);
            var table = htmlDoc.DocumentNode.SelectSingleNode("//table[1]");//Получение первой табиоцы
            var week = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='hide-on-med-and-up']/div[2]");//Получение типа недели
            type_week = week.InnerText;
            string[] words = type_week.ToString().Replace(" ", "").Split(',');


            string week_s = words[1].Replace("\r", "");
            week_s = week_s.Replace("\n", "");
            //Определение типа недели и запись в переменную
            if (week_s == "Числитель")
            {
                type_week_s = "(числ.)";
            }
            else if (week_s == "Знаменатель")
            {
                type_week_s = "(знам.)";
            }
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
            int i = 0;
            Lessons = new List<Lesson>();
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

            foreach(var time in Lessons)
            {
                DateTime dateTimeStart;
                if (DateTime.TryParse(time.data_time.Substring(0, 5), DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None, out dateTimeStart))
                {
                    DateTime dateTimeNow = DateTime.Now;
                    if (dateTimeNow < dateTimeStart)
                    {
                        TimeSpan remaining = dateTimeNow - dateTimeStart;
                        string remaining_str = remaining.ToString();
                        widgetView.SetTextViewText(Resource.Id.textView1, "Время до следущего урока : " + 
                            remaining_str.Substring(1,8) + " Часов");
                        break;
                    }
                }
            }
        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            var intent = new Intent(context, typeof(my_widget_class));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            widgetView.SetOnClickPendingIntent(Resource.Id.widget, piBackground);
        }

        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(my_widget_class));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
        }
    }
}