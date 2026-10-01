using GreenKeeper.Scheduling;
using System.ComponentModel;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// An input step that produces a care schedule or a sunlight requirement:
    /// the detail steps of the Add Schedule wizard and the steps of the edit
    /// dialog. The dialog logic only needs these two members, whichever kind
    /// of step is currently shown.
    /// </summary>
    public interface IScheduleInputStep : INotifyPropertyChanged
    {
        bool HasValidAmount { get; }

        // Only meaningful while HasValidAmount is true.
        ScheduleInput CreateInput();
    }
}
