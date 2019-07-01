using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace CoreLibrary
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// <br/><br/>
    /// This class holds a number of generic coroutine building blocks.
    /// These are necessary for a more fluent approach to using coroutines.
    /// By using these building blocks, one may construct complex coroutines
    /// without bloating the class with many <code>private IEnumerator foo(...)</code> functions.
    /// By using static imports one may use these directly without prefixes.
    /// For further documentation, consult the CaveGame wiki.
    /// <br/><br/>
    /// <i>Note that these only work on unity coroutines, not collection-like IEnumerators.</i>
    /// </summary>
    /// <example><code>
    /// using static CoreLibrary.Coroutines;
    /// </code></example>
    public static class Coroutines
    {
        /// <summary>
        /// A block of code that can be passed around as a value to be executed
        /// as many times as desired by the function it is passed to.
        /// </summary>
        /// <example><code>
        /// CodeBlock setDone = () => _done = true;
        /// 
        /// CodeBlock doRandomStuff = () => {
        ///     DoSomeRandomStuff();
        ///     Player.Instance.transform.position = Vector3.zero; 
        ///     _done = true;
        /// };
        ///
        /// void ExecuteTwice([CanBeNull] CodeBlock code)
        /// {
        ///     code();
        ///     code();
        /// }
        ///
        /// void ExecuteRandomly([CanBeNull] CodeBlock code)
        /// {
        ///     if (Random.Range(0,2) == 0)
        ///         code?.Invoke();
        /// }
        /// 
        /// </code></example>
        public delegate void CodeBlock();

        /// <summary>
        /// A block of code (see <see cref="CodeBlock"/>) that returns
        /// a value which is yielded in a coroutine. For more information
        /// about what to return from a coroutine, see the
        /// <a href="https://docs.unity3d.com/Manual/Coroutines.html">
        ///     documentation</a>.
        /// </summary>
        public delegate object YieldAction();

        /// <summary>
        /// A block of code (see <see cref="CodeBlock"/>) that returns 
        /// a value of type boolean. This code may be executed as 
        /// many times as necessary by the function it is passed to.
        /// <br/>
        /// Note that a Condition is usually a <i>closure</i>, meaning
        /// that it uses variables defined outside of it's scope.
        /// This is important, as a constant Condition may cause endless
        /// loops, e.g. in <see cref="Coroutines.YieldWhile"/>.
        /// An example of this can be seen below.
        /// </summary>
        /// <seealso cref="CodeBlock"/>
        /// <example><code>
        /// // a custom implementation of an until-loop using tail recursion
        /// public void Until([NotNull] Condition cond, [NotNull] CodeBlock code)
        /// {
        ///     if (!cond)
        ///     {
        ///         code();
        ///         Until(cond, code);
        ///     }
        /// }
        /// </code></example>
        /// <example><code>
        /// var i = 0;
        /// Until(() => i == 5, () =>
        /// {
        ///     i++;
        /// });
        /// </code></example>
        /// <example><code>
        /// IEnumerator enumerator = ...;
        /// Until(() =>
        /// {
        ///     bool hasNext = enumerator.MoveNext();
        ///     return !hasNext;
        /// }, () =>
        /// {
        ///     Debug.Log(enumerator.Current);
        /// });
        /// </code></example>
        public delegate bool Condition();

        /// <summary>
        /// An empty lazy singleton that is used to start coroutines with <see cref="Coroutines.Start"/>.
        /// Calling .Instance on this class creates a game object with an instance if not already present.
        /// </summary>
        public sealed class CoroutineRunner : LazySingleton<CoroutineRunner> { }

        /// <summary>
        /// Starts this coroutine. An alternative to calling <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.
        /// Spawns an instance of <see cref="CoroutineRunner"/> if not already present in the scene.
        /// </summary>
        public static Coroutine Start(this IEnumerator coroutine)
        {
            return CoroutineRunner.Instance.StartCoroutine(coroutine);
        } 

        /// <summary>
        /// This function exists solely for making manually iterating through coroutines safe.
        /// When calling .Flatten() on an IEnumerator, every nested IEnumerator becomes directly
        /// embedded into the current IEnumerator, making manual iteration safe.
        /// </summary>
        /// <example><code>
        /// var flattened = myIEnumerator.Flatten();
        /// while (flattened.MoveNext())
        /// {
        ///   // do something...
        ///   yield return flattened.Current;
        /// }
        /// </code></example>
        [NotNull] public static IEnumerator Flatten(this IEnumerator coroutine)
        {
            while (coroutine.MoveNext())
            {
                var curr = coroutine.Current as IEnumerator;
                if (curr != null)
                {
                    var sub = curr.Flatten();
                    while (sub.MoveNext())
                    {
                        yield return sub.Current;
                    }
                }
                else yield return coroutine.Current;
            }
        }

        /// <summary>
        /// <b>Before</b> each execution step of this coroutine, check if the
        /// specified condition holds true. Once it fails, cancel the coroutine immediately.
        /// One can use <see cref="Afterwards"/> to execute code regardless of interruption.
        /// This checks the condition once <b>before</b> executing any code.
        /// </summary>
        [NotNull] public static IEnumerator YieldWhile(this IEnumerator coroutine, [NotNull] Condition condition)
        {
            var flattened = coroutine.Flatten();
            while (condition() && flattened.MoveNext())
                yield return flattened.Current;
        }

        /// <summary>
        /// Concatenates two coroutines. As soon as this coroutine stops yielding
        /// the passed afterwards-coroutine is yielded. This can be used to mimic
        /// more complex control flow.
        /// </summary>
        /// <example><code>
        /// Do(WaitForSeconds(2).AndThen(RepeatWhile(...)))
        /// </code></example>
        /// <param name="afterwards">
        /// The coroutine to execute after this coroutine is done.
        /// </param>
        // ReSharper disable once InvalidXmlDocComment
        [NotNull] public static IEnumerator AndThen(this IEnumerator coroutine, [NotNull] IEnumerator afterwards)
        {
            yield return coroutine;
            yield return afterwards;
        }
        
        /// <summary>
        /// Executes the specified code when this coroutine ends, even if an exception is thrown.
        /// Mostly used for 'cleanup code', especially in combination with <see cref="YieldWhile"/>.
        /// </summary>
        /// <param name="afterwards">The block of code to be executed once the execution of this coroutine ends.</param>
        // ReSharper disable once InvalidXmlDocComment
        [NotNull] public static IEnumerator Afterwards(this IEnumerator coroutine, [NotNull] CodeBlock afterwards)
        {
            var flattened = coroutine.Flatten();
            try
            {
                while (flattened.MoveNext())
                    yield return flattened.Current;
            }
            finally
            {
                afterwards();
            }
        }

        /// <summary>
        /// 'Constructor' for a singleton coroutine.
        /// Takes a <see cref="YieldInstruction"/>, 
        /// executes it's code and `yield return`s the result.
        /// <br/><br/>
        /// By chaining calls of <see cref="Do(CoreLibrary.Coroutines.YieldAction)"/>,
        /// <see cref="AndThen"/> and <see cref="YieldWhile"/>
        /// you can construct any coroutine from functions only. 
        /// </summary>
        /// <param name="action">
        /// The action containing a single step of a coroutine and returns a value to be yielded.
        /// </param>
        /// <returns>
        /// A singleton coroutine, which when executed calls the passed action and yields the action's result.
        /// </returns>
        public static IEnumerator Do([NotNull] YieldAction action)
        {
            yield return action();
        }

        /// <summary>
        /// Turns a block of code into a coroutine that never yields anything.
        /// As soon as the coroutine is started, the passed <see cref="CodeBlock"/>
        /// is executed and the coroutine ends.
        /// <br/><br/>
        /// You can use this as a code-to-coroutine conversion for a function
        /// that expects a coroutine, but should work immediately instead. 
        /// </summary>
        /// <param name="code">
        /// The code that is executed as soon as this coroutine is started.
        /// </param>
        /// <returns>
        /// A coroutine that terminates immediately after executing the passed code.
        /// </returns>
        public static IEnumerator Do([NotNull] CodeBlock code)
        {
            code();
            yield break;
        }
        
        /// <summary>
        /// The empty coroutine. Just immediately calls <code>yield break;</code>.
        /// This is basically the 'null' for constructing coroutines: You can use
        /// this when code expects a coroutine, but you do not need any behavior.
        /// </summary>
        /// <returns>
        /// An IEnumerator that does nothing, not even wait.
        /// </returns>
        public static IEnumerator DoNothing() { yield break; }

        /// <summary>
        /// Repeats the passed <see cref="YieldInstruction"/> either the
        /// specified number of times or forever if not specified.
        /// <br/><br/>
        /// You can use <see cref="YieldWhile"/> to repeat your custom
        /// action until a condition is met. This is different from
        /// <see cref="RepeatWhile"/> and <see cref="RepeatEverySeconds"/>
        /// in that you can be more specific about what you want to yield
        /// between repetitions. 
        /// </summary>
        /// <example><code>
        /// var launch = Repeat(() => {
        ///     _rb.AddForce(Vector3.up * Push);
        ///     return new WaitForFixedUpdate();
        /// }, (int) (LaunchTime / Time.fixedDeltaTime));
        ///
        /// var areWeThereYet = Repeat(() => {
        ///     Say("Are we there yet?");
        ///     return new WaitForSeconds(Interval);
        /// }).YieldWhile(NotThere); 
        /// </code></example>
        /// <param name="action">
        /// The action containing a single step of a coroutine and returns a value to be yielded repeatedly.
        /// </param>
        /// <param name="times">
        /// The optional number of times the passed action is called and yielded.
        /// </param>
        public static IEnumerator Repeat([NotNull] YieldAction action, [CanBeNull] int? times = null)
        {
            for (var i = 0; i != times; ++i) yield return action();
        }

        /// <summary>
        /// Prepends an action to an existing coroutine.
        /// Especially useful in combination with <see cref="YieldWhile"/>
        /// and <see cref="RepeatWhile"/> in order to execute code even if
        /// the passed condition is false. 
        /// </summary>
        /// <example><code>
        /// private bool _isRunning = false;
        /// private IEnumerator RunImpl() { ... }
        /// public IEnumerator Run() => DoBefore(
        ///     () => _isRunning = true,
        ///     RunImpl().YieldWhile(() => _isRunning));
        /// </code></example>
        /// <param name="action">The code to execute before the passed coroutine.</param>
        /// <param name="coroutine">The coroutine to prepend the action to.</param>
        public static IEnumerator DoBefore([NotNull] CodeBlock action, IEnumerator coroutine)
        {
            action();
            yield return coroutine;
        }

        /// <summary>
        /// Evaluates the passed condition each frame. As soon as it returns true, 
        /// executes the block of code `afterwards`.
        /// </summary>
        /// <param name="afterwards">
        /// The block of code to be executed once the condition evaluates to true.
        /// </param>
        /// <param name="yieldInstructionGetter">
        /// A function which returns the object which should be yielded to the Unity runtime
        /// after each condition check. If null, yields null and waits for the next frame.
        /// </param>
        // ReSharper disable once InvalidXmlDocComment
        [Pure] public static IEnumerator WaitUntil([NotNull] Condition condition, [CanBeNull] CodeBlock afterwards = null,
            Func<object> yieldInstructionGetter = null)
        {
            while(!condition()) yield return 
                yieldInstructionGetter != null 
                ? yieldInstructionGetter.Invoke() 
                : null;
            if (afterwards != null) afterwards.Invoke();
        }

        /// <summary>
        /// Executes the passed action after the passed number of frames.
        /// </summary>
        /// <param name="afterwards">
        /// The block of code to be executed once the specified number of frames have passed.
        /// </param>
        // ReSharper disable all InvalidXmlDocComment
        [Pure] public static IEnumerator DelayForFrames(uint frames, [CanBeNull] CodeBlock afterwards = null,
            bool fixedUpdate = false)
        {
            for (var i = 0; i < frames; ++i)
                yield return fixedUpdate ? new WaitForFixedUpdate() : null;
            if (afterwards != null) afterwards.Invoke();
        }

        /// <summary>
        /// Executes the passed callback after `time` seconds.
        /// </summary>
        /// <param name="afterwards">
        /// The block of code to be executed once the specified time has passed.
        /// </param>
        [Pure] public static IEnumerator WaitForSeconds(float time, [CanBeNull] CodeBlock afterwards = null)
        {
            yield return new WaitForSeconds(time);
            if (afterwards != null) afterwards.Invoke();
        }

        /// <summary>
        /// Repeats the passed action each frame (or each fixedUpdate) until
        /// a number of seconds have passed.
        /// </summary>
        /// <param name="action">The body of the loop</param>
        /// <param name="seconds">Number of seconds the action is executed for.</param>
        /// <param name="fixedUpdate">
        /// If true, executes the action once per fixedUpdate.
        /// Otherwise once per frame. Defaults to false.
        /// </param>
        [Pure] public static IEnumerator RepeatForSeconds(float seconds, [NotNull] CodeBlock action, 
            bool fixedUpdate = false)
        {
            var start = Time.time;
            do
            {
                action();
                yield return fixedUpdate ? new WaitForFixedUpdate() : null;
            } while (Time.time - start < seconds);
        }

        /// <summary>
        /// Repeats the passed action a number of times as a coroutine.
        /// </summary>
        /// <param name="action">The body of the loop</param>
        /// <param name="frames">Number of times the body is executed</param>
        /// <param name="fixedUpdate">
        /// If true, executes the action once per fixedUpdate.
        /// Otherwise once per frame. Defaults to false.
        /// </param>
        [Pure] public static IEnumerator RepeatForFrames(uint frames, [NotNull] CodeBlock action, bool fixedUpdate = false)
        {
            for (var i = 0; i < frames; ++i)
            {
                action();
                yield return fixedUpdate ? new WaitForFixedUpdate() : null;
            }
        }

        /// <summary>
        /// Repeats the passed action as long as the predicate holds true.
        /// If the condition starts at `false`, the action is never executed.
        /// </summary>
        /// <param name="action">The body of the loop</param>
        /// <param name="condition">Execute the action as long as this function returns true</param>
        /// <param name="yieldInstructionGetter">
        /// A function which returns the object which should be yielded to the Unity runtime
        /// after each condition check. If null, yields null and waits for the next frame.
        /// </param>
        [Pure] public static IEnumerator RepeatWhile([NotNull] Condition condition, [NotNull] CodeBlock action,
            Func<object> yieldInstructionGetter = null)
        {
            while (condition())
            {
                action();
                yield return 
                    yieldInstructionGetter != null 
                    ? yieldInstructionGetter.Invoke() 
                    : null;
            }
        }

        /// <summary>
        /// Repeats the passed action every <paramref name="interval"/> seconds
        /// either forever or up to <paramref name="repetitions"/> times if specified,
        /// <b>starting immediately</b>.
        /// Can be conditionally limited using <see cref="YieldWhile"/>.
        /// </summary>
        /// <param name="interval">The number of seconds between each call to action</param>
        /// <param name="action">The body of the loop</param>
        [Pure] public static IEnumerator RepeatEverySeconds(float interval, [NotNull] CodeBlock action, 
            int? repetitions = null)
        {
            for (var i = 0; i != repetitions; ++i)
            {
                action();
                yield return new WaitForSeconds(interval);
            }
            
        }
    }
}