# WPF Bulk Renaming Tool (MVVM Principles)

Utilizes Caliburn.Micro as the MVVM framework.

## Demo 

Standalone .NET Core 3.1 demo available [here](https://github.com/Reittu/wpf-mvvm-rename-tool/releases)

.NET framework 4.7.2 demo can be found under bin/Debug directory 

![fstool_example](https://user-images.githubusercontent.com/54769604/72728168-6c380d00-3b95-11ea-95d7-a993b06ca5b5.PNG)

## Other tools

In case this tool doesn't do what you want, you can most likely solve your problem with CLI commands such as:

`ren *.txt *.js`

Or with Powershell:

`Dir -filter *.jpg | %{Rename-Item $_ -NewName ("example_name_{0}.jpg" -f $nr++)}`
