using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogFilter.Model
{
    internal class LogSystem
    {
        internal ObservableCollection<string> items = new ObservableCollection<string>();
        internal FileInfo savePath = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\File");
        internal int countMax = 400;

        internal bool running = true;
        internal bool saving = false;

        private readonly SemaphoreSlim _itemsSemaphoreSlim = new SemaphoreSlim(1, 1);

        internal async Task<bool> Save()
        {
            if (!running)
                return false;

            bool result = true;
            saving = true;

            await _itemsSemaphoreSlim.WaitAsync();
            try
            {
                ObservableCollection<string> tempItems = new ObservableCollection<string>(items);
                FileInfo tempSavePath = new FileInfo(savePath.FullName);

                using (var fileStream = tempSavePath.OpenWrite())
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    await streamWriter.WriteAsync("");
                    foreach (var item in tempItems)
                    {
                        await streamWriter.WriteLineAsync(item);
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                _itemsSemaphoreSlim.Release();
                saving = false;
            }

            return result;
        }

        internal async Task Clear()
        {
            await _itemsSemaphoreSlim.WaitAsync();
            try
            {
                items.Clear();
            }
            finally
            {
                _itemsSemaphoreSlim.Release();
            }
        }

        internal async Task Add(string str)
        {
            if (!running)
                return;

            await _itemsSemaphoreSlim.WaitAsync();
            try
            {
                items.Add(str);
                while (countMax <= items.Count)
                {
                    if (items.Count == 0)
                        break;

                    items.RemoveAt(0);
                }
            }
            finally
            {
                _itemsSemaphoreSlim.Release();
            }
        }


    }
}