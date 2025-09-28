using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;

namespace Apk_Decompiler.Services
{
    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private LocalizationService()
        {
            _resourceManager = new ResourceManager("Apk_Decompiler.Resources.Strings", typeof(LocalizationService).Assembly);
            
            // Set default culture to Arabic
            _currentCulture = new CultureInfo("ar");
            SetCulture(_currentCulture);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            private set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    OnPropertyChanged(nameof(CurrentCulture));
                    OnPropertyChanged(nameof(IsRightToLeft));
                    OnPropertyChanged(nameof(FlowDirection));
                    
                    // Notify all string properties changed
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        public bool IsRightToLeft => _currentCulture.TextInfo.IsRightToLeft;
        
        public FlowDirection FlowDirection => IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        public string GetString(string key)
        {
            try
            {
                return _resourceManager.GetString(key, _currentCulture) ?? key;
            }
            catch
            {
                return key;
            }
        }

        public void SetCulture(CultureInfo culture)
        {
            CurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            
            // Update thread culture
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public void ToggleLanguage()
        {
            var newCulture = _currentCulture.Name == "ar" 
                ? new CultureInfo("en") 
                : new CultureInfo("ar");
            
            SetCulture(newCulture);
        }

        // Localized string properties
        public string AppTitle => GetString("AppTitle");
        public string AppSubtitle => GetString("AppSubtitle");
        
        public string JavaCheckTitle => GetString("JavaCheckTitle");
        public string JavaCheckDescription => GetString("JavaCheckDescription");
        public string ToolManagementTitle => GetString("ToolManagementTitle");
        public string ToolManagementDescription => GetString("ToolManagementDescription");
        public string ApkSelectionTitle => GetString("ApkSelectionTitle");
        public string ApkSelectionDescription => GetString("ApkSelectionDescription");
        public string DecompileTitle => GetString("DecompileTitle");
        public string DecompileDescription => GetString("DecompileDescription");
        public string EditPhaseTitle => GetString("EditPhaseTitle");
        public string EditPhaseDescription => GetString("EditPhaseDescription");
        public string RecompileTitle => GetString("RecompileTitle");
        public string RecompileDescription => GetString("RecompileDescription");
        public string SignTitle => GetString("SignTitle");
        public string SignDescription => GetString("SignDescription");
        public string CompleteTitle => GetString("CompleteTitle");
        public string CompleteDescription => GetString("CompleteDescription");
        
        public string CheckJavaButton => GetString("CheckJavaButton");
        public string CheckToolsButton => GetString("CheckToolsButton");
        public string SelectApkButton => GetString("SelectApkButton");
        public string DecompileButton => GetString("DecompileButton");
        public string OpenWorkspaceButton => GetString("OpenWorkspaceButton");
        public string RecompileSignButton => GetString("RecompileSignButton");
        public string OpenOutputButton => GetString("OpenOutputButton");
        public string ViewLogsButton => GetString("ViewLogsButton");
        public string LanguageButton => GetString("LanguageButton");
        
        public string Ready => GetString("Ready");
        public string ProcessSteps => GetString("ProcessSteps");
        public string SelectedApk => GetString("SelectedApk");
        
        // Java Check Messages
        public string CheckingJavaInstallation => GetString("CheckingJavaInstallation");
        public string JavaFound(string version) => string.Format(GetString("JavaFound"), version);
        public string JavaInstalled(string version) => string.Format(GetString("JavaInstalled"), version);
        public string JavaNotFound => GetString("JavaNotFound");
        public string JavaRequired => GetString("JavaRequired");
        public string InstallJavaRestart => GetString("InstallJavaRestart");
        
        // Tool Management Messages
        public string CheckingTools => GetString("CheckingTools");
        public string CheckingApkTool => GetString("CheckingApkTool");
        public string DownloadingApkTool => GetString("DownloadingApkTool");
        public string CheckingUberSigner => GetString("CheckingUberSigner");
        public string DownloadingUberSigner => GetString("DownloadingUberSigner");
        public string AllToolsReady => GetString("AllToolsReady");
        public string ToolsReady => GetString("ToolsReady");
        public string ReadyToSelectApk => GetString("ReadyToSelectApk");
        public string ToolSetupFailed(string error) => string.Format(GetString("ToolSetupFailed"), error);
        public string ToolSetupFailedShort => GetString("ToolSetupFailedShort");
        
        // APK Selection Messages
        public string ApkFilesFilter => GetString("ApkFilesFilter");
        public string SelectApkFileTitle => GetString("SelectApkFileTitle");
        public string SelectedApkFile(string filename) => string.Format(GetString("SelectedApkFile"), filename);
        public string ApkSelected(string filename) => string.Format(GetString("ApkSelected"), filename);
        
        // Decompilation Messages
        public string StartingDecompilation => GetString("StartingDecompilation");
        public string DecompilationCompleted => GetString("DecompilationCompleted");
        public string ApkDecompiledSuccessfully => GetString("ApkDecompiledSuccessfully");
        public string ReadyForEditing => GetString("ReadyForEditing");
        public string DecompilationFailed => GetString("DecompilationFailed");
        public string DecompilationCompletedSuccessfully => GetString("DecompilationCompletedSuccessfully");
        
        // Recompilation Messages
        public string NoDecompiledProjects => GetString("NoDecompiledProjects");
        public string EditingCompleted => GetString("EditingCompleted");
        public string StartingRecompilation => GetString("StartingRecompilation");
        public string RecompilationCompleted => GetString("RecompilationCompleted");
        public string RecompilationFailed => GetString("RecompilationFailed");
        public string RecompilationCompletedSuccessfully => GetString("RecompilationCompletedSuccessfully");
        public string ProjectDirectoryNotFound => GetString("ProjectDirectoryNotFound");
        
        // Signing Messages
        public string StartingApkSigning => GetString("StartingApkSigning");
        public string ApkSignedSuccessfully => GetString("ApkSignedSuccessfully");
        public string ProcessCompletedSuccessfully => GetString("ProcessCompletedSuccessfully");
        public string ApkProcessingCompleted => GetString("ApkProcessingCompleted");
        public string SigningFailed => GetString("SigningFailed");
        public string ApkSigningFailed => GetString("ApkSigningFailed");
        
        // Log Messages
        public string NoLogFileFound => GetString("NoLogFileFound");
        public string FailedToOpenLogs(string error) => string.Format(GetString("FailedToOpenLogs"), error);
        
        // Error Messages
        public string Error(string message) => string.Format(GetString("Error"), message);
        public string Signing(string message) => string.Format(GetString("Signing"), message);
        public string ErrorDuring(string operation, string error) => string.Format(GetString("ErrorDuring"), operation, error);
        public string FailedWithExitCode(int exitCode) => string.Format(GetString("FailedWithExitCode"), exitCode);

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
