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
        private readonly Type _type;
        private readonly SignalMode _signalMode;
        private readonly SignalType _signalType;
        private readonly List<Delegate> _receivers = new();
        private readonly List<Func<object, Task>> _asyncTaskReceivers = new();
        private readonly List<Func<object, UniTask>> _asyncUniTaskReceivers = new();
        #endregion

        #region Getters
        public Type Type => _type;
        public SignalMode SignalMode => _signalMode;
        public SignalType SignalType => _signalType;
        public bool HasReceivers() => _receivers.Count > 0;
        public bool HasAsyncTaskReceivers() => _asyncTaskReceivers.Count > 0;
        public bool HasAsyncUniTaskReceivers() => _asyncUniTaskReceivers.Count > 0;
        #endregion
        
        #region Constructors
        public SignalSubscription(Type type, SignalMode signalMode, SignalType signalType)
        {
            _type = type;
            _signalMode = signalMode;
            _signalType = signalType;
        }
        #endregion

        #region Executes
        public void Add(Delegate receiver)
        {
            switch (receiver)
            {
                case Func<object, Task> asyncTaskReceiver:
                    _asyncTaskReceivers.Add(asyncTaskReceiver);
                    break;
                case Func<object, UniTask> asyncUniTaskReceiver:
                    _asyncUniTaskReceivers.Add(asyncUniTaskReceiver);
                    break;
                default:
                    _receivers.Add(receiver);
                    break;
            }
        }
        public void Remove(Delegate receiver)
        {
            switch (receiver)
            {
                case Func<object, Task> asyncTaskReceiver:
                    _asyncTaskReceivers.Remove(asyncTaskReceiver);
                    break;
                case Func<object, UniTask> asyncUniTaskReceiver:
                    _asyncUniTaskReceivers.Remove(asyncUniTaskReceiver);
                    break;
                default:
                    _receivers.Remove(receiver);
                    break;
            }
        }
        public void InvokeSyncNormal(object signal) => _receivers.ForEach(receiver => receiver.DynamicInvoke(signal));
        public async Task InvokeAsyncTask(object signal)
        {
            List<Task> tasks = _asyncTaskReceivers.Select(asyncTaskHandler => asyncTaskHandler.Invoke(signal)).ToList();

            await Task.WhenAll(tasks);
        }
        public async UniTask InvokeAsyncUniTask(object signal)
        {
            List<UniTask> tasks = Enumerable
                .Select(_asyncUniTaskReceivers, asyncUniTaskHandler => asyncUniTaskHandler.Invoke(signal)).ToList();

            await UniTask.WhenAll(tasks);
        }
        #endregion
    }
}