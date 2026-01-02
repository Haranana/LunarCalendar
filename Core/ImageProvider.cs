using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Core
{
    public interface IImageProvider
    {
        BitmapImage getMoonImage(MoonPhases phase);
    }
    public class ImageProvider : IImageProvider
    {
        public BitmapImage getMoonImage(MoonPhases phase)
        {
           
            switch (phase)
            {
                case MoonPhases.NewMoon:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/NewMoon.png"));
                case MoonPhases.WaxingCrescent:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/WaxingCrescent.png"));
                case MoonPhases.WaningCrescent:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/WaningCrescent.png"));
                case MoonPhases.FullMoon:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/FullMoon.png"));
                case MoonPhases.WaxingGibbous:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/WaxingGibbous.png"));
                case MoonPhases.WaningGibbous:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/WaningGibbous.png"));
                case MoonPhases.FirstQuarter:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/FirstQuarter.png"));
                case MoonPhases.LastQuarter:
                    return new BitmapImage(new Uri("pack://application:,,,/Assets/LastQuarter.png"));
            }
            return new BitmapImage(new Uri("pack://application:,,,/Assets/NewMoon.png"));
        }
    }
}
