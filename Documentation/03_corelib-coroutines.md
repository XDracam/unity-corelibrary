# Coroutine Composition Utilities

## Coroutine Basics

Consider the following example why coroutines can be useful: You want to have an object that may be launched only once at some point, making it fly for exactly a specified number of seconds:

```cs
// without coroutines

[RequireComponent(typeof(Rigidbody))]
public class BottleRocket : MonoBehaviour
{
    public float FlightTime;

    private bool _launched = false;
    private float _launchTime = 0; 

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        if (_launched) return; // can't relaunch
        _launched = true;
        _launchTime = Time.time;
    }

    private void FixedUpdate()
    {
        var endTime = _launchTime + FlightTime;
        if (_launched && Time.time < endTime)
        {
            _rb.AddForce(Vector3.up * 1000); // FLY!
        }
    }
}
```

Basically, coroutines in the context of games are functions that execute 'asynchronously' over multiple frames once started. In the above example, the launch code in `FixedUpdate()` only runs under special conditions: Only when the component has been launched (`_launched == true`) and less than `FlightTime` seconds have passed since launch. All the state required for even this very simple case is difficult to overview and therefore error-prone. And even worse, `endTime` is calculated every `FixedUpdate` over the lifetime of the object! Ouch.

```cs
// with coroutines

[RequireComponent(typeof(Rigidbody))]
public class BottleRocket : MonoBehaviour
{
    public float FlightTime;

    private bool _launchedOnce = false;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        StartCoroutine(LaunchRoutine());
    }

    private IEnumerator LaunchRoutine()
    {
        if (_launchedOnce) yield break; // can't relaunch!
        _launchedOnce = true;

        // launch time stays local
        var _launchTime = Time.time;

        while (_launchTime + FlightTime < Time.time)
        {
            _rb.AddForce(Vector3.up * 1000); // FLY!
            yield return new WaitForFixedUpdate();
        }
    }

    private void FixedUpdate()
    {
        /* unnecessary */
    }
}
```

Having a method return `IEnumerator` enables the use of `yield` statements. A `yield return` lazily adds a value to the returned IEnumerator. In the context of game development with Unity, it usually returns a `YieldInstruction`: either `null` for *wait for next frame*, `new WaitForFixedUpdate()` to pause until the next fixedUpdate or `new WaitForSeconds(float)` to pause executions for a number of seconds before continuing the code, among others. Execution of a coroutine method is paused once it reaches a `yield return` and continued once the next value is requested. 

It is important to note that just calling `LaunchRoutine` returns an `IEnumerator`, a list of values that are not yet computed. Before calling `enumerator.MoveNext()`, no code is executed. Unity handles these `.MoveNext()` calls internally and uses the returned `YieldInstruction`s to pause execution as specified by the returned value. For this to work, the coroutine has to be passed to a call of `StartCoroutine`. In the CoreLibrary, we provide the `coroutine.Start()` function, which is just a shorter way for writing `StartCoroutine` with less braces. 

**TL;DR: Never forget to call `StartCoroutine()` or `.Start()`!**

Because forgetting the `StartCoroutine` call is such a common mistake that can't be caught by the type system, it is a *good design practice* to make the coroutine implementation private and provide a public method (here `Launch()`), that just takes care of starting the coroutine.

Now imagine multiple of these method-implementation pairs in a single component: Without a lot of discipline this quickly becomes a mess. Especially when you are having a lot of `private IEnumerator ...` methods that just wait a few seconds before executing code. For reasons like this we provide a number of *coroutine control and composition structures*. These enable you to replace almost all `private IEnumerator ...` methods with *anonymous functions (lambdas)*. Consider the above example with the Core Library:

```cs
// with CoreLibrary features

using static CoreLibrary.Coroutines;

[RequireComponent(typeof(Rigidbody))]
public class BottleRocket : BaseBehaviour
{
    public float FlightTime;

    private bool _launchedOnce = false;
    private Rigidbody _rb;

    private void Start()
    {
        AssignComponent(out _rb);
    }

    public void Launch()
    {
        if (_launchedOnce) return; // can't relaunch!
        _launchedOnce = true;

        // launch time stays local
        var _launchTime = Time.time;

        RepeatWhile(
            () => _launchTime + FlightTime < Time.time,
            () => _rb.AddForce(Vector3.up * 100),
            () => new WaitForFixedUpdate()
        ).Start();
    }

    private void FixedUpdate()
    {
        /* unnecessary */
    }
}
```

