FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Microsoft.SPID.Proxy/Microsoft.SPID.Proxy.csproj", "Microsoft.SPID.Proxy/"]
RUN dotnet restore "Microsoft.SPID.Proxy/Microsoft.SPID.Proxy.csproj"
COPY . .
WORKDIR "/src/Microsoft.SPID.Proxy"
RUN dotnet build "Microsoft.SPID.Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Microsoft.SPID.Proxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microsoft.SPID.Proxy.dll"]