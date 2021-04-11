using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Action
{
    public class ActionExecutor
    {
        private readonly Dictionary<string, ActionExecutionRegister> actionMap;

        public ActionExecutor()
        {
            actionMap = new Dictionary<string, ActionExecutionRegister>();
        }

        internal void RegisterModule(string identifier, IModule module, bool enable)
        {
            if (actionMap.ContainsKey(identifier)) throw new Exception("Module with name " + module.Name + " already registered");
            var aer = new ActionExecutionRegister(identifier)
            {
                Enabled = enable
            };
            module.RegisterActions(aer);

            actionMap.Add(identifier, aer);
        }

        internal void UnregisterModule(string identifier)
        {
            actionMap.Remove(identifier);
        }

        internal void EnableModule(string identifier)
        {
            var aer = GetModule(identifier);
            if (aer != null) aer.Enabled = true;
        }

        internal void DisableModule(string identifier)
        {
            var aer = GetModule(identifier);
            if (aer != null) aer.Enabled = false;
        }

        internal HashSet<string> SetEnabledSelection(ICollection<string> identifiers)
        {
            HashSet<string> notFound = new HashSet<string>(identifiers);
            foreach (var keyValue in actionMap)
            {
                keyValue.Value.Enabled = notFound.Remove(keyValue.Key);
            }
            return notFound;
        }

        internal ActionExecutionRegister GetModule(string identifier)
        {
            if (actionMap.TryGetValue(identifier, out ActionExecutionRegister aer)) return aer;
            return null;
        }

        internal void Reset()
        {
            actionMap.Clear();
        }

        public void Call(string evt, bool asAsync)
        {
            string root = evt.Substring(0, evt.IndexOf('.'));
            var di = evt.IndexOf('$');
            string data = di >= 0 ? evt.Substring(di + 1) : string.Empty;

            if (actionMap.TryGetValue(root, out ActionExecutionRegister aer))
            {
                var cmd = di >= 0 ? evt.Substring(0, di) : evt;
                var act = aer.TryGetAction(cmd);
                if (act == null) return;

                bool invoke = false;
                bool delayAfter = false;
                int delay = 0;
                long currentTicks = Environment.TickCount;

                switch (act.GetDebounceMode())
                {
                    case DebounceMode.None:
                        act.TimesToExecute = 1;
                        invoke = true;
                        break;
                    case DebounceMode.IgnoreSubsequent:
                        if (act.LastExecution + act.GetDebounceTime() < currentTicks && !act.Running)
                        {
                            act.TimesToExecute = 1;
                            invoke = true;
                        }
                        break;
                    case DebounceMode.IgnoreSubsequentUntilDelay:
                        if (act.LastExecution + act.GetDebounceTime() < currentTicks && !act.Running)
                        {
                            act.TimesToExecute = 1;
                            invoke = true;
                        }
                        else
                        {
                            act.LastExecution = currentTicks;
                        }
                        break;
                    case DebounceMode.CountFromFirst:
                        if (act.TimesToExecute == 0 && !act.Running)
                        {
                            invoke = true;
                            act.TimesToExecute = 1;
                            delay = act.GetDebounceTime();
                        }
                        else
                        {
                            act.TimesToExecute++;
                        }
                        break;
                    case DebounceMode.CountFromSecond:
                        if (act.TimesToExecute == 0 && !act.Running)
                        {
                            invoke = true;
                            act.TimesToExecute = 1;
                            delayAfter = true;
                            delay = act.GetDebounceTime();
                        }
                        else
                        {
                            act.TimesToExecute++;
                        }
                        break;
                }

                if (invoke)
                {
                    var task = act.GetInvokeAction(data);
                    if (task == null) return;

                    act.Running = true;

                    if (delay > 0)
                    {
                        // Run it once, then wait, then run it again
                        if (delayAfter)
                        {
                            var secondTask = act.GetInvokeAction(data);
                            Task.Run(async delegate
                            {
                                task.RunSynchronously();
                                await Task.Delay(delay);
                                if (act.TimesToExecute > 1)
                                {
                                    act.TimesToExecute--;
                                    secondTask.RunSynchronously();
                                    UpdateExecution(act, secondTask.Result);
                                }
                                else
                                {
                                    UpdateExecution(act, task.Result);
                                }
                            });
                        }
                        // Run it later
                        else
                        {
                            Task.Run(async delegate
                            {
                                await Task.Delay(delay);
                                task.RunSynchronously();
                                UpdateExecution(act, task.Result);
                            });
                        }
                        return;
                    }

                    // Run it now, async
                    if (asAsync)
                    {
                        task.ContinueWith(previous => {
                            UpdateExecution(act, previous.Result);
                        });
                        task.Start();
                    }
                    // Run it now, sync
                    else
                    {
                        task.RunSynchronously();
                        UpdateExecution(act, task.Result);
                    }
                }
            }
        }

        private void UpdateExecution(DelegatedAction action, bool result)
        {
            action.TimesToExecute = 0;
            action.Running = false;
            action.LastResult = result;
            action.LastExecution = Environment.TickCount;;
        }
    }
}
