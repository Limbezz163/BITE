FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Yammy.Backend/Yammy.Backend.csproj Yammy.Backend/
RUN dotnet restore Yammy.Backend/Yammy.Backend.csproj

COPY Yammy.Backend/ Yammy.Backend/
COPY frontend/ frontend/

RUN dotnet publish Yammy.Backend/Yammy.Backend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Yammy.Backend.dll"]
