using System;
using System.Configuration;
using System.Linq;
using System.Web;
using CogisoftConnector;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;
using Hangfire;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace CogisoftConnector
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);

            GlobalConfiguration.Configuration.UseNinjectActivator(kernel);

            System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = new Ninject.Web.WebApi.NinjectDependencyResolver(kernel);
            return kernel;
        }
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ILogger>().ToMethod(ctx => LoggerFactory.CreateLogger(null)).InNamedOrBackgroundJobScope(context => context.Kernel.Components.GetAll<INinjectHttpApplicationPlugin>().Select(c => c.GetRequestScope(context)).FirstOrDefault(s => s != null));
            kernel.Bind<CogisoftSyncVacationDataLogic>().ToSelf().InRequestScope();
            kernel.Bind<CogisoftWebhookLogic>().ToSelf().InRequestScope();

            bool mockMode = bool.TryParse(ConfigurationManager.AppSettings["MockMode"], out mockMode) && mockMode;
            kernel.Bind<ICogisoftVacationValidationLogic>().To<CogisoftVacationValidationMockLogic>().When(ctx => mockMode).InRequestScope();
            kernel.Bind<ICogisoftVacationValidationLogic>().To<CogisoftVacationValidationLogic>().When(ctx => !mockMode).InRequestScope();
            kernel.Bind<IWebhookLogic>().To<CogisoftWebhookMockLogic>().When(ctx => mockMode).InRequestScope();
            kernel.Bind<IWebhookLogic>().To<CogisoftWebhookLogic>().When(ctx => !mockMode).InRequestScope();
            kernel.Bind<ISyncVacationDataLogic>().To<CogisoftSyncVacationDataMockLogic>().When(ctx => mockMode).InRequestScope();
            kernel.Bind<ISyncVacationDataLogic>().To<CogisoftSyncVacationDataLogic>().When(ctx => !mockMode).InRequestScope();
            kernel.Bind<ConfigurationTestLogic>().ToSelf().InRequestScope();
            kernel.Bind<EmployeeImportLogic>().ToSelf().InRequestScope();
        }
    }
}