The `using static CoreLibrary.Coroutines;` lets you omit the `Coroutines.` prefix in front of every coroutine-related function like the `RepeatWhile` seen above. *All further examples assume you have this line somewhere at the top of your source file*.

The notation `() => doSomething()` is called an *anonymous function* or *lambda*. It is a function object taking no arguments (hence the `()`) and when called executes the code on the right of the `=>`. The core library uses lambdas almost exclusively as **code blocks to execute later**. In the case `RepeatWhile` one must pass two lambdas: the first returns a boolean and is evaluated again before every loop, breaking the loop if it returns false. The second lambda can return whatever: It holds the body of loop. 

An alternative to writing `() => doSomething()` is `() => {return doSomething();}`. By using curly braces after the arrow you can use more than one statement in a lambda, using them to pass more complex blocks of code to our custom control structures.

The `RepeatWhile` and `WaitUntil` methods have an optional last argument of type `Func<object>`. This is a 'getter' - a function which is called every time the condition is checked and which determines what should be `yield return`ed. Since Unity can work with types which extend both `YieldInstruction` as well as `CustomYieldInstruction` without providing a common supertype, the CoreLibrary has to allow any type to be yielded. This is not less safe than writing coroutines the regular way, since Unity itself does not enforce any type safety in coroutine yield results. Since the yielded value could change during execution of the coroutine, the CoreLibrary uses a function and not just a simple value.

The `RepeatForSeconds`, `RepeatForFrames` and `DelayForFrames` methods have an optional `bool fixedUpdate = false` parameter. By default, these functions cause execution to halt until the next `Update` cycle. However, if you want to use these every `FixedUpdate` instead, you can pass `fixedUpdate: true` as last argument. The `fixedUpdate: true` is a named parameter. Prefixing boolean parameters with the argument name makes the code more readable and is often considered a best practice. 

The goal of this module is to *keep the definition in a concurrent task as local as possible*, as the more state a class has, the harder it is to understand what it is actually doing. And having the definition for a single task in one place is much easier to understand than when you have to jump between `private` functions and keep parameters and fields in mind.

The rest of this page explains our coroutine utilities by first presenting some regular Unity code followed by what you would replace it with.

**Hint: No coroutine-related function in the CoreLibrary actually *starts* the coroutine.** They are for composing new IEnumerables which may be later started by calling `enumerable.Start()`.

## StartCoroutine() vs. .Start()

When using `StartCoroutone(myRoutine)`, the execution of the coroutine is *registered to the game object that started it*. This means that when the game object is destroyed or *set inactive*, the coroutine ends. This might be the desired behaviour when you want to implement an infinite coroutine.

In contrast, calling `myRoutine.Start()` registers the execution of the coroutine to a *global CoroutineRunner*. Coroutines started this way only stop when the scene ends.

## DelayForFrames

Imagine you have a button. Every time the button is pressed, a sound should play after exactly `n` frames. If you press the button multiple times before the first sound plays all sounds should play. Building this without coroutine would require a custom queue that keeps track of frame count and similar. 

```cs
// -- before
public void PlaySound(uint n) 
{
    StartCoroutine(DelayPlayingSound(n));
}

private IEnumerator DelayPlayingSound(uint n) 
{
    for (var i = 0; i < n; ++i) yield return new WaitForFixedUpdate();
    sound.Play();
}
```

Now, for every delayed action you'll have to write another method-coroutine-pair, making your code a mess. Consider instead:

```cs
// -- after
public void PlaySound(uint n) => DelayForFrames(n, () => sound.Play(), fixedUpdate: true).Start();
```

C# 6.0 allows you to shorten single-line function bodies into a `declaration => expression;` for readability. This is completely optional, but shorter. This notation is especially useful for providing a lot of simple methods without polluting the class with way too many lines holding only curly braces.

