// ═══════════════════════════════════════════════════════════════════════════════
//  DomFlowVisualizer.cs  —  Layer 1: Binned DOM Bid/Ask Bar Chart
//  NinjaTrader 8 Custom Indicator
//
//  WHAT THIS DOES
//  ──────────────
//  Subscribes to the live Depth of Market (DOM) feed, aggregates raw price
//  levels into fixed-size price bins, then renders a horizontal bar chart
//  in a dedicated panel beneath the price chart:
//
//        <── bid volume | ask volume ──>
//        [████████       |    ████████ ]  ← each row = one price bin
//        [██████████████ |  ██         ]
//        [████           |████████████ ]
//                        ↑
//                    center line
//
//  HOW TO INSTALL
//  ──────────────
//  1. In NinjaTrader → Tools → Edit NinjaScript → Indicators
//  2. Paste this file, click Compile
//  3. Add the indicator to any chart — it will appear in its own panel
//
//  ARCHITECTURE (three layers, this file is Layer 1)
//  ──────────────────────────────────────────────────
//  Layer 1  →  Binned DOM bar chart                  ← YOU ARE HERE
//  Layer 2  →  Smoothing / spoofing filter            (future)
//  Layer 3  →  Cumulative delta line overlay          (future)
// ═══════════════════════════════════════════════════════════════════════════════

