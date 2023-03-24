using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Tags;
using MagicUI.Core;
using MagicUI.Elements;
using Modding;
using System;
using System.Linq;

namespace RandoChecksCounter
{
    public class RandoChecksCounter : Mod
    {
        private static RandoChecksCounter? _instance;
        private LayoutRoot? layout;
        private TextFormatter<int>? text;

        internal static RandoChecksCounter Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(RandoChecksCounter)} was never initialized");
                }
                return _instance;
            }
        }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public RandoChecksCounter() : base()
        {
            _instance = this;
        }

        // rather than ignore start specifically, ignore placements weighted 0 - this is a sensible and free
        // way to ignore remote placements in sane multiworlds.
        // side effect - also ignores area blitz nothings. not sure if desirable (probably this is not a bad thing), may make a setting
        bool ShouldIgnorePlacement(AbstractPlacement p) => p.GetTag(out CompletionWeightTag t) && t.Weight == 0;

        bool IsChecked(AbstractPlacement p)
        {
            if (!ShouldIgnorePlacement(p))
            {
                if (p.CheckVisitedAny(VisitState.Previewed | VisitState.ObtainedAnyItem))
                {
                    return true;
                }
            }
            return false;
        }

        int CountCheckedLocations() => Ref.Settings.GetPlacements().Where(IsChecked).Count();

        public override void Initialize()
        {
            Log("Initializing");

            Events.OnEnterGame += OnEnterGame;
            AbstractPlacement.OnVisitStateChangedGlobal += OnPlacementChecked;
            Events.OnItemChangerUnhook += OnExitGame;

            Log("Initialized");
        }

        private void OnPlacementChecked(VisitStateChangedEventArgs e)
        {
            // VisitStateChanged happens before any actual change occurs, so we pre-check if the location was checked already
            // before now to avoid double-counting
            if (!e.NoChange && !IsChecked(e.Placement) && !ShouldIgnorePlacement(e.Placement))
            {
                if (e.NewFlags.HasFlag(VisitState.Previewed) || e.NewFlags.HasFlag(VisitState.ObtainedAnyItem))
                {
                    if (text != null)
                    {
                        text.Data++;
                    }
                }
            }
        }

        private void OnEnterGame()
        {
            int initialCounter = CountCheckedLocations();
            layout = new LayoutRoot(true, "Live Counter");

            TextObject internalText = new(layout)
            {
                Font = UI.TrajanNormal,
                FontSize = 25
            };
            text = new TextFormatter<int>(layout, initialCounter, (counter) => $"Locations Checked: {counter}")
            {
                Text = internalText,
                Padding = new Padding(10),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
        }

        private void OnExitGame()
        {
            text?.Destroy();
            layout?.Destroy();
            text = null;
            layout = null;
        }
    }
}
