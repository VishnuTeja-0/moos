using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.Draggable;
using Avalonia.Xaml.Interactivity;

namespace moos.Behaviors;

public class ClampedCanvasDragBehavior : StyledElementBehavior<Control>
{
    private Point _start;
    private Point _startOffset;
    private bool _dragging;
    private Control? _parent;
    private Control? _draggedContainer;
    private bool _captured;
    public double XLimit {get; set;}
    public double YLimit {get; set;}
    
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
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed && AssociatedObject is Control { Parent: Canvas canvas } control)
        {
            _dragging = true;
            _start = e.GetPosition(canvas);
            _parent = _parent;
            _draggedContainer = AssociatedObject;
            _startOffset = new Point(Canvas.GetLeft(control), Canvas.GetTop(control));
            SetDraggingPseudoClasses(_draggedContainer, true);
        }
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_dragging && AssociatedObject is Control control && control.Parent is Canvas canvas)
        {
            var pos = e.GetPosition(canvas);
            double dx = pos.X - _start.X;
            double dy = pos.Y - _start.Y;

            double newX = Math.Clamp(_startOffset.X + dx, 0, XLimit - control.Bounds.Width);
            double newY = Math.Clamp(_startOffset.Y + dy, 0, YLimit - control.Bounds.Height);

            Canvas.SetLeft(control, newX);
            Canvas.SetTop(control, newY);
        }
    }

    private void Released()
    {
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