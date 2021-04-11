using DashLink.Core.Action;
using DashLink.Core.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Builtin
{
    public sealed class CommandModule : IModule
    {
        public string Name => "command";

        private readonly HashSet<CommandAction> commands;

        public event ModuleMessageHandler ModuleMessageEvent;

        public int DebounceTime { get; set; }

        public CommandModule()
        {
            commands = new HashSet<CommandAction>();
        }

        public void Start(DashLinkHost host)
        {
            commands.Clear();
            DebounceTime = host.Config.CommandDebounce;

            foreach (var cmd in host.Config.Commands)
            {
                switch (cmd.Type)
                {
                    case CommandType.Native:
                    case CommandType.NativePrivileged:
                        ModuleMessageEvent?.Invoke(this, new ModuleMessageEventArgs(this, ModuleMessageLevel.Debug, "Registered program start " + cmd.Name));
                        commands.Add(new NativeCommandAction(Name + "." + cmd.Name, cmd.Execute.Run, cmd.Execute.Args, cmd.Execute.StartIn, cmd.Type == CommandType.NativePrivileged, DebounceTime));
                        break;
                    case CommandType.PowerShell:
                        bool script = cmd.Execute.Run != null;
                        ModuleMessageEvent?.Invoke(this, new ModuleMessageEventArgs(this, ModuleMessageLevel.Debug, "Registered PowerShell start " + cmd.Name));
                        commands.Add(new PowerShellCommandAction(Name + "." + cmd.Name, script ? cmd.Execute.Run : cmd.Execute.Args, cmd.Execute.StartIn, script, DebounceTime));
                        break;
                }
            }
        }

        public void Stop()
        { }

        public void RegisterActions(ActionExecutionRegister aer)
        {
            foreach (CommandAction action in commands)
            {
                aer.Register(action.Name, action);
            }
        }

        public static bool IsCurrentlyRunningElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool StartProcess(string path, string args, string startIn, bool elevated)
        {
            Process process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.UseShellExecute = true;
            if (elevated) process.StartInfo.Verb = "runas";

            if (!string.IsNullOrEmpty(args))
            {
                process.StartInfo.Arguments = args;
            }

            if (!string.IsNullOrEmpty(startIn))
            {
                process.StartInfo.WorkingDirectory = startIn;
            }
            return process.Start();
        }

        public static bool ExecutePowershellCommand(string command, string startIn, bool fileScript)
        {
            PowerShell ps = PowerShell.Create();
            if (!string.IsNullOrEmpty(startIn))
            {
                ps.AddCommand("Set-Location").AddParameter("Path", startIn);
            }
            ps.AddScript(fileScript ? File.ReadAllText(command) : command);

            ps.Invoke();
            return true;
        }

        public abstract class CommandAction : DelegatedAction
        {
            public string Name { get; protected set; }
            private int debounce;

            public CommandAction(int debounce)
            {
                this.debounce = debounce;
            }

            public override DebounceMode GetDebounceMode()
            {
                return DebounceMode.IgnoreSubsequent;
            }

            public override int GetDebounceTime()
            {
                return 2;
            }
        }

        public class NativeCommandAction : CommandAction
        {
            public string Path { get; }
            public string Args { get; }
            public string StartIn { get; }
            public bool Elevated { get; }

            public NativeCommandAction(string name, string path, string args, string startIn, bool elevated, int debounce) : base(debounce)
            {
                Name = name;
                Path = string.IsNullOrEmpty(path) ? throw new ArgumentNullException(nameof(path)) : path;
                Args = args;
                StartIn = startIn;
                Elevated = elevated;
            }

            public override Task<bool> GetInvokeAction(string data)
            {
                return new Task<bool>(() =>
                {
                    return StartProcess(Path, Args, StartIn, Elevated);
                });
            }
        }

        public class PowerShellCommandAction : CommandAction
        {
            public string Command { get; }
            public string StartIn { get; }
            public bool FileScript { get; }

            public PowerShellCommandAction(string name, string command, string startIn, bool fileScript, int debounce) : base(debounce)
            {
                Name = name;
                Command = string.IsNullOrEmpty(command) ? throw new ArgumentNullException(nameof(command)) : command;
                StartIn = startIn;
                FileScript = fileScript;
            }

            public override Task<bool> GetInvokeAction(string data)
            {
                return new Task<bool>(() =>
                {
                    return ExecutePowershellCommand(Command, StartIn, FileScript);
                });
            }
        }
    }
}
