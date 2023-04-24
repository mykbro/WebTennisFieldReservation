using Microsoft.AspNetCore.DataProtection;
using WebTennisFieldReservation.Constants.Names;
using WebTennisFieldReservation.Data;
using WebTennisFieldReservation.Settings;
using WebTennisFieldReservation.Services;
using SmtpLibrary;
using Microsoft.AspNetCore.Authentication.Cookies;

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
            builder.Services.AddSingleton<ClaimsPrincipalFactory>(new ClaimsPrincipalFactory("Cookies" /*AuthenticationSchemesNames.MyAuthScheme*/));

            // Add authentication
            builder.Services.AddAuthentication("Cookies").AddCookie(options =>
            {
                options.AccessDeniedPath = "/forbidden";
                options.ReturnUrlParameter = QueryFieldsNames.ReturnUrl;
                options.LoginPath = "/users/login";
                options.LogoutPath = "/users/logout";
            });


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
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}