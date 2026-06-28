FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first for layer-cached restore
COPY src/Agenda.Api/Agenda.Api.csproj src/Agenda.Api/
COPY src/Agenda.Application/Agenda.Application.csproj src/Agenda.Application/
COPY src/Agenda.Infrastructure/Agenda.Infrastructure.csproj src/Agenda.Infrastructure/
COPY src/Agenda.Domain/Agenda.Domain.csproj src/Agenda.Domain/
COPY src/Agenda.Contracts/Agenda.Contracts.csproj src/Agenda.Contracts/
RUN dotnet restore src/Agenda.Api/Agenda.Api.csproj

# Copy source and publish
COPY src/Agenda.Api/ src/Agenda.Api/
COPY src/Agenda.Application/ src/Agenda.Application/
COPY src/Agenda.Infrastructure/ src/Agenda.Infrastructure/
COPY src/Agenda.Domain/ src/Agenda.Domain/
COPY src/Agenda.Contracts/ src/Agenda.Contracts/
RUN dotnet publish src/Agenda.Api/Agenda.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Agenda.Api.dll"]
