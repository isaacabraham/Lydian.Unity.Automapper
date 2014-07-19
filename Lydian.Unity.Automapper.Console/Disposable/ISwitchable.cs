using Lydian.Unity.Automapper;

namespace Lydian.Disposable
{
    [DoNotMap]
    public interface ISwitchable
    {
        void On();
        void Off();
    }
}