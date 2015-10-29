﻿
using MSHTML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.Extensions;
using WinFormsClient.WBMode;

namespace WinFormsClient
{
    public partial class TaoLoginForm : BaseForm
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public WBTaoLoginState wbMode;
        public TaoLoginForm(WBTaoLoginState wbMode)
        {
            this.wbMode = wbMode;
            InitializeComponent();
            this.Load += (s, e) => { wbMode.Navigate("https://login.taobao.com/member/login.jhtml?style=mini_top&redirectURL=http%3A%2F%2Fcontainer.api.taobao.com%2Fcontainer%3Fappkey%3D23140690"); };
            wbMode.AskHideUI += () =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close(); 
            };
            wbMode.WB.ProgressChanged2 += WbMode_ProgressChanged;
            this.Controls.Add(wbMode.WB);

            this.Text += " [IE v" + wbMode.WB.Version.ToString() + "]";
        }


        private void WbMode_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.progressBar1.Maximum = 100;
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_CLOSE))
            {
                // 点击winform右上关闭按钮 
                // 加入想要的逻辑处理
                this.wbMode.OnUserCancelLogin();
                this.DialogResult = DialogResult.Cancel;
            }
            base.WndProc(ref msg);
        }
    }
}
