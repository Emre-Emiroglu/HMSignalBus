# HMSignalBus
HMSignalBus is an event-driven package based on the pub-sub pattern that ensures loosely coupled communication between classes and components. When a signal is fired, all classes and components that subscribe to this signal are notified. In this way, it provides loose coupling between classes and components and provides a more flexible and sustainable architecture.

## Features
SignalBus manages signal declarations, subscriptions, and firing.
* Declare: Declares a new signal type.
* Subscribe: Allows subscribing to a signal. When the signal is fired, the specified callback is invoked. The priority parameter allows you to specify which of the subscribers will be notified first.
* Unsubscribe: Allows unsubscribing from a signal. If you have changed the priority parameter when subscribing to a signal, you must specify the same value when unsubscribing.
* Fire: Fires a signal and notifies all subscribers.

SignalBusUtilities is a static helper class designed to centrally manage the core functions of the SignalBus.
* Construct the SignalBus: It holds a SignalBus field and is responsible for ensuring the proper construction of SignalBus. SignalBus needs to be constructed before the classes or components that will use it. This requirement is addressed in the Initialize() method of SignalBusUtilities, which is marked with the `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` attribute. This ensures that SignalBus is automatically constructed before the scene loads.
* Centrally manage the core functions of the SignalBus: Classes and components that wish to utilize SignalBus methods can simply call the methods of SignalBusUtilities, which are named the same as the SignalBus methods . This provides a convenient and efficient way to use the SignalBus.

SignalBinding is responsible for managing the subscription and invocation of signals. It is used internally by SignalBus. It allows prioritizing subscribers and ensures that signals are delivered in the correct order.
* Add: Adds a receiver for the signal with a specified priority.
* Remove: Removes a receiver from the signal's invocation list. If the receiver has been subscribed with a priority, the same priority must be used when removing it.
* Invoke: Calls all subscribed methods in order of priority when a signal is fired.
* HasReceivers: Checks whether the signal has any active subscribers.

## Getting Started
Clone the repository
```bash
  git clone https://github.com/Emre-Emiroglu/HMSignalBus.git
```
This project is developed using Unity version 6000.0.42f1.

## Usage
To use the HMSignalBus package, you first need to declare a signal type. Then, you can subscribe to the signal and trigger it when necessary.
* Example usage:
  * Signal Declare: Declare your signal type:
    ```csharp
    SignalBusUtilities.DeclareSignal<MySignal>();
    ```
  * Subscribing (Subscribe): Subscribe to the signal:
    ```csharp
    SignalBusUtilities.Subscribe<MySignal>(OnSignalReceived);
    
    SignalBusUtilities.Subscribe<MySignal>(OnSignalReceived, 1);
    ```
  * Unsubscribing (Unsubscribe): Unsubscribe from the signal:
    ```csharp
    SignalBusUtilities.Unsubscribe<MySignal>(OnSignalReceived);
    
    SignalBusUtilities.Unsubscribe<MySignal>(OnSignalReceived, 1);
    ```
  * Firing the Signal (Fire): Fire the signal and notify subscribers:
    ```csharp
    SignalBusUtilities.Fire(new MySignal());
    ```
* Error handling:
    * If a signal is declared multiple times, an exception `ThrowMultipleDeclareException` is thrown.
    * If a signal is used without being declared, `ThrowNotDeclaredException` is thrown.
    * If a fired signal has no subscribers, a warning `LogNoSubscriberWarning` is logged.

## Acknowledgments
Special thanks to the Unity community for their invaluable resources and tools.

For more information, visit the GitHub repository.