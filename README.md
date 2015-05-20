# AlexaDo
Amazon Echo Invocation Plug-in Application
C# / .NET 4.5

This is a plug-in based application that will monitor activities spoken to the Amazon Echo, listen for key phrases then dispatch plug-in based actions based on what was heard. New plug-ins can be written in C# and configured through an XML based "commands" definition file. Inlcuded with the installation is an "AppLauncher" plug-in with two sample command definitions:

1. "OK Google Responder" - allows commands like "__Alexa Google How old is George Washington Stop__"
2. "E-mail Me" - when configured with a mailer like "[mailsend](http://github.com/muquit/mailsend)" allows commands like "__Alexa Simon Says email me feed the dog when I get home__" (_work on this adapter is still in progess_)

Note that the Google Reponder is the only plug-in that automatically enabled. This plug-in will "speak" the OK Google results in the Google voice back through the Amazon Echo if the computer is connected to the Echo via Bluetooth. Other plug-ins can use Windows based test-to-speech for responses to triggered actions.

When installed, the application currently runs on Windows in the background and is accesible from the task-bar via the AlexaDo icon: <img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/src/AlexaDo/AlexaDo.ico" height="16" width="16" >.

[Download Installer: Setup.zip](https://raw.github.com/ritchiecarroll/AlexaDo/master/Setup.zip)

When the application is first run, you will need to authenticate with Amazon Echo:

<img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/images/login.png" >

This only needs to be done once, the application will securely cache login credentials for future runs. As long as the application has authenticated it will remain minimized to the task bar. You can pull up the monitoring screen at any time by clicking on the AlexaDo icon:

<img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/images/monitor.png" >

On this screen you can watch as activities are triggered and change the desired text-to-speech voice. If your Windows installation only has a single voice, see [this article](https://forums.robertsspaceindustries.com/discussion/147385/voice-attack-getting-free-alternate-tts-voices-working-with-win7-8-64bit) to install more voices. Zira is the most similar to Alexa and is selected by default if it is installed.

