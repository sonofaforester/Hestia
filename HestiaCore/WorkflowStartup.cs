using HestiaCore.ActionProviders;
using HestiaDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HestiaCore
{
    public static class WorkflowStartup
    {
        private static IApplicationBuilder app;

        public static void UseWorkflow(this IApplicationBuilder app)
        {
            WorkflowStartup.app = app;

            var runtime = app.ApplicationServices.GetService<WorkflowRuntime>();

            //starts the WorkflowRuntime
            //TODO If you have planned use Timers the best way to start WorkflowRuntime is somwhere outside of this function in Global.asax for example
            runtime.Start();
        }

        public static void AddWorkflow(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration[$"ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Please init ConnectionString before calling the Runtime!");
            }

            //TODO If you have a license key, you have to register it here
            //WorkflowRuntime.RegisterLicense("your license key text");

            var dbProvider = new PostgreSQLProvider(connectionString);

            var builder = new WorkflowBuilder<XElement>(
                dbProvider,
                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                dbProvider
            ).WithDefaultCache();

            var runtime = new WorkflowRuntime()
                     .WithBuilder(builder)
                     .WithPersistenceProvider(dbProvider)
                     .EnableCodeActions()
                     .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                     .WithActionProvider(new ActionProvider())
                     .AsSingleServer();

            //events subscription
            runtime.OnProcessActivityChanged += (sender, args) => { };
            runtime.OnProcessStatusChangedAsync += (sender, args, token) => { return ProcessStatusChanged(args, runtime); };
            //TODO If you have planned to use Code Actions functionality that required references to external assemblies you have to register them here
            //runtime.RegisterAssemblyForCodeActions(Assembly.GetAssembly(typeof(SomeTypeFromMyAssembly)));

            services.AddSingleton(s => runtime);            
        }

        static async Task ProcessStatusChanged(ProcessStatusChangedEventArgs args, WorkflowRuntime runtime)
        {
            var serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();

            using (var scope = serviceProvider.CreateScope())
            {
                if (args.NewStatus != ProcessStatus.Idled && args.NewStatus != ProcessStatus.Finalized)
                    return;

                if (string.IsNullOrEmpty(args.SchemeCode))
                    return;

                // DataServiceProvider.Get<IDocumentRepository>().DeleteEmptyPreHistory(e.ProcessId);
                runtime.PreExecuteFromCurrentActivity(args.ProcessId);

                //Inbox
                //var ir = DataServiceProvider.Get<IInboxRepository>();
                //ir.DropWorkflowInbox(e.ProcessId);

                //if (e.NewStatus != ProcessStatus.Finalized)
                //{
                //    Task.Run(() => PreExecuteAndFillInbox(e));
                //}

                //Change state name
                if (!args.IsSubprocess)
                {
                    var nextState = args.ProcessInstance.CurrentState;
                    if (nextState == null)
                    {
                        nextState = args.ProcessInstance.CurrentActivityName;
                    }
                    var nextStateName = runtime.GetLocalizedStateName(args.ProcessId, nextState);

                    var docRepository = scope.ServiceProvider.GetService<HestiaCoreContext>();

                    var ticket = docRepository.Tickets.FirstOrDefault(t => t.Id == args.ProcessId);

                    if (ticket != null)
                    {
                        ticket.NextState = nextState;
                        ticket.NextStateName = nextStateName;
                        await docRepository.SaveChangesAsync();
                    }
                    //docRepository.ChangeState(e.ProcessId, nextState, nextStateName);
                }
            }
        }
    }
}
