
using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace SystemMute
{
    public class NotifyIconWrapper : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NotifyIconWrapper), new PropertyMetadata(
                (d, e) =>
                {
                    var notifyIcon = ((NotifyIconWrapper)d)._notifyIcon;
                    if (notifyIcon == null)
                        return;
                    notifyIcon.Text = (string)e.NewValue;
                }));

        private static readonly DependencyProperty NotifyRequestProperty =
            DependencyProperty.Register("NotifyRequest", typeof(NotifyRequestRecord), typeof(NotifyIconWrapper),
                new PropertyMetadata(
                    (d, e) =>
                    {
                        var r = (NotifyRequestRecord)e.NewValue;
                        ((NotifyIconWrapper)d)._notifyIcon?.ShowBalloonTip(r.Duration, r.Title, r.Text, r.Icon);
                    }));

        private static readonly RoutedEvent OpenSelectedEvent = EventManager.RegisterRoutedEvent("OpenSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NotifyIconWrapper));

        private static readonly RoutedEvent ExitSelectedEvent = EventManager.RegisterRoutedEvent("ExitSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(NotifyIconWrapper));

        private readonly NotifyIcon _notifyIcon;
        public event EventHandler ExitItemOnClickEvent;
        public event EventHandler OpenItemOnClickEvent;
        public event EventHandler MuteOrUnMuteOnClickEvent;
        public NotifyIconWrapper()
        {
            //if (DesignerProperties.GetIsInDesignMode(this))
            //    return;
            _notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
            _notifyIcon.DoubleClick += OpenItemOnClick;
            Application.Current.Exit += (obj, args) => { _notifyIcon.Dispose(); };

        }
        public void ShowNotification(bool isMuted)
        {

            _notifyIcon.Visible = true;
            // Shows a notification with specified message and title
            if (isMuted)
                _notifyIcon.ShowBalloonTip(3000, "Mute", "Your system is Mute and no application can't listen to you mics", ToolTipIcon.Info);
            else
                _notifyIcon.ShowBalloonTip(3000, "UnMute", "Your system is UnMute and applications in windows can listen to you mics", ToolTipIcon.Warning);


        }
        public void ChangeIcon(Icon icon)
        {
            _notifyIcon.Icon = icon;
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public NotifyRequestRecord NotifyRequest
        {
            get => (NotifyRequestRecord)GetValue(NotifyRequestProperty);
            set => SetValue(NotifyRequestProperty, value);
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }

        public event RoutedEventHandler OpenSelected
        {
            add => AddHandler(OpenSelectedEvent, value);
            remove => RemoveHandler(OpenSelectedEvent, value);
        }

        public event RoutedEventHandler ExitSelected
        {
            add => AddHandler(ExitSelectedEvent, value);
            remove => RemoveHandler(ExitSelectedEvent, value);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += OpenItemOnClick;

            var muteOrUnMuteItem = new ToolStripMenuItem("Mute/UmMute");
            muteOrUnMuteItem.Click += MuteOrUnMuteItemOnClick;

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += ExitItemOnClick;
            var contextMenu = new ContextMenuStrip { Items = { openItem, muteOrUnMuteItem, exitItem } };
            return contextMenu;
        }

        public void UpdateMuteOrUnMuteText(bool isMute)
        {
            _notifyIcon.ContextMenuStrip.Items[1].Text = !isMute ? "Mute" : "Unmute";
        }

        private void OpenItemOnClick(object sender, EventArgs eventArgs)
        {
            OpenItemOnClickEvent?.Invoke(sender, eventArgs);
        }
        private void MuteOrUnMuteItemOnClick(object? sender, EventArgs eventArgs)
        {
            MuteOrUnMuteOnClickEvent?.Invoke(sender, eventArgs);
        }
        private void ExitItemOnClick(object sender, EventArgs eventArgs)
        {
            ExitItemOnClickEvent?.Invoke(sender, eventArgs);
        }

        public class NotifyRequestRecord
        {
            public string Title { get; set; } = "";
            public string Text { get; set; } = "";
            public int Duration { get; set; } = 1000;
            public ToolTipIcon Icon { get; set; } = ToolTipIcon.Info;
        }
    }
}
