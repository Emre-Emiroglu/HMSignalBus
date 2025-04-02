using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        public async void Fire<TSignal>(TSignal signal) => await FireProcess(signal);
        private void SubscribeProcess<TSignal>(Action<TSignal> receiver)
        {
            Type type = typeof(TSignal);

            if (!_subscriptions.TryGetValue(type, out SignalSubscription subscription))
                ThrowInvalidOperationException(type);
            else
                subscription.Add(receiver);
        }
        private void UnsubscribeProcess<TSignal>(Action<TSignal> receiver)
        {
            Type type = typeof(TSignal);

            if (!_subscriptions.TryGetValue(type, out SignalSubscription subscription))
                ThrowInvalidOperationException(type);
            else
                subscription.Remove(receiver);
        }
        private void DeclareProcess<TSignal>(SignalMode signalMode = SignalMode.Synchronous,
            SignalType signalType = SignalType.Normal)
        {
            Type type = typeof(TSignal);

            if (_subscriptions.ContainsKey(type))
                ThrowMultipleDeclareWarning(type);
            else
                _subscriptions[type] = new SignalSubscription(type, signalMode, signalType);
        }
        private async Task FireProcess<TSignal>(TSignal signal)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowInvalidOperationException(signalType);
            else
            {
                switch (subscription.SignalMode)
                {
                    case SignalMode.Synchronous:
                        switch (subscription.SignalType)
                        {
                            case SignalType.Normal:
                                if (subscription.HasReceivers())
                                {
                                    SynchronousFireProcess(signal, subscription);
                                }
                                else
                                {
                                    ThrowNoSubscribeWarning(signalType);
                                }
                                break;
                        }
                        break;
                    case SignalMode.Asynchronous:
                        switch (subscription.SignalType)
                        {
                            case SignalType.Task:
                                if (subscription.HasAsyncTaskReceivers())
                                {
                                    await AsynchronousTaskFireProcess(signal, subscription);
                                }
                                else
                                {
                                    ThrowNoSubscribeWarning(signalType); 
                                }
                                break;
                            case SignalType.UniTask:
                                if (subscription.HasAsyncUniTaskReceivers())
                                {
                                    await AsynchronousUniTaskFireProcess(signal, subscription);
                                }
                                else
                                {
                                    ThrowNoSubscribeWarning(signalType); 
                                }
                                break;
                        }
                        break;
                }
            }
        }
        private void SynchronousFireProcess<TSignal>(TSignal signal, SignalSubscription subscription) => subscription.InvokeSyncNormal(signal);
        private Task AsynchronousTaskFireProcess<TSignal>(TSignal signal, SignalSubscription subscription) => subscription.InvokeAsyncTask(signal);
        private UniTask AsynchronousUniTaskFireProcess<TSignal>(TSignal signal, SignalSubscription subscription) => subscription.InvokeAsyncUniTask(signal);
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