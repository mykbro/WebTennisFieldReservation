# WebTennisFieldReservation

## Disclaimer
This codebase is made for self-teaching and educational purposes only.
Many features like input validation, object disposed checks, some exception handling, etc... are mostly missing.
As such this codebase cannot be considered production ready.

## What's this ?
This is a Asp.Net Core MVC Web App that implements a concurrent reservation system for an hypothetical tennis court complex.

The app's main features are:
*	User registration and management
*	Court management
*	Reservation slot management
*	Template management for easier slot configuration
*	Slot reservation system
*	Paypal payment system integration

While also providing:
*	Concurrency management for 'same slot' reservations:  
	no two reservations can share a common slot
*	Resiliency against powerlosses and/or application errors or crashes:  
	a background service will check for any ongoing incomplete reservation/payment process   
*	Resiliency against Paypal server network communication problem:  
	a background service will check for any pending payment 
*	Easiness to scale out thanks to a sessionless design 
*	Usage of async methods everywhere for efficient multi-threading

This project is a case study for the whole Asp.Net Core framework, the Dependency Injection framework, the Entity Framework, Javascript, jQuery and AJAX requests, and Azure integration. 

## How does it work ?

### How does the authentication system work ?
We use a custom authentication system similar to the one used by the Identity framework (the 'Password reset' and the 'Confirm Email' link generation mechanism also use a TokenManager service that levareges the DataProtection service like Identity).  
On a SignIn we append an auth Cookie to the Response.Cookies (either persistent or session based) encrypted and authenticated using the DataProtection infrastructure.  
Passwords hashes are derived using PBKDF2 with a number of iterations configured in appsetting.json and salted with a 32 byte array.

The Claims included in this cookie are:
*	UserId (used to determine who we are and what AuthZ we have)
*	SecurityStamp (used to determine if any security related data has changed since this cookie was issued)
*	IssueTime (used to determine how recent is the last login, useful for example when we want to change security related data like passwords)
*	IsAdmin (used for special AuthZ, could have omitted this and fetched from the db on each request during authentication)
*	RememberMe (used when we want to edit our password and automatically SignIn to acquire a new auth cookie, we need a way to remember the user choice)
 
On every request we run our custom AuthHandler HandleAuthentication() method.  
In this method we check if the cookie values is present and decrypts correctly (no tampering).  
If so we fetch from the db in a single call some user related data that we always need (like the username and the email) together with the user SecurityStamp.  
The SecurityStamp is a GUID that changes every time we update some security related data for that user (like a password or higher lvl AuthZ related data).  
We then check the SecurityStamp that we fetch from the db against the one in the cookies and see if they match, otherwise we don't authenticate and delete the cookie.  
If the authentication is succesful we add the user data as new Claims to the ClaimsPrincipal.

While it would seem wasteful to query the db on every request the truth is that:
*	We fetch both user data and SecurityStamp in the same query from a single table and the values will be probably cached by the db after the first query
*	The user data (username and email) shown by the webpages is always up to date even for concurrent sessions
*	We avoid sessions for user data storage. For distributed apps a db backed session would be needed rendering the point moot (more of this later...)
*	The SecurityStamp check would've been there anyway albeit not for every request but with a certain frequency (like once every 5 mins)

To learn more about the auth system please check the AuthenticationSchemes.MyAuthScheme.MyAuthSchemeHandler source code and the Controllers.UsersController source code.

### About the sessionless design
Sessions have a lot of downsides:
* They must be deserialized/serialized on each request
* They must be kept synchronized with the db when user data changes
* They must be backed by a database for load balanced servers or rely on sticky sessions
* They use server resources
* They need session locks stifling concurrency

Hence we prefer to avoid them and use the approach described in the previous point. 

### How does the backend work ?
The app is backed by a database accessed using the EF (Entity Framework).  
The DbContext and all the EF queries, commands and transactions are encapsulated in a repository service using the 'Repository pattern'. 

This allows to: 
* segregate all the db interactions in a single class
* mock the db queries for testing purposes
* keep the Controllers clean and decoupled from the db

For more info please check the Entities folder and the Data folder.

The web pages can also interact with the server trough AJAX calls.  
The ApiController is responsible for answering these requests querying the app Repository (db wrapper).  
As these calls are SameSite calls we use the standard cookie authentication as those are passed in each XHR request.

Please check the Controllers.ApiController source code and the various Views and scripts for more info.

### How does the concurrency management work ?
Slot assignment correctness for multiple concurrent reservations is managed in the DbCourtComplexRepository.TryToFulfillReservationAsync(Guid reservationId) method.  
In this method we run a transaction using pessimistic concurrency and the default Isolation Level (ReadCommitted).  
However, we do not rely on any SELECT statement inside the transaction that would lead to the 'Deadlock detection' approach for concurrency control.  
Instead, and that's why we use the default isolation level, we rely on UPDATING a certaing amount of rows and checking the number of updates executed to determine if we can proceed in our transaction or rollback and abort.

