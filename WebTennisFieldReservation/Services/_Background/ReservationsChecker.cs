using System.Runtime.CompilerServices;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Models.Reservations;
using WebTennisFieldReservation.Services.HttpClients;
using WebTennisFieldReservation.Services.SingleUserMailSender;
using SmtpLibrary;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Entities;
using WebTennisFieldReservation.Exceptions;

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
         * We DO NOT capture a payment if it wasn't captured.
         *    
         * Please note that in a distributed web farm were this host is replicated background services (this one included) should be run only on one instance
         * (even better they should be run on a separate process decoupled from the WebApp)
         * This can be done by checking a flag (set to true only for one instance) in appsettings.json. 
         * 
         * Another strategy is to use db locks, but liveness strategies (for failures that do not release the locks) must then be implemented.          
         * 
         */

        private readonly BackgroundReservationsCheckerSettings _settings;
        private readonly IServiceScopeFactory _serviceScopeFactory;      
        private readonly ISingleUserMailSender _mailSender;

        public ReservationsChecker(BackgroundReservationsCheckerSettings settings, IServiceScopeFactory serviceScopeFactory, ISingleUserMailSender mailSender)
        {
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
            _mailSender = mailSender;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.DelayBetweenInvocationsInSecs)))
            {
                //we use a do...while bc we want to do the first action immediately
                do
                {
                    //we need to catch to avoid the whole process to be killed
                    try
                    {                        
                        await CheckReservationsAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        //we catch anything here... TaskCancelledEx, HttpRequestEx for auth, Db exceptions, etc
                        //we try again next timer tick
                    }
                    
                }
                while(!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
            }
        }

        private async Task CheckReservationsAsync(CancellationToken cancToken)
        {
            //we don't pass the cancellation token further down, we let any ongoing request finish
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                //we need to request the following services here because they must be scoped
                ICourtComplexRepository repo = scope.ServiceProvider.GetService<ICourtComplexRepository>()!;    //the repo Dispose is taken care by the scope Dispose
                PaypalAuthenticationClient authClient = scope.ServiceProvider.GetService<PaypalAuthenticationClient>()!;
                PaypalCheckOrderClient checkOrderClient = scope.ServiceProvider.GetService<PaypalCheckOrderClient>()!;

                //we try to get an auth token (any exception will be thrown and catched upstream)
                string authToken = await authClient.GetAuthTokenAsync();

                //we get all the Fulfilled reservations that were last updated less than ElapsedTimeNeededForCheckInSecs secs ago
                TimeSpan backwardTimespan = -TimeSpan.FromSeconds(_settings.ElapsedTimeNeededForCheckInSecs);   //there's a minus at the beginning               
                List<ReservationCheckerModel> fulfilledReservations = await repo.GetExpiredFulfilledReservationsDataAsync(DateTimeOffset.Now.Add(backwardTimespan));                

                for (int i=0; i<fulfilledReservations.Count && !cancToken.IsCancellationRequested; i++)  //we also check for a cancellation request
                {
                    ReservationCheckerModel resData = fulfilledReservations[i];

                    //we need to catch for every request in order to process the remaining ones
                    try
                    {
                        string orderStatus = await checkOrderClient.CheckOrderAsync(authToken, resData.PaymentToken);
                        
                        if(orderStatus == "COMPLETED")
                        {
                            //we need to send a confirmation mail and update the db
                            string mailSubject = "Reservation confirmed";
                            string mailBody = $"Your reservation #{resData.ReservationId} was confirmed !";

                            try
                            {
                                await _mailSender.SendEmailAsync(resData.EmailAddress, mailSubject, mailBody);
                            }
                            catch (Exception ex)
                            {
                                //log the failure
                            }

                            //we then update the database
                            int updatesDone = await repo.UpdateReservationToConfirmedAsync(resData.ReservationId);

                            //profilactic check
                            if (updatesDone != 1)
                            {
                                //for debug purposes, should log here
                            }
                        }
                        else
                        {
                            //we update to Aborted
                            int updates = await repo.UpdateReservationToAbortedAsync(resData.ReservationId);    //should return 1

                            //profilactic check
                            if (updates != 1) 
                            { 
                                //for debug purposes, should log here
                            }
                        }
                    }
                    catch (PaypalCheckOrderException ex)
                    {
                        //we shouldn't have a 404 "Order not found" for a Fulfilled reservation in our db but we clean it anyway
                        await repo.UpdateReservationToAbortedAsync(resData.ReservationId);
                    }
                    catch (Exception ex)
                    {                        
                        //other errors will be catched as well but we don't abort the reservation
                    }                                                           
                }
            }
        }
    }
}
