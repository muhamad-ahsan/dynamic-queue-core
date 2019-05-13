![logo](https://github.com/muhamad-ahsan/dynamic-queue-core/blob/master/logo_banner.jpg)
# About
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

The [dynamic-queue](https://github.com/muhamad-ahsan/dynamic-queue) library is very handy to manage the Message Queues. This project is the exact port of `dynamic-queue` for `.Net Core`. For the full details about the `dynamic-queue`, please visit the [dynamic-queue](https://github.com/muhamad-ahsan/dynamic-queue).

### Technology
The framework has built using `C# 6.0` and `.Net Core 2.1`.

### Configuration
As in `.Net Core` we do not have the classic `***.config` file for the configuration, now it is based on the `***.json` file. The default implementation of the interface `IQueueConfigurationProvider` has been added to read the configuration from the `***.json` file.

# Setup
If you want to run the code in `Visual Studio` or any other `.Net IDE`, just download the source code, restore the nuget packages, update the configuration and you are good to go.

# Nuget

##### ServiceBus
[![NuGet](https://img.shields.io/badge/nuget--ServiceBus-v1.0-blue.svg)](https://www.nuget.org/packages/DynamicQueue.ServiceBus.Core/)

##### Logging-NLog 
[![NuGet](https://img.shields.io/badge/nuget--Logger--NLog-v1.0-blue.svg)](https://www.nuget.org/packages/DynamicQueue.Log.NLog.Core/)
