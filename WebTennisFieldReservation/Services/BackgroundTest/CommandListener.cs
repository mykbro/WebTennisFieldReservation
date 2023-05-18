namespace WebTennisFieldReservation.Services.BackgroundTest
{
    public class CommandListener : BackgroundService
    {
        private BackgroundTaskTest _serviceToStop;

        public CommandListener(BackgroundTaskTest serviceToStop)
        {
            _serviceToStop = serviceToStop;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                bool done = false;

                while (!done)
                {
                    string? cmd = Console.ReadLine();

                    if (cmd!.Equals("x", StringComparison.OrdinalIgnoreCase))
                    {
                        _serviceToStop.StopAsync(stoppingToken);
                        done = true;
                    }
                }

                Console.WriteLine("DONE");
            });                
        }
    }
}
