using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormSpammer {
    public partial class Form1 : Form {
        private dmsoft dm;
        Script script;
        static String LocalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stormspammer");
        public Form1() {
            try {
                if (!IsAdministrator()) {
                    MessageBox.Show("请以管理员身份打开");
                    return;
                }
                // Determine whether the directory exists.
                if (!Directory.Exists(LocalPath))
                    Directory.CreateDirectory(LocalPath);
                if (!File.Exists(LocalPath + @"\dm.dll")) { 
                    byte[] Save = global::StormSpammer.Properties.Resources.dm;
                    FileStream fsObj = new FileStream(LocalPath + @"\dm.dll", FileMode.CreateNew);
                    fsObj.Write(Save, 0, Save.Length);
                    fsObj.Close();
                }
                RegCom.AutoRegCom("regsvr32 " + LocalPath + @"\dm.dll -s");
            }
            catch (Exception e) {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            // 创建全局对象，此对象必须全程保持，不可释放.
            dm = new dmsoft();
            InitializeComponent();
            toolStripStatusLabel2.Text = "大漠插件版本：" + dm.Ver();
        }

        public static bool IsAdministrator() {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            //WindowsBuiltInRole可以枚举出很多权限，例如系统用户、User、Guest等等
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e) {
            script.Stop();
        }

        private void SetProgress(object sender, object e) {
            int status = Convert.ToInt32(e);
            if(status == 0)
                buttonControl.Text = "开始";
            else if (status == 1)
                buttonControl.Text = "停止"; ;
        }
        private void buttonControl_Click(object sender, EventArgs e) {
            int count = Int32.Parse(textBoxCount.Text);
            int interval = Int32.Parse(textBoxInterval.Text);
            script = new Script(dm, int.Parse(textBoxCount.Text), int.Parse(textBoxInterval.Text), textBoxContent.Text);
            script.OnProgressChanged += SetProgress;

            if (count == 0 || interval == 0) {
                MessageBox.Show("不能为0");
                return;
            }
            if (buttonControl.Text == "开始") {               
                script.Start();        
            } else {
                script.Stop();
            }
        }

        private void select_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string file = dialog.FileName;
                Debug.WriteLine(file);
                string text = System.IO.File.ReadAllText(@file);
                // Display the file contents to the console. Variable text is a string.
                textBoxContent.Text = text;
            }
        }
    }
}
