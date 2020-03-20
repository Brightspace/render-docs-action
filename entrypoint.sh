#!/bin/bash

if [ $# -ne 1 ]; then
	echo Usage: $0 docs_path
	exit 1
fi

dotnet run -p /render/src/Render/D2L.Dev.Docs.Render.csproj --input $1 --output output
