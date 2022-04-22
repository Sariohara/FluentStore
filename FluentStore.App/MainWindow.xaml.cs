﻿using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PInvoke;
using HRESULT = Vanara.PInvoke.HRESULT;
using DwmApi = Vanara.PInvoke.DwmApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IRecipient<SetPageHeaderMessage>
    {
        private IntPtr m_hwnd;
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        private Stack<object> m_navStack = new();

        public MainWindow()
        {
            this.InitializeComponent();

            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SubClassing();

            LoadIcon(@"Assets\AppIcon.ico");
            UpdateTitleBarTheme();
        }

        public void Navigate(Type type)
        {
            var page = type.GetConstructor(Type.EmptyTypes).Invoke(null) as UIElement;
            Navigate(page);
        }

        public void Navigate(UIElement newContent)
        {
            var oldContent = WindowContent.Content;
            m_navStack.Push(oldContent);
            if (TryGetAppContent(out var oldAppContent))
                oldAppContent.OnNavigatedFrom();

            WindowContent.Content = newContent;
            if (TryGetAppContent(out var newAppContent))
                newAppContent.OnNavigatedTo();
        }

        public bool NavigateBack()
        {
            if (m_navStack.TryPop(out var oldContent))
            {
                if (TryGetAppContent(out var newAppContent))
                    newAppContent.OnNavigatedFrom();

                WindowContent.Content = oldContent;
                if (TryGetAppContent(out var oldAppContent))
                    oldAppContent.OnNavigatedTo();
                return true;
            }
            return false;
        }

        public bool TryGetAppContent(out IAppContent content)
        {
            content = WindowContent.Content as IAppContent;
            return content != null;
        }

        public void ClearNavigationStack() => m_navStack.Clear();

        [DllImport("user32")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        enum BOOL
        {
            FALSE = 0,
            TRUE = 1
        };

        private delegate IntPtr WinProc(IntPtr hWnd, User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        private void SubClassing()
        {
            newWndProc = new WinProc(NewWindowProc);
            Kernel32.SetLastError(0);
            oldWndProc = SetWindowLongPtr(m_hwnd, User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }

        private void LoadIcon(string iconName)
        {
            IntPtr hIcon = User32.LoadImage(IntPtr.Zero, iconName,
                User32.ImageType.IMAGE_ICON, 16, 16, User32.LoadImageFlags.LR_LOADFROMFILE);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            User32.SendMessage(m_hwnd, User32.WindowMessage.WM_SETICON, IntPtr.Zero, hIcon);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }

        private unsafe IntPtr NewWindowProc(IntPtr hWnd, User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case User32.WindowMessage.WM_SETTINGCHANGE:
                case User32.WindowMessage.WM_THEMECHANGED:
                    HRESULT hr = UpdateTitleBarTheme();
                    hr.ThrowIfFailed();
                    break;

                case User32.WindowMessage.WM_GETMINMAXINFO:
                    var dpi = User32.GetDpiForWindow(hWnd);
                    float scalingFactor = (float)dpi / 96;

                    //MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    //minMaxInfo.ptMinTrackSize.x = (int)(MinWidth * scalingFactor);
                    //minMaxInfo.ptMinTrackSize.y = (int)(MinHeight * scalingFactor); 
                    //Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;
            }
            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        private unsafe HRESULT UpdateTitleBarTheme()
        {
            bool isDark;
            if (Content is FrameworkElement element)
                isDark = element.ActualTheme == ElementTheme.Dark;
            else
                isDark = App.Current.RequestedTheme == ApplicationTheme.Dark;

            BOOL winIsDark = isDark ? BOOL.TRUE : BOOL.FALSE;
            return DwmApi.DwmSetWindowAttribute(m_hwnd, (DwmApi.DWMWINDOWATTRIBUTE)20, new(&winIsDark), sizeof(BOOL));
        }

        void IRecipient<SetPageHeaderMessage>.Receive(SetPageHeaderMessage m)
        {
            Title = App.AppName;
            if (TryGetAppContent(out var content) && content.IsCompact)
                Title += " - " + m.Value;
        }
    }
}
