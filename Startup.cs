using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CogisoftConnector.Startup))]

namespace CogisoftConnector
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            var option = new BackgroundJobServerOptions() { WorkerCount = Environment.ProcessorCount * 5 };
            app.UseHangfireServer(option);
            app.UseHangfireDashboard();
        }
    }
}
