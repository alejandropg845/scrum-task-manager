

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Authentication/TaskManager.Authentication.csproj", "TaskManager.Authentication/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Authentication/TaskManager.Authentication.csproj"


COPY . .
WORKDIR "/src/TaskManager.Authentication"


RUN dotnet build "TaskManager.Authentication.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Authentication.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Authentication.dll"]
