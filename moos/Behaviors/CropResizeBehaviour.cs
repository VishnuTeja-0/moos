using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using System;
using System.Linq;

namespace moos.Behaviors
{
    public class CropResizeBehaviour : Behavior<Control>
    {
        private Point _startPoint;
        private double _initialSize;
        private Border? _cropBox;
        private Canvas? _cropBoxCanvas;
        private bool _captured;
        private int minSize = 50;
        public int? XLimit { get; set; }
        public int? YLimit { get; set; }

        protected override void OnAttachedToVisualTree()
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        }

        protected override void OnDetachedFromVisualTree()
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        }

        private void Pressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed && AssociatedObject is Control control)
            {
                _cropBox = AssociatedObject.GetVisualAncestors().OfType<Border>().FirstOrDefault();
                if (_cropBox is not null)
                {
                    _cropBoxCanvas = _cropBox.GetVisualAncestors().OfType<Canvas>().FirstOrDefault();
                    _initialSize = _cropBox?.Width ?? 0;
                    _startPoint = e.GetPosition(control);
                    _captured = true;
                }
            }
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (_captured && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed && AssociatedObject is Control control)
            {

                var current = e.GetPosition(control);
                var delta = current - _startPoint;
                double deltaSize = _initialSize + Math.Max(delta.X, delta.Y);
                double maxSize = _cropBoxCanvas!.Width - Math.Max(_cropBox!.Bounds.X, _cropBox!.Bounds.Y);
                double newSize = Math.Clamp(Math.Round(deltaSize), minSize, maxSize);

                if (_cropBox is not null)
                {
                    _cropBox.Width = newSize;
                    _cropBox.Height = newSize;
                }
            }
        }

        private void Released(object? sender, PointerReleasedEventArgs e)
        {
            if (_captured)
            {
                if (e.InitialPressMouseButton == MouseButton.Left) 
                { 
                    _cropBox = null;
                    _cropBoxCanvas = null;
                }
                _captured = false;
            }
        }
    }
}
