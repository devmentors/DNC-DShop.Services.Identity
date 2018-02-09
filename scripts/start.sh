#!/bin/bash
export ASPNETCORE_ENVIRONMENT=local
cd src/DShop.Services.Identity
dotnet run --no-restore