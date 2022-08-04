@echo off
set name=%1 
set app="%~dp0..\Huppy.App"
dotnet ef migrations add "%1" --startup-project "%app%"