using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CodeCatGames.HSignalBus.Runtime
{
    public sealed class SignalSubscription
    {
        #region ReadonlyFields
        private readonly List<Delegate> _receivers = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Type _type;
        private readonly SignalSyncType _syncType;
        private readonly SignalStyleType _styleType;
        #endregion
        
        #region Getters
        public Type Type => _type;
        public SignalSyncType SyncType => _syncType;
        public SignalStyleType StyleType => _styleType;
        #endregion

        #region Constructor
        public SignalSubscription(Type type, SignalSyncType syncType = SignalSyncType.Synchronous,
            SignalStyleType styleType = SignalStyleType.Normal)
        {
            _type = type;
            _syncType = syncType;
            _styleType = styleType;
        }
        #endregion

        #region Executes
        public void Add(Delegate handler)
        {
            _lock.EnterWriteLock();
            try
            {
                _receivers.Add(handler);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void Remove(Delegate handler)
        {
            _lock.EnterWriteLock();
            try
            {
                _receivers.Remove(handler);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void Invoke(object signal)
        {
            Delegate[] handlersCopy;
            _lock.EnterReadLock();
            try
            {
                handlersCopy = _receivers.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (Delegate handler in handlersCopy)
            {
                try
                {
                    handler.DynamicInvoke(signal);
                }
                catch (Exception ex) when (ex.InnerException != null)
                {
                    Debug.LogError(ex.InnerException);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }
        public bool HasHandlers()
        {
            _lock.EnterReadLock();
            try
            {
                return _receivers.Count > 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        #endregion
    }
}