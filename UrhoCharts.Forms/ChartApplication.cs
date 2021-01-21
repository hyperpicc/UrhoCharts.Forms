using System;
using Urho;
using Urho.Actions;


namespace UrhoCharts.Forms
{
    public class ChartApplication : Application
    {
        internal SurfaceChart Chart { get; set; } = new SurfaceChart();

        private SurfaceChart _lastChart = new SurfaceChart();
        private Scene        _scene;

        [Preserve]
        public ChartApplication(ApplicationOptions options = null)
            : base(options)
        {
            UnhandledException += (s, e) =>
            {
                e.Handled = true;
                throw new Exception(e.Exception.Message);
            };
        }

        protected override void Start()
        {
            base.Start();
            CreateScene();
        }

        private void CreateScene()
        {
            _scene = new Scene();
            var octree = _scene.CreateComponent<Octree>();

            // Camera
            var cameraNode = new Node
            {
                Name     = "CameraNode",
                Position = new Vector3(120, 120, 120),
                Rotation = new Quaternion(-0.121f, 0.878f, -0.305f, -0.35f),
            };

            var camera = new Camera();

            cameraNode.AddComponent(camera);
            _scene.AddChild(cameraNode);

            // Light
            var lightNode = new Node
            {
                Name = "LightNode"
            };

            var light = new Light
            {
                LightType  = LightType.Point,
                Range      = 600,
                Brightness = 1.3f
            };

            lightNode.AddComponent(light);
            _scene.AddChild(lightNode);

            // Chart
            var backgroundColor = Xamarin.Forms.Color.Transparent;
            if (Chart.IsValid == true)
            {
                CreateChartAsync();
                backgroundColor = Chart.BackgroundColor;
            }

            // Viewport and background color
            var viewport = new Viewport(Context, _scene, camera);
            Renderer.SetViewport(0, viewport);
            viewport.SetClearColor(ConvertToUrhoColor(backgroundColor));

            // Input events
            Input.TouchEnd += OnTouched;
        }

        private async void CreateChartAsync()
        {
            _scene.RemoveChild(_scene.GetChild("ChartNode"));

            var chartNode = new Node
            {
                Name = "ChartNode"
            };

            var planeNode = new Node
            {
                Name  = "PlaneNode",
                Scale = new Vector3(Chart.XSize * 2f, 1, Chart.YSize * 2f),
            };

            chartNode.AddChild(planeNode);

            var chart    = new CustomGeometry();
            var material = new Material();
            material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 3);
            chart.SetMaterial(material);

            chart.BeginGeometry(0, PrimitiveType.TriangleList);
            for (var r = 1; r < Chart.YSize; r++)
            {
                for (var c = 1; c < Chart.XSize; c++)
                {
                    var localData = new byte[]
                    {
                        Chart.ZData[(r - 1) * Chart.XSize + (c - 1)],
                        Chart.ZData[(r - 0) * Chart.XSize + (c - 1)],
                        Chart.ZData[(r - 0) * Chart.XSize + (c - 0)],
                        Chart.ZData[(r - 1) * Chart.XSize + (c - 0)],
                    };

                    Vector3 p0 = new Vector3(Chart.XSize / 2f - (c - 1), localData[0] / 10f, Chart.YSize / 2f - (r - 1));
                    Vector3 p1 = new Vector3(Chart.XSize / 2f - (c - 1), localData[1] / 10f, Chart.YSize / 2f - (r - 0));
                    Vector3 p2 = new Vector3(Chart.XSize / 2f - (c - 0), localData[2] / 10f, Chart.YSize / 2f - (r - 0));
                    Vector3 p3 = new Vector3(Chart.XSize / 2f - (c - 0), localData[3] / 10f, Chart.YSize / 2f - (r - 1));

                    chart.DefineVertex(p0);
                    chart.DefineColor(MapRainBowColor(localData[0] / 256f));
                    chart.DefineVertex(p1);
                    chart.DefineColor(MapRainBowColor(localData[1] / 256f));
                    chart.DefineVertex(p2);
                    chart.DefineColor(MapRainBowColor(localData[2] / 256f));
                    chart.DefineVertex(p3);
                    chart.DefineColor(MapRainBowColor(localData[3] / 256f));
                    chart.DefineVertex(p0);
                    chart.DefineColor(MapRainBowColor(localData[0] / 256f));
                    chart.DefineVertex(p2);
                    chart.DefineColor(MapRainBowColor(localData[2] / 256f));
                }
            }
            chart.Commit();

            chartNode.AddComponent(chart);
            _scene.AddChild(chartNode);

            await chartNode.RunActionsAsync(new EaseBackOut(new RotateBy(2.5f, 0, 360f, 0)));
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if ((_lastChart != Chart) && (Chart.IsValid == true))
            {
                CreateChartAsync();
                _lastChart = Chart;
            }

            if (Input.NumTouches >= 1)
            {
                var touch = Input.GetTouch(0);
                var chartNode = _scene.GetChild("ChartNode");
                chartNode.Rotate(new Quaternion(-touch.Delta.Y / 1.5f, -touch.Delta.X / 1.5f, 0));
            }
        }

        private void OnTouched(TouchEndEventArgs e)
        {
            var camera = _scene.GetChild("CameraNode").GetComponent<Camera>();
            var cameraRay = camera.GetScreenRay((float)e.X / Graphics.Width, (float)e.Y / Graphics.Height);

            var octree = _scene.GetComponent<Octree>();
            octree.RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);
        }

        private Color MapRainBowColor(float ratio)
        {
            var adjustedRatio = 1f - ratio;
            return new Color(GetColorFromHue(adjustedRatio) / 256f,
                             GetColorFromHue(adjustedRatio - 1f / 3) / 256f,
                             GetColorFromHue(adjustedRatio + 1f / 3) / 256f);
        }

        private byte GetColorFromHue(float hue)
        {
            var clampedHue = hue - (float) Math.Floor(hue);
            if (clampedHue < 0f)
            {
                clampedHue += 1f;
            }

            if (clampedHue < 1f/6)
            {
                return (byte) (0.5f + 255f * clampedHue * 6f);
            }
            else if (clampedHue < 1f/2)
            {
                return 0xFF;
            }
            else if (clampedHue < 2f/3)
            {
                return (byte) (0.5f + 255f * (2/3 - clampedHue) * 6f);
            }

            return 0;
        }

        private Color ConvertToUrhoColor(Xamarin.Forms.Color color)
        {
            return Color.FromByteFormat(
                (byte) Math.Round(color.R, MidpointRounding.AwayFromZero),
                (byte) Math.Round(color.G, MidpointRounding.AwayFromZero),
                (byte) Math.Round(color.B, MidpointRounding.AwayFromZero),
                (byte) Math.Round(color.A, MidpointRounding.AwayFromZero));
        }
    }
}