### How does the reservation system provides integrity and consistency ? (AKA "Am I going to be billed for a reservation that wasn't saved after a crash ???")
The system provides integrity and consistency for the reservations and the payments using:
* #### A ReservationStatus field for each reservation in the database
	This acts as a write-back log for the reservation/payment process, and will help us know at which point of the process we were before the powerloss/crash.  
	A reservation can be in one of these statuses: PENDING, PAYMENT_CREATED, PAYMENT_APPROVED, FULFILLED, CONFIRMED and ABORTED.  
	More details can be found in the Reservation class in the Entities folder.  
	Please check the "What does the reservation and payment process looks like in detail ?" section for more info about how these statuses are used. 	


* #### A background hosted service that periodically checks for "hanging" reservations
	A reservation can be hanging due to a powerloss/crash or because a Paypal capture request timeouts.  
	Please check the "What does the reservation background service do ?" for more details.

### What does the reservation and payment process looks like in detail ?
The typical process that starts when a user confirm a reservation is the following:
1. The user navigates to /courtavailability/view.  
   A new CheckoutToken GUID is generated, this token will protect against duplicate requests.
1. The user clicks the 'Continue' button.  
   A POST request to /reservations/checkout is made passing the CheckoutToken.
1. The user click on 'Confirm' making a POST request to /reservations/create starting the reservation/payment process.
1. We try to place a new Reservation with Id = CheckoutToken, Status = PENDING and PaymentId = null in the db, fixing the price. This WON'T reserve the slots.  
   This is how the CheckoutToken protects against duplicate reservations from the same ckeckout.  
   We also create a new GUID and place it inside the ConfirmationToken field for the Reservation. This will be used to authenticate the callback request from Paypal. More of this later.
1. We try to create a Paypal order and get an OrderId back.  
   In the creation request JSON payload we specify a callback returnURL to be called after the order's authorization from the customer.  
   This returnURL is /reservations/confirm/\{reservationId:guid}?confirmationToken=\{confirmationToken}.  
   If the creation succeeded we update the reservation PaymentId and its status to PAYMENT_CREATED.
1. We redirect the user to the Paypal order authorization page using the PaymentId.
1. After the order's authorization, Paypal will redirect the user to the returnUrl we provided appending the PaymentId (Token) and the PayerId to it as additional query parameters.  
   This will call our /reservations/confirm/\{reservationId} endpoint.
1. If the ConfirmationToken and the PaymentId check out for the provided ReservationId we update the reservation status to PAYMENT_APPROVED, but only if it previously was set to PAYMENT_CONFIRMED.  
   This will act as a gateway against replays and/or concurrent calls for the same /reservations/confirm/\{reservationId} endpoint.
1. We try to fulfill the order checking if the requested slots are still available and not have been taken meanwhile.  
   If so we run a transaction where we atomically set the slots's availability to 'false' and update the reservation status to FULFILLED.
1. We finally try to capture the payment using the PaypalCapturePaymentClient service.  
   Here we can have 3 outcomes:
	* we get an ORDER_CONFIRMED response:  
	we try to send a confirmation email and then (even after a sending failure) we update the reservation status to CONFIRMED.
	* we get a response which is not ORDER_CONFIRMED or we aren't able to connect to Paypal servers:  
	we rollback the order fulfillment, reverting the slots' status back to 'available' and the reservation status to ABORTED.
	* we send the capture request but we don't get an answer back (timeout):  
    this is the worst outcome as we can't know if the payment capture succeeded.  
    We leave the reservation in the FULFILLED status and let the background hosted service deal with it.  
    Please check the "What does the reservation background service do ?" for more details.

For more info please check the ReservationsController source code in the Controllers folder.

### What does the reservation background service do ?
For every FULFILLED reservations where a certain amount of time has passed after its last status update, the service will periodically try to communicate with the Paypal servers and query them for the reservation's payment status.

This must be done for two reasons:
* a powerloss/crash could happen during the /reservations/confirm endpoint execution leaving a reservation in the FULFILLED state
* a timeout can occur when trying to capture a Paypal payment

If the order was CONFIRMED it tries to send a confirmation email and update the reservation status also to CONFIRMED. Otherwise it frees the reservation's slots and update the reservation status to ABORTED.  
If the server is still unreacheable or the request timeouts we'll retry the same thing on the next service invocation over and over again.  
In the worst case scenario where the server was never reacheable again and the payment was never captured in reality, we lose the payment and the customer get its reservation for free.
		
Please note that we care only for reservation in the FULFILLED status as a payment may be involved.  
If a powerloss/crash occurs and a reservation is left in any other state or if a user never completes the process approving the payment, we just don't care and we'll leave the reservation as it is as no slots would be reserved.

