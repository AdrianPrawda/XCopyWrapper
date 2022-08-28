using System.Collections.Concurrent;
using System.IO.Pipes;
using XCopyWrapper;

const string ROOT_FOLDER = "root\\";
const string IPC_ID = "XCopyWrapperIPC";
const string MUTEX_ID = "XCopyWrapperMutex";

BlockingCollection<string> copyQueue = new();
string rootFolder = Path.Join(AppContext.BaseDirectory, ROOT_FOLDER);

Mutex mutext = new(false, MUTEX_ID);
try
{
    if (mutext.WaitOne(0, false))
    {
#if DEBUG
        Console.WriteLine("[DEBUG] Running as parent process");
#endif
        AnsiConsole.WriteLine("\x1b[35;1m[INFO]\x1b[0m Workspace: {0}", rootFolder);

        if (args.Length > 0)
        {
            foreach (string path in args)
            {
                copyQueue.Add(path);
            }
        }

        RunXCopyServer(copyQueue, rootFolder);
        await RunIPCServer();
        mutext.ReleaseMutex();
    }
    else
    {
#if DEBUG
        Console.WriteLine("[DEBUG] Running as child process");
#endif
        RunIPCClient();

#if DEBUG
        WriteBlockingLine();
#endif
    }
}
finally
{
    mutext.Close();
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