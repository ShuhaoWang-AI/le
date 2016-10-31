var args = WScript.arguments;
var wsshell = new ActiveXObject("WScript.Shell");
var fso = new ActiveXObject("Scripting.FileSystemObject");
var dir = fso.GetFolder(".");
var fc = new Enumerator(dir.files);
var numProc = 0;
var numErrs = 0;
for (; !fc.atEnd(); fc.moveNext())
{
    var file = fc.item();
    if (file.name.search(/\.sql$/i) >= 0)
    {
        var cmdline = "sqlcmd -S " + args(0) + " -d " + args(1) + " -U " + args(2) + " -P " + args(3) + " -b -i " + file.name;        
    	WScript.echo(cmdline);
    	var retCode = wsshell.run(cmdline, 0, true);
    	numProc += 1;
    	if (retCode != 0)
	{
	    WScript.echo("error: " + retCode);
	    numErrs += 1;
	}	    
    }
}
WScript.echo(numProc + " stored procedures processed.");
if (numErrs > 0)
{
    WScript.echo("WARNING! Error(s) were detected!");
    WScript.echo("--------------------------------");
    WScript.echo("Please evaluate the situation and, if needed,");
    WScript.echo("restart this command file. You may need to");
    WScript.echo("supply command parameters when executing");
    WScript.echo("this command file.");
}
