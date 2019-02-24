# Utilities

The `Util` class contains a number of static methods, which are not specific to any single module, but have otherwise would have to be redefined every time they are required. You can import all utilities by adding the following line to the top of your source file:

```cs
using static CoreLibrary.Util;
```

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

## VectorProxy

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