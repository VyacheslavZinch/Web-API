using System;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();


app.MapGet("/getprocesses", () =>
{
    StringBuilder allProcess = new StringBuilder();
    Process[] localAll = Process.GetProcesses();

    foreach (var process in localAll)
    {
        allProcess.Append($"{process.ProcessName} {((double)process.VirtualMemorySize64 / 1024)}kb\n");
    }

    GC.Collect();
    return Results.Json(allProcess.ToString());

});

app.MapGet("/shutdown", () =>
{
    Process.Start("ShutDown", "/s");
    return Results.Json("Shutdown device.");
});

app.MapGet("/reboot", () =>
{
    Process.Start("ShutDown", "/r");
    return Results.Json("Rebooting device.");
});

app.MapGet("/killprocess/{processname}", (string processname) =>
{   try
    {
        Process[] AllProcesses = Process.GetProcessesByName(processname);
        foreach (Process worker in AllProcesses)
        {
            worker.Kill();
            worker.WaitForExit();
            worker.Dispose();
        }
        GC.Collect();
        return Results.Json($"Process {processname} was finished.");
    }
    catch(ArgumentException)
    {
        return Results.Json("Process was not finished. Please set another timer");
    }
    catch (NullReferenceException)
    {
        return Results.Json("Process was not found.");
    }

});


app.MapGet("/lockdevice/{minutes}", (int minutes) =>
{
    try
    {
        string result = "Device was locked"; ;

        checked
        {
            minutes = minutes * 60 * 1000;
        }

        Task startLock = new(() =>
        {
            Task.Delay(minutes).Wait();
            Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
            
        });

        startLock.Start();
        return Results.Json(result);

    }
    catch(OverflowException)
    {
        return Results.Json($"Try setting a shorter period.");
    }
    catch(ArgumentNullException)
    {
        return Results.Json($"Try again.");
    }

});



app.Run();
