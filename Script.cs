using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormSpammer {
    public class Script {
        bool status;
        dmsoft dm;
        int count;
        int interval;
        string phrase;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public Script(dmsoft m_dm, int c, int i, string p) {
            status = false;
            dm = m_dm;
            count = c; 
            interval = i;
            phrase = p;          
        }

        public void Start() {
            status = true;
            ProgressChanged(this, status);
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var task = Task.Factory.StartNew(() => {
                int hwnd = dm.FindWindow("Heroes of the Storm", "");
                dm.BindWindow(hwnd, "gdi", "windows", "windows", 0);
                for (int i = 0; i < count; i++) {
                    string[] words = phrase.Split('#');
                    foreach (var word in words) {
                        if (token.IsCancellationRequested) {
                            Console.WriteLine("Abort mission success!");
                            return;
                        }
                        int ret1 = dm.SendString(hwnd, $"{word}");
                        if (ret1 == 0) {
                            MessageBox.Show("失败");
                            break;
                        }
                        dm.KeyPress(13); // Enter
                        Thread.Sleep(interval);
                        Console.WriteLine(status);
                    }
                }
                status = false;
                ProgressChanged(this, status);            
            }, tokenSource.Token);
            //tokenSource.Cancel();
        }

        public void Stop() {
            tokenSource.Cancel();
            status = false;
            ProgressChanged(this, status);
        }    

        // 声明一个监控进度变化的委托
        public delegate void ProgressChangedHandler(object sender, object e);
        // 声明一个监控进度变化的事件
        public event ProgressChangedHandler OnProgressChanged;
        //处理注册的事件
        private void ProgressChanged(object sender, object e) {
            //判断事件是否注册，注册了就执行对应的委托方法
            OnProgressChanged?.Invoke(sender, e);
        }
    }
}
