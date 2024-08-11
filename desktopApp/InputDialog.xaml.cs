using System.Windows;

namespace StarChatDesktopApp;

public partial class InputDialog : Window
{
    public string ResponseText { get; private set; }

    public InputDialog(string question)
    {
        InitializeComponent();
        PromptTextBlock.Text = question;
        Title = question;
        ServerIdTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ResponseText = ServerIdTextBox.Text;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
