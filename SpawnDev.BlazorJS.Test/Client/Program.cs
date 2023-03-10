using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Test;
using SpawnDev.BlazorJS.Test.Services;
using SpawnDev.BlazorJS.WebWorkers;




var tt = new ValueTask<string>();
var tr = tt.GetType();
var gtd = tr.GetGenericTypeDefinition();
var bb = typeof(ValueTask).IsAssignableFrom(tr);
var b2 = typeof(ValueTask<>).IsAssignableFrom(tr);
var b3 = tr.GetGenericTypeDefinition() == typeof(ValueTask);
var b4 = tr.GetGenericTypeDefinition() == typeof(ValueTask<>);
var b5 = tr.IsValueTask();


var p = new Promise();

JS.Set("_pp", p);

_ = Task.Run(async () => {
    await Task.Delay(10000);
    p.Resolve("Hello promises!");
});



using var navigator = JS.Get<Navigator>("navigator");
using var locks = navigator.Locks;

Console.WriteLine($"lock: 1");

using var waitLock = locks.Request("my_lock", Callback.CreateOne((Lock lockObj) => new Promise(async () => {
    Console.WriteLine($"lock acquired 3");
    await Task.Delay(5000);
    Console.WriteLine($"lock released 4");
})));

using var waitLock2 = locks.Request("my_lock", Callback.CreateOne((Lock lockObj) => new Promise(async () => {
    Console.WriteLine($"lock acquired 5");
    await Task.Delay(5000);
    Console.WriteLine($"lock released 6");
})));

Console.WriteLine($"lock: 2");



//var ttt = new AsyncActionCallback<int>(async (int ttttt) => {
//    Console.WriteLine($"ttttt: {ttttt}");
//});
//JS.Set("_ttt", ttt);





var builder = WebAssemblyHostBuilder.CreateDefault(args);
if (JS.IsWindow)
{
    // we can skip adding dom objects in non UI threads
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
}
// add services
builder.Services.AddSingleton((sp) => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// SpawnDev.BlazorJS.WebWorkers
builder.Services.AddSingleton<WebWorkerService>();
builder.Services.AddSingleton<WebWorkerPool>();
// 
builder.Services.AddSingleton<IFaceAPIService, FaceAPIService>();
builder.Services.AddSingleton<IMathsService, MathsService>();

builder.Services.AddSingleton<MediaDevices>();
builder.Services.AddSingleton<MediaDevicesService>();
// Radzen
builder.Services.AddSingleton<DialogService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<TooltipService>();
builder.Services.AddSingleton<ContextMenuService>();
// build 
WebAssemblyHost host = builder.Build();
// initialize WebWorkerService
var webWorkerService = host.Services.GetRequiredService<WebWorkerService>();
await webWorkerService.InitAsync();
#region TestingSection
WebWorker? _testWorker = null;
if (JS.IsWindow)
{
    // window thread
    JS.Set("_getTest", Callback.Create(new Action<bool?>(async (verbose) =>
    {
        if (_testWorker == null) _testWorker = await webWorkerService.GetWebWorker(verbose.HasValue && verbose.Value);
    })));
    JS.Set("_disposeTest", Callback.Create(new Action<bool?>(async (verbose) =>
    {
        _testWorker?.Dispose();
        _testWorker = null;
    })));
    JS.Set("_getWorker", Callback.Create(new Action<bool?>(async (verbose) =>
    {
        using var webWorker = await webWorkerService.GetWebWorker(verbose.HasValue && verbose.Value);
        var faceApiServiceWorker = webWorker.GetService<IFaceAPIService>();
        //await faceApiServiceWorker.CallTest();
        //var ret = await webWorker.InvokeAsync<MathsService, string>(nameof(MathsService.CalculatePiWithActionProgress), 100, new Action<int>((i) =>
        //{
        //    Console.WriteLine($"Progress: {i}");
        //}));
        Console.WriteLine($"ret: called");
    })));
    JS.Set("_getSharedWorker", Callback.Create(new Action<bool?>(async (verbose) =>
    {
        using var webWorker = await webWorkerService.GetSharedWebWorker(verboseMode: verbose.HasValue && verbose.Value);
        var ret = await webWorker.InvokeAsync<MathsService, string>(nameof(MathsService.CalculatePiWithActionProgress), 100, new Action<int>((i) =>
        {
            Console.WriteLine($"Progress: {i}");
        }));
        Console.WriteLine($"ret: {ret}");
    })));
}
else if (JS.IsDedicatedWorkerGlobalScope)
{
    // this method can be called using the browser debug console from the webworker context to call into the main window
    // this allows debugging the service worker call
    JS.Set("_getWorker", Callback.Create(new Action<bool?>(async (verbose) => {

        var webWorker = webWorkerService.DedicatedWorkerParent;
        var faceApiServiceWorker = webWorker.GetService<IFaceAPIService>();
        //await faceApiServiceWorker.CallTest();
        //var ret = await webWorker.InvokeAsync<MathsService, string>(nameof(MathsService.CalculatePiWithActionProgress), 100, new Action<int>((i) =>
        //{
        //    Console.WriteLine($"Progress: {i}");
        //}));
        Console.WriteLine($"ret: called");
    })));

    //Console.WriteLine($"IsDedicatedWorkerGlobalScope: -----------------");
    //// dedicated worker thread
    //var webWorker = webWorkerService.DedicatedWorkerParent;
    //var ret = await webWorker.InvokeAsync<MathsService, string>(nameof(MathsService.CalculatePiWithActionProgress), 100, new Action<int>((i) =>
    //{
    //    Console.WriteLine($"Progress: {i}");
    //}));
    //Console.WriteLine($"ret: {ret}");
}
else if (JS.IsWorker)
{
    // worker thread

}
#endregion
await host.RunAsync();