using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace XCopyWrapper
{
    internal class XCopyServer
    {
        private readonly BlockingCollection<string> _copyQueue;
        private readonly string _rootPath;

        public XCopyServer(BlockingCollection<string> queue, string rootPath)
        {
            _copyQueue = queue;
            _rootPath = rootPath;
        }

        public void Start()
        {
#if DEBUG
            Console.WriteLine("[DEBUG] Starting XCopy Server");
#endif
            while(true)
            {
                var path = _copyQueue.Take();
                HandleCopyRequest(path);
            }
        }

        private void HandleCopyRequest(string path)
        {
            if(File.Exists(path) || Directory.Exists(path))
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo();
                process.StartInfo = startInfo;
                startInfo.FileName = "cmd.exe";

                if (File.Exists(path))
                {
                    Console.WriteLine("\x1b[35m\x1b[1m[INFO]\x1b[0m Copying file \x1b[45m\x1b[97m{0}\x1b[0m", path);
                    string additionalFlags = "";
                    if(new FileInfo(path).Length > 1e8)
                    {
#if DEBUG
                        Console.WriteLine("[DEBUG] File {0} is very large (> 100MB), disabling buffering");
#endif
                        additionalFlags = "/j";
                    }
                    startInfo.Arguments = string.Format("/C xcopy /v/h/y/f{0} {1} {2}", additionalFlags, path, BuildTargetPath(path));
                }
                else if (Directory.Exists(path))
                {
                    Console.WriteLine("\x1b[35m\x1b[1m[INFO]\x1b[0m Copying directory \x1b[45m\x1b[97m{0}\x1b[0m recursively", path);
                    startInfo.Arguments = string.Format("/C xcopy /s/e/v/h/i/f {0} {1}", path, BuildTargetPath(path));
                }

                process.Start();
                process.WaitForExit();

                if(process.ExitCode != 0)
                {
                    string errReason = process.ExitCode switch
                    {
                        1 => "No files found",
                        2 => "User terminated copy process",
                        4 => "Initialisation error",
                        5 => "Disk write error",
                        _ => string.Format("Error Code {0}", process.ExitCode),
                    };
                    Console.WriteLine("\x1b[31m\x1b[1m[ERROR]\x1b[0m Error copying {0}: {1}", path, errReason);
                }
            }
            else
            {
                Console.WriteLine("\x1b[31m[ERROR]\x1b[0m {0} not found or no longer exists.", path);
            }
        }

        private string BuildTargetPath(string path)
        {
            Regex colonRegex = new(Regex.Escape(":"));
            return Path.Combine(_rootPath, colonRegex.Replace(path, "", 1));
        }
    }
}