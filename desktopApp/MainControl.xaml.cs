using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using StarChatBackend.DTOs;
using StarChatDesktopApp.Services;

namespace StarChatDesktopApp;

public partial class MainControl : UserControl
{
    private string _jwtToken;
    private List<ShortServerResponseDto> servers = new List<ShortServerResponseDto>();
    private readonly HttpClient _httpClient;
    private readonly string _currentUserId;
    private ClientWebSocket _webSocket;
    private Button _selectedServerButton;

    public ObservableCollection<ChatMessageViewModel> Messages { get; set; } =
        new ObservableCollection<ChatMessageViewModel>();

    public string _selectedServerId = "Select server";
    
    public MainControl()
    {
        InitializeComponent();
        _jwtToken = AppSettingsService.Instance.AuthToken;
        _currentUserId = JwtService.GetUserIdFromToken(_jwtToken);
        _httpClient = AppSettingsService.Instance.HttpClient;
        DataContext = this;
        LoadUserDataAsync();
    }

    private async void LoadUserDataAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                MessageBox.Show("Failed to decode user ID from token.");
                return;
            }
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
            var apiUrl = AppSettingsService.Instance.ApiUrl;
                
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl + "/users/" + _currentUserId);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userResponseDto = JsonConvert.DeserializeObject<UserResponseDto>(jsonResponse);

            await GetServerNames(userResponseDto.JoinedServerIds);

            if (userResponseDto != null)
            {
                UsernameTextBlock.Text = userResponseDto.Username;
                    
                ServerListStackPanel.Children.Clear();
                    
                foreach (var server in servers)
                {
                    Button serverButton = new Button
                    {
                        Content = server.Name,
                        Style = (Style)FindResource("ServerListButtonStyle"),
                        Tag = server.Id
                    };
                        
                    serverButton.Click += ServerButton_Click;
                            
                    ServerListStackPanel.Children.Add(serverButton);
                }
                    
                AddCreateNewServerButton();
                AddJoinNewServerButton();
            }
            else
            {
                MessageBox.Show("Failed to retrieve username.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }

    private async Task GetServerNames(List<string> serverIds)
    {
        if (!serverIds.Any())
        {
            return;
        }
        
        try
        {
            // prepare the get request url
            var query = string.Join("&", serverIds.Select(id => $"serverIds={id}"));
            var apiUrl = AppSettingsService.Instance.ApiUrl;
            var url = $"{apiUrl}/servers/ids-to-names?{query}";
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
                
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<ShortServerResponseWrapper>(jsonResponse);

            servers = wrapper.Servers;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
    
    private void AddJoinNewServerButton()
    {
        Button joinNewServerButton = new Button
        {
            Content = "Join New Server",
            Style = (Style)FindResource("ServerListButtonStyle")
        };
        joinNewServerButton.Click += JoinNewServerButton_Click;

        ServerListStackPanel.Children.Add(joinNewServerButton);
    }
    
    private void AddCreateNewServerButton()
    {
        Button createNewServerButton = new Button
        {
            Content = "Create New Server",
            Style = (Style)FindResource("ServerListButtonStyle")
        };
        createNewServerButton.Click += CreateNewServerButton_Click;

        ServerListStackPanel.Children.Add(createNewServerButton);
    }
    
    private async void JoinNewServerButton_Click(object sender, RoutedEventArgs e)
    {
        string serverId = PromptForServerId();

        if (string.IsNullOrEmpty(serverId))
        {
            return;
        }
            
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
            var apiUrl = AppSettingsService.Instance.ApiUrl;
                
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "/servers/" + serverId + "/members", null);

            if (response.IsSuccessStatusCode)
            {
                LoadUserDataAsync();
            }
            else
            {
                MessageBox.Show("Failed to join server.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }

    private string PromptForServerId()
    {
        var dialog = new InputDialog("Enter server ID:");
        if (dialog.ShowDialog() == true)
        {
            return dialog.ResponseText;
        }
        return null;
    }
    
    private async void ServerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton)
        {
            if (_selectedServerButton != null)
            {
                _selectedServerButton.Style = (Style)FindResource("ServerListButtonStyle");
            }

            clickedButton.Style = (Style)FindResource("SelectedServerListButtonStyle");
            _selectedServerButton = clickedButton;

            _selectedServerId = clickedButton.Tag as string;

            ServerIdTextBox.Text = _selectedServerId;

            if (string.IsNullOrEmpty(_selectedServerId))
            {
                MessageBox.Show("Server ID not found.");
                return;
            }

            await LoadChatMessagesAsync();
            await DisconnectWebSocketAsync();
            await ConnectToWebSocketAsync();
        }
    }

    private async Task LoadChatMessagesAsync()
    {
        try
        {
            var apiUrl = AppSettingsService.Instance.ApiUrl;
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
            var url = $"{apiUrl}/servers/{_selectedServerId}/messages?page=1&limit=50";
                    
            HttpResponseMessage response = await _httpClient.GetAsync(url);
                
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<ChatMessageWrapper>(jsonResponse);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Clear();
                
                if (wrapper.Messages != null && wrapper.Messages.Any())
                {
                    foreach (var message in wrapper.Messages)
                    {
                        var isUserMessage = message.SenderId == _currentUserId;
                        Messages.Add(new ChatMessageViewModel(message, isUserMessage));
                    }
                }
                else
                {
                    Messages.Add(new ChatMessageViewModel(new ChatMessageDto
                    {
                        Content = "No messages have been sent on this server yet.",
                        SenderId = string.Empty,
                        SendDate = ""
                    }, false));
                }
                
                if (ChatMessageListBox.Items.Count > 0)
                {
                    ChatMessageListBox.ScrollIntoView(ChatMessageListBox.Items[ChatMessageListBox.Items.Count - 1]);
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while loading messages: {ex.Message}");
        }
    }

    private void MessageInputTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (MessageInputTextBox.Text == "Enter your message here...")
        {
            MessageInputTextBox.Text = "";
            MessageInputTextBox.Foreground = Brushes.White;
        }
    }

    private void MessageInputTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(MessageInputTextBox.Text))
        {
            MessageInputTextBox.Text = "Enter your message here...";
            MessageInputTextBox.Foreground = (Brush)new BrushConverter().ConvertFrom("#D0D0D0");
        }
    }
    
    private async Task ConnectToWebSocketAsync()
    {
        try
        {
            _webSocket = new ClientWebSocket();
            
            var apiUrl = AppSettingsService.Instance.ApiUrl;
            // to cut the default https://
            apiUrl = apiUrl.Substring(8);
            var wsUri = new Uri($"wss://{apiUrl}/servers/{_selectedServerId}/connect?token={_jwtToken}");
            await _webSocket.ConnectAsync(wsUri, CancellationToken.None);
            _ = ReceiveMessagesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebSocket connection failed: {ex.Message}");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
    
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var messageContent = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var chatMessage = JsonConvert.DeserializeObject<ChatMessageDto>(messageContent);
            
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var isUserMessage = chatMessage.SenderId == _currentUserId;
                    Messages.Add(new ChatMessageViewModel(chatMessage, isUserMessage));
                    
                    if (ChatMessageListBox.Items.Count > 0)
                    {
                        ChatMessageListBox.ScrollIntoView(ChatMessageListBox.Items[ChatMessageListBox.Items.Count - 1]);
                    }
                });
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
    
    public async Task DisconnectWebSocketAsync()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User logged out", CancellationToken.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while disconnecting WebSocket: {ex.Message}");
            }
            finally
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }
    }
    
    private async void MessageInputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;

            var messageText = MessageInputTextBox.Text;

            if (!string.IsNullOrWhiteSpace(messageText) && _webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(messageText);
            
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                MessageInputTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("WebSocket is not connected.");
            }
        }
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        AppSettingsService.Instance.AuthToken = string.Empty;
        _jwtToken = String.Empty;
        await DisconnectWebSocketAsync();
        
        if (_selectedServerButton != null)
        {
            _selectedServerButton.Background = new SolidColorBrush(Colors.Gray);
        }
        
        var parent = Window.GetWindow(this) as MainWindow;
        parent.LoadLoginControl();
    }
    
    private async void CreateNewServerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog("Enter new server name:");
        if (dialog.ShowDialog() == true)
        {
            var serverName = dialog.ResponseText;

            if (string.IsNullOrEmpty(serverName))
            {
                MessageBox.Show("Server name cannot be empty.");
                return;
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
                var apiUrl = AppSettingsService.Instance.ApiUrl;

                var content = new StringContent(JsonConvert.SerializeObject(new { ServerName = serverName }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl + "/servers", content);

                if (response.IsSuccessStatusCode)
                {
                    LoadUserDataAsync();
                }
                else
                {
                    MessageBox.Show($"Failed to create server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
