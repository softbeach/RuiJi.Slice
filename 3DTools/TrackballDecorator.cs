//---------------------------------------------------------------------------
//
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Limited Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedpermissivelicense.mspx
// All other rights reserved.
//
// This file is part of the 3D Tools for Windows Presentation Foundation
// project.  For more information, see:
// 
// http://CodePlex.com/Wiki/View.aspx?ProjectName=3DTools
//
// The following article discusses the mechanics behind this
// trackball implementation: http://viewport3d.com/trackball.htm
//
// Reading the article is not required to use this sample code,
// but skimming it might be useful.
//
//---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup; // IAddChild, ContentPropertyAttribute

namespace _3DTools
{
    public class TrackballDecorator : Viewport3DDecorator
    {
        private Point _previousPosition2D;
        private Vector3D _previousPosition3D = new Vector3D(0, 0, 1);

        private Point _worldpreviousPosition2D;
        private Vector3D _worldpreviousPosition3D = new Vector3D(0, 0, 1);

        private Transform3DGroup _worldtransform;
        private ScaleTransform3D _worldscale = new ScaleTransform3D();
        private AxisAngleRotation3D _worldrotation = new AxisAngleRotation3D();

        private Border _eventSource;

        public TrackballDecorator()
        {
            _worldtransform = new Transform3DGroup();
            _worldtransform.Children.Add(_worldscale);
            _worldtransform.Children.Add(new RotateTransform3D(_worldrotation));

            // used so that we always get events while activity occurs within
            // the viewport3D
            _eventSource = new Border();
            _eventSource.Background = Brushes.Transparent;

            PreViewportChildren.Add(_eventSource);
        }

        #region Event Handling
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _previousPosition2D = e.GetPosition(this);
                _previousPosition3D = ProjectToTrackball(ActualWidth,
                                                         ActualHeight,
                                                         _previousPosition2D);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _worldpreviousPosition2D = e.GetPosition(this);
                _worldpreviousPosition3D = ProjectToTrackball(ActualWidth,
                                                         ActualHeight,
                                                         _worldpreviousPosition2D);
            }
            if (Mouse.Captured == null)
            {
                Mouse.Capture(this, CaptureMode.Element);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (IsMouseCaptured)
            {
                Mouse.Capture(this, CaptureMode.None);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsMouseCaptured)
            {
                Point currentPosition = e.GetPosition(this);

                // avoid any zero axis conditions
                if (currentPosition == _previousPosition2D) return;

                // Prefer tracking to zooming if both buttons are pressed.
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var _rotation = Track(currentPosition);

                    _previousPosition2D = currentPosition;

                    Viewport3D viewport3D = this.Viewport3D;
                    if (viewport3D != null)
                    {
                        if (viewport3D.Children.Count > 2)
                        {
                            var m = viewport3D.Children[viewport3D.Children.Count - 1].Transform as Transform3DGroup;
                            m.Children[2] = new RotateTransform3D(_rotation);
                        }
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    TrackWorld(currentPosition);

                    _worldpreviousPosition2D = currentPosition;
                    Viewport3D viewport3D = this.Viewport3D;
                    if (viewport3D != null)
                    {
                        if (viewport3D.Camera != null)
                        {
                            if (viewport3D.Camera.IsFrozen)
                            {
                                viewport3D.Camera = viewport3D.Camera.Clone();
                            }

                            if (viewport3D.Camera.Transform != _worldtransform)
                            {
                                viewport3D.Camera.Transform = _worldtransform;
                            }
                        }
                    }
                }
            }
        }
        #endregion Event Handling

        private AxisAngleRotation3D Track(Point currentPosition)
        {
            Vector3D currentPosition3D = ProjectToTrackball(
                ActualWidth, ActualHeight, currentPosition);

            Vector3D axis = Vector3D.CrossProduct(_previousPosition3D, currentPosition3D);
            double angle = Vector3D.AngleBetween(_previousPosition3D, currentPosition3D);

            // quaterion will throw if this happens - sometimes we can get 3D positions that
            // are very similar, so we avoid the throw by doing this check and just ignoring
            // the event 
            if (axis.Length == 0) return new AxisAngleRotation3D();

            Quaternion delta = new Quaternion(axis, -angle);

            // Get the current orientantion from the RotateTransform3D
            var _rotation = ((Viewport3D.Children[Viewport3D.Children.Count - 1].Transform as Transform3DGroup).Children[2] as RotateTransform3D).Rotation as AxisAngleRotation3D;
            AxisAngleRotation3D r = _rotation;
            Quaternion q = new Quaternion(_rotation.Axis, _rotation.Angle);

            // Compose the delta with the previous orientation
            q *= delta;

            // Write the new orientation back to the Rotation3D
            _rotation.Axis = q.Axis;
            _rotation.Angle = q.Angle;

            _previousPosition3D = currentPosition3D;

            return _rotation;
        }

        private void TrackWorld(Point currentPosition)
        {
            Vector3D currentPosition3D = ProjectToTrackball(
                ActualWidth, ActualHeight, currentPosition);

            Vector3D axis = Vector3D.CrossProduct(_worldpreviousPosition3D, currentPosition3D);
            double angle = Vector3D.AngleBetween(_worldpreviousPosition3D, currentPosition3D);

            // quaterion will throw if this happens - sometimes we can get 3D positions that
            // are very similar, so we avoid the throw by doing this check and just ignoring
            // the event 
            if (axis.Length == 0) return;

            Quaternion delta = new Quaternion(axis, -angle);

            // Get the current orientantion from the RotateTransform3D
            AxisAngleRotation3D r = _worldrotation;
            Quaternion q = new Quaternion(_worldrotation.Axis, _worldrotation.Angle);

            // Compose the delta with the previous orientation
            q *= delta;

            // Write the new orientation back to the Rotation3D
            _worldrotation.Axis = q.Axis;
            _worldrotation.Angle = q.Angle;

            _worldpreviousPosition3D = currentPosition3D;
        }

        private Vector3D ProjectToTrackball(double width, double height, Point point)
        {
            double x = point.X / (width / 2);    // Scale so bounds map to [0,0] - [2,2]
            double y = point.Y / (height / 2);

            x = x - 1;                           // Translate 0,0 to the center
            y = 1 - y;                           // Flip so +Y is up instead of down

            double z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;

            return new Vector3D(x, y, z);
        }
    }
}