#region Using declarations
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using SharpDX;
using SharpDX.Direct2D1;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class DomFlowVisualizer : Indicator
    {
        // ═══════════════════════════════════════════════════════════════════
        //  SECTION 1 — RAW DOM DATA STORES
        //
        //  NinjaTrader fires OnMarketDepth() on a background thread whenever
        //  the order book changes. OnRender() runs on the UI thread.
        //  ConcurrentDictionary lets both threads read/write without locks.
        //
        //  Key   = exact price level (e.g. 5012.25)
        //  Value = current contracts sitting at that level (snapshot, not delta)
        // ═══════════════════════════════════════════════════════════════════
        private ConcurrentDictionary<double, long> bidVolumes; // bid side of the book
        private ConcurrentDictionary<double, long> askVolumes; // ask side of the book

        // ═══════════════════════════════════════════════════════════════════
        //  SECTION 2 — BINNED (AGGREGATED) VOLUME STORES
        //
        //  Raw DOM has one entry per tick increment — too granular to read
        //  visually. We bucket adjacent price levels together into "bins."
        //
        //  Example with BinSizeTicks = 4 on ES (tick = 0.25):
        //    bin size = 4 * 0.25 = 1.0 point
        //    prices 5012.00, 5012.25, 5012.50, 5012.75  →  one bin at 5012.00
        //
        //  These are written only inside OnRender (single UI thread), so a
        //  regular Dictionary is fine here — no concurrent access.
        // ═══════════════════════════════════════════════════════════════════
        private Dictionary<double, long> binnedBid;
        private Dictionary<double, long> binnedAsk;

        // The highest volume found in any single bin during the current render
        // pass. Used to scale all bar widths proportionally (like a bar chart).
        private long maxBinnedVolume;

        // ═══════════════════════════════════════════════════════════════════
        //  SECTION 3 — RENDERING RESOURCES (SharpDX / Direct2D)
        //
        //  NinjaTrader 8 chart indicators render via Direct2D (GPU-accelerated).
        //  Brushes are GPU resources — they must be:
        //    • created in OnRenderTargetChanged() (when the surface is ready)
        //    • disposed in OnRenderTargetChanged() + State.Terminated (cleanup)
        //
        //  You cannot create these in OnStateChange because the GPU surface
        //  doesn't exist yet at that point in the lifecycle.
        // ═══════════════════════════════════════════════════════════════════
        private SharpDX.Direct2D1.SolidColorBrush bidBrushDx;      // bid bar fill
        private SharpDX.Direct2D1.SolidColorBrush askBrushDx;      // ask bar fill
        private SharpDX.Direct2D1.SolidColorBrush priceLineBrushDx;// current price line
        private SharpDX.Direct2D1.SolidColorBrush centerLineBrushDx;// bid/ask divider
        private SharpDX.Direct2D1.SolidColorBrush labelBrushDx;    // volume label text

        // Cached from the instrument to avoid repeated property lookups per tick
        private double tickSize;


        // ═══════════════════════════════════════════════════════════════════
        //  LIFECYCLE — OnStateChange
        //
        //  NinjaTrader calls this method each time the indicator moves through
        //  its state machine:
        //    SetDefaults → Configure → Active → (Finalized) → Terminated
        // ═══════════════════════════════════════════════════════════════════
        protected override void OnStateChange()
        {
            // ── SetDefaults ─────────────────────────────────────────────
            // Runs first, before any connection. Sets the values users see
            // in the indicator properties dialog before they change anything.
            if (State == State.SetDefaults)
            {
                Name                     = "DOM Flow Visualizer";
                Description              = "Binned bid/ask DOM depth plotted as a horizontal bar chart.";

                // Calculate.OnEachTick ensures OnBarUpdate fires on every tick
                // so the price line stays current, even though DOM data arrives
                // independently through OnMarketDepth
                Calculate                = Calculate.OnEachTick;

                // false = indicator draws in its OWN panel below price
                // true  = indicator draws on top of the price candles
                IsOverlay                = false;

                DisplayInDataBox         = false;
                DrawOnPricePanel         = false;

                // IsSuspendedWhileInactive stops updates when the chart tab
                // is not visible, saving CPU during heavy DOM activity
                IsSuspendedWhileInactive = true;

                // ── Default user-configurable values ──────────────────────
                BinSizeTicks   = 4;  // 4 ticks per bin (1 point on ES)

                // Brush is the NT8 standard for color properties (matches SuperDom column pattern).
                // We use SolidColorBrush with an explicit alpha so bars are semi-transparent.
                BidColor       = new System.Windows.Media.SolidColorBrush(
                                     System.Windows.Media.Color.FromArgb(200, 30, 144, 255));  // DodgerBlue
                AskColor       = new System.Windows.Media.SolidColorBrush(
                                     System.Windows.Media.Color.FromArgb(200, 220, 50, 50));   // Crimson
                PriceLineColor = new System.Windows.Media.SolidColorBrush(
                                     System.Windows.Media.Color.FromArgb(255, 255, 220, 0));   // Yellow
            }

            // ── Configure ───────────────────────────────────────────────
            // Runs after connection is established. Safe to access Instrument here.
            else if (State == State.Configure)
            {
                // Cache tick size so binning math doesn't call into NT on every DOM event
                tickSize = Instrument.MasterInstrument.TickSize;

                // Initialize the concurrent stores for raw DOM data
                bidVolumes = new ConcurrentDictionary<double, long>();
                askVolumes = new ConcurrentDictionary<double, long>();

                // Initialize the render-thread stores for binned data
                binnedBid = new Dictionary<double, long>();
                binnedAsk = new Dictionary<double, long>();
            }

            // ── Terminated ──────────────────────────────────────────────
            // Runs when the indicator is removed or the chart closes.
            // ALWAYS release SharpDX GPU resources here — they are unmanaged
            // memory and won't be collected by the garbage collector.
            else if (State == State.Terminated)
            {
                DisposeBrushes();
            }
        }


        // ═══════════════════════════════════════════════════════════════════
        //  OnMarketDepth — the DOM feed callback
        //
        //  Fires every time a price level on the order book changes.
        //  This is the raw input for this entire indicator.
        //
        //  MarketDepthEventArgs fields we use:
        //    e.MarketDataType  → Bid or Ask side
        //    e.Operation       → Add / Update / Remove
        //    e.Price           → exact price level affected
        //    e.Volume          → total contracts at this level RIGHT NOW
        //                        (this is a snapshot, not a delta)
        //    e.IsReset         → true when the whole book is refreshed
        // ═══════════════════════════════════════════════════════════════════
        protected override void OnMarketDepth(MarketDepthEventArgs e)
        {
            // Guard: dictionaries must exist before we touch them
            if (bidVolumes == null || askVolumes == null) return;

            // ── Full book reset (e.g. reconnect or exchange resend) ─────
            // Wipe everything and let the fresh snapshot rebuild the book
            if (e.IsReset)
            {
                bidVolumes.Clear();
                askVolumes.Clear();
                return;
            }

            // ── Select which side of the book to update ─────────────────
            ConcurrentDictionary<double, long> book =
                e.MarketDataType == MarketDataType.Bid ? bidVolumes : askVolumes;

            if (e.Operation == Operation.Add || e.Operation == Operation.Update)
            {
                // Overwrite the volume at this exact price level.
                // AddOrUpdate atomically handles both "first time seen"
                // and "already exists" — we always want the latest snapshot value.
                book.AddOrUpdate(
                    key:                e.Price,
                    addValue:           e.Volume,
                    updateValueFactory: (price, oldVolume) => e.Volume  // replace, not accumulate
                );
            }
            else if (e.Operation == Operation.Remove)
            {
                // This level has been fully pulled from the book
                book.TryRemove(e.Price, out _);
            }
        }


        // ═══════════════════════════════════════════════════════════════════
        //  OnRenderTargetChanged — GPU surface lifecycle
        //
        //  Called whenever Direct2D creates a new render surface — this happens:
        //    • When the chart first loads
        //    • When the window is resized
        //    • When the chart is dragged between monitors (DPI change)
        //
        //  We MUST recreate all SharpDX brushes here because they are tied
        //  to a specific render target. Old brushes from a previous target
        //  are invalid and will crash if used.
        // ═══════════════════════════════════════════════════════════════════
        public override void OnRenderTargetChanged()
        {
            // Always dispose first — even if RenderTarget is null (surface lost)
            DisposeBrushes();

            if (RenderTarget == null) return;

            // Create GPU brushes from the user-chosen WPF colors
            bidBrushDx       = ToSolidBrush(BidColor);
            askBrushDx       = ToSolidBrush(AskColor);
            priceLineBrushDx = ToSolidBrush(PriceLineColor);

            // Fixed utility colors (not user-configurable)
            centerLineBrushDx = new SharpDX.Direct2D1.SolidColorBrush(
                RenderTarget,
                new SharpDX.Color4(1f, 1f, 1f, 0.15f) // subtle white divider
            );

            labelBrushDx = new SharpDX.Direct2D1.SolidColorBrush(
                RenderTarget,
                new SharpDX.Color4(1f, 1f, 1f, 0.60f) // dim white for volume numbers
            );
        }


        // ═══════════════════════════════════════════════════════════════════
        //  OnRender — the main drawing method
        //
        //  NinjaTrader calls this on the UI thread whenever the chart repaints.
        //  This is where all the visual output happens.
        //
        //  chartControl  → chart geometry: panel bounds, bar spacing, etc.
        //  chartScale    → price↔pixel conversion for the Y axis
        // ═══════════════════════════════════════════════════════════════════
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            // ── Guard: brushes must exist ────────────────────────────────
            if (bidBrushDx == null || askBrushDx == null) return;
            if (bidVolumes == null || askVolumes == null) return;

            // ────────────────────────────────────────────────────────────
            //  STEP 1 — COMPUTE PANEL GEOMETRY
            //
            //  The render target covers the ENTIRE chart canvas. Our indicator
            //  panel is a sub-region of that canvas. All drawing coordinates
            //  must be offset by the panel's top-left corner.
            // ────────────────────────────────────────────────────────────
            float panelLeft   = (float)ChartPanel.X;          // left edge of our panel (pixels)
            float panelTop    = (float)ChartPanel.Y;          // top edge of our panel (pixels)
            float panelWidth  = (float)ChartPanel.W;          // pixel width of our panel
            float panelHeight = (float)ChartPanel.H;          // pixel height of our panel
            float panelRight  = panelLeft + panelWidth;
            float panelBottom = panelTop  + panelHeight;

            // The vertical divider between bid (left) and ask (right) sides
            float centerX = panelLeft + panelWidth / 2f;

            // Maximum bar half-width — bars fill from center toward each edge,
            // minus a small margin so bars don't overlap the panel border
            float maxHalfWidth = (panelWidth / 2f) - 4f;

            // ────────────────────────────────────────────────────────────
            //  STEP 2 — BIN THE RAW DOM DATA
            //
            //  Walk every raw price level in the DOM dictionaries and group
            //  them into fixed-size price buckets.
            //
            //  binSizePrice = the dollar/point range each bucket covers
            //  binKey       = the lowest price of the bucket (the bucket's ID)
            //
            //  Example: binSizePrice = 1.00, price = 5012.37
            //    binKey = floor(5012.37 / 1.00) * 1.00 = 5012.00
            // ────────────────────────────────────────────────────────────
            binnedBid.Clear();
            binnedAsk.Clear();
            maxBinnedVolume = 1; // start at 1 to prevent divide-by-zero later

            double binSizePrice = tickSize * BinSizeTicks;

            // ── Aggregate bid side ───────────────────────────────────────
            foreach (KeyValuePair<double, long> kvp in bidVolumes)
            {
                // Which bin does this price level fall into?
                double binKey = Math.Floor(kvp.Key / binSizePrice) * binSizePrice;

                // Round to avoid floating-point key mismatches (e.g. 5012.000000001)
                binKey = Math.Round(binKey, 8);

                if (!binnedBid.ContainsKey(binKey))
                    binnedBid[binKey] = 0;

                binnedBid[binKey] += kvp.Value;

                // Track the largest bin volume for proportional scaling
                if (binnedBid[binKey] > maxBinnedVolume)
                    maxBinnedVolume = binnedBid[binKey];
            }

            // ── Aggregate ask side ───────────────────────────────────────
            foreach (KeyValuePair<double, long> kvp in askVolumes)
            {
                double binKey = Math.Round(Math.Floor(kvp.Key / binSizePrice) * binSizePrice, 8);

                if (!binnedAsk.ContainsKey(binKey))
                    binnedAsk[binKey] = 0;

                binnedAsk[binKey] += kvp.Value;

                if (binnedAsk[binKey] > maxBinnedVolume)
                    maxBinnedVolume = binnedAsk[binKey];
            }

            // ────────────────────────────────────────────────────────────
            //  STEP 3 — DRAW BID BARS (left side, growing leftward)
            //
            //  For each bin that has bid volume:
            //    1. Convert the bin's price range to Y pixel coordinates
            //    2. Scale bar width proportionally to maxBinnedVolume
            //    3. Draw a filled rectangle from center leftward
            // ────────────────────────────────────────────────────────────
            foreach (KeyValuePair<double, long> kvp in binnedBid)
            {
                double binBottomPrice = kvp.Key;                  // low end of this bin
                double binTopPrice    = binBottomPrice + binSizePrice; // high end of this bin
                long   volume         = kvp.Value;

                // chartScale.GetYByValue: higher prices → smaller Y (price chart convention)
                // So the top of our rectangle is at the HIGHER price
                float yTop    = (float)chartScale.GetYByValue(binTopPrice)    + 1f; // +1 inner margin
                float yBottom = (float)chartScale.GetYByValue(binBottomPrice) - 1f; // -1 inner margin

                // Skip bins that are entirely outside the visible panel area
                if (yTop > panelBottom || yBottom < panelTop) continue;

                // Clamp to panel boundaries so bars don't overflow
                yTop    = Math.Max(yTop,    panelTop);
                yBottom = Math.Min(yBottom, panelBottom);

                float barHeight = yBottom - yTop;
                if (barHeight < 1f) barHeight = 1f; // always at least 1px visible

                // Bar width = proportion of max volume, scaled to half the panel width
                float barWidth = maxHalfWidth * ((float)volume / maxBinnedVolume);

                // Bids extend LEFTWARD from center
                var rect = new SharpDX.RectangleF(
                    x:      centerX - barWidth,   // left edge
                    y:      yTop,
                    width:  barWidth,
                    height: barHeight
                );

                RenderTarget.FillRectangle(rect, bidBrushDx);
            }

            // ────────────────────────────────────────────────────────────
            //  STEP 4 — DRAW ASK BARS (right side, growing rightward)
            //
            //  Same logic as bids, but bars extend rightward from center.
            // ────────────────────────────────────────────────────────────
            foreach (KeyValuePair<double, long> kvp in binnedAsk)
            {
                double binBottomPrice = kvp.Key;
                double binTopPrice    = binBottomPrice + binSizePrice;
                long   volume         = kvp.Value;

                float yTop    = (float)chartScale.GetYByValue(binTopPrice)    + 1f;
                float yBottom = (float)chartScale.GetYByValue(binBottomPrice) - 1f;

                if (yTop > panelBottom || yBottom < panelTop) continue;

                yTop    = Math.Max(yTop,    panelTop);
                yBottom = Math.Min(yBottom, panelBottom);

                float barHeight = yBottom - yTop;
                if (barHeight < 1f) barHeight = 1f;

                float barWidth = maxHalfWidth * ((float)volume / maxBinnedVolume);

                // Asks extend RIGHTWARD from center
                var rect = new SharpDX.RectangleF(
                    x:      centerX,        // left edge = center
                    y:      yTop,
                    width:  barWidth,
                    height: barHeight
                );

                RenderTarget.FillRectangle(rect, askBrushDx);
            }

            // ────────────────────────────────────────────────────────────
            //  STEP 5 — DRAW THE CENTER DIVIDER LINE
            //
            //  A vertical line down the middle separating bid vs ask.
            //  Draw this AFTER the bars so it renders on top of them.
            // ────────────────────────────────────────────────────────────
            RenderTarget.DrawLine(
                point0:      new SharpDX.Vector2(centerX, panelTop),
                point1:      new SharpDX.Vector2(centerX, panelBottom),
                brush:       centerLineBrushDx,
                strokeWidth: 1f
            );

            // ────────────────────────────────────────────────────────────
            //  STEP 6 — DRAW THE CURRENT PRICE LINE
            //
            //  A horizontal line showing where the last traded price sits
            //  relative to the DOM bars. This is the key visual anchor —
            //  it shows whether bids or asks dominate around current price.
            // ────────────────────────────────────────────────────────────
            if (CurrentBar >= 0)
            {
                // Close[0] = the last traded price on the current bar
                // chartScale converts that price to a Y pixel position
                float priceY = (float)chartScale.GetYByValue(Close[0]);

                // Only draw if the current price is within our panel
                if (priceY >= panelTop && priceY <= panelBottom)
                {
                    RenderTarget.DrawLine(
                        point0:      new SharpDX.Vector2(panelLeft,  priceY),
                        point1:      new SharpDX.Vector2(panelRight, priceY),
                        brush:       priceLineBrushDx,
                        strokeWidth: 1.5f
                    );
                }
            }
        }


        // ═══════════════════════════════════════════════════════════════════
        //  HELPER — ToSolidBrush
        //
        //  Converts a WPF System.Windows.Media.Color (what NT8 property editors
        //  use) into a SharpDX SolidColorBrush (what Direct2D renders with).
        //
        //  The conversion: WPF Color channels are 0–255 bytes,
        //  SharpDX Color4 channels are 0.0–1.0 floats.
        // ═══════════════════════════════════════════════════════════════════
        private SharpDX.Direct2D1.SolidColorBrush ToSolidBrush(System.Windows.Media.Brush wpfBrush)
        {
            // NT8 exposes colors as Brush — cast to SolidColorBrush to read the Color value.
            // Falls back to white if the brush is a gradient or null.
            System.Windows.Media.Color c = wpfBrush is System.Windows.Media.SolidColorBrush scb ? scb.Color : System.Windows.Media.Colors.White;

            return new SharpDX.Direct2D1.SolidColorBrush(
                RenderTarget,
                new SharpDX.Color4(
                    c.R / 255f,  // red   channel (byte 0–255 → float 0.0–1.0)
                    c.G / 255f,  // green channel
                    c.B / 255f,  // blue  channel
                    c.A / 255f   // alpha channel (controls transparency)
                )
            );
        }

        // ═══════════════════════════════════════════════════════════════════
        //  HELPER — DisposeBrushes
        //
        //  SharpDX resources are COM objects backed by GPU memory. The .NET
        //  garbage collector does NOT manage them. You must call Dispose()
        //  explicitly or you will leak GPU memory. The null-check pattern
        //  (brush?.Dispose()) is safe to call multiple times.
        // ═══════════════════════════════════════════════════════════════════
        private void DisposeBrushes()
        {
            bidBrushDx?.Dispose();        bidBrushDx        = null;
            askBrushDx?.Dispose();        askBrushDx        = null;
            priceLineBrushDx?.Dispose();  priceLineBrushDx  = null;
            centerLineBrushDx?.Dispose(); centerLineBrushDx = null;
            labelBrushDx?.Dispose();      labelBrushDx      = null;
        }


        // ═══════════════════════════════════════════════════════════════════
        //  PROPERTIES — exposed in the NinjaTrader indicator dialog
        //
        //  The pattern for each color property:
        //    [XmlIgnore]  →  tell the XML serializer to skip this property
        //    public Color →  the actual property the UI binds to
        //    Serialize    →  a string companion property that NT8 uses to
        //                    save/load the color from workspace XML files
        // ═══════════════════════════════════════════════════════════════════

        [Range(1, 50)]
        [Display(
            Name        = "Bin Size (ticks)",
            Description = "Number of ticks per price bucket. Larger = fewer, wider bars. Try 4 for ES.",
            GroupName   = "Settings",
            Order       = 1
        )]
        public int BinSizeTicks { get; set; }

        // ── Bid color ────────────────────────────────────────────────────
        // Brush (not Color) is the NT8 standard for color properties.
        // The companion *Serialize property is what NT8 uses to save/load
        // the value from workspace XML — it must be a plain string.
        // Gui.Serialize.BrushToString / StringToBrush are NT8's built-in
        // helpers for this, matching the same pattern as the SuperDom columns.
        [XmlIgnore]
        [Display(Name = "Bid Bar Color", GroupName = "Colors", Order = 1)]
        public System.Windows.Media.Brush BidColor { get; set; }

        [Browsable(false)]
        public string BidColorSerialize
        {
            get => Gui.Serialize.BrushToString(BidColor);
            set => BidColor = Gui.Serialize.StringToBrush(value);
        }

        // ── Ask color ────────────────────────────────────────────────────
        [XmlIgnore]
        [Display(Name = "Ask Bar Color", GroupName = "Colors", Order = 2)]
        public System.Windows.Media.Brush AskColor { get; set; }

        [Browsable(false)]
        public string AskColorSerialize
        {
            get => Gui.Serialize.BrushToString(AskColor);
            set => AskColor = Gui.Serialize.StringToBrush(value);
        }

        // ── Price line color ─────────────────────────────────────────────
        [XmlIgnore]
        [Display(Name = "Price Line Color", GroupName = "Colors", Order = 3)]
        public System.Windows.Media.Brush PriceLineColor { get; set; }

        [Browsable(false)]
        public string PriceLineColorSerialize
        {
            get => Gui.Serialize.BrushToString(PriceLineColor);
            set => PriceLineColor = Gui.Serialize.StringToBrush(value);
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  LAYER 2 PREP — What to add next
//
//  When you're ready to add smoothing / spoofing filters (Layer 2), you will:
//
//  1. Add a MinDisplaySize property (e.g. minimum 10 contracts to render a bar)
//     → In OnRender, skip bins where volume < MinDisplaySize
//
//  2. Add an ExponentialSmooth() pass over binnedBid/binnedAsk before drawing
//     → smoothedVol[i] = alpha * rawVol[i] + (1 - alpha) * smoothedVol[i-1]
//     → alpha = 0.1 to 0.3 is a good starting range for DOM smoothing
//
//  3. Track "volume withdrawn" (spoofing signal):
//     → In OnMarketDepth, when Operation == Update, compare new volume to old
//     → If volume dropped >50% in <500ms at a level that never traded, flag it
//
// ═══════════════════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════════════════
//  LAYER 3 PREP — What to add next after Layer 2
//
//  Cumulative delta = sum(ask fills) - sum(bid fills) over time.
//  This turns into a line chart overlaid on the bar chart.
//
//  1. In OnMarketDepth, track trades that hit the ask vs bid
//     → Compare last traded price to current best ask/bid to classify side
//
//  2. Maintain a rolling CumulativeDelta value updated each trade
//
//  3. In OnRender, draw a line connecting CumulativeDelta points per bar,
//     scaled to the right half of the panel (or a third panel layer)
//
// ═══════════════════════════════════════════════════════════════════════════════

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DomFlowVisualizer[] cacheDomFlowVisualizer;
		public DomFlowVisualizer DomFlowVisualizer()
		{
			return DomFlowVisualizer(Input);
		}

		public DomFlowVisualizer DomFlowVisualizer(ISeries<double> input)
		{
			if (cacheDomFlowVisualizer != null)
				for (int idx = 0; idx < cacheDomFlowVisualizer.Length; idx++)
					if (cacheDomFlowVisualizer[idx] != null &&  cacheDomFlowVisualizer[idx].EqualsInput(input))
						return cacheDomFlowVisualizer[idx];
			return CacheIndicator<DomFlowVisualizer>(new DomFlowVisualizer(), input, ref cacheDomFlowVisualizer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DomFlowVisualizer DomFlowVisualizer()
		{
			return indicator.DomFlowVisualizer(Input);
		}

		public Indicators.DomFlowVisualizer DomFlowVisualizer(ISeries<double> input )
		{
			return indicator.DomFlowVisualizer(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DomFlowVisualizer DomFlowVisualizer()
		{
			return indicator.DomFlowVisualizer(Input);
		}

		public Indicators.DomFlowVisualizer DomFlowVisualizer(ISeries<double> input )
		{
			return indicator.DomFlowVisualizer(input);
		}
	}
}

#endregion
