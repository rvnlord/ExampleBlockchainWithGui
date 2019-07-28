﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlockchainApp.Source.Common.Utils.UtilClasses
{
    public class Arc : Shape
    {
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(0.0, UpdateArc));

        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(90.0, UpdateArc));

        public SweepDirection Direction
        {
            get => (SweepDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(SweepDirection), typeof(Arc),
                new UIPropertyMetadata(SweepDirection.Clockwise));

        public double OriginRotationDegrees
        {
            get => (double)GetValue(OriginRotationDegreesProperty);
            set => SetValue(OriginRotationDegreesProperty, value);
        }

        public static readonly DependencyProperty OriginRotationDegreesProperty =
            DependencyProperty.Register("OriginRotationDegrees", typeof(double), typeof(Arc),
                new UIPropertyMetadata(270.0, UpdateArc));

        protected static void UpdateArc(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var arc = d as Arc;
            arc?.InvalidateVisual();
        }

        protected override Geometry DefiningGeometry => GetArcGeometry();

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            drawingContext.DrawGeometry(null, new Pen(Stroke, StrokeThickness), GetArcGeometry());
        }

        private Geometry GetArcGeometry()
        {
            var startPoint = PointAtAngle(Math.Min(StartAngle, EndAngle), Direction);
            var endPoint = PointAtAngle(Math.Max(StartAngle, EndAngle), Direction);

            var arcSize = new Size(Math.Max(0, (RenderSize.Width - StrokeThickness) / 2),
                Math.Max(0, (RenderSize.Height - StrokeThickness) / 2));
            var isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

            var geom = new StreamGeometry();
            using (StreamGeometryContext context = geom.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.ArcTo(endPoint, arcSize, 0, isLargeArc, Direction, true, false);
            }
            geom.Transform = new TranslateTransform(StrokeThickness / 2, StrokeThickness / 2);
            return geom;
        }

        private Point PointAtAngle(double angle, SweepDirection sweep)
        {
            var translatedAngle = angle + OriginRotationDegrees;
            var radAngle = translatedAngle * (Math.PI / 180);
            var xr = (RenderSize.Width - StrokeThickness) / 2;
            var yr = (RenderSize.Height - StrokeThickness) / 2;

            var x = xr + xr * Math.Cos(radAngle);
            var y = yr * Math.Sin(radAngle);

            if (sweep == SweepDirection.Counterclockwise)
                y = yr - y;
            else
                y = yr + y;

            return new Point(x, y);
        }
    }
}