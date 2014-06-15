﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace HazeronAdviser
{
    static public class DrawingTools
    {
        static public Pen Pen(Color color)
        {
            return new Pen(color, 1);
        }
        static public Pen Marker(Color color)
        {
            return new Pen(color, 3);
        }
        static Font _font = new Font("Ariel", 6);
        static public Font Font
        {
            get { return _font; }
        }
        static public SolidBrush Brush(Color color)
        {
            return new SolidBrush(color);
        }

        const int edgeUR = 10;
        const int edgeLD = 20;
        //const float intervalX = 25;
        //const int intervalY = 1;

        static public void DrawGraphAxles(Panel panel, Graphics graphObj, string xAxleName, string yAxleName)
        {
            float intervalX = (float)Math.Max(0.25, (panel.Width - edgeLD - edgeUR) / 18);
            float intervalY = (float)Math.Max(0.25, (panel.Height - edgeLD - edgeUR) / 1250);
            int intervalNumber = 40;
            graphObj.DrawString(xAxleName, DrawingTools.Font, DrawingTools.Brush(Color.Black), panel.Width - edgeUR - 12, panel.Height - edgeLD + 8);
            graphObj.DrawString(yAxleName, DrawingTools.Font, DrawingTools.Brush(Color.Black), edgeLD - 19, edgeUR - 7);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), edgeLD, edgeUR, edgeLD, panel.Height - edgeLD);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), edgeLD, panel.Height - edgeLD, panel.Width - edgeUR, panel.Height - edgeLD);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), edgeLD, 10, edgeLD + 5, edgeLD - 5);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), edgeLD, 10, edgeLD - 5, edgeLD - 5);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), panel.Width - edgeUR, panel.Height - edgeLD, panel.Width - edgeUR - 5, panel.Height - edgeLD - 5);
            graphObj.DrawLine(DrawingTools.Pen(Color.Black), panel.Width - edgeUR, panel.Height - edgeLD, panel.Width - edgeUR - 5, panel.Height - edgeLD + 5);
            int number = 1;
            for (int loop = edgeLD + (int)intervalX; loop < panel.Width - edgeUR * 2; loop += (int)intervalX)
            {
                int numberOffset = 0;
                graphObj.DrawLine(DrawingTools.Pen(Color.Black), loop, panel.Height - edgeLD - 2, loop, panel.Height - edgeLD + 5);
                if (number > 9)
                    numberOffset = -4;
                graphObj.DrawString(number++.ToString(), DrawingTools.Font, DrawingTools.Brush(Color.Black), loop - 3 + numberOffset, panel.Height - edgeLD + 5 + 3);
            }
            number = 0;
            for (int loop = panel.Height - edgeLD; loop > edgeUR * 2; loop -= (int)(intervalY * intervalNumber))
            {
                int numberOffset = 0;
                graphObj.DrawLine(DrawingTools.Pen(Color.Black), edgeLD - 5, loop, edgeLD + 2, loop);
                if (number > 9)
                    numberOffset = -4;
                if (number > 99)
                    numberOffset = -8;
                if (number > 999)
                    numberOffset = -12;
                graphObj.DrawString(number.ToString(), DrawingTools.Font, DrawingTools.Brush(Color.Black), edgeLD - 5 - 7 + numberOffset, loop - 3);
                number += intervalNumber;
            }
        }

        static public void DrawGraph(Panel panel, Graphics graphObj, int[] yAxle, Color color)
        {
            float intervalX = (float)Math.Max(0.25, (panel.Width - edgeLD - edgeUR) / 18);
            float intervalY = (float)Math.Max(0.25, (panel.Height - edgeLD - edgeUR) / 1250);
            graphObj.DrawEllipse(DrawingTools.Marker(color), edgeLD + 0 - 1, panel.Height - edgeLD - yAxle[0] * intervalY - 1, 2, 2);
            for (int loop = 1; loop < yAxle.Length; loop++)
            {
                Point from = new Point(edgeLD + Convert.ToInt32((loop - 1) * intervalX), panel.Height - edgeLD - Convert.ToInt32(yAxle[loop - 1] * intervalY));
                Point to = new Point(edgeLD + Convert.ToInt32(loop * intervalX), panel.Height - edgeLD - Convert.ToInt32(yAxle[loop] * intervalY));
                graphObj.DrawLine(DrawingTools.Pen(color), from, to);
                graphObj.DrawEllipse(DrawingTools.Marker(color), to.X - 1, to.Y - 1, 2, 2);
            }
        }
    }
}