# Madden Web Api

This is a minimal web api that will download franchise data from the Madden Companion mobile app.

It can be run from the command line with `dotnet run`. By default it will download the data in folder named `Data`, (download location can be changed by modifing the `DataPath` variable)

The data will be saved in json files.

This api will only download the raw files from your league.

## Configuration

1. Change the local address and/or port number in `Madden.Api/Properties/launchSettings.json` to the ip address of the computer you are running this from and any open port.

   ```json
   {
     "profiles": {
        "MaddenApi": {
           "commandName": "Project",
           "dotnetRunMessages": true,
           "launchBrowser": true,
           "launchUrl": "swagger",
           "applicationUrl": "https://localhost:7083;http://{local-ip-address}:{local-port}",
           "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
           }
        }
     }
   }
   ```

2. Run `dotnet run` from the root folder

3. The first time it's run, you should be prompted to allow access for the web api; allow access.

4. Unless your computer is in the DMZ, you'll probably have to configure a port forward in your router to forward port 3000 to your local ip address.

## Testing

You can test the api but opening the [swagger](https://localhost:7083/swagger/index.html) test page _(if you changed the address in the launchSettings.json file update this url to match)_.

## Exporting

When running the Madden Companion App, you will need to know your external ip address. One way to find this out is to browse to [WhatIsMyIPAddress.com](https://whatismyipaddress.com). Whatever your external ip address is, is what you will point the Maddan Companion App to.

> For example: if your external ip address is `95.44.142.106` and the port you configured the app to run on is `5268`, then you would use `http://95.44.142.106:5268`.