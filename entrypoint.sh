#!/bin/bash

if [ $# -ne 1 ]; then
	echo Usage: $0 docs_path
	exit 1
fi

dotnet run -p ./src/Render --input $1 --output /output
