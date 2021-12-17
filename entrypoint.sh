#!/bin/bash

cd /render/src/Render

if [ ! -z $3 ]; then
	dotnet run --no-launch-profile -p D2L.Dev.Docs.Render.csproj --repo-root /github/workspace/ --docs-path $1 --output /github/workspace/$2 --template-path $3
else
	dotnet run --no-launch-profile -p D2L.Dev.Docs.Render.csproj --repo-root /github/workspace/ --docs-path $1 --output /github/workspace/$2
fi
