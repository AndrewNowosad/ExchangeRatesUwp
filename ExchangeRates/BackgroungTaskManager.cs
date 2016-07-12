using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Popups;

namespace ExchangeRates
{
    static class BackgroungTaskManager
    {
        static readonly string TaskName = "UpdateTilesTask";

        static public async Task RemoveBackgroundTaskAsync()
        {
            if (await BackgroundExecutionManager.RequestAccessAsync() == BackgroundAccessStatus.Denied)
            {
                await (new MessageDialog("Нет доступа к фоновым задачам!")).ShowAsync();
                return;
            }
            var taskList = BackgroundTaskRegistration.AllTasks;
            var task = taskList.FirstOrDefault(x => x.Value.Name == TaskName);
            if (task.Value == null) return;
            task.Value.Unregister(true);
        }

        static public async Task RegisterBackgroundTaskAsync()
        {
            if (await BackgroundExecutionManager.RequestAccessAsync() == BackgroundAccessStatus.Denied)
            {
                await (new MessageDialog("Нет доступа к фоновым задачам!")).ShowAsync();
                return;
            }
            await RemoveBackgroundTaskAsync();
            var taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = TaskName;
            taskBuilder.TaskEntryPoint = typeof(BackgroundTask.UpdateTilesTask).ToString();
            var trigger = new TimeTrigger((uint)Singletone.UpdatePeriodicity.TotalMinutes, false);
            taskBuilder.SetTrigger(trigger);
            taskBuilder.Register();
        }
    }
}
