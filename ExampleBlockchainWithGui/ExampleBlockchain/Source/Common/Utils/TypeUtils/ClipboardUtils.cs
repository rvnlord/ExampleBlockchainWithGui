using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class ClipboardUtils
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hwnd, StringBuilder text, int count);

        private static string GetOpenClipboardWindowText()
        {
            var hwnd = GetOpenClipboardWindow();
            var sb = new StringBuilder(501);
            GetWindowText(hwnd.ToInt32(), sb, 500);
            return sb.ToString();
        }

        public static ActionStatus TrySetText(string text)
        {
            Exception lastEx = null;

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetDataObject(text);
                    return ActionStatus.Success();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            var sbMessage = new StringBuilder();

            sbMessage.Append(lastEx?.Message);
            sbMessage.Append(Environment.NewLine);
            sbMessage.Append(Environment.NewLine);
            sbMessage.Append("Problem:");
            sbMessage.Append(Environment.NewLine);
            sbMessage.Append(GetOpenClipboardWindowText());

            return new ActionStatus(ErrorCode.CannotSetClipboardText, sbMessage.ToString());
        }
    }
}
