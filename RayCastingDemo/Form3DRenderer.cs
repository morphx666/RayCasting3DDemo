using RayCasting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RayCastingDemo {
    public partial class Form3DRenderer : Form {
        public enum RenderModes {
            Shaded,
            Textured
        }

        private readonly Particle camera;
        private readonly List<Vector> walls;
        private readonly List<Particle> lights;
        private readonly Renderer renderer;
        private Size wbSize = new Size(Properties.Resources.WallBmp.Width, Properties.Resources.WallBmp.Height);

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

            this.Paint += (object sender, PaintEventArgs e) => {
                switch(RenderMode) {
                    case RenderModes.Shaded: renderer.Render3DMapShaded(e.Graphics, this.DisplayRectangle); break;
                    case RenderModes.Textured: renderer.Render3DMapTextured(e.Graphics, this.DisplayRectangle, Properties.Resources.WallBmp); break;
                }
            };
        }
    }
}