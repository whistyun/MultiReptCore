using Avalonia.Collections;
using MultiReptCore.ViewModels;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime;

namespace MultiReptCore.Models
{
    public static class HistoryHelper
    {
        private readonly static string HistoryDirectory = Path.Combine(Program.AppData, "Histories");

        public static AvaloniaList<HistoryViewModel> Cache { get; }

        static HistoryHelper()
        {
            if (!Directory.Exists(HistoryDirectory))
                Directory.CreateDirectory(HistoryDirectory);

            var keyFiles = Directory.GetFiles(HistoryDirectory, "*.key");

            Cache = new AvaloniaList<HistoryViewModel>();

            foreach (var keyFile in keyFiles)
            {
                using (var stream = new FileStream(keyFile, FileMode.Open))
                using (var reader = new StreamReader(stream, true))
                {
                    var txt = reader.ReadToEnd();
                    var obj = JsonConvert.DeserializeObject<HistoryViewModel>(txt);
                    Cache.Add(obj);

                    obj.PropertyChanged += History_PropertyChanged;
                }
            }

        }

        public static void Register(MainWindowViewModel vm)
        {
            CheckKeyFiles();

            // make new id
            long newId = 0;
            FileStream stream = null; ;

            foreach (var retryCnt in Enumerable.Range(0, 10))
            {
                var registeredId = Directory.GetFiles(HistoryDirectory, "*.key")
                         .Select(p => Path.GetFileNameWithoutExtension(p))
                         .Where(p => Regex.IsMatch(p, @"^\d+$"))
                         .Select(p => Int64.Parse(p))
                         .Concat(new[] { 0L })
                         .Max();

                newId = registeredId + 1;
                var newKeyFile = Path.Combine(HistoryDirectory, newId + ".key");
                try
                {
                    stream = new FileStream(newKeyFile, FileMode.CreateNew);
                    break;
                }
                catch (IOException)
                {
                    Task.Delay(800);
                }
            }

            if (stream is null) throw new IOException();

            try
            {
                var history = new HistoryViewModel(
                    newId,
                    vm.RootDirectory,
                    DateTime.Now,
                    vm.RootDirectory,
                    vm.FilenameFilter,
                    vm.SelectedEncode.Name,
                    vm.IgnoreHiddenFile,
                    vm.IsEncodeAutoDetect,
                    vm.Keywords
                        .Select(keyword => new HistoryKeywordViewModel(keyword.Keyword, keyword.ReplaceWord, keyword.FindMethod))
                        .ToList()
                    );

                history.PropertyChanged += History_PropertyChanged;

                using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                {
                    var json = JsonConvert.SerializeObject(history, Formatting.Indented);
                    writer.Write(json);
                }

                Cache.Add(history);
            }
            finally
            {
                stream.Close();
            }

            Task.Run(() => Compact());
        }

        public static void Remove(HistoryViewModel vm) => Remove(vm.Id);

        public static void Remove(long id)
        {
            var file = Path.Combine(HistoryDirectory, id + ".key");
            var dir = Path.Combine(HistoryDirectory, id.ToString());

            var keyDeleted = false;
            foreach (var retryCnt in Enumerable.Range(0, 5))
            {
                try
                {
                    File.Delete(file);
                    keyDeleted = true;
                    break;
                }
                catch (IOException) { }
            }

            if (keyDeleted)
            {
                DeleteDir(dir);
            }

            CheckKeyFiles();
        }


        private static void History_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CheckKeyFiles();

            if (sender is HistoryViewModel vm)
            {
                var keyFile = Path.Combine(HistoryDirectory, vm.Id + ".key");
                if (!File.Exists(keyFile)) return;

                try
                {
                    using (var stream = new FileStream(keyFile, FileMode.Create))
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                    {
                        var json = JsonConvert.SerializeObject(vm, Formatting.Indented);
                        writer.Write(json);
                    }
                }
                catch (IOException) {/* giveup */}
            }
        }

        private static void CheckKeyFiles()
        {
            for (var i = Cache.Count - 1; i >= 0; --i)
            {
                var vm = Cache[i];
                var keyFile = Path.Combine(HistoryDirectory, vm.Id + ".key");
                if (!File.Exists(keyFile))
                {
                    Cache.RemoveAt(i);
                }
            }
        }

        private static void Compact()
        {
            foreach (var delKey in Directory.GetFiles(HistoryDirectory, "*.key")
                         .Select(p => Path.GetFileNameWithoutExtension(p))
                         .Where(p => Regex.IsMatch(p, @"^\d+$"))
                         .Select(p => Int64.Parse(p))
                         .OrderByDescending(p => p)
                         .Skip(Program.Config.HistoryBackupCount))
            {
                Remove(delKey);
            }
        }

        private static void DeleteDir(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;

            foreach (var file in Directory.GetFiles(dirPath))
            {
                var attr = File.GetAttributes(file);
                File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(dirPath))
            {
                DeleteDir(dirPath);
            }

            Directory.Delete(dirPath);
        }
    }
}
