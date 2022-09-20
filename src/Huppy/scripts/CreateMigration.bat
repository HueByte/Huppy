@echo off
set name=%1 
set app="..\Huppy.App"
set infrastructure="..\Huppy.Infrastructure"
cd %infrastructure%
dotnet ef migrations add "%1" --startup-project %app%