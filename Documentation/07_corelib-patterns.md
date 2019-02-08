# Design Patterns

This section contains a collection of design patters for using CoreLibrary code nicely. It will expand over time as more patterns and best practices are discovered.

## Bind coroutine lifespan to boolean variable

```cs
// don't do this

public class SomeComponent : BaseBehaviour
{
    private Coroutine _routine = null;

    public void DoSomethingUseful() 
    {
        _routine = SomeCoroutine().Start();
    }

    public void StopDoingSomethingUseful()
    {
        if (_routine != null) StopCoroutine(_routine);
    }

    public bool IsRunning => ???; // how?!
}
```

```cs
// do this instead

public class SomeComponent : BaseBehaviour
{
    private bool _isRunning = false;

    public void DoSomethingUseful()
    {
        _isRunning = true;
        SomeCoroutine()
            .YieldWhile(() => _isRunning)
            .Afterwards(() => _isRunning = false)
            .Start();
    }

    public void StopDoingSomethingUseful() => _isRunning = false;

    public void IsRunning => _isRunning;
}
```

This pattern is used for ending coroutines early in a nice way. In regular Unity code, if you want to stop a running coroutine early, you have to save a reference to the returned `Coroutine` object. While that produces working code, it doesn't give you any way to check whether the coroutine is actually running right now. Now, if you want to implement `IsRunning`, you'd usually have to keep track of an additional boolean. So why not make that boolean the only thing managing the state of the coroutine?  

### Don'ts

You might consider encapsulating the coroutine entirely behind a boolean property, but that is a terrible idea. Not only does writing `IsRunning = true` shouldn't have side effects like starting a coroutine, but you'll also quickly run into questions like *"what happens if it is already running?"*. Trust me, I just did while trying to write example code. It's a mess. Keep it simple.

## Use `foreach` and `.ForEach`

Everything related to sequences of somethings yields an `IEnumerable<T>`. This means that the values are calculated *lazily* or *on demand*. Calling `.ToList()` forces all these values to compute, whether you need them or not. For example, when using `.FirstOrDefault()` only the very first value is computed.

This is why *calls to `.ToList()` should be avoided* whenever possible.

### Don't do this

```cs
List<Cat> allMyCats = allMyYellowCats.AndAlso(allMyBrownCats).ToList();
for (var i = 0; i < allMyCats.Count(); ++i) 
{
    Debug.Log($"Cat number {i} is called {allMyCats[i].name}!");
}
```

This example is chosen explicitly so that we need the index `i`, which *could* mandate the use of a counting `for`. However, using a counting `for` here forces us to build an entirely new `List` only to use the indexing operator `[i]` on it.

Counting `for` loops were a great thing back in the 80's when you only worked with `array`s, or linearly aligned blocks of memory. But times have changed, and some data structures (such as `HashSet`) cannot be represented as a linear block of memory, making indexing a *very inflexible* way of accessing values in a collection of elements.  

### Instead do this

```cs
IEnumerable<Cat> allMyCats = allMyYellowCats.AndAlso(allMyBrownCats);
var catIndex = 0;
foreach (var cat in allMyCats) 
{
    Debug.Log($"Cat number {catIndex++} is called {cat.name}!");
}
```

"But we have an index so shouldn't we use counting `for`?" -- **NO.**

The cats may not even have a strict order depending on where you got the yellow and brown cats. We never explicitly sort them. What's more important: `.AndAlso` yields an `IEnumerable<Cat>`, which means that it is implemented using `yield return`. So we do not need any additional memory for `allMyCats`, as opposed to when we explicitly create a new list. As a consequence, index access takes `O(n)` runtime, as the values are obviously not linearly aligned in memory. Calling `.ToList` forces this linear alignment, but... why should we do it?

As an added bonus: `foreach` is much less likely to cause a bug than `for`. I once spent [half a day](https://stackoverflow.com/questions/40621865/java-program-terminates-in-first-recursion-level-white-initializing-a-string) chasing a simple bug in a `for`: I wrote `for (int i = 0; i < tabs; tabs++)`. Oops. In this case however, I wasn't iterating through a collection. Still, counting `for`s are dangerous and should *not ever be used for iterating through a sequence of elements* (unless you are writing code in C or Golang).

`.ForEach` is the CoreLibrary's way for having one less variable to find a name for. The above code could be rewritten as: 

```cs
var catIndex = 0;
allMyYellowCats.AndAlso(allMyBrownCats).ForEach(cat => 
{
    Debug.Log($"Cat number {catIndex++} is called {cat.name}!");
});
```

In this case, using `foreach` is the more readable way. However, sometimes it is hard to give something a useful name, especially after a long chain of LINQ calls. Or you just want to keep it very short, e.g. 

```cs

void printCat(Cat cat, int index) => 
    Debug.Log($"Cat number {index++} is called {cat.name}!");

/* ... */

var catIndex = 0;
allMyYellowCats.AndAlso(allMyBrownCats)
    .ForEach(c => printCat(c, catIndex++))
```

Which one you use is a matter of taste and code style. Generally, I would prefer using `foreach` unless I want to use a direct method reference or single expression. Just don't use `for`.

\newpage
