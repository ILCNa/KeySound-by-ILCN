using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using IWshRuntimeLibrary;
namespace keySound
{
    public partial class MainForm : Form
    {
        WaveOutEvent[] commonPlayers;
        WaveOutEvent[] enterPlayers;
        WaveOutEvent[] spacePlayers;
        WaveOutEvent[] backSpacePlayers;

        AudioFileReader[] commonAduioFiles;
        AudioFileReader[] enterAduioFiles;
        AudioFileReader[] spaceAduioFiles;
        AudioFileReader[] backSpaceAduioFiles;
        
        string packName = "test";//加载音效包名

        string[] packNames = Directory.GetFileSystemEntries(@"packs");



        bool autoStart = false;
        bool statring = false;//是否可以播放

        string[] commonfiles;
        string[] enterfiles;
        string[] spacefiles;
        string[] backSpacefiles;

        Random random;
        int randLast=0;//上一次随机结果
        int randNow=0;//此次随机结果

        Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>();

        public MainForm()
        {
            InitializeComponent();
            
            initDirectory();
            for (int i = 0; i < packNames.Length; i++) { packNames[i] = packNames[i].Split('\\')[1]; }
            soundChoseBox.Text = packName;
            soundChoseBox.Items.AddRange(packNames);

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-minimized")
                {
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
                }

            }
            if (IsLnk()) 开机自启动ToolStripMenuItem.Checked = true;
            else 开机自启动ToolStripMenuItem.Checked = false;

            if (!System.IO.File.Exists("config.txt")){
                using (StreamWriter writer = new StreamWriter("config.txt"))
                {
                    autoStart = false;
                    writer.WriteLine("autoStart=false");
                } }
            using (StreamReader reader = new StreamReader("config.txt"))
            {
                string line = reader.ReadLine();
                if (line.Split('=')[1] == "true") { autoStart = true;toolStripMenuItem4.Checked = true; statring = true; buttonTurn.Text = "停止"; }
            }

