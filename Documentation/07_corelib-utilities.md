# Utilities

The `Util` class contains a number of static methods, which are not specific to any single module, but have otherwise would have to be redefined every time they are required. You can import all utilities by adding the following line to the top of your source file:

```cs
using static CoreLibrary.Util;
```

## Construct an IEnumerable from constants with `Seq`

Sometimes people write methods which take an `IEnumerable<T>` parameter and do something with this sequence. And sometimes, you want to call these methods with a constant number of arguments. The most concise solution to do this without the CoreLibrary is the following:

```cs
DoStuffToAllOf(new [] { arg1, arg2, arg3 });
```

But this array construction looks a bit weird and may be confusing for C#-beginners. So instead you can now write:

```cs
DoStuffToAllof(Seq(arg1, arg2, arg3));
```

Less characters and the focus now lies and what is passed and not how it is passed. 

## Modulus

The `Util.Mod(int x, int m)` function defines a mathematically correct modulus. You see, the result of the `%` operator can be negative when one of it's arguments is negative. However, this is often not what you want. So instead of writing `(x % m + m) % m` every time (thereby confusing readers), the CoreLibrary provides this utility. The `Mod` function is positive for every `x`, as long as `m > 0`. For now, this only works for `int`s, as there is no robust way to check for "a type which provides the `%` operator" during compile time.

## Generic Null Check

Unity overrides the `==` operator for every `Component`, so that `component == null` works when the component is destroyed in theory (e.g. after a `Destroy(component)`), but has not yet been removed from memory. You can find more information in [this blog post](https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/).

