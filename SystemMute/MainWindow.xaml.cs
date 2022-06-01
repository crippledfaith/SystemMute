using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;


namespace SystemMute
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        GlobleKeyboardShotcut globleKeyboardShotcut = new GlobleKeyboardShotcut();
        private NotifyIconWrapper notifyIconWrapper = new NotifyIconWrapper();
        private bool _showInTaskbar;
        private bool IsMuted { get; set; }
        private Timer timer { get; set; } = new Timer();
        private CoreAudioController CoreAudioController { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var location = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            key.SetValue("Mic Mute", location);
            CoreAudioController = new CoreAudioController();
            globleKeyboardShotcut.MuteShutcutPressedEvent += MuteShutcutPressedEvent;
            notifyIconWrapper.ExitItemOnClickEvent += ExitItemOnClickEvent;
            notifyIconWrapper.OpenItemOnClickEvent += OpenItemOnClickEvent;
            notifyIconWrapper.MuteOrUnMuteOnClickEvent += MuteOrUnMuteOnClickEvent;
            UpdateIconAndNotify(false);
            SetupUpdateTimer();
        }


        private void SetupUpdateTimer()
        {
            timer.Interval = 3000;
            timer.Start();
            timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            Application.Current.Dispatcher.Invoke(() => UpdateIconAndNotify(true));
            timer.Start();
        }

        public void UpdateIconAndNotify(bool notify = false)
        {
            var isMuted = IsMuted;
            var devices = CoreAudioController.GetDevices(DeviceType.Capture, DeviceState.Active);
            var device = devices.FirstOrDefault(x => x.IsDefaultDevice);
            if (device != null)
            {
                Stream iconStream = Application
                    .GetResourceStream(new Uri("pack://application:,,,/SystemMute;component/Microphone Mute.ico"))
                    .Stream;
                notifyIconWrapper.UpdateMuteOrUnMuteText(device.IsMuted);
                if (!device.IsMuted)
                {
                    iconStream = Application
                        .GetResourceStream(new Uri("pack://application:,,,/SystemMute;component/Microphone.ico"))
                        .Stream;
                }
                IsMuted = device.IsMuted;
                notifyIconWrapper.ChangeIcon(new System.Drawing.Icon(iconStream));
            }
            if (notify && isMuted != IsMuted)
                notifyIconWrapper.ShowNotification(IsMuted);
        }
        private void OpenItemOnClickEvent(object sender, EventArgs e)
        {
            MuteUnMute();
        }

        private void ExitItemOnClickEvent(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MuteOrUnMuteOnClickEvent(object? sender, EventArgs e)
        {
            MuteUnMute();
        }
        private void MuteShutcutPressedEvent()
        {
            MuteUnMute();
        }

        private void MuteUnMute()
        {

            var devices = CoreAudioController.GetDevices(DeviceType.Capture, DeviceState.Active);
            var device = devices.FirstOrDefault(x => x.IsDefaultDevice);
            if (device != null)
            {
                device?.Mute(!device.IsMuted);
                notifyIconWrapper.UpdateMuteOrUnMuteText(device.IsMuted);
                Stream iconStream = Application
                    .GetResourceStream(new Uri("pack://application:,,,/SystemMute;component/Microphone Mute.ico"))
                    .Stream;
                if (!device.IsMuted)
                {
                    iconStream = Application
                        .GetResourceStream(new Uri("pack://application:,,,/SystemMute;component/Microphone.ico"))
                        .Stream;
                }
                IsMuted = device.IsMuted;
                notifyIconWrapper.ChangeIcon(new System.Drawing.Icon(iconStream));
            }
            notifyIconWrapper.ShowNotification(IsMuted);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            globleKeyboardShotcut.OnSourceInitialized(this);
            this.Visibility = Visibility.Hidden;
        }



        protected override void OnClosed(EventArgs e)
        {
            globleKeyboardShotcut.UnRegister();

            base.OnClosed(e);
        }
    }

}
