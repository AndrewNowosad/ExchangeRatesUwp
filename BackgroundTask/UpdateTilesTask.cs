using ExchangeRates;
using Windows.ApplicationModel.Background;

namespace BackgroundTask
{
    public sealed class UpdateTilesTask : IBackgroundTask
    {
        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await Singletone.LoadSettings();
            await Singletone.LoadCourse();

            deferral.Complete();
        }
    }
}
