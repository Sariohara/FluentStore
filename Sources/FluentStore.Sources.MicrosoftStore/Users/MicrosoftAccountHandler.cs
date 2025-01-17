﻿using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Flurl;
using Microsoft.Graph;
using Microsoft.Marketplace.Storefront.Contracts;
using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccountHandler : AccountHandlerBase<MicrosoftAccount>
    {
        public const string APPIDURI_MSGRAPH = "https://graph.microsoft.com/";
        public const string BASEURL_MSGRAPH = APPIDURI_MSGRAPH + "v1.0/";

        private readonly INavigationService _navService = Ioc.Default.GetRequiredService<INavigationService>();
        private GraphServiceClient _graphClient;
        private AccountsSettingsPane _asp;

        public override string Id => "msal";

        public override string DisplayName => "Microsoft Account";

        public MicrosoftAccountHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {

        }

        protected override async Task<Account> UpdateCurrentUser()
        {
            return null;
            //_graphClient = new(new TokenAuthProvider(Token));
            //var user = await _graphClient.Me.Request().GetAsync();

            //return new MicrosoftAccount(user);
        }

        public override AbstractForm CreateManageAccountForm()
        {
            INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
            return AbstractUIHelper.CreateOpenInBrowserForm("ManageCollection", "Manage your account on the website.",
                "https://account.microsoft.com/profile", navService);
        }

        /// <summary>
        /// Sets the token of the logged-in user.
        /// </summary>
        /// <param name="requestOptions">
        /// The <see cref="RequestOptions"/> to set authentication on.
        /// </param>
        public void AuthenticateRequest(RequestOptions requestOptions)
        {
            //requestOptions.Token = Token;
        }

        public override Task<bool> SignInAsync(CredentialBase credential)
        {
            return Task.FromResult(false);
        }

        public override Task SignOutAsync()
        {
            return Task.CompletedTask;
        }

        public override Task HandleAuthActivation(Url url)
        {
            throw new System.NotImplementedException();
        }

        public override AbstractForm CreateSignInForm()
        {
            AbstractForm form = AbstractUIHelper.CreateSingleButtonForm($"{Id}_SignIn", "Sign in with a Microsoft Account",
                "Sign in", OnSignInFormSubmitted);
            return form;
        }

        public override AbstractForm CreateSignUpForm()
        {
            throw new NotImplementedException();
        }

        private async void OnSignInFormSubmitted(object sender, EventArgs args)
        {
            var hwnd = _navService.GetMainWindowHandle();
            _asp = AccountsSettingsPaneInterop.GetForWindow(hwnd);

            _asp.AccountCommandsRequested += Asp_AccountCommandsRequested;
            await AccountsSettingsPaneInterop.ShowAddAccountForWindowAsync(hwnd);
        }

        private async void Asp_AccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "consumers");

            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);
            args.WebAccountProviderCommands.Add(command);

            deferral.Complete();
        }

        private async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            WebTokenRequest request = new(command.WebAccountProvider, "wl.basic");

            var hwnd = _navService.GetMainWindowHandle();
            WebTokenRequestResult result = await WebAuthenticationCoreManagerInterop.RequestTokenForWindowAsync(hwnd, request);

            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string token = result.ResponseData[0].Token;
            }
        }
    }
}
