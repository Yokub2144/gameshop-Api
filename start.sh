#!/bin/bash

# ออกถ้ามีคำสั่งใดล้มเหลว
set -e

echo "Building the project..."
dotnet build

echo "Running the project..."
dotnet run --urls "http://0.0.0.0:5000"