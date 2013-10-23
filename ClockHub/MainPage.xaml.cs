using System;
using System.Windows.Controls;
using ApplicationApi;
using Microsoft.Phone.Controls;
using System.Windows;
using ClockHub;
using System.Collections.Generic;
using System.Threading;

namespace ClockHub
{
    public partial class MainPage : PhoneApplicationPage
    {


        #region "File functions"

        private class EnumFile
        {
            public string FileName;
            public bool isFolder;
        }

        EnumFile[] EnumerateFiles(string folder)
        {
            InteropSvc.InteropLib.WIN32_FIND_DATA data;
            uint handle = InteropSvc.InteropLib.Instance.FindFirstFile7(folder + "\\*", out data);
            var list = new List<EnumFile>();

            if (handle != 0xFFFFFFFFU)
            {
                bool result = false;
                do
                {
                    if (data.cFileName != "." && data.cFileName != "..")
                    {
                        EnumFile ef = new EnumFile();
                        ef.FileName = data.cFileName;
                        bool t = ((data.dwFileAttributes & 0x10) == 0x10) ? true : false;
                        ef.isFolder = t ? true : false;
                        list.Add(ef);
                    }
                    result = InteropSvc.InteropLib.Instance.FindNextFile7(handle, out data);
                } while (result != false);
                InteropSvc.InteropLib.Instance.FindClose7(handle);
            }
            return list.ToArray();
        }

        void CopyDirectory(string src, string dest)
        {
            EnumFile[] files = EnumerateFiles(src);
            InteropSvc.InteropLib.Instance.CreateDirectory7(dest);
            if (files.Length > 0)
            {
                foreach (EnumFile ef in files)
                {
                    if (ef.isFolder == true)
                    {
                        CopyDirectory(src + "\\" + ef.FileName, dest + "\\" + ef.FileName);
                    }
                    else
                    {
                        InteropSvc.InteropLib.Instance.DeleteFile7(dest + "\\" + ef.FileName);
                        InteropSvc.InteropLib.Instance.CopyFile7(src + "\\" + ef.FileName, dest + "\\" + ef.FileName, false);
                    }
                }
            }
        }

        #endregion

        void StartThread()
        {
            InteropSvc.InteropLib.Initialize();
            if (InteropSvc.InteropLib.HasRootAccess() == false)
            {
                // double check
                System.Threading.Thread.Sleep(1000);
                if (InteropSvc.InteropLib.HasRootAccess() == false)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        MessageBox.Show(LocalizedResources.NoRootAccess, LocalizedResources.Error, MessageBoxButton.OK);
                    });
                    throw new Exception("Quit");
                }
            }
            bool installed = false;
            try
            {
                var sb = new System.Text.StringBuilder(500);
                InteropSvc.InteropLib.Instance.RegistryGetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "init", "Launch163", sb, 500);
                if (sb.ToString() == "LiveTokens.exe" || sb.ToString() == "ClockHubNative.exe")
                {
                    installed = true;
                }
            }
            catch (Exception ex)
            {
                installed = false;
            }
            if (!installed)
            {
                this.Dispatcher.BeginInvoke(delegate()
                    {
                        if (MessageBox.Show(LocalizedResources.NotInstalled, LocalizedResources.Title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            bool error = false;
                            try
                            {
                                CopyDirectory("\\Applications\\Install\\f64d312e-52ce-4718-885e-997b00927fc8\\Install\\WindowsFolder", "\\Windows");
                                InteropSvc.InteropLib.Instance.RegistrySetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "init", "Launch163", "ClockHubNative.exe");
                                InteropSvc.InteropLib.Instance.TerminateProcess7("LiveTokens.exe");
                                InteropSvc.InteropLib.Instance.TerminateProcess7("ClockHubNative.exe");
                                uint handle;
                                InteropSvc.InteropLib.Instance.CreateProcess("\\Windows\\ClockHubNative.exe", "-pin", "standard", out handle);
                                if (handle != 0xFFFFFFFFU)
                                {
                                    InteropSvc.InteropLib.Instance.CloseHandle7(handle);
                                }
                            }
                            catch (Exception ex)
                            {
                                error = true;
                            }
                            if (error)
                            {
                                MessageBox.Show(LocalizedResources.InstalledIncorrectly, LocalizedResources.Error, MessageBoxButton.OK);
                            }
                            throw new Exception("Quit");
                        }
                        else
                        {
                            throw new Exception("Quit");
                        }
                    });
            }
        }
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            var thread = new Thread(StartThread);
            thread.Start();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = lstItems.SelectedIndex;
            if (i == 0)
            {
                Functions.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA560A/Default");
            }
            else if (i == 1)
            {
                Functions.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA5606/_default");
            }
            else if (i == 2)
            {
                Functions.LaunchSession("app://5B04B775-356B-4AA0-AAF8-6491FFEA5612/Default");
            }
            else if (i == 3)
            {
                NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
                return;
            }
            throw new Exception("Quit");
        }

    }
}