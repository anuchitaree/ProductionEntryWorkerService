using ProductionEntryWorkerService;
using ProductionEntryWorkerService.WorkerServices;

namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<RecieveSerialPortWorker>();

                })
                .Build();

            host.Run();
        }
    }
}