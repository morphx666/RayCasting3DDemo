using OpenSimplexNoiseSample;
using RayCasting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RayCastingDemo {
    public partial class Form3DRenderer : Form {
        public enum RenderModes {
            Shaded = 0,
            Textured = 1,
            TexturedShaded = 2
        }

        private readonly Particle camera;
        private readonly List<Vector> walls;
        private readonly List<Particle> lights;
        private readonly Renderer renderer;
        private Bitmap texture;

        public RenderModes RenderMode { get; set; } = RenderModes.Shaded;

        public Form3DRenderer(Particle viewer, List<Vector> walls, List<Particle> lights, Renderer renderer) {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.camera = viewer;
            this.walls = walls;
            this.lights = lights;
            this.renderer = renderer;

            texture = (Bitmap)Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Textures\WallBmp.png"));

            this.Paint += (object sender, PaintEventArgs e) => {
                switch(RenderMode) {
                    case RenderModes.Shaded: renderer.Render3DMapShaded(e.Graphics, this.DisplayRectangle); break;
                    case RenderModes.Textured: renderer.Render3DMapTextured(e.Graphics, this.DisplayRectangle, texture); break;
                    case RenderModes.TexturedShaded: renderer.Render3DMapTextured(e.Graphics, this.DisplayRectangle, texture, 0.93); break;
                }
            };

            UpdateTitleBarText();
        }

        public void UpdateTitleBarText() {
            this.Text = $"RayCasting Demo (3D Scene): {RenderMode}";
        }
    }
}