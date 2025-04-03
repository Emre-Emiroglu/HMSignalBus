using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeCatGames.HSignalBus.Runtime
{
    public sealed class SignalSubscription
    {
        #region ReadonlyFields
        private readonly SortedDictionary<int, List<Delegate>> _receivers = new();
        private readonly SortedDictionary<int, List<Func<object, Task>>> _asyncTaskReceivers = new();
        private readonly SortedDictionary<int, List<Func<object, UniTask>>> _asyncUniTaskReceivers = new();
        #endregion

        #region Getters
        public SignalType SignalType { get; }
        #endregion
        
        #region Constructors
        public SignalSubscription(SignalType signalType = SignalType.Normal) => SignalType = signalType;
        #endregion

        #region Executes
        public void Add(Delegate receiver, int priority = 0)
        {
            switch (receiver)
            {
                case Func<object, Task> asyncTaskReceiver:
                    if (!_asyncTaskReceivers.ContainsKey(priority))
                        _asyncTaskReceivers[priority] = new List<Func<object, Task>>();
                    
                    _asyncTaskReceivers[priority].Add(asyncTaskReceiver);
                    break;
                case Func<object, UniTask> asyncUniTaskReceiver:
                    if (!_asyncUniTaskReceivers.ContainsKey(priority))
                        _asyncUniTaskReceivers[priority] = new List<Func<object, UniTask>>();
                    
                    _asyncUniTaskReceivers[priority].Add(asyncUniTaskReceiver);
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
                case Func<object, Task> asyncTaskReceiver:
                    if (_asyncTaskReceivers.ContainsKey(priority))
                    {
                        _asyncTaskReceivers[priority].Remove(asyncTaskReceiver);
                        
                        if (_asyncTaskReceivers[priority].Count == 0) 
                            _asyncTaskReceivers.Remove(priority);
                    }
                    break;
                case Func<object, UniTask> asyncUniTaskReceiver:
                    if (_asyncUniTaskReceivers.ContainsKey(priority))
                    {
                        _asyncUniTaskReceivers[priority].Remove(asyncUniTaskReceiver);
                        
                        if (_asyncUniTaskReceivers[priority].Count == 0) 
                            _asyncUniTaskReceivers.Remove(priority);
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
            foreach (KeyValuePair<int, List<Delegate>> receivers in _receivers.OrderByDescending(k => k.Key))
                foreach (Delegate receiver in receivers.Value)
                    ((Action<object>)receiver)(signal);
        }
        public async Task InvokeAsyncTask(object signal)
        {
            List<Task> tasks = new();
            foreach (KeyValuePair<int, List<Func<object, Task>>> receivers in _asyncTaskReceivers.OrderByDescending(k =>
                         k.Key))
                foreach (Func<object, Task> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await Task.WhenAll(tasks);
        }
        public async UniTask InvokeAsyncUniTask(object signal)
        {
            List<UniTask> tasks = new();
            foreach (KeyValuePair<int, List<Func<object, UniTask>>> receivers in _asyncUniTaskReceivers
                         .OrderByDescending(k => k.Key))
                foreach (Func<object, UniTask> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await UniTask.WhenAll(tasks);
        }
        public bool HasReceivers() =>
            SignalType switch
            {
                SignalType.Normal => _receivers.Any(receivers => receivers.Value.Count > 0),
                SignalType.Task => _asyncTaskReceivers.Any(receivers => receivers.Value.Count > 0),
                SignalType.UniTask => _asyncUniTaskReceivers.Any(receivers => receivers.Value.Count > 0),
                _ => false
            };
        #endregion
    }
}
