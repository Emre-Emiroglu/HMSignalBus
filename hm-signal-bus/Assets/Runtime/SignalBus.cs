using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeCatGames.HMSignalBus.Runtime
{
    public sealed class SignalBus
    {
        #region ReadonlyFields
        private readonly ConcurrentDictionary<Type, SignalSubscription> _subscriptions = new();
        #endregion
        
        #region Executes
        public void Subscribe<TSignal>(Action<TSignal> receiver, int priority = 0)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowNotDeclaredException(signalType);
            else
                subscription.Add(receiver, priority);
        }
        public void Unsubscribe<TSignal>(Action<TSignal> receiver, int priority = 0)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowNotDeclaredException(signalType);
            else
                subscription.Remove(receiver, priority);
        }
        public void DeclareSignal<TSignal>(SignalStyle signalStyle = SignalStyle.Normal)
        {
            Type signalType = typeof(TSignal);

            if (_subscriptions.ContainsKey(signalType))
                ThrowMultipleDeclareException(signalType);
            else
                _subscriptions[signalType] = new SignalSubscription(signalStyle);
        }
        public async Task Fire<TSignal>(TSignal signal)
        {
            Type signalType = typeof(TSignal);

            if (!_subscriptions.TryGetValue(signalType, out SignalSubscription subscription))
                ThrowNotDeclaredException(signalType);
            else if (!subscription.HasReceivers())
                LogNoSubscriberWarning(signalType);
            else
            {
                try
                {
                    switch (subscription.SignalStyle)
                    {
                        case SignalStyle.Normal:
                            subscription.Invoke(signal);
                            break;
                        case SignalStyle.Task:
                            await subscription.InvokeAsyncTask(signal);
                            break;
                        case SignalStyle.UniTask:
                            await subscription.InvokeAsyncUniTask(signal);
                            break;
                        default:
                            subscription.Invoke(signal);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ThrowFireErrorException(signalType, ex);
                }
            }
        }
        private void ThrowFireErrorException(Type signalType, Exception exception) =>
            throw new Exception($"Error while firing signal '{signalType.Name}': {exception.Message}");
        private void ThrowNotDeclaredException(Type signalType) =>
            throw new InvalidOperationException(
                $"Signal '{signalType.Name}' has not been declared. Please declare it.");
        private static void ThrowMultipleDeclareException(Type signalType) =>
            throw new InvalidOperationException($"Signal '{signalType.Name}' has already been declared.");
        private static void LogNoSubscriberWarning(Type signalType) =>
            Debug.LogWarning($"No subscribers for signal '{signalType.Name}'.");
        #endregion
    }
}
