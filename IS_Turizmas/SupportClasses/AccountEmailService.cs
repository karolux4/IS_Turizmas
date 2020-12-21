using IS_Turizmas.Controllers;
using IS_Turizmas.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IS_Turizmas.SupportClasses
{
    public class AccountEmailService : IHostedService
    {
        //private readonly IAccountService accountServive;
        private readonly ApplicationDbContext _context;

        public AccountEmailService(ApplicationDbContext context)
        {
            _context = context;
            //this.accountServive = accountServive;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TimeSpan interval = TimeSpan.FromDays(1);
            //calculate time to run the first time & delay to set the timer
            //DateTime.Today gives time of midnight 00.00
            var nextRunTime = DateTime.Today;
            var curTime = DateTime.Now;
            var firstInterval = nextRunTime.Subtract(curTime);

            Action action = () =>
            {
                var t1 = Task.Delay(firstInterval);
                t1.Wait();
                //remove inactive accounts at expected time
                SendEmailsToAccounts(null);
                //now schedule it to be called every 24 hours for future
                // timer repeates call to RemoveScheduledAccounts every 24 hours.
                var _timer = new Timer(
                    SendEmailsToAccounts,
                    null,
                    TimeSpan.Zero,
                    interval
                );
            };

            // no need to await this call here because this task is scheduled to run much much later.
            Task.Run(action);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var _timer = new Timer(
            SendEmailsToAccounts,
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(24)
        );

            return Task.CompletedTask;
        }

        private void SendEmailsToAccounts(object state)
        {
            Console.WriteLine("Sent emails");
            UserController userController = new UserController(_context, null, null);
            //userController.SendEmailsMethod();
        }
    }
}
