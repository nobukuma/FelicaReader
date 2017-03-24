using Plugin.FelicaReader.Abstractions;
using System;
#if UWP
using Windows.Devices.SmartCards;
#elif __ANDROID__
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
#endif

namespace Plugin.FelicaReader
{
    /// <summary>
    /// Cross platform FelicaReader implemenations
    /// </summary>
    public class CrossFelicaReader
    {
#if __ANDROID__
        public static void Init(Activity activity, System.Type type)
        {
            Current = new FelicaReaderImplementation(activity, type);
        }

        private static IFelicaReader current;
        public static IFelicaReader Current
        {
            get
            {
                if (current == null)
                {
                    throw new ArgumentException("Call Init first");
                }
                return current;
            }
            set
            {
                current = value;
            }
        }
#else
        static Lazy<IFelicaReader> IFelicaReaderImplementation = new Lazy<IFelicaReader>(() => CreateFelicaReader(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IFelicaReader Current
        {
            get
            {
                var ret = IFelicaReaderImplementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }
#endif

        static IFelicaReader CreateFelicaReader()
        {
#if PORTABLE
            return null;
#elif __ANDROID__
            throw new ArgumentException("In android, you must call CrossFelicaReader.Init(Activity,System.Type,Android.Nfc.Tag) from MainActivity");
#else
            return new FelicaReaderImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }

}
