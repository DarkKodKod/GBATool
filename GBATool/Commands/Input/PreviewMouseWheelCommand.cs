using ArchitectureLibrary.Commands;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.VOs;
using System.Windows.Input;

namespace GBATool.Commands
{
    public class PreviewMouseWheelCommand : Command
    {
        public override void Execute(object parameter)
        {
            if (parameter is MouseWheelEventArgs wheelEvent)
            {
                MouseWheelVO vo = new()
                {
                    Delta = wheelEvent.Delta,
                    OriginalSource = wheelEvent.OriginalSource,
                    Sender = wheelEvent.Source
                };

                SignalManager.Get<MouseWheelSignal>().Dispatch(vo);
            }
        }
    }
}
