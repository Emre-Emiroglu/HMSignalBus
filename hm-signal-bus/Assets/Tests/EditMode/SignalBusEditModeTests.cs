using System;
using CodeCatGames.HMSignalBus.Runtime;
using NUnit.Framework;

namespace CodeCatGames.HMSignalBus.Tests.EditMode
{
    public sealed class SignalBusEditModeTests
    {
        private SignalBus _signalBus;

        [SetUp]
        public void SetUp() => _signalBus = new SignalBus();

        [Test]
        public void DeclareSignal_ShouldStoreSignalBinding()
        {
            _signalBus.DeclareSignal<TestSignal>();
            
            Assert.DoesNotThrow(() => _signalBus.Subscribe<TestSignal>(_ => { }));
        }

        [Test]
        public void DeclareSignal_DuplicateDeclaration_ShouldThrowException()
        {
            _signalBus.DeclareSignal<TestSignal>();
            
            Assert.Throws<InvalidOperationException>(() => _signalBus.DeclareSignal<TestSignal>());
        }

        [Test]
        public void Subscribe_Unsubscribe_ShouldNotThrow()
        {
            _signalBus.DeclareSignal<TestSignal>();
            
            Assert.DoesNotThrow(() => _signalBus.Subscribe<TestSignal>(Receiver));
            
            Assert.DoesNotThrow(() => _signalBus.Unsubscribe<TestSignal>(Receiver));
        }

        [Test]
        public void Subscribe_WithoutDeclaration_ShouldThrowException() =>
            Assert.Throws<InvalidOperationException>(() => _signalBus.Subscribe<TestSignal>(_ => { }));

        [Test]
        public void Unsubscribe_WithoutDeclaration_ShouldThrowException() =>
            Assert.Throws<InvalidOperationException>(() => _signalBus.Unsubscribe<TestSignal>(_ => { }));
        
        private static void Receiver(TestSignal signal) { }

        private class TestSignal {}
    }
}