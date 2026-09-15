#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Automated EditMode tests for EventManager.EvaluateConditions(GameEvent, EventEvaluationContext).
///
/// Runs entirely inside the Unity Test Runner (Window > General > Test Runner > EditMode) without
/// entering Play Mode, loading a scene, touching real time/random weather, or driving any UI.
/// Every test builds an explicit GameEvent + EventEvaluationContext (the existing "mock context"
/// abstraction already used by EventConditionTestRunner) and calls the 2-argument
/// EvaluateConditions(e, ctx) overload directly, bypassing the static EventManager.MockContext
/// field and EventEvaluationContext.CreateFromRuntime() entirely so tests never depend on
/// GameManager/PlayerStats/SocialManager/FlagManager/DatabaseManager singletons being present.
///
/// A real production bug was found and fixed while writing these tests: see the "prerequisite"
/// tests below and EventManager.cs EvaluateConditions() step 7 for details.
/// </summary>
[TestFixture]
public class EventManagerConditionTests
{
    private GameObject _host;
    private EventManager _eventManager;

    [OneTimeSetUp]
    public void CreateIsolatedEventManager()
    {
        // Kunci determinisme: GameObject dibuat NON-AKTIF sebelum AddComponent() supaya
        // Awake() (yang mengelola singleton _instance & bisa memanggil Destroy() — ilegal
        // di luar Play Mode) tidak pernah terpanggil. Instance ini murni host lokal untuk
        // memanggil EvaluateConditions(), terpisah dari EventManager.Instance yang sesungguhnya.
        _host = new GameObject("EventManagerConditionTests_Host");
        _host.SetActive(false);
        _eventManager = _host.AddComponent<EventManager>();
    }

    [OneTimeTearDown]
    public void DestroyIsolatedEventManager()
    {
        if (_host != null)
        {
            Object.DestroyImmediate(_host);
        }
    }

    // =====================================================================
    // Factory helpers — setiap test hanya meng-override field yang benar-benar diuji;
    // sisanya diset ke nilai "wide open" agar tidak pernah jadi penyebab gagal yang tidak disengaja.
    // =====================================================================

    private static GameEvent MakeEvent(
        int eventId = 9001,
        string eventTitle = "Test Event",
        int minDay = 1, int maxDay = 60,
        string timeBlock = "Any", string dayType = "Any",
        int minLang = 0, int minEtiq = 0,
        int minMh = 0, int maxMh = 100, int minPh = 0,
        int minTheory = 0, int minPractice = 0,
        int? npcId = null, int minGuanxi = 0, int minAffectionState = 0,
        int minRumorLevel = 0,
        int? prereqEventId = null,
        string reqFlagName = null, int reqFlagVal = 1,
        int priority = 10, int startNodeId = 1001, bool isRepeatable = false)
    {
        return new GameEvent
        {
            eventId = eventId,
            eventTitle = eventTitle,
            npcId = npcId,
            minDay = minDay,
            maxDay = maxDay,
            timeBlock = timeBlock,
            dayType = dayType,
            minLang = minLang,
            minEtiq = minEtiq,
            minMh = minMh,
            maxMh = maxMh,
            minPh = minPh,
            minTheory = minTheory,
            minPractice = minPractice,
            minGuanxi = minGuanxi,
            minAffectionState = minAffectionState,
            minRumorLevel = minRumorLevel,
            prereqEventId = prereqEventId,
            reqFlagName = reqFlagName,
            reqFlagVal = reqFlagVal,
            priority = priority,
            startNodeId = startNodeId,
            isRepeatable = isRepeatable,
            isCompleted = false
        };
    }

    private static EventEvaluationContext MakeContext(
        int currentDay = 10, TimeBlock currentTimeBlock = TimeBlock.Siang, bool isWorkday = true,
        int languageProficiency = 100, int culturalEtiquette = 100,
        int mentalHealth = 50, int physicalHealth = 100,
        int academicTheoretical = 100, int academicPractical = 100,
        int globalRumorLevel = 3)
    {
        return new EventEvaluationContext
        {
            currentDay = currentDay,
            currentTimeBlock = currentTimeBlock,
            isWorkday = isWorkday,
            languageProficiency = languageProficiency,
            culturalEtiquette = culturalEtiquette,
            mentalHealth = mentalHealth,
            physicalHealth = physicalHealth,
            academicTheoretical = academicTheoretical,
            academicPractical = academicPractical,
            globalRumorLevel = globalRumorLevel,
            npcRelations = new Dictionary<int, (int guanxi, int affection)>(),
            completedEvents = new HashSet<int>(),
            flags = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
        };
    }

    private bool Evaluate(GameEvent e, EventEvaluationContext ctx) => _eventManager.EvaluateConditions(e, ctx);

