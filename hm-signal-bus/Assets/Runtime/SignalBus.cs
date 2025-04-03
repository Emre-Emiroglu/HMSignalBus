using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

namespace CodeCatGames.HMSignalBus.Runtime
{
    /// <summary>
    /// Manages signal declarations, subscriptions, and firing within the SignalBus system.
    /// </summary>
    public sealed class SignalBus
    {
        #region ReadonlyFields
        private readonly ConcurrentDictionary<Type, SignalBinding> _bindings = new();
        #endregion
        
        #region Executes
        /// <summary>
        /// Declares a signal for use in the SignalBus system.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to declare.</typeparam>
        /// <param name="signalBindingStyle">The binding style for the signal.</param>
        public void DeclareSignal<TSignal>(SignalBindingStyle signalBindingStyle = SignalBindingStyle.Normal)
        {
            Type signalType = typeof(TSignal);

            if (_bindings.ContainsKey(signalType))
                SignalBusUtilities.ThrowMultipleDeclareException(signalType);
            else
                _bindings[signalType] = new SignalBinding(signalBindingStyle);
        }
        
        /// <summary>
        /// Subscribes a receiver to a signal.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The action to invoke when the signal is fired.</param>
        /// <param name="priority">Priority level of the subscriber.</param>
        public void Subscribe<TSignal>(Action<TSignal> receiver, int priority = 0)
        {
            Type signalType = typeof(TSignal);

            if (!_bindings.TryGetValue(signalType, out SignalBinding binding))
                SignalBusUtilities.ThrowNotDeclaredException(signalType);
            else
                binding.Add(receiver, priority);
        }
        
        /// <summary>
        /// Unsubscribes a receiver from a signal.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The action to remove from the signal invocation list.</param>
        /// <param name="priority">Priority level of the subscriber.</param>
        public void Unsubscribe<TSignal>(Action<TSignal> receiver, int priority = 0)
        {
            Type signalType = typeof(TSignal);

            if (!_bindings.TryGetValue(signalType, out SignalBinding binding))
                SignalBusUtilities.ThrowNotDeclaredException(signalType);
            else
                binding.Remove(receiver, priority);
        }
        
        /// <summary>
        /// Fires a signal asynchronously and notifies all subscribers.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to fire.</typeparam>
        /// <param name="signal">The signal instance to send.</param>
        public async UniTask Fire<TSignal>(TSignal signal)
        {
            Type signalType = typeof(TSignal);

            if (!_bindings.TryGetValue(signalType, out SignalBinding binding))
                SignalBusUtilities.ThrowNotDeclaredException(signalType);
            else if (!binding.HasReceivers())
                SignalBusUtilities.LogNoSubscriberWarning(signalType);
            else
            {
                try
                {
                    switch (binding.SignalBindingStyle)
                    {
                        case SignalBindingStyle.Normal:
                            binding.Invoke(signal);
                            break;
                        case SignalBindingStyle.Task:
                            await binding.InvokeAsyncTask(signal);
                            break;
                        case SignalBindingStyle.UniTask:
                            await binding.InvokeAsyncUniTask(signal);
                            break;
                        default:
                            binding.Invoke(signal);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    SignalBusUtilities.ThrowFireErrorException(signalType, ex);
                }
            }
        }
        #endregion
    }
}
