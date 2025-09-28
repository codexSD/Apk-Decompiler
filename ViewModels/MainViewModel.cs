using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Apk_Decompiler.Models;
using Apk_Decompiler.Services;

namespace Apk_Decompiler.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ToolManagerService toolManager;
        private readonly ApkProcessorService apkProcessor;
        private readonly LocalizationService localization;
        
        private string _selectedApkPath = string.Empty;
        private string _statusMessage = "Ready";
        private bool _isProcessing = false;
        private ProcessStep _currentStep = ProcessStep.JavaCheck;

        public ObservableCollection<ApkProcessStep> ProcessSteps { get; }

        public string SelectedApkPath
        {
            get => _selectedApkPath;
            set => SetProperty(ref _selectedApkPath, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ProcessStep CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public LocalizationService Localization => localization;

        public ICommand CheckJavaCommand { get; }
        public ICommand CheckToolsCommand { get; }
        public ICommand SelectApkCommand { get; }
        public ICommand DecompileCommand { get; }
        public ICommand OpenWorkspaceCommand { get; }
        public ICommand RecompileCommand { get; }
        public ICommand OpenOutputCommand { get; }
        public ICommand OpenLogsCommand { get; }
        public ICommand ToggleLanguageCommand { get; }

        public MainViewModel()
        {
            toolManager = new ToolManagerService();
            apkProcessor = new ApkProcessorService(toolManager);
            localization = LocalizationService.Instance;

            ProcessSteps = new ObservableCollection<ApkProcessStep>();
            
            InitializeProcessSteps();
            
            // Subscribe to localization changes to update step titles
            localization.PropertyChanged += OnLocalizationChanged;

            CheckJavaCommand = new AsyncRelayCommand(CheckJavaAsync);
            CheckToolsCommand = new AsyncRelayCommand(CheckToolsAsync);
            SelectApkCommand = new RelayCommand(SelectApk);
            DecompileCommand = new AsyncRelayCommand(DecompileAsync, () => !string.IsNullOrEmpty(SelectedApkPath) && !IsProcessing);
            OpenWorkspaceCommand = new RelayCommand(() => apkProcessor.OpenWorkspaceFolder());
            RecompileCommand = new AsyncRelayCommand(RecompileAsync, () => !IsProcessing);
            OpenOutputCommand = new RelayCommand(() => apkProcessor.OpenOutputFolder());
            OpenLogsCommand = new RelayCommand(OpenLogs);
            ToggleLanguageCommand = new RelayCommand(() => localization.ToggleLanguage());

            // Initialize status message
            StatusMessage = localization.Ready;

            // Start with Java check
            _ = Task.Run(CheckJavaAsync);
        }

        private void InitializeProcessSteps()
        {
            ProcessSteps.Clear();
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.JavaCheck,
                Title = localization.JavaCheckTitle,
                Description = localization.JavaCheckDescription,
                Icon = "☕"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.ToolDownload,
                Title = localization.ToolManagementTitle,
                Description = localization.ToolManagementDescription,
                Icon = "🔧"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.ApkSelection,
                Title = localization.ApkSelectionTitle,
                Description = localization.ApkSelectionDescription,
                Icon = "📱"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.Decompile,
                Title = localization.DecompileTitle,
                Description = localization.DecompileDescription,
                Icon = "📦"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.EditPhase,
                Title = localization.EditPhaseTitle,
                Description = localization.EditPhaseDescription,
                Icon = "✏️"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.Recompile,
                Title = localization.RecompileTitle,
                Description = localization.RecompileDescription,
                Icon = "🔨"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.Sign,
                Title = localization.SignTitle,
                Description = localization.SignDescription,
                Icon = "🔐"
            });
            ProcessSteps.Add(new ApkProcessStep
            {
                Step = ProcessStep.Complete,
                Title = localization.CompleteTitle,
                Description = localization.CompleteDescription,
                Icon = "✅"
            });
        }

        private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                // All properties changed, update everything
                InitializeProcessSteps();
                StatusMessage = localization.Ready;
                OnPropertyChanged(nameof(Localization));
            }
        }

        private async Task CheckJavaAsync()
        {
            var javaStep = ProcessSteps.First(s => s.Step == ProcessStep.JavaCheck);
            javaStep.Status = StepStatus.InProgress;
            javaStep.Message = localization.CheckingJavaInstallation;
            CurrentStep = ProcessStep.JavaCheck;

            await Task.Delay(500); // Small delay for UI

            if (JavaService.IsJavaInstalled())
            {
                var version = JavaService.GetJavaVersion();
                javaStep.Status = StepStatus.Completed;
                javaStep.Message = localization.JavaFound(version);
                javaStep.Progress = 100;
                StatusMessage = localization.JavaInstalled(version);
                
                // Automatically proceed to tool check
                await CheckToolsAsync();
            }
            else
            {
                javaStep.Status = StepStatus.Failed;
                javaStep.Message = localization.JavaNotFound;
                StatusMessage = localization.JavaRequired;
                
                // Open Java download page and exit
                JavaService.OpenJavaDownloadPage();
                StatusMessage = localization.InstallJavaRestart;
            }
        }

        private async Task CheckToolsAsync()
        {
            var toolStep = ProcessSteps.First(s => s.Step == ProcessStep.ToolDownload);
            toolStep.Status = StepStatus.InProgress;
            toolStep.Message = localization.CheckingTools;
            CurrentStep = ProcessStep.ToolDownload;

            try
            {
                // Check APKTool
                toolStep.Message = localization.CheckingApkTool;
                var apkToolInfo = await toolManager.CheckApkToolAsync();
                
                if (!apkToolInfo.IsInstalled)
                {
                    toolStep.Message = localization.DownloadingApkTool;
                    var progress = new Progress<int>(p => toolStep.Progress = p / 2); // First half of progress
                    await toolManager.DownloadToolAsync(apkToolInfo, progress);
                }

                // Check UberSigner
                toolStep.Message = localization.CheckingUberSigner;
                var uberSignerInfo = await toolManager.CheckUberSignerAsync();
                
                if (!uberSignerInfo.IsInstalled)
                {
                    toolStep.Message = localization.DownloadingUberSigner;
                    var progress = new Progress<int>(p => toolStep.Progress = 50 + (p / 2)); // Second half of progress
                    await toolManager.DownloadToolAsync(uberSignerInfo, progress);
                }

                toolStep.Status = StepStatus.Completed;
                toolStep.Message = localization.AllToolsReady;
                toolStep.Progress = 100;
                StatusMessage = localization.ToolsReady;

                // Enable APK selection
                var apkStep = ProcessSteps.First(s => s.Step == ProcessStep.ApkSelection);
                CurrentStep = ProcessStep.ApkSelection;
                apkStep.Message = localization.ReadyToSelectApk;
            }
            catch (Exception ex)
            {
                toolStep.Status = StepStatus.Failed;
                toolStep.Message = localization.ToolSetupFailed(ex.Message);
                StatusMessage = localization.ToolSetupFailedShort;
                LoggingService.LogError($"Tool setup error: {ex.Message}");
            }
        }

        private void SelectApk()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = localization.ApkFilesFilter,
                Title = localization.SelectApkFileTitle
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedApkPath = dialog.FileName;
                var apkStep = ProcessSteps.First(s => s.Step == ProcessStep.ApkSelection);
                apkStep.Status = StepStatus.Completed;
                apkStep.Message = localization.SelectedApkFile(Path.GetFileName(SelectedApkPath));
                apkStep.Progress = 100;
                StatusMessage = localization.ApkSelected(Path.GetFileName(SelectedApkPath));
                
                ((AsyncRelayCommand)DecompileCommand).NotifyCanExecuteChanged();
            }
        }

        private async Task DecompileAsync()
        {
            if (string.IsNullOrEmpty(SelectedApkPath) || !File.Exists(SelectedApkPath))
                return;

            IsProcessing = true;
            var decompileStep = ProcessSteps.First(s => s.Step == ProcessStep.Decompile);
            decompileStep.Status = StepStatus.InProgress;
            decompileStep.Progress = 0;
            CurrentStep = ProcessStep.Decompile;

            var progress = new Progress<string>(message =>
            {
                decompileStep.Message = message;
                StatusMessage = message;
                decompileStep.Progress = Math.Min(decompileStep.Progress + 5, 95);
            });

            var success = await apkProcessor.DecompileApkAsync(SelectedApkPath, progress);

            if (success)
            {
                decompileStep.Status = StepStatus.Completed;
                decompileStep.Message = localization.DecompilationCompleted;
                decompileStep.Progress = 100;
                StatusMessage = localization.ApkDecompiledSuccessfully;

                // Enable edit phase
                var editStep = ProcessSteps.First(s => s.Step == ProcessStep.EditPhase);
                editStep.Status = StepStatus.InProgress;
                editStep.Message = localization.ReadyForEditing;
                CurrentStep = ProcessStep.EditPhase;
            }
            else
            {
                decompileStep.Status = StepStatus.Failed;
                decompileStep.Message = localization.DecompilationFailed;
                StatusMessage = localization.DecompilationFailed;
            }

            IsProcessing = false;
        }

        private async Task RecompileAsync()
        {
            var projects = apkProcessor.GetDecompiledProjects();
            if (projects.Length == 0)
            {
                StatusMessage = localization.NoDecompiledProjects;
                return;
            }

            IsProcessing = true;
            
            // Mark edit phase as completed
            var editStep = ProcessSteps.First(s => s.Step == ProcessStep.EditPhase);
            editStep.Status = StepStatus.Completed;
            editStep.Message = localization.EditingCompleted;
            editStep.Progress = 100;

            // Start recompile
            var recompileStep = ProcessSteps.First(s => s.Step == ProcessStep.Recompile);
            recompileStep.Status = StepStatus.InProgress;
            recompileStep.Progress = 0;
            CurrentStep = ProcessStep.Recompile;

            var progress = new Progress<string>(message =>
            {
                recompileStep.Message = message;
                StatusMessage = message;
                recompileStep.Progress = Math.Min(recompileStep.Progress + 5, 95);
            });

            var projectName = projects[0]; // Use first project
            var success = await apkProcessor.RecompileApkAsync(projectName, progress);

            if (success)
            {
                recompileStep.Status = StepStatus.Completed;
                recompileStep.Message = localization.RecompilationCompleted;
                recompileStep.Progress = 100;

                // Start signing automatically
                await SignApkAsync(projectName);
            }
            else
            {
                recompileStep.Status = StepStatus.Failed;
                recompileStep.Message = localization.RecompilationFailed;
                StatusMessage = localization.RecompilationFailed;
            }

            IsProcessing = false;
        }

        private async Task SignApkAsync(string projectName)
        {
            var signStep = ProcessSteps.First(s => s.Step == ProcessStep.Sign);
            signStep.Status = StepStatus.InProgress;
            signStep.Progress = 0;
            CurrentStep = ProcessStep.Sign;

            var outputDir = toolManager.GetOutputDirectory();
            var apkPath = Path.Combine(outputDir, $"{projectName}_recompiled.apk");

            var progress = new Progress<string>(message =>
            {
                signStep.Message = message;
                StatusMessage = message;
                signStep.Progress = Math.Min(signStep.Progress + 10, 95);
            });

            var success = await apkProcessor.SignApkAsync(apkPath, progress);

            if (success)
            {
                signStep.Status = StepStatus.Completed;
                signStep.Message = localization.ApkSignedSuccessfully;
                signStep.Progress = 100;

                // Complete the process
                var completeStep = ProcessSteps.First(s => s.Step == ProcessStep.Complete);
                completeStep.Status = StepStatus.Completed;
                completeStep.Message = localization.ProcessCompletedSuccessfully;
                completeStep.Progress = 100;
                CurrentStep = ProcessStep.Complete;

                StatusMessage = localization.ApkProcessingCompleted;
            }
            else
            {
                signStep.Status = StepStatus.Failed;
                signStep.Message = localization.SigningFailed;
                StatusMessage = localization.ApkSigningFailed;
            }
        }

        private void OpenLogs()
        {
            try
            {
                var logPath = LoggingService.GetLogFilePath();
                if (File.Exists(logPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = logPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    StatusMessage = localization.NoLogFileFound;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = localization.FailedToOpenLogs(ex.Message);
            }
        }
    }
}
