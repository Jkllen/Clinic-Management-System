@echo off
echo ========================================
echo Cleaning CruzNeryClinic project...
echo ========================================

dotnet clean

echo.
echo Removing bin and obj folders...
echo.

if exist bin (
    rmdir /s /q bin
)

if exist obj (
    rmdir /s /q obj
)

echo.
echo Restoring packages...
echo.

dotnet restore

echo.
echo Building project...
echo.

dotnet build

echo.
echo ========================================
echo Done.
echo ========================================

pause