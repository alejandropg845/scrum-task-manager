

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.GroupsRoles/TaskManager.GroupsRoles.csproj", "TaskManager.GroupsRoles/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.GroupsRoles/TaskManager.GroupsRoles.csproj"


COPY . .
WORKDIR "/src/TaskManager.GroupsRoles"


RUN dotnet build "TaskManager.GroupsRoles.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.GroupsRoles.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.GroupsRoles.dll"]
