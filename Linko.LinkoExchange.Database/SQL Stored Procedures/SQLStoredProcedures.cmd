@echo off
REM: Command File Created by Microsoft Visual Database Tools
REM: Date Generated: 5/12/2005
REM: Authentication type: SQL Server
REM: Usage: CommandFilename [Server] [Database] [Login] [Password]

if '%1' == '' goto usage
if '%2' == '' goto usage
if '%3' == '' goto usage

if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage


cscript smartUpdate.js %1 %2 %3 %4
if %ERRORLEVEL% NEQ 0 goto errors
goto finish

REM: How to use screen
:usage
echo.
echo Usage: MyScript Server Database User [Password]
echo Server: the name of the target SQL Server
echo Database: the name of the target database
echo User: the login name on the target server
echo Password: the password for the login on the target server (optional)
echo.
echo Example: MyScript.cmd MainServer MainDatabase MyName MyPassword
echo.
echo.
goto done

REM: error handler
:errors
echo.
echo WARNING! Error(s) were detected!
echo --------------------------------
echo Please evaluate the situation and, if needed,
echo restart this command file. You may need to
echo supply command parameters when executing
echo this command file.
echo.

goto done

REM: finished execution
:finish
echo.
echo Script execution is complete!
:done
@echo on