For more info please check the ReservationsChecker class in the Services._Background folder.	

### How does the Paypal integration work ?
The app uses many services that wraps internal injected Http clients to talk with the Paypal API system.  
The services can be found in the Services.HttpClients folder and are configured in Program.cs


	// Add HTTP clients
	builder.Services.AddSingleton<PaypalApiSettings>(paypalApiSettings);    //we need to inject this in the Paypal clients			
    builder.Services.AddHttpClient<PaypalAuthenticationClient>();
    builder.Services.AddHttpClient<PaypalCreateOrderClient>();
    builder.Services.AddHttpClient<PaypalCapturePaymentClient>();
    builder.Services.AddHttpClient<PaypalCheckOrderClient>();

These services are Scoped and leverage the IHttpClientFactory infrastructure to prevent port exhaustion and reuse HttpHandlers.

As recommended, when we create a new order we pass the ReservationId as an idempotency token. This will prevent the creation of multiple payments for the same reservation.

Every operation, like "Create an order", "Capture a payment" or "Check order" requires a Bearer auth token that can be requested by the PaypalAuthenticationClient.  
In this app an auth token is requested before each paypal endpoint call. A more sophisticated approach that caches a token for multiple and concurrent requests can be used.

Please check the source code of each service for more info.

## How should I use this ?

### How do I setup the app ?
In order to operate the website you first need:
* A MS SQL database (a different DBMS can be used but requires changes to the DbContext and the DesignTimeDbContextFactory, both of which can be found in the Data folder)
* A paypal developer account with sandbox test accounts (you may ofc use live accounts if you wish)
* An email address for sending emails from (email sending can however be disabled in appsetting.json through the "MailSender:MockMailSender" property)
* A text file containing your email address password (the password isn't written directly in the appsettings.json file, the file path is)
* (Optional) A location where to store DataProtection keys (otherwise the default for the platform will be used)

Either you want to use a local version or you want to publish the website, you must first run the db migration using the CLI commands or the publish wizard. 

After this you must setup an appsettings.Development.json file, an appsettings.Production.json file or modify the provided appsettings.json file.  
All the information needed to configure the app is provided inside the appsetings.json file itself.

### How do I create admin users ?
Make sure that the "UserRegistration:CreateAsAdmin" setting in appsettings.json is set to 'true' and if you want to spare the hassle or you don't have the email sending service 
setup also set the "UserRegistration:EmailConfirmationRequired" to 'false'.  

Click on the 'Register' button in the top banner and fill in the form. Done !

Remember to change your "UserRegistration" settings after all you admin accounts have been created.

### What can I do as a normal user ?
A regular user can:
* Edit its profile, clicking on the "Welcome \<username>" button in the top banner
* Check for slots availability and reserve them, using the link in the home page

### What can I do as an admin user ?
As and admin you can do everything a normal user can but also have access to the 'Administration area' through a link in the top banner.  

In this area you can:
* See user details and delete them
* Manage the courts
* Manage the templates
* Manage the slots that can be reserved 

### How can I manage the slots for reservation ?
A slot is 1h of court access.  
As an admin you can choose which slots you want to be made reservable and at what price.  
The price can be chosen for each slot.

You can manage the slots in the /administration/reservationslots area: 
1. Select a Court (you must have created one)
1. Select a date (the slots for the whole week, starting from that date's Monday, will be loaded)
1. Select/Deselect any slot you want and fill in the price for each selected slot (unselected slots will not be selected even if a price has been specified) 
1. Click the 'Submit' button (a 'Submission OK' or 'Submission FAILED' msg will appear nearby)

To facilitate the slot management, instead of setting up slots one by one the admin can use 'Templates' to quickly setup the whole week.   
In order to do so, select a template and click the 'Fill' button.  
This will transfer that 'week template's values to the current week.

An admin can create and edit templates in the /administration/templates area.  
Select/Deselect any slot you want and fill in the price for each selected slot.  
Remember to hit 'Submit' to save your template.


Watch out that in this app there's no safety check that disallows an admin to unselect slots already reserved.  
Deleting these slots will also delete the reservations for them due to the db referential integrity.

### How do I reserve a slot ?
Click on the link in the main page and select a date from the date picker.  
You'll see a list of all the slots for all the courts for that day.  
* Gray boxes represent slots that are not up for reservation
* Red boxes represent slots already taken
* Green boxes represents available slots
* Yellow boxes represents boxes currently selected for reservation

Plese note that in this app it's not possible to select slots across multiple dates.  
The system doesn't implement a cart and doesn't "remember" previous selections when picking dates.

Select at least one box and press 'Continue'.  
You'll be sent to the checkout page where you can revisit your selection and proceed to the payment.  
Clicking on the 'Confirm' button will redirect you to the Paypal authorization page.







