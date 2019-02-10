using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CoreLibrary.Tests
{
    /// <summary>
    /// Author: Cameron Reuschel
    /// </summary>
    public class TestCoroutines
    {
        [UnityTest]
        public IEnumerator TestWaitUntil()
        {
            var cond = false;
            var done = false;
            // ReSharper disable once AccessToModifiedClosure
            Coroutines.WaitUntil(() => cond, () => done = true).Start();
            Assert.IsFalse(done);
            yield return null;
            Assert.IsFalse(done);
            yield return null;
            Assert.IsFalse(done);
            cond = true;
            yield return null;
            Assert.IsTrue(done);
        }
        
        [UnityTest]
        public IEnumerator TestDelayForFrames()
        {
            var done = false;
            Coroutines.DelayForFrames(5, () => done = true).Start();
            Assert.IsFalse(done, "after 0 frames");
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.IsFalse(done, "after 4 frames");
            yield return null;
            Assert.IsTrue(done, "after 5 frames");
        }
        
        [UnityTest]
        public IEnumerator TestDelayForFrames_FixedUpdate()
        {
            var done = false;
            Coroutines.DelayForFrames(5, () => done = true, fixedUpdate: true).Start();
            Assert.IsFalse(done, "after 0 fu");
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(done, "after 4 fu");
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(done, "after 5 fu");
        }

        [UnityTest]
        public IEnumerator TestWaitForSeconds()
        {
            var done = false;
            Coroutines.WaitForSeconds(2, () => done = true).Start();
            Assert.IsFalse(done, "after 0 seconds");
            yield return new WaitForSeconds(1);
            Assert.IsFalse(done, "after 1 second");
            yield return new WaitForSeconds(1.1f);
            Assert.IsTrue(done, "after 2.1 seconds");
        }

        [UnityTest]
        public IEnumerator TestRepeatForSeconds()
        { // how do you even test this properly when it depends on framerate?
            var c = 0;
            Coroutines.RepeatForSeconds(2, () => ++c).Start();
            for (var i = c; i < 10; ++i)
            {
                Assert.AreEqual(i, c);
                yield return null;
            }

            yield return Coroutines.WaitForSeconds(1);

            for (var i = c; i < 10; ++i)
            {
                Assert.AreEqual(i, c);
                yield return null;
            }
            
            yield return new WaitForSeconds(1.1f);
            var lastC = c;
            yield return new WaitForSeconds(1);
            Assert.AreEqual(lastC, c);
        }

        [UnityTest]
        public IEnumerator TestRepeatEverySeconds()
        {
            var c = 0;
            Coroutines.RepeatEverySeconds(1, () => ++c, 3).Start();
            yield return new WaitForSeconds(.5f);
            Assert.AreEqual(c, 1);
            yield return new WaitForSeconds(1);
            Assert.AreEqual(c, 2);
            yield return new WaitForSeconds(1);
            Assert.AreEqual(c, 3);
            yield return new WaitForSeconds(1);
            Assert.AreEqual(c, 3);

            c = 0;
            var done = false;
            Coroutines.RepeatEverySeconds(1, () => ++c).YieldWhile(() => !done).Start();
            yield return new WaitForSeconds(.5f);
            Assert.AreEqual(c, 1);
            yield return new WaitForSeconds(1);
            Assert.AreEqual(c, 2);
            done = true;
            yield return new WaitForSeconds(1);
            Assert.AreEqual(c, 2);
        }

        [UnityTest]
        public IEnumerator TestRepeatForFrames()
        {
            var c = 0;
            Coroutines.RepeatForFrames(5, () => ++c).Start();
            Assert.AreEqual(1, c);
            yield return null;
            Assert.AreEqual(2, c);
            yield return null;
            Assert.AreEqual(3, c);
            yield return null;
            Assert.AreEqual(4, c);
            yield return null;
            Assert.AreEqual(5, c);
            yield return null;
            Assert.AreEqual(5, c, "in 6th frame");
        }

        [UnityTest]
        public IEnumerator TestRepeatWhile()
        {
            // ensure never executed on false
            var done = false;
            Coroutines.RepeatWhile(() => false, () => done = true).Start();
            Assert.IsFalse(done);
            yield return null;
            yield return null;
            Assert.IsFalse(done);
            
            // ensure stops once false
            var c = 0;
            Coroutines.RepeatWhile(() => !done, () => ++c).Start();
            for (var i = c; i < 10; ++i)
            {
                Assert.AreEqual(i, c);
                yield return null;
            }

            var finalC = c;
            done = true;
            yield return null;
            Assert.AreEqual(finalC, c);
            yield return null;
            Assert.AreEqual(finalC, c);
        } 
        
        [UnityTest]
        public IEnumerator TestYieldWhile()
        {
            // ensure never executed on false
            var done = false;
            Coroutines.RepeatWhile(() => true, () => done = true)
                .YieldWhile(() => false).Start();
            Assert.IsFalse(done);
            yield return null;
            yield return null;
            Assert.IsFalse(done);
            
            // ensure stops once false
            var c = 0;
            Coroutines.RepeatWhile(() => true, () => ++c)
                // ReSharper disable once AccessToModifiedClosure
                .YieldWhile(() => !done).Start();
            for (var i = c; i < 10; ++i)
            {
                Assert.AreEqual(i, c);
                yield return null;
            }
            var finalC = c;
            done = true;
            yield return null;
            Assert.AreEqual(finalC, c);
            yield return null;
            Assert.AreEqual(finalC, c);
        }

        [UnityTest]
        public IEnumerator TestAfterwards()
        {
            var repDone = false;
            var afterwardsDone = false;
            Coroutines.RepeatWhile(() => false, () => repDone = true)
                .Afterwards(() => afterwardsDone = true).Start();
            Assert.IsFalse(repDone);
            Assert.IsTrue(afterwardsDone);
            yield return null;
            Assert.IsFalse(repDone);
            Assert.IsTrue(afterwardsDone);
            
            // test that it works even with exceptions
            var counter = 0;
            Coroutines.RepeatWhile(
                    () => true, 
                    () => { throw new Exception("Planned test exception"); })
                .Afterwards(() => counter++).Start();
            LogAssert.Expect(LogType.Exception, new Regex(".*"));
            yield return null;
            yield return null;
            yield return null;
            Assert.AreEqual(counter, 1);
        }

        // yields 0 to 5
        private static IEnumerator TestFlattenHelper()
        {
            yield return 0;
            yield return TestFlattenHelper2(1);
            yield return 3;
            yield return TestFlattenHelper2(4);
        }

        private static IEnumerator TestFlattenHelper2(int offset)
        {
            yield return offset;
            yield return TestFlattenHelper3(offset);
        }
        
        private static IEnumerator TestFlattenHelper3(int offset)
        {
            yield return offset + 1;
        }

        [Test]
        public void TestDo()
        {
            var done = false;
            var action = Coroutines.Do(() =>
            {
                done = true;
                return 13;
            });
            Assert.IsFalse(done);
            Assert.IsTrue(action.MoveNext());
            Assert.IsTrue(done);
            Assert.AreEqual(13, action.Current);
            Assert.IsFalse(action.MoveNext());
        }
        
        [UnityTest]
        public IEnumerator TestDoBefore()
        {
            var done = false;
            Coroutines.RepeatWhile(() => false, () => done = true).Start();
            Assert.IsFalse(done);
            yield return null;
            yield return null;
            Assert.IsFalse(done);

            var called = false;
            Coroutines.DoBefore(() => done = true, Coroutines.RepeatWhile(() => false, () => called = true)).Start();
            Assert.IsFalse(called);
            Assert.IsTrue(done);
            yield return null;
            yield return null;
            Assert.IsFalse(called);
        }

        [UnityTest]
        public IEnumerator TestRepeat()
        {
            var c = 0;
            Coroutines.Repeat(() =>
            {
                c++;
                return null;
            }, 3).Start();
            
            Assert.AreEqual(1, c);
            yield return null;
            Assert.AreEqual(2, c);
            yield return null;
            Assert.AreEqual(3, c);
            yield return null;
            Assert.AreEqual(3, c);

            var done = false;
            c = 0;
            Coroutines.Repeat(() =>
            {
                c++;
                return new WaitForSeconds(.5f);
            }).YieldWhile(() => !done).Start();
            
            Assert.AreEqual(1, c);
            yield return new WaitForSeconds(.6f);
            Assert.AreEqual(2, c);
            yield return new WaitForSeconds(.6f);
            Assert.AreEqual(3, c);
            done = true;
            yield return new WaitForSeconds(.6f);
            Assert.AreEqual(3, c);
        }

        [Test]
        public void TestFlatten()
        {
            var flattened = TestFlattenHelper().Flatten();
            flattened.MoveNext();
            Assert.AreEqual(0, flattened.Current);
            flattened.MoveNext();
            Assert.AreEqual(1, flattened.Current);
            flattened.MoveNext();
            Assert.AreEqual(2, flattened.Current);
            flattened.MoveNext();
            Assert.AreEqual(3, flattened.Current);
            flattened.MoveNext();
            Assert.AreEqual(4, flattened.Current);
            flattened.MoveNext();
            Assert.AreEqual(5, flattened.Current);
            Assert.IsFalse(flattened.MoveNext());
        }

        [UnityTest]
        public IEnumerator TestAndThen()
        {
            var first = false;
            var second = false;
            Coroutines.DelayForFrames(1, () => first = true)
                .AndThen(Coroutines.DelayForFrames(1, () => second = true)).Start();
            Assert.IsFalse(first);
            Assert.IsFalse(second);
            yield return null;
            Assert.IsTrue(first);
            Assert.IsFalse(second);
            yield return null;
            Assert.IsTrue(first);
            Assert.IsTrue(second);
        }
    }
}
