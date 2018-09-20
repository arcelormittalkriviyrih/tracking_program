using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace KovshService
{
    public partial class Service1 : ServiceBase
    {
        public static StreamWriter file;
        private System.Timers.Timer timer1;
        public static string[] datas = new string[14];

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            file = new StreamWriter(File.Open("WaterService.log", FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.GetEncoding(1251));
            file.Flush();
            try
            {
                datas = System.IO.File.ReadAllLines("C:\\Service\\Config.txt");
            }
            catch (Exception ex) { }
            int intr = Convert.ToInt16(datas[3]);
            if (intr == null || intr == 0)
                intr = 60000;
            this.timer1 = new System.Timers.Timer();
            this.timer1.Enabled = true;
            this.timer1.Interval = intr;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
            this.timer1.AutoReset = true;
            this.timer1.Start();
            prot("  Служба запущена ");
        }

        protected override void OnStop()
        {
            try
            {
                file.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " Служба остановлена ");
                file.Flush();
            }
            catch (Exception ee)
            {
                EventLog.WriteEntry("KovshService-ErrorOnStop", ee.Message);
            }
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (File.Exists("C:\\Service\\stopws.txt") == false)
            {
                timer1.Enabled = false;
                try
                {
                    datas = System.IO.File.ReadAllLines("C:\\Service\\Config.txt");
                }
                catch (Exception ex) { prot("  ОШИБКА ОТКРЫТИЯ КОНФИГУРАЦИОННОГО ФАЙЛА - " + ex.ToString()); }

                Process proc = new Process();
            
                string fname = datas[0];
                proc.StartInfo.FileName = fname;
                proc.StartInfo.Arguments = "";

                bool yp = false;
                Process[] process = Process.GetProcesses();
                string pr = "";
                string prc = "";
                int iSub = datas[0].LastIndexOf("\\");
                iSub++;
                prc = datas[0].Substring(iSub, datas[0].Length - iSub);

                for (int i = 0; i < process.Length; i++)
                {
                    pr = process[i].ProcessName.ToString();
                    if (pr == prc)
                    {
                        yp = true;
                    }
                }
                prot(" Поиск процесса : " + prc + "  " + yp.ToString());

                if (!yp)
                {
                    prot("Запуск процесс : " + fname);
                    proc.Start();
                    proc.WaitForExit();
                    prot("Код завершения : " + proc.ExitCode.ToString());
                }
                else
                {
                    prot("Процесс : " + fname + " уже работает");
                }

                proc.Close();

                timer1.Enabled = true;
             
            }
            else
            {
                prot("  простой по наличию файла stopws.txt ");
            }
        }

        private void prot(string text)
        {
            file.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + text);
            file.Flush();
        }
    }
}
