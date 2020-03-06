using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace XwaBackupRestorer
{
    public sealed class FileRichTextBox : RichTextBox
    {
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(FileRichTextBox), new PropertyMetadata(OnFilePathChanged));

        private static void OnFilePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is FileRichTextBox box)
            {
                box.Selection.Text = string.Empty;

                if (!string.IsNullOrEmpty(box.FilePath) && System.IO.File.Exists(box.FilePath))
                {
                    using (var file = System.IO.File.OpenRead(box.FilePath))
                    {
                        box.Selection.Load(file, DataFormats.Rtf);
                    }
                }
            }
        }
    }
}
