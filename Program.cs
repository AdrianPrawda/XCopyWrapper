using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using XCopyWrapper;

const string ROOT_FOLDER = "root\\";
const string IPC_ID = "XCopyWrapperIPC";

BlockingCollection<string> copyQueue = new();
string rootFolder = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), ROOT_FOLDER);

if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location)).Length > 1)
{
#if DEBUG
    Console.WriteLine("[DEBUG] Running as child process");
#endif
    RunIPCClient();

#if DEBUG
    WriteBlockingLine();
#endif
}
else
{
#if DEBUG
    Console.WriteLine("[DEBUG] Running as parent process");
#endif
    Console.WriteLine("\x1b[35m\x1b[1m[INFO]\x1b[0m Workspace: {0}", rootFolder);

    if(args.Length > 0)
    {
        foreach(string path in args)
        {
            copyQueue.Add(path);
        }
    }

    RunXCopyServer(copyQueue, rootFolder);
    await RunIPCServer();
}

void RunIPCClient()
{
    var client = new NamedPipeClientStream(IPC_ID);
    client.Connect();

    using(var streamWriter = new StreamWriter(client))
    {
        foreach(var path in args)
        {
#if DEBUG
            Console.WriteLine("Sending data to server: {0}", path);
#endif
            streamWriter.WriteLine(path);
        }
    }

    client.Dispose();
}

async Task RunIPCServer()
{
    var pipeServer = new PipeServer(IPC_ID);
    pipeServer.MessageReceived += HandleMessageReceived;
    await Task.Run(() =>
    {
        pipeServer.Start();
    });
}

async void RunXCopyServer(BlockingCollection<string> queue, string rootFolder)
{
    var copyServer = new XCopyServer(queue, rootFolder);
    await Task.Run(() =>
    {
        copyServer.Start();
    });
}

void HandleMessageReceived(object? sender, MessageReceivedEventArgs e)
{
#if DEBUG
    Console.WriteLine("Handling MessageReceived event with args: {0}", e.Message);
#endif
    copyQueue.Add(e.Message);
}

static void WriteBlockingLine(string line = "Press any key to continue...")
{
    Console.WriteLine(line);
    Console.ReadKey();
}