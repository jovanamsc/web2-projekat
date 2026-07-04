using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.ShareService;

try
{
    await ServiceRuntime.RegisterServiceAsync("ShareServiceType",
        context => new ShareService(context));
    Thread.Sleep(Timeout.Infinite);
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    throw;
}
