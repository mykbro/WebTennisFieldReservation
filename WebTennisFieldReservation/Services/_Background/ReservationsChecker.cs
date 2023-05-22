using System.Runtime.CompilerServices;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Settings;

namespace WebTennisFieldReservation.Services._Background
{
    public class ReservationsChecker : BackgroundService
    {
        /*
         * This background service will take care of periodically checking every Reservation that still is in the Fulfilled status after a certain amount of time.  
         * These orphaned reservations can be caused by 2 events: 
         * 1) Something happened during the execution of the /reservations/confirm endpoint between _repo.UpdateReservationToPaymentApprovedAsync() and 
         *   _repo.UpdaUpdateReservationToConfirmedAsync(). An uncatched exception, an unexpected crash or a powerloss event.
         * 2) A timeout happened during the ConfirmPaymentCapture request. We were able to make the request but didn't receive an answer .
         * 
         * In both cases we cannot know if the payment has been correctly captured or not, so we need to query the Paypal server (hopefully receiving an answer back)
         * and based on the answer mark the reservation as Completed (after sending a confirmation email that could've already be sent) or Aborted.
         *    
         * Please note that in a distributed web farm were this host is replicated background services (this one included) should be run only on one instance
         * (even better they should be run on a separate process decoupled from the WebApp)
         * This can be done by checking a flag (set to true only for one instance) in appsettings.json. 
         * 
         * Another strategy is to use db locks but keep-alive strategies (for failures that do not release the locks) must be implemented.          
         * 
         */

        private readonly BackgroundReservationsCheckerSettings _settings;
        private readonly IServiceScopeFactory _serviceScopeFactory;        

        public ReservationsChecker(BackgroundReservationsCheckerSettings settings, IServiceScopeFactory serviceScopeFactory)
        {
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.DelayTimespanInSecs)))
            {
                //we use a do...while bc we want to do the first action immediately
                do
                {
                    await CheckReservationsAsync(stoppingToken);
                }
                while(!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
            }
        }

        private async Task CheckReservationsAsync(CancellationToken token)
        {
            using(IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                ICourtComplexRepository repo = scope.ServiceProvider.GetService<ICourtComplexRepository>()!;        //the repo Dispose is taken care by the scope Dispose

                //repo.



            }
        }
    }
}
