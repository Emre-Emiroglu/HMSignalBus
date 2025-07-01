using System;
using System.Collections.Generic;
using System.Linq;

namespace HMSignalBus.Runtime
{
    /// <summary>
    /// Represents a binding for signals, managing subscribers and their priorities.
    /// </summary>
    public sealed class SignalBinding
    {
        #region ReadonlyFields
        private readonly SortedDictionary<int, List<Delegate>> _receivers = new();
        #endregion

        #region Executes
        /// <summary>
        /// Adds a receiver for the signal with a specified priority.
        /// </summary>
        /// <param name="receiver">The delegate to be invoked when the signal is fired.</param>
        /// <param name="priority">Priority of the receiver.</param>
        public void Add(Delegate receiver, int priority = 0)
        {
            if (!_receivers.ContainsKey(priority))
                _receivers[priority] = new List<Delegate>();
                    
            _receivers[priority].Add(receiver);
        }
        
        /// <summary>
        /// Removes a receiver from the signal's invocation list.
        /// </summary>
        /// <param name="receiver">The delegate to remove.</param>
        /// <param name="priority">Priority of the receiver.</param>
        public void Remove(Delegate receiver, int priority = 0)
        {
            if (!_receivers.TryGetValue(priority, out List<Delegate> receivers))
                return;
            
            receivers.Remove(receiver);
                        
            if (_receivers[priority].Count == 0) 
                _receivers.Remove(priority);
        }
        
        /// <summary>
        /// Invokes all subscribers synchronously.
        /// </summary>
        /// <param name="signal">The signal instance.</param>
        public void Invoke<TSignal>(TSignal signal)
        {
            foreach (KeyValuePair<int, List<Delegate>> receivers in _receivers.OrderByDescending(kvp => kvp.Key))
                foreach (Delegate receiver in receivers.Value)
                   ((Action<TSignal>)receiver).Invoke(signal);
        }

        /// <summary>
        /// Determines whether there are any subscribers to the signal.
        /// </summary>
        /// <returns>True if there are subscribers; otherwise, false.</returns>
        public bool HasReceivers() => _receivers.Any(receivers => receivers.Value.Count > 0);
        #endregion
    }
}
