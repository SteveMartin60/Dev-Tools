@echo off
echo Creating folder structure...
mkdir Core
mkdir Views
mkdir Services
mkdir Handlers
mkdir Input
mkdir Storage
mkdir Resources
mkdir backup

echo Moving stable files...
move AdBlocker.cs Services\
move BlockList.cs Services\
move HtmlCaptureManager.cs Services\
move ImageToggleHandler.cs Handlers\
move UrlHistoryProvider.cs Services\
move history-store.cs Storage\
move TabViewModel.cs Core\
move WebViewEnvironment.cs Core\
move WebViewSecuritySettings.cs Core\
move DevToolsWindow.xaml Views\
move DevToolsWindow.xaml.cs Views\
move RelayCommand.cs Input\
move KeyboardHelper.cs Input\
move findHelper.js Resources\

echo Archiving large files for splitting...
move WebViewNavigationHandler.cs backup\
move WebView2.xaml.cs backup\
move HtmlCaptureService.cs backup\
move EventHandlers.cs backup\
move hotkeys.cs backup\
move html-capture.cs backup\

echo Updating project references...
echo Please update App.xaml StartupUri to Views/WebView2.xaml
echo Please replace archived files with the split versions provided next.
pause