    // =====================================================================
    // 1. DAY BOUNDARIES
    // =====================================================================

    [TestCase(9, false, TestName = "DayBoundary_9_OneBeforeStart_False")]
    [TestCase(10, true, TestName = "DayBoundary_10_AtStart_True")]
    [TestCase(20, true, TestName = "DayBoundary_20_AtEnd_True")]
    [TestCase(21, false, TestName = "DayBoundary_21_OneAfterEnd_False")]
    public void DayBoundary_Range10To20(int day, bool expected)
    {
        var evt = MakeEvent(minDay: 10, maxDay: 20);
        var ctx = MakeContext(currentDay: day);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    // =====================================================================
    // 2. STAT BOUNDARIES (threshold-1 / threshold / threshold+1 per stat)
    // =====================================================================

    [TestCase(39, false)]
    [TestCase(40, true)]
    [TestCase(41, true)]
    public void StatBoundary_PhysicalHealth_Min40(int ph, bool expected)
    {
        var evt = MakeEvent(minPh: 40);
        var ctx = MakeContext(physicalHealth: ph);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(19, false)]
    [TestCase(20, true)]
    [TestCase(21, true)]
    public void StatBoundary_MentalHealth_MinBound20(int mh, bool expected)
    {
        var evt = MakeEvent(minMh: 20, maxMh: 100);
        var ctx = MakeContext(mentalHealth: mh);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(79, true)]
    [TestCase(80, true)]
    [TestCase(81, false)]
    public void StatBoundary_MentalHealth_MaxBound80(int mh, bool expected)
    {
        var evt = MakeEvent(minMh: 0, maxMh: 80);
        var ctx = MakeContext(mentalHealth: mh);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(29, false)]
    [TestCase(30, true)]
    [TestCase(31, true)]
    public void StatBoundary_Language_Min30(int lang, bool expected)
    {
        var evt = MakeEvent(minLang: 30);
        var ctx = MakeContext(languageProficiency: lang);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(24, false)]
    [TestCase(25, true)]
    [TestCase(26, true)]
    public void StatBoundary_Etiquette_Min25(int etiq, bool expected)
    {
        var evt = MakeEvent(minEtiq: 25);
        var ctx = MakeContext(culturalEtiquette: etiq);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(49, false)]
    [TestCase(50, true)]
    [TestCase(51, true)]
    public void StatBoundary_Theoretical_Min50(int theory, bool expected)
    {
        var evt = MakeEvent(minTheory: 50);
        var ctx = MakeContext(academicTheoretical: theory);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(44, false)]
    [TestCase(45, true)]
    [TestCase(46, true)]
    public void StatBoundary_Practical_Min45(int practice, bool expected)
    {
        var evt = MakeEvent(minPractice: 45);
        var ctx = MakeContext(academicPractical: practice);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    // =====================================================================
    // 3. GUANXI
    // =====================================================================

    [TestCase(49, false)]
    [TestCase(50, true)]
    [TestCase(51, true)]
    public void Guanxi_Min50(int guanxi, bool expected)
    {
        var evt = MakeEvent(npcId: 101, minGuanxi: 50, minAffectionState: 0);
        var ctx = MakeContext();
        ctx.npcRelations[101] = (guanxi, 3); // affection dimaksimalkan agar tidak ikut menggagalkan
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    // =====================================================================
    // 4. AFFECTION STATE (0: Cold, 1: Neutral, 2: Friend, 3: Tokimeki)
    // =====================================================================

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void Affection_Min1_NeutralBoundary(int affection, bool expected)
    {
        var evt = MakeEvent(npcId: 101, minGuanxi: 0, minAffectionState: 1);
        var ctx = MakeContext();
        ctx.npcRelations[101] = (999, affection); // guanxi dimaksimalkan agar tidak ikut menggagalkan
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Affection_Min2_FriendBoundary(int affection, bool expected)
    {
        var evt = MakeEvent(npcId: 101, minGuanxi: 0, minAffectionState: 2);
        var ctx = MakeContext();
        ctx.npcRelations[101] = (999, affection);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Affection_Min3_TokimekiBoundary(int affection, bool expected)
    {
        var evt = MakeEvent(npcId: 101, minGuanxi: 0, minAffectionState: 3);
        var ctx = MakeContext();
        ctx.npcRelations[101] = (999, affection);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    // =====================================================================
    // 5. RUMOR LEVEL (0..3)
    // =====================================================================

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void Rumor_Min1(int rumor, bool expected)
    {
        var evt = MakeEvent(minRumorLevel: 1);
        var ctx = MakeContext(globalRumorLevel: rumor);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Rumor_Min2(int rumor, bool expected)
    {
        var evt = MakeEvent(minRumorLevel: 2);
        var ctx = MakeContext(globalRumorLevel: rumor);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Rumor_Min3(int rumor, bool expected)
    {
        var evt = MakeEvent(minRumorLevel: 3);
        var ctx = MakeContext(globalRumorLevel: rumor);
        Assert.AreEqual(expected, Evaluate(evt, ctx));
    }

    // =====================================================================
    // 6. STORY FLAGS
    // NOTE: tbl_events / GameEvent expose exactly ONE reqFlagName+reqFlagVal per event
    // (no list of required flags exists in the schema). "Multiple required flags" is
    // therefore tested as: (a) unrelated flags present in the context must not interfere,
    // and (b) two independent events each gated by their own distinct flag behave correctly
    // against the same shared context.
    // =====================================================================

    [Test]
    public void Flag_Absent_ReturnsFalse()
    {
        var evt = MakeEvent(reqFlagName: "met_xiang_bai", reqFlagVal: 1);
        var ctx = MakeContext();
        ctx.flags["met_xiang_bai"] = 0; // eksplisit "belum diset" — hindari fallback ke FlagManager.Instance
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void Flag_PresentAtRequiredValue_ReturnsTrue()
    {
        var evt = MakeEvent(reqFlagName: "met_xiang_bai", reqFlagVal: 1);
        var ctx = MakeContext();
        ctx.flags["met_xiang_bai"] = 1;
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void Flag_PresentBelowRequiredValue_ReturnsFalse()
    {
        var evt = MakeEvent(reqFlagName: "trust_level", reqFlagVal: 2);
        var ctx = MakeContext();
        ctx.flags["trust_level"] = 1;
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void Flag_UnrelatedFlagsInContext_DoNotInterfere()
    {
        var evt = MakeEvent(reqFlagName: "flagA", reqFlagVal: 1);
        var ctx = MakeContext();
        ctx.flags["flagA"] = 1;
        ctx.flags["flagB"] = 0;
        ctx.flags["flagC"] = 5;
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void Flag_TwoIndependentEvents_EachGatedByOwnFlag()
    {
        var evtA = MakeEvent(eventId: 9101, reqFlagName: "flagA", reqFlagVal: 1);
        var evtB = MakeEvent(eventId: 9102, reqFlagName: "flagB", reqFlagVal: 1);
        var ctx = MakeContext();
        ctx.flags["flagA"] = 1;
        ctx.flags["flagB"] = 0;

        Assert.IsTrue(Evaluate(evtA, ctx), "Event A's own flag is satisfied");
        Assert.IsFalse(Evaluate(evtB, ctx), "Event B's own flag is not satisfied");
    }

    // =====================================================================
    // 7. EVENT PREREQUISITES
    //
    // BUG FOUND & FIXED: EvaluateConditions() step 7 used to be
    //   if (!ctx.completedEvents.Contains(id) && !IsEventCompleted(id)) return false;
    // IsEventCompleted() queries DatabaseManager.Instance directly. Because of the way
    // the previous production code was written, calling this method to check whether a
    // prerequisite was NOT yet completed via a mocked context could not be distinguished
    // from "context does not track this" and always fell through to a real SQLite query —
    // this API silently touched the live database. This is why EventConditionTestRunner
    // (the existing BVA tool) never exercised prerequisite events at all.
    // Fix: EventEvaluationContext.CreateFromRuntime() now pre-loads the full completed-event
    // set once, so ctx.completedEvents is the single source of truth for BOTH runtime and
    // mocked evaluation, and the redundant IsEventCompleted() fallback was removed. Runtime
    // behavior for real gameplay is unchanged (a completed event is still detected as
    // completed); only the mocked/testable path becomes fully deterministic.
    // =====================================================================

    [Test]
    public void Prerequisite_Incomplete_ReturnsFalse()
    {
        var evt = MakeEvent(prereqEventId: 500);
        var ctx = MakeContext(); // completedEvents kosong -> prasyarat belum selesai
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void Prerequisite_Complete_ReturnsTrue()
    {
        var evt = MakeEvent(prereqEventId: 500);
        var ctx = MakeContext();
        ctx.completedEvents.Add(500);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    // =====================================================================
    // 8. WORKDAY / WEEKEND
    // =====================================================================

    [Test]
    public void DayType_WorkdayEvent_OnWorkday_ReturnsTrue()
    {
        var evt = MakeEvent(dayType: "Workday");
        var ctx = MakeContext(isWorkday: true);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void DayType_WorkdayEvent_OnWeekend_ReturnsFalse()
    {
        var evt = MakeEvent(dayType: "Workday");
        var ctx = MakeContext(isWorkday: false);
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void DayType_WeekendEvent_OnWeekend_ReturnsTrue()
    {
        var evt = MakeEvent(dayType: "Weekend");
        var ctx = MakeContext(isWorkday: false);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void DayType_WeekendEvent_OnWorkday_ReturnsFalse()
    {
        var evt = MakeEvent(dayType: "Weekend");
        var ctx = MakeContext(isWorkday: true);
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    // =====================================================================
    // 9. TIME BLOCK
    // =====================================================================

    [Test]
    public void TimeBlock_ExactMatch_Pagi_ReturnsTrue()
    {
        var evt = MakeEvent(timeBlock: "Pagi");
        var ctx = MakeContext(currentTimeBlock: TimeBlock.Pagi);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void TimeBlock_WrongBlock_PagiEventDuringSiang_ReturnsFalse()
    {
        var evt = MakeEvent(timeBlock: "Pagi");
        var ctx = MakeContext(currentTimeBlock: TimeBlock.Siang);
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void TimeBlock_ExactMatch_Malam_ReturnsTrue()
    {
        var evt = MakeEvent(timeBlock: "Malam");
        var ctx = MakeContext(currentTimeBlock: TimeBlock.Malam);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    [Test]
    public void TimeBlock_WrongBlock_MalamEventDuringPagi_ReturnsFalse()
    {
        var evt = MakeEvent(timeBlock: "Malam");
        var ctx = MakeContext(currentTimeBlock: TimeBlock.Pagi);
        Assert.IsFalse(Evaluate(evt, ctx));
    }

    [Test]
    public void TimeBlock_Any_MatchesAnyBlock_ReturnsTrue()
    {
        var evt = MakeEvent(timeBlock: "Any");
        var ctx = MakeContext(currentTimeBlock: TimeBlock.Siang);
        Assert.IsTrue(Evaluate(evt, ctx));
    }

    // =====================================================================
    // 10. COMBINED CONDITIONS
    // =====================================================================

    private static GameEvent MakeFullyGatedEvent()
    {
        return MakeEvent(
            minDay: 10, maxDay: 20,
            timeBlock: "Siang", dayType: "Workday",
            minLang: 30, minEtiq: 25, minMh: 20, maxMh: 100, minPh: 40, minTheory: 50, minPractice: 45,
            npcId: 101, minGuanxi: 50, minAffectionState: 2,
            minRumorLevel: 1,
            prereqEventId: 500,
            reqFlagName: "flagA", reqFlagVal: 1);
    }

    private static EventEvaluationContext MakeFullySatisfyingContext()
    {
        var ctx = MakeContext(
            currentDay: 15, currentTimeBlock: TimeBlock.Siang, isWorkday: true,
            languageProficiency: 30, culturalEtiquette: 25, mentalHealth: 50, physicalHealth: 40,
            academicTheoretical: 50, academicPractical: 45,
            globalRumorLevel: 1);
        ctx.npcRelations[101] = (50, 2);
        ctx.completedEvents.Add(500);
        ctx.flags["flagA"] = 1;
        return ctx;
    }

    [Test]
    public void Combined_AllConditionsSatisfied_ReturnsTrue()
    {
        Assert.IsTrue(Evaluate(MakeFullyGatedEvent(), MakeFullySatisfyingContext()));
    }

    [Test]
    public void Combined_AllSatisfiedExceptStatThreshold_ReturnsFalse()
    {
        var ctx = MakeFullySatisfyingContext();
        ctx.academicTheoretical = 49; // di bawah minTheory: 50
        Assert.IsFalse(Evaluate(MakeFullyGatedEvent(), ctx));
    }

    [Test]
    public void Combined_AllSatisfiedExceptFlag_ReturnsFalse()
    {
        var ctx = MakeFullySatisfyingContext();
        ctx.flags["flagA"] = 0;
        Assert.IsFalse(Evaluate(MakeFullyGatedEvent(), ctx));
    }

    [Test]
    public void Combined_AllSatisfiedExceptPrerequisite_ReturnsFalse()
    {
        var ctx = MakeFullySatisfyingContext();
        ctx.completedEvents.Clear();
        Assert.IsFalse(Evaluate(MakeFullyGatedEvent(), ctx));
    }

    // =====================================================================
    // GUARD CLAUSES
    // =====================================================================

    [Test]
    public void EvaluateConditions_NullEvent_ReturnsFalse()
    {
        Assert.IsFalse(Evaluate(null, MakeContext()));
    }

    [Test]
    public void EvaluateConditions_NullContext_ReturnsFalse()
    {
        Assert.IsFalse(Evaluate(MakeEvent(), null));
    }
}
#endif
