using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using StarChatDesktopApp.Services;

namespace StarChatDesktopApp;

public partial class LoginControl : UserControl
{
    public LoginControl()
    {
        InitializeComponent();
        UpdateUIForLoginMode();
    }
    
    private void SwitchToLoginMode(object sender, RoutedEventArgs e)
    {
        UpdateUIForLoginMode();
    }

    private void SwitchToRegisterMode(object sender, RoutedEventArgs e)
    {
        UpdateUIForRegisterMode();
    }

    private void UpdateUIForLoginMode()
    {
        LoginSection.Visibility = Visibility.Visible;
        RegisterSection.Visibility = Visibility.Collapsed;
        LoginModeButton.IsEnabled = false;
        RegisterModeButton.IsEnabled = true;
    }

    private void UpdateUIForRegisterMode()
    {
        LoginSection.Visibility = Visibility.Collapsed;
        RegisterSection.Visibility = Visibility.Visible;
        LoginModeButton.IsEnabled = true;
        RegisterModeButton.IsEnabled = false;
    }
    
    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string email = EmailTextBox.Text;
        string password = PasswordBox.Password;

        string apiUrl = AppSettingsService.Instance.ApiUrl + "/auth/login";

        try
        {
            var loginRequestDto = new { email, password };
            string jsonData = JsonConvert.SerializeObject(loginRequestDto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await AppSettingsService.Instance.HttpClient.PostAsync(apiUrl, content);

            string responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

            if (jsonResponse.ContainsKey("token"))
            {
                string token = jsonResponse["token"];
                AppSettingsService.Instance.AuthToken = token;
                NotifyParent();
            }
            else
            {
                string errorMessage = jsonResponse["message"];
                ShowErrorMessage(errorMessage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
    
    private async void RegisterSubmitButton_Click(object sender, RoutedEventArgs e)
    {
        string email = RegisterEmailTextBox.Text;
        string username = UsernameTextBox.Text;
        string password = RegisterPasswordBox.Password;
        string confirmPassword = ConfirmPasswordBox.Password;

        if (password != confirmPassword)
        {
            ShowRegisterErrorMessage("Passwords do not match.");
            return;
        }

        string apiUrl = AppSettingsService.Instance.ApiUrl + "/auth/register";

        try
        {
            var registerRequestDto = new { email, username, password, confirmPassword };
            string jsonData = JsonConvert.SerializeObject(registerRequestDto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await AppSettingsService.Instance.HttpClient.PostAsync(apiUrl, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateUIForLoginMode();
            }
            else
            {
                string errorMessage = jsonResponse["message"];
                ShowRegisterErrorMessage(errorMessage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
    
    private void ShowErrorMessage(string message)
    {
        ErrorMessageTextBlock.Text = message;
        ErrorMessageTextBlock.Visibility = System.Windows.Visibility.Visible;
    }
    
    private void ShowRegisterErrorMessage(string message)
    {
        RegisterErrorMessageTextBlock.Text = message;
        RegisterErrorMessageTextBlock.Visibility = Visibility.Visible;
    }
    
    private void NotifyParent()
    {
        var parent = Window.GetWindow(this) as MainWindow;
        parent?.HandleLoginSuccess();
    }
}
