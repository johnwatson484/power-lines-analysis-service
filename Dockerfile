# Development
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS development

RUN apk update \
  && apk --no-cache add curl procps unzip \
  && wget -qO- https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --chown=dotnet:dotnet . .

RUN dotnet publish ./PowerLinesAnalysisService/ -c Release -o /home/dotnet/out

ARG PORT=5003
ENV PORT=${PORT}
ENV ASPNETCORE_URLS=http://*:5003
ENV ASPNETCORE_ENVIRONMENT=development
EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "watch", "--project", "./PowerLinesAnalysisService", "run"]

# Production
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS production

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --from=development /home/dotnet/out/ ./
ARG PORT=5003
ENV ASPNETCORE_URLS=http://*:5003
ENV ASPNETCORE_ENVIRONMENT=production
EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "PowerLinesAnalysisService.dll"]