## RepeatForFrames

Sometimes instead of waiting a number of frames you want to repeat an action for a number of continuous frames. This is mostly useful for a small number of frames, like executing an action three times with a frame between each. To be honest I can't think of a useful example, so here is an abstract one:

```cs
// -- before
public void Foo() 
{
    StartCoroutine(FooRoutine());
}

private IEnumerator FooRoutine() 
{
    DoStuff();
    yield return null; 
    DoStuff();
    yield return null;
    DoStuff();
}
```

```cs
// -- after
public void Foo() => RepeatForFrames(3, () => DoStuff());
```

## WaitForSeconds

A very common case is to wait a certain time before executing some action. If fact it is so common that Project Synergy had at least 6 single `private IEnumerator ...` methods just to wait before doing something. 

Imagine you just defeated a boss. You want some stuff to happen like sounds or light effects. Then, after exactly 5 seconds a door should open.

```cs
// -- before
public void InitiateDoorOpening() 
{
    /* Do other stuff like play sound */
    StartCoroutine(OpenDoorRoutine());
}

private IEnumerator OpenDoorRoutine() {
    yield return new WaitForSeconds(5);
    door.Open();
}
```

```cs
// -- after
public void InitiateDoorOpening() 
{
    /* Do other stuff like play sound */
    WaitForSeconds(5, () => door.Open()).Start();
}
```

## RepeatForSeconds

Another very common case is to repeat an action every frame for a number of seconds. The example for this case is the example at the beginning of this page. No need to copy-paste.

## RepeatEverySeconds

The method `RepeatEverySeconds(float interval, CodeBlock action, int? repetitions = null)` differs from the other repeat methods. It exists solely for the use case of repeating a block of code (`action`) every `interval` seconds, either until the coroutine is stopped or until the code has been executed `repetitions` times.

Imagine a simple component that does nothing but cause a light to blink every `BlinkInterval` seconds. But only for `MaxBlinks` times **or** until it is `.Stop()`ed. For example when coding a christmas tree, or whatever. I don't care.

```cs
// -- before
public float BlinkInterval;
public int MaxBlinks;

private bool _stopped = false;
public void Stop() => _stopped = true;

private void Blink() { ... }

private float _lastBlinkTime;
private int _doneBlinks;

private void Update()
{
    if (_stopped || _doneBlinks >= MaxBlinks) return;
    _doneBlinks += 1;

    if (Time.time - _lastBlinkTime > BlinkInterval)
    {
        _lastBlinkTime = Time.time;
        Blink();
    }
}
```

```cs
// -- after
public float BlinkInterval;
public int MaxBlinks;

private bool _stopped = false;
public void Stop() => _stopped = true;

private void Blink() { ... }

private IEnumerator Start() =>
    RepeatEverySeconds(BlinkInterval, () => Blink(), MaxBlinks)
    .YieldWhile(() => !_stopped);
```

The only **disadvantage** of using `RepeatEverySeconds` is that both the `interval` and the number of `repetitions` **cannot be changed** afterwards.

## WaitUntil

It sometimes happens that you want to *schedule* some code for execution once a certain condition becomes true. For example, you are building an RPG and the player just defeated a boss, but you only want him/her to progress once he/she reaches a certain level. Then the door to the next area opens.

```cs
// -- before
public class NextAreaDoor : MonoBehaviour
{
    public BossEnemy Boss;
    public int MinLevel;
    private bool _opened = false;
    private void Update()
    {
        if (!_opened 
            && Boss.IsDefeated 
            && Player.Instance.Level >= MinLevel) 
        {
            GetComponent<Door>().Open();
            _opened = false;
        }
    }
}
```

In this code, we don't even use a coroutine. Some door component keeps checking every frame whether it can finally open. It's a whole file just dedicated to this single piece of logic. You can't put it into the boss logic because the player might not have the required level after defeating him. Or can you?

