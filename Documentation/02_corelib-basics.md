# CoreLibrary Basics

## Requirements

The CoreLibrary comes with no dependencies or requirements other than *Unity 2018.2* or higher.

Everything should work with *C#4 (.NET 3.5)* and further versions out of the box. However, all following examples assume you use *C#6 (.NET 4.x)* or higher.

You can work with any editor or IDE, however I *highly recommend using either Visual Studio with [JetBrains ReSharper](https://www.jetbrains.com/resharper/) or the [JetBrains Rider](https://www.jetbrains.com/rider/) IDE* in order to make full use of the supplied annotations and keep code quality high. 

*(I am not affiliated with Microsoft or JetBrains in any way)*

## Usage

Paste the `CoreLibrary` folder from the [GitHub repo](https://github.com/XDracam/unity-corelibrary) into your `Scripts` folder or download from the [Asset Store](???). Then add the line 
```cs
using CoreLibrary;
``` 
to the top of your file. This will usually happen automatically when you extend `BaseBehaviour` instead of `MonoBehaviour`. For now everything is included in this single namespace, in order to ease the usage of the included extension methods.

## General Notes

For flexibility reasons, nothing in the CoreLibrary ever returns a `List<T>` or `HashSet<T>`. Instead, `IEnumerable<T>` and `IEnumerator` are used. Everything is *lazy*, meaning that no sequence operation (with the exception of `ForEach`) calculates anything before the value itself is required. This makes extensions such as `.Collect` as well as `.AndAlso` feel like natural extensions to the LINQ framework.

*An `IEnumerable<T>` can only be safely traversed **once***.

When you want to safely traverse an `IEnumerable<T>` multiple times, you have to call `.ToList()` on it.

## BaseBehaviour

Every component in Unity extends the class `UnityEngine.MonoBehaviour`. However, if you want all features from the CoreLibrary you should extend `CoreLibrary.Base.BaseBehaviour` instead, which itself extends `MonoBehaviour`.

`BaseBehaviour` lets you use shortcuts to `SetPerceivable`, `AssignComponent`, `AssignIfAbsent` and `IfAbsentCompute` among others as described later.

An added bonus is the `public Position` property, which behaves *exactly* like `.transform.position` except that it *also* enables you to modify single coordinates directly:

```cs
someBaseBehaviour.Position.y += 5;
```

This is implemented using the `Util.[VectorProxy]` class.

## Utility extensions

`SetPerceivable(bool state)` redirects to the extension method on `GameObject` and enables you to make an object *unperceivable*, meaning that all `Collider`s and `Renderer`s are inactive, but the object itself isn't. This allows audio tracks and coroutines played on the object to *end properly*. However, lifecycle methods such as `Update()`, `FixedUpdate()` etc. are still called, causing a potential performance loss when overusing.

`foo.IsNull()` is a more safe version of `foo == null`, which accounts for Unity's custom override of the `==` operator on components. You should only use this when working with a generic type `T` that does *not always* extend `UnityEngine.Component` or some subclass of it. For more information see the section [Generic Null Check].

`Transform.GetChildren()` is something urgently missing from Unity. Without it, when you want to use LINQ methods over all children, you have to use something along the lines of:

```cs
var children = new List<Transform>();
foreach (Transform t in transform) children.Add(t);
children.Select(...)...;
```

or even worse

```cs
var children = new List<Transform>(); 
for (var i = 0; i < transform.childCount(); ++i) children.Add(transform.GetChild(i));
children.Select(...)...;
```

`GetChildren` is actually implemented using `foreach` over the transform it is called on, since it is safe to add, remove and move children while iterating without causing weird bugs. 

In order to efficiently chain further LINQ queries (such as `.Find`, `.Where` etc.) the method returns an `IEnumerable<T>`, which is only **traversable once**.

## Vector extensions

Imagine you have an object and you want to always have it at the same Y position as some other object. For some reason, you also want it's Z position to double every 2 seconds. Let's look at this in regular Unity:

```cs
private IEnumerator Start()
{
    while (true)
    {
        yield return new WaitForSeconds(2);
        this.transform.position = new Vector3(
            this.transform.position.x,
            this.transform.position.y,
            this.transform.position.z * 2);
    }
}

private Transform _other;
private void Update()
{
    // can't just set position
    this.transform.position = new Vector3(
        this.transform.position.x,
        _other.position.y,
        this.transform.position.z);
}
```

In order to get rid of this repetitive hassle once for all, we provide extension methods for both `Vector2` and `Vector3` to either set or transform any individual coordinate without modifying the original vector:

```cs
private IEnumerator Start()
{
    while (true)
    {
        yield return new WaitForSeconds(2);
        this.transform.position =
            this.transform.position.WithZ(z => z * 2);
    }
}

private Transform _other;
private void Update()
{
    // can't just set position.y
    this.transform.position =
        this.transform.position.WithY(_other.transform.position.y);
}
```

This makes working with vectors in an immutable (= **safer**) manner a lot more comfortable. Note that methods such as `v.WithXY(w.x, w.y)` are *not* provided, as that would be equal to `w.WithZ(v.z)`. In cases where two coordinates are not from the same source, keeping the `With?` calls separate causes more understandable code.

## LINQ extensions

LINQ is a lovely framework that brings the *mapreduce* paradigm to C# in an efficient and SQL-ish way. With it you can replace almost any

```cs
for (int i=0;i<n;++i)
```

loop once and for all, which increases the expressibility of your code while making it a lot less error-prone. Everything in LINQ is internally implemented as simple Coroutines with `yield`. You can probably write it yourself.

As great as LINQ is, some heavy use cases are still missing.

`Enumerable.ForEach(Action<T> action)` replaces the need to save the result of a lovely chain of LINQ calls into a variable only to write a clunky `foreach` loop. It's not for everyone, but I think it can make the code more overviewable.

`Enumerable.Collect(Func<T, TRes> mapping)` is equal to \
`Enumerable.Select(mapping).Where(v => v != null)`. It applies the `mapping` function to every element in the Enumerable, yielding a new Enumerable with the results. It also filters out every `null` value. Useful for `Select`ing with a mapping that may fail, such as `v.As<Whatever>()`.

`Enumerable.CollectMany(Func<T, IEnumerable<TRes>> mapping)` is a version of collect that maps each value in the Enumerable to a new Enumerable and then flattens the nested Enumerables into one flat Enumerable, in order. It also filters out nulls. Calls LINQ's `SelectMany` in the background.

`Enumerable.AndAlso(IEnumerable other)` merges together two LINQ 'streams' of the *same type* and returns a new `IEnumerable`.

`Enumerable.Shuffle(System.Random random = null)` does exactly what it says it does: It returns a shuffled version of the enumerable using the Fisher-Yates algorithm. **It does not modify the original Enumerable**. You may pass your own instance of `System.Random` if you want to achieve a deterministic behavior (in Unity? HAHAHAHA). Otherwise the algorithm uses it's own. *This randomness is not cryptographically secure so don't even think about it*.

## Singleton

It sometimes happens that an object should only be present *once per scene*. Such an object is called a `Singleton`. Often, this single object holds some importance and is often referenced from other code. An example of this is the `Player` component. Usually getting this code involves a very expensive call to `FindObjectOfType<T>()` in every referencing component, which can be devastating for scene load times if many objects call this. A simpler solution would be to store a reference to the one instance of the component in the *static context* of the class itself.

```cs
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private void Awake()
    {
        if (Player.Instance != null)
            throw new WrongSingletonUsageException(
                "Already a player component in the scene!");
        Player.Instance = this;

        /* ... */
    }
}
```

Typing this out every time is tedious and also leads to easy errors (e.g. when someone accidentally writes `!=` instead of `==`). For this case, we provide a class:

```cs
abstract class CoreLibrary.Base.Singleton<T> : BaseBehaviour where T : Singleton<T>
```

The `where`-clause guarantees that the parameter `T` is the type of the implementing class itself. This is necessary to ensure safe typing for the `public static T Instance` property provided. Extending the class itself is enough for full singleton functionality:

```cs
public class Player : Singleton<Player> { /* ... */ }

// somewhere else
Player.Instance.Hp -= 5;
```

Some hints:

- There is no need to save the reference to `Player.Instance` during some call to `Start` or `Awake`, as the call itself is efficient enough.
- However, if typing out `Player.Instance` every time annoys you, you may define a getter somewhere in the class like `private Player pl => Player.Instance;`

## LazySingleton

While `Singleton` throws an exception when it can't find an object, `LazySingleton` instantiates a new empty `GameObject` with only the requested component attached to it. This is especially useful for objects that are required in every scene but are easily forgotten. It otherwise behaves exactly like `Singleton`.
