@echo off
echo Refreshing Windows icon cache...

taskkill /f /im explorer.exe

del /a /q "%localappdata%\IconCache.db" >nul 2>&1
del /a /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache*" >nul 2>&1

start explorer.exe

echo.
echo Icon cache refreshed.
echo If the icon still does not update, restart Windows.
pause