```cs
// -- after
public class FirstAreaBoss : BossEnemy
{
    public Door NextAreaDoor;
    public int MinLevel;
    
    public float Hp { get; private set; }

    /* some other fields and state and methods and stuff */

    public void TakeDamage(float amount)
    {
        Hp -= CalcActualDamage(amount);
        if (Hp > 0) 
        {
            /* animations, sound... */
        }
        else 
        {
            /* death animations, sound... */
            WaitUntil(
                () => Player.Instance.Level >= MinLevel, 
                () => Door.Open()
            ).Start();
        }
    }
}
```

Now the repeated level checks are abstracted into an anonymous coroutine that runs in parallel until it is ready. One whole file less. 

## RepeatWhile

Sometimes you want to repeat something until a certain condition is met. Consider a heat tracking missile that follows a target until it hits it.

```cs
// -- before

public Transform Target;

public float SpeedInMPS = 100;

public void LockAndFollow() 
{
    StartCoroutine(FollowRoutine());
}

private IEnumerator FollowRoutine()
{
    while (Vector3.Distance(Target.position, transform.position) < 5) 
    {
        var goalDir = Target.position - transform.position;
        var dir = Vector3.Slerp(transform.forward, goalDir, .2f);
        transform.forward = dir;
        transform.position += 
            transform.forward.normalized * SpeedInMPS * Time.deltaTime;
        yield return null;
    }
    Explode();
}
```

```cs
// -- after

public Transform Target;

public float SpeedInMPS = 100;

public void LockAndFollow() 
{
    RepeatWhile(() => {
        return Vector3.Distance(Target.position, transform.position) < 5;
    }, () => {
        var goalDir = Target.position - transform.position;
        var dir = Vector3.Slerp(transform.forward, goalDir, .2f);
        transform.forward = dir;
        transform.position += 
            transform.forward.normalized * SpeedInMPS * Time.deltaTime;
    }).Afterwards(() => Explode());
}
```

`RepeatWhile` does *not* execute the passed action when the condition is already `false`.

## .Afterwards and .YieldWhile

The above example also shows the usage of `.Afterwards(code)`, which can be called on an existing IEnumerator (= not started coroutine). The passed code is executed after the coroutine it is called on runs out, regardless of how it ends. **Even if an exception is thrown!** `.Afterwards` is often used for some cleanup code. The resulting IEnumerator must still be passed to `StartCoroutine()` or `.Start()`. The passed code takes the form of a lambda without arguments. 

Additionally we provide a `.YieldWhile(() => condition)` extension method to interrupt existing IEnumerators early. The resulting IEnumerator checks the condition before every evaluation and if `false` stops the whole coroutine. It can be useful when used in combination with `WaitForSeconds`, where the delayed action is only executed when the passed condition still holds true.

Imagine you have some function `IEnumerator OpenDoor()` that rotates some door until it is open. There is also a function `IEnumerator CloseDoor()` that does the opposite: Rotate the door until it is closed. Of course we want to be able to interrupt the opening and start closing the door at any point.

```cs
// -- before

var isOpening = false;
var isClosing = false;

public void Open() => InterruptibleOpenRoutine();

private void InterruptibleOpenRoutine()
{
    isClosing = false;
    isOpening = true;
    var handler = StartCoroutine(OpenDoor());
    while (isOpening) yield return null;
    StopCoroutine(handler);
    isOpening = false;
}

public void Close() => InterruptibleCloseRoutine();

private void InterruptibleCloseRoutine() 
{
    isOpening = false;
    isClosing = true;
    var handler = StartCoroutine(CloseDoor());
    while (isClosing) yield return null;
    StopCoroutine(handler);
    isClosing = false;
}

public void HoldTheDoor()
{
    isOpening = false;
    isClosing = false;
}
```

This implementation is already pretty damn smart: It uses coroutines that start another coroutine and keep track of the state in parallel, stopping and cleaning up afterwards. But be honest, *would you have written the code this way?* Still, it can be better:

```cs
// -- after

private bool _isOpening = false;
private bool _isClosing = false;

public void Open() 
{
    _isClosing = false;
    _isOpening = true;
    OpenDoor()
        .YieldWhile(() => _isOpening)
        .Afterwards(() => _isOpening = false).Start();
}

public void Close()
{
    _isOpening = false;
    _isClosing = true;
    CloseDoor()
        .YieldWhile(() => _isClosing)
        .Afterwards(() => _isClosing = false).Start();
} 

public void HoldTheDoor()
{
    _isOpening = false;
    _isClosing = false;
}
```

