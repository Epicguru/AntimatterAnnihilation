using AntimatterAnnihilation.Buildings;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.UI
{
    public class UI_PowerNetConsole : Window
    {
        public static bool InstantFlickActive
        {
            get
            {
                if (AADefOf.InstantFlick_AA == null)
                {
                    Log.Error("Failed to get ref to research project.");
                    return false;
                }

                return AADefOf.InstantFlick_AA.IsFinished;
            }
        }

        private static ObjectPool<Category> pool = new ObjectPool<Category>() { CreateFunction = () => new Category() };
        private static FieldInfo WantsToBeOnInfo
        {
            get
            {

                if (fieldInt == null)
                {
                    fieldInt = typeof(CompFlickable).GetField("wantSwitchOn", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return fieldInt;
            }
        }
        private static FieldInfo fieldInt;


        public static UI_PowerNetConsole Open(Building_PowerNetConsole cons)
        {
            var created = new UI_PowerNetConsole(cons);
            Find.WindowStack?.Add(created);
            return created;
        }

        public struct PowerThing
        {
            public Thing Thing;
            public CompPowerTrader PowerTrader;

            public PowerThing(Thing thing, CompPowerTrader trader)
            {
                this.Thing = thing;
                this.PowerTrader = trader;
            }

            public CompFlickable FlickComp
            {
                get
                {
                    return Thing?.TryGetComp<CompFlickable>();
                }
            }

            public float Watts
            {
                get
                {
                    if (PowerTrader?.PowerOn ?? false)
                        return PowerTrader.PowerOutput;
                    else
                        return 0;
                }
            }

            public void Flick()
            {
                if (FlickComp == null)
                    return;

                bool currentWantsToBeOn = (bool)WantsToBeOnInfo.GetValue(FlickComp);
                WantsToBeOnInfo.SetValue(FlickComp, !currentWantsToBeOn);

                if (InstantFlickActive)
                {
                    FlickComp.DoFlick();
                }
                else
                {
                    FlickUtility.UpdateFlickDesignation(Thing);
                }
            }

            public bool IsFlickedOn()
            {
                if (FlickComp == null)
                    return false;

                if (InstantFlickActive)
                    return FlickComp.SwitchIsOn;
                else
                    return (bool)WantsToBeOnInfo.GetValue(FlickComp);
            }
        }
        public override Vector2 InitialSize => new Vector2(620, 650);

        private Building_PowerNetConsole console;

        private UI_PowerNetConsole(Building_PowerNetConsole cons)
        {
            this.console = cons;
            cons.OnDestroyed += OnConsoleDestroyed;
            resizeable = true;
            doCloseX = true;
            draggable = true;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        private void OnConsoleDestroyed()
        {
            Close();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            Refresh();
        }

        public override void PreClose()
        {
            base.PreClose();

            console.OnDestroyed -= OnConsoleDestroyed;
            console = null;
        }

        public int RefreshInterval = 500;
        public SortMode SortingMode = SortMode.Name;
        public bool Ascending = true;
        public List<PowerThing> PowerThings = new List<PowerThing>();
        public Dictionary<string, Category> Categories = new Dictionary<string, Category>();
        public List<Category> AllCats = new List<Category>();

        private Vector2 scroll;
        private Stopwatch st = new Stopwatch();
        private Dictionary<string, bool> expanded = new Dictionary<string, bool>();
        private float lastHeight;
        private float[] columnWidths = { 262, 110, 120 };
        private float[] cumulativeWidths = { 0, 262, 372 };

        public void Refresh()
        {
            PowerThings.Clear();

            if (console?.PowerTraderComp?.PowerNet == null)
                return;

            if (!console.PowerTraderComp.PowerOn)
                return;

            foreach (var trader in console.PowerTraderComp.PowerNet.powerComps)
            {
                var thing = new PowerThing(trader.parent, trader);
                PowerThings.Add(thing);
            }

            Categorize();
            Sort();
        }

        private void ResetCategories()
        {
            foreach (var cat in AllCats)
            {
                pool.Return(cat);
            }
            AllCats.Clear();
            Categories.Clear();
        }

        private void Categorize()
        {
            ResetCategories();

            foreach (var thing in PowerThings)
            {
                string defName = thing.Thing.def.defName;
                if (Categories.ContainsKey(defName))
                {
                    var cat = Categories[defName];
                    cat.Things.Add(thing);
                }
                else
                {
                    var cat = pool.GetOrCreate();
                    cat.DefName = thing.Thing.def.defName;
                    cat.Label = thing.Thing.LabelShortCap;
                    cat.Things.Add(thing);

                    AllCats.Add(cat);
                    Categories.Add(defName, cat);
                }
            }
        }

        private void Sort()
        {
            switch (SortingMode)
            {
                case SortMode.Name:
                    AllCats.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.Ordinal) * (Ascending ? 1 : -1));
                    break;
                case SortMode.Power:
                    AllCats.Sort((a, b) => (int)(a.TotalPower - b.TotalPower) * (Ascending ? 1 : -1));
                    break;
                case SortMode.Enabled:
                    AllCats.Sort((a, b) =>
                    {
                        bool aState = a.AreAllEnabled(out int aCount);
                        bool bState = b.AreAllEnabled(out int bCount);

                        if (aCount == 0)
                            return 1;
                        if (bCount == 0)
                            return -1;

                        int val;
                        if (aState && !bState)
                            val = 1;
                        else if (!aState && bState)
                            val = -1;
                        else
                            val = 0;

                        if (!Ascending)
                            val *= -1;

                        return val;

                    });
                    break;
            }
        }

        public bool IsExpanded(string defName)
        {
            bool hasValue = expanded.TryGetValue(defName, out bool exp);
            return hasValue && exp;
        }

        public void SetExpanded(string defName, bool exp)
        {
            if (expanded.ContainsKey(defName))
                expanded[defName] = exp;
            else
                expanded.Add(defName, exp);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (console == null)
                return;

            if (!st.IsRunning)
                st.Start();

            if(st.ElapsedMilliseconds >= RefreshInterval)
            {
                st.Restart();
                Refresh();
            }

            bool indented = false;
            int column = 0;
            float itemHeight;

            Text.Font = GameFont.Medium;
            Widgets.Label(GetItemRect(40), $"<b>{"AA.PowerNetInfo".Translate()}</b>");
            float totalPower = console.PowerTraderComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            string totalPowerText = GetPrettyPower(totalPower);
            if (totalPower > 0)
                totalPowerText = $"<color=green>{totalPowerText}</color>";
            if (totalPower < 0)
                totalPowerText = $"<color=red>{totalPowerText}</color>";
            Widgets.Label(new Rect(inRect.x + 200, inRect.y, 200, 32), totalPowerText);

            if (InstantFlickActive)
            {
                string flickActiveText = $"<color=yellow>{"AA.InstantFlickActive".Translate()}</color>";
                var flickActiveSize = Text.CalcSize(flickActiveText);
                Widgets.Label(new Rect(inRect.xMax - flickActiveSize.x - 5, inRect.y, flickActiveSize.x, 32), flickActiveText);
            }
            MoveDown(45);
            Text.Font = GameFont.Small;

            if (console?.PowerTraderComp?.PowerNet == null)
            {
                Widgets.Label(GetItemRect(40), "AA.ConsoleNotConnected".Translate());
                return;
            }
            if (!console.PowerTraderComp.PowerOn)
            {
                Widgets.Label(GetItemRect(40), "AA.ConsoleNotPowered".Translate());
                return;
            }

            GetItemRect(32);

            string ascDec = Ascending ? "AA.Ascending".Translate() : "AA.Descending".Translate();
            var isCat = true;
            // Name.
            if(Widgets.ButtonTextSubtle(GetColumnRect(), $"<b>{"AA.Name".Translate().CapitalizeFirst()}</b> {(SortingMode == SortMode.Name ? ascDec : "")}"))
            {
                if (SortingMode != SortMode.Name)
                {
                    SortingMode = SortMode.Name;
                    Ascending = true;
                    Refresh();
                    return;
                }
                else
                {
                    Ascending = !Ascending;
                    Refresh();
                    return;
                }
            }

            // Power in/out.
            if(Widgets.ButtonTextSubtle(GetColumnRect(), $"<b>{"AA.Power".Translate().CapitalizeFirst()}</b> {(SortingMode == SortMode.Power ? ascDec : "")}"))
            {
                {
                    if (SortingMode != SortMode.Power)
                    {
                        SortingMode = SortMode.Power;
                        Ascending = true;
                        Refresh();
                        return;
                    }
                    else
                    {
                        Ascending = !Ascending;
                        Refresh();
                        return;
                    }
                }
            }

            // Turn on/off
            if (Widgets.ButtonTextSubtle(GetColumnRect(), $"<b>{"AA.Enabled".Translate().CapitalizeFirst()}</b> {(SortingMode == SortMode.Enabled ? ascDec : "")}"))
            {
                {
                    if (SortingMode != SortMode.Enabled)
                    {
                        SortingMode = SortMode.Enabled;
                        Ascending = true;
                        Refresh();
                        return;
                    }
                    else
                    {
                        Ascending = !Ascending;
                        Refresh();
                        return;
                    }
                }
            }

            MoveDown(32 + 10f);

            Widgets.BeginScrollView(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 100), ref scroll, new Rect(inRect.x, inRect.y, inRect.width - 20, lastHeight));
            lastHeight = 0f;

            foreach (var cat in AllCats)
            {
                if(cat.Count == 1)
                {
                    indented = false;
                    isCat = true;
                    DrawThing(cat.Things[0]);
                }
                else
                {
                    indented = false;
                    isCat = true;
                    var r = GetItemRect(36f);
                    column = 0;
                    Widgets.DrawBox(r, 2);

                    var expRect = new Rect(inRect.x, inRect.y, 32, 32);
                    bool isExp = IsExpanded(cat.DefName);
                    if (Widgets.ButtonImage(expRect, isExp ? Content.Collapse : Content.Expand))
                    {
                        isExp = !isExp;
                        SetExpanded(cat.DefName, isExp);
                    }

                    // Name.
                    var labelRect = GetColumnRect();
                    Widgets.Label(labelRect.GetInner(), cat.Label + $" x{cat.Count}");

                    if (Widgets.ButtonInvisible(labelRect))
                    {
                        Vector3 averagePos = Vector3.zero;
                        if (!Input.GetKey(KeyCode.LeftShift))
                            Find.Selector.ClearSelection();
                        foreach (var thing in cat.Things)
                        {
                            averagePos += thing.Thing.DrawPos;
                            Find.Selector.Select(thing.Thing);
                        }
                        averagePos /= cat.Things.Count;

                        Find.CameraDriver.JumpToCurrentMapLoc(averagePos);
                    }

                    // Power in/out.
                    float catPower = cat.TotalPower;
                    string wattText = GetPrettyPower(catPower);
                    if (catPower > 0)
                        wattText = $"<color=green>{wattText}</color>";
                    if (catPower < 0)
                        wattText = $"<color=red>{wattText}</color>";

                    Widgets.Label(GetColumnRect().GetInner(), wattText);

                    // Turn on/off
                    var fr = GetColumnRect();
                    bool on = cat.AreAllEnabled(out int flickable);

                    if (flickable > 0 && Widgets.ButtonText(fr.GetInner(3), $"{"AA.TurnAll".Translate()} {(on ? "AA.Off".Translate() : "AA.On".Translate())}"))
                    {
                        cat.FlickAll(on);
                        Refresh();
                        Widgets.EndScrollView();
                        return;
                    }

                    MoveDown(32 + 10f);

                    isCat = false;

                    if (!isExp)
                        continue;
                    indented = true;
                    foreach(var thing in cat.Things)
                    {
                        DrawThing(thing);
                    }
                }
            }

            Widgets.EndScrollView();

            void DrawThing(PowerThing thing)
            {
                var r = GetItemRect(32);
                column = 0;
                Widgets.DrawBox(r);

                // Name.
                Rect nameRect = GetColumnRect();
                Widgets.Label(nameRect.GetInner(), thing.Thing.LabelShortCap);

                // Power in/out.
                string wattText = GetPrettyPower(thing.Watts);
                if (thing.Watts > 0)
                    wattText = $"<color=green>{wattText}</color>";
                if (thing.Watts < 0)
                    wattText = $"<color=red>{wattText}</color>";

                Widgets.Label(GetColumnRect().GetInner(), wattText);

                // Turn on/off
                var fr = GetColumnRect();
                if (thing.FlickComp != null)
                {
                    if (Widgets.ButtonText(fr.GetInner(3), $"{"AA.Turn".Translate().CapitalizeFirst()} {(thing.IsFlickedOn() ? "AA.Off".Translate() : "AA.On".Translate())}"))
                    {
                        thing.Flick();
                    }
                }

                if (Widgets.ButtonInvisible(nameRect))
                {
                    Find.CameraDriver.JumpToCurrentMapLoc(thing.Thing.DrawPos);
                    if(!Input.GetKey(KeyCode.LeftShift))
                        Find.Selector.ClearSelection();
                    Find.Selector.Select(thing.Thing);
                }

                MoveDown(32 + 10f);
            }

            Rect GetItemRect(float height)
            {
                int add = indented ? 32 : 0;
                var r = new Rect(inRect.x + add, inRect.y, inRect.width - 20 - add, height);
                itemHeight = height;
                return r;
            }

            Rect GetColumnRect()
            {
                int add = indented ? 32 : 0;
                if (isCat)
                    add += 32;
                var r = new Rect(inRect.x + cumulativeWidths[column] + add, inRect.y,  columnWidths[column], itemHeight);
                column++;
                return r;
            }

            void MoveDown(float amount)
            {
                inRect.y += amount;
                inRect.height += amount;
                lastHeight += amount;
            }
        }

        public string GetPrettyPower(float watts)
        {
            const float KILO = 1000f;
            const float MEGA = 1000000f;
            const float GIGA = 1000000000f;

            bool positive = watts > 0;
            bool negative = watts < 0;

            watts = Mathf.Abs(watts);

            string retVal = $"{(positive ? "+" : negative ? "-" : "")} ";

            if (watts >= GIGA)
            {
                retVal += $"{watts / GIGA:N2} {"AA.GigaWatts".Translate()}";
            }
            else if (watts >= MEGA)
            {
                retVal += $"{watts / MEGA:N2} {"AA.MegaWatts".Translate()}";
            }
            else if (watts >= KILO)
            {
                retVal += $"{watts / KILO:N1} {"AA.KiloWatts".Translate()}";
            }
            else
            {
                retVal += $"{watts:N0} {"AA.Watts".Translate()}";
            }

            return retVal;
        }

        public enum SortMode
        {
            Name,
            Power,
            Enabled
        }

        public class Category : IResettable
        {
            public string DefName;
            public string Label;
            public int Count
            {
                get
                {
                    return Things.Count;
                }
            }

            public float TotalPower
            {
                get
                {
                    float p = 0f;
                    foreach (var thing in Things)
                    {
                        p += thing.Watts;
                    }
                    return p;
                }
            }
            public List<PowerThing> Things = new List<PowerThing>();

            public bool AreAllEnabled(out int haveComp)
            {
                haveComp = 0;
                foreach (var thing in Things)
                {
                    if (thing.FlickComp == null)
                        continue;

                    haveComp++;
                    if (!thing.IsFlickedOn())
                        return false;
                }
                return true;
            }

            public void FlickAll(bool startingState)
            {
                foreach (var thing in Things)
                {
                    var fc = thing.FlickComp;
                    if (fc == null)
                        continue;

                    bool currentState = InstantFlickActive ? fc.SwitchIsOn : (bool)WantsToBeOnInfo.GetValue(fc);

                    if (startingState && currentState)
                        thing.Flick();
                    else if (!startingState && !currentState)
                        thing.Flick();
                }
            }

            public void Reset()
            {
                Things.Clear();
            }
        }
    }
}
