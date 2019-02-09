# Scene Query System

## Introduction

When you want to find for example all `Resource`s in a scene, calling `Object.FindObjectsOfType<T>()` is *really inefficient*. Sometimes however, you need to query all objects of a type in a scene.

A possible approach to this is to write some `class ResourceManager : Singleton<ResourceManager>`, which holds a list of all Resources. Now, at the beginning of each scene, `Object.FindObjectsOfType<Resource>` is called once to fill that list.

While this solves the problem for resources, do you really want to create an additional component for *every single type you want to query*? Also, how do you manage new `Resource`s that are spawned at runtime and are not found during the initial query?

The CoreLibrary provides the `QueryableBaseBehaviour` and `Query` classes to solve these problems for every behaviour in a safe and robust way.

## Using CoreLibrary Queries

When you want your behaviour to be queryable, all you have to do is extend `QueryableBaseBehaviour` instead of `BaseBehaviour`. The *only* difference to extending `BaseBehaviour` directly is that `QueryableBaseBehaviour` automatically adds a component of type `Queryable`. This component interacts with the `Query` singleton, ensuring that it's game object is properly represented.

In order to have a `Resource` be queryable, all you have to do is this:

```cs
class Resource : QueryableBaseBehaviour { ... }
```

So later you can easily query all resources:

```cs
// this loops through every object in the scene
// which has a subtype of `Resource` attached to it
foreach (var resource in Query.All<Resource>()) { ... }
```

## .All and .AllActive

Additionally to the aforementioned `Query.All<T>()`, the CoreLibrary provides `Query.AllActive<T>()`, which (big surprise) does only retrieve the objects in the scene with components that extend `T` **with active game objects**.

## .AllWith and .AllActiveWith

You might to query only the `Resource`s which also have an `Interactable` attached to it, assuming that `Interactable : QueryableBaseBehaviour` as well. For this use case, there's `Query.AllWith<T>(other, types)` and the corresponding `Query.AllActiveWith`.

```cs
// get all `Resource`s which also have an `Interactable` component
var resources = Query.AllWith<Resource>(typeof(Interactable));
```

`.AllWith` takes at least one additional type bound, but can take as many as you want. The queried objects need to have at least one component of each of the specified types in order to be found.
