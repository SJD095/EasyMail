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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MidtermProject
{
    //这里仅有类的一部分，另一部分由VS自动生成，所以带有partial标记
    public sealed partial class Account : Page
    {
        //提供本地数据存储以存储页面状态
        ApplicationDataContainer localseetings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //默认初始化
        public Account()
        {
            this.InitializeComponent();

        }

        //导航到此页面时执行的动作
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            //如果可返回则显示返回按钮
            if (rootFrame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        //点击更改按钮
        async private void Register_Click(object sender, RoutedEventArgs e)
        {
            //检查输入是否合法
            if (Username.Password == "")
            {
                var i = new MessageDialog("Please enter the username!").ShowAsync();
                return;
            }

            if (newPassword.Password != Configure.Password || newPassword.Password == "")
            {
                var i = new MessageDialog("Please check the password!").ShowAsync();
                return;
            }

            //查看是否同意用户协议
            if (Agree.IsChecked == false) return;

            //根据本地文件存储的用户信息等向服务器发送确认信息
            string data =  localseetings.Values["user"].ToString() + '\t' + Username.Password + '\t' + newPassword.Password;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7000/change", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();

            //显示服务器返回内容
            var c = new MessageDialog(receive).ShowAsync();

            //如果更改用户信息成功则导航到登录页面，到登录页面后不可点击返回按钮返回到本此页面
            if (receive == "change success") Frame.Navigate(typeof(MainPage), "");
        }
    }

}
