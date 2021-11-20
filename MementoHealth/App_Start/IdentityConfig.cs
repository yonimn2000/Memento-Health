using MementoHealth.Classes;
using MementoHealth.Exceptions;
using MementoHealth.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
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
        public async Task SendAsync(IdentityMessage message)
        {
            await SendAsync(message.Destination, message.Subject, message.Body);
        }

        public static async Task<bool> SendAsync(string destination, string subject, string body)
        {
#if DEBUG
            Debug.WriteLine($"Email to: {destination}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Body: {body}");
#endif
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
                await SmtpServer.SendMailAsync(mail);
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
    public class ApplicationUserManager : UserManager<ApplicationUser, string>
    {
        public const int MinPinLength = 4;
        public const int MaxPinLength = 8;
        public const int MaxPinAccessFailedCount = 3;

        public ApplicationUserManager(IUserStore<ApplicationUser, string> store) : base(store) { }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>(context.Get<ApplicationDbContext>()));
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

        public async Task<string> GetFullNameAsync(string userId)
        {
            ApplicationUser user = await FindByIdAsync(userId);
            return user.FullName;
        }

        public async Task SetFullNameAsync(string userId, string fullName)
        {
            ApplicationUser user = await FindByIdAsync(userId);
            user.FullName = fullName;
            await UpdateAsync(user);
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

            if (SecurePasswordHasher.Verify(pin, user.PinHash))
                return true;

            if (user.PinAccessFailedCount >= MaxPinAccessFailedCount)
                throw new ExceededMaxPinAccessFailedCountException(MaxPinAccessFailedCount);

            return false;
        }

        public async Task PinAccessFailedAsync(string userId)
        {
            ApplicationUser user = await FindByIdAsync(userId);
            user.PinAccessFailedCount++;
            await UpdateAsync(user);
        }

        public async Task ResetPinAccessFailedCountAsync(string userId)
        {
            ApplicationUser user = await FindByIdAsync(userId);
            user.PinAccessFailedCount = 0;
            await UpdateAsync(user);
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
