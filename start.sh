#!/bin/bash
set -e  # ออกทันทีถ้ามีคำสั่งใดล้มเหลว

echo "Restoring dependencies..."
dotnet restore

echo "Building and publishing the project..."
dotnet publish -c Release -o out

echo "Starting the application..."
dotnet out/GameshopApi.dll --urls "http://0.0.0.0:5000"