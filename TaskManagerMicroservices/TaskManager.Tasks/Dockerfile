

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Tasks/TaskManager.Tasks.csproj", "TaskManager.Tasks/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Tasks/TaskManager.Tasks.csproj"


COPY . .
WORKDIR "/src/TaskManager.Tasks"


RUN dotnet build "TaskManager.Tasks.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Tasks.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Tasks.dll"]
