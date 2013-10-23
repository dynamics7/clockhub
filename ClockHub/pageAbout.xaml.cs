using Microsoft.Phone.Controls;
using ClockHub;
using System.Collections.Generic;
using System;
using System.Windows;

namespace ClockHub
{
    public partial class pageAbout : PhoneApplicationPage
    {
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

        public pageAbout()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                InteropSvc.InteropLib.Instance.TerminateProcess7("LiveTokens.exe");
                InteropSvc.InteropLib.Instance.TerminateProcess7("ClockHubNative.exe");
                uint handle;
                InteropSvc.InteropLib.Instance.CreateProcess("\\Windows\\ClockHubNative.exe", "-unpin", "standard", out handle);
                if (handle != 0xFFFFFFFFU)
                {
                    InteropSvc.InteropLib.Instance.WaitForSingleObject7(handle, 10000);
                    InteropSvc.InteropLib.Instance.CloseHandle7(handle);
                }
                
                var files = EnumerateFiles("\\Applications\\Install\\f64d312e-52ce-4718-885e-997b00927fc8\\Install\\WindowsFolder");
                foreach (var file in files)
                {
                    InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\" + file.FileName);
                }
                InteropSvc.InteropLib.Instance.DeleteRegVal(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "init", "Launch163");
            }
            catch (Exception ex)
            {
                error = true;
            }
            if (error)
            {
                MessageBox.Show(LocalizedResources.UninstalledIncorrectly, LocalizedResources.Error, MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show(LocalizedResources.UninstalledSuccessfully, LocalizedResources.Done, MessageBoxButton.OK);
            }
            throw new Exception("Quit");
        }
    }
}