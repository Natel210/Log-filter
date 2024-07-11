using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LogFilter.Model
{
    internal class FileCopySystem
    {
        private readonly SynchronizationContext _syncContext;
        internal FileCopySystem()
        {
            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        internal DirectoryInfo srcDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        internal DirectoryInfo destDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        internal ObservableCollection<string> srcExts = new ObservableCollection<string>() { "txt", "log", "logFilter" };
        internal string curSrcExt = "txt";
        internal ObservableCollection<string> srcFiles = new ObservableCollection<string>();
        internal int cuttingMax = 40;
        internal string cuttingString = ".....";
        internal string regularString = "";
        internal bool isLineNumber = false;
        internal async Task<string> FindSrcFiles()
        {
            try
            {
                // 예외 처리: 디렉토리가 존재하지 않는 경우
                if (!Directory.Exists(srcDir.FullName))
                    throw new DirectoryNotFoundException($"The directory '{srcDir.FullName}' does not exist.");
                return await Task.Run(() =>
                {
                    // 특정 디렉토리의 파일을 검색하고 특정 확장자의 파일만 필터링
                    var files = Directory.EnumerateFiles(srcDir.FullName, $"*.{curSrcExt}", SearchOption.TopDirectoryOnly).ToList();
                    _syncContext.Post(_ =>
                    {
                        srcFiles.Clear();
                        foreach (var file in files)
                        {
                            srcFiles.Add(Path.GetFileName(file));
                        }
                    }, null);
                    return $"{files.Count}의 파일 찾음";
                });
            }
            catch (UnauthorizedAccessException ex) {
                throw new UnauthorizedAccessException($"하나 이상의 파일에 액세스할 수 있는 권한이 없습니다.",ex); }
            catch (Exception ex) { throw new Exception($"파일을 가져오는 동안 오류가 발생했습니다.",ex); }
        }

        internal async Task<List<string>> FileCopyWithFiltering()
        {
            List<string> logs = new List<string>();
            var tasks = new List<Task>();

            try
            {
                int fileIndex = 0;
                var srcFilesCopy = srcFiles.ToList();
                foreach (var item in srcFilesCopy)
                {
                    int currentIndex = Interlocked.Increment(ref fileIndex) - 1;
                    tasks.Add(Task.Run(async () =>
                    {
                        FileInfo srcFile = new FileInfo($"{srcDir.FullName}\\{item}");
                        FileInfo destFile = new FileInfo($"{destDir.FullName}\\{Path.GetFileNameWithoutExtension(item)}.logFilter");

                        var logMessage = new StringBuilder();
                        logMessage.AppendLine($"작업 인덱스 \t: {currentIndex}");
                        logMessage.AppendLine($"원본 파일 이름 \t: {srcFile.Name}");
                        logMessage.AppendLine($"저장 파일 이름 \t: {destFile.Name}");

                        if (destFile.Exists)
                            destFile.Delete();

                        using (var srcFileStream = srcFile.OpenRead())
                        using (var destFileStream = destFile.OpenWrite())
                        using (var srcStreamReader = new StreamReader(srcFileStream, Encoding.UTF8))
                        using (var destStreamWriter = new StreamWriter(destFileStream, Encoding.UTF8))
                        {
                            string line;
                            int lineNumber = 0;
                            destStreamWriter.Write("");
                            while ((line = await srcStreamReader.ReadLineAsync()) != null)
                            {
                                ++lineNumber;
                                if (string.IsNullOrEmpty(regularString) || Regex.IsMatch(line, regularString)) // 무형식의 경우에는 ALL
                                {
                                    string writeLine;
                                    if (isLineNumber is true)
                                        writeLine = $"L{lineNumber:D8}\t{line}";
                                    else
                                        writeLine = $"{line}";
                                    await destStreamWriter.WriteLineAsync(writeLine);
                                }
                            }
                        }
                        logs.Add(logMessage.ToString());
                    }));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) { throw new Exception("파일을 가져오는 동안 오류가 발생했습니다.",ex); }
            return logs;
        }

        internal string CutFrontDirectoryPath(DirectoryInfo directoryInfo)
        {
            string resultString = "";
            int calculateCount = cuttingMax - cuttingString.Length;
            if (directoryInfo.FullName.Length > calculateCount)
                resultString = $"{cuttingString}{directoryInfo.FullName.Substring(directoryInfo.FullName.Length - calculateCount)}";
            else
                resultString = directoryInfo.FullName;
            return resultString;
        }
    }
}