Everything happens exactly as the code tells you. In my opinion everyone should see this code and instantly understand what is happening. Open the door as long as it is opening and afterwards set the flag to false, just in case. No weird coroutine handling tricks. It's really hard to make errors. 

**Bonus:** If you can't keep track of multiple boolean variables, implement a simple state machine: 

```cs
// outside of the behaviour
public class DoorOpenState 
{
    public IsOpening { get; private set; } = false;
    public IsClosing { get; private set; } = false;
    public void Open() 
    {
        IsOpening = true;
        IsClosing = false;
    }
    public void Close()
    {
        IsOpening = false;
        IsClosing = true;
    }
    public void Hold()
    {
        IsOpening = false;
        IsClosing = false;
    }
}

// later in behaviour
private DoorOpenState _doorState = new DoorOpenState();
private bool IsOpening => _openState.IsOpening();
private bool IsClosing => _openState.IsClosing();

public void Open() 
{
    _doorState.Open();
    OpenDoor()
        .YieldWhile(() => IsOpening)
        .Afterwards(() => _doorState.Hold()).Start();
}

public void Close()
{
    _doorState.Close();
    CloseDoor()
        .YieldWhile(() => IsClosing)
        .Afterwards(() => _doorState.Hold()).Start();
} 

public void HoldTheDoor() => _doorState.Hold();
```

`.YieldWhile` does *not* execute the passed action when the condition is already `false`.

## .AndThen

`.Afterwards` lets you somehow combine multiple coroutines by calling

```cs
// don't do this
SomeRoutine().Afterwards(() => OtherRoutine().Start()).Start();
```
This might work for simple cases. However, it also has some downsides: 

- The coroutine handler returned by the first `.Start()` has nothing to do with the execution of `OtherRoutine`. 
- When you want to use additional `.Afterwards` or `.YieldWhile` calls on any routine it gets complicated quickly. 
- It has `.Start()).Start()` - wtf?

That is why the CoreLibrary provides an extension method `IEnumerator.AndThen(otherRoutine)` for concatenating routines before starting them. With it, the above example is as simple as

```cs
// do this instead
SomeRoutine().AndThen(OtherRoutine()).Start();
```

If you still say 'do you *really* need this?' consider this:

```cs
// don't do this
SomeRoutine().Afterwards(
    () => OtherRoutine().Afterwards(
        () => ThirdRoutine().Start()).Start()).Start();
```

```cs
// do this instead
SomeRoutine().AndThen(OtherRoutine()).AndThen(ThirdRoutine()).Start();
```

Note that the passed routines are not prefixed by a `() =>`. This is because IEnumerators are already *lazy* - there is no need to wrap them into a function object in order to execute them later as they are already only ever executed once they are started by `StartCoroutine` or `Do`. 

**Bonus:** `.AndThen` can be used to delay a coroutine for some time. For example:

```cs
WaitUntil(() => Player.Level >= 10).AndThen( StartFirstClassQuest() ).Start();

WaitForSeconds(5).AndThen( OpenDoor() ).Start();

DelayForFrames(2).AndThen( SomeRoutineThatNeededToBeDelayed() ).Start();
```
This is the reason why the `afterwards` parameter is always optional.

## DoBefore

Imagine a use case where you wanted to execute some code before using `RepeatWhile` or `.YieldWhile`, regardless of whether the passed condition is initially false or not. When you want to immediately start the constructed coroutine, that is no problem:

```cs
private bool _isRunning = false;

public void Run()
{
    _isRunning = true;
    RepeatWhile(() => _isRunning, () => DoSomething()).Start();
}
```

When you do not want to start the coroutine immediately, you can also write it like this:

```cs
private bool _isRunning = false;

public IEnumerator Run()
{
    _isRunning = true;
    yield return RepeatWhile(() => _isRunning, () => DoSomething());
}
```

