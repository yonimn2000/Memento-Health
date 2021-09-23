using MementoHealth.Classes;
using MementoHealth.Exceptions;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MementoHealth
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            if (SendAsync(message.Destination, message.Subject, message.Body).Result)
                return Task.FromResult(0);
            return Task.FromResult(-1);
        }

        public static async Task<bool> SendAsync(string destination, string subject, string body)
        {
            string apiKey = Environment.GetEnvironmentVariable("MEMENTO_SENDGRID_KEY");
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net", 587)
            {
                Credentials = new NetworkCredential("apikey", apiKey),
                EnableSsl = true
            };
            mail.From = new MailAddress("Memento Health <memento@yman.dev>");
            mail.To.Add(destination);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            try
            {
                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public const int MinPinLength = 4;
        public const int MaxPinLength = 8;
        public const int MaxPinAccessFailedCount = 3;

        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store) { }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
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
                RequiredLength = 8,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            /*manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });*/
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
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
                        TokenLifespan = TimeSpan.FromHours(3)
                    };
            }
            return manager;
        }

        public async Task SetPinAsync(string userId, string pin)
        {
            if (!pin.All(char.IsDigit))
                throw new ArgumentException("PIN code must contain digits only.");

            if (pin.Length < MinPinLength || pin.Length > MaxPinLength)
                throw new ArgumentOutOfRangeException($"PIN code length must be between {MinPinLength} and {MaxPinLength} digits long. {pin.Length} digits given.");

            ApplicationUser user = await FindByIdAsync(userId);
            user.PinAccessFailedCount = 0;
            user.PinHash = SecurePasswordHasher.Hash(pin);
            await UpdateAsync(user);
        }

        public async Task<bool> VerifyPinAsync(string userId, string pin)
        {
            ApplicationUser user = await FindByIdAsync(userId);
            bool pinCorrect = SecurePasswordHasher.Verify(pin, user.PinHash);
            if (!pinCorrect)
            {
                user.PinAccessFailedCount++;
                await UpdateAsync(user);
                throw new ExceededPinAccessFailedCountException(MaxPinAccessFailedCount);
            }
            user.PinAccessFailedCount = 0;
            return pinCorrect;
        }

        public async Task<bool> HasPinAsync(string userId) => !string.IsNullOrEmpty((await FindByIdAsync(userId)).PinHash);
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

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
