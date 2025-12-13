

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.TaskItems/TaskManager.TaskItems.csproj", "TaskManager.TaskItems/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.TaskItems/TaskManager.TaskItems.csproj"


COPY . .
WORKDIR "/src/TaskManager.TaskItems"


RUN dotnet build "TaskManager.TaskItems.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.TaskItems.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.TaskItems.dll"]
