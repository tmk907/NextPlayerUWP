// Copyright (c) Morten Nielsen. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;
using NextPlayerWinUI;
using System;

namespace WindowsStateTriggers
{
    /// <summary>
    /// Trigger for switching on orientation. A desktop app has no per-view DisplayInformation,
    /// so orientation follows the shape of the app window.
    /// </summary>
    public class OrientationStateTrigger : StateTriggerBase, ITriggerValue
    {
        public OrientationStateTrigger()
        {
            var window = App.Window;
            if (window != null)
            {
                var weakEvent = new WeakEventListener<OrientationStateTrigger, object, WindowSizeChangedEventArgs>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.UpdateTrigger(),
                    OnDetachAction = (instance, listener) => window.SizeChanged -= listener.OnEvent,
                };
                window.SizeChanged += weakEvent.OnEvent;
            }
        }

        private void UpdateTrigger()
        {
            var window = App.Window;
            if (window == null || Orientation == Orientations.None)
            {
                IsActive = false;
                return;
            }

            bool isPortrait = window.Bounds.Height > window.Bounds.Width;
            IsActive = Orientation == (isPortrait ? Orientations.Portrait : Orientations.Landscape);
        }

        /// <summary>
        /// Gets or sets the orientation to trigger on.
        /// </summary>
        public Orientations Orientation
        {
            get { return (Orientations)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> parameter.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientations), typeof(OrientationStateTrigger),
            new PropertyMetadata(Orientations.None, (d, e) => ((OrientationStateTrigger)d).UpdateTrigger()));

        #region ITriggerValue

        private bool m_IsActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return m_IsActive; }
            private set
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    SetActive(value);
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        #endregion ITriggerValue

        /// <summary>
        /// Orientations
        /// </summary>
        public enum Orientations
        {
            /// <summary>
            /// none
            /// </summary>
            None,
            /// <summary>
            /// landscape
            /// </summary>
            Landscape,
            /// <summary>
            /// portrait
            /// </summary>
            Portrait
        }
    }
}
