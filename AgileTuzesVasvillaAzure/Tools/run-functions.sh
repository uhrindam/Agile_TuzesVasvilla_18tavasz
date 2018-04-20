#!/bin/sh

cd ..
dotnet publish
cd bin/Debug/netcoreapp2.0/publish
func host start
