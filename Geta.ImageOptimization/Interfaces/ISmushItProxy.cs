using Geta.ImageOptimization.Messaging;

namespace Geta.ImageOptimization.Interfaces
{
    public interface ISmushItProxy
    {
        SmushItResponse ProcessImage(SmushItRequest smushItRequest);
    }
}