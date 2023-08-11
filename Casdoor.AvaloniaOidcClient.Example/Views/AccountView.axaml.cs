using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Web;
using System;

namespace Casdoor.AvaloniaOidcClient.Example.Views;

public partial class AccountView : UserControl
{
    private readonly CasdoorApi _casdoorApi;
    private string? _authCode;
    public AccountView()
    {
        InitializeComponent();
        Loaded += AccountView_Loaded;
        _casdoorApi = new CasdoorApi(CasdoorVariables.Domain);
        this.CodeReceived += LoginWindow_CodeReceived;
    }


    private async void LoginWindow_CodeReceived(object? sender, CodeReceivedEventArgs e)
    {
        _authCode = e.Code;

        var token = await _casdoorApi.RequestToken(
            CasdoorVariables.ClientId,
            CasdoorVariables.ClientSecret,
            _authCode
        );

        // Assume request token and get user process is in happy path..
        var user = await _casdoorApi.GetUserInfo(token!);

        UsernameLabel.Content = user.Name;
        EmailLabel.Content = user.Email;

        WebPanel.IsVisible = false;
        StartPanel.IsVisible = false;
        AccountPanel.IsVisible = true;
    }

    private string GetLoginUrl()
          => $"{CasdoorVariables.Domain}/login/oauth/authorize?client_id={CasdoorVariables.ClientId}&response_type=code&redirect_uri={CasdoorVariables.CallbackUrl}&scope=profile,email&state={CasdoorVariables.AppName}&noRedirect=true";
    private void LoginBtn_Click(object sender, RoutedEventArgs e)
    {
        WebPanel.IsVisible = true;
        AccountPanel.IsVisible = false;
        StartPanel.IsVisible = false;
        PART_WebView.Url = new Uri(GetLoginUrl());
    }

    private void LogoutBtn_Click(object sender, RoutedEventArgs e)
    {
        PART_WebView.Url = new Uri($"{CasdoorVariables.Domain}/api/logout?client_id={CasdoorVariables.ClientId}&returnTo={CasdoorVariables.CallbackUrl}");
        WebPanel.IsVisible = false;
        AccountPanel.IsVisible = false;
        StartPanel.IsVisible = true;
    }


    private void AccountView_Loaded(object? sender, RoutedEventArgs e)
    {
        PART_WebView.WebViewNewWindowRequested += PART_WebView_WebViewNewWindowRequested;
    }

    private void PART_WebView_WebViewNewWindowRequested(object? sender, WebViewCore.Events.WebViewNewWindowEventArgs e)
    {
        string GetCodeFromUrl(string url) => HttpUtility.ParseQueryString(new Uri(url).Query).Get("code")!;

        if (e.Url.AbsoluteUri.StartsWith("casdoor://", StringComparison.OrdinalIgnoreCase))
        {
            var code = GetCodeFromUrl(e.Url.AbsoluteUri);
            CodeReceived?.Invoke(this, new CodeReceivedEventArgs(code));
        }
    }

    public event EventHandler<CodeReceivedEventArgs>? CodeReceived;
}

