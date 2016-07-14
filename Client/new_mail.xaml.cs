//13331233 孙中阳
//szy@sunzhongyang.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MidtermProject
{
    //这里仅有类的一部分，另一部分由VS自动生成，所以带有partial标记
    public sealed partial class new_mail : Page
    {
        //建立MailBox对象，用于完成数据绑定功能，当绑定的数据发生变化后通知UI
        MailViewModel Mailbox = new MailViewModel();

        //提供本地数据存储以存储页面状态
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //执行默认初始化
        public new_mail()
        {
            this.InitializeComponent();
        }

        //点击发信按钮
        async private void createButton_Click_1(object sender, RoutedEventArgs e)
        {
            //构造发送的邮件的内容并发送至服务器
            string data =  t.Text + "\t" + '\n' + localseetings.Values["user"].ToString() + '\n' + Title.Text + '\n' + "2016" + '\n' + Details.Text;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7001/send", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();

            //如发送成功则将信件信息存入数据库
            if (receive == "success")
            {
                var i = new MessageDialog("success").ShowAsync();
                var db = App.conn;

                var custstmt = db.Prepare("INSERT INTO mail (user, mailbox, sender, receiver, title, time, content) VALUES (?, ?, ?, ?, ?, ?, ?)");

                custstmt.Bind(1, localseetings.Values["user"].ToString());
                custstmt.Bind(2, "sender_box");
                custstmt.Bind(3, localseetings.Values["user"].ToString());
                custstmt.Bind(4, t.Text);
                custstmt.Bind(5, Title.Text);
                custstmt.Bind(6, "2016");
                custstmt.Bind(7, Details.Text);
                custstmt.Step();
                Frame.Navigate(typeof(MailPage), "");
            }

            else
            {
                //发送不成功则弹出窗口提示
                var i = new MessageDialog("failed").ShowAsync();
            }
        }

        //设置导航到此页面时默认执行的动作
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter.GetType() == typeof(string))
            {
                this.t.Text = (string)(e.Parameter);
            }
        }

        //点击取消按钮返回邮件页面
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MailPage), Mailbox);
        }
    }
}
