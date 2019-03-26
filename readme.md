**Downloads to a fixed dir _dlDirectory as set in Program.cs**

Checks the feed as set in /settings/setting.txt every 5 minutes and downloads all <item><link> XML elements.

## Publish program 
dotnet publish -c Release -r linux-x64

## Run 
dotnet RssHitter.dll

ctrl + c to exit.