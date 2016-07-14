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
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace MidtermProject
{
    //这里仅有类的一部分，另一部分由VS自动生成，所以带有partial标记
    public sealed partial class MainPage : Page
    {
        //设置本地存储，用于保存页面信息
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //默认初始化
        public MainPage()
        {
            this.InitializeComponent();

        }
        //被导航到页面后确定顶部导航栏
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
        }

        //点击注册按钮后跳转至注册页面
        private void Regester_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegesiterScene), "");
        }

        //点击登录按钮后登录，检查必要的信息是否完整，并显示登录是否成功
        async private void Login_Click(object sender, RoutedEventArgs e)
        {
            if(Username.Text == "")
            {
                var i = new MessageDialog("Please enter the username!").ShowAsync();
                return;
            }

            if (Password.Password == "")
            {
                var i = new MessageDialog("Please enter the password!").ShowAsync();
                return;
            }

            //发送用户名和密码
            string data = Username.Text + '\t' + Password.Password;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7000/login", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();

            //登录成功
            if (receive == "Login success")
            {
                this.localseetings.Values["user"] = Username.Text;
                this.localseetings.Values["box"] = "receive";
                Frame.Navigate(typeof(MailPage), "");

            }

            //登录不成功
            else
            {
                var c = new MessageDialog(receive).ShowAsync();
            }
        }
    }
}
