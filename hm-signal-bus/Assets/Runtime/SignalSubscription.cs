using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeCatGames.HMSignalBus.Runtime
{
    public sealed class SignalSubscription
    {
        #region ReadonlyFields
        private readonly SortedDictionary<int, List<Delegate>> _receivers = new();
        private readonly SortedDictionary<int, List<Func<object, Task>>> _taskReceivers = new();
        private readonly SortedDictionary<int, List<Func<object, UniTask>>> _uniTaskReceivers = new();
        #endregion

        #region Getters
        public SignalStyle SignalStyle { get; }
        #endregion
        
        #region Constructors
        public SignalSubscription(SignalStyle signalStyle = SignalStyle.Normal) => SignalStyle = signalStyle;
        #endregion

        #region Executes
        public void Add(Delegate receiver, int priority = 0)
        {
            switch (receiver)
            {
                case Func<object, Task> taskReceiver:
                    if (!_taskReceivers.ContainsKey(priority))
                        _taskReceivers[priority] = new List<Func<object, Task>>();
                    
                    _taskReceivers[priority].Add(taskReceiver);
                    break;
                case Func<object, UniTask> uniTaskReceiver:
                    if (!_uniTaskReceivers.ContainsKey(priority))
                        _uniTaskReceivers[priority] = new List<Func<object, UniTask>>();
                    
                    _uniTaskReceivers[priority].Add(uniTaskReceiver);
                    break;
                default:
                    if (!_receivers.ContainsKey(priority))
                        _receivers[priority] = new List<Delegate>();
                    
                    _receivers[priority].Add(receiver);
                    break;
            }
        }
        public void Remove(Delegate receiver, int priority = 0)
        {
            switch (receiver)
            {
                case Func<object, Task> taskReceiver:
                    if (_taskReceivers.ContainsKey(priority))
                    {
                        _taskReceivers[priority].Remove(taskReceiver);
                        
                        if (_taskReceivers[priority].Count == 0) 
                            _taskReceivers.Remove(priority);
                    }
                    break;
                case Func<object, UniTask> uniTaskReceiver:
                    if (_uniTaskReceivers.ContainsKey(priority))
                    {
                        _uniTaskReceivers[priority].Remove(uniTaskReceiver);
                        
                        if (_uniTaskReceivers[priority].Count == 0) 
                            _uniTaskReceivers.Remove(priority);
                    }
                    break;
                default:
                    if (_receivers.ContainsKey(priority))
                    {
                        _receivers[priority].Remove(receiver);
                        
                        if (_receivers[priority].Count == 0) 
                            _receivers.Remove(priority);
                    }
                    break;
            }
        }
        public void Invoke(object signal)
        {
            foreach (KeyValuePair<int, List<Delegate>> receivers in _receivers.OrderByDescending(kvp => kvp.Key))
                foreach (Delegate receiver in receivers.Value)
                    ((Action<object>)receiver)(signal);
        }
        public async Task InvokeAsyncTask(object signal)
        {
            List<Task> tasks = new();

            foreach (KeyValuePair<int, List<Func<object, Task>>> receivers in _taskReceivers.OrderByDescending(kvp =>
                         kvp.Key))
                foreach (Func<object, Task> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await Task.WhenAll(tasks);
        }
        public async UniTask InvokeAsyncUniTask(object signal)
        {
            List<UniTask> tasks = new();

            foreach (KeyValuePair<int, List<Func<object, UniTask>>> receivers in _uniTaskReceivers.OrderByDescending(
                         kvp => kvp.Key))
                foreach (Func<object, UniTask> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await UniTask.WhenAll(tasks);
        }
        public bool HasReceivers() =>
            SignalStyle switch
            {
                SignalStyle.Normal => _receivers.Any(receivers => receivers.Value.Count > 0),
                SignalStyle.Task => _taskReceivers.Any(receivers => receivers.Value.Count > 0),
                SignalStyle.UniTask => _uniTaskReceivers.Any(receivers => receivers.Value.Count > 0),
                _ => false
            };
        #endregion
    }
}
