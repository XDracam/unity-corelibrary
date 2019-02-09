# Component Queries

## Introduction

In regular Unity, one may retrieve a GameObject's components by using `gameObject.GetComponent<Renderer>()`. However, this is not only verbose but also not very flexible. For example, `GetComponent` only searches in the object itself, not in it's children or parents. For both use cases other, even more verbose methods, exist. However, these can't search more than one layer deep (I guess, it's not exactly well documented...). 

Because of these shortcomings the core library provides a number of special query methods, which will be presented in the rest of this page. All of these methods have an optional parameter `Search where`, which lets you decide the scope of the search. `Search` is an enum containing the values `InObjectOnly`, `InChildren`, `InParents` and `InWholeHierarchy`. The default is always `InObjectOnly`. `InChildren` does a depth-first search through all the children until an instance of the requested component is found. `InParents` linearly traverses all parents until the scene root or until the requested component is found. `InWholeHierarchy` searches the parents first for efficiency reasons, then the children. In all cases, the object itself is searched first.

The rest of this page explains all queries by first presenting how it is usually done followed by a proper use case of the corresponding CoreLibrary query.

## `AssignComponent<T>` and `AssignIfAbsent<T>`

Usually, finding other relevant components on the same game object and saving them into instance fields is a tedious but necessary task, since `GetComponent` is an expensive call and should therefore be called as little as possible. For this, we extend the `GameObject` class with two extension methods:

-  `void AssignComponent<T>(out T variable, Search where = Search.InObjectOnly) where T:Component`
-  `bool AssignIfAbsent<T>(ref T variable, Search where = Search.InObjectOnly) where T:Component`


Both of these methods are also handily available in `BaseBehaviour`.
Consider the following code:

```cs
// in regular Unity code
[RequireComponent(typeof(Animator))]
public class SampleComponent : MonoBehaviour
{
    // may be in some parent object as well
    private Rigidbody _rb;

    // in this object only
    private Animator _anim;

    private void Awake() {
        // some complicated code that might set _anim
    }

    private void Start() {
        // rigidbody may be in some parent object
        var currentTransform = this.transform;
        do {
            if (_rb == null)
                _rb = currentTransform.GetComponent<Rigidbody>();
            if (_rb != null) break;
            currentTransform = currentTransform.parent;
        } while (currentTransform != null);
        if (_rb == null) 
            throw new Exception("No rigidbody for " + gameObject.name);

        if (_anim != null) 
        { // if already set in Awake
            /* Play some special animation */
        } else _anim = GetComponent<Animator>();
    }
}
```

```cs
// semantically equivalent code with our CoreLibrary
[RequireComponent(typeof(Animator))]
public class SampleComponent : BaseBehaviour
{
    private Rigidbody _rb;
    private Animator _anim = null; // for use as `ref` param

    private void Awake() {
        // some complicated code that might set _anim
    }

    private void Start() {
        // type parameter T is automatically inferred
        // an error is thrown when nothing can be found
        AssignComponent(out _rb, Search.InParents);

        if (!AssignIfAbsent(ref _anim)) 
        { // if already set in Awake
            /* Play some special animation */
        }
    }
}
```

`AssignComponent` takes a reference to a variable marked as `out`. This guarantees that the variable is assigned somewhere inside the function. It also infers the type `T` for you, saving a lot of unnecessary verbosity.

`AssignIfAbsent` however takes a `ref` parameter. This does not guarantee that the variable will be set, however it requires the passed variable to be explicitly assigned before passing it to the function. This is necessary to check whether the variable already contains a value. For convenience, `true` is returned if the passed variable was `null` and has been assigned, and `false` if no assignment happened. 

`AssignIfAbsent` is particularly useful to avoid potential multiple calls to `AssignComponent`, which repeats the search every time. You can use it for loading a component lazily or *on demand*: Consider some object with an `Animator` that is only needed in one specific way of interacting with it. There are hundreds of these objects in the scene and you are only going to interact with very few of them, and with some of them multiple times. Now, calling `AssignComponent` somewhere in `Start` is wasted computing time 90% of the time. Calling `AssignComponent` every time the interaction happen s may waste valuable resources on redundant searches. In this case `AssignIfAbsent` offers an efficient solution, as the cost of re-searching is only one very cheap `null`-check. The `bool` return value may be ignored in this case. 

## `Is<T>`, `As<T>` and `All<T>`

Imagine you are an awesome flying space cat shooting your EMP laser eyes at multiple invading alien spacecraft stupidly flying in a straight row. All their antigravity drives deactivate and they fall to their deaths, exploding as they hit the hard and dry ground of the Arizona desert. **"What?"** - what? 

Shooting laser rays sounds exactly like a Raycast. Now, in Unity, a [RaycastAll](https://docs.unity3d.com/ScriptReference/Physics.RaycastAll.html) yields a `RaycastHit[]`. Each hit contains a reference to the hit `Rigidbody` as well as the object's `Transform`. So consider the following code:

```cs
RaycastHit[] hits = /* ... */;

// -- regular Unity
// Fails if components are distributed over a more 
// complex object hierarchy such as in complex prefabs.
// Also fails on other edge cases...
var shipsHit = hits
    .Where(h => h.transform.GetComponent<Spaceship>() != null).ToList();
var hitAnimations = shipsHit
    .Select(s => s.transform.GetComponent<Animator>())
    .Where(a => a != null);
var engines = shipsHit
    .SelectMany(s => s.transform.GetComponents<AntigravityEngine>());
foreach (var anim in hitAnimations) anim.play("hit");
foreach (var engine in engines) engine.Explode();
foreach (var hit in shipsHit) hit.rigidbody.useGravity(true);
```
```cs
// -- with CoreLibrary code
var shipsHit = hits
    .Where(h => h.transform.Is<Spaceship>(Search.InWholeHierarchy));
shipsHit
    .Collect(s => s.transform.As<Animator>())
    .ForEach(anim => anim.play("hit"));
shipsHit
    .SelectMany(s => s.transform.All<AntigravityEngine>(Search.InWholeHierarchy))
    .ForEach(engine => engine.Explode());
shipsHit.ForEach(hit => hit.rigidbody.useGravity(true));
```

As you can see, `Is`, `As` and `All` make the code more concise but they as well as complex searches much easier! `.Collect` is a nice shortcut to lose the null checks. `.ForEach` depends on one's taste. I personally prefer it to an additional variable and a loop.

All three methods are available as extensions to both `GameObject` and `Transform` classes for convenience.
