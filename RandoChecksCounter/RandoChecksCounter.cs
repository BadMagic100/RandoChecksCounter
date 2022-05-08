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

        bool IsChecked(AbstractPlacement p)
        {
            if (p.Name != LocationNames.Start)
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

        int counter = 0;
        private void OnPlacementChecked(VisitStateChangedEventArgs e)
        {
            // VisitStateChanged happens before any actual change occurs, so we pre-check if the location was checked already
            // before now to avoid double-counting
            if (!e.NoChange && !IsChecked(e.Placement) && e.Placement.Name != "Start")
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
