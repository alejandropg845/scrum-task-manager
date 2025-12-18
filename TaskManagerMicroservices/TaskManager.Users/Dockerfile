

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Users/TaskManager.Users.csproj", "TaskManager.Users/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Users/TaskManager.Users.csproj"


COPY . .
WORKDIR "/src/TaskManager.Users"


RUN dotnet build "TaskManager.Users.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Users.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Users.dll"]