But for the very rare case where you want the entire new coroutine in a single expression, the CoreLibrary provides `IEnumerator DoBefore(CodeBlock action, IEnumerator coroutine)`:

```cs
private bool _isRunning = false;

public void Run() => DoBefore(
    () => _isRunning = true,
    RepeatWhile(
        () => _isRunning, 
        () => DoSomething()
    ));
```

## Do

For completeness, the CoreLibrary provides 
```cs
IEnumerable Do(YieldAction action)
```
A `YieldAction` is a function which takes no arguments and returns something, whatever it is. `Do` turns this function into a coroutine: It executes the function and `yield return`s the result of the function. The use of `Do` can mostly be avoided by using the right design. 

*`Do` is a constructor for a singleton coroutine: One that only generates a single result.*

## Repeat

If all the previous methods do not fit your need for a repetition, for example when the yielded values alternate between iterations, the CoreLibrary provides

```cs
IEnumerable Repeat(YieldAction action, int? times = null)
```

This is the most generic form of executing code over time without actually writing `yield return` yourself. The `times` parameter is optional, and if you omit it, the coroutine will run forever. You can further bind the repetition to a condition using `.YieldWhile`.

```cs
// -- before (naive)

public class RoadTrip : MonoBehaviour
{
    /* ... */

    private bool _inProgress = false;

    public float MinQuestionInterval;
    public float MaxQuestionInterval;

    private float _nextQuestionTime = 0f;

    private float RandomInterval()
    {
        var boundsDiff = MaxQuestionInterval - MinQuestionInterval;
        var intervalDiff = Random.value * boundsDiff;
        return MinQuestionInterval + intervalDiff;
    }

    public void StartRoadTrip()
    {
        /* ... */
        _inProgress = true;
        _nextQuestionTime = Time.time + RandomInterval();
    }

    private void Update()
    {
        if (_inProgress && Time.time > _nextQuestionTime)
        {
            Say("Are we there yet?");
            _nextQuestionTime = Time.time + RandomInterval();
        }

        /* ... */
    }
}
```

```cs
// -- before (with coroutine)

public class RoadTrip : MonoBehaviour
{
    /* ... */

    private bool _inProgress = false;

    public float MinQuestionInterval;
    public float MaxQuestionInterval;

    private float RandomInterval()
    {
        var boundsDiff = MaxQuestionInterval - MinQuestionInterval;
        var intervalDiff = Random.value * boundsDiff;
        return MinQuestionInterval + intervalDiff;
    }

    private IEnumerator QuestionRoutine()
    {
        while (_inProgress) {
            yield return new WaitForSeconds(RandomInterval());
            Say("Are we there yet?");
        }
    }

    public void StartRoadTrip()
    {
        /* ... */
        _inProgress = true;
        StartCoroutine(QuestionRoutine());
    }

    private void Update()
    {
        /* ... */
    }
}
```

```cs
// -- after

using static CoreLibrary.Coroutines;

public class RoadTrip : BaseBehaviour
{
    /* ... */

    private bool _inProgress = false;

    public float MinQuestionInterval;
    public float MaxQuestionInterval;

    private float RandomInterval()
    {
        var boundsDiff = MaxQuestionInterval - MinQuestionInterval;
        var intervalDiff = Random.value * boundsDiff;
        return MinQuestionInterval + intervalDiff;
    }

    public void StartRoadTrip()
    {
        /* ... */
        _inProgress = true;

        WaitForSeconds(RandomInterval()).AndThen(
            Repeat(() => {
                Say("Are we there yet?");
                return new WaitForSeconds(RandomInterval());
            }).YieldWhile(() => _inProgress)
        ).Start();
    }

    private void Update()
    {
        /* ... */
    }
}
```

## .Flatten

For the rare cases when you want to **manually iterate through an `IEnumerator`**, the CoreLibrary provides `.Flatten()`.

In unity, when writing coroutines, it is common to `yield return anotherIEnumerator`. This other `IEnumerator` might yield other nested `IEnumerator`s as well. When the Unity runtime encounters a nested coroutine after `StartCoroutine` or the CoreLibrary's `.Start()`, it executes the whole nested coroutine first before proceeding with the current one.

