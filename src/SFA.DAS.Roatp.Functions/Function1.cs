using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Roatp.Functions.Configuration;

namespace SFA.DAS.Roatp.Functions
{
    public class Function1
    {
        private readonly IConfiguration _configuration;
        private readonly ApplyApiAuthentication _applyApiAuthentication;
        private readonly QnaApiAuthentication _qnaApiAuthentication;
        
        public Function1(IConfiguration configuration, IOptions<ApplyApiAuthentication> applyApiAuthentication, IServiceProvider serviceProvider)
        {
            // NOTE: Here are several different ways to get config. Best practise is to use IOptions but I know some prefer IConfiguration
            _configuration = configuration;
            _applyApiAuthentication = applyApiAuthentication.Value;
            _qnaApiAuthentication = serviceProvider.GetService<IOptions<QnaApiAuthentication>>().Value;
        }


        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("C# Timer trigger function is running later than scheduled");
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            await Task.CompletedTask;
        }
    }
}
