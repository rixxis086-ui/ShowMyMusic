using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ShowMyMusic.Models;

namespace ShowMyMusic.Helpers
{
    public static class AnimationHelper
    {
        public static void AnimateIn(
            FrameworkElement targetElement,
            AnimationType animationType,
            SimpleZone zone,
            int durationMs,
            Action? onCompleted = null)
        {
            var duration = TimeSpan.FromMilliseconds(Math.Max(50, durationMs));
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var storyboard = new Storyboard();

            if (animationType == AnimationType.Fade || animationType == AnimationType.SlideAndFade)
            {
                var opacityAnim = new DoubleAnimation { From = 0.0, To = 1.0, Duration = duration, EasingFunction = ease };
                Storyboard.SetTarget(opacityAnim, targetElement);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnim);
            }
            else
            {
                targetElement.Opacity = 1.0;
            }

            if (animationType == AnimationType.Slide || animationType == AnimationType.SlideAndFade)
            {
                // Always set TranslateTransform explicitly before animating
                var tt = new TranslateTransform(0, 0);
                targetElement.RenderTransform = tt;

                double startOffset = 28.0;

                if (zone == SimpleZone.Top || zone == SimpleZone.Bottom)
                {
                    double fromY = zone == SimpleZone.Top ? -startOffset : startOffset;
                    var animY = new DoubleAnimation { From = fromY, To = 0.0, Duration = duration, EasingFunction = ease };
                    Storyboard.SetTarget(animY, targetElement);
                    Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                    storyboard.Children.Add(animY);
                }
                else
                {
                    double fromX = zone == SimpleZone.Left ? -startOffset : startOffset;
                    var animX = new DoubleAnimation { From = fromX, To = 0.0, Duration = duration, EasingFunction = ease };
                    Storyboard.SetTarget(animX, targetElement);
                    Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                    storyboard.Children.Add(animX);
                }
            }
            else
            {
                targetElement.RenderTransform = new TranslateTransform(0, 0);
            }

            storyboard.Completed += (s, e) =>
            {
                targetElement.Opacity = 1.0;
                targetElement.RenderTransform = new TranslateTransform(0, 0);
                onCompleted?.Invoke();
            };

            storyboard.Begin();
        }

        public static void AnimateOut(
            FrameworkElement targetElement,
            AnimationType animationType,
            SimpleZone zone,
            int durationMs,
            Action? onCompleted = null)
        {
            var duration = TimeSpan.FromMilliseconds(Math.Max(50, durationMs));
            var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
            var storyboard = new Storyboard();

            if (animationType == AnimationType.Fade || animationType == AnimationType.SlideAndFade)
            {
                var opacityAnim = new DoubleAnimation { From = 1.0, To = 0.0, Duration = duration, EasingFunction = ease };
                Storyboard.SetTarget(opacityAnim, targetElement);
                Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnim);
            }

            if (animationType == AnimationType.Slide || animationType == AnimationType.SlideAndFade)
            {
                // BUG FIX #13: Always ensure TranslateTransform is set before trying to animate it
                if (targetElement.RenderTransform is not TranslateTransform)
                    targetElement.RenderTransform = new TranslateTransform(0, 0);

                double endOffset = 28.0;

                if (zone == SimpleZone.Top || zone == SimpleZone.Bottom)
                {
                    double toY = zone == SimpleZone.Top ? -endOffset : endOffset;
                    var animY = new DoubleAnimation { From = 0.0, To = toY, Duration = duration, EasingFunction = ease };
                    Storyboard.SetTarget(animY, targetElement);
                    Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                    storyboard.Children.Add(animY);
                }
                else
                {
                    double toX = zone == SimpleZone.Left ? -endOffset : endOffset;
                    var animX = new DoubleAnimation { From = 0.0, To = toX, Duration = duration, EasingFunction = ease };
                    Storyboard.SetTarget(animX, targetElement);
                    Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                    storyboard.Children.Add(animX);
                }
            }

            storyboard.Completed += (s, e) => onCompleted?.Invoke();
            storyboard.Begin();
        }
    }
}