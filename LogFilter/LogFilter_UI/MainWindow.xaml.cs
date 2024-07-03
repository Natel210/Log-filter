using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Windows;

namespace LogFilter_UI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new LogFilter.ViewModel(SynchronizationContext.Current);
            DataContext = viewModel;
        }

        private void SelectSourcePath(object sender, RoutedEventArgs e)
        {
            LogFilter.ViewModel dataContext = null;
            if (CasttingDataContext(ref dataContext, true) is false)
                return;
            dataContext.FileCopySrcDir = SelectedFolder(dataContext.FileCopySrcDir);
        }

        private void SelectDestinationPath(object sender, RoutedEventArgs e)
        {
            LogFilter.ViewModel dataContext = null;
            if (CasttingDataContext(ref dataContext, true) is false)
                return;
            dataContext.FileCopyDestDir = SelectedFolder(dataContext.FileCopyDestDir);
        }

        private void SelectSaveLogPath(object sender, RoutedEventArgs e)
        {
            LogFilter.ViewModel dataContext = null;
            if (CasttingDataContext(ref dataContext, true) is false)
                return;
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.CheckPathExists = true;
            dlg.AddExtension = true;
            dlg.Title = "로그 저장";
            dlg.InitialDirectory = dataContext.LogSavePath.Directory.FullName;
            dlg.Filter = "텍스트 파일 (*.txt)|*.txt|로그 파일 (*.log)|*.log|모든 파일 (*.*)|*.*";
            if (dlg.ShowDialog() is true)
            {
                dataContext.LogSavePath = new FileInfo(dlg.FileName);
                dataContext.LogSaveCommand.Execute(null);
            }
        }
    }

    public partial class MainWindow : Window
    {
        private bool CasttingDataContext<T>(ref T dataContext, bool popupMessage = false) where T : class
        {
            bool result = true;
            dataContext = DataContext as T;
            if (dataContext is null)
                result = false;
            if (result is false && popupMessage is true)
                MessageBox.Show("DataContext가 케스팅에 실패 했습니다.\n코드를 다시 확인 해주세요.", "버튼을 활성화 할 수 없습니다.", MessageBoxButton.OK);
            return result;
        }

        private DirectoryInfo SelectedFolder(DirectoryInfo prevDirectoryInfo)
        {
            if (prevDirectoryInfo is null)
            {
                MessageBox.Show("마지막 디렉토리의 위치가 비정상 입니다.\n코드를 다시 확인 해주세요.", "버튼을 활성화 할 수 없습니다.", MessageBoxButton.OK);
                return prevDirectoryInfo;
            }
            var dialog = new OpenFileDialog {
                ValidateNames = false, CheckFileExists = false,
                CheckPathExists = true, InitialDirectory = prevDirectoryInfo.FullName,
                FileName = "folder", Title = "폴더 선택", Multiselect = false };
            if (dialog.ShowDialog() is true)
                return new DirectoryInfo(System.IO.Path.GetDirectoryName(dialog.FileName));
            return prevDirectoryInfo;
        }
    }
}
