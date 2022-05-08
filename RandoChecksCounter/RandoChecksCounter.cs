using ItemChanger;
using ItemChanger.Internal;
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
        private TextObject? text;

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

        int PlacementScore(AbstractPlacement p)
        {
            if (p.Name != LocationNames.Start)
            {
                if (p.CheckVisitedAny(VisitState.Previewed | VisitState.ObtainedAnyItem))
                {
                    return 1;
                }
            }
            return 0;
        }

        int CountCheckedLocations() => Ref.Settings.GetPlacements().Select(PlacementScore).Sum();

        // if you need preloads, you will need to implement GetPreloadNames and use the other signature of Initialize.
        public override void Initialize()
        {
            Log("Initializing");

            Events.OnEnterGame += OnEnterGame;
            AbstractPlacement.OnVisitStateChangedGlobal += OnPlacementChecked;
            Events.OnItemChangerUnhook += OnExitGame;

            Log("Initialized");
        }

        int counter = 0;
        private void OnPlacementChecked(VisitStateChangedEventArgs e)
        {
            if (!e.NoChange && PlacementScore(e.Placement) == 0 && e.Placement.Name != "Start")
            {
                if (e.NewFlags.HasFlag(VisitState.Previewed) || e.NewFlags.HasFlag(VisitState.ObtainedAnyItem))
                {
                    counter++;
                    if (text != null)
                    {
                        text.Text = $"Locations Checked: {counter}";
                    }
                }
            }
        }

        private void OnEnterGame()
        {
            counter = CountCheckedLocations();
            layout = new LayoutRoot(true, "Live Counter");
            text = new TextObject(layout)
            {
                Text = $"Locations Checked: {counter}",
                Font = UI.TrajanNormal,
                FontSize = 25,
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
