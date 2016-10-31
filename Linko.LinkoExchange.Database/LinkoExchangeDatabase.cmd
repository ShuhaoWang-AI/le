@echo off
REM: Command File Created by Microsoft Visual Database Tools
REM: Date Generated: 27/03/2005, Updated: 12/12/2014
REM: Authentication type: SQL Server
REM: Usage: CommandFilename [Server] [Database] [Login] [Password]

if '%1' == '' goto usage
if '%2' == '' goto usage
if '%3' == '' goto usage

if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

REM: comment out the execution of this folder after the initial release since the DB has been created
cd ".\Schema Create Scripts"
 sqlcmd -S %1 -d %2 -U %3 -P %4 -b -i "LinkoExchangeCreateScript.sql"
if %ERRORLEVEL% NEQ 0 goto errors

cd "..\Schema Change Scripts"
 sqlcmd -S %1 -d %2 -U %3 -P %4 -b -i "AlterTableScript.sql"
if %ERRORLEVEL% NEQ 0 goto errors
 sqlcmd -S %1 -d %2 -U %3 -P %4 -b -i "AlterTrigger.sql"
if %ERRORLEVEL% NEQ 0 goto errors

cd "..\SQL Views"
 REM: call "SQLViews.cmd" %1 %2 %3 %4

cd "..\SQL Functions"
 REM: call "SQLFunctions.cmd" %1 %2 %3 %4

cd "..\SQL Stored Procedures"
 call "SQLStoredProcedures.cmd" %1 %2 %3 %4

cd..

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
if '%5' == '' goto pauseMe
goto done

:pauseMe
pause

REM: finished execution
:finish
echo.
echo Script execution is complete!
:done
exit %ERRORLEVEL%
@echo on
 