@echo off
REM PROTOTYPE - NOT FOR PRODUCTION
REM Question: Is one long, punishing climb fun to play and to watch for an hour?
REM Date: 2026-11-17
REM Double-click this to stream Long Climb with your TikTok LIVE: your viewers' gifts change your climb.
REM Go LIVE on TikTok first, then run this and type your TikTok name. Keep this window open while you play.
REM The first time, it installs the one package it needs (tiktok-live-connector); that needs the internet.
cd /d "%~dp0"
where node >nul 2>nul || (echo Node.js is not installed. Get it from nodejs.org, then run this again. & pause & exit /b 1)
if not exist node_modules\tiktok-live-connector (
  echo Installing the TikTok LIVE connector, this happens once...
  call npm install --no-audit --no-fund
  if errorlevel 1 (echo The install failed. Check the internet connection and run this again. & pause & exit /b 1)
)
echo.
set "TT="
set /p "TT=Your TikTok name, without the @: "
if "%TT%"=="" (echo No name given. & pause & exit /b 1)
set "TT=%TT:@=%"
start "" http://localhost:8080
node server.js 8080 @%TT%
echo.
echo The server stopped. If it says the address is in use, it is already running: close the other window first.
pause
