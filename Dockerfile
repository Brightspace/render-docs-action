FROM mcr.microsoft.com/dotnet/core/sdk:3.1

COPY ./ /render

ENTRYPOINT ["/render/entrypoint.sh"]
