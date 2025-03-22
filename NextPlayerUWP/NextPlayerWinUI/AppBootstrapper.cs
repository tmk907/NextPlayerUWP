using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace NextPlayerWinUI
{
    public partial class App
    {
        public enum StartKind { Launch, Activate }


        public const string DefaultTileID = "App";

        public enum AdditionalKinds { Primary, Toast, SecondaryTile, Other, JumpListItem }

        /// <summary>
        /// This determines the simplest case for starting. This should handle 80% of common scenarios. 
        /// When Other is returned the developer must determine start manually using IActivatedEventArgs.Kind
        /// </summary>
        public static AdditionalKinds DetermineStartCause(Microsoft.UI.Xaml.LaunchActivatedEventArgs args, AppActivationArguments activatedEventArgs)
        {
            if (activatedEventArgs.Kind == ExtendedActivationKind.ToastNotification)
            {
                return AdditionalKinds.Toast;
            }
            if (activatedEventArgs.Kind == ExtendedActivationKind.Launch && args.Arguments == DefaultTileID)
            {
                return AdditionalKinds.Primary;
            }
            else if (activatedEventArgs.Kind == ExtendedActivationKind.Launch && string.IsNullOrEmpty(args.Arguments))
            {
                return AdditionalKinds.JumpListItem;
            }
            //else if (!string.IsNullOrEmpty(e?.TileId) && e?.TileId != DefaultTileID)
            //{
            //    return AdditionalKinds.SecondaryTile;
            //}
            else
            {
                return AdditionalKinds.Other;
            }
        }

        private object _PageKeys;
        // T must be a custom Enum
        public Dictionary<T, Type> PageKeys<T>()
            where T : struct, IConvertible
        {
            if (!typeof(T).GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            if (_PageKeys != null && _PageKeys is Dictionary<T, Type>)
            {
                return _PageKeys as Dictionary<T, Type>;
            }
            return (_PageKeys = new Dictionary<T, Type>()) as Dictionary<T, Type>;
        }
    }
}
