using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeCatGames.HSignalBus.Runtime
{
    public sealed class SignalBus
    {
        #region ReadonlyFields
        private readonly Dictionary<Type, SignalSubscription> _subscriptions = new();
        #endregion
        
        #region Executes
        public void Subscribe<TSignal>(Action<TSignal> receiver) => SubscribeProcess(receiver);
        public void Unsubscribe<TSignal>(Action<TSignal> receiver) => UnsubscribeProcess(receiver);
        public void DeclareSignal<TSignal>() => DeclareProcess<TSignal>();
        public void Fire<TSignal>(TSignal signal) => FireProcess(signal);
        private void SubscribeProcess<TSignal>(Action<TSignal> receiver)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowInvalidOperationException(signalType);
            else
                subscription.Add(receiver);
        }
        private void UnsubscribeProcess<TSignal>(Action<TSignal> receiver)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowInvalidOperationException(signalType);
            else
                subscription.Remove(receiver);
        }
        private void DeclareProcess<TSignal>(SignalSyncType syncType = SignalSyncType.Synchronous,
            SignalStyleType styleType = SignalStyleType.Normal)
        {
            Type signalType = typeof(TSignal);

            if (_subscriptions.ContainsKey(signalType))
                ThrowMultipleDeclareWarning(signalType);
            else
                _subscriptions[signalType] = new SignalSubscription(signalType, syncType, styleType);
        }
        private void FireProcess<TSignal>(TSignal signal)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowInvalidOperationException(signalType);
            else
                if (subscription.HasHandlers())
                    subscription.Invoke(signal);
                else
                    ThrowNoSubscribeWarning(signalType);
        }
        private void ThrowInvalidOperationException(Type signalType) =>
            throw new InvalidOperationException(
                $"Signal '{signalType.Name}' has not been declared. Please declare it.");
        private static void ThrowMultipleDeclareWarning(Type signalType) => Debug.LogWarning(
            $"Signal '{signalType.Name}' has already been declared. Please be careful not to declare it more than once.");
        private static void ThrowNoSubscribeWarning(Type signalType) =>
            Debug.LogWarning($"No subscribers for signal '{signalType.Name}'.");
        #endregion
    }
}