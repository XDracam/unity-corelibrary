# The Unity CoreLibrary
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

The *CoreLibrary* is a collection of classes and extension methods you didn't even know you were missing. It provides generic, well-tested, well-documented components such as a Pool implementation and object queries as well as numerous extension methods and utilities that make your everyday code cleaner and more type safe.

This project is being developed primarily at theUuniveristy of Würzburg, Germany for the [Cavelands](http://cavelands.de/) student project. It remains free and open source for all Games Engineering students as well as anyone else who wants to use it. 

The main goal is to encourage the *good sides* of functional programming:

- Usage of first class functions (delegates)
- Immutability (esp. concerning vectors)
- Composition of behaviour (coroutines)
- Concise, generic and maintainable code
- Complete type safety

## What the *CoreLibrary* Offers

For a complete list, see the [full documentation](Documentation/documentation.pdf).

### Unity Extensions

```cs
var newVec = transform.position.WithY(y => y + 2);

foreach (var child in transform.GetChildren()) { ... }

var rb = gameObject.As<Rigidbody>(Search.InParents);

SetPerceivable(false); // continues coroutines and animations

if (AssignIfAbsent(ref rigidbody)) { /* newly assigned, do sth */ }
```

### LINQ extensions

```cs
var list1 = new List<int> { 1, 2, 3 };
var list2 = new List<int> { 4, 5, 6 };

list2.Where(v => v % 2 == 0).ForEach(Debug.Log);

list1.AndAlso(list2);     // IEnumerable<int> { 1, 2, 3, 4, 5, 6 }

list1.Shuffled();         // IEnumerable<int> { 3, 1, 2 }
```

### Declarative Coroutine Creation

```cs
StartCoroutine(RepeatEverySeconds(1f, () => Debug.Log("tik tok")));

MyCoroutine().Start() // works outside of MonoBehaviours!

WaitUntil(
    () => InRange(),
    () => Attack()
).Start();

RepeatForSeconds(FlightTime,
    () => _rb.AddForce(Vector3.up * Lift),
    fixedUpdate: true
).Start();

WaitForSeconds(3)
    .YieldWhile(() => InRange())
    .Afterwards(() => {
        Explode();
        Destroy(gameObject);
    }).Start();
```

### Generic Object Pooling

// TODO: inspector screenshot

```cs
var bullet = bulletPool.RequestItem(transform.position);
```

- Grows by a factor you can set once empty
- Independent interface for reusing objects
- No impact on your component design decisions

### Object Queries in Scene

```cs
abstract class Interactable : QueryableBaseBehaviour { ... }

Query.All<Interactable>()
    .Where(i => Distance(i.transform.position, transform.position) < Threshold)
    .ToList();
```

### Other Utilities

```cs
void DoSomething<T>(T value) 
{
    // works around Unity's custom null comparison bugs
    if (!Util.IsNull(value)) { ... }
}
```

## Getting Started

Requires Unity 2018.1 or above. Compatible with *C# 4 (.NET 3.5)* and above.

Either clone this repository and paste the `CoreLibrary` folder into your Scripts folder (or anywhere) or download directly from the [Asset Store](???).

Then add the line
```cs
using CoreLibrary;
``` 
to the top of your file. This will usually happen automatically when you extend `BaseBehaviour` instead of `MonoBehaviour`. For now everything is included in this single namespace, in order to ease the usage of the included extension methods.

## Contributing

If you have any ideas, problems or feature requests, please add an issue to this project. 

I will gladly accept any pull request for a feature you believe is useful for everyone. But be prepared that I might rewrite it to find the general style.

## Authors

* **Cameron Reuschel** - [XDracam](https://github.com/xdracam)

Want your name and link here? Go and send a pull request!

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

Special thanks to [Daniel Götz](https://github.com/Eregerog) for constant feedback and motivated usage.
