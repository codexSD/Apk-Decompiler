using System.ComponentModel;

namespace Apk_Decompiler.Models
{
    public enum StepStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Skipped
    }

    public enum ProcessStep
    {
        JavaCheck,
        ToolDownload,
        ApkSelection,
        Decompile,
        EditPhase,
        Recompile,
        Sign,
        Complete
    }

    public class ApkProcessStep : INotifyPropertyChanged
    {
        private StepStatus _status = StepStatus.Pending;
        private string _message = string.Empty;
        private int _progress = 0;

        public ProcessStep Step { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        public StepStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = Math.Max(0, Math.Min(100, value));
                OnPropertyChanged(nameof(Progress));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
