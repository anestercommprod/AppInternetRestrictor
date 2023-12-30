# App Internet Restrictor
Restrict Windows Applications from being able to access internet, both in and outbound connections with a simple command line interface.

![image](https://github.com/anestercommprod/AppInternetRestrictor/assets/108794312/a96ead12-202c-4268-94c6-bc28a033d143)

# Steps to use:
1. Download compiled application (file `AppRestricter.exe`) or compile the code by yourself
2. Launch the program with Admin privileges.
3. Type "help" in the command line interface

# What does the program do?

This program is creating a new rule for each selected file for both in and outbound connections, so that the program won't be able to connect to/with any service/ip.
It also features remove and displaying added programs methods, so you won't miss with any other rules that wasn't created by the program, as well as you won't be able to create two identical rules. 

# Program compiling requirements:
1. References
   
Set up the references for `System.Windows.Forms` and  `NetFwTypeLib` in the Visual Studio .NET 4.8 Console Project Solution.

2. Code
   
Create a new item file `program.cs` inside your project if it doesn't exist or replace all the code within existing file with mine.

Press F6 to build solution.

# Support

Program was tested on x64 bit Windows 10 & Windows 11 systems and no unexpected behaviours were observed, yet feel free to open an issue if something works not as intended.
