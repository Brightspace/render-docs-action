FROM mcr.microsoft.com/dotnet/core/runtime:3.1

COPY ./ /

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
