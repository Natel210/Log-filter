using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;

namespace LogFilter
{
    public partial class ViewModel : AViewModelBase
    {
        private readonly SynchronizationContext _syncContext;
        public ViewModel(SynchronizationContext syncContext = null)
        {
            _syncContext = syncContext ?? SynchronizationContext.Current;
            InitCommand();
        }

        private void InitCommand()
        {
            FileCopyInitCommand();
            RunWorkCommand = new SimpleCommand(RunWork);
            LogInitCommand();
        }
        public ICommand RunWorkCommand { get; private set; }
        private async void RunWork()
        {
            Stopwatch stopwatch = new Stopwatch();
            LogAdd("========================================");
            LogAdd($"필터링 작업 시작");
            LogAdd($"시작시간 : {DateTime.Now.ToString("yy.MM.dd HH:mm:ss")}");
            stopwatch.Start();
            string prevDataString = "";
            prevDataString += "사전 정보\n";
            prevDataString += $"필터 값 : {FileCopyRegularString}\n";
            prevDataString += $"원본 디렉토리 : {_fileCopySystem.CutFrontDirectoryPath(FileCopySrcDir)}\n";
            prevDataString += $"저장 디렉토리 : {_fileCopySystem.CutFrontDirectoryPath(FileCopyDestDir)}";
            LogAdd(prevDataString);
            LogAdd("소스 파일을 탐색합니다.");
            FileCopyFindSrcFiles();// 파일 재탐색
            List<string> Logs = new List<string>();
            Logs = await _fileCopySystem.FileCopyWithFiltering();
            foreach (var item in Logs)
            {
                LogAdd(item);
            }
            stopwatch.Stop();
            LogAdd($"작업 완료 : {stopwatch.Elapsed.Days}day {stopwatch.Elapsed.Hours:D2}:{stopwatch.Elapsed.Minutes:D2}:{stopwatch.Elapsed.Seconds:D2}.{stopwatch.Elapsed.Milliseconds:D3}");
            LogAdd("========================================");
        }
    }

    /// <summary>
    /// 삽입된 모델들의 데이터
    /// </summary>
    public partial class ViewModel : AViewModelBase
    {
        #region FileCopySystem
        private Model.FileCopySystem _fileCopySystem = new Model.FileCopySystem();
        public DirectoryInfo FileCopySrcDir { get { return _fileCopySystem.srcDir; } set { Set(ref _fileCopySystem.srcDir, value, nameof(FileCopySrcDir)); } }
        public DirectoryInfo FileCopyDestDir { get { return _fileCopySystem.destDir; } set { Set(ref _fileCopySystem.destDir, value, nameof(FileCopyDestDir)); } }
        public ObservableCollection<string> FileCopySrcExtItems { get { return _fileCopySystem.srcExts; } set { Set(ref _fileCopySystem.srcExts, value, nameof(FileCopySrcExtItems)); } }
        public string FileCopyCurSrcExt { get { return _fileCopySystem.curSrcExt; } set { Set(ref _fileCopySystem.curSrcExt, value, nameof(FileCopyCurSrcExt)); } }
        public ObservableCollection<string> FileCopySrcFileItems { get { return _fileCopySystem.srcFiles; } set { Set(ref _fileCopySystem.srcFiles, value, nameof(FileCopySrcFileItems)); } }
        public int FileCopyCuttingMax { get { return _fileCopySystem.cuttingMax; } set => Set(ref _fileCopySystem.cuttingMax, value, nameof(FileCopyCuttingMax)); }
        public string FileCopyCuttingString { get { return _fileCopySystem.cuttingString; } set => Set(ref _fileCopySystem.cuttingString, value, nameof(FileCopyCuttingString)); }
        public string FileCopyRegularString { get { return _fileCopySystem.regularString; } set => Set(ref _fileCopySystem.regularString, value, nameof(FileCopyRegularString)); }
        public bool FileCopyIsLineNumber { get { return _fileCopySystem.isLineNumber; } set => Set(ref _fileCopySystem.isLineNumber, value, nameof(FileCopyIsLineNumber)); }

        public ICommand FileCopyFindSrcFilesCommand { get; private set; }

        private void FileCopyInitCommand()
        {
            FileCopyFindSrcFilesCommand = new SimpleCommand(FileCopyFindSrcFiles);
        }

        private async void FileCopyFindSrcFiles()
        {
            string log = "";
            log += "디렉토리에 파일 찾기";
            log += $"\n대상 디랙토리 : {_fileCopySystem.CutFrontDirectoryPath(FileCopySrcDir)}";
            try
            {
                log += $"\n{await _fileCopySystem.FindSrcFiles()}";
            }
            catch (Exception ex)
            {
                log += "\n찾기 실패";
                log += $"\n{ex}";
                if (ex.InnerException != null)
                {
                    log += $"\n{ex.InnerException}";
                }
            }
            finally
            {
                LogAdd(log);
            }
        }
        #endregion

        #region LogSystem
        private Model.LogSystem _logSystem = new Model.LogSystem();
        public ObservableCollection<string> LogItems { get { return _logSystem.items; } set { Set(ref _logSystem.items, value, nameof(LogItems)); } }
        public FileInfo LogSavePath { get { return _logSystem.savePath; } set => Set(ref _logSystem.savePath, value, nameof(LogSavePath)); }
        public  int LogCountMax { get { return _logSystem.countMax; } set => Set(ref _logSystem.countMax, value, nameof(LogCountMax)); }
        public bool LogRunning { get { return _logSystem.running; } set => Set(ref _logSystem.running, value, nameof(LogRunning)); }

        public ICommand LogSaveCommand { get; private set; }
        public ICommand LogClearCommand { get; private set; }

        private void LogInitCommand()
        {
            LogSaveCommand = new SimpleCommand(LogSave);
            LogClearCommand = new SimpleCommand(LogClear);
        }

        private async void LogSave() { await _logSystem.Save(); }
        private async void LogClear() { await _logSystem.Clear(); }
        private void LogAdd(string str) { _syncContext.Post(async _ =>  { await _logSystem.Add(str); }, null); }
        #endregion
    }
}