Now, when we want to *manually inject code between each step of some `IEnumerator`*, we have to manually iterate through it like this:

```cs
IEnumerator myCoroutine = ...;
while (myCoroutine.MoveNext())
{
    if (_inactive) continue;
    if (_somethingHappened) yield break;
    yield return myCoroutine.Current;
}
```

In this artificial case, we want to not have the Unity runtime wait for whatever when `_inactive` and stop early if `_somethingHappened`.
Patterns like this cause *unexpected behaviour* when encountering a nested `IEnumerator`. Consider the following code:

```cs
private IEnumerator SetupLaunch() { ... }
private IEnumerator Launch() { ... }
private IEnumerator PostLaunch() { ... }
private IEnumerator LaunchProcedure()
{
    yield return SetupLaunch();
    yield return Launch();
    yield return PostLaunch();
}
```

When setting `myCoroutine = LaunchProcedure();` in the first example, then the conditions are checked exactly *thrice*: once each after `SetupLaunch`, `Launch` and `PostLaunch` have completed. This is because when Unity is handed a complete IEnumerable through `yield return`, it executes the entire coroutine before continueing the above `while` loop. But what if we wanted to execute code **after every single** `yield return null` or other `YieldInstruction`, regardless of how `myCoroutine` is implemented?

For this use case, the CoreLibrary offers `.Flatten()` - for the Unity runtime, `.Flatten()` does absolutely nothing. However, the structure of *arbitrarily deeply nested* `IEnumerator`s is *flattened* into a single `IEnumerator` that is guaranteed to never `yield return` another `IEnumerator`, so that you can execute code after every single actual execution pause. This is for example used in the implementation of `.YieldWhile` to ensure that the end condition is checked as often as possible.

If you did not understand the use of `.Flatten()` yet, here's an example that breaks:

```cs
bool _running = true;

IEnumerator a()
{
  yield return b();
}

IEnumerator b()
{
  _running = false;
  yield return null;
  DoSomethingDangerous();
}

IEnumerator BrokenYieldWhile(IEnumerator wrapped, Condition cond)
{
    while (cond() && wrapped.MoveNext()) yield return wrapped.Current;
}

BrokenYieldWhile(a(), () => _running).Start();
```

In the above case the Unity runtime executes the whole `b()` coroutine before `BrokenYieldWhile` checks the condition and has a chance to interrupt. This means that, unintuitively, `DoSomethingDangerous` is actually called. And for this reason, we need `.Flatten()`.

## Working with code that expects `IEnumerable`s

Sometimes you write or use some code that expects an `IEnumerable` and wraps it or executes it in a context. Maybe the function that expects an `IEnumerable` just exists for code reuse or unit testing. But now you want the function to work *immediately* and not over time. What do you do? 

```cs
// -- before

public void DoSomethingThenExecute(IEnumerator coroutine) { ... }

private IEnumerator DoSomething() 
{
    DoSomethingInstant();
    yield break;
}

DoSomethingThenExecute(DoSomething());
```

Great, now you just wrote an extra private method, which requires space and an explicit name, just to wrap your `DoSomething` method to use it in `DoSomethingThenExecute`. A naive solution might be to thing "Wait, if my `DoSomethingThenExecute` method sometimes does not need behaviour over time, then why not write a second version?". I am going to omit a concrete example here, but you can see that this approach yields either a copy-pasted block of code with a single line changed or you're just moving the above problem somewhere else. 

The CoreLibrary provides an overload of the static `IEnumerator Do(CodeBlock code)` method, which takes a block of code and wraps it into a coroutine, which does nothing and returns immediately. This is basically the `DoSomethingInstant` pattern from above, generalized to whatever you might need.

"But what if I want to do nothing?" - well, you could always write `Do(() => {})`, but that is ugly. So the CoreLibrary also provides `IEnumerator DoNothing()`, which does exactly as it says: absolutely nothing. But by using this instead of `null` for your `IEnumerator` argument, you can save some null checks, thus reducing code complexity and increasing maintainability, all while explicitly stating that you *want* that code to do nothing. 
