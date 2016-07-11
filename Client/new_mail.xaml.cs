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
    public sealed partial class new_mail : Page
    {
        MailViewModel Mailbox = new MailViewModel();
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public new_mail()
        {
            this.InitializeComponent();
        }

        async private void createButton_Click_1(object sender, RoutedEventArgs e)
        {
            string data =  t.Text + "\t" + '\n' + localseetings.Values["user"].ToString() + '\n' + Title.Text + '\n' + "2016" + '\n' + Details.Text;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7001/send", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();
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
                var i = new MessageDialog("failed").ShowAsync();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter.GetType() == typeof(string))
            {
                this.t.Text = (string)(e.Parameter);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MailPage), Mailbox);
        }
    }
}
