// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ABI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Wat2Wasm : Page
    {
        private readonly Brush errorBrush;
        private readonly Brush successBrush;

        private readonly List<string> commands = new List<string>
        {
            "--help",
            "--version",
            "--verbose",
            "--output=FILENAME",
            "--fold-exprs",
            "--enable-exceptions",
            "--disable-mutable-globals",
            "--disable-saturating-float-to-int",
            "--disable-sign-extension",
            "--disable-simd",
            "--enable-threads",
            "--enable-function-references",
            "--disable-multi-value",
            "--enable-tail-call",
            "--disable-bulk-memory",
            "--disable-reference-types",
            "--enable-annotations",
            "--enable-code-metadata",
            "--enable-gc",
            "--enable-memory64",
            "--enable-multi-memory",
            "--enable-extended-const",
            "--enable-relaxed-simd",
            "--enable-all",
            "--inline-exports",
            "--inline-imports",
            "--no-debug-names",
            "--ignore-custom-section-errors",
            "--generate-names",
            "--no-check",
        };

        public Wat2Wasm()
        {
            this.InitializeComponent();

            errorBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 50, 50));
            successBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 200, 50));
        }


        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var args = ArgListView.Items.Cast<ArgView>().Select(t => t.Text).ToList();
            args.Insert(0, Settings.InputPath);
            RunWat2Wasm(args.ToArray());
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            RunWat2Wasm("--help");
        }

        private void AddError(string msg) => AddMessage(msg, errorBrush, HorizontalAlignment.Left);
        private void AddOutput(string msg) => AddMessage(msg, successBrush, HorizontalAlignment.Right);


        private void AddMessage(string msg, Brush color, HorizontalAlignment alignment)
        {
            InvertedListView.Items.Add(new Message()
            {
                BgColor = color,
                MsgText = msg,
                MsgDateTime = DateTime.Now,
                MsgAlignment = alignment
            });
        }

        private void RunWat2Wasm(params string[] args)
        {
            var wabtFolder = Settings.WabtDirectory;

            if (string.IsNullOrEmpty(wabtFolder))
                return;

            var argBuilder = new StringBuilder($"-Command \"cd {wabtFolder}; .\\wat2wasm.exe ");
            foreach (var arg in args)
            {
                argBuilder.Append($"{arg} ");
            }
            argBuilder.Append(";\"");

            var proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = argBuilder.ToString(),

                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string errorOutput = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            var errors = errorOutput.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var outputs = output.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var error in errors)
            {
                AddError(error);
            }

            foreach (var outp in outputs)
            {
                AddOutput(outp);
            }
        }

        private void InputBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                var splitText = sender.Text.ToLower().Split(" ");
                foreach (var cmd in commands)
                {
                    var found = splitText.All((key) =>
                    {
                        return cmd.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(cmd);
                    }
                }
                if (suitableItems.Count == 0)
                {
                    suitableItems.Add("No results found");
                }
                sender.ItemsSource = suitableItems;
            }

        }

        private void InputBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ArgListView.Items.Add(new ArgView()
            {
                Text = args.QueryText
            });

            sender.ItemsSource = null;
            sender.Text = "";
        }

        private void DeleteArgButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var arg = button.DataContext as ArgView;
            ArgListView.Items.Remove(arg);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            InvertedListView.Items.Clear();
        }
    }
}
