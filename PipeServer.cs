using System.IO.Pipes;

namespace XCopyWrapper
{
    internal class PipeServer
    {
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        private readonly string _pipeName;

        public PipeServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start()
        {
#if DEBUG
            Console.WriteLine("[DEBUG] Starting main server loop");
#endif

            while (true)
            {
                var pipe = new NamedPipeServerStream(_pipeName);
                pipe.WaitForConnection();

                using var streamReader = new StreamReader(pipe);
                while (!streamReader.EndOfStream)
                {
                    var path = streamReader.ReadLine();
#if DEBUG
                    Console.WriteLine("[DEBUG] Received message: {0}", path);
#endif

                    if(path != null)
                    {
                        OnMessageReceived(new MessageReceivedEventArgs(path));
                    }
                }

                pipe.Disconnect();
                pipe.Dispose();
            }
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }

    internal class MessageReceivedEventArgs
    {
        public string Message { get; private set; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}