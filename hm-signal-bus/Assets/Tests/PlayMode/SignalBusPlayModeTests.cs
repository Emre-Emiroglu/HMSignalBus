using CodeCatGames.HMSignalBus.Runtime;
using NUnit.Framework;

namespace CodeCatGames.HMSignalBus.Tests.PlayMode
{
    public sealed class SignalBusPlayModeTests
    {
        private SignalBus _signalBus;

        [SetUp]
        public void SetUp() => _signalBus = new SignalBus();


        private class TestSignal
        {
            public int Value { get; }

            public TestSignal(int value) => Value = value;
        }
    }
}