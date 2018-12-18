using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using spolks.Extensions;
using spolks.HostAddressValidator;
using spolks.ParallelPing;
using spolks.Tracert;

namespace spolks
{
    public class Program
    {
        private static CommandLineApplication app;
        private static PingManager pingManager;
        private static IAddressValidator validator;
        
        static void Main(string[] args)
        {
            pingManager = new PingManager();
            validator = AddressValidatorProvider.GetValidator();
            while (true)
            {
                ConfigureCommandLine();
                PrintInvitationConsole();
                var command = Console.ReadLine();
                ExecuteCommand(command);
            }
        }

        private static void PrintInvitationConsole()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("=> ");
            Console.ResetColor();
        }
        
        private static void ExecuteCommand(string command)
        {
            try
            {
                app.Execute(command.Split(' '));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ConfigureCommandLine()
        {
            app = new CommandLineApplication();
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                Console.WriteLine("Hello World!");
                return 0;
            });

            app.Command("ping", command =>
            {
                command.SetInitialConfiguration("Pings specified hosts.");
                command.OnExecute(() =>
                {
                    var result = pingManager.Ping();
                    Console.WriteLine(result);
                    
                    return 0;
                });

            });
            
            app.Command("add", command =>
            {
                command.SetInitialConfiguration("Adds hosts to ping.");
                var hosts = command.Argument("[hosts]",
                    "A list of end points to ping.", true).Values;
                command.OnExecute(() =>
                {
                    var validatedHosts = validator.Validate(hosts);
                    pingManager.AddRange(validatedHosts);
                    validatedHosts.ToList().ForEach(Console.WriteLine);
                    
                    return 0;
                });

            });

            app.Command("clear", command =>
            {
                command.SetInitialConfiguration("Cleans a list of host we need to ping.");
                command.OnExecute(() =>
                {
                    pingManager.Clear();
                    return 0;
                });
            });

            app.Command("show", command =>
            {
                command.SetInitialConfiguration("Displays a list of hosts.");
                command.OnExecute(() =>
                {
                    Console.WriteLine(pingManager.DisplayHosts());

                    return 0;
                });
            });

            app.Command("remove", command =>
            {
                command.SetInitialConfiguration("Removes provided host from the list.");
                var host = command.Argument("[host]", "Host to remove");

                command.OnExecute(() =>
                {
                    pingManager.Remove(host.Value);

                    return 0;
                });
            });
            
            app.Command("trace", command =>
            {
                command.SetInitialConfiguration("Traces the way to host.");
                var host = command.Argument("[host]",
                    "A list of end points to ping.");
                command.OnExecute(() =>
                {
                    Console.Write(new TracertOrchestrator().Trace(host.Value));
                    
                    return 0;
                });

            });
        }
    }
}
