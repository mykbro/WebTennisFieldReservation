using Microsoft.AspNetCore.DataProtection;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebTennisFieldReservation.AuthenticationSchemes.MyAuthScheme;
using WebTennisFieldReservation.AuthorizationPolicies.SameUser;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using SmtpLibrary;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebTennisFieldReservation.Services.PasswordHasher;
using WebTennisFieldReservation.Services.ClaimPrincipalFactory;
using WebTennisFieldReservation.Services.TokenManager;
using WebTennisFieldReservation.Services.SingleUserMailSender;
using WebTennisFieldReservation.Services.HttpClients;
using WebTennisFieldReservation.Services.BackgroundTest;
using WebTennisFieldReservation.Services._Background;

namespace WebTennisFieldReservation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Bind settings objects
            PasswordSettings passwordSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.Passwords).Get<PasswordSettings>();
            DataProtectionSettings dataProtectionSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.DataProtection).Get<DataProtectionSettings>();
            MailSenderSettings mailSenderSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.MailSender).Get<MailSenderSettings>();
            TokenManagerSettings tokenManagerSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.TokenManager).Get<TokenManagerSettings>();
            AuthenticationSchemeSettings myAuthSchemeSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.AuthenticationSchemes + ":" + AuthenticationSchemesNames.MyAuthScheme).Get<AuthenticationSchemeSettings>();
            LoggedRecentlyPolicySettings loggedRecentlyPolicySettings = builder.Configuration.GetSection(ConfigurationSectionsNames.LoggedRecentlyPolicy).Get<LoggedRecentlyPolicySettings>();
            PaypalApiSettings paypalApiSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.PaypalApi).Get<PaypalApiSettings>();
            BackgroundReservationsCheckerSettings backgroundReservationsCheckerSettings = builder.Configuration.GetSection(ConfigurationSectionsNames.BackgroundReservationsChecker).Get<BackgroundReservationsCheckerSettings>();

            // Add dbcontext backed repository
            string connString = builder.Configuration.GetConnectionString(ConnectionStringsNames.Default) ?? throw new InvalidOperationException("Connection string missing");
            builder.Services.AddScoped<ICourtComplexRepository, DbCourtComplexRepository>( _ => new DbCourtComplexRepository(connString, log: true));

            // Add password hasher
            builder.Services.AddSingleton<IPasswordHasher>(new Pbkdf2PasswordHasher(passwordSettings.Iterations));

            // Add data protection (could have used just Configure)
            var dataProtBuilder = builder.Services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = dataProtectionSettings.AppDiscriminator;
            });

            if (dataProtectionSettings.UseKeysFolder)
            {
                dataProtBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionSettings.KeysFolderPath));
            }            

            // Add security token manager            
            builder.Services.AddSingleton<TokenManagerSettings>(tokenManagerSettings);  //required for controllers that use ITokenManager
            builder.Services.AddScoped<ITokenManager, DataProtectionTokenManager>();    //needs to be scoped because it uses scoped IDataProtectionProvider

            // Add mail sending service
            if (mailSenderSettings.MockMailSender)
            {
                builder.Services.AddSingleton<ISingleUserMailSender, ConsoleMailSender>();          //mock that displays email msgs in the console
            }
            else
            {
                //else we use the "true" service
                builder.Services.AddSingleton<ISingleUserMailSender, SingleUserPooledMailSender>();
            }   

            // Add claims builder
            builder.Services.AddSingleton<ClaimsPrincipalFactory>(new ClaimsPrincipalFactory(AuthenticationSchemesNames.MyAuthScheme));

            // Add authentication
            builder.Services.AddAuthentication(defaultScheme: AuthenticationSchemesNames.MyAuthScheme)
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = "/home/forbidden";
                    options.ReturnUrlParameter = QueryFieldsNames.ReturnUrl;
                    options.LoginPath = "/users/login";
                    options.LogoutPath = "/users/logout";
                })
                .AddScheme<MyAuthSchemeOptions, MyAuthSchemeHandler>(AuthenticationSchemesNames.MyAuthScheme, options =>
                {
                    options.CookieMaxAge = TimeSpan.FromMinutes(myAuthSchemeSettings.CookieMaxAgeInMinutes);
                    options.AccessDeniedPath = "/home/forbidden";
                    options.LoginPath = "/users/login";
                    options.ReturnUrlParameter = QueryFieldsNames.ReturnUrl;
                });

            // Add authorization policies (and services)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPoliciesNames.IsAdmin, policyBuilder =>
                {
                    policyBuilder.RequireClaim(ClaimsNames.IsAdmin, true.ToString());
                });

                options.AddPolicy(AuthorizationPoliciesNames.SameUser, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new SameUserRequirement());
                });

                options.AddPolicy(AuthorizationPoliciesNames.LoggedRecently, policyBuilder =>
                {
                    //the auth cookie IssueTime must be in the MaxAge window defined in appsettings.json
                    policyBuilder.RequireAssertion(context =>
                    {
                        string? issueTimeAsString = context.User.FindFirstValue(ClaimsNames.IssueTime);

                        if(issueTimeAsString != null)
                        {
							DateTimeOffset cookieIssueTime = DateTimeOffset.Parse(issueTimeAsString);
							return DateTimeOffset.Now <= cookieIssueTime.AddMinutes(loggedRecentlyPolicySettings.MaxAgeInMins);
						}
                        else
                        {
                            return false;
                        }						
                    });
                });
            });

            builder.Services.AddSingleton<IAuthorizationHandler, SameUserAuthZHandler>();

            // Add antiforgery header option for XHR Post calls           
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = HttpHeadersNames.X_CSRF_TOKEN;
            });

			// Add HTTP clients
			builder.Services.AddSingleton<PaypalApiSettings>(paypalApiSettings);    //we need to inject this in the Paypal clients			
            builder.Services.AddHttpClient<PaypalAuthenticationClient>();
            builder.Services.AddHttpClient<PaypalCreateOrderClient>();
            builder.Services.AddHttpClient<PaypalCapturePaymentClient>();
            builder.Services.AddHttpClient<PaypalCheckOrderClient>();

            // Add background services          
            builder.Services.AddSingleton<BackgroundReservationsCheckerSettings>(backgroundReservationsCheckerSettings);
            builder.Services.AddHostedService<ReservationsChecker>();


            //*************** BUILD ***************
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();                
                app.UseHttpsRedirection();
            }            
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}