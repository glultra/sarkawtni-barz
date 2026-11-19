@echo off
REM PROTOTYPE - NOT FOR PRODUCTION
REM Question: Is one long, punishing climb fun to play and to watch for an hour?
REM Date: 2026-11-16
REM Double-click this to host Long Climb rooms: it starts the room server and opens the game.
REM Keep this window open while people play. Close it to stop.
cd /d "%~dp0"
where node >nul 2>nul || (echo Node.js is not installed. Get it from nodejs.org, then run this again. & pause & exit /b 1)
start "" http://localhost:8080
node server.js 8080
echo.
echo The server stopped. If it says the address is in use, it is already running: just open http://localhost:8080
pause
