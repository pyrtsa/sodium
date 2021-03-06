using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Sodium;
using Sodium.src.Sodium;

namespace Tests.sodium
{

  [TestFixture]
  public class BehaviorTester
  {
    [TearDown]
    public void TearDown()
    {
      GC.Collect();
      Thread.Sleep(100);
    }

    [Test]
    public void TestHold()
    {
      var e = new EventSink<Integer>();
      Behavior<Integer> b = e.Hold(0);
      var @out = new List<Integer>();
      Listener l = b.Updates().Listen(new Handler<Integer> { Run = x => @out.Add(x) });
      e.Send(2);
      e.Send(9);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 2, 9 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestSnapshot()
    {
      var b = new BehaviorSink<Integer>(0);
      var trigger = new EventSink<Long>();
      var @out = new List<string>();
      var l = trigger.Snapshot(b, (x, y) => x + " " + y)
          .Listen(new Handler<string> { Run = x => @out.Add(x) });
      trigger.Send(100L);
      b.Send(2);
      trigger.Send(200L);
      b.Send(9);
      b.Send(1);
      trigger.Send(300L);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "100 0", "200 2", "300 1" }, @out);
    }

    [Test]
    public void TestValues()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      var l = b.Value().Listen(x => @out.Add(x));
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9, 2, 7 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestConstantBehavior()
    {
      var b = new Behavior<Integer>(12);
      var @out = new List<Integer>();
      Listener l = b.Value().Listen(x => { @out.Add(x); });
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 12 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesThenMap()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = b.Value().Map<Integer>(x => x + 100).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 109, 102, 107 }, @out.Select(x => (int)x));
    }

    /**
     * This is used for tests where value() produces a single initial value on listen,
     * and then we double that up by causing that single initial event to be repeated.
     * This needs testing separately, because the code must be done carefully to achieve
     * this.
     */
    private static Event<Integer> DoubleUp(Event<Integer> ev)
    {
      return Event<Integer>.Merge(ev, ev);
    }

    [Test]
    public void TestValuesTwiceThenMap()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = DoubleUp(b.Value()).Map<Integer>(x => x + 100).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 109, 109, 102, 102, 107, 107 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesThenCoalesce()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = b.Value().Coalesce((fst, snd) => snd).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9, 2, 7 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesTwiceThenCoalesce()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = DoubleUp(b.Value()).Coalesce((fst, snd) => fst + snd).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 18, 4, 14 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesThenSnapshot()
    {
      var bi = new BehaviorSink<Integer>(9);
      var bc = new BehaviorSink<Character>('a');
      var @out = new List<Character>();
      Listener l = bi.Value().Snapshot(bc).Listen(x => { @out.Add(x); });
      bc.Send('b');
      bi.Send(2);
      bc.Send('c');
      bi.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 'a', 'b', 'c' }, @out.Select(x => (char)x));
    }

    [Test]
    public void TestValuesTwiceThenSnapshot()
    {
      var bi = new BehaviorSink<Integer>(9);
      var bc = new BehaviorSink<Character>('a');
      var @out = new List<Character>();
      Listener l = DoubleUp(bi.Value()).Snapshot(bc).Listen(x => { @out.Add(x); });
      bc.Send('b');
      bi.Send(2);
      bc.Send('c');
      bi.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 'a', 'a', 'b', 'b', 'c', 'c' }, @out.Select(x => (char)x));
    }

    [Test]
    public void TestValuesThenMerge()
    {
      var bi = new BehaviorSink<Integer>(9);
      var bj = new BehaviorSink<Integer>(2);
      var @out = new List<Integer>();
      Listener l = Event<Integer>.MergeWith((x, y) => x + y, bi.Value(), bj.Value())
          .Listen(x => { @out.Add(x); });
      bi.Send(1);
      bj.Send(4);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 11, 1, 4 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesThenFilter()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = b.Value().Filter(a => true).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9, 2, 7 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesTwiceThenFilter()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = DoubleUp(b.Value()).Filter(a => true).Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9, 9, 2, 2, 7, 7 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesThenOnce()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = b.Value().Once().Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesTwiceThenOnce()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Listener l = DoubleUp(b.Value()).Once().Listen(x => { @out.Add(x); });
      b.Send(2);
      b.Send(7);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 9 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestValuesLateListen()
    {
      var b = new BehaviorSink<Integer>(9);
      var @out = new List<Integer>();
      Event<Integer> value = b.Value();
      b.Send(8);
      Listener l = value.Listen(x => { @out.Add(x); });
      b.Send(2);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 8, 2 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestMapB()
    {
      var b = new BehaviorSink<Integer>(6);
      var @out = new List<String>();
      Listener l = b.Map(x => x.ToString())
          .Value().Listen(x => { @out.Add(x); });
      b.Send(8);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "6", "8" }, @out);
    }

    [Test]
    public void TestMapBLateListen()
    {
      var b = new BehaviorSink<Integer>(6);
      var @out = new List<String>();
      Behavior<String> bm = b.Map(x => x.ToString());
      b.Send(2);
      Listener l = bm.Value().Listen(x => { @out.Add(x); });
      b.Send(8);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "2", "8" }, @out);
    }

    [Test]
    public void TestTransaction()
    {
      var calledBack = new bool[1];
      Transaction.Run(trans => trans.Prioritized(Node.Null, trans2 => { calledBack[0] = true; }));
      Assert.That(calledBack[0], Is.True);
    }

    [Test]
    public void TestApply()
    {
      var bf = new BehaviorSink<Func<Long, String>>(b => "1 " + b);
      var ba = new BehaviorSink<Long>(5L);
      var @out = new List<String>();
      Listener l = Behavior<Long>.Apply(bf, ba).Value().Listen(x => { @out.Add(x); });
      bf.Send(b => "12 " + b);
      ba.Send(6L);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "1 5", "12 5", "12 6" }, @out);
    }

    [Test]
    public void TestLift()
    {
      var a = new BehaviorSink<Integer>(1);
      var b = new BehaviorSink<Long>(5L);
      var @out = new List<String>();
      Listener l = Behavior<Integer>.Lift(
        (x, y) => x + " " + y,
        a,
        b
      ).Value().Listen(x => { @out.Add(x); });
      a.Send(12);
      b.Send(6L);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "1 5", "12 5", "12 6" }, @out);
    }

    [Test]
    public void TestLiftGlitch()
    {
      var a = new BehaviorSink<Integer>(1);
      Behavior<Integer> a3 = a.Map<Integer>(x => x * 3);
      Behavior<Integer> a5 = a.Map<Integer>(x => x * 5);
      Behavior<String> b = Behavior<Integer>.Lift((x, y) => x + " " + y, a3, a5);
      var @out = new List<String>();
      Listener l = b.Value().Listen(x => { @out.Add(x); });
      a.Send(2);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "3 5", "6 10" }, @out);
    }

    [Test]
    public void TestHoldIsDelayed()
    {
      var e = new EventSink<Integer>();
      Behavior<Integer> h = e.Hold(0);
      Event<String> pair = e.Snapshot(h, (a, b) => a + " " + b);
      var @out = new List<String>();
      Listener l = pair.Listen(x => { @out.Add(x); });
      e.Send(2);
      e.Send(3);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "2 0", "3 2" }, @out);
    }

    class SB
    {
      public SB(Character a, Character b, Behavior<Character> sw)
      {
        A = a;
        B = b;
        Sw = sw;
      }
      public readonly Character A;
      public readonly Character B;
      public readonly Behavior<Character> Sw;
    }

    [Test]
    public void TestSwitchB()
    {
      var esb = new EventSink<SB>();
      // Split each field @out of SB so we can update multiple behaviours in a
      // single transaction.
      Behavior<Character> ba = esb.Map(s => s.A).FilterNotNull().Hold('A');
      Behavior<Character> bb = esb.Map(s => s.B).FilterNotNull().Hold('a');
      Behavior<Behavior<Character>> bsw = esb.Map(s => s.Sw).FilterNotNull().Hold(ba);
      Behavior<Character> bo = Behavior<Character>.SwitchB(bsw);
      var @out = new List<Character>();
      Listener l = bo.Value().Listen(c => { @out.Add(c); });
      esb.Send(new SB('B', 'b', null));
      esb.Send(new SB('C', 'c', bb));
      esb.Send(new SB('D', 'd', null));
      esb.Send(new SB('E', 'e', ba));
      esb.Send(new SB('F', 'f', null));
      esb.Send(new SB(null, null, bb));
      esb.Send(new SB(null, null, ba));
      esb.Send(new SB('G', 'g', bb));
      esb.Send(new SB('H', 'h', ba));
      esb.Send(new SB('I', 'i', ba));
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 'A', 'B', 'c', 'd', 'E', 'F', 'f', 'F', 'g', 'H', 'I' }, @out.Select(x => (char)x));
    }

    class SE
    {
      public SE(Character a, Character b, Event<Character> sw)
      {
        A = a;
        B = b;
        Sw = sw;
      }
      public readonly Character A;
      public readonly Character B;
      public readonly Event<Character> Sw;
    }

    [Test]
    public void TestSwitchE()
    {
      var ese = new EventSink<SE>();
      Event<Character> ea = ese.Map(s => s.A).FilterNotNull();
      Event<Character> eb = ese.Map(s => s.B).FilterNotNull();
      Behavior<Event<Character>> bsw = ese.Map(s => s.Sw).FilterNotNull().Hold(ea);
      var @out = new List<Character>();
      Event<Character> eo = Behavior<Character>.SwitchE(bsw);
      Listener l = eo.Listen(@out.Add);
      ese.Send(new SE('A', 'a', null));
      ese.Send(new SE('B', 'b', null));
      ese.Send(new SE('C', 'c', eb));
      ese.Send(new SE('D', 'd', null));
      ese.Send(new SE('E', 'e', ea));
      ese.Send(new SE('F', 'f', null));
      ese.Send(new SE('G', 'g', eb));
      ese.Send(new SE('H', 'h', ea));
      ese.Send(new SE('I', 'i', ea));
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 'A', 'B', 'C', 'd', 'e', 'F', 'G', 'h', 'I' }, @out.Select(x => (char)x));
    }

    [Test]
    public void TestLoopBehavior()
    {
      var ea = new EventSink<Integer>();
      Behavior<Integer> sum_out = Transaction.Run(() =>
      {
        var sum = new BehaviorLoop<Integer>();
        Behavior<Integer> sumOut = ea.Snapshot<Integer, Integer>(sum, (x, y) => x + y).Hold(0);
        sum.Loop(sumOut);
        return sumOut;
      });
      var @out = new List<Integer>();
      Listener l = sum_out.Value().Listen(x => { @out.Add(x); });
      ea.Send(2);
      ea.Send(3);
      ea.Send(1);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 0, 2, 5, 6 }, @out.Select(x => (int)x));
      Assert.That((int)sum_out.Sample(), Is.EqualTo(6));
    }

    [Test]
    public void TestCollect()
    {
      var ea = new EventSink<Integer>();
      var @out = new List<Integer>();
      Behavior<Integer> sum = ea.Hold(100).Collect(
        (Integer)0,
        //(a,s) => new Tuple2(a+s, a+s)
        (a, s) => new Tuple<Integer, Integer>(a + s, a + s));
      Listener l = sum.Value().Listen(x => { @out.Add(x); });
      ea.Send(5);
      ea.Send(7);
      ea.Send(1);
      ea.Send(2);
      ea.Send(3);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 100, 105, 112, 113, 115, 118 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestAccum()
    {
      var ea = new EventSink<Integer>();
      var @out = new List<Integer>();
      Behavior<Integer> sum = ea.Accum<Integer>(100, (a, s) => a + s);
      Listener l = sum.Value().Listen(x => { @out.Add(x); });
      ea.Send(5);
      ea.Send(7);
      ea.Send(1);
      ea.Send(2);
      ea.Send(3);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { 100, 105, 112, 113, 115, 118 }, @out.Select(x => (int)x));
    }

    [Test]
    public void TestLoopValueSnapshot()
    {
      var @out = new List<string>();
      Event<String> eSnap = Transaction.Run(() =>
      {
        var a = new Behavior<string>("lettuce");
        var b = new BehaviorLoop<string>();
        Event<String> eSnap_ = a.Value().Snapshot(b, (aa, bb) => aa + " " + bb);
        b.Loop(new Behavior<String>("cheese"));
        return eSnap_;
      });
      Listener l = eSnap.Listen((x) => { @out.Add(x); });
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "lettuce cheese" }, @out);
    }

    [Test]
    public void TestLoopValueHold()
    {
      var @out = new List<string>();
      Behavior<String> value = Transaction.Run(() =>
      {
        var a = new BehaviorLoop<string>();
        Behavior<String> value_ = a.Value().Hold("onion");
        a.Loop(new Behavior<String>("cheese"));
        return value_;
      });
      var eTick = new EventSink<string>();
      Listener l = eTick.Snapshot(value).Listen(x => { @out.Add(x); });
      eTick.Send(UnitEnum.Unit);
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "cheese" }, @out);
    }

    [Test]
    public void TestLiftLoop()
    {
      var @out = new List<string>();
      var b = new BehaviorSink<string>("kettle");
      Behavior<String> c = Transaction.Run(() =>
      {
        var a = new BehaviorLoop<string>();
        Behavior<String> c_ = Behavior<string>.Lift(
            (aa, bb) => aa + " " + bb,
            a, b);
        a.Loop(new Behavior<String>("tea"));
        return c_;
      });
      Listener l = c.Value().Listen(x => { @out.Add(x); });
      b.Send("caddy");
      l.Unlisten();
      CollectionAssert.AreEqual(new[] { "tea kettle", "tea caddy" }, @out);
    }
  }

}
