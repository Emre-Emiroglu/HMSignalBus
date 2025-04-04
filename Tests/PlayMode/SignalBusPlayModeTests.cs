using System;
using System.Collections;
using CodeCatGames.HMSignalBus.Runtime;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CodeCatGames.HMSignalBus.Tests.PlayMode
{
    public sealed class SignalBusPlayModeTests
    {
        private SignalBus _signalBus;

        [SetUp]
        public void SetUp() => _signalBus = new SignalBus();

        [UnityTest]
        public IEnumerator Fire_Signal_ShouldInvokeSubscriber()
        {
            int receivedValue = 0;

            _signalBus.DeclareSignal<TestSignal>();

            _signalBus.Subscribe<TestSignal>(receiver => receivedValue = receiver.Value);
            
            _signalBus.Fire(new TestSignal(42));

            yield return null;
            
            Assert.AreEqual(42, receivedValue);
        }
        
        [UnityTest]
        public IEnumerator Fire_MultipleSignals_ShouldInvokeSubscribersMultipleTimes()
        {
            int[] receivedValues = new int[3];
            
            _signalBus.DeclareSignal<TestSignal>();
            
            _signalBus.Subscribe<TestSignal>(receiver => receivedValues[0] = receiver.Value);
            _signalBus.Subscribe<TestSignal>(receiver => receivedValues[1] = receiver.Value);
            _signalBus.Subscribe<TestSignal>(receiver => receivedValues[2] = receiver.Value);
        
            _signalBus.Fire(new TestSignal(42));
            _signalBus.Fire(new TestSignal(24));

            yield return null;
            
            Assert.AreEqual(24, receivedValues[0]);
            Assert.AreEqual(24, receivedValues[1]);
            Assert.AreEqual(24, receivedValues[2]);
        }
        
        [UnityTest]
        public IEnumerator Fire_Signal_WithPriority_ShouldInvokeSubscribersInPriorityOrder()
        {
            int[] callOrder = new int[3];
            int timestamp = 0;
            
            _signalBus.DeclareSignal<TestSignal>();
            
            _signalBus.Subscribe<TestSignal>(_ => callOrder[0] = timestamp++, priority: 0);
            _signalBus.Subscribe<TestSignal>(_ => callOrder[1] = timestamp++, priority: 1);
            _signalBus.Subscribe<TestSignal>(_ => callOrder[2] = timestamp++, priority: 2);
        
            _signalBus.Fire(new TestSignal(42));

            yield return null;
            
            Assert.AreEqual(0, callOrder[2]);
            Assert.AreEqual(1, callOrder[1]);
            Assert.AreEqual(2, callOrder[0]);
        }
        
        [UnityTest]
        public IEnumerator Fire_Signal_WithoutSubscribers_ShouldNotCauseError()
        {
            _signalBus.DeclareSignal<TestSignal>();
            
            _signalBus.Fire(new TestSignal(42));

            yield return null;
        }
        
        [UnityTest]
        public IEnumerator Fire_UndeclaredSignal_ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() => _signalBus.Fire(new TestSignal(42)));
            
            yield return null;
        }

        private class TestSignal
        {
            public int Value { get; }

            public TestSignal(int value) => Value = value;
        }
    }
}