            // 注册全局键盘事件监听器
            KeyboardHook.Start();
            KeyboardHook.KeyDown += KeyboardHook_KeyDown;
            KeyboardHook.KeyUp += KeyboardHook_KeyUp;

        }
        private bool IsLnk()
        {

            if (System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), string.Format("{0}.lnk", "KeySound"))))
                return true;
            else return false;
        }
        private void initDirectory()
        {
            commonfiles = Directory.GetFiles(@"packs\" + packName + "\\common");
            enterfiles = Directory.GetFiles(@"packs\" + packName + "\\enter");
            spacefiles = Directory.GetFiles(@"packs\" + packName + "\\space");
            backSpacefiles = Directory.GetFiles(@"packs\" + packName + "\\backspace");

            commonPlayers = new WaveOutEvent[commonfiles.Length];
            enterPlayers = new WaveOutEvent[enterfiles.Length];
            spacePlayers = new WaveOutEvent[spacefiles.Length];
            backSpacePlayers = new WaveOutEvent[backSpacefiles.Length];

            commonAduioFiles = new AudioFileReader[commonfiles.Length];
            enterAduioFiles = new AudioFileReader[enterfiles.Length];
            spaceAduioFiles = new AudioFileReader[spacefiles.Length];
            backSpaceAduioFiles = new AudioFileReader[backSpacefiles.Length];

            initWaveOutEvents(commonfiles, commonPlayers, commonAduioFiles);
            initWaveOutEvents(enterfiles, enterPlayers, enterAduioFiles);
            initWaveOutEvents(spacefiles, spacePlayers, spaceAduioFiles);
            initWaveOutEvents(backSpacefiles, backSpacePlayers, backSpaceAduioFiles);

            random = new Random(GetHashCode());
        }

        private void initWaveOutEvents(String[] commonfiles,WaveOutEvent[] commonPlayers,AudioFileReader[] commonAduioFiles)
        {
            for (int i = 0; i < commonfiles.Length; i++)
            {
                commonAduioFiles[i] = new AudioFileReader(commonfiles[i]);
                commonPlayers[i] = new WaveOutEvent();
                commonPlayers[i].Init(commonAduioFiles[i]);
            }
        }

        private void KeyboardHook_KeyUp(Keys key)
        {
            keyStates[key] = false;
        }

        private void KeyboardHook_KeyDown(Keys key)
        {
            if (!keyStates.ContainsKey(key)|| keyStates[key] == false)
            {
                
                keyStates[key] = true;
                if (key.Equals(Keys.Enter))
                {
                    playSound(enterPlayers,enterAduioFiles);
                }
                else if (key.Equals(Keys.Space))
                {
                    playSound(spacePlayers,spaceAduioFiles);
                }
                else if (key.Equals(Keys.Back))
                {
                    playSound(backSpacePlayers,backSpaceAduioFiles);
                }
                else
                {
                    playSound(commonPlayers,commonAduioFiles);
                }
              
            }
     
            else return;

        }

        private void buttonTurn_Click(object sender, EventArgs e)
        {
            if (statring == false) { statring = true; buttonTurn.Text = "停止";

            }
            else
            {
                statring = false ; buttonTurn.Text = "启动";
            }
        }


        void playSound(WaveOutEvent[] commonPlayers, AudioFileReader[] commonAduioFiles)
        {
            if(statring == false) {return; }

  

            randLast =randNow=random.Next()%commonAduioFiles.Length;
            if (commonAduioFiles[randNow].Position != 0) commonAduioFiles[randNow].Position = 0;
            commonPlayers[randNow].Play();

            
        }


        private void volumeChange(object sender, EventArgs e)
        {
            for (int i = 0;i<commonfiles.Length;i++)
                commonPlayers[i].Volume = volumeSlider.Volume;
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // 在窗体关闭时取消全局键盘事件监听器
            KeyboardHook.KeyDown -= KeyboardHook_KeyDown;
            KeyboardHook.Stop();
        }

        private void changePack(object sender, EventArgs e)
        {
            packName=soundChoseBox.Text;
            initDirectory();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("欢迎使用KeySound\n反馈请联系2433749091@qq.com","关于");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)    //最小化到系统托盘
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;

                ShowInTaskbar = false;

            }
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1_DoubleClick(sender);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;

            ShowInTaskbar = true;
        }

        private void notifyIcon1_DoubleClick(object sender)
        {
            WindowState = FormWindowState.Normal;

            ShowInTaskbar = true;
        }

        private void 开机自启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!开机自启动ToolStripMenuItem.Checked)
            {
                开机自启动ToolStripMenuItem.Checked = true;

                creatLnk();

            }
            else
            {
                开机自启动ToolStripMenuItem.Checked = false;
                deleteLnk();
            }
        }

        private void deleteLnk()
        {
            System.IO.File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), string.Format("{0}.lnk", "KeySound")));
        }

        private void creatLnk()
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), string.Format("{0}.lnk", "KeySound")));//创建快捷方式对象
            shortcut.TargetPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "KeySound.exe"; //标路径
            shortcut.Arguments = "-minimized";
            shortcut.WorkingDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "KeySound.exe");//设置起始位置
            shortcut.WindowStyle = 7;//设置运行方式，默认为常规窗口
            shortcut.Description = "Nothing";//设置备注
            shortcut.IconLocation = string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "resources/main.ico") ? AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "KeySound.exe" : AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"resources/main.ico";//设置图标路径
            shortcut.Save();//保存快捷方式

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //自动启动音效
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
           
            if(toolStripMenuItem4.Checked)
            {
                using (StreamWriter writer = new StreamWriter("config.txt"))
                {
                    toolStripMenuItem4.Checked = false;
                    autoStart = false;
                    writer.WriteLine("autoStart=false");
                }
            }
            else
            using (StreamWriter writer = new StreamWriter("config.txt"))
            {
                    toolStripMenuItem4.Checked = true;
                    autoStart = true;
                    writer.WriteLine("autoStart=true");
            }
        }
    }

    public static class KeyboardHook
    {
        public delegate void KeyDownHandler(Keys key);
        public static event KeyDownHandler KeyDown;

        public delegate void KeyUpHandler(Keys key);
        public static event KeyUpHandler KeyUp; // 添加键盘抬起事件

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc proc = HookCallback;
        private static IntPtr hookId = IntPtr.Zero;

        public static void Start()
        {
            hookId = SetHook(proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(hookId);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyDown?.Invoke((Keys)vkCode);
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) // 捕获键盘抬起事件
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyUp?.Invoke((Keys)vkCode);
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
