#!/bin/bash

if [ $# -ne 2 ]; then
	echo Usage: $0 docs_path output_dir
	exit 1
fi

dotnet run --no-launch-profile -p:RunWorkingDirectory=/render/src/ -p /render/src/Render/D2L.Dev.Docs.Render.csproj --input $1 --output $2
