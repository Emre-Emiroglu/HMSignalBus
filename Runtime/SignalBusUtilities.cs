using System;
using UnityEngine;

namespace CodeCatGames.HMSignalBus.Runtime
{
    /// <summary>
    /// Provides utility methods for managing the SignalBus.
    /// Handles initialization and common operations related to signals.
    /// </summary>
    public static class SignalBusUtilities
    {
        #region Fields
        private static SignalBus _signalBus;
        #endregion
        
        #region Core
        /// <summary>
        /// Initializes the SignalBus.
        /// </summary>
        public static void Initialize() => _signalBus = new SignalBus();
        #endregion

        #region Executes
        /// <summary>
        /// Declares a signal type for use in the SignalBus.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to declare.</typeparam>
        public static void DeclareSignal<TSignal>() => _signalBus.DeclareSignal<TSignal>();
        
        /// <summary>
        /// Subscribes a receiver to a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The action to invoke when the signal is fired.</param>
        /// <param name="priority">Priority level of the subscriber.</param>
        public static void Subscribe<TSignal>(Action<TSignal> receiver, int priority = 0) =>
            _signalBus.Subscribe(receiver, priority);
        
        /// <summary>
        /// Unsubscribes a receiver from a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The action to remove from the signal invocation list.</param>
        /// <param name="priority">Priority level of the subscriber.</param>
        public static void Unsubscribe<TSignal>(Action<TSignal> receiver, int priority = 0) =>
            _signalBus.Unsubscribe(receiver, priority);
        
        /// <summary>
        /// Fires a signal and notifies all subscribers.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to fire.</typeparam>
        /// <param name="signal">The signal instance to send.</param>
        public static void Fire<TSignal>(TSignal signal) => _signalBus.Fire(signal);
        
        /// <summary>
        /// Throws an exception when a signal is declared multiple times.
        /// </summary>
        /// <param name="signalType">The type of signal that was declared multiple times.</param>
        public static void ThrowMultipleDeclareException(Type signalType) =>
            throw new InvalidOperationException($"Signal '{signalType.Name}' has already been declared.");
        
        /// <summary>
        /// Throws an exception when trying to use an undeclared signal.
        /// </summary>
        /// <param name="signalType">The type of signal that was not declared.</param>
        public static void ThrowNotDeclaredException(Type signalType) =>
            throw new InvalidOperationException(
                $"Signal '{signalType.Name}' has not been declared. Please declare it.");
        
        /// <summary>
        /// Logs a warning when a signal is fired but has no subscribers.
        /// </summary>
        /// <param name="signalType">The type of signal that has no subscribers.</param>
        public static void LogNoSubscriberWarning(Type signalType) =>
            Debug.LogWarning($"No subscribers for signal '{signalType.Name}'.");
        #endregion
    }
}