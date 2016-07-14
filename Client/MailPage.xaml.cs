//13331233 孙中阳
//szy@sunzhongyang.com

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

namespace MidtermProject
{
    //这里仅有类的一部分，另一部分由VS自动生成，所以带有partial标记
    public sealed partial class MailPage : Page
    {
        //用于实现数据绑定的Mailbox对象
        MailViewModel Mailbox;

        //计时器
        DispatcherTimer timer;

        //用于保存页面状态的本地数据
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //通过实现notify接口的source对象实现双向的数据绑定
        ObservableCollection<Mail> source =  new ObservableCollection<Mail>();

        //确定在此页面上
        bool onpage = true;

        //和分享有关
        DataTransferManager dtm;

        //初始化页面和计时器
        public MailPage()
        {
            this.InitializeComponent();

            listview.ItemsSource = source;
            Mailbox = new MailViewModel();

            //计时器每0.5秒检查一次新邮件，如果有新邮件则跳转到check_mail()函数
            timer = new DispatcherTimer();
            timer.Tick += (s, e) => {
                if(onpage == true) check_mail();
            };
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Start();

        }

        //向服务器检查是否有新邮件
        async void check_mail()
        {
            //根据用户名检查
            string data = localseetings.Values["user"].ToString();
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7001/check", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();

            //如果有新邮件
            if (receive != "No")
            {
                //处理报文
                string[] mail = receive.Split('\n');

                //显示邮件到达有关信息
                var idf = new MessageDialog("You have got a mail from " + mail[1]).ShowAsync();
                var db = App.conn;

                //将收到的邮件存入数据库
                var custstmt = db.Prepare("INSERT INTO mail (user, mailbox, sender, receiver, title, time, content) VALUES (?, ?, ?, ?, ?, ?, ?)");

                custstmt.Bind(1, localseetings.Values["user"].ToString());
                custstmt.Bind(2, "receiver_box");
                custstmt.Bind(3, mail[1]);
                custstmt.Bind(4, mail[0]);
                custstmt.Bind(5, mail[2]);
                custstmt.Bind(6, mail[3]);
                custstmt.Bind(7, mail[4]);
                custstmt.Step();

                //如果当前视图正在显示收件箱中的邮件，则更新收件箱视图
                if (localseetings.Values["box"].ToString() == "receive")
                {
                    Sql_Select_mailbox("receiver_box");
                }

                //显示磁贴
                string from = source[0].receiver;
                string subject = source[0].title;

                TileContent content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        DisplayName = "Todos",

                        TileSmall = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                {
                    new TileText()
                    {
                        Text = from
                    },

                    new TileText()
                    {
                        Text = subject,
                        Style = TileTextStyle.CaptionSubtle
                    },
                }
                            }
                        },
                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                {
                    new TileText()
                    {
                        Text = from
                    },

                    new TileText()
                    {
                        Text = subject,
                        Style = TileTextStyle.CaptionSubtle
                    },
                }
                            }
                        },

                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                {
                    new TileText()
                    {
                        Text = from,
                        Style = TileTextStyle.Subtitle
                    },

                    new TileText()
                    {
                        Text = subject,
                        Style = TileTextStyle.CaptionSubtle
                    },
                }
                            }
                        }
                    }
                };

                var notification = new TileNotification(content.GetXml());

                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

                //更新磁贴
                if (SecondaryTile.Exists("MySecondaryTile"))
                {
                    var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile("MySecondaryTile");

                    updater.Update(notification);
                }

            }
        }

        //被导航到此页面后执行的动作
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            onpage = true;
            dtm = DataTransferManager.GetForCurrentView();
            dtm.DataRequested += dtm_DataRequested;

            //恢复之前页面显示的信箱
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

        //点击更改账户信息的按钮后跳转到更改账户信息的页面
        private void setting_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Account), "");
        }

        //点击收件箱中邮件后执行的动作
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //确定点击的邮件
            Mailbox.selectmail = (Mail)(e.ClickedItem);
            title.Text = Mailbox.selectmail.title;

            //根据邮件信息在邮件详细信息栏显示邮件详细信息
            if (localseetings.Values["box"].ToString() == "receive")  this.sender.Text = Mailbox.selectmail.sender;
            else this.sender.Text = Mailbox.selectmail.receiver;

            //将邮件详细信息栏设置为可见，并初始化其内容
            content.Text = Mailbox.selectmail.content;
            this.show_content.Opacity = 0.95;
            time.Text = Mailbox.selectmail.time;

            //将编辑邮件栏设置为不可见
            this.show_content.Visibility = Visibility.Visible;
            showmail.Visibility = Visibility.Collapsed;

        }

        //点击新邮件按钮后执行的动作
        private void New_mail_Click()
        {
            //如果此时页面宽度大于650，则显示编辑邮件栏，否则跳转到新页面
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

        //点击发件箱后执行的动作
        private void Sendbox__Click()
        {
            //设置页面状态为发件箱
            localseetings.Values["box"] = "send";

            //初始化信箱显示内容为发件箱内容
            Sql_Select_mailbox("sender_box");

            //将编辑邮件栏设置为不可见，邮件详情栏设为可见
            this.listview.Opacity = 0.8;
            this.show_content.Visibility = Visibility.Visible;
            showmail.Visibility = Visibility.Collapsed;
            show_list.Opacity = 1;
            send_or_receive.Text = "Send Box";
        }

        //点击收件箱后执行的动作
        private void Receivebox__Click()
        {
            //设置页面状态为收件箱
            localseetings.Values["box"] = "receive";

            //初始化信箱显示内容为收件箱内容
            Sql_Select_mailbox("receiver_box");
            this.listview.Opacity = 0.8;
            this.show_content.Visibility = Visibility.Visible;

            //将编辑邮件栏设置为不可见，邮件详情栏设为可见
            showmail.Visibility = Visibility.Collapsed;
            show_list.Opacity = 1;
            send_or_receive.Text = "Receive Box";
        }

        //点击登出按钮，跳转至登录页面
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //清空页面状态
            localseetings.Values["user"] = "";
            Frame.Navigate(typeof(MainPage), "");
        }

        //确定点击的类型，根据点击的类型决定如何初始化邮件简略信息栏
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

        //点击新建邮件按钮
        async private void createButton_Click_1(object sender, RoutedEventArgs e)
        {
            //获取编辑邮件栏中邮件的具体内容
            string data = t.Text + "\t" + '\n' + localseetings.Values["user"].ToString() + '\n' + Title.Text + '\n' + "2016" + '\n' + Details.Text;

            //向服务器发送邮件具体信息
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7001/send", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();

            //如果发送成功则将发送成功的邮件存入数据库
            if (receive == "success")
            {
                var ix = new MessageDialog("success").ShowAsync();

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

                //如果当前邮件简略信息栏显示发件箱信息，则更新发件箱
                if (localseetings.Values["box"].ToString() == "send")
                {
                    Sql_Select_mailbox("sender_box");
                }
            }

            //如果发送失败，则弹窗提示，并清空编辑邮件栏中的全部内容
            else
            {
                var i = new MessageDialog("failed").ShowAsync();
            }

            t.Text = "";
            Title.Text = "";
            Details.Text = "";
        }

        //点击删除邮件按钮
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //从发件箱，收件箱和数据绑定对象中查找需要删除的邮件，如果有则移除
            if(source.Contains(Mailbox.selectmail)) source.Remove(Mailbox.selectmail);
            if (Mailbox.send_mails.Contains(Mailbox.selectmail)) Mailbox.send_mails.Remove(Mailbox.selectmail);
            if (Mailbox.receive_mails.Contains(Mailbox.selectmail)) Mailbox.receive_mails.Remove(Mailbox.selectmail);

            //从数据库中移除需要删除的邮件
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

            //清空邮件详情栏中的所有内容
            title.Text = "";
            this.sender.Text = "";
            content.Text = "";
            time.Text = "";

        }

        //点击分享按钮后执行的动作
        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem mn = (MenuFlyoutItem)e.OriginalSource;
            DataTransferManager.ShowShareUI();
        }

        //向系统发送分享的内容
        void dtm_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            string textSource = content.Text;
            string textTitle = title.Text;
            DataPackage data = e.Request.Data;
            data.Properties.Title = textTitle;
            data.SetText(textSource);
        }

        //点击查询按钮
        private void Query_button_Click(object sender, RoutedEventArgs e)
        {
            //确定查询文本
            string query = Query.Text;
            query = "%" + query + "%";

            string result = Sql_Select(query);

            var i = new MessageDialog(result).ShowAsync();
        }

        //根据查询文本从数据库中获取需要的邮件
        public string Sql_Select(string query_input)
        {
            //存放查询结果的字符串
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

        //根据信箱种类决定数据绑定对象的内容
        public void Sql_Select_mailbox(string query_input)
        {
            var db = App.conn;
            var statement = db.Prepare("SELECT sender, receiver, title, content, time FROM mail WHERE user = ? and mailbox = ?");
            statement.Bind(1, localseetings.Values["user"].ToString());
            statement.Bind(2, query_input);

            //删除原邮件
            source.Clear();

            //增加查找到的邮件
            do
            {
                statement.Step();
                if (statement[0] == null) break;
                source.Insert(0, new Mail { sender = (string)statement[0], receiver = (string)statement[1], title = (string)statement[2], content = (string)statement[3], time = (string)statement[4] });
            } while (true);
        }

        //点击回复按钮
        private void reply_Click(object sender, RoutedEventArgs e)
        {
            //获取回复邮件的收件人
            string receive_user = this.sender.Text;

            //根据当前窗口大小决定是否跳转到新页面完成邮件的写作
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
