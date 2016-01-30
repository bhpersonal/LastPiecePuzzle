using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LastPiecePuzzle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int i = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            var piece = Real.Pieces[(i / 4)];

            var orientation = piece.Orientations[i%4];

            var sb = new StringBuilder();
            for (var y = 0; y < orientation.Height; y++)
            {
                for (var x = 0; x < orientation.Width; x++)
                {
                    sb.Append(orientation.IsFilledIn[y, x] ? (orientation.IsOuty[y, x] ? " O " : " I ") : "   ");
                }
                sb.AppendLine();
            }

            textBox1.Text = sb.ToString();

            i++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var board = Real.Board;

            board.PlacePiece(Real.Pieces[13], 6, 6, 0);

            textBox1.Text = board.ToString();
        }

        private int iiiii;

        private void button3_Click(object sender, EventArgs e)
        {
            var s = new Solver();



            s.OnAnalysing = board =>
            {
                
                iiiii++;
                if (iiiii%100 > 0) return;

                textBox1.Text = board.ToString();
                Application.DoEvents();
            };

            s.OnDeadEnd = board =>
            {
                
                iiiii++;
                if (iiiii % 100 > 0) return;

                textBox1.Text = board.ToString() + "\nDEAD END!";
                Application.DoEvents();
            };

            string solutions = "";
            string lastSolution = "";
            s.OnSolved = board =>
            {
                textBox1.Text = board.ToString();

                if (board.ToString() == lastSolution) return;

                lastSolution = board.ToString();
                File.AppendAllText("C:\\temp\\all solutions.txt", board.ToString() + "\n===\n");
               
            };

            s.OnFailed = () =>
            {
                textBox1.Text =
                    "UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE UNSOLVABLEUNSOLVABLEUNSOLVABLE ";
                MessageBox.Show("THE UNTHINKABLE HAS HAPPENED!!! It's unsolvable.");
            };

            s.Solve();

            File.WriteAllText("C:\\temp\\all solutions.txt", solutions);
        }
    }
}
