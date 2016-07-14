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
    public sealed partial class RegesiterScene : Page
    {
        //构造函数
        public RegesiterScene()
        {
            //默认初始化
            this.InitializeComponent();

            //设置标题栏
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        //设置导航到此页面时默认执行的动作
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            //显示返回按钮
            if (rootFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }
        }

        //点击注册按钮执行注册
        async private void Register_Click(object sender, RoutedEventArgs e)
        {

            //检测用户输入是否有异常
            if (Username.Text == "")
            {
                //有异常则弹出提示信息
                var i = new MessageDialog("Please enter the username!").ShowAsync();
                return;
            }

            if (Password.Password != Configure.Password || Password.Password == "")
            {
                var i = new MessageDialog("Please check the password!").ShowAsync();
                return;
            }

            //检测是否已同意用户协议
            if (Agree.IsChecked == false) return;

            //向服务器发送用户名和密码
            string data =Username.Text + '\t' + Password.Password;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync("http://sunzhongyang.com:7000/register", new StringContent(data));
            string receive = await response.Content.ReadAsStringAsync();
            var c = new MessageDialog(receive).ShowAsync();

        }
    }
}
