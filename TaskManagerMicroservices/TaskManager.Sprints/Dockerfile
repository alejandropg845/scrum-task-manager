

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Sprints/TaskManager.Sprints.csproj", "TaskManager.Sprints/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Sprints/TaskManager.Sprints.csproj"


COPY . .
WORKDIR "/src/TaskManager.Sprints"


RUN dotnet build "TaskManager.Sprints.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Sprints.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Sprints.dll"]
