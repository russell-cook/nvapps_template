using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using AdminApps.DAL;
using System.Data.Entity.ModelConfiguration;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace AdminApps.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private const int PasswordRequiredLength = 8;
        private const bool PasswordRequireNonLetterOrDigit = true;
        private const bool PasswordRequireDigit = true;
        private const bool PasswordRequireLowercase = true;
        private const bool PasswordRequireUppercase = true;

        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = PasswordRequiredLength,
                RequireNonLetterOrDigit = PasswordRequireNonLetterOrDigit,
                RequireDigit = PasswordRequireDigit,
                RequireLowercase = PasswordRequireLowercase,
                RequireUppercase = PasswordRequireUppercase
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromDays(14)
                    };
            }
            return manager;
        }

        public async Task<string> GenerateRandomPasswordAsync()
        {
            const string lower = @"abcdefghijklmnopqrstuvwxyz";
            const string upper = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = @"1234567890";
            //const string nonld = @"~!@#$%^&*()-_=+[{]}\|;"":',<.>/?";
            const string nonld = @"~!#$%^&*()-_=+[{]}|;:',<.>?";

            // See note at the the top (constants)
            // using required character sets, create appropriate source 
            var source = String.Format("{0}{1}{2}{3}",
                (PasswordRequireLowercase ? lower : String.Empty),
                (PasswordRequireUppercase ? upper : String.Empty),
                (PasswordRequireDigit ? digits : String.Empty),
                (PasswordRequireNonLetterOrDigit ? nonld : String.Empty)
                );
            // sanity check, this should never occur
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Source character set is empty!");
            }

            var sourceChars = source.ToCharArray();
            var data = new byte[PasswordRequiredLength];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(PasswordRequiredLength);
            foreach (var b in data)
            {
                result.Append(sourceChars[b % (sourceChars.Length)]);
            }
            var generatedPassword = result.ToString();

            // sanity check, this should never occur
            var isValid = await this.PasswordValidator.ValidateAsync(generatedPassword);
            if (!isValid.Succeeded)
            {
                throw new InvalidOperationException("Generated password failed validation!");
            }

            return generatedPassword;
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly. 
    // Russ 2014-11-12: Extensible ApplicationRole class implemented per http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore)
            : base(roleStore)
        {
        }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<ApplicationRole>(context.Get<ApplicationDbContext>()));
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            // return Task.FromResult(0);

            // Credentials:
            var credentialUserName = ConfigurationManager.AppSettings["MAIL_USER"];
            var sentFrom = "AdminApps@sendgrid.com";
            var pwd = ConfigurationManager.AppSettings["MAIL_PASSWORD"];

            // Configure the client:
            System.Net.Mail.SmtpClient client =
                new System.Net.Mail.SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));

            client.Port = 587;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Create the credentials:
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential(credentialUserName, pwd);

            client.EnableSsl = true;
            client.Credentials = credentials;

            // Create the message:
            var mail =
                new System.Net.Mail.MailMessage(sentFrom, message.Destination);

            mail.IsBodyHtml = true;
            mail.Subject = message.Subject;
            mail.Body = message.Body;

            // Send:
            return client.SendMailAsync(mail);

        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : 
            base(userManager, authenticationManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

}