using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XwaBackupRestorer
{
    public sealed class BackupCraft : INotifyPropertyChanged
    {
        private static readonly Regex _craftFolderRegex = new Regex(@"\A(\d+-\d+-\d+_\d+\.\d+\.\d+)_(.+)\z", RegexOptions.CultureInvariant);

        private string folderName;

        private string craftName;

        private DateTime creationDate;

        private string readmeFileName;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string FolderName
        {
            get
            {
                return this.folderName;
            }

            set
            {
                if (value != this.folderName)
                {
                    this.folderName = value;
                    this.NotifyPropertyChanged(nameof(FolderName));
                }
            }
        }

        public string CraftName
        {
            get
            {
                return this.craftName;
            }

            set
            {
                if (value != this.craftName)
                {
                    this.craftName = value;
                    this.NotifyPropertyChanged(nameof(CraftName));
                }
            }
        }

        public DateTime CreationDate
        {
            get
            {
                return this.creationDate;
            }

            set
            {
                if (value != this.creationDate)
                {
                    this.creationDate = value;
                    this.NotifyPropertyChanged(nameof(CreationDate));
                }
            }
        }

        public string ReadmeFileName
        {
            get
            {
                return this.readmeFileName;
            }

            set
            {
                if (value != this.readmeFileName)
                {
                    this.readmeFileName = value;
                    this.NotifyPropertyChanged(nameof(this.ReadmeFileName));
                }
            }
        }

        public ObservableCollection<string> FileNames { get; } = new ObservableCollection<string>();

        public void Restore(string xwaDirectory)
        {
            foreach (string fileName in this.FileNames)
            {
                if (fileName.StartsWith("Backup\\", StringComparison.OrdinalIgnoreCase))
                {
                    string name = fileName.Substring("Backup\\".Length);
                    string backupPath = Path.Combine(xwaDirectory, "Backup", this.FolderName, name);
                    string xwaPath = Path.Combine(xwaDirectory, name);

                    Directory.CreateDirectory(Path.GetDirectoryName(xwaPath));
                    File.Copy(backupPath, xwaPath, true);
                }
                else if (fileName.StartsWith("New\\", StringComparison.OrdinalIgnoreCase))
                {
                    string name = fileName.Substring("New\\".Length);
                    string xwaPath = Path.Combine(xwaDirectory, name);

                    File.Delete(xwaPath);
                }
            }

            Directory.Delete(Path.Combine(xwaDirectory, "Backup", this.FolderName), true);

            if (!string.IsNullOrEmpty(this.ReadmeFileName))
            {
                string readmePath = Path.Combine(xwaDirectory, "Backup", this.ReadmeFileName);
                File.Delete(readmePath);
            }
        }

        public static List<BackupCraft> LoadList(string xwaDirectory, bool includeNewFiles)
        {
            var crafts = new List<BackupCraft>();

            string backupDirectory = Path.Combine(xwaDirectory, "Backup");

            if (!Directory.Exists(backupDirectory))
            {
                return crafts;
            }

            foreach (string directoryName in Directory.EnumerateDirectories(backupDirectory))
            {
                string item = System.IO.Path.GetFileName(directoryName);
                Match match = _craftFolderRegex.Match(item);

                if (!match.Success)
                {
                    continue;
                }

                var craft = new BackupCraft
                {
                    FolderName = item,
                    CraftName = match.Groups[2].Value,
                    CreationDate = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd_H.mm.ss", CultureInfo.InvariantCulture)
                };

                craft.ReadmeFileName = Path.GetFileName(Directory.EnumerateFiles(backupDirectory, string.Concat(craft.CraftName, "v*Readme.*")).FirstOrDefault());

                foreach (string fileName in Directory.EnumerateFiles(directoryName, "*.*", SearchOption.AllDirectories))
                {
                    craft.FileNames.Add(fileName.Replace(directoryName, "Backup"));
                }

                crafts.Add(craft);
            }

            crafts.Sort((x, y) => x.CreationDate.CompareTo(y.CreationDate));

            if (includeNewFiles)
            {
                DateTime now = DateTime.Now.AddMilliseconds(1);

                for (int craftIndex = 0; craftIndex < crafts.Count; craftIndex++)
                {
                    BackupCraft craft = crafts[craftIndex];

                    DateTime from = craft.CreationDate;
                    DateTime to = craftIndex + 1 < crafts.Count ? crafts[craftIndex + 1].CreationDate : now;

                    foreach (string fileName in Directory.EnumerateFiles(xwaDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        if (fileName.StartsWith(backupDirectory, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string name = Path.GetFileName(fileName);

                        if (name.StartsWith("flightscreen", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (name.StartsWith("frontscreen", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string fileNameBackup = fileName.Replace(xwaDirectory, "Backup");
                        if (craft.FileNames.Any(t => t.Equals(fileNameBackup, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        DateTime creationDateTime = File.GetCreationTime(fileName);

                        if (creationDateTime >= from && creationDateTime < to)
                        {
                            craft.FileNames.Add(fileName.Replace(xwaDirectory, "New"));
                        }
                    }
                }
            }

            return crafts;
        }
    }
}
