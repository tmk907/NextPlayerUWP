using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace NextPlayerWinUI.Controls
{
    /// <summary>Small replacements for the animation helpers of the UWP Community Toolkit.</summary>
    public static class ElementAnimations
    {
        public static void FadeIn(this UIElement element, int milliseconds)
        {
            element.Opacity = 0;
            _ = element.AnimateAsync(TimeSpan.FromMilliseconds(milliseconds), opacity: 1, offsetX: null);
        }

        /// <summary>Animates opacity and, when given, the horizontal offset of the element.</summary>
        public static Task AnimateAsync(this UIElement element, TimeSpan duration, double opacity, double? offsetX)
        {
            var storyboard = new Storyboard();
            Add(storyboard, element, "Opacity", opacity, duration);

            if (offsetX.HasValue)
            {
                if (element.RenderTransform is not TranslateTransform transform)
                {
                    transform = new TranslateTransform();
                    element.RenderTransform = transform;
                }
                Add(storyboard, transform, "X", offsetX.Value, duration);
            }

            var completion = new TaskCompletionSource();
            storyboard.Completed += (_, _) => completion.TrySetResult();
            storyboard.Begin();
            return completion.Task;
        }

        private static void Add(Storyboard storyboard, DependencyObject target, string property, double to, TimeSpan duration)
        {
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = duration,
                EnableDependentAnimation = false,
            };
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);
            storyboard.Children.Add(animation);
        }
    }
}
