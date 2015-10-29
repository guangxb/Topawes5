﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using WinFormsClient.Extensions;
using Nx.EasyHtml.Html;
using Moonlight;
using System.Collections;
using Moonlight.WindowsForms.Controls;
using MSHTML;

namespace WinFormsClient.WBMode
{
    public class WBTaoLoginState : WebBrowserMode<WBTaoLoginState>
    {
        public event Action UserCancelLogin;
        public event Action AskHideUI;
        public string AuthorizedUrl { get; set; }
        public bool IsPasswordLogin { get; set; }
        public WBTaoLoginState()
        {
            AuthorizedUrl = "http://123.56.122.122:8080/";
        }

        protected override void EnterModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {
            webBrowser.Dock = DockStyle.Fill;
            webBrowser.ScrollBarsEnabled = false;
            webBrowser.ScriptErrorsSuppressed = true;
            //WB.IsWebBrowserContextMenuEnabled = false;
        }

        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith("http://fuwu.taobao.com"))
            {
                e.Cancel = true;
                Process.Start(e.Url.ToString());
            }
        }
        //获取所有的frame
        private List<HtmlWindow> GetAllWindow(HtmlWindow wb)
        {
            List<HtmlWindow> res = new List<HtmlWindow>();
            foreach (HtmlWindow item in wb.Document.Window.Frames)
            {
                res.Add(item);
            }
            foreach (HtmlElement item in wb.Document.All)
            {
                if (item.Children.Count > 0)
                {
                    foreach (HtmlWindow hw in item.Document.Window.Frames)
                    {
                        res.AddRange(GetAllWindow(hw));
                    }
                }
            }
            return res;
        }

        protected override async void OnDocumentCompleted(string html, string url)
        {
            Application.DoEvents();
            var doc = (HTMLDocument)WB.Document.DomDocument;
            //var winList = GetAllWindow(WB.Document.Window);
            if (WB.Document.Title == "导航已取消" || WB.Document.Title == "无法显示此页")
            {
                return;
            }

            HtmlElement J_Message = WB.Document.All["J_Message"];
            if (J_Message != null && !string.IsNullOrEmpty(J_Message.InnerText))
            {
                return;
            }
            if (html.IndexOf("secondVerifyRender();") != -1)
            {
                return;
            }
            if (html.IndexOf("auther('true')") != -1)
            {
                IHTMLControlElement button = (IHTMLControlElement)doc.getElementById("sub");
                if (button.clientHeight > 0)
                {
                    //自动点击授权按钮
                    WB.Document.InvokeScript("auther", new object[] { "true" });
                    return;
                }
            }
            if (url.IndexOf("https://login.taobao.com/member/login.jhtml") != -1)
            {
                try
                {
                    IHTMLControlElement J_QuickLogin = (IHTMLControlElement)doc.getElementById("J_QuickLogin");
                    IHTMLControlElement J_Static = (IHTMLControlElement)doc.getElementById("J_Static");
                    var body = doc.getElementsByTagName("body").AsList<IHTMLControlElement>()[0];
                    if (J_Static == null)
                    {
                        //可能遇到了javascript跳转逻辑
                        return;
                    }
                    WB.Document.All["J_SubmitStatic"].Click += WBTaoLoginState_Click;
                    await Task.Delay(100);
                    if (J_Static.clientWidth > 0 && J_Static.clientHeight > 0)
                    {
                        IsPasswordLogin = true;
                        StaticLogin(doc);
                    }
                    else if (J_QuickLogin.clientWidth > 0 && J_QuickLogin.clientHeight > 0)
                    {
                        IsPasswordLogin = false;
                        //<span class="title">检测到您已经登录的账户:</span>
                        //<form action = "" >
                        //    < input name="redirectURL" id="el_redirectURL" type="hidden" value="http://www.taobao.com">
                        //    <ul class="userlist">
                        //    <li class="item-sso-user current"><input name = "user" class="r-sso-user r-wwuser" id="ra-0" type="radio" checked="" value="cntaobaocendart" data-type="ww" data-index="0"> <label for="ra-0">cendart</label></li></ul>
                        //    <div class="submit">
                        //        <button id = "J_SubmitQuick" type="submit">快速登录</button>
                        //    </div>
                        //    <ul class="entries">        
                        //    <li><a class="module-switch" id="J_Quick2Static" href="#" data-target="static">使用其他账户登录</a></li>
                        //      </ul>
                        //</form>
                        var current = WB.Document.Body.JQuerySelect("#J_QuickLogin .userlist .current label");
                        var xdoc = new Nx.EasyHtml.Html.Parser.JumonyParser().Parse(WB.Document.Body.InnerHtml, WB.Document.Url);
                        var label = xdoc.FindFirst(".current label");
                        if (current.Any())
                        {
                            if (AppSetting.UserSetting != null)
                            {
                                //使用其他账户登录
                                WB.Document.All["J_Quick2Static"].InvokeMember("click");
                                StaticLogin(doc);
                            }
                            else
                            {
                                var nick = current[0].InnerText;
                                InitAppUserSetting(nick);
                                if (!string.IsNullOrEmpty(AppSetting.UserSetting.Get<string>("TaoUserName")))
                                {
                                    if (nick == AppSetting.UserSetting.Get<string>("TaoUserName"))
                                    {
                                        if (AppSetting.UserSetting.Get<bool>("AutoSelectLoginedAccount"))
                                            WB.Document.All["J_SubmitQuick"].InvokeMember("click");
                                    }
                                    else
                                    {
                                        //使用其他账户登录
                                        WB.Document.All["J_Quick2Static"].InvokeMember("click");
                                        StaticLogin(doc);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (html.IndexOf("选择其中一个已登录的账户") != -1)
            {
                //帮着点了
                WB.Document.All["J_SubmitQuick"].InvokeMember("click");
            }
            if (url.StartsWith(AuthorizedUrl))
            {
                if (url != AuthorizedUrl)
                {
                    WB.Navigate(AuthorizedUrl);
                }
            }
            if (url.StartsWith("http://container.api.taobao.com/container") && WB.DocumentTitle == "错误页面")
            {
                AppSetting.UserSetting.SetNull("AutoSelectLoginedAccount");
            }
        }

        private void WBTaoLoginState_Click(object sender, HtmlElementEventArgs e)
        {
            var doc = (HTMLDocument)WB.Document.DomDocument;
            IHTMLInputTextElement TPL_username_1 = (IHTMLInputTextElement)doc.getElementById("TPL_username_1");
            var userName = TPL_username_1.value;
            InitAppUserSetting(userName);
            IHTMLInputTextElement TPL_password_1 = (IHTMLInputTextElement)doc.getElementById("TPL_password_1");
            var password = TPL_password_1.value;
            AppSetting.UserSetting.Set("TaoUserName", userName, true);
            AppSetting.UserSetting.Set("TaoPassword", password, true);
        }

        private void InitAppUserSetting(string userName)
        {
            AppSetting.InitializeUserSetting(userName, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, userName + ".bin"));
        }


        private async void StaticLogin(HTMLDocument doc)
        {
            IHTMLInputTextElement TPL_username_1 = (IHTMLInputTextElement)doc.getElementById("TPL_username_1");
            if (AppSetting.UserSetting != null)
            {
                TPL_username_1.value = AppSetting.UserSetting.UserName;
            }
            var userName = TPL_username_1.value;
            InitAppUserSetting(userName);
            //J_Static表单登录
            if (!string.IsNullOrEmpty(TPL_username_1.value))
            {
                if (AppSetting.UserSetting.UserName != TPL_username_1.value)
                {
                    userName = TPL_username_1.value;
                    InitAppUserSetting(userName);
                }
                var userNameEl = WB.Document.GetElementById("TPL_username_1");
                if (userNameEl != null)
                {
                    userNameEl.InvokeMember("focus");

                    var passwordEl = WB.Document.GetElementById("TPL_password_1");
                    passwordEl.InvokeMember("focus");
                    if (AppSetting.UserSetting.Get<bool>("AutoSelectLoginedAccount"))
                    {
                        var password = AppSetting.UserSetting.Get<string>("TaoPassword");
                        if (!string.IsNullOrEmpty(password))
                        {
                            passwordEl.SetAttribute("value", password);
                            Application.DoEvents();
                            //如果验证码窗口没有显示就点击提交按钮，否则用户自己去点
                            var xdoc = new Nx.EasyHtml.Html.Parser.JumonyParser().Parse(WB.Document.Body.InnerHtml, WB.Document.Url);
                            var checkCode = xdoc.Find(".field-checkcode.hidden");
                            if (checkCode.Any())
                            {
                                await Task.Delay(200);
                                WB.Document.All["J_SubmitStatic"].InvokeMember("click");
                            }
                            return;
                        }
                    }
                }
            }
        }

        protected override void OnNavigated(string url)
        {
            if (url == AuthorizedUrl)
            {
                WB.Stop();
                if (IsPasswordLogin)
                {
                    AppSetting.UserSetting.Set("TaoPasswordOK", true);
                }
                InvokeEventHandler(this.AskHideUI);
            }
        }

        private void InvokeEventHandlerAsync(Action eventHandler)
        {
            var h = eventHandler;
            if (h != null)
            {
                h.BeginInvoke(null, null);
            }
        }

        private void InvokeEventHandler(Action eventHandler)
        {
            var h = eventHandler;
            if (h != null)
            {
                h.Invoke();
            }
        }

        /// <summary>
        /// 触发用户取消了登录
        /// </summary>
        public void OnUserCancelLogin()
        {
            InvokeEventHandler(UserCancelLogin);
        }

        protected override void LeaveModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {
            webBrowser.ScrollBarsEnabled = true;
            webBrowser.ScriptErrorsSuppressed = true;
        }
    }
}