Problems arise when working with a generic type `T`, which might not always extend `Component`. Consider the following example taken from [this forum post](https://forum.unity.com/threads/null-check-inconsistency-c.220649/):

```cs
public void AddNotNull<T>(IList<T> list, T item)
{
    if (item != null)
    {
        Debug.Log(item + " is not null.");
        list.Add(item);
    }
}
```

Since type `T` is not constrained (e.g. by a `where T : Component`), the compiler does not know that it is supposed to invoke some custom `==` operator. As a consequence, calling `AddNotNull` for `T = Collider` or any other component can cause `null is not null` to be printed to the console.

In order to achieve proper null checks for unconstrained generic types which might be components, the CoreLibrary provides the `Util.IsNull<T>(T value)` function, which works for any type and has a special fix for Unity `Component`s built in.

## Working with Unity `null`

In Unity, there are two cases for some `UnityEngine.Object foo` when `foo == null`: Either the field has no value yet (C# null) or the referenced object has been destroyed using `Object.Destroy(foo)`. In the latter case, the reference to `foo` is set to a special state which allows it to be treated as `null` by comparisons. However, this causes problems when working with the `?.` and `??` syntacic constructs that come with C#, since a destroyed object is never really `null`. This causes unexpected bugs, especially since there is no warning (unless you are using JetBrains Rider as IDE). 

To enable you to use a style similar to `?.` and `??`, the CoreLibrary provides the `obj.IsNotNull(Action<T> action, Action elseAction)` extension method and derivates, so you can rewrite your potentially buggy code now:

```cs
// -- before

var foo = bar?.baz();
if (foo == null) {
    foo = doSomethingWithFoo(foo);
}
if (foo != null) {
    foo2 = foo.baz();
} else handleErrorCase();

var foo3 = foo2?.bar() ?? quoz;

// -- after

var foo = bar.IfNotNull(b => b.baz());

foo.IfNotNull(f => foo2 = f.baz(), elseAction: handleErrorCase);

foo3 = foo2.IfNotNull(f => f.bar(), elseResult: quoz);
```

To preserve the semantics of chaining `?.`s and ending with a `??`, the *else* case is called either when the value itself is null or the result of the action (if there is one) is null. These extension methods work for anything, so if you are using an unconstrained generic parameter `T` which can be a unity object, then using `.IfNotNull` is safer than using null-logic directly.

## `Object.SafeDestroy()`

Unity [strongly discourages you](https://docs.unity3d.com/ScriptReference/Object.DestroyImmediate.html) from using `Object.DestroyImmediate(obj)` instead of `Object.Destroy(obj)`. However, when writing editor code, the delayed destruction caused by `Destroy(obj)` is never executed.
This poses a difficulty when writing code that is used both in the editor as well as during rumtime. So you might end up with a code snippet like this:

```cs
// without CoreLibrary

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                UnityEngine.Object.DestroyImmediate(obj);
            else
#endif
            UnityEngine.Object.Destroy(obj);

```

Pretty ugly and distracting, right? For this special case, the CoreLibrary provides the `obj.SafeDestroy()` extension method for your shared code needs:

```cs
// with CoreLibrary

obj.SafeDestroy();
```

## `IfAbsentCompute`

When you instantiate a new game object from an existing one during runtime, all the object's field are already set. In this case, you would not want to repopulate all fields in the `Start` and `Awake` methods of each attached behaviour. To fix this, you would add an `if (myField != null)` before every single field that could be unnecessarily reassigned. This adds a lot to complexity, and other developers reading your code might not get your idea and remove these 'unnecessary' checks.

As a programmer, you want to clearly state your intentions. This is why the CoreLibrary provides the `bool IfAbsentCompute<T>(ref T field, Func<T> getter)` method on `Util` as well as on `BaseBehaviour`. If the passed reference to a field is either null, equal to null according to Unity or equal to its default value (if it's a value type), it is assigned the result of calling the `getter` function and `true` is returned. Otherwise, the value of the passed field remains untouched, `getter` is never called and `false` is returned.

Using this method has one notable disadvantage: C# makes sure that `ref` parameter are always initialized beforehand. So instead of

```cs
// without CoreLibrary

private Renderer _renderer;
private Collider _collider;
private Vector3 _startOffset;

public GameObject OtherObject;

private void Start() 
{
	if (_renderer != null) _renderer = GetComponent<Renderer>();
	if (_collider != null) _collider = GetComponent<Collider>();
	if (_startOffset == Vector3.zero) _startOffset = transform.position - OtherObject.transform.position;
}
```

you now have to write

```cs
// with CoreLibrary

// explicit assignments necessary for use as ref params
private Renderer _renderer = null;
private Collider _collider = null;
private Vector3 _startOffset = default(Vector3);

public GameObject OtherObject;

private void Start() 
{
	IfAbsentCompute(ref _renderer, () => this.As<Renderer>());
	IfAbsentCompute(ref _collider, () => this.As<Renderer>());
	IfAbsentCompute(ref _startOffset, () => Position - OtherObject.transform.position);
}
```

## `VectorProxy`

Unity has a problem in that you can not write the following line of code:

```cs
transform.position.y = 5;
```

This has a simple reason. The `Vector3` class is declared as a `struct`. As such, calling `transform.position` returns a *copy* of the position vector. Modifying this copy without saving it in a variable makes no sense, which is why it does not compile.

In order to modify single coordinates of a vector *by reference* the CoreLibrary provides the class `Util.VectorProxy`. The class was primarily made to enable the following code:

```cs
someBaseBehaviour.Position.y = 5;
```

A `VectorProxy` can be constructed either by directly wrapping a `Vector3` or by providing *indirections* to another vector:

```cs
// modify coordinates on this object directly from elsewhere
public VectorProxy someVector = new VectorProxy(Vector3.zero);

// wrap an already existing public vector field from elsewhere
public VectorProxy position = new VectorProxy(
    () => transform.position, pos => transform.position = pos);
```

Basically a `VectorProxy` is **exactly** like a `Vector3`, except that it *also* enables setting it's properties directly per reference. You might consider using a `public VectorProxy` instead of a `public Vector3` field if you do not need inspector support. 
