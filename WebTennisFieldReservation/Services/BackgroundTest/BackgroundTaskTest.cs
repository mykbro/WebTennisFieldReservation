using System.Diagnostics;

namespace WebTennisFieldReservation.Services.BackgroundTest
{
    public class BackgroundTaskTest : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Timer t = new Timer(_ => Console.WriteLine(DateTime.Now.TimeOfDay), null, 0, 1000);
            //return Task.CompletedTask;

            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1)))
            {
                bool done = false;

                while (!done)
                {
                    try
                    {
                        await timer.WaitForNextTickAsync(stoppingToken);
                        Console.WriteLine(DateTime.Now.TimeOfDay);
                    }
                    catch(Exception ex)                   
                    {
                        done = true;
                    }
                }
            }
        }
    }
}
