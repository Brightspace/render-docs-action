#!/bin/bash

if [ $# -ne 2 ]; then
	echo Usage: $0 docs_path output_dir
	exit 1
fi

cd /render/src/Render

dotnet run --no-launch-profile -p D2L.Dev.Docs.Render.csproj --repo-root /github/workspace/ --docs-path $1 --output /github/workspace/$2
