using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NotificationsExtensions.Tiles;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.ApplicationModel.DataTransfer;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MidtermProject
{
    public sealed partial class MailPage : Page
    {
        MailViewModel Mailbox;
        DispatcherTimer timer;
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;
        ObservableCollection<Mail> source =  new ObservableCollection<Mail>();
        bool onpage = true;
        DataTransferManager dtm;

        public MailPage()
        {
            this.InitializeComponent();
            //var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            //viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            //viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;

            listview.ItemsSource = source;
            Mailbox = new MailViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            onpage = true;
            dtm = DataTransferManager.GetForCurrentView();
            dtm.DataRequested += dtm_DataRequested;

            if (e.Parameter.GetType() == typeof(MailViewModel))
            {
                this.Mailbox = (MailViewModel)(e.Parameter);
                if (localseetings.Values["box"].ToString() == "send")
                {
                    Sql_Select_mailbox("sender_box");
                }
                else
                {
                    Sql_Select_mailbox("receiver_box");
                }
            }
        }

        // list元素点击事件
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Mailbox.selectmail = (Mail)(e.ClickedItem);
            title.Text = Mailbox.selectmail.title;
            if (localseetings.Values["box"].ToString() == "receive")  this.sender.Text = Mailbox.selectmail.sender;
            else this.sender.Text = Mailbox.selectmail.receiver;
            content.Text = Mailbox.selectmail.content;
            this.show_content.Opacity = 0.95;
            time.Text = Mailbox.selectmail.time;

            this.show_content.Visibility = Visibility.Visible;
            showmail.Visibility = Visibility.Collapsed;

        }

        // Visual State自适应页面
        private void New_mail_Click()
        {
            // 页面宽度不够时新建信息会在新页面执行
            if (this.ActualWidth > 650)
            {
                this.show_content.Visibility = Visibility.Collapsed;
                showmail.Visibility = Visibility.Visible;
            }
            else
            {
                Frame.Navigate(typeof(new_mail), Mailbox);
                onpage = false;
            }
        }

        // 设定按钮点击事件
>>>>>>> SDP-EasyMail/master
        private void setting_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Account), "");
        }

        private void Sendbox__Click()
        {
            localseetings.Values["box"] = "send";
            Sql_Select_mailbox("sender_box");
            this.listview.Opacity = 0.8;
            this.show_content.Visibility = Visibility.Visible;
            showmail.Visibility = Visibility.Collapsed;
            show_list.Opacity = 1;
            send_or_receive.Text = "Send Box";
        }

        private void Receivebox__Click()
        {
            localseetings.Values["box"] = "receive";
            Sql_Select_mailbox("receiver_box");
            this.listview.Opacity = 0.8;
            this.show_content.Visibility = Visibility.Visible;
            showmail.Visibility = Visibility.Collapsed;
            show_list.Opacity = 1;
            send_or_receive.Text = "Receive Box";
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            localseetings.Values["user"] = "";
            Frame.Navigate(typeof(MainPage), "");
        }

        private void ListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var auto = (TextBlock)(e.ClickedItem);
            if (auto.Tag.ToString() == "Receivebox__Click")
            {
                Receivebox__Click();
            }
            else if(auto.Tag.ToString() == "New_mail_Click")
            {
                New_mail_Click();
            }

            else if (auto.Tag.ToString() == "Sendbox__Click")
            {
                Sendbox__Click();
            }
        }


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(source.Contains(Mailbox.selectmail)) source.Remove(Mailbox.selectmail);
            if (Mailbox.send_mails.Contains(Mailbox.selectmail)) Mailbox.send_mails.Remove(Mailbox.selectmail);
            if (Mailbox.receive_mails.Contains(Mailbox.selectmail)) Mailbox.receive_mails.Remove(Mailbox.selectmail);

            var db = App.conn;
            var statement = db.Prepare("DELETE FROM mail WHERE user = ? and sender = ? and title = ? and content = ? and mailbox = ?" );
            {
                var i = new MessageDialog("Delete success").ShowAsync();
                statement.Bind(1, localseetings.Values["user"].ToString());
                statement.Bind(2, this.sender.Text);
                statement.Bind(3, title.Text);
                statement.Bind(4, content.Text);
                statement.Bind(5, localseetings.Values["box"].ToString() == "send" ? "sender_box":"receiver_box");
                statement.Step();
            }

            title.Text = "";
            this.sender.Text = "";
            content.Text = "";
            time.Text = "";

        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mn = (MenuFlyoutItem)e.OriginalSource;
            DataTransferManager.ShowShareUI();
        }


        private void Query_button_Click(object sender, RoutedEventArgs e)
        {
            string query = Query.Text;
            query = "%" + query + "%";

            string result = Sql_Select(query);

            var i = new MessageDialog(result).ShowAsync();
        }

        //从数据库中取出对应用户的邮件
        public string Sql_Select(string query_input)
        {
            string result = "";

            var db = App.conn;
            var statement = db.Prepare("SELECT user, mailbox, sender, receiver, title, time, content FROM mail WHERE user = ? and title LIKE ? ");
            statement.Bind(1, localseetings.Values["user"].ToString());
            statement.Bind(2, query_input);

            do
            {
                statement.Step();
                if (statement[0] == null) break;
                result += " user: " + (string)statement[0];
                result += " mailbox: " + (string)statement[1];
                result += " sender: " + (string)statement[2];
                result += " receiver: " + (string)statement[3];
                result += " title: " + (string)statement[4];
                result += " time: " + (string)statement[5];
                result += " content: " + (string)statement[6];
                result += "\n";
            } while (true);

            return result;
        }

        public void Sql_Select_mailbox(string query_input)
        {
            var db = App.conn;
            var statement = db.Prepare("SELECT sender, receiver, title, content, time FROM mail WHERE user = ? and mailbox = ?");
            statement.Bind(1, localseetings.Values["user"].ToString());
            statement.Bind(2, query_input);
            source.Clear();
            do
            {
                statement.Step();
                if (statement[0] == null) break;
                source.Insert(0, new Mail { sender = (string)statement[0], receiver = (string)statement[1], title = (string)statement[2], content = (string)statement[3], time = (string)statement[4] });
            } while (true);
        }

        private void reply_Click(object sender, RoutedEventArgs e)
        {
            string receive_user = this.sender.Text;

            if (this.ActualWidth > 650)
            {
                this.show_content.Visibility = Visibility.Collapsed;
                showmail.Visibility = Visibility.Visible;
                t.Text = receive_user;
            }
            else
            {
                Frame.Navigate(typeof(new_mail), receive_user);
                onpage = false;
            }
        }
    }
}
