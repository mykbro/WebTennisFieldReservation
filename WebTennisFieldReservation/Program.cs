using Microsoft.AspNetCore.DataProtection;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Services;
using SmtpLibrary;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebTennisFieldReservation.AuthenticationSchemes.MyAuthScheme;
using WebTennisFieldReservation.AuthorizationPolicies.SameUser;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

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

            // Add dbcontext backed repository
            string connString = builder.Configuration.GetConnectionString(ConnectionStringsNames.Default) ?? throw new InvalidOperationException("Connection string missing");
            builder.Services.AddScoped<ICourtComplexRepository, DbCourtComplexRepository>(_ => new DbCourtComplexRepository(connString));

            // Add password hasher
            builder.Services.AddSingleton<IPasswordHasher>(new Pbkdf2PasswordHasher(passwordSettings.Iterations));

            // Add data protection
            builder.Services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = dataProtectionSettings.AppDiscriminator;                
            }).PersistKeysToFileSystem(new DirectoryInfo(dataProtectionSettings.KeysFolderPath));

            // Add security token manager            
            builder.Services.AddSingleton<TokenManagerSettings>(tokenManagerSettings);  //required for controllers that use ITokenManager
            builder.Services.AddScoped<ITokenManager, DataProtectionTokenManager>();    //needs to be scoped because it uses scoped IDataProtectionProvider

            // Add mail sending service            
            string smtpPassword = File.ReadAllText(@"D:\smtppwd.txt");
            
            SmtpClientFactory smtpClientFactory = new SmtpClientFactory(mailSenderSettings.HostName, mailSenderSettings.Port, mailSenderSettings.UseSSL, mailSenderSettings.User, smtpPassword);
            SmtpClientPoolSender smtpClientPoolSender = new SmtpClientPoolSender(smtpClientFactory, 1, 10);
            builder.Services.AddSingleton<ISingleUserMailSender>(new SingleUserPooledMailSender(smtpClientPoolSender, mailSenderSettings.User));

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
                        DateTimeOffset cookieIssueTime = DateTimeOffset.Parse(context.User.FindFirstValue(ClaimsNames.IssueTime));

                        return DateTimeOffset.Now <= cookieIssueTime.AddMinutes(loggedRecentlyPolicySettings.MaxAgeInMins);
                    });
                });
            });

            builder.Services.AddSingleton<IAuthorizationHandler, SameUserAuthZHandler>();

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