

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Groups/TaskManager.Groups.csproj", "TaskManager.Groups/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Groups/TaskManager.Groups.csproj"


COPY . .
WORKDIR "/src/TaskManager.Groups"


RUN dotnet build "TaskManager.Groups.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Groups.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Groups.dll"]
