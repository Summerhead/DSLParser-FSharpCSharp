using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static ParserFSharp.Parser;

namespace DrawingParser {
    public partial class PicturesForm : Form {

        public readonly List<string> tasks = new List<string>() {
            @"set 80 80; right 60, down 10, right 50, down 80, 
            left 30, up 30, left 10, down 30, left 30, up 40, left 10, 
            down 20, left 10, down 10, left 10, up 20, left 10, up 60",

            @"set 80 80; right 10, down 10, right 20, up 10, 
            right 10, down 20, right 50, up 20, left 10, up 10, right 20, 
            down 90, left 10, up 20, left 10, down 20, left 10, up 30, 
            left 30, down 30, left 10, up 20, left 10, down 20, left 10, 
            up 40, left 10, up 40",

            @"set 50 50; up 30, right 10, up 10, right 20, 
            up 10, right 40, down 10, right 10, down 10, 
            repeat 2 times: (right 10, down 10, right 20, down 10), 
            right 10, down 10, right 10, down 20, right 10, 
            down 20, right 10, down 30, right 20, down 20, left 30, down 100, 
            left 20, up 50, left 10, up 40, left 10, up 30, left 10, down 20, 
            left 30, up 10, right 20, up 20, left 20, up 10, left 20, up 10, 
            left 10, up 10, left 10, up 20, left 10, up 20, left 10, up 20, 
            left 20, down 10, left 10"
        };

        public List<PictureBox> pictureBoxes = new List<PictureBox>();

        public PicturesForm() {
            InitializeComponent();
            configureForm();
            configureMainPanel();
            setPanels();
            configureButtons();
        }

        private void configureMainPanel() {
            mainPanel.Size = new Size(ClientSize.Width, ClientSize.Height);
        }

        private void configureButtons() {
            buttonPrev.Visible = false;
            if (pictureBoxes.Count == 1 || pictureBoxes.Count == 0) {
                buttonNext.Visible = false;
            }
        }

        private void configureForm() {
            Size = new Size(400, 400);
            Text = "Paint";
        }

        private void setPanels() {
            foreach (var task in tasks.Select((Value, Index) => new { Value, Index })) {
                PictureBox pictureBox = new PictureBox();

                pictureBox.Paint += new PaintEventHandler((sender, e) => createPicture(sender, e, task.Value));
                pictureBox.Size = new Size(Width, Height - 120);
                pictureBox.Location = new Point(50, 10);
                pictureBox.Anchor = AnchorStyles.None;

                if (task.Index != 0) {
                    pictureBox.Visible = false;
                }

                pictureBoxes.Add(pictureBox);
                mainPanel.Controls.Add(pictureBox);
            }
        }

        private void createPicture(object sender, PaintEventArgs e, string task) {
            Console.WriteLine("Task: {0}", task);
            var ans = result(task).Value.ToArray();
            var pairwise = ans.Zip(ans.Skip(1), (a, b) => Tuple.Create(a, b)).ToArray();

            Pen penCurrent = new Pen(Color.Red);
            foreach (var pair in pairwise) {
                Console.WriteLine("Coords: {0}", pair);
                var (firstPoint, secondPoint) = pair;
                var ((x1, y1), (x2, y2)) = (firstPoint, secondPoint);
                e.Graphics.DrawLine(penCurrent, x1, y1, x2, y2);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            foreach (var pictureBox in pictureBoxes.Select((Value, Index) => new { Value, Index })) {
                if (pictureBox.Value.Visible) {
                    pictureBox.Value.Visible = false;
                    int prevIndex = pictureBox.Index - 1;
                    if (prevIndex == 0) {
                        buttonPrev.Visible = false;
                    }
                    buttonNext.Visible = true;
                    pictureBoxes[prevIndex].Visible = true;
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            foreach (var pictureBox in pictureBoxes.Select((Value, Index) => new { Value, Index })) {
                if (pictureBox.Value.Visible) {
                    pictureBox.Value.Visible = false;
                    int nextIndex = pictureBox.Index + 1;
                    if (nextIndex == pictureBoxes.Count - 1) {
                        buttonNext.Visible = false;
                    }
                    buttonPrev.Visible = true;
                    pictureBoxes[nextIndex].Visible = true;
                    break;
                }
            }
        }
    }
}
