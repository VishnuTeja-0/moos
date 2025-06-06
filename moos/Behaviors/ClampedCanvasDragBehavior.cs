using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace moos.Behaviors;

public class ClampedCanvasDragBehavior : StyledElementBehavior<Control>
{
    private Avalonia.Point _start;
    private Avalonia.Point _startOffset;
    private bool _dragging;
    private Control? _parent;
    private Control? _draggedContainer;
    private bool _captured;
    
    protected override void OnAttachedToVisualTree()
    {
        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
    }
    
    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Avalonia.Controls.Shapes.Rectangle) return;
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed && AssociatedObject is Control { Parent: Canvas canvas } control)
        {
            _dragging = true;
            _start = e.GetPosition(canvas);
            _parent = canvas;
            _draggedContainer = AssociatedObject;
            _startOffset = new Avalonia.Point(Canvas.GetLeft(control), Canvas.GetTop(control));
            SetDraggingPseudoClasses(_draggedContainer, true);
            _captured = true;
        }
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (e.Source is Avalonia.Controls.Shapes.Rectangle) return;
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (_captured && properties.IsLeftButtonPressed && AssociatedObject is Control { Parent: Canvas canvas } control)
        {
            if(_parent is null || _draggedContainer is null || !_dragging)
            {
                return;
            }

            var pos = e.GetPosition(canvas);
            double dx = pos.X - _start.X;
            double dy = pos.Y - _start.Y;

            double newX = Math.Clamp(_startOffset.X + dx, 0, canvas.Width - control.Bounds.Width);
            double newY = Math.Clamp(_startOffset.Y + dy, 0, canvas.Height - control.Bounds.Height);

            Canvas.SetLeft(control, newX);
            Canvas.SetTop(control, newY);
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is Avalonia.Controls.Shapes.Rectangle) return;
        if (_captured)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Released();
            }
        }
        _captured = false;
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
        _captured = false;
    }

    private void Released()
    {
        if (_dragging)
        {
            if(_draggedContainer is not null)
            {
                SetDraggingPseudoClasses(_draggedContainer, false);
            }
            _dragging = false;
            _parent = null;
            _draggedContainer = null;
        }
    }
    
    private void SetDraggingPseudoClasses(Control control, bool isDragging)
    {
        if (isDragging)
        {
            ((IPseudoClasses)control.Classes).Add(":dragging");
        }
        else
        {
            ((IPseudoClasses)control.Classes).Remove(":dragging");
        }
    }
}