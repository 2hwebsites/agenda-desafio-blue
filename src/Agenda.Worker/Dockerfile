FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first for layer-cached restore
COPY src/Agenda.Worker/Agenda.Worker.csproj src/Agenda.Worker/
COPY src/Agenda.Contracts/Agenda.Contracts.csproj src/Agenda.Contracts/
RUN dotnet restore src/Agenda.Worker/Agenda.Worker.csproj

# Copy source and publish
COPY src/Agenda.Worker/ src/Agenda.Worker/
COPY src/Agenda.Contracts/ src/Agenda.Contracts/
RUN dotnet publish src/Agenda.Worker/Agenda.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Agenda.Worker.dll"]
