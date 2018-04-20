#!/bin/sh

cd ..
dotnet publish
cd bin/Debug/netcoreapp2.0/publish
func azure functionapp publish [https://agiletuzesvasvilla18tavasz.azurewebsites.net/]
