# AlexaDo ![AlexaDo](https://raw.github.com/ritchiecarroll/AlexaDo/master/images/logo.png)
Amazon Echo Invocation Plug-in Application
C# / .NET 4.6

## Project Status

This project was developed before Amazon released an offical SDK for interacting with the Echo. Since Amazon now has an excellent [Alexa skills development kit](https://developer.amazon.com/alexa-skills-kit), I would certainly recommend using the official SDK for any new Alexa development.

I have, however, recently updated this tool (12/3/2016) to make sure it still works as expected with recent changes to the `alexa.amazon.com` web site. I will continue to leave this project up as an example of how you can do simple Alexa integrations, e.g., getting Alexa to spawn your own application or as a model for playing around with the cards API that is part of the Alexa application home page -- plus it's really still just fun to ask _`Alexa, Simon Says Google How old is Barack Obama?`_ then just have her repeat the question and hear Google respond through her speakers :-)

## About

This is a plug-in based application that will monitor activities spoken to the Amazon Echo, listen for key phrases then dispatch plug-in based actions based on what was heard. New plug-ins can be written in C# and configured through an XML based "commands" definition file. Included with the installation is an "AppLauncher" plug-in with two sample command definitions ([view source](https://github.com/ritchiecarroll/AlexaDo/blob/master/src/Plugins/AppLauncher/AppLauncher.commands)):

1. "OK Google Responder" - allows commands like "__Alexa Google How old is George Washington Stop__"
2. "E-mail Me" - when configured with a mailer like "[mailsend](http://github.com/muquit/mailsend)" allows commands like "__Alexa Simon Says email me feed the dog when I get home__"

Note that the Google Responder is the only plug-in that is automatically enabled. This plug-in will "speak" the OK Google results in the Google voice back through the Amazon Echo if the computer is connected to the Echo via Bluetooth. Note that this uses Chrome and the settings may need to be updated to enable "OK Google" to start a voice search to ensure audio feedback works.  Other plug-ins can use Windows based text-to-speech (TTS) for responses to triggered actions.

## Installation

Download the installers zip file (<a href="#versions">versions</a>):

&nbsp;&nbsp;&nbsp; **_[Releases](https://github.com/ritchiecarroll/AlexaDo/releases)_**

Unzip files and run the proper __Setup.msi__ for your target OS platform, i.e., 32-bit or 64-bit.

Once the application is installed, run the application by finding "AlexaDo Echo Monitor" from the start menu. When the application is run for the first time, a window will pop-up and you will need to authenticate with Amazon Echo:

<img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/images/login.png" >

This only needs to be done once, the application will securely cache login credentials for future runs. As long as the application has authenticated it will run in the background and remain minimized to the task bar. You can access the application at any time from the task-bar via the AlexaDo icon: <img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/images/logo.png" height="16" width="16" >. Clicking on the AlexaDo icon will pull up the monitoring screen:

<img src="https://raw.github.com/ritchiecarroll/AlexaDo/master/images/monitor.png" >

On this screen you can watch as activities are triggered and change the desired TTS voice. If your Windows installation only has a single TTS voice, see [this article](http://superuser.com/questions/590779/how-to-install-more-voices-to-windows-speech) to install more voices. Zira is the most similar to Alexa and is selected by default if it is installed.

Commands will be triggered regardless of whether or not Alexa understands the spoken key phrases. However, for best results, it helps to prevent Alexa from trying to interpret the commands. For example, if you are not expecting immediate feedback from a command, you can always start the command with "Simon Says" and Alexa will repeat the command back to you. Another option is to end your commands with a clearly enunciated "Stop" which causes Alexa to cancel trying to interpret what you said and allowing any matched plug-ins to handle the command.

Having trouble? Check the AlexaDo log file: %APPDATA%\AlexaDo\AlexaDo.log

## Technical

Note that this application attempts to attach to the Amazon WebSocket socket used to monitor cards for quick and dynamic response (see [piettes.com/echo forum](http://www.piettes.com/echo/viewtopic.php?f=3&t=10) for info on how this works). If the application fails to attach to the WebSocket, it will fall back on polling activities on an interval.

In order to process text-to-speech feedback through Bluetooth, this application must currently be run inside of an active Windows session. It may be possible to change this application to run as a Windows service, but this is generally more work than I would like to tackle for a toy-project at the moment. ~~Perhaps Amazon will open the Echo SDK such to allow Alexa to "say" something, if this happens, changing this application to run as service would be easier.~~ 

## Writing Plug-ins

The included plug-in, [AppLauncher](https://github.com/ritchiecarroll/AlexaDo/blob/master/src/Plugins/AppLauncher/Execute.cs), will provide the best example for writing new plug-ins. In general all you will need to do is create a new .NET class library project, reference _AlexaDoPlugin.dll_ and _log4net.dll_, inherit from _AlexaDoPluginBase_ and override the _ProcessActivity_ method. You can optionally override the _Initialize_ method to parse parameters passed to your plug-in. If you compile AlexaDo in Debug mode, it includes a "Test Command" button on the home page that can be used to easily test word combinations and patterns without having to talk to Alexa.

In order to instantiate your plug-in you will need to create a "commands" definition file. In the XML based _.commands_ definition example below, a command will be triggered when either "do my thing" or "work that thing" is heard at the beginning of the detected speech. Note that the "StartsWith" match style means that even if you said "do my thing and dance a jig", this would trigger a match. You could create another command that triggered an "EndsWith" match style for "dance a jig" and it would match the heard key phrase and execute the command also. Multiple plug-ins can match a single command and will be triggered in the order that they are matched. Note that you can define multiple commands in one file.

Example: _MyIftttHandler.commands_
```xml
<?xml version="1.0" encoding="utf-8" ?>
<commands>
  <command description="My IFTTT Handler" usage="alexa do my thing stop" enabled="true">
    <trigger>
      <keyPhrase>do my thing|work that thing</keyPhrase>
      <matchStyle>StartsWith</matchStyle>
    </trigger>
    <action>
      <assemblyName>MyIftttHandler.dll</assemblyName>
      <typeName>MyIftttHandler.DoIt</typeName>
      <parameters encrypted="false">parameter1=&quot;do this&quot; parameter2=&quot;set that&quot;</parameters>
    </action>
    <response>
      <succeeded type="tts">I did your thing</succeeded>
      <failed type="tts">Unable to do your thing, [reason].</failed>
    </response>
    <notes>
      <![CDATA[
        Examples:
          alexa do my thing stop
          alexa simon says work that thing
          
        Note: Define help for "How do I help other configure My IFTTT Handler to do your own thing?"
      ]]>
    </notes>
  </command>
</commands>
```
<a name="versions" id="versions"></a>
## Versions
1.0.0.3 - 5/20/2015 - Initial release with installation package.<br>
1.0.0.4 - 5/26/2015 - Posted a fix for writing processed activity cache with non-admin rights.<br>
1.0.0.7 - 12/3/2016 - Updated to work with more recent versions of alexa.amazon.com.

## Developer Notes

Because of recent changes in the Amazon authentication layer, the WebBrowser control must now be used to get JSON responses from activities API\*. Since this control is based on Internet Explorer and the default IE action for receiving a JSON response is to download it, this doesn't work well for automation activities -- it's not very useful to engage the user to download a file at the UI layer during automated voice responses :-p. In order to get IE, and hence the WebBrowser control, to simply display received JSON responses instead of downloading them (like it does with a text file), the following registry settings need to be applied. Note that if you install the application, the setup package will take care of this change for you - otherwise, you will need to apply these registry changes manually in order to run this project in debug mode:
```
Windows Registry Editor Version 5.00
;
; Tell IE to open JSON documents in the browser.  
; 25336920-03F9-11cf-8FD0-00AA00686F13 is the CLSID for the "Browse in place" .
;  

[HKEY_CLASSES_ROOT\MIME\Database\Content Type\application/json]
"CLSID"="{25336920-03F9-11cf-8FD0-00AA00686F13}"
"Encoding"=hex:08,00,00,00

[HKEY_CLASSES_ROOT\MIME\Database\Content Type\application/x-json]
"CLSID"="{25336920-03F9-11cf-8FD0-00AA00686F13}"
"Encoding"=hex:08,00,00,00

[HKEY_CLASSES_ROOT\MIME\Database\Content Type\text/json]
"CLSID"="{25336920-03F9-11cf-8FD0-00AA00686F13}"
"Encoding"=hex:08,00,00,00
```
To apply manually, save the above registry settings to a file with a `.reg` suffix and import using the `RegEdit` tool - you can usually just double click on the `.reg.` file to import the keys.

\* FYI, you used to be able just pass along cookies acquired from the WebBrowser control after authentication to a WebClient instance and everything would still work fine, but Amazon security is obviously getting better ;-)
