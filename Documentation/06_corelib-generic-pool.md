# Generic Pool

## Introduction

Imagine you want to implement a gun that shoots bullets. These bullets are physical objects, so that they can ricochet and have a realistic hitbox detection. A naive implementation may look like this:

```cs
public class Gun : MonoBehaviour
{
    public Rigidbody BulletPrefab;

    public float Impulse;

    public void Shoot()
    {
        var newBullet = Instantiate(BulletPrefab);
        newBullet.transform.position = transform.position;
        newBullet.transform.forward = transform.forward;
        newBullet.AddForce(
            transform.forward.normalized * Impulse, ForceMode.Impulse);
    }
}

public class Bullet : MonoBehaviour
{
    // some code that calls Despawn()

    public void Despawn()
    {
        Object.Destroy(this);
    }
}
```

The problem with this implementation is that you need to spawn a new Bullet each time `Shoot()` is called a new `GameObject` is created by the engine. This can lead to FPS drops when firing many shots in rapid succession.

A common solution to this problem is spawning a number of bullets at the beginning of the scene. Then, every time `Shoot()` is called, a bullet is taken out of the **pool**. Bullets in the pool are **reusable**, meaning that once they stop moving or despawn they can be used again. No additional costly creations or destructions involved.

## General Usage

With the CoreLibrary, our gun would look like this:

```cs
[RequireComponent(typeof(GenericPool))]
public class Gun : BaseBehaviour
{
    public Rigidbody BulletPrefab;

    public float Impulse;

    GenericPool _pool;

    private void Start()
    {
        AssignComponent(out _pool);
        _pool.Template = BulletPrefab;
        _pool.Capacity = 100;
        _pool.GrowRate = 0;
        // save settings and initialize
        _pool.Init();
    }

    public void Shoot()
    {
        var newBullet = _pool.RequestItem(transform.position);
        newBullet.transform.forward = transform.forward;
        newBullet.AddForce(
            transform.forward.normalized * Impulse, ForceMode.Impulse);
    }
}
```

    Oh no, the code got larger! I thought that never happens!

-- Yeah, you are right. This is because I did not write a custom, use-case specific pool for comparison.

In the example above, instead of configuring the pool in the Unity inspector we added a `RequireComponent(typeof(GenericPool))` to our `Gun` class. This automatically adds a `GenericPool` component, which we configured in the `Start()` method. Whenever we configure a pool in a script, we need to call the `pool.Init()` method. When you don't, then the pool initializes itself at the first call to `RequestItem()`, which might cause a small freeze.

When the pool does not find any item to reuse (which never happens in our example, why later), it's behaviour depends on the value of `GrowRate`. You see, in order to prevent glitches when there suddenly are no bullet lefts, the pool **grows** similar to the way a `List<T>` grows it's underlying array. When an item is requested but there are no free items to be found, the pool grows by a factor of `GrowRate`. Per default, `GrowRate` is set to `0.3`. There is a rough estimation. *If you know better, set it yourself.*

In the above case, since we never run out of bullets, we set `GrowRate = 0`. Now when we run out of bullets due to a bug, an `PoolOutOfItemsException` is thrown to signal that something went wrong.

## Usage from the Inspector

You do not have to write `[RequireComponent(typeof(GenericPool))]`. Maybe you want all your guns in the scene to share a single pool of bullets, just to be safe. For this use case, you should create an additional class for each type of **singleton pool** you need in the scene:

```cs
[RequireComponent(typeof(GenericPool))]
class BulletPool : Singleton<BulletPool>
{
    private GenericPool _pool;

    private void Start() => AssignComponent(out _pool);

    public static GameObject RequestBullet(Vector3 position) 
    {
        return Instance._pool.RequestItem(position);
    }
}
```

The `Singleton<BulletPool>` ensures that there is exactly one component of this type in the whole scene. Then you place this pool on any game object in the scene, and request a new bullet like this *in any script*:

```cs
BulletPool.RequestBullet(transform.position);
```

Obviously, the pool isn't configured in the script. Instead, you configure the pool through the Unity inspector. Drag the bullet prefab into the `Template Object` field and set `Capacity` and `GrowRate` as required.

**When configuring from the inspector, it is important to tick `Init On Scene Start`**, so that initialization is done at scene start and not once the first item is requested, to prevent potential freezes. 

## Reusables

```cs
public class Bullet : Reusable
{
    // we want the bullets to lie around in the world
    // as physical objects until they need to be reused

    public override void ResetForReuse()
    {
        // this is called *before* reusing
        // you never have to call this yourself
    }

    public override void AfterReuse()
    {
        // this is called *after* a successful reuse
        // you never have to call this yourself
    }

    public override void ReuseRequested()
    {
        // this is called when there are no items left
        // when you call FreeForReuse() in this method,
        // the object is immediately reused.

        // you never have to call this yourself

        gameObject.SetActive(false);
        FreeForReuse();
    }
}
```

The `Bullet` class now extends `Reusable` and has to override three methods.

- `ResetForReuse` is called by the pool every time it is about to reuse the item. This is analog to the `Reset` method in Unity, but for reusable items.

- `AfterReuse` is called by the pool just after it has successfully reused the item. It is analog to `Start`, in that it allows you to call initialization code after an object is 'spawned'.

- `ReuseRequested` is called by the pool every time no item is available anymore. The pool loops through all items it manages and requests a reuse from each until an item calls `FreeForReuse()` in this method.

You do not have to add code to any of the three methods. However, overriding them is still mandatory in order to force you to think about your `Reusable`'s behaviour and prevent potential bugs.

## Optimizations

The goal of the CoreLibrary is to create generic, robust and well-document extensions to the Unity engine that can be used in any project. For this reason, the `GenericPool` component has some notable behaviours which make using it safer for everyone.

### Lazy Initialization

During initialization, the pool allocates it's buffer and fills it with clones of the specified `Template`. *Changes to `Template` and `Capacity` do not matter once a pool is initialized*. Because of this reason, initialization is performed lazily per default: Either when `.Init()` is explicitly called or once the very first item is requested. This enables you to configure a pool in another script (as seen in the above example). It also gives you the possibility to improve scene loading times by moving initialization to a later point in time.

This decision comes at a cost: When the user configures a pool in the inspector and forgets to tick `Init on Scene Start`, a freeze might occur once the first item is requested later in the scene.

### Slow Growth

When a pool runs out of available items, it's capacity increases by a factor of `GrowRate`. If we created all items at the moment of growth we would cause a potential freeze. And we do not want freezes. For this reason, the pool only grows by *one item per frame*. When the pool is still growing and another item is requested, it is instantly instantiated on demand.
