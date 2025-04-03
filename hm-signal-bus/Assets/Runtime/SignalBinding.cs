using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeCatGames.HMSignalBus.Runtime
{
    /// <summary>
    /// Represents a binding for signals, managing subscribers and their priorities.
    /// </summary>
    public sealed class SignalBinding
    {
        #region ReadonlyFields
        private readonly SortedDictionary<int, List<Delegate>> _receivers = new();
        private readonly SortedDictionary<int, List<Func<object, Task>>> _taskReceivers = new();
        private readonly SortedDictionary<int, List<Func<object, UniTask>>> _uniTaskReceivers = new();
        #endregion

        #region Getters
        /// <summary>
        /// Gets the signal binding style.
        /// </summary>
        public SignalBindingStyle SignalBindingStyle { get; }
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalBinding"/> class with the specified binding style.
        /// </summary>
        /// <param name="signalBindingStyle">The binding style for this signal.</param>
        public SignalBinding(SignalBindingStyle signalBindingStyle = SignalBindingStyle.Normal) =>
            SignalBindingStyle = signalBindingStyle;
        #endregion

        #region Executes
        /// <summary>
        /// Adds a receiver for the signal with a specified priority.
        /// </summary>
        /// <param name="receiver">The delegate to be invoked when the signal is fired.</param>
        /// <param name="priority">Priority of the receiver.</param>
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
        
        /// <summary>
        /// Removes a receiver from the signal's invocation list.
        /// </summary>
        /// <param name="receiver">The delegate to remove.</param>
        /// <param name="priority">Priority of the receiver.</param>
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
        
        /// <summary>
        /// Invokes all subscribers synchronously.
        /// </summary>
        /// <param name="signal">The signal instance.</param>
        public void Invoke(object signal)
        {
            foreach (KeyValuePair<int, List<Delegate>> receivers in _receivers.OrderByDescending(kvp => kvp.Key))
                foreach (Delegate receiver in receivers.Value)
                    ((Action<object>)receiver)(signal);
        }
        
        /// <summary>
        /// Invokes all subscribers asynchronously using tasks.
        /// </summary>
        /// <param name="signal">The signal instance.</param>
        public async Task InvokeAsyncTask(object signal)
        {
            List<Task> tasks = new();

            foreach (KeyValuePair<int, List<Func<object, Task>>> receivers in _taskReceivers.OrderByDescending(kvp =>
                         kvp.Key))
                foreach (Func<object, Task> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await Task.WhenAll(tasks);
        }
        
        /// <summary>
        /// Invokes all subscribers asynchronously using UniTask.
        /// </summary>
        /// <param name="signal">The signal instance.</param>
        public async UniTask InvokeAsyncUniTask(object signal)
        {
            List<UniTask> tasks = new();

            foreach (KeyValuePair<int, List<Func<object, UniTask>>> receivers in _uniTaskReceivers.OrderByDescending(
                         kvp => kvp.Key))
                foreach (Func<object, UniTask> receiver in receivers.Value)
                    tasks.Add(receiver.Invoke(signal));

            await UniTask.WhenAll(tasks);
        }
        
        /// <summary>
        /// Determines whether there are any subscribers to the signal.
        /// </summary>
        /// <returns>True if there are subscribers; otherwise, false.</returns>
        public bool HasReceivers() =>
            SignalBindingStyle switch
            {
                SignalBindingStyle.Normal => _receivers.Any(receivers => receivers.Value.Count > 0),
                SignalBindingStyle.Task => _taskReceivers.Any(receivers => receivers.Value.Count > 0),
                SignalBindingStyle.UniTask => _uniTaskReceivers.Any(receivers => receivers.Value.Count > 0),
                _ => false
            };
        #endregion
    }
}
