using System;
using System.Windows;
using StarChatDesktopApp.Services;

namespace StarChatDesktopApp
{
    public partial class MainWindow : Window
    {
        private MainControl _mainControl;
        
        public MainWindow()
        {
            InitializeComponent();
            LoadLoginControl();
        }
        
        public void LoadLoginControl()
        {
            var loginControl = new LoginControl();
            ContentArea.Content = loginControl;
        }

        public void HandleLoginSuccess()
        {
            var mainControl = new MainControl();
            _mainControl = mainControl;
            ContentArea.Content = mainControl;
        }

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_mainControl != null)
            {
                await _mainControl.DisconnectWebSocketAsync();
            }
        }
    }
}
