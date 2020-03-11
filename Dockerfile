FROM mcr.microsoft.com/dotnet/core/runtime:3.1

COPY ./ /

ENTRYPOINT ["/entrypoint.